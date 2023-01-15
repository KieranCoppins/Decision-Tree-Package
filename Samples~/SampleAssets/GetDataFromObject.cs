using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KieranCoppins.DecisionTrees;

public class GetDataFromObject : Function<bool>
{
    private DTRunner _dtRunner;

    // Presents a short summary on what this condition is checking
    public override string GetSummary(BaseNodeView nodeView)
    {
        return "checks the value of test bool";
    }

    public override bool Invoke()
    {
        if (_dtRunner)
            return _dtRunner.TestBool;

        return false;
    }

    // Runs on tree initialisation, if the metaData passed is a dtRunner, then store a reference to it
    public override void Initialise<T>(T metaData)
    {
        base.Initialise(metaData);
        if (metaData is DTRunner)
            _dtRunner = metaData as DTRunner;
    }
}
