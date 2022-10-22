using Sxm.ProceduralMeshGenerator.Modification;
using UnityEngine;
using UnityEditor;

namespace Sxm.ProceduralMeshGenerator
{
    public static class MeshModifierMenuCreator
    {
        [MenuItem("GameObject/Mesh Modifiers/Sine", false, 0)]
        public static void Sine()
        {
            AddModifier<SineMeshModifier>("Sine");
        }
        
        [MenuItem("GameObject/Mesh Modifiers/Sine", true)]
        public static bool ValidateSine()
        {
            return IsGameObjectWithComponentSelected<ProceduralMeshGenerator>();
        }
        
        [MenuItem("GameObject/Mesh Modifiers/Ripple", false, 0)]
        public static void Ripple()
        {
            AddModifier<RippleMeshModifier>("Ripple");
        }
        
        [MenuItem("GameObject/Mesh Modifiers/Ripple", true)]
        public static bool ValidateRipple()
        {
            return IsGameObjectWithComponentSelected<ProceduralMeshGenerator>();
        }

        private static void AddModifier<TModifier>(string name) where TModifier : BaseMeshModifier
        {
            var child = new GameObject(name);
            TModifier childModifier = child.AddComponent<TModifier>();
            child.transform.SetParent(Selection.activeTransform);
            
            Selection.activeGameObject.GetComponent<ProceduralMeshGenerator>().AddModifier(childModifier);
            EditorGUIUtility.PingObject(child);
        }
        
        private static bool IsGameObjectWithComponentSelected<TComponent>() where TComponent : Component
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent<TComponent>(out _);
        }
    }
}
