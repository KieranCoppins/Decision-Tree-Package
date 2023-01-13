using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

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
        if (node is Action)
        {
            AddToClassList("action");
        }
        else if (node is Decision)
        {
            AddToClassList("decision");
        }
        else if (node is RootNode)
        {
            AddToClassList("root");
        }
    }

    void CreateInputPorts()
    {
        if (node is RootNode)
            return;

        Port port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
        port.portName = "Entry";
        port.name = "main";
        inputPorts.Add("main", port);
        inputContainer.Add(port);
        //port.style.flexDirection = FlexDirection.Column;
        var constructors = node.GetType().GetConstructors();
        foreach(var constructor in constructors)
        {
            if (constructor.GetParameters().Length > 0)
            {
                foreach(var parameter in constructor.GetParameters())
                {
                    port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, parameter.ParameterType);
                    port.portName = parameter.Name;
                    port.name = parameter.Name;
                    inputPorts.Add(parameter.Name, port);
                    inputContainer.Add(port);
                    //port.style.flexDirection = FlexDirection.Column;
                }
            }
        }
    }

    void CreateOutputPorts()
    {
        if (node is Action)
        {

        }
        else if (node is Decision)
        {
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.portName = "TRUE";
            port.name = "TRUE";
            outputPorts.Add("TRUE", port);
            outputContainer.Add(port);
            //port.style.flexDirection = FlexDirection.ColumnReverse;


            port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.name = "FALSE";
            port.portName = "FALSE";
            outputPorts.Add("FALSE", port);
            outputContainer.Add(port);
            //port.style.flexDirection = FlexDirection.ColumnReverse;
        }
        else if (node is RootNode)
        {
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.name = "main";
            port.portName = "";
            outputPorts.Add("main", port);
            outputContainer.Add(port);
            //port.style.flexDirection = FlexDirection.ColumnReverse;
        }
    }
}