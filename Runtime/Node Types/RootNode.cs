using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : DecisionTreeNode
{
    [HideInInspector] public DecisionTreeNode Child;

    public override DecisionTreeNode MakeDecision() => Child.MakeDecision();

    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        Child.Initialise(metaData);
    }

    public override DecisionTreeEditorNodeBase Clone()
    {
        RootNode node = Instantiate(this);
        node.Child = (DecisionTreeNode)Child.Clone();
        return node;
    }

    public override string GetTitle() => GenericHelpers.SplitCamelCase(name);

    public override string GetDescription(BaseNodeView nodeView) => "This is the root node of the decision tree. This is your starting point.";

    public override List<DecisionTreeEditorNodeBase> GetChildren() => new() { Child };
}
