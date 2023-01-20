using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Data.Common;

namespace KieranCoppins.DecisionTrees
{
    [CreateAssetMenu(menuName = "Decision Tree/Decision Tree")]
    public class DecisionTree : ScriptableObject
    {
        public Vector3 ViewPosition { 
            get { return _viewPosition; } 
            
            set 
            {
                Undo.RecordObject(this, "Decision Tree (Change viewport location)");
                _viewPosition = value; 
                EditorUtility.SetDirty(this);
            } 
        }

        [HideInInspector] [SerializeField] private Vector3 _viewPosition = Vector3.zero;

        public Vector3 ViewScale { 
            get { return _viewScale; } 
            set 
            {
                Undo.RecordObject(this, "Decision Tree (Change viewport location)");
                _viewScale = value;
                EditorUtility.SetDirty(this);
            } 
        }

        public bool IsClone { get; private set; }

        [HideInInspector][SerializeField] private Vector3 _viewScale = Vector3.one;

        [HideInInspector] public RootNode Root;

        /// Editor Values
        // A list of nodes for our editor, they don't have to be linked to the tree
        [HideInInspector] public List<DecisionTreeEditorNodeBase> Nodes = new List<DecisionTreeEditorNodeBase>();
        [HideInInspector] public List<InputOutputPorts> Inputs = new List<InputOutputPorts>();

        /// <summary>
        /// Run this tree
        /// </summary>
        /// <returns>The action determined by the tree to do next</returns>
        public Action Run() => Root.MakeDecision() as Action;

        /// <summary>
        /// Initialise this tree with the given meta data. Meta data is passed down the tree to each node.
        /// </summary>
        /// <typeparam name="T">The type of metadata</typeparam>
        /// <param name="metaData">The meta data to pass to each node</param>
        public virtual void Initialise<T>(T metaData) => Root.Initialise(metaData);

        /// <summary>
        /// Traverses through the tree and applies the visiter function to each node
        /// </summary>
        /// <param name="node">The node to traverse</param>
        /// <param name="visiter">The function to the apply to the traversed nodes' children</param>
        public void Traverse(DecisionTreeEditorNodeBase node, Action<DecisionTreeEditorNodeBase> visiter)
        {
            if (node)
            {
                visiter.Invoke(node);
                var children = node.GetChildren();
                children?.ForEach((n) => { if (n) Traverse(n, visiter); });
            }
        }

        /// <summary>
        /// Create a new instance of this tree
        /// </summary>
        /// <param name="name">An optional name to give this tree</param>
        /// <returns>A clone of this decision tree</returns>
        public DecisionTree Clone(string name = null)
        {
            DecisionTree tree = Instantiate(this);
            if (name != null)
                tree.name = name;
            tree.Root = Root.Clone() as RootNode;
            tree.Nodes = new List<DecisionTreeEditorNodeBase>();
            Traverse(tree.Root, (n) =>
            {
                tree.Nodes.Add(n);
            });
            tree.IsClone= true;
            return tree;
        }

