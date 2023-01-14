using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_OR : F_LogicGate
{
    public F_OR(Function<bool> A, Function<bool> B) : base(A, B) { }

    public override bool Invoke() => A.Invoke() || B.Invoke();

    public override string GetSummary(BaseNodeView nodeView)
    {
        try
        {
            nodeView.Error = "";
            return $"{A.GetSummary(nodeView)} or {B.GetSummary(nodeView)}";
        } 
        catch (System.Exception e)
        {
            nodeView.Error = e.Message;
            return "";
        }
    }
}
