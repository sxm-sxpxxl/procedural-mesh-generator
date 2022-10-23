using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Sxm.ProceduralMeshGenerator.Modification;
using Object = UnityEngine.Object;

namespace Sxm.ProceduralMeshGenerator
{
    public sealed class DetailedListViewController<TItemType> where TItemType : Component
    {
        private static readonly string ScriptPropertyName = "m_Script";
        private static readonly KeyCode RemoveKey = KeyCode.Delete;
        
        private SerializedObject _parentSerializedObject;
        private ListView _listView;
        private ObjectField _selectedTargetField;
        private VisualElement _targetDetailsContainer;
        private Func<SerializedProperty, SerializedProperty> _getTargetProperty;
        private Action<SerializedProperty, Object> _setTargetForItemProperty;
        
        private SerializedProperty ListProperty => _parentSerializedObject.FindProperty(_listView.bindingPath);
        
        public DetailedListViewController(
            SerializedObject parentSerializedObject,
            VisualElement dragDropContainer,
            Button clearButton,
            Button cleanButton,
            ListView listView,
            ObjectField selectedTargetField,
            VisualElement targetDetailsContainer,
            Func<SerializedProperty, SerializedProperty> getTargetProperty,
            Action<SerializedProperty, Object> setTargetForItemProperty
        )
        {
            _parentSerializedObject = parentSerializedObject;
            _listView = listView;
            _selectedTargetField = selectedTargetField;
            _targetDetailsContainer = targetDetailsContainer;
            _getTargetProperty = getTargetProperty;
            _setTargetForItemProperty = setTargetForItemProperty;
            
            dragDropContainer.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            dragDropContainer.RegisterCallback<DragPerformEvent>(OnDragPerformed);
            
            clearButton.RegisterCallback<ClickEvent>(OnClearButtonClicked);
            cleanButton.RegisterCallback<ClickEvent>(OnCleanButtonClicked);
            
            _listView.selectionType = SelectionType.Single;
            _listView.onSelectionChange += OnSelectionItemChanged;
            _listView.itemIndexChanged += OnItemReordered;
            _listView.itemsAdded += OnItemAdded;
            _listView.itemsRemoved += OnItemRemoved;
            _listView.RegisterCallback<KeyDownEvent>(OnRemoveKeyDown);
            
            _selectedTargetField.objectType = typeof(TItemType);
            _selectedTargetField.SetEnabled(false);
        }
        
        private void OnClearButtonClicked(ClickEvent evt)
        {
            ListProperty.ClearArray();
            
            _parentSerializedObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
        
        private void OnCleanButtonClicked(ClickEvent evt)
        {
            for (int i = 0; i < ListProperty.arraySize;)
            {
                if (_getTargetProperty.Invoke(ListProperty.GetArrayElementAtIndex(i)).objectReferenceValue == null)
                {
                    RemoveItemFromListByIndex(i);
                }
                else
                {
                    i++;
                }
            }
        }
        
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (IsGameObjectWithComponentDragged(out TItemType _) == false)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }
        
        private void OnDragPerformed(DragPerformEvent evt)
        {
            if (IsGameObjectWithComponentDragged(out TItemType modifier) == false)
            {
                return;
            }
            
            AddTargetForItemToList(modifier);
            SetLastSelectionItem();
        }
        
        private void OnSelectionItemChanged(IEnumerable<object> selectionItems)
        {
            var items = selectionItems.ToArray();
            if (items.Length != 1)
            {
                return;
            }
                
            var selectionItemProperty = items[0] as SerializedProperty;
            RebindSelectedTargetFor(selectionItemProperty);
        }
        
        private void OnItemReordered(int oldIndex, int newIndex)
        {
            _listView.selectedIndex = newIndex;
            _listView.Rebuild();
        }

        private void OnItemAdded(IEnumerable<int> itemIndices)
        {
            foreach (var itemIndex in itemIndices)
            {
                SetTargetForItemByIndex(itemIndex, newTarget: null);
            }
        }
        
        private void OnItemRemoved(IEnumerable<int> itemIndices)
        {
            if (ListProperty.arraySize == 0)
            {
                ResetSelectedItem();
            }
        }
        
        private void OnRemoveKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != RemoveKey)
            {
                return;
            }
            
            RemoveItemFromListByIndex(_listView.selectedIndex);

            if (ListProperty.arraySize == 0)
            {
                ResetSelectedItem();
            }
            
            if (_listView.selectedIndex == ListProperty.arraySize)
            {
                SetLastSelectionItem();
            }
        }

        private void AddTargetForItemToList(Object target)
        {
            var listProperty = ListProperty;
            
            listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
            int lastIndex = listProperty.arraySize - 1;

            SetTargetForItemByIndex(lastIndex, newTarget: target);
            _listView.RefreshItems();
        }
        
        private void RemoveItemFromListByIndex(int index)
        {
            ListProperty.DeleteArrayElementAtIndex(index);
            
            _parentSerializedObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
        
        private void SetTargetForItemByIndex(int index, Object newTarget)
        {
            var itemProperty = ListProperty.GetArrayElementAtIndex(index);
            _setTargetForItemProperty.Invoke(itemProperty, newTarget);

            _parentSerializedObject.ApplyModifiedProperties();
        }
        
        private void SetLastSelectionItem()
        {
            int lastIndex = ListProperty.arraySize - 1;
            SetSelectionItemByIndex(lastIndex);
        }
        
        private void SetSelectionItemByIndex(int index)
        {
            if (index < 0 || index >= ListProperty.arraySize)
            {
                return;
            }
            
            SerializedProperty selectionItemProperty = ListProperty.GetArrayElementAtIndex(index);
            
            _listView.SetSelectionWithoutNotify(new int[] { index });
            RebindSelectedTargetFor(selectionItemProperty);
        }
        
        private void RebindSelectedTargetFor(SerializedProperty itemProperty)
        {
            var targetProperty = _getTargetProperty.Invoke(itemProperty);
            
            _selectedTargetField.Unbind();
            _selectedTargetField.BindProperty(targetProperty);
            
            _selectedTargetField.RegisterCallback<ChangeEvent<Object>>(evt =>
            {
                UpdateTargetDetails(evt.newValue);
            });
            
            UpdateTargetDetails(targetProperty.objectReferenceValue);
        }
        
        private void UpdateTargetDetails(Object item)
        {
            _targetDetailsContainer.Clear();
            if (item == null)
            {
                return;
            }
            
            var serializedItem = new SerializedObject(item);
            var currentChildProperty = serializedItem.GetIterator();

            while (currentChildProperty.NextVisible(enterChildren: true))
            {
                if (currentChildProperty.name == ScriptPropertyName)
                {
                    continue;
                }
                    
                var propertyField = new PropertyField { label = currentChildProperty.displayName };
                propertyField.BindProperty(currentChildProperty);
                    
                _targetDetailsContainer.Add(propertyField);
            }
        }
        
        private void ResetSelectedItem()
        {
            _selectedTargetField.Unbind();
            _selectedTargetField.value = null;
        }
        
        private static bool IsGameObjectWithComponentDragged<TComponent>(out TComponent result) where TComponent : Component
        {
            result = null;
            var selectedGameObject = DragAndDrop.objectReferences[0] as GameObject;
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out result);
        }
    }
}
