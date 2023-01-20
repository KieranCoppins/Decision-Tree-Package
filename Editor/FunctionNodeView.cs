using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using KieranCoppins.DecisionTrees;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace KieranCoppins.DecisionTreesEditor
{
    public class FunctionNodeView : BaseNodeView
    {
        private InspectorView _nodeInspectorView;

        public FunctionNodeView(object function) : base(function as DecisionTreeEditorNodeBase)
        {
            _nodeInspectorView = this.Q<InspectorView>();
            _nodeInspectorView.UpdateSelection(this);

            CreateInputPorts();

            System.Type type = null;
            System.Type current = function.GetType();
            while (type == null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    type = current;
                }
                current = current.BaseType;
            }

            // Function nodes should only ever have 1 output node
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, type.GetGenericArguments()[0]);
            port.portName = "Output";
            port.name = "Output";
            port.tooltip = type.GetGenericArguments()[0].ToString();
            port.portColor = GetColorForType(type);
            OutputPorts.Add("Output", port);
            outputContainer.Add(port);
            AddToClassList("function");
        }

        /// <summary>
        /// Generates all the input nodes for this function node based on its constructors
        /// </summary>
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
                        port.portColor = GetColorForType(parameter.ParameterType);
                        port.tooltip = parameter.ParameterType.GetGenericArguments()[0].ToString();
                        InputPorts.Add(parameter.Name, port);
                        inputContainer.Add(port);
                    }
                }
            }
        }
    }
}
