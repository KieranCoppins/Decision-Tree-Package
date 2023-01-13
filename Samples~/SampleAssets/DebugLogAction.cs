using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogAction : Action
{
    object metaData;

    // Stores a reference to the meta data regardless of type on initialisation
    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        this.metaData = metaData;
    }

    // A simple action that debug logs the metaData object
    public override IEnumerator Execute()
    {
        Debug.Log(metaData);
        yield return null;
    }
}
