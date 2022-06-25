using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTEditorWindow : GraphViewEditorWindow
    {
        private BTGraphView _graphView;
        private BehaviorTree _inspectedBT;
        private BTNodeSearchWindow _searchWindow;
        private Toolbar _toolbar;

        public static System.Action OnClose { get; set; }

        public static void Init(BehaviorTree behaviorTree)
        {
            var window = GetWindow<BTEditorWindow>("Behavior Tree");
            window._inspectedBT = behaviorTree;
            window._graphView = window.CreateGraphView(behaviorTree.DesignContainer);
            window._searchWindow = window.CreateNodeSearchWindow(window._graphView);
            window._toolbar = window.CreateToolbar();
            window.rootVisualElement.Add(window._graphView);
            window.rootVisualElement.Add(window._toolbar);
        }

        private BTGraphView CreateGraphView(BTDesignContainer designContainer)
        {
            var graphView = new BTGraphView(designContainer);
            graphView.StretchToParentSize();
            return graphView;
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();          

            var saveBtn = new Button(() => 
            { 
                _inspectedBT.DesignContainer.Save();
            }) { text = "Save" };

            var cleanupBtn = new Button(() => 
            { 
                _inspectedBT.DesignContainer.Cleanup();
            }) { text = "Cleanup" };

            var settingsBtn = new Button(() => 
            {
                _graphView.OpenGraphSettingsWnd();
            }) { text = "Settings" };

            toolbar.Add(saveBtn);
            toolbar.Add(cleanupBtn);
            toolbar.Add(settingsBtn);

            return toolbar;
        }

        private BTNodeSearchWindow CreateNodeSearchWindow(BTGraphView graphView)
        {
            var window = UnityEngine.ScriptableObject.CreateInstance<BTNodeSearchWindow>();
            graphView.nodeCreationRequest += context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), window);
            System.Func<Vector2, Vector2> contextToLocalMousePos = contextMousePos =>
            {
                var worldPos = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, contextMousePos - position.position);
                return graphView.WorldToLocal(worldPos);
            };

            window.Init((nodeType, pos) => 
            {
                var localMousePos = contextToLocalMousePos(pos);
                var node = BTGraphNodeFactory.CreateDefaultGraphNode(
                    nodeType, 
                    _graphView.GetBlackboard() as GraphBlackboard, 
                    localMousePos,
                    graphView.DesignContainer.GetOrCreateTask);
                graphView.AddNode(node, localMousePos);
            });

            return window;
        }

        private void OnDisable()
        {
            OnClose?.Invoke();

            if (OnClose != null)
            {
                foreach (var listener in OnClose.GetInvocationList())
                {
                    OnClose -= (System.Action) listener;
                }
            }  

            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }

            if (_toolbar != null)
            {
                rootVisualElement.Remove(_toolbar);
            }
        }
    }
}
