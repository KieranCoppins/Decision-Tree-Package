using UnityEditor.Experimental.GraphView;

/// <summary>
/// The node view for all decision tree nodes (such as actions & decisions)
/// </summary>
public class DecisionTreeNodeView : BaseNodeView
{

    public DecisionTreeNodeView(DecisionTreeNode node) : base(node)
    {
        CreateInputPorts();
        CreateOutputPorts();
        StyleClassAssignment();
    }

    /// <summary>
    /// Apply different styling depending on the type of note
    /// </summary>
    private void StyleClassAssignment()
    {
        if (Node is Action)
            AddToClassList("action");
        else if (Node is Decision)
            AddToClassList("decision");
        else if (Node is RootNode)
            AddToClassList("root");
    }

    /// <summary>
    /// Creates all the input ports for this node
    /// </summary>
    private void CreateInputPorts()
    {
        if (Node is RootNode)
            return;

        Port port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
        port.portName = "Entry";
        port.name = "main";
        InputPorts.Add("main", port);
        inputContainer.Add(port);

        // Depending on the constructors set of this node, procedurally generate input ports for this node
        var constructors = Node.GetType().GetConstructors();
        foreach(var constructor in constructors)
        {
            if (constructor.GetParameters().Length > 0)
            {
                foreach(var parameter in constructor.GetParameters())
                {
                    port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, parameter.ParameterType);
                    port.portName = parameter.Name;
                    port.name = parameter.Name;
                    InputPorts.Add(parameter.Name, port);
                    inputContainer.Add(port);
                }
            }
        }
    }

    /// <summary>
    /// Creates all the output ports for this node
    /// </summary>
    private void CreateOutputPorts()
    {
        if (Node is Action)
        {

        }
        else if (Node is Decision)
        {
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.portName = "TRUE";
            port.name = "TRUE";
            OutputPorts.Add("TRUE", port);
            outputContainer.Add(port);


            port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.name = "FALSE";
            port.portName = "FALSE";
            OutputPorts.Add("FALSE", port);
            outputContainer.Add(port);
        }
        else if (Node is RootNode)
        {
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.name = "main";
            port.portName = "";
            OutputPorts.Add("main", port);
            outputContainer.Add(port);
        }
    }
}