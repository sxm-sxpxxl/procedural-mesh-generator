using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator.Editor
{
    // https://forum.unity.com/threads/uxmltraits-and-custom-attributes-resetting-in-inspector.966215/#post-6311601
    public sealed class FoldoutGroup : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<FoldoutGroup, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _labelAttribute = new UxmlStringAttributeDescription()
            {
                name = "label",
                defaultValue = "Foldout label"
            };
            
            private readonly UxmlBoolAttributeDescription _expandedAttribute = new UxmlBoolAttributeDescription
            {
                name = "expanded",
                defaultValue = true
            };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                
                var target = visualElement as FoldoutGroup;
                target.Label = _labelAttribute.GetValueFromBag(bag, context);
                target.Expanded = _expandedAttribute.GetValueFromBag(bag, context);
            }
        }
        
        private static readonly string FoldoutGroupClassName = "foldout-group";
        private static readonly string ContentName = "content";
        
        private readonly Toggle _toggle;
        private readonly VisualElement _content;
        private string _label;
        private bool _expanded;

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                _toggle.label = _label;
            }
        }

        public bool Expanded
        {
            get => _expanded;
            set
            {
                _expanded = value;
                _toggle.SetValueWithoutNotify(_expanded);
                _content.SetDisplay(_expanded);
            }
        }
        
        public override VisualElement contentContainer => _content;
        
        public FoldoutGroup()
        {
            _toggle = new Toggle();
            _toggle.RegisterCallback<ChangeEvent<bool>>(OnToggleChange);
            _toggle.AddToClassList("foldout-group__toggle");
            hierarchy.Add(_toggle);
            
            _content = new VisualElement { name = ContentName };
            hierarchy.Add(_content);
            
            AddToClassList(FoldoutGroupClassName);
            styleSheets.Add(AssetDatabaseUtils.GetRelativeStyle());
        }
        
        private static void OnToggleChange(ChangeEvent<bool> evt)
        {
            ((evt.currentTarget as VisualElement).parent as FoldoutGroup).Expanded = evt.newValue;
        }
    }
}
