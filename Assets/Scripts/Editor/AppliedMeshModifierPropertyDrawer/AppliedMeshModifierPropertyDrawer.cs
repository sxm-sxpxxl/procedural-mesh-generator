using System.Linq;
using Sxm.ProceduralMeshGenerator.Modification;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator.Editor
{
    [CustomPropertyDrawer(typeof(ProceduralMeshGenerator.AppliedMeshModifier))]
    public sealed class AppliedMeshModifierPropertyDrawer : PropertyDrawer
    {
        private static readonly string VisibleIconName = "animationvisibilitytoggleon";
        private static readonly string NotVisibleIconName = "animationvisibilitytoggleoff";
        
        private static readonly Color VisibleColor = new Color(0f, 1f, 0f, 0.75f);
        private static readonly Color NotVisibleColor = new Color(1f, 0f, 0f, 0.75f);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabaseUtils.GetRootStylesFor(nameof(AppliedMeshModifierPropertyDrawer)));
            root.AddToClassList("root");
            
            InitVisibilityToggle(root, property);
            InitMeshModifierObjectField(root, property);
            
            return root;
        }

        private void InitVisibilityToggle(VisualElement container, SerializedProperty property)
        {
            var visibilityToggle = new Toggle { bindingPath = "isActive" };
            visibilityToggle.Children().First().Add(new Image());
            visibilityToggle.AddToClassList("toggle");
            
            container.Add(visibilityToggle);
            
            SerializedProperty activeProperty = property.FindPropertyRelative(visibilityToggle.bindingPath);
            visibilityToggle.RegisterAndSetField<Toggle, bool>(activeProperty, OnVisibilityToggleChanged);
        }

        private void InitMeshModifierObjectField(VisualElement container, SerializedProperty property)
        {
            var modifierField = new ObjectField { objectType = typeof(BaseMeshModifier), bindingPath = "target" };
            modifierField.AddToClassList("mesh-modifier");
            
            container.Add(modifierField);
            
            SerializedProperty modifierProperty = property.FindPropertyRelative(modifierField.bindingPath);
            modifierField.RegisterAndSetField<ObjectField, Object>(modifierProperty, OnModifierChanged);
        }

        private static void OnModifierChanged(ObjectField modifierField, Object value)
        {
            modifierField.parent.Q<Toggle>().SetDisplay(value != null);
        }

        private static void OnVisibilityToggleChanged(Toggle toggle, bool isVisibility)
        {
            var icon = toggle.Q<Image>();
            
            icon.image = EditorGUIUtility.IconContent(isVisibility ? VisibleIconName : NotVisibleIconName).image;
            icon.tintColor = isVisibility ? VisibleColor : NotVisibleColor;
        }
    }
}
