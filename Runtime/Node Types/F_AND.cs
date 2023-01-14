using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_AND : F_LogicGate
{
    public F_AND(F_Condition A, F_Condition B) : base(A, B) { }

    public override bool Invoke() => A.Invoke() && B.Invoke();

    public override string GetSummary(BaseNodeView nodeView)
    {
        try
        {
            nodeView.Error = "";
            return $"{A.GetSummary(nodeView)} AND {B.GetSummary(nodeView)}";
        }
        catch(System.Exception e)
        {
            nodeView.Error += e.Message;
            return "";
        }
    }
}
