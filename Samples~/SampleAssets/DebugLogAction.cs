using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KieranCoppins.DecisionTrees;
public class DebugLogAction : Action
{
    private object _metaData;

    // Stores a reference to the meta data regardless of type on initialisation
    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        _metaData = metaData;
    }

    // A simple action that debug logs the metaData object
    public override IEnumerator Execute()
    {
        Debug.Log(_metaData);
        yield return null;
    }
}
