using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class FunctionNodeView : BaseNodeView
{
    public FunctionNodeView(object function) : base(function as DecisionTreeEditorNodeBase)
    {
        CreateInputPorts();

        Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, GenericHelpers.GetGenericType(function));
        port.portName = "Output";
        port.name = "Output";
        OutputPorts.Add("Output", port);
        outputContainer.Add(port);
        AddToClassList("function");
    }

    void CreateInputPorts()
    {
        if (Node is RootNode)
            return;

        var constructors = Node.GetType().GetConstructors();
        foreach (var constructor in constructors)
        {
            if (constructor.GetParameters().Length > 0)
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    Port port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, parameter.ParameterType);
                    port.portName = parameter.Name;
                    port.name = parameter.Name;
                    InputPorts.Add(parameter.Name, port);
                    inputContainer.Add(port);
                }
            }
        }
    }
}
