using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    public class CommandStackUnityObject : ScriptableObject
    {
        public List<string> PreviousStackList;
        public List<string> NextStackList;
    }
    
    [CustomEditor(typeof(DevConsole))]
    public class DevConsoleEditor : Editor
    {
        SerializedObject stackSO;
        SerializedProperty PreviousStack;
        SerializedProperty NextStack;
        DevConsole targetDevConsole;
        
        void OnEnable()
        {
            if (!Application.isPlaying) return;
            targetDevConsole = target as DevConsole;

            targetDevConsole.OnCommandStackUpdate += OnCommandStackUpdate;
            OnCommandStackUpdate();
        }

        void OnDisable()
        {
            if (!Application.isPlaying) return;
            targetDevConsole.OnCommandStackUpdate -= OnCommandStackUpdate;
        }

        void OnCommandStackUpdate()
        {
            if (!Application.isPlaying) return;
            CommandStackUnityObject CSUO = ScriptableObject.CreateInstance<CommandStackUnityObject>();
            CSUO.PreviousStackList = new List<string>(DevConsole.PreviousCommandStack);
            CSUO.NextStackList = new List<string>(DevConsole.NextCommandStack);
            stackSO = new SerializedObject(CSUO);

            PreviousStack = stackSO.FindProperty("PreviousStackList");
            NextStack = stackSO.FindProperty("NextStackList");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying) return;
            
            serializedObject.Update();

            EditorGUILayout.PropertyField(PreviousStack);
            EditorGUILayout.PropertyField(NextStack);

            serializedObject.ApplyModifiedProperties();
        }
    }
}