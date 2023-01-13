using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogString : Action
{
    [SerializeField] string textToPrint;
    public override IEnumerator Execute()
    {
        Debug.Log(textToPrint);
        yield return null;
    }
}
