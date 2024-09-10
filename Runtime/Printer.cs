using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEditor;
#endif

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using System.Reflection;
using System.Text.RegularExpressions;
#endif

namespace dkstlzu.Utility
{
    public enum LogLevel
    {
        Default,
        Warning,
        Error,
    }

    public enum LogInterval
    {
        Default,
        Individual,
        Frame,
        // Group,
    }
    
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class Printer
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        class FramePrinter : ILateUpdatable
        {
            private StringBuilder _builder;
            public LogLevel MaximumLogLevel = LogLevel.Default;
            private bool _addedThisFrame = false;
            
            public FramePrinter()
            {
                _builder = new StringBuilder();
            }

            public void Add(object message, string customTag = null, LogLevel level = LogLevel.Default)
            {
                _addedThisFrame = true;

                switch (level)
                {
                    case LogLevel.Warning:
                        _builder.Append("<color=yellow>");
                        break;
                    case LogLevel.Error: 
                        _builder.Append("<color=red>");
                        break;
                }
                _builder.Append(BracketTag(customTag));

                _builder.Append(message);
                
                switch (level)
                {
                    case LogLevel.Warning:
                    case LogLevel.Error: 
                        _builder.Append("</color>");
                        break;
                }
                
                _builder.AppendLine();

                MaximumLogLevel = (LogLevel)Mathf.Max((int)MaximumLogLevel, (int)level);
            }
            
            public void ManualLateUpdate(float delta)
            {
                if (!_addedThisFrame || _builder.Length == 0)
                {
                    return;
                }
                
                try
                {
                    switch (MaximumLogLevel)
                    {
                        default:
                        case LogLevel.Default:
                            Debug.Log(_builder);
                            break;
                        case LogLevel.Warning:
                            Debug.LogWarning(_builder);
                            break;
                        case LogLevel.Error:
                            Debug.LogError(_builder);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    _addedThisFrame = false;
                    _builder.Clear();
                    _builder.AppendLine($"FramePrint {Time.frameCount}");
                    MaximumLogLevel = LogLevel.Default;
                }
            }
        }

        private static FramePrinter _framePrinter;
#endif

        public static string DefaultTag = Application.productName;
        private static LogInterval _defaultLogInterval = LogInterval.Individual;

        public static LogInterval DefaultLogInterval
        {
            get => _defaultLogInterval;
            set
            {
                if (value == default)
                {
                    return;
                }

                _defaultLogInterval = value;
#if UNITY_EDITOR
                EditorPrefs.SetInt("PrinterLogInterval", (int)_defaultLogInterval);
#endif
            }
        }

        private static int _currentPriority;
        public static int CurrentPriority
        {
            get => _currentPriority;
            set
            {
                _currentPriority = value;
#if UNITY_EDITOR
                EditorPrefs.SetInt("PrinterPriority", _currentPriority);
#endif
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static StringBuilder S_stringBuilder;

        static Printer()
        {
            S_stringBuilder = new StringBuilder();
#if UNITY_EDITOR
            _currentPriority = EditorPrefs.GetInt("PrinterPriority", 0);
            _defaultLogInterval = (LogInterval)EditorPrefs.GetInt("PrinterLogInterval", 0);
#endif
        }
#endif

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default, int priority = 0, LogInterval logInterval = LogInterval.Default)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (priority < CurrentPriority) return;

            LogInterval per = logInterval;
            if (logInterval == LogInterval.Default)
            {
                per = DefaultLogInterval;                
            }
            
            if (per == LogInterval.Individual)
            {
                print(message, customTag, logLevel);
            }
            else if (per == LogInterval.Frame)
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.LogError($"Printer.Print() with ConsolePer.Frame only supports Play Mode.");
                    print(message, customTag, logLevel);
                    return;
                }
                
                if (_framePrinter == null)
                {
                    _framePrinter = new FramePrinter();
                    UpdateManager.GetOrCreate().AddUpdatable(_framePrinter);
                }
                
                _framePrinter.Add(message, customTag, logLevel);
            }
#endif
        }

        private static void print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default)
        {
            S_stringBuilder.Clear();
            S_stringBuilder.Append(BracketTag(customTag));

            S_stringBuilder.Append(message);
            S_stringBuilder.AppendLine();
            
            try
            {
                switch (logLevel)
                {
                    default:
                    case LogLevel.Default: 
                        Debug.Log(S_stringBuilder);
                        break;
                    case LogLevel.Warning: 
                        Debug.LogWarning(S_stringBuilder);
                        break;
                    case LogLevel.Error: 
                        Debug.LogError(S_stringBuilder);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string BracketTag(string customTag = null)
        {
            string tag = customTag ?? DefaultTag;
            return $"[{tag}] : ";
        }

#if UNITY_EDITOR
        [OnOpenAsset(1)]
        private static bool OnOpenDebugLog(int instance, int line)
        {
            string name = EditorUtility.InstanceIDToObject(instance).name;
            if (!name.Equals(nameof(Printer))) return false;

            // 에디터 콘솔 윈도우의 인스턴스를 찾는다.
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));
            if(assembly == null) return false;

            var consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType == null) return false;

            var consoleWindowField = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (consoleWindowField == null) return false;

            var consoleWindowInstance = consoleWindowField.GetValue(null);
            if (consoleWindowInstance == null) return false;

            if (consoleWindowInstance != (object)EditorWindow.focusedWindow) return false;

            // 콘솔 윈도우 인스턴스의 활성화된 텍스트를 찾는다.
            var activeTextField = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeTextField == null) return false;

            string activeTextValue = activeTextField.GetValue(consoleWindowInstance).ToString();
            if (string.IsNullOrEmpty(activeTextValue)) return false;

            // 디버그 로그를 호출한 파일 경로를 찾아 편집기로 연다.
            Match match = Regex.Match(activeTextValue, @"\(at (.+)\)");
            if (match.Success)
            {
                match = match.NextMatch(); // stack trace의 첫번째를 건너뛴다.
            }

            if (match.Success)
            {
                string path = match.Groups[1].Value;
                var split = path.Split(':');
                string filePath = split[0];
                int lineNum = Convert.ToInt32(split[1]);

                string dataPath = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("Assets"));
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(dataPath + filePath, lineNum);
                return true;
            }
            return false;
        }
#endif
    }
}