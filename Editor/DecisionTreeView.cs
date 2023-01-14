using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DecisionTreeView : GraphView
{
    public System.Action<BaseNodeView> OnNodeSelected { get; set; }

    public new class UxmlFactory : UxmlFactory<DecisionTreeView, UxmlTraits> { };

    /// <summary>
    /// The tree that is being displayed
    /// </summary>
    private DecisionTree _tree;

    public DecisionTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.kierancoppins.decision-trees/Editor/DecisionTreeEditor.uss");
        styleSheets.Add(styleSheet);

        graphViewChanged += OnGraphViewChanged;
    }

    /// <summary>
    /// Populate our tree view
    /// </summary>
    /// <param name="tree">The tree to populate the tree view with</param>
    public void PopulateView(DecisionTree tree)
    {
        _tree = tree;
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (_tree.Root == null)
        {
            _tree.Root = _tree.CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
            EditorUtility.SetDirty(_tree);
            AssetDatabase.SaveAssets();
        }

        // Create node views
        _tree.Nodes.ForEach(node =>
        {
            if (node is DecisionTreeNode)
                CreateNodeView(node as DecisionTreeNode);
            else if (GenericHelpers.IsSubClassOfRawGeneric(typeof(Function<>), node.GetType()))
                CreateNodeView(node);
        });

        // Create edges
        _tree.Inputs.ForEach(input =>
        {
            var inputNode = GetNodeByGuid(input.InputGUID) as BaseNodeView;
            var outputNode = GetNodeByGuid(input.OutputGUID) as BaseNodeView;
            if (inputNode != null && outputNode != null)
            {
                Edge edge = outputNode.OutputPorts[input.OutputPortName].ConnectTo(inputNode.InputPorts[input.InputPortName]);
                inputNode.ConnectedNodes.Add(outputNode);
                outputNode.ConnectedNodes.Add(inputNode);
                AddElement(edge);
            }
        });

    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 clickPoint = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
        GridBackground grid = contentContainer[0] as GridBackground;
        var types = TypeCache.GetTypesDerivedFrom<DecisionTreeEditorNodeBase>();
        foreach(var type in types)
        {
            // We want to ignore any abstract types and root nodes
            if (!type.IsAbstract && type != typeof(RootNode))
            {
                System.Type rootBaseType = type.BaseType;
                string pathString = "";
                while (rootBaseType != null && rootBaseType != typeof(DecisionTreeEditorNodeBase))
                {
                    if (rootBaseType.IsGenericType && rootBaseType.GetGenericTypeDefinition() == typeof(Function<>))
                        pathString = $"Function Node/" + pathString;
                    else
                        pathString = $"{rootBaseType.Name}/" + pathString;
                    rootBaseType = rootBaseType.BaseType;
                }
                evt.menu.AppendAction(pathString + $"{type.Name}", (a) => CreateNode(type, clickPoint));
            }
        }
    }

    /// <summary>
    /// Creates a node in the tree view and inside the decision tree
    /// </summary>
    /// <param name="type">The type of node to create</param>
    /// <param name="creationPos">The position at which to place the node</param>
    void CreateNode(System.Type type, Vector2 creationPos)
    {
        DecisionTreeEditorNodeBase node = _tree.CreateNode(type, creationPos);
        if (node is DecisionTreeNode)
            CreateNodeView(node as DecisionTreeNode);
        else if (GenericHelpers.IsSubClassOfRawGeneric(typeof(Function<>), node.GetType()))
            CreateNodeView(node);

    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }

    /// <summary>
    /// Creates a node view for action & decision nodes
    /// </summary>
    /// <param name="node"></param>
    private void CreateNodeView(DecisionTreeNode node)
    {
        DecisionTreeNodeView nodeView = new(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    /// <summary>
    /// Creates a node view for function nodes
    /// </summary>
    /// <param name="node"></param>
    private void CreateNodeView(object node)
    {
        FunctionNodeView nodeView = new(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    /// <summary>
    /// Is called whenever the graph view changes
    /// </summary>
    /// <param name="graphViewChange"></param>
    /// <returns></returns>
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                // Delete our node from our tree
                if (elem is BaseNodeView)
                {
                    BaseNodeView nodeView = elem as BaseNodeView;
                    foreach (var node in nodeView.ConnectedNodes)
                    {
                        node.ConnectedNodes.Remove(nodeView);
                    }
                    _tree.DeleteNode(nodeView.Node);
                }

                // If our element is an edge, delete the edge
                if (elem is Edge)
                {
                    Edge edge = elem as Edge;
                    BaseNodeView inputNode = edge.input.node as BaseNodeView;
                    BaseNodeView outputNode = edge.output.node as BaseNodeView;

                    if (outputNode.Node is Decision)
                    {
                        Decision decisionNode = outputNode.Node as Decision;
                        if (edge.output.portName == "TRUE")
                            decisionNode.TrueNode = null;
                        else if (edge.output.portName == "FALSE")
                            decisionNode.FalseNode = null;
                        else
                            Debug.LogError("Decision node was set from an invalid output?!");
                    }

                    inputNode.ConnectedNodes.Remove(outputNode);
                    outputNode.ConnectedNodes.Remove(inputNode);

                    _tree.Inputs = _tree.Inputs.Where((input) => input != edge).ToList();

                    var constructors = inputNode.Node.GetType().GetConstructors();
                    foreach (var constructor in constructors)
                    {
                        if (constructor.GetParameters().Length > 0)
                        {
                            foreach (var param in constructor.GetParameters())
                            {
                                if (edge.input.portType == param.ParameterType && edge.input.portName == param.Name)
                                {
                                    GenericHelpers.SetVariable(inputNode.Node, null, param.Name);
                                }
                            }
                        }
                    }
                    inputNode.title = inputNode.Node.GetTitle();
                    inputNode.Description = inputNode.Node.GetDescription(inputNode);
                    outputNode.title = outputNode.Node.GetTitle();
                    outputNode.Description = outputNode.Node.GetDescription(outputNode);
                }
            });
        }
        if (graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(elem =>
            {
                // Create the edge graphically
                BaseNodeView inputNode = elem.input.node as BaseNodeView;
                BaseNodeView outputNode = elem.output.node as BaseNodeView;

                InputOutputPorts input = new(inputNode.Node.Guid, elem.input.name, outputNode.Node.Guid, elem.output.name);

                // Apply the edge for real

                // If we're a decision node
                if (outputNode.Node is Decision)
                {
                    Decision decisionNode = outputNode.Node as Decision;
                    if (input.OutputPortName == "TRUE")
                        decisionNode.TrueNode = inputNode.Node as DecisionTreeNode;
                    else if (input.OutputPortName == "FALSE")
                        decisionNode.FalseNode = inputNode.Node as DecisionTreeNode;
                    else
                        Debug.LogError("Decision node was set from an invalid output?!");
                }

                // If we're a root node
                else if(outputNode.Node is RootNode)
                {
                    RootNode rootNode = outputNode.Node as RootNode;
                    rootNode.Child = inputNode.Node as DecisionTreeNode;
                }

                // Otherwise we can add these dynamically
                else
                {
                    var constructors = inputNode.Node.GetType().GetConstructors();
                    foreach(var constructor in constructors)
                    {
                        if (constructor.GetParameters().Length > 0)
                        {
                            foreach (var param in constructor.GetParameters())
                            {
                                if (elem.input.portType == param.ParameterType && elem.input.portName == param.Name)
                                {
                                    GenericHelpers.SetVariable(inputNode.Node, outputNode.Node, param.Name);
                                }
                            }
                        }
                    }
                }

                outputNode.ConnectedNodes.Add(inputNode);
                inputNode.ConnectedNodes.Add(outputNode);
                inputNode.title = inputNode.Node.GetTitle();
                inputNode.Description = inputNode.Node.GetDescription(inputNode);
                outputNode.title = outputNode.Node.GetTitle();
                outputNode.Description = outputNode.Node.GetDescription(outputNode);
                _tree.Inputs.Add(input);
            });
        }

        // Save our asset on any changes
        EditorUtility.SetDirty(_tree);
        AssetDatabase.SaveAssets();
        return graphViewChange;
    }

    /// <summary>
    /// Update all nodes' state
    /// </summary>
    public void UpdateNodeStates()
    {
        nodes.ForEach(node =>
        {
            BaseNodeView nodeView = node as BaseNodeView;
            nodeView.UpdateState();
        });
    }
}
