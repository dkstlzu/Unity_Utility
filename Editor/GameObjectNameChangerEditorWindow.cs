using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class GameObjectNameChangerEditorWindow : EditorWindow
    {
        [MenuItem("Dev/게임오브젝트 이름변경창", priority = 501)]
        private static void ShowWindow()
        {
            var window = GetWindow<GameObjectNameChangerEditorWindow>();
            window.titleContent = new GUIContent("게임오브젝트 이름변경");
            window.containChildren = EditorPrefs.GetBool("GameObjectNameChanger_ContainChildren", true);

            window.Show();
        }

        private bool containChildren = true;
        private bool selectionFoldout = false;
        private Vector2 scrollPosition = Vector2.zero;
        private HashSet<GameObject> selectedGos = new HashSet<GameObject>();
        
        private string searchText = "";
        private string replaceText = "";
        
        private void OnGUI()
        {
            searchText = EditorGUILayout.TextField("찾기", searchText);
            replaceText = EditorGUILayout.TextField("변경", replaceText);

            EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("바꾸기"))
            // {
            //     foreach (GameObject selectedGo in selectedGos)
            //     {
            //         selectedGo.name = selectedGo.name.Replace(searchText, replaceText);
            //     }
            // }
            
            if (GUILayout.Button("모두 바꾸기"))
            {
                foreach (GameObject selectedGo in selectedGos)
                {
                    selectedGo.name = selectedGo.name.Replace(searchText, replaceText);
                }
                
                
                foreach (GameObject selectedGo in selectedGos)
                {
                    selectedGo.name = selectedGo.name.Replace(searchText, replaceText);
                    EditorUtility.SetDirty(selectedGo);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            containChildren = EditorGUILayout.Toggle("자식오브젝트 포함", containChildren);
                
            selectedGos.Clear();
            if (containChildren)
            {
                foreach (GameObject selected in Selection.gameObjects)
                {
                    selected.transform.TraverseDescendents(null, tr => !selectedGos.Add(tr.gameObject));
                }
            }
            else
            {
                foreach (GameObject selected in Selection.gameObjects)
                {
                    selectedGos.Add(selected);
                }    
            }
            
            selectionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(selectionFoldout, "선택 목록");

            if (selectionFoldout)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
                GUI.enabled = false;
                var previousColor = GUI.color;

                foreach (GameObject selectedGo in selectedGos)
                {
                    GUI.color = string.IsNullOrEmpty(searchText) ? previousColor : selectedGo.name.Contains(searchText) ? Color.red : previousColor;

                    EditorGUILayout.ObjectField(selectedGo, typeof(GameObject), true, GUILayout.Width(EditorGUIUtility.currentViewWidth - GUI.skin.verticalScrollbar.fixedWidth - 10));
                }
                
                GUI.color = previousColor;
                GUI.enabled = true;
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
