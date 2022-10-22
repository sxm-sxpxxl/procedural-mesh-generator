using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Sxm.ProceduralMeshGenerator.Modification;

namespace Sxm.ProceduralMeshGenerator
{
    public sealed class DetailedListViewController<TItemType>
    {
        private static readonly string ScriptPropertyName = "m_Script";
        private static readonly KeyCode RemoveKey = KeyCode.Delete;
        
        private SerializedObject _targetObject;
        private ListView _listView;
        private ObjectField _selectedItem;
        private VisualElement _itemDetailsContainer;
        
        private SerializedProperty ListProperty => _targetObject.FindProperty(_listView.bindingPath);
        
        public DetailedListViewController(
            SerializedObject targetObject,
            VisualElement dragDropContainer,
            Button clearButton,
            Button cleanButton,
            ListView listView,
            ObjectField selectedItem,
            VisualElement itemDetailsContainer
        )
        {
            _targetObject = targetObject;
            _listView = listView;
            _selectedItem = selectedItem;
            _itemDetailsContainer = itemDetailsContainer;
            
            dragDropContainer.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            dragDropContainer.RegisterCallback<DragPerformEvent>(OnDragPerformed);
            
            clearButton.RegisterCallback<ClickEvent>(OnClearButtonClicked);
            cleanButton.RegisterCallback<ClickEvent>(OnCleanButtonClicked);
            
            _listView.selectionType = SelectionType.Single;
            _listView.onSelectionChange += OnSelectionItemChanged;
            _listView.itemIndexChanged += OnItemReordered;
            _listView.itemsRemoved += OnItemRemoved;
            _listView.RegisterCallback<KeyDownEvent>(OnRemoveKeyDown);
            
            _selectedItem.objectType = typeof(TItemType);
            _selectedItem.SetEnabled(false);
        }
        
        private void OnClearButtonClicked(ClickEvent evt)
        {
            ListProperty.ClearArray();
            
            _targetObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
        
        private void OnCleanButtonClicked(ClickEvent evt)
        {
            for (int i = 0; i < ListProperty.arraySize;)
            {
                if (ListProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    RemoveItemByIndex(i);
                }
                else
                {
                    i++;
                }
            }
        }
        
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (IsGameObjectWithComponentDragged(out BaseMeshModifier _) == false)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }
        
        private void OnDragPerformed(DragPerformEvent evt)
        {
            if (IsGameObjectWithComponentDragged(out BaseMeshModifier modifier) == false)
            {
                return;
            }

            AddItem(modifier);
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
            RebindSelectedItemWith(selectionItemProperty);
        }
        
        private void OnItemReordered(int oldIndex, int newIndex)
        {
            _listView.selectedIndex = newIndex;
            _listView.Rebuild();
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
            
            RemoveItemByIndex(_listView.selectedIndex);

            if (ListProperty.arraySize == 0)
            {
                ResetSelectedItem();
            }
            
            if (_listView.selectedIndex == ListProperty.arraySize)
            {
                SetLastSelectionItem();
            }
        }

        private void AddItem(Object item)
        {
            var listProperty = ListProperty;
            
            listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
            int lastIndex = listProperty.arraySize - 1;
            
            var addedProperty = listProperty.GetArrayElementAtIndex(lastIndex);
            addedProperty.objectReferenceValue = item;

            _targetObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
        
        private void RemoveItemByIndex(int index)
        {
            ListProperty.DeleteArrayElementAtIndex(index);
            
            _targetObject.ApplyModifiedProperties();
            _listView.RefreshItems();
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
            RebindSelectedItemWith(selectionItemProperty);
        }
        
        private void RebindSelectedItemWith(SerializedProperty property)
        {
            _selectedItem.Unbind();
            _selectedItem.BindProperty(property);
            
            _selectedItem.RegisterCallback<ChangeEvent<Object>>(evt =>
            {
                UpdateItemDetails(evt.newValue);
            });

            UpdateItemDetails(property.objectReferenceValue);
        }
        
        private void UpdateItemDetails(Object item)
        {
            _itemDetailsContainer.Clear();
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
                    
                _itemDetailsContainer.Add(propertyField);
            }
        }
        
        private void ResetSelectedItem()
        {
            _selectedItem.Unbind();
            _selectedItem.value = null;
        }
        
        private static bool IsGameObjectWithComponentDragged<TComponent>(out TComponent result) where TComponent : Component
        {
            result = null;
            var selectedGameObject = DragAndDrop.objectReferences[0] as GameObject;
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out result);
        }
    }
}
