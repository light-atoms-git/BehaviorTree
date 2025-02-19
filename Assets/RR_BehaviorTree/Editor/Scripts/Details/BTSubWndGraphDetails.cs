using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTSubWndGraphDetails : BaseSubWindow
    {
        private BTDetailsPropFieldFactory _propFieldFactory;
        private TextField _nameField, _descField;
        private VisualElement _taskPropsContentContainer;
        private float _height, _width;


        public BTSubWndGraphDetails(UnityEngine.Rect rect)
        {
            _propFieldFactory = new BTDetailsPropFieldFactory();

            style.backgroundColor = RR.Utils.ColorExtension.Create(96f);
            SetPosition(rect);
            _height = rect.height;
            _width = rect.width;
            
            var titleContainer = Title("Details");
            Add(titleContainer);

            var generalContainer = CreateGeneralContainer();
            Add(generalContainer);

            var taskPropsContainer = CreateTaskPropsContainer();
            Add(taskPropsContainer);
        }

        private VisualElement CreateContainerBase(string labelText, VisualElement content)
        {
            var container = new VisualElement();
            container.style.backgroundColor = RR.Utils.ColorExtension.Create(62f);

            var label = new Label(labelText);
            label.style.backgroundColor = RR.Utils.ColorExtension.Create(30f);
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            label.style.paddingLeft = 5f;
            label.style.paddingTop = 2f;
            label.style.paddingBottom = 2f;

            content.style.paddingTop = 2f;
            content.style.paddingBottom = 2f;

            AddVerticalSpaceToFields(content);

            container.Add(label);
            container.Add(content);

            return container;
        }

        private void AddVerticalSpaceToFields(VisualElement container)
        {
            foreach (var child in container.Children())
            {
                child.style.paddingBottom = 2f;
                child.style.paddingTop = 2f;
            }
        }

        private VisualElement CreateGeneralContainer()
        {
            var container = new VisualElement();
            _nameField = new TextField("Name");
            _nameField.labelElement.style.marginRight = -70;
            _descField = new TextField("Description") { multiline = true };
            _descField.labelElement.style.marginRight = -70;
            _descField.style.whiteSpace = WhiteSpace.Normal;

            container.Add(_nameField);
            container.Add(_descField);

            return CreateContainerBase("General", container);
        }

        private VisualElement CreateTaskPropsContainer()
        {
            var container = new VisualElement();
            _taskPropsContentContainer = new VisualElement();
            _taskPropsContentContainer.style.paddingLeft = 2f;
            container.Add(_taskPropsContentContainer);
            return CreateContainerBase("Properties", container);
        }

        public void ShowNodeInfo(string name, string desc)
        {
            _nameField.value = name;
            _descField.value = desc;
        }

        public void DrawTaskProperties(object propFieldValue, System.Type propType, GraphBlackboard blackboard)
        {
            ClearTaskPropsContent();

            var serializableAttribs = propType.GetCustomAttributes(typeof(System.SerializableAttribute), true);
            
            if (serializableAttribs.Length == 0)
            {
                Debug.LogError($"{propType} must be Serializable");
                return;
            }

            var container = new VisualElement();
            var fieldInfoList = propType.GetFields();

            foreach (var fieldInfo in fieldInfoList)
            {
                var childContainer = CreatePropFieldContainer();
                childContainer.Add(CreatePropLabel(fieldInfo.Name, 150f)); 

                var field = DrawPropField(fieldInfo, propFieldValue, blackboard);
                childContainer.Add(field);

                container.Add(childContainer);
            }

            AddVerticalSpaceToFields(container);
            _taskPropsContentContainer.Add(container);
        }

        private VisualElement CreatePropFieldContainer()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            return container;
        }

        private VisualElement CreatePropLabel(string text, float width)
        {
            var label = new Label(RR.Utils.StringUtility.InsertWhiteSpaces(text));
            label.style.width = width;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 12;
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }

        private VisualElement DrawPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            var type = fieldInfo.FieldType;
            return StylizePropField(_propFieldFactory.PropField(type, fieldInfo, propFieldValue, blackboard));
        }

        private VisualElement StylizePropField(VisualElement field)
        {
            field.style.width = _width - 150f - 14f;
            return field;
        }

        public void ClearTaskPropsContent() => _taskPropsContentContainer.Clear();

        // public void ShowTaskProp(SerializedProperty prop)
        // {
        //     _taskPropContainer.Clear();

        //     if (prop == null)
        //     {
        //         return;
        //     }

        //     foreach (SerializedProperty propField in prop)
        //     {
        //         var UIField = new UnityEditor.UIElements.PropertyField(propField, propField.displayName);
        //         _taskPropContainer.Add(UIField);
        //     }
        // }
    }
}
