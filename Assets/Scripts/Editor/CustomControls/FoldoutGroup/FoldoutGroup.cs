using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator
{
    public sealed class FoldoutGroup : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<FoldoutGroup, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription _labelAttribute = new UxmlStringAttributeDescription()
            {
                name = "Label",
                defaultValue = "Foldout label"
            };
            
            private UxmlBoolAttributeDescription _expandedAttribute = new UxmlBoolAttributeDescription
            {
                name = "Expanded",
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

        private Toggle _toggle;
        private VisualElement _content;
        
        private string _label;
        private bool _expanded;

        public override VisualElement contentContainer => _content;
        
        private bool Expanded
        {
            get => _expanded;
            set => _toggle.value = _expanded = value;
        }
        
        private string Label
        {
            get => _label;
            set => _toggle.label = _label = value;
        }

        public FoldoutGroup()
        {
            _toggle = new Toggle();
            _toggle.RegisterCallback<ChangeEvent<bool>>(evt => OnToggleChange(evt));
            _toggle.AddToClassList("foldout-group__toggle");
            hierarchy.Add(_toggle);
            
            _content = new VisualElement { name = ContentName };
            hierarchy.Add(_content);

            AddToClassList(FoldoutGroupClassName);

            var relativePath = AssetDatabaseUtils.GetAssetPathFor(nameof(FoldoutGroup), "Assets/Scripts");
            var ussPath = Path.ChangeExtension(relativePath, "uss");

            StyleSheet asset = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            styleSheets.Add(asset);
        }

        private static void OnToggleChange(ChangeEvent<bool> evt)
        {
            var content = (evt.currentTarget as VisualElement).parent.Q<VisualElement>(ContentName);
            content.style.display = new StyleEnum<DisplayStyle>(evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            evt.StopPropagation();
        }
    }
}
