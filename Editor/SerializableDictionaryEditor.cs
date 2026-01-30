using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace dkstlzu.Utility
{
    public class SerializableStringDictionaryEditor : PropertyDrawer
    {
        public string searchString;
        private MultiColumnListView listView;
        private SerializedProperty keySP;
        private SerializedProperty valueSP;
        private VisualElement root;
        private HelpBox helpBox;

        // 중복 키 캐시
        private HashSet<int> _cachedDuplicateIndices = new HashSet<int>();
        private IVisualElementScheduledItem _scheduledUpdate;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            root = new VisualElement();

            // 1. 검색창
            VisualElement searchVe = new VisualElement();
            searchVe.style.flexDirection = FlexDirection.Row;
            searchVe.style.marginBottom = 5;
            
            Label searchLabel = new Label("검색");
            searchLabel.style.alignSelf = Align.Center;
            searchVe.Add(searchLabel);

            TextField searchField = new TextField();
            searchField.RegisterValueChangedCallback(OnSearch);
            searchField.style.flexGrow = 1;
            searchField.style.marginLeft = 5;
            searchString = EditorPrefs.GetString(nameof(SerializableStringDictionaryEditor), String.Empty);
            searchField.value = searchString; 
            searchVe.Add(searchField);
            root.Add(searchVe);

            keySP = property.FindPropertyRelative("keys");
            valueSP = property.FindPropertyRelative("values");
            
            // 2. 컬럼 설정 (삭제 버튼용 컬럼 추가)
            Columns columns = new Columns();
            columns.stretchMode = Columns.StretchMode.GrowAndFill;
            
            Column keyColumn = new Column { name = "key", title = "키", width = 120, minWidth = 80, stretchable = true };
            Column valueColumn = new Column { name = "value", title = "값", width = 120, minWidth = 80, stretchable = true };
            Column delColumn = new Column { name = "del", title = "", width = 25, minWidth = 25, maxWidth = 25, stretchable = false }; // 삭제 버튼용

            // [키 바인딩]
            keyColumn.makeCell = () => new TextField();
            keyColumn.bindCell = (ve, i) =>
            {
                var textField = (TextField)ve;
                textField.Unbind();
                
                var indices = listView.itemsSource as IList<int>;
                if (indices == null || i >= indices.Count) return;
                int realIndex = indices[i];

                if (realIndex < keySP.arraySize)
                {
                    var sp = keySP.GetArrayElementAtIndex(realIndex);
                    if (sp != null)
                    {
                        textField.BindProperty(sp);
                        // 중복 시 빨간색 표시
                        var textInput = textField.Q(TextField.textInputUssName);
                        if (textInput != null)
                        {
                            textInput.style.color = _cachedDuplicateIndices.Contains(realIndex) 
                                ? new StyleColor(Color.red) : new StyleColor(StyleKeyword.Null);
                        }
                    }
                }
            };
            
            // [값 바인딩]
            valueColumn.makeCell = () => new PropertyField();
            valueColumn.bindCell = (ve, i) =>
            {
                var propField = (PropertyField)ve;
                propField.Unbind();

                var indices = listView.itemsSource as IList<int>;
                if (indices == null || i >= indices.Count) return;
                int realIndex = indices[i];

                if (realIndex < valueSP.arraySize)
                {
                    var sp = valueSP.GetArrayElementAtIndex(realIndex);
                    if (sp != null) propField.BindProperty(sp);
                }
            };

            // [삭제 버튼 바인딩] - 여기가 핵심입니다
            delColumn.makeCell = () => 
            {
                var btn = new Button();
                btn.text = "X";
                btn.style.color = Color.red;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                // 버튼 클릭 이벤트 등록 (한번만)
                btn.clicked += () => 
                {
                    // userData에 저장된 인덱스를 가져와서 삭제
                    if (btn.userData is int indexToDelete)
                    {
                        RemoveItem(indexToDelete);
                    }
                };
                return btn;
            };
            delColumn.bindCell = (ve, i) =>
            {
                var btn = (Button)ve;
                var indices = listView.itemsSource as IList<int>;
                if (indices != null && i < indices.Count)
                {
                    // 현재 버튼이 담당하는 실제 데이터 인덱스를 userData에 저장
                    btn.userData = indices[i];
                }
            };
            
            columns.Add(keyColumn);
            columns.Add(valueColumn);
            columns.Add(delColumn); // 컬럼 추가

            // 3. 리스트뷰 설정
            listView = new MultiColumnListView(columns);
            listView.showBorder = true;
            listView.reorderable = true; // 순서 변경 가능
            listView.selectionType = SelectionType.None; // 선택 에러 방지용 (필요하면 Single로)
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.style.maxHeight = 300;
            
            // *** 중요: 기본 Footer 끄기 ***
            listView.showAddRemoveFooter = false; 

            // 순서 변경 이벤트
            listView.itemIndexChanged += (src, dst) =>
            {
                property.serializedObject.Update();
                keySP.MoveArrayElement(src, dst);
                valueSP.MoveArrayElement(src, dst);
                property.serializedObject.ApplyModifiedProperties();
                UpdateDataAndUI(forceRebuild: true);
            };

            // 4. 경고 박스
            helpBox = new HelpBox("", HelpBoxMessageType.Error);
            helpBox.name = "duplicateKeyNoti";

            root.Add(listView);

            // 5. 커스텀 추가 버튼 (리스트 아래)
            Button addButton = new Button(() => AddItem());
            addButton.text = "+ 항목 추가";
            addButton.style.height = 25;
            addButton.style.marginTop = 2;
            root.Add(addButton);

            // Undo.undoRedoPerformed += ScheduleUpdate;
            
            // // 외부 변경 감지 (Undo/Redo)
            // root.TrackPropertyValue(keySP, (p) => 
            // {
            //     // ScheduleUpdate();
            // });

            // 초기화
            UpdateDataAndUI(forceRebuild: true);

            string scrollTargetKey = EditorPrefs.GetString($"{nameof(SerializableStringDictionaryEditor)}Scroll", string.Empty);
            for (int i = 0; i < keySP.arraySize; i++)
            {
                if (keySP.GetArrayElementAtIndex(i).stringValue == scrollTargetKey)
                {
                    listView.ScrollToItem(i);
                }
            }
            EditorPrefs.DeleteKey($"{nameof(SerializableStringDictionaryEditor)}Scroll");
            
            return root;
        }

        // --- 기능 구현부 ---

        private void AddItem()
        {
            keySP.serializedObject.Update();
            int index = keySP.arraySize;
            keySP.InsertArrayElementAtIndex(index);
            valueSP.InsertArrayElementAtIndex(index);
            
            var element = keySP.GetArrayElementAtIndex(index);
            element.stringValue = "Key " + index; // 기본값
            
            keySP.serializedObject.ApplyModifiedProperties();
            UpdateDataAndUI(forceRebuild: true);
            
            // 스크롤을 맨 아래로
            listView.ScrollToItem(listView.itemsSource.Count - 1);
        }

        private void RemoveItem(int index)
        {
            keySP.serializedObject.Update();
            
            // 인덱스 유효성 검사
            if (index >= 0 && index < keySP.arraySize)
            {
                keySP.DeleteArrayElementAtIndex(index);
                valueSP.DeleteArrayElementAtIndex(index);
            }

            keySP.serializedObject.ApplyModifiedProperties();
            
            // 바로 갱신에 버그가 있는데 어떻게 고쳐야 할지 몰라서 Selection을 바꿨다가 오는 임시방편으로 해뒀습니다.
            // UpdateDataAndUI(forceRebuild: true);

            if (index - 1 >= 0)
            {
                EditorPrefs.SetString($"{nameof(SerializableStringDictionaryEditor)}Scroll", keySP.GetArrayElementAtIndex(index - 1).stringValue);
            }

            Object selected = Selection.activeObject;
            Selection.activeObject = null;
            EditorApplication.delayCall += () =>
            {
                Selection.activeObject = selected;
            };
        }
        
        private void OnSearch(ChangeEvent<string> evt)
        {
            searchString = evt.newValue;

            EditorPrefs.SetString(nameof(SerializableStringDictionaryEditor), searchString);
            
            UpdateDataAndUI(forceRebuild: true);
        }

        private void ScheduleUpdate()
        {
            if (_scheduledUpdate == null)
            {
                _scheduledUpdate = root.schedule.Execute(() => 
                {
                    UpdateDataAndUI(forceRebuild: false);
                }).StartingIn(200);
            }
        }

        private void UpdateDataAndUI(bool forceRebuild)
        {
            if (_scheduledUpdate != null)
            {
                _scheduledUpdate.Pause();
                _scheduledUpdate = null;
            }
            
            // 0. 가장 중요: 데이터 동기화
            keySP.serializedObject.Update();

            // 1. ItemsSource 계산
            var allIndices = Enumerable.Range(0, keySP.arraySize);
            List<int> filteredIndices;

            if (string.IsNullOrEmpty(searchString))
            {
                filteredIndices = allIndices.ToList();
            }
            else
            {
                filteredIndices = allIndices
                    .Where(i => 
                    {
                        // 범위 안전 체크
                        if (i >= keySP.arraySize) return false;
                        return keySP.GetArrayElementAtIndex(i).stringValue.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToList();
            }

            listView.itemsSource = filteredIndices;

            // 2. 중복 검사
            _cachedDuplicateIndices.Clear();
            Dictionary<string, List<int>> keyToIndicesMap = new Dictionary<string, List<int>>();

            for (int i = 0; i < keySP.arraySize; i++)
            {
                string key = keySP.GetArrayElementAtIndex(i).stringValue;
                if (!keyToIndicesMap.ContainsKey(key)) keyToIndicesMap[key] = new List<int>();
                keyToIndicesMap[key].Add(i);
            }

            HashSet<string> duplicateKeyStrings = new HashSet<string>();
            foreach (var kvp in keyToIndicesMap)
            {
                if (kvp.Value.Count > 1)
                {
                    duplicateKeyStrings.Add(kvp.Key);
                    foreach (int index in kvp.Value) _cachedDuplicateIndices.Add(index);
                }
            }

            // 3. UI 갱신
            if (duplicateKeyStrings.Count > 0)
            {
                if (root.Q<HelpBox>("duplicateKeyNoti") == null)
                    root.Insert(root.IndexOf(listView), helpBox);
                
                StringBuilder builder = new StringBuilder();
                builder.Append("중복키: ");
                int count = 0;
                foreach (string k in duplicateKeyStrings)
                {
                    if (count++ > 3) { builder.Append("..."); break; }
                    builder.Append($"[{k}] ");
                }
                helpBox.text = builder.ToString();
            }
            else
            {
                if (root.Q<HelpBox>("duplicateKeyNoti") != null) root.Remove(helpBox);
            }

            // 4. 리스트 뷰 갱신
            if (forceRebuild) listView.Rebuild();
            else listView.RefreshItems();
        }
    }
}