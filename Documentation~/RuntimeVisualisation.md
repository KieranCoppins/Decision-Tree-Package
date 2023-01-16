![banner](Resources/RuntimeVisualisation.png)
# Runtime Visualisation
The decision tree visual editor does support runtime debugging. It allows for programmers to click on Game Objects that are running the decision tree and highlight which nodes are currently running.

This does require a small amount of setup as it depends entirely on which objects you want to monitor. For this tutorial I will discuss how to do this with the Sample Assets provided in the package.

### Creating a Custom Editor with your changes
So first we need to create a new editor that inherits the one provided in the package. If you don't have one already, create an Editor folder in the root of your assets directory.
Inside this editor folder, we need to create a new custom editor. You can name this whatever you want but make sure it inherits `DecisionTreeEditor` and dont forget to include the decision tree editor namespace `KieranCoppins.DecisionTreesEditor` Here is an example empty class:

```c#
using KieranCoppins.DecisionTreeEditor;

public class CustomDecisionTreeViewer : DecisionTreeEditor
{

}
```
**Note**: If there are multiple custom editors that inherit `DecisionTreeEditor` it will just pick the first one it can find. There is no garentee as to which one that is

Next we need to override the OnSelectionChange function and add our functionality to select our object and read the decision tree attached. Below is the full code if you just want to copy and paste, but below it I will explain further what this does.

```c#
public override void OnSelectionChange()
{
    DecisionTree decisionTree = Selection.activeObject as DecisionTree;
    if (!decisionTree)
    {
        DTRunner dtRunner = Selection.activeObject?.GetComponent<DTRunner>();
        if (dtRunner?.DecisionTree && Application.isPlaying)
            TreeView.PopulateView(dtRunner.DecisionTree);
    }
    else
    {
        base.OnSelectionChange();
    }
}
```

**Note**: If your IDE hasn't done it automatically make sure to add `using Unity.VisualScripting` to access the `GetComponent` function of `Selection.activeObject`.

To begin with we get our active selection and try cast it directly as a Decision Tree. If we have selected a decision tree then we can just load that one like normal. However, if the cast returns null that means we haven't selected a decision tree. Therefore we *might* have selected our object that has a component that uses the decision tree.

To extract the decision tree we just have to get the comonent that has it from our selected object. Then check if our component has a decision tree set. We also want to make sure we are in playmode so we have loaded the cloned version of the tree that the component would be using. Once we have confirmed this we can just populate the view with that decision tree rather than the saved decision tree in the asset database.

You can check if the visual editor is using your custom one by checking the title of the decision tree editor. It should contain (custom) at the end of the title. You can also just put a debug log inside your custom editor's OnSelectionChange.

And thats about it! Now you should be able to click on your version of the DTRunner and see nodes that are running have been highlighted