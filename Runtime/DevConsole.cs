#if ENABLE_INPUT_SYSTEM
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using TMPro;

using Context = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace dkstlzu.Utility
{
    public class DevConsole : Singleton<DevConsole>
    {
        [Serializable]
        public class CommandAction
        {
            public string Command;
            public UnityEvent<string> Event;

            public CommandAction(string command)
            {
                Command = command;
            }

            public CommandAction(string command, UnityAction<string> action)
            {
                Command = command;
#if UNITY_EDITOR
                UnityEditor.Events.UnityEventTools.AddPersistentListener(Event, action);
#else
                Event.AddListener(action);
#endif
            }
        }

        public bool IgnoreCase;
        public TMP_InputField InputField;
        public InputAction PreviousCommandAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/upArrow");
        public InputAction NextCommandAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/downArrow");
        public List<CommandAction> CommandActions;

        public static Stack<string> PreviousCommandStack = new Stack<string>();
        public static Stack<string> NextCommandStack = new Stack<string>();

        public event Action OnCommandStackUpdate;

        void Awake()
        {
            // PreviousCommandStack = new Stack<string>();
            // NextCommandStack = new Stack<string>();

            InputField.onSubmit.AddListener(OnInputSubmit);
            InputField.onSelect.AddListener((str) => OnSelect());
            InputField.onDeselect.AddListener((str) => OnDeselect());

            PreviousCommandAction.performed += PreviousCommand;
            NextCommandAction.performed += NextCommand;

            foreach(CommandAction commandAction in CommandActions)
            {
                while (commandAction.Event.GetPersistentEventCount() > 0)
                {
                    // print($"{commandAction.Event.GetPersistentTarget(0)}, {commandAction.Event.GetPersistentMethodName(0)}");
                    MethodInfo mi;

                    mi = UnityEventBase.GetValidMethodInfo(commandAction.Event.GetPersistentTarget(0), commandAction.Event.GetPersistentMethodName(0), new Type[]{typeof(string)});
                    if (mi != null)
                    {
                        UnityAction<string> strAction = mi.CreateDelegate(typeof(UnityAction<string>), commandAction.Event.GetPersistentTarget(0)) as UnityAction<string>;
                        if (strAction.GetInvocationList().Length > 0)
                        {
                            commandAction.Event.AddListener(strAction);
                        }
                    }

                    mi = UnityEventBase.GetValidMethodInfo(commandAction.Event.GetPersistentTarget(0), commandAction.Event.GetPersistentMethodName(0), new Type[0]);
                    if (mi != null)
                    {
                        UnityAction action = mi.CreateDelegate(typeof(UnityAction), commandAction.Event.GetPersistentTarget(0)) as UnityAction;
                        if (action.GetInvocationList().Length > 0)
                        {
                            commandAction.Event.AddListener((str) => action?.Invoke());
                        }
                    }
#if UNITY_EDITOR
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(commandAction.Event, 0);
#endif
                }
            }
        }

        void OnSelect()
        {
            PreviousCommandAction.Enable();
            NextCommandAction.Enable();
        }

        void OnDeselect()
        {
            PreviousCommandAction.Disable();
            NextCommandAction.Disable();
        }

        void PreviousCommand(Context context)
        {
            if (PreviousCommandStack.Count <= 0) return;
            NextCommandStack.Push(InputField.text);
            InputField.text = PreviousCommandStack.Pop();
        }

        void NextCommand(Context context)
        {
            if (NextCommandStack.Count <= 0) return;
            PreviousCommandStack.Push(InputField.text);
            InputField.text = NextCommandStack.Pop();
        }

        public static void OnInputSubmit(string command)
        {
            while (NextCommandStack.Count > 0)
            {
                PreviousCommandStack.Push(NextCommandStack.Pop());
            }

            if (PreviousCommandStack.Count <= 0 || PreviousCommandStack.Peek() != command)
            {
                PreviousCommandStack.Push(command);
            }

            GetOrNull.OnCommandStackUpdate?.Invoke();

            GetOrNull.InputField.text = string.Empty;

            ProcessCommand(command);
        }

        public static void ProcessCommand(string command)
        {
            string[] temp = command.Split(' ');
            string[] commandInput = new string[2];
            try
            {
                commandInput[0] = temp[0];
            } catch(Exception)
            {
                commandInput[0] = string.Empty;
            }

            try
            {
                commandInput[1] = temp[1];
            } catch(Exception)
            {
                commandInput[1] = string.Empty;
            }

            Printer.Print($"DevConsole ProcessCommand : {commandInput[0]}, {commandInput[1]}", logLevel : LogLevel.Error);

            UnityEvent<string> ue = GetEvent(commandInput[0]);
            if (ue != null)
            {
                ue.Invoke(commandInput[1]);
            } else
            {
                GetOrNull.InputField.interactable = false;
                GetOrNull.InputField.text = "Wrong command";
                CoroutineHelper.Delay(() => 
                {
                    GetOrNull.InputField.interactable = true;
                    GetOrNull.InputField.text = string.Empty;
                }, 1f);
            }
        }

        public static UnityEvent<string> GetEvent(string command)
        {
            return GetOrNull.CommandActions.Find((ca) => String.Compare(ca.Command, command, GetOrNull.IgnoreCase) == 0)?.Event ?? null;
        }

        [ContextMenu("PrintCommandStackInfo")]
        public void PrintCommandStackInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("PreviousCommandStack");

            foreach (var command in PreviousCommandStack)
            {
                sb.AppendLine(command);
            }

            sb.AppendLine("NextCommandStack");

            foreach (var command in NextCommandStack)
            {
                sb.AppendLine(command);
            }

            print(sb);
        }
    }
}
#endif
