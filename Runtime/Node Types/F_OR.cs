using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_OR : F_LogicGate
{
    public F_OR(F_Condition A, F_Condition B) : base(A, B)
    {
    }

    public override bool Invoke()
    {
        return A.Invoke() || B.Invoke();
    }

    public override string GetSummary(BaseNodeView nodeView)
    {
        try
        {
            nodeView.error = "";
            return $"{A.GetSummary(nodeView)} or {B.GetSummary(nodeView)}";
        } 
        catch (System.Exception e)
        {
            nodeView.error = e.Message;
            return "";
        }
    }
}
