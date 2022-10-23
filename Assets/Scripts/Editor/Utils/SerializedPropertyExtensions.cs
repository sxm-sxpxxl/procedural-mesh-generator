using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator
{
    public static class SerializedPropertyExtensions
    {
        // https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-7405697
        public static T GetValue<T>(this SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            
            string[] propertyNames = property.propertyPath.Split('.');
            var propertyNamesClean = new List<String>(capacity: propertyNames.Length);
            
            for (int i = 0; i < propertyNames.Length; i++)
            {
                if (propertyNames[i] == "Array")
                {
                    if (i != (propertyNames.Length - 1) && propertyNames[i + 1].StartsWith("data"))
                    {
                        int index = int.Parse(propertyNames[i + 1].Split('[', ']')[1]);
                        propertyNamesClean.Add($"-GetArray_{index}");
                        i++;
                    }
                    else
                    {
                        propertyNamesClean.Add(propertyNames[i]);
                    }
                }
                else
                {
                    propertyNamesClean.Add(propertyNames[i]);
                }
            }
            
            // Get the last object of the property path.
            foreach (string path in propertyNamesClean)
            {
                if (path.StartsWith("-GetArray"))
                {
                    string[] split = path.Split('_');
                    int index = int.Parse(split[split.Length - 1]);
                    
                    IList list = (IList) targetObject;
                    targetObject = list[index];
                }
                else
                {
                    targetObject = targetObject.GetType()
                        .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(targetObject);
                }
            }
 
            return (T) targetObject;
        }
    }
}
