using System;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;
using Object = UnityEngine.Object;

namespace Sxm.ProceduralMeshGenerator.Editor
{
    [CustomEditor(typeof(ProceduralMeshGenerator))]
    public class ProceduralMeshGeneratorInspector : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset template;

        private ProceduralMeshGenerator _target;
        private VisualElement _root;
        private DetailedListViewController<BaseMeshModifier> _detailedListViewController;

        private static readonly (string, MeshType)[] AvailableMeshTypes = new (string, MeshType)[]
        {
            ("plane-type", MeshType.Plane),
            ("cube-type", MeshType.Cube),
            ("sphere-type", MeshType.Sphere)
        };

        private void OnEnable()
        {
            _target = target as ProceduralMeshGenerator;
            _target.OnMeshUpdated += OnMeshUpdated;
        }

        private void OnDisable()
        {
            _target.OnMeshUpdated -= OnMeshUpdated;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            template.CloneTree(_root);
            
            _root.RegisterAndSetNestedField<Toggle, bool>("show-vertices", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetNestedField<Toggle, bool>("show-labels", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetNestedField<Toggle, bool>("show-normals", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetNestedField<Toggle, bool>("show-bounds", serializedObject, OnDebugOptionToggleChanged);
            _root.RegisterAndSetField<EnumField, Enum>("mesh-type", serializedObject, OnMeshTypeChanged);
            _root.RegisterAndSetField<Toggle, bool>("backface-culling", serializedObject, OnBackfaceCullingChanged);
            
            _detailedListViewController = new DetailedListViewController<BaseMeshModifier>(
                serializedObject,
                dragDropContainer: _root.Q<VisualElement>("drag-and-drop-container"),
                clearButton: _root.Q<Button>("clear-button"),
                cleanButton: _root.Q<Button>("clean-button"),
                listView: _root.Q<ListView>("modifiers"),
                selectedItemPropertyField: _root.Q<PropertyField>("selected-modifier"),
                targetDetailsContainer: _root.Q<VisualElement>("selected-modifier-container"),
                getTargetProperty: GetTargetPropertyForMeshModifier,
                setTargetForItemProperty: SetMeshModifierProperty,
                initialSelectedIndex: GetSelectedModifierIndexProperty().intValue
            );
            
            _detailedListViewController.OnSelectedItemIndexChanged += OnSelectedItemIndexChanged;
            
            return _root;
        }

        private void OnMeshUpdated()
        {
            _root.Q<Label>("vertices-size").text = _target.VerticesDebugInfo;
            _root.Q<Label>("triangles-size").text = _target.TrianglesDebugInfo;
            _root.Q<Label>("bounds-value").text = _target.BoundsDebugInfo;
        }
        
        private void OnSelectedItemIndexChanged(int index)
        {
            GetSelectedModifierIndexProperty().intValue = index;
            serializedObject.ApplyModifiedProperties();
        }
        
        private static void SetMeshModifierProperty(SerializedProperty meshModifierProperty, Object meshModifier)
        {
            Assert.IsTrue(meshModifier is BaseMeshModifier or null);
            GetActivePropertyForMeshModifier(meshModifierProperty).boolValue = true;
            GetTargetPropertyForMeshModifier(meshModifierProperty).objectReferenceValue = meshModifier;
        }
        
        private SerializedProperty GetSelectedModifierIndexProperty() =>
            serializedObject.FindProperty("selectedModifierIndex");
        
        private static SerializedProperty GetActivePropertyForMeshModifier(SerializedProperty meshModifierProperty) =>
            meshModifierProperty.FindPropertyRelative("isActive");
        
        private static SerializedProperty GetTargetPropertyForMeshModifier(SerializedProperty meshModifierProperty) =>
            meshModifierProperty.FindPropertyRelative("target");
        
        private static void OnBackfaceCullingChanged(Toggle backfaceCullingToggle, bool newValue)
        {
            backfaceCullingToggle.parent.Q<Toggle>("forward-facing").SetDisplay(newValue);
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
        
        private static void OnDebugOptionToggleChanged(Toggle debugOptionToggle, bool newValue)
        {
            debugOptionToggle.parent.Children()
                .Where(x => x.name != debugOptionToggle.name)
                .ForEach(x => x.SetDisplay(newValue));
        }
    }
}
