using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogString : Action
{
    [SerializeField] private string _textToPrint;
    public override IEnumerator Execute()
    {
        Debug.Log(_textToPrint);
        yield return null;
    }
}
