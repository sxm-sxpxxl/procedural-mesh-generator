using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
            ListView listView,
            ObjectField selectedItem,
            VisualElement itemDetailsContainer
        )
        {
            _targetObject = targetObject;
            _listView = listView;
            _selectedItem = selectedItem;
            _itemDetailsContainer = itemDetailsContainer;
            
            _selectedItem.objectType = typeof(TItemType);
            _selectedItem.SetEnabled(false);

            _listView.selectionType = SelectionType.Single;
            _listView.onSelectionChange += OnSelectionItemChanged;
            _listView.itemIndexChanged += OnItemReordered;
            _listView.itemsRemoved += OnItemRemoved;
            _listView.RegisterCallback<KeyDownEvent>(OnRemoveKeyDown);
        }

        private void OnSelectionItemChanged(IEnumerable<object> selectionItems)
        {
            var items = selectionItems.ToArray();
            if (items.Length != 1)
            {
                return;
            }
                
            var itemProperty = items[0] as SerializedProperty;
                
            _selectedItem.BindProperty(itemProperty);
            _selectedItem.RegisterCallback<ChangeEvent<Object>>(evt =>
            {
                UpdateItemDetails(evt.newValue!);
            });
                
            UpdateItemDetails(itemProperty!.objectReferenceValue);
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

        private void OnItemReordered(int oldIndex, int newIndex)
        {
            _listView.selectedIndex = newIndex;
            _listView.Rebuild();
        }


        private void OnItemRemoved(IEnumerable<int> itemIndices)
        {
            if (ListProperty.arraySize != 0)
            {
                return;
            }
                
            _selectedItem.Unbind();
            _selectedItem.value = null;
        }

        private void OnRemoveKeyDown(KeyDownEvent evt)
        {
            int selectedIndex = _listView.selectedIndex;
            SerializedProperty listProperty = ListProperty;
                
            bool isRemoveKeyDown = evt.keyCode == RemoveKey;
            bool isSelectedIndexCorrect = selectedIndex >= 0 && selectedIndex < listProperty.arraySize;
                
            if (isRemoveKeyDown == false || isSelectedIndexCorrect == false)
            {
                return;
            }
                
            listProperty.DeleteArrayElementAtIndex(_listView.selectedIndex);
                    
            if (listProperty.arraySize == 0)
            {
                _selectedItem.Unbind();
                _selectedItem.value = null;
            }
                    
            if (_listView.selectedIndex == listProperty.arraySize)
            {
                _listView.selectedIndex -= 1;
            }

            _targetObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
    }
}