        /// <summary>
        /// Creates a new node of the given type, saves the asset and adds it to the tree.
        /// </summary>
        /// <param name="type">The type of node to create</param>
        /// <param name="creationPos"> The position inside the visual editor to place this node</param>
        /// <returns>The node that was created</returns>
        public DecisionTreeEditorNodeBase CreateNode(Type type, Vector2 creationPos)
        {
            DecisionTreeEditorNodeBase node = CreateInstance(type) as DecisionTreeEditorNodeBase;
            node.name = type.Name;
            node.Guid = GUID.Generate().ToString();
            node.PositionalData.xMin = creationPos.x;
            node.PositionalData.yMin = creationPos.y;

            Undo.RecordObject(this, "Decision Tree (Create Node)");

            Nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, "Decision Tree (Create Node)");
            AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary>
        /// Deletes the given node
        /// </summary>
        /// <param name="node">The node to delete</param>
        public void DeleteNode(DecisionTreeEditorNodeBase node)
        {
            Undo.RecordObject(this, "Decision Tree (Delete Node)");
            Nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// States of the node to be in, defaults to idle
    /// </summary>
    public enum DecisionTreeNodeRunningState
    {
        Idle,
        Running,
        Finished,
        Interrupted,
    }

    /// <summary>
    /// The base class for every node in the decision tree.
    /// </summary>
    public abstract class DecisionTreeEditorNodeBase : ScriptableObject
    {
        /// Editor Values
        [HideInInspector] public string Guid;
        [HideInInspector] public Rect PositionalData;

        public DecisionTreeNodeRunningState NodeState { get; set; }

        public System.Action OnValidateCallback { get; set; }

        /// <summary>
        /// Clone this node - if the node contains child nodes then it should clone those as well
        /// </summary>
        /// <returns>A cloned version of this node (An instance of this node)</returns>
        public virtual DecisionTreeEditorNodeBase Clone() => Instantiate(this);

        /// <summary>
        /// Gets all the child nodes associated with this node
        /// </summary>
        /// <returns>A list of all the children nodes of this node</returns>
        public virtual List<DecisionTreeEditorNodeBase> GetChildren() => null;

        /// <summary>
        /// Generate a stylised title to display in the visual editor
        /// </summary>
        /// <returns>The title of this node</returns>
        public virtual string GetTitle() => GenericHelpers.GenericHelpers.SplitCamelCase(name);

        /// <summary>
        /// Generate a description of this node
        /// </summary>
        /// <param name="nodeView"></param>
        /// <returns>a description of the node</returns>
        public virtual string GetDescription(BaseNodeView nodeView) => "This is the default description of a DecisionTreeEditorNode";

        private void OnValidate() => OnValidateCallback?.Invoke();

        /// <summary>
        /// Initialise this node with the meta data provided. This should also initialise any children nodes as well
        /// </summary>
        /// <typeparam name="T">The type of meta data</typeparam>
        /// <param name="metaData">The metadata to initialise with</param>
        public virtual void Initialise<T>(T metaData) { }
    }

    /// <summary>
    /// A base class for decisions and actions
    /// </summary>
    public abstract class DecisionTreeNode : DecisionTreeEditorNodeBase
    {

        public DecisionTreeNode() { }

        /// <summary>
        /// Tell this node to make a decision
        /// </summary>
        /// <returns> Returns the node at the end of the decision making </returns>
        public abstract DecisionTreeNode MakeDecision();
    }

    /// <summary>
    /// A base class for all action nodes. Sets some default flags
    /// </summary>
    public abstract class Action : DecisionTreeNode
    {
        /// <summary>
        /// Flags to determine how this action should be executed.
        /// </summary>
        [Flags]
        public enum ActionFlags
        {
            AsyncAction = 1 << 0,
            Interruptor = 1 << 1,
            Interruptable = 1 << 2,
        }

        /// <summary>
        /// The current flags set on this action.
        /// </summary>
        public ActionFlags Flags { get { return _flags; } protected set { _flags = value; } }

        [EnumFlags][SerializeField] private ActionFlags _flags;

        public Action()
        {
            Flags = 0;

            // Default interruptable to true
            Flags |= ActionFlags.Interruptable;
        }

        public override DecisionTreeNode MakeDecision() => this;

        /// <summary>
        /// Do this action
        /// </summary>
        public abstract IEnumerator Execute();
    }

    /// <summary>
    /// A base decision nodes. Decision nodes handle which path of the tree to move to next.
    /// </summary>
    public abstract class Decision : DecisionTreeNode
    {
        [HideInInspector] public DecisionTreeNode TrueNode;

        [HideInInspector] public DecisionTreeNode FalseNode;

        /// <summary>
        /// Returns the node which this decision node has decided.
        /// </summary>
        /// <returns></returns>
        public abstract DecisionTreeNode GetBranch();


        public override void Initialise<T>(T metaData)
        {
            base.Initialise(metaData);
            TrueNode.Initialise(metaData);
            FalseNode.Initialise(metaData);
        }

        public override DecisionTreeNode MakeDecision() => GetBranch().MakeDecision();

        public override DecisionTreeEditorNodeBase Clone()
        {
            Decision node = Instantiate(this);
            node.TrueNode = (DecisionTreeNode)TrueNode.Clone();
            node.FalseNode = (DecisionTreeNode)FalseNode.Clone();
            return node;
        }

        public override List<DecisionTreeEditorNodeBase> GetChildren() => new() { TrueNode, FalseNode };
    }

    /// <summary>
    /// A base function node. Function nodes can be plugged into any other node. When invoked they return their type T.
    /// </summary>
    /// <typeparam name="T">The type that this function node will return</typeparam>
    public abstract class Function<T> : DecisionTreeEditorNodeBase
    {
        public Function() { }

        /// <summary>
        /// Invoke this function node.
        /// </summary>
        /// <returns>The outcome of the function</returns>
        public abstract T Invoke();

        /// <summary>
        /// This should return a quick summary of what the function does. NOT THE DESCRIPTION. This will be used within the description of other nodes.
        /// </summary>
        /// <returns></returns>
        public abstract string GetSummary(BaseNodeView nodeView);

        public override string GetDescription(BaseNodeView nodeView) => $"This is the default function description.";
    }

    /// <summary>
    /// A base logic gate node. Logic gate nodes take 2 boolean function nodes and applies a logic calculation on them. Such as AND.
    /// </summary>
    public abstract class F_LogicGate : Function<bool>
    {
        [HideInInspector][SerializeField] protected Function<bool> A;

        [HideInInspector][SerializeField] protected Function<bool> B;

        public F_LogicGate(Function<bool> A, Function<bool> B) { }

        public override void Initialise<T>(T metaData)
        {
            base.Initialise(metaData);
            A.Initialise(metaData);
            B.Initialise(metaData);
        }

        public override DecisionTreeEditorNodeBase Clone()
        {
            F_LogicGate node = Instantiate(this);
            node.A = (Function<bool>)A.Clone();
            node.B = (Function<bool>)B.Clone();
            return node;
        }

        public override List<DecisionTreeEditorNodeBase> GetChildren() => new() { A, B };
    }

    /// <summary>
    /// A class that stores information about edges inside the map.
    /// </summary>
    [Serializable]
    public class InputOutputPorts
    {
        /// <summary>
        /// The GUID of the input node for this edge
        /// </summary>
        public string InputGUID;

        /// <summary>
        /// The port name of the input node for this edge
        /// </summary>
        public string InputPortName;

        /// <summary>
        /// The GUID of the output node for this edge
        /// </summary>
        public string OutputGUID;


        /// <summary>
        /// The port name of the output node for this edge
        /// </summary>
        public string OutputPortName;

        public InputOutputPorts(string inputGUID, string inputPortName, string outputGUID, string outputPortName)
        {
            InputGUID = inputGUID;
            InputPortName = inputPortName;
            OutputGUID = outputGUID;
            OutputPortName = outputPortName;
        }

        public override string ToString() => $"In GUID: {InputGUID} | In Port: {InputPortName} | Out GUID: {OutputGUID} | Out Port: {OutputPortName}";

        public static bool operator !=(InputOutputPorts input, Edge edge)
        {
            return !(input == edge);
        }

        public static bool operator ==(InputOutputPorts input, Edge edge)
        {
            BaseNodeView inputNodeView = edge.input.node as BaseNodeView;
            BaseNodeView outputNodeView = edge.output.node as BaseNodeView;
            return input.InputGUID == inputNodeView.Node.Guid &&
                input.InputPortName == edge.input.name &&
                input.OutputGUID == outputNodeView.Node.Guid &&
                input.OutputPortName == edge.output.name;
        }

        public bool Equals(InputOutputPorts input)
        {
            return InputGUID == input.InputGUID &&
                InputPortName == input.InputPortName &&
                OutputGUID == input.OutputGUID &&
                OutputPortName == input.OutputPortName;
        }

        public override bool Equals(object obj) => Equals(obj as InputOutputPorts);

        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// The base node view class. This contains all the information that every node in the visual editor will need.
    /// </summary>
    public abstract class BaseNodeView : Node
    {
        /// <summary>
        /// A dictionary containing datatype to colours
        /// </summary>
        public static IDictionary<Type, Color> TypeColourDictionary = new Dictionary<Type, Color>()
        {
            {typeof(float), new Color(.243f, .980f, .265f) },
            {typeof(int), new Color(.243f, .980f, .265f) },
            {typeof(decimal), new Color(.243f, .980f, .265f) },
            {typeof(long), new Color(.243f, .980f, .265f) },
            {typeof(bool), new Color(.980f, .243f, .243f)},
            {typeof(string), new Color(.936f, .243f, .980f) },
            {typeof(char), new Color(.936f, .243f, .980f) },
            {typeof(Vector2), new Color(.690f, .980f, .243f) },
            {typeof(Vector3), new Color(.690f, .980f, .243f) },
        };
        public Action<BaseNodeView> OnNodeSelected { get; set; }

        /// <summary>
        /// The node that this node view contains
        /// </summary>
        public DecisionTreeEditorNodeBase Node { get; }

        /// <summary>
        /// A dictionary containing all input ports, keyed by their port names
        /// </summary>
        public readonly Dictionary<string, Port> InputPorts = new();

        /// <summary>
        /// A dictionary containing all output ports, keyed by their port names
        /// </summary>
        public readonly Dictionary<string, Port> OutputPorts = new();

        /// <summary>
        /// A list off all other nodes that are connected to this node
        /// </summary>
        public readonly List<BaseNodeView> ConnectedNodes = new();

        private readonly Label _descriptionLabel;
        private readonly Label _errorLabel;
        private readonly VisualElement _errorContainer;

        public bool ReadOnly { get; set; }

        /// <summary>
        /// The description that is applied to the node view inside the visual editor
        /// </summary>
        public string Description
        {
            get { return _descriptionLabel.text; }

            set
            {
                if (value != Description)
                {
                    _descriptionLabel.text = value;

                    // Update all nodes connected to this node until theres no change to be made
                    foreach (var node in ConnectedNodes)
                    {
                        node.Description = node.Node.GetDescription(node);
                    }
                }
            }
        }

        /// <summary>
        /// The error that is displayed in the visual editor
        /// </summary>
        public string Error
        {
            get { return _errorLabel.text; }
            set
            {
                if (value == null || value == "")
                {
                    _errorLabel.style.display = DisplayStyle.None;
                    _errorContainer.style.display = DisplayStyle.None;
                    _errorLabel.text = "";
                }
                else
                {
                    _errorLabel.style.display = DisplayStyle.Flex;
                    _errorContainer.style.display = DisplayStyle.Flex;
                    _errorLabel.text = value;
                }
            }
        }

        public BaseNodeView(DecisionTreeEditorNodeBase node, string uxml = "Packages/com.kierancoppins.decision-trees/Editor/DecisionTreeNodeView.uxml") : base(uxml)
        {
            Node = node;
            Node.OnValidateCallback += OnValidate;
            title = Node.GetTitle();

            // Default our error to be hidden
            _errorLabel = this.Q<Label>("error-label");
            _errorContainer = this.Q<VisualElement>("error");
            _errorLabel.style.display = DisplayStyle.None;
            _errorContainer.style.display = DisplayStyle.None;

            _descriptionLabel = this.Q<Label>("description");
            Description = Node.GetDescription(this);

            style.left = Node.PositionalData.xMin;
            style.top = Node.PositionalData.yMin;
            viewDataKey = Node.Guid;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(Node, "Decision Tree (Set Position)");
            Node.PositionalData = newPos;
            EditorUtility.SetDirty(Node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
                OnNodeSelected.Invoke(this);
        }

        /// <summary>
        /// Links to the OnValidateCallback, runs whenever values change on the node.
        /// </summary>
        void OnValidate()
        {
            title = Node.GetTitle();
            Description = Node.GetDescription(this);
        }

        /// <summary>
        /// Updates the visual state for this node.
        /// </summary>
        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("finished");
            RemoveFromClassList("idle");
            RemoveFromClassList("interrupted");

            if (Application.isPlaying)
            {
                switch (Node.NodeState)
                {
                    case DecisionTreeNodeRunningState.Running:
                        AddToClassList("running");
                        break;
                    case DecisionTreeNodeRunningState.Finished:
                        AddToClassList("finished");
                        break;
                    case DecisionTreeNodeRunningState.Idle:
                        AddToClassList("idle");
                        break;
                    case DecisionTreeNodeRunningState.Interrupted:
                        AddToClassList("interrupted");
                        break;
                }
            }
        }

        protected static Color GetColorForType(Type type)
        {
            return TypeColourDictionary.ContainsKey(type.GetGenericArguments()[0]) ? TypeColourDictionary[type.GetGenericArguments()[0]] : new Color(.243f, .265f, .980f);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!ReadOnly)
            {
                // Add an option to be able to open the script of the Node in an IDE
                evt.menu.AppendAction("Open in IDE", (a) => {
                    string[] guids = AssetDatabase.FindAssets($"{Node.GetType().Name} t:script", null);
                    if (guids.Length > 0)
                        AssetDatabase.OpenAsset(MonoImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guids[0])).GetInstanceID());
                });
                base.BuildContextualMenu(evt);
            }
        }
    }
    public class EnumFlagsAttribute : PropertyAttribute
    {

    }
}
