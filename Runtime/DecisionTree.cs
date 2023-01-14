using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Decision Tree/Decision Tree")]
public class DecisionTree : ScriptableObject
{
    [HideInInspector] public RootNode Root;

    /// Editor Values
    // A list of nodes for our editor, they don't have to be linked to the tree
    [HideInInspector] public List<DecisionTreeEditorNodeBase> Nodes = new List<DecisionTreeEditorNodeBase>();
    [HideInInspector] public List<InputOutputPorts> Inputs = new List<InputOutputPorts>();

    public Action Run() => Root.MakeDecision() as Action;

    public virtual void Initialise<T>(T metaData) => Root.Initialise(metaData);

    public List<DecisionTreeEditorNodeBase> GetChildren(DecisionTreeEditorNodeBase node) => node.GetChildren();

    public void Traverse(DecisionTreeEditorNodeBase node, System.Action<DecisionTreeEditorNodeBase> visiter)
    {
        if (node)
        {
            visiter.Invoke(node);
            var children = GetChildren(node);
            children?.ForEach((n) => { if (n) Traverse(n, visiter); });
        }
    }

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
        Debug.Log(tree.Nodes.Count);
        return tree;
    }

    /// Editor Functions

    public DecisionTreeEditorNodeBase CreateNode(Type type, Vector2 creationPos)
    {
        DecisionTreeEditorNodeBase node = CreateInstance(type) as DecisionTreeEditorNodeBase;
        node.name = type.Name;
        node.Guid = GUID.Generate().ToString();
        node.PositionalData.xMin = creationPos.x;
        node.PositionalData.yMin = creationPos.y;
        Nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();
        return node;
    }

    public void DeleteNode(DecisionTreeEditorNodeBase node)
    {
        Nodes.Remove(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }
}

public enum DecisionTreeNodeRunningState
{
    Idle,
    Running,
    Finished,
    Interrupted,
}

public abstract class DecisionTreeEditorNodeBase : ScriptableObject
{
    /// Editor Values
    [HideInInspector] public string Guid;
    [HideInInspector] public Rect PositionalData;

    public DecisionTreeNodeRunningState NodeState { get; set; }

    public System.Action OnValidateCallback { get; set; }

    public virtual DecisionTreeEditorNodeBase Clone() => Instantiate(this);

    public virtual List<DecisionTreeEditorNodeBase> GetChildren() => null;

    public virtual string GetTitle() => GenericHelpers.SplitCamelCase(name);

    public virtual string GetDescription(BaseNodeView nodeView) => "This is the default description of a DecisionTreeEditorNode";

    private void OnValidate() => OnValidateCallback?.Invoke();

    public virtual void Initialise<T>(T metaData) { }
}

public abstract class DecisionTreeNode : DecisionTreeEditorNodeBase
{

    public DecisionTreeNode() { }

    public abstract DecisionTreeNode MakeDecision();
}


public abstract class Action : DecisionTreeNode
{
    [Flags]
    public enum ActionFlags
    {
        SyncAction = 1 << 0,
        Interruptor = 1 << 1,
        Interruptable = 1 << 2,
    }

    public ActionFlags Flags { get { return _flags; } protected set { _flags = value; } }

    [EnumFlags] [SerializeField] private ActionFlags _flags;

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

public abstract class Decision : DecisionTreeNode
{
    [HideInInspector] public DecisionTreeNode TrueNode;

    [HideInInspector] public DecisionTreeNode FalseNode;

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

// A base function node that can return a given value
public abstract class Function<T> : DecisionTreeEditorNodeBase
{
    public Function() { }

    // The condition to invoke
    public abstract T Invoke();
}

public abstract class F_Condition : Function<bool>
{

    /// <summary>
    /// This should return a quick summary of what the function does. NOT THE DESCRIPTION. This will be used within the description of other nodes.
    /// </summary>
    /// <returns></returns>
    public abstract string GetSummary(BaseNodeView nodeView);
    public override string GetDescription(BaseNodeView nodeView) => $"Returns true if {GetSummary(nodeView)}.";
}


public abstract class F_LogicGate : F_Condition
{
    [HideInInspector] [SerializeField] protected F_Condition A;

    [HideInInspector][SerializeField] protected F_Condition B;

    public F_LogicGate(F_Condition A, F_Condition B) { }

    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        A.Initialise(metaData);
        B.Initialise(metaData);
    }

    public override DecisionTreeEditorNodeBase Clone()
    {
        F_LogicGate node = Instantiate(this);
        node.A = (F_Condition)A.Clone();
        node.B = (F_Condition)B.Clone();
        return node;
    }

    public override List<DecisionTreeEditorNodeBase> GetChildren() => new() { A, B };
}

///  Editor classes that are also referenced in engine

[Serializable]
public class InputOutputPorts
{
    public string InputGUID;
    public string InputPortName;
    public string OutputGUID;
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

public abstract class BaseNodeView : Node
{
    public Action<BaseNodeView> OnNodeSelected { get; set; }

    public DecisionTreeEditorNodeBase Node { get; }

    public readonly Dictionary<string, Port> InputPorts = new ();
    public readonly Dictionary<string, Port> OutputPorts = new ();
    public readonly List<BaseNodeView> ConnectedNodes = new ();

    private readonly Label _descriptionLabel;
    private readonly Label _errorLabel;
    private readonly VisualElement _errorContainer;

    public string Description {  
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


    public BaseNodeView(DecisionTreeEditorNodeBase node) : base("Packages/com.kierancoppins.decision-trees/Editor/DecisionTreeNodeView.uxml")
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
        Node.PositionalData = newPos;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (OnNodeSelected != null)
            OnNodeSelected.Invoke(this);
    }

    void OnValidate()
    {
        title = Node.GetTitle();
        Description = Node.GetDescription(this);
    }

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

}

public class EnumFlagsAttribute : PropertyAttribute
{

}

