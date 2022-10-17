using System;
using System.Reflection;
using UnityEditor;

namespace Sxm.ProceduralMeshGenerator
{
    public static class SerializedPropertyExtensions
    {
        public static T GetValue<T>(this SerializedProperty property)
        {
            Object targetObject = property.serializedObject.targetObject;
            FieldInfo field = targetObject.GetType().GetField(
                property.propertyPath,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (field == null)
            {
                throw new NullReferenceException($"Unable to get field binded with property '${property.name}'");
            }
            
            return (T) field.GetValue(targetObject);
        }
    }
}
