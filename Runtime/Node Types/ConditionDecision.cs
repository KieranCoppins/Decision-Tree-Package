using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A decision node that uses a condition node to determine its decision.
/// </summary>
public class ConditionDecision : Decision
{
    [HideInInspector] [SerializeField] protected Function<bool> Condition;

    public ConditionDecision() { }

    public ConditionDecision(Function<bool> Condition) { }

    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        Condition.Initialise(metaData);
    }

    public override DecisionTreeNode GetBranch()
    {
        return Condition.Invoke() ? TrueNode : FalseNode;
    }

    public override DecisionTreeEditorNodeBase Clone()
    {
        ConditionDecision node = Instantiate(this);
        node.TrueNode = (DecisionTreeNode)TrueNode.Clone();
        node.FalseNode = (DecisionTreeNode)FalseNode.Clone();
        node.Condition = (Function<bool>)Condition.Clone();
        return node;
    }

    public override string GetDescription(BaseNodeView nodeView)
    {
        try
        {
            nodeView.Error = "";
            return $"The mob will {TrueNode.GetTitle().ToLower()} if {Condition.GetSummary(nodeView).ToLower()}. Otherwise the mob will {FalseNode.GetTitle().ToLower()}.";
        }
        catch (System.Exception e)
        {
            nodeView.Error = e.Message;
            return "There was an issue with this description";
        }
    }

    public override List<DecisionTreeEditorNodeBase> GetChildren()
    {
        return new() { TrueNode, FalseNode, Condition };
    }
}
