using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { };
    Editor editor;
    public InspectorView()
    {

    }

    public void UpdateSelection(BaseNodeView nodeView)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(nodeView.node);

        IMGUIContainer container = new(() => { editor.OnInspectorGUI(); });
        Add(container);
    }
}
