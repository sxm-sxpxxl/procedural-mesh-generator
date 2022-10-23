using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator
{
    public static class VisualElementExtensions
    {
        private static readonly StyleEnum<DisplayStyle> DisplayedStyle = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        private static readonly StyleEnum<DisplayStyle> NotDisplayedStyle = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    
        public static void SetDisplay(this VisualElement visualElement, bool isDisplayed)
        {
            var displayStyle = isDisplayed ? DisplayedStyle : NotDisplayedStyle;
            visualElement.style.display = displayStyle;
        }
        
        public static void RegisterAndSetField<TFieldType, TValueType>(
            this TFieldType field,
            SerializedProperty targetProperty,
            Action<TFieldType, TValueType> fieldChangeCallback
        ) where TFieldType : BaseField<TValueType>
        {
            field.RegisterCallback<ChangeEvent<TValueType>>(evt =>
            {
                fieldChangeCallback.Invoke(evt.currentTarget as TFieldType, evt.newValue);
            });

            var initValue = targetProperty.GetValue<TValueType>();
            field.SetValueWithoutNotify(initValue);
            fieldChangeCallback.Invoke(field, initValue);
        }
        
        public static void RegisterAndSetField<TFieldType, TValueType>(
            this VisualElement root,
            string fieldName,
            SerializedObject parentSerializedObject,
            Action<TFieldType, TValueType> fieldChangeCallback
        ) where TFieldType : BaseField<TValueType>
        {
            TFieldType field = root.Q<TFieldType>(fieldName);
            SerializedProperty childProperty = GetBindedPropertyFor(parentSerializedObject, field);
            
            RegisterAndSetField(field, childProperty, fieldChangeCallback);
        }
        
        private static SerializedProperty GetBindedPropertyFor(
            SerializedObject serializedObject,
            BindableElement bindableElement
        ) => serializedObject.FindProperty(bindableElement.bindingPath);
    }
}
