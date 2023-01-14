using UnityEditor.Experimental.GraphView;

public class DecisionTreeNodeView : BaseNodeView
{

    public DecisionTreeNodeView(DecisionTreeNode node) : base(node)
    {
        CreateInputPorts();
        CreateOutputPorts();
        StyleClassAssignment();
    }

    void StyleClassAssignment()
    {
        if (Node is Action)
            AddToClassList("action");
        else if (Node is Decision)
            AddToClassList("decision");
        else if (Node is RootNode)
            AddToClassList("root");
    }

    void CreateInputPorts()
    {
        if (Node is RootNode)
            return;

        Port port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
        port.portName = "Entry";
        port.name = "main";
        InputPorts.Add("main", port);
        inputContainer.Add(port);
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

    void CreateOutputPorts()
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