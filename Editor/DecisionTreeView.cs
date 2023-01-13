using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEngine.Windows;

public class DecisionTreeView : GraphView
{
    public System.Action<BaseNodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<DecisionTreeView, UxmlTraits> { };
    DecisionTree tree;
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

    public void PopulateView(DecisionTree tree)
    {
        this.tree = tree;
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (tree.root == null)
        {
            tree.root = tree.CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        // Create node views
        tree.nodes.ForEach(node =>
        {
            if (node is DecisionTreeNode)
                CreateNodeView(node as DecisionTreeNode);
            else if (GenericHelpers.IsSubClassOfRawGeneric(typeof(Function<>), node.GetType()))
                CreateNodeView(node);
        });

        // Create edges
        tree.inputs.ForEach(input =>
        {
            var inputNode = GetNodeByGuid(input.inputGUID) as BaseNodeView;
            var outputNode = GetNodeByGuid(input.outputGUID) as BaseNodeView;
            Edge edge = outputNode.outputPorts[input.outputPortName].ConnectTo(inputNode.inputPorts[input.inputPortName]);
            inputNode.connectedNodes.Add(outputNode);
            outputNode.connectedNodes.Add(inputNode);
            AddElement(edge);
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

    void CreateNode(System.Type type, Vector2 creationPos)
    {
        DecisionTreeEditorNodeBase node = tree.CreateNode(type, creationPos);
        if (node is DecisionTreeNode)
            CreateNodeView(node as DecisionTreeNode);
        else if (GenericHelpers.IsSubClassOfRawGeneric(typeof(Function<>), node.GetType()))
            CreateNodeView(node);

    }

    void CreateNode(ScriptableObject scriptableObject, Vector2 creationPos)
    {
        DecisionTreeEditorNodeBase node = tree.CreateNode(scriptableObject, creationPos);
        if (node is DecisionTreeNode)
            CreateNodeView(node as DecisionTreeNode);
        else if (GenericHelpers.IsSubClassOfRawGeneric(typeof(Function<>), node.GetType()))
            CreateNodeView(node);
    }


    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }

    void CreateNodeView(DecisionTreeNode node)
    {
        DecisionTreeNodeView nodeView = new(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    void CreateNodeView(object node)
    {
        FunctionNodeView nodeView = new(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                // Delete our node from our tree
                BaseNodeView nodeView = elem as BaseNodeView;
                if (nodeView != null)
                {
                    foreach(var node in nodeView.connectedNodes)
                    {
                        node.connectedNodes.Remove(nodeView);
                    }
                    tree.DeleteNode(nodeView.node);
                }

                // If our element is an edge, delete the edge
                Edge edge = elem as Edge;
                if (edge != null)
                {
                    BaseNodeView inputNode = edge.input.node as BaseNodeView;
                    BaseNodeView outputNode = edge.output.node as BaseNodeView;

                    Decision decisionNode = outputNode.node as Decision;
                    if (decisionNode != null)
                    {
                        if (edge.output.portName == "TRUE")
                            decisionNode.trueNode = null;
                        else if (edge.output.portName == "FALSE")
                            decisionNode.falseNode = null;
                        else
                            Debug.LogError("Decision node was set from an invalid output?!");
                    }

                    inputNode.connectedNodes.Remove(outputNode);
                    outputNode.connectedNodes.Remove(inputNode);

                    tree.inputs = tree.inputs.Where((input) => input != edge).ToList();

                    var constructors = inputNode.node.GetType().GetConstructors();
                    foreach (var constructor in constructors)
                    {
                        if (constructor.GetParameters().Length > 0)
                        {
                            foreach (var param in constructor.GetParameters())
                            {
                                if (edge.input.portType == param.ParameterType && edge.input.portName == param.Name)
                                {
                                    inputNode.node.GetType().GetField(param.Name).SetValue(inputNode.node, null);
                                }
                            }
                        }
                    }
                    inputNode.title = inputNode.node.GetTitle();
                    inputNode.description = inputNode.node.GetDescription(inputNode);
                    outputNode.title = outputNode.node.GetTitle();
                    outputNode.description = outputNode.node.GetDescription(outputNode);
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

                InputOutputPorts input = new(inputNode.node.guid, elem.input.name, outputNode.node.guid, elem.output.name);

                // Apply the edge for real

                // If we're a decision node
                Decision decisionNode = outputNode.node as Decision;
                RootNode rootNode = outputNode.node as RootNode;
                if (decisionNode != null)
                {
                    if (input.outputPortName == "TRUE")
                        decisionNode.trueNode = inputNode.node as DecisionTreeNode;
                    else if (input.outputPortName == "FALSE")
                        decisionNode.falseNode = inputNode.node as DecisionTreeNode;
                    else
                        Debug.LogError("Decision node was set from an invalid output?!");
                }

                // If we're a root node
                else if(rootNode != null)
                {
                    rootNode.child = inputNode.node as DecisionTreeNode;
                }

                // Otherwise we can add these dynamically
                else
                {
                    var constructors = inputNode.node.GetType().GetConstructors();
                    foreach(var constructor in constructors)
                    {
                        if (constructor.GetParameters().Length > 0)
                        {
                            foreach (var param in constructor.GetParameters())
                            {
                                if (elem.input.portType == param.ParameterType && elem.input.portName == param.Name)
                                {
                                    inputNode.node.GetType().GetField(param.Name).SetValue(inputNode.node, outputNode.node);
                                }
                            }
                        }
                    }
                }

                outputNode.connectedNodes.Add(inputNode);
                inputNode.connectedNodes.Add(outputNode);
                inputNode.title = inputNode.node.GetTitle();
                inputNode.description = inputNode.node.GetDescription(inputNode);
                outputNode.title = outputNode.node.GetTitle();
                outputNode.description = outputNode.node.GetDescription(outputNode);
                tree.inputs.Add(input);
            });
        }
        return graphViewChange;
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(node =>
        {
            BaseNodeView nodeView = node as BaseNodeView;
            nodeView.UpdateState();
        });
    }
}
