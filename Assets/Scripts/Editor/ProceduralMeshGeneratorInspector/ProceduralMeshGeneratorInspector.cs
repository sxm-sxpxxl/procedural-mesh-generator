using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;

namespace Sxm.ProceduralMeshGenerator
{
    [CustomEditor(typeof(ProceduralMeshGenerator))]
    public class ProceduralMeshGeneratorInspector : Editor
    {
        [SerializeField] private VisualTreeAsset template;
        
        private VisualElement _root;
        private DetailedListViewController<BaseMeshModifier> _detailedListViewController;

        private static readonly (string, MeshType)[] AvailableMeshTypes = new (string, MeshType)[]
        {
            ("plane-type", MeshType.Plane),
            ("cube-type", MeshType.Cube),
            ("sphere-type", MeshType.Sphere)
        };
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            template.CloneTree(_root);
            
            _root.RegisterAndSetField<Toggle, bool>("show-vertices", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetField<Toggle, bool>("show-labels", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetField<Toggle, bool>("show-normals", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetField<EnumField, Enum>("mesh-type", serializedObject, OnMeshTypeChanged);
            
            _detailedListViewController = new DetailedListViewController<BaseMeshModifier>(
                serializedObject,
                dragDropContainer: _root.Q<VisualElement>("drag-and-drop-container"),
                clearButton: _root.Q<Button>("clear-button"),
                cleanButton: _root.Q<Button>("clean-button"),
                listView: _root.Q<ListView>("modifiers"),
                selectedItem: _root.Q<ObjectField>("selected-modifier"),
                itemDetailsContainer: _root.Q<VisualElement>("selected-modifier-container")
            );
            
            return _root;
        }
        
        private static void OnMeshTypeChanged(EnumField meshTypeField, Enum newValue)
        {
            var root = meshTypeField.parent;
            var meshType = (MeshType) newValue;
            
            AvailableMeshTypes.ForEach(x =>
            {
                var (name, type) = x;
                root.Q<VisualElement>(name).SetDisplay(meshType == type);
            });
            
            root.Q<VisualElement>("size-2d").SetDisplay(meshType == MeshType.Plane);
            root.Q<VisualElement>("size-3d").SetDisplay(meshType != MeshType.Plane);
        }
        
        private static void OnDebugOptionToggleChanged(Toggle toggle, bool newValue)
        {
            toggle.parent.Children()
                .Where(x => x.name != toggle.name)
                .ForEach(x => x.SetDisplay(newValue));
        }
    }
}
