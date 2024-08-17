#if UNITY_EDITOR

using UnityEngine.UIElements;
using UnityEditor;
using KieranCoppins.DecisionTrees;

namespace KieranCoppins.DecisionTreesEditor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { };
        Editor editor;
        public InspectorView()
        {

        }

        /// <summary>
        /// Updates the inspector
        /// </summary>
        /// <param name="nodeView">The node we want to display in the inspector</param>
        public void UpdateSelection(BaseNodeView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(nodeView.Node);

            IMGUIContainer container = new(() => { 
                if (editor.target)
                    editor.OnInspectorGUI(); 
            });
            Add(container);
        }
    }
}

#endif