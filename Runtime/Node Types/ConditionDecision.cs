using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ConditionDecision : Decision
{
    [HideInInspector] public F_Condition Condition;

    public ConditionDecision()
    {

    }

    public ConditionDecision(F_Condition Condition)
    {
        this.Condition = Condition;
    }

    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        Condition.Initialise(metaData);
    }

    public override DecisionTreeNode GetBranch()
    {
        return Condition.Invoke() ? trueNode : falseNode;
    }

    public override DecisionTreeEditorNodeBase Clone()
    {
        ConditionDecision node = Instantiate(this);
        node.trueNode = (DecisionTreeNode)trueNode.Clone();
        node.falseNode = (DecisionTreeNode)falseNode.Clone();
        node.Condition = (F_Condition)Condition.Clone();
        return node;
    }

    public override string GetDescription(BaseNodeView nodeView)
    {
        try
        {
            nodeView.error = "";
            return $"The mob will {trueNode.GetTitle().ToLower()} if {Condition.GetSummary(nodeView).ToLower()}. Otherwise the mob will {falseNode.GetTitle().ToLower()}.";
        }
        catch (System.Exception e)
        {
            nodeView.error = e.Message;
            return "There was an issue with this description";
        }
    }

    public override List<DecisionTreeEditorNodeBase> GetChildren()
    {
        return new() { trueNode, falseNode, Condition };
    }
}
