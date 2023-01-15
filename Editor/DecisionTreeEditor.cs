using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using KieranCoppins.DecisionTrees;

namespace KieranCoppins.DecisionTreesEditor
{
    /// <summary>
    /// The main class of our decision tree visual editor
    /// </summary>
    public class DecisionTreeEditor : EditorWindow
    {

        private InspectorView _inspectorView;
        protected DecisionTreeView TreeView;

        [MenuItem("Window/AI/Decision Tree Editor")]
        public static void ShowExample()
        {
            // Get all types that have the type of DecisionTreeEditor
            var types = TypeCache.GetTypesDerivedFrom<DecisionTreeEditor>();
            foreach (var type in types)
            {
                var customWnd = GetWindow(type);
                customWnd.titleContent = new GUIContent("Decision Tree Editor");
                return;
            }
            // Otherwise we had no assets so just load this one
            DecisionTreeEditor wnd = GetWindow<DecisionTreeEditor>();
            wnd.titleContent = new GUIContent("Decision Tree Editor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is DecisionTree)
            {
                ShowExample();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.kierancoppins.decision-trees/Editor/DecisionTreeEditor.uxml");
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.kierancoppins.decision-trees/Editor/DecisionTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            _inspectorView = root.Q<InspectorView>();
            TreeView = root.Q<DecisionTreeView>();
            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }


        public virtual void OnSelectionChange()
        {
            DecisionTree decisionTree = Selection.activeObject as DecisionTree;
            if (decisionTree && AssetDatabase.CanOpenAssetInEditor(decisionTree.GetInstanceID()))
                TreeView.PopulateView(decisionTree);
        }

        /// <summary>
        /// Runs when our node selection has changed
        /// </summary>
        /// <param name="nodeView">The node we now have selected</param>
        void OnNodeSelectionChanged(BaseNodeView nodeView)
        {
            _inspectorView.UpdateSelection(nodeView);
        }

        private void OnInspectorUpdate()
        {
            TreeView?.UpdateNodeStates();
        }
    }
}