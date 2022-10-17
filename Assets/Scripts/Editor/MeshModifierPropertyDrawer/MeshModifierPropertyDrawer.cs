using System.Linq;
using Sxm.ProceduralMeshGenerator.Modification;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator
{
    [CustomPropertyDrawer(typeof(MeshModifier))]
    public sealed class MeshModifierPropertyDrawer : PropertyDrawer
    {
        private static readonly string VisibleIconName = "animationvisibilitytoggleon";
        private static readonly string NotVisibleIconName = "animationvisibilitytoggleoff";
        
        private static readonly Color VisibleColor = new Color(0f, 1f, 0f, 0.75f);
        private static readonly Color NotVisibleColor = new Color(1f, 0f, 0f, 0.75f);
        
        private VisualElement _root;
        private Toggle _visibilityToggle;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();
            _root.styleSheets.Add(AssetDatabaseUtils.GetRootStylesFor(nameof(MeshModifierPropertyDrawer)));
            _root.AddToClassList("root");
            
            _visibilityToggle = new Toggle { name = "mesh-modifier-toggle" };
            _visibilityToggle.Children().First().Add(new Image());
            _visibilityToggle.AddToClassList("toggle");
            _root.Add(_visibilityToggle);

            var meshModifierField = new ObjectField
            {
                name = "mesh-modifier",
                objectType = typeof(MeshModifier),
                bindingPath = property.propertyPath
            };
            meshModifierField.RegisterCallback<ChangeEvent<Object>>(OnModifierChanged);
            meshModifierField.AddToClassList("mesh-modifier");
            _root.Add(meshModifierField);

            return _root;
        }

        private void OnModifierChanged(ChangeEvent<Object> evt)
        {
            _visibilityToggle.Q<Image>().SetDisplay(evt.newValue != null);

            if (evt.newValue == null)
            {
                _visibilityToggle.Unbind();
                return;
            }

            var serializedObject = new SerializedObject(evt.newValue);
            var isActiveProperty = serializedObject.FindProperty("isActive");
                                         
            _visibilityToggle.BindProperty(isActiveProperty);
            _root.RegisterAndSetField<Toggle, bool>(_visibilityToggle.name, serializedObject, OnVisibilityToggleChanged);
                                         
            serializedObject.ApplyModifiedProperties();
        }

        private static void OnVisibilityToggleChanged(Toggle toggle, bool isVisibility)
        {
            var icon = toggle.Q<Image>();
            
            icon.image = EditorGUIUtility.IconContent(isVisibility ? VisibleIconName : NotVisibleIconName).image;
            icon.tintColor = isVisibility ? VisibleColor : NotVisibleColor;
        }
    }
}
