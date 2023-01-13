using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Require the action manager as a component so it is added automatically, 
// this is needed to execute the actions inside the decision tree
[RequireComponent(typeof(ActionManager))]
public class DTRunner : MonoBehaviour
{
    public bool TestBool;
    public DecisionTree decisionTree;

    ActionManager actionManager;

    // Start is called before the first frame update
    void Start()
    {
        // First we get a reference to our action manager
        actionManager = GetComponent<ActionManager>();

        // Then we need to create a copy of our decision tree
        decisionTree = decisionTree.Clone(name);

        // Initialise our decision tree using this object as the meta data
        // This means that every node within the tree will be able to grab a reference from their
        // initialise function to store with them.
        decisionTree.Initialise(this);

        // Start our simple think coroutine
        StartCoroutine(Think());
    }
    
    // A simple think coroutine
    IEnumerator Think()
    {
        while (true)
        {
            // Run our decision tree
            Action actionToBeScheduled = decisionTree.Run();

            // Schedule our action
            actionManager.ScheduleAction(actionToBeScheduled);

            // Execute our action manager
            actionManager.Execute();

            // Wait 100ms - not neccessary but we also don't really want to get a task every frame
            yield return new WaitForSeconds(0.1f);
        }
    }
}
