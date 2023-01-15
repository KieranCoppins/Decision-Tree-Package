using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using KieranCoppins.DecisionTrees;
using UnityEditor.UIElements;

namespace KieranCoppins.DecisionTreesEditor
{
    /// <summary>
    /// The main class of our decision tree visual editor
    /// </summary>
    public class DecisionTreeEditor : EditorWindow
    {

        private InspectorView _inspectorView;

        private VisualElement _leftPanel;
        private VisualElement _dragLine;

        protected ToolbarMenu OptionMenu;

        protected DecisionTreeView TreeView;

        private bool _simpleNodeView = true;

        public System.Action<bool> OnSimpleNodeViewChanged;

        [MenuItem("Window/AI/Decision Tree Editor")]
        public static void ShowExample()
        {
            // Get all types that have the type of DecisionTreeEditor
            var types = TypeCache.GetTypesDerivedFrom<DecisionTreeEditor>();
            foreach (var type in types)
            {
                var customWnd = GetWindow(type);
                customWnd.titleContent = new GUIContent("Decision Tree Editor (Custom)");
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

        public virtual void CreateGUI()
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
            _leftPanel = root.Q<VisualElement>("left-panel");
            _dragLine = root.Q<VisualElement>("unity-dragline-anchor");
            TreeView = root.Q<DecisionTreeView>();
            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            OptionMenu = root.Q<ToolbarMenu>();
            OnSimpleNodeViewChanged += OnSimpleNodeClick;

            UpdateOptionMenu();

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


        void UpdateOptionMenu()
        {
            // Clear our option menu off all items
            OptionMenu.menu.MenuItems().Clear();

            // Re add items
            if (_simpleNodeView)
                OptionMenu.menu.AppendAction("Simple node view", action => { _simpleNodeView = false; OnSimpleNodeViewChanged?.Invoke(_simpleNodeView); }, action => DropdownMenuAction.Status.Checked);
            else
                OptionMenu.menu.AppendAction("Simple node view", action => { _simpleNodeView = true; OnSimpleNodeViewChanged?.Invoke(_simpleNodeView); }, action => DropdownMenuAction.Status.Normal);
        }

        void OnSimpleNodeClick(bool simpleNodeView)
        {
            // Hide the inspector and side view line
            DisplayStyle displayStyle = simpleNodeView ? DisplayStyle.Flex : DisplayStyle.None;
            _leftPanel.style.display = displayStyle;
            _dragLine.style.display = displayStyle;

            // Update our options
            UpdateOptionMenu();

            // Update our node view
            OnSelectionChange();
        }


        public virtual void OnSelectionChange()
        {
            DecisionTree decisionTree = Selection.activeObject as DecisionTree;
            if (decisionTree && AssetDatabase.CanOpenAssetInEditor(decisionTree.GetInstanceID()))
                TreeView.PopulateView(decisionTree, _simpleNodeView);
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