using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using KieranCoppins.DecisionTrees;

namespace KieranCoppins.DecisionTreesEditor
{
    public class FunctionNodeView : BaseNodeView
    {
        public FunctionNodeView(object function) : base(function as DecisionTreeEditorNodeBase)
        {
            CreateInputPorts();

            // Function nodes should only ever have 1 output node
            Port port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, GenericHelpers.GenericHelpers.GetGenericType(function));
            port.portName = "Output";
            port.name = "Output";
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
                        InputPorts.Add(parameter.Name, port);
                        inputContainer.Add(port);
                    }
                }
            }
        }
    }
}
