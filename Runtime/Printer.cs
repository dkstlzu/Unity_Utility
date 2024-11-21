#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_5_3_OR_NEWER
#define UNITY
#endif

#if DEBUG
#define LOG_ON
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

#if UNITY
using UnityEngine;
using Debug = UnityEngine.Debug;
#endif

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

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
[Serializable]
public class Printer
{
#if LOG_ON

#if UNITY
    #region MonoBehaviour

    public class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        public static MonoBehaviour instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
        
            instance = this;
        }

        private void LateUpdate()
        {
            framePrinterInstance?.ManualLateUpdate();
        }
    }
    
    #endregion
    
    class FramePrinter
    {
        private StringBuilder stringBuilder;
        public LogLevel maximumLogLevel = LogLevel.Default;
        private bool wasAddedOnThisFrame = false;
        
        public FramePrinter()
        {
            stringBuilder = new StringBuilder();
            new GameObject("Printer", typeof(MonoBehaviour));
        }

        public void Add(object message, string customTag = null, LogLevel level = LogLevel.Default, string callerMethod = "")
        {
            MonoBehaviour.instance.enabled = true;
            wasAddedOnThisFrame = true;

            switch (level)
            {
                case LogLevel.Warning:
                    stringBuilder.Append("<color=yellow>");
                    break;
                case LogLevel.Error: 
                    stringBuilder.Append("<color=red>");
                    break;
            }
            stringBuilder.Append(BracketTag(customTag));

            if (!string.IsNullOrEmpty(callerMethod))
            {
                stringBuilder.Append($"{callerMethod} -> ");
            }
            stringBuilder.Append(message);
            
            switch (level)
            {
                case LogLevel.Warning:
                case LogLevel.Error: 
                    stringBuilder.Append("</color>");
                    break;
            }
            
            stringBuilder.AppendLine();

            maximumLogLevel = (LogLevel)Mathf.Max((int)maximumLogLevel, (int)level);
        }
        
        public void ManualLateUpdate()
        {
            if (!wasAddedOnThisFrame || stringBuilder.Length == 0)
            {
                return;
            }
            
            stringBuilder.Insert(0, $"FramePrint {Time.frameCount}\n");

            switch (maximumLogLevel)
            {
                default:
                case LogLevel.Default:
                    Debug.Log(stringBuilder);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(stringBuilder);
                    break;
                case LogLevel.Error:
                    Debug.LogError(stringBuilder);
                    break;
            }
    
            wasAddedOnThisFrame = false;
            stringBuilder.Clear();
            maximumLogLevel = LogLevel.Default;
            MonoBehaviour.instance.enabled = false;
        }
    }

    private static FramePrinter framePrinterInstance;
#endif

#endif

#if UNITY
    public static string DefaultTag => $"[{Application.productName}] : ";
#else
    public static string DefaultTag => $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} : ";
#endif

#if UNITY
    private static LogInterval defaultLogInterval = (LogInterval)int.MinValue;
    public static LogInterval DefaultLogInterval
    {
        get
        {
#if UNITY_EDITOR
            if ((int)defaultLogInterval == int.MinValue)
            {
                defaultLogInterval = (LogInterval)EditorPrefs.GetInt("PrinterLogInterval", 1);
            }  
#endif

            return defaultLogInterval;
        }
        set
        {
            if (value == default)
            {
                return;
            }

            defaultLogInterval = value;
#if UNITY_EDITOR
            EditorPrefs.SetInt("PrinterLogInterval", (int)defaultLogInterval);
#endif
        }
    }
#endif

    private static int _currentPriority
#if UNITY_EDITOR
        = int.MinValue;
#else
        = 0;
#endif
    
    public static int CurrentPriority
    {
        get
        {
#if UNITY_EDITOR
            if (_currentPriority == int.MinValue)
            {
                _currentPriority = EditorPrefs.GetInt("PrinterPriority", 0);
            }
#endif
            return _currentPriority;
        }
        set
        {
            _currentPriority = value;
#if UNITY_EDITOR
            EditorPrefs.SetInt("PrinterPriority", _currentPriority);
#endif
        }
    }

#if LOG_ON
    private static StringBuilder _stringBuilder;

    static Printer()
    {
        _stringBuilder = new StringBuilder();
#if UNITY
        defaultLogInterval = LogInterval.Individual;
#endif
    }
#endif

    [Conditional("DEBUG"), Conditional("LOG_ON")]
#if UNITY
    public static void Print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default, int priority = 0, LogInterval logInterval = LogInterval.Default, Color color = default, UnityEngine.Object context = null, [CallerMemberName]string callerMethod = "")
#else
    public static void Print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default, int priority = 0)
#endif
    {
#if LOG_ON
        if (priority < CurrentPriority) return;

#if UNITY
        LogInterval per = logInterval;
        if (logInterval == LogInterval.Default)
        {
            per = DefaultLogInterval;                
        }
        
        if (per == LogInterval.Individual)
        {
            print(message, customTag, logLevel, color, context);
        }
        else if (per == LogInterval.Frame)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                print(message, customTag, logLevel, color, context);
                return;
            }
#endif
            
            if (framePrinterInstance == null)
            {
                framePrinterInstance = new FramePrinter();
            }

            if (MonoBehaviour.instance == null)
            {
                new GameObject("Printer", typeof(MonoBehaviour));
            }
            
            framePrinterInstance.Add(message, customTag, logLevel, callerMethod);
        }
#else
        print(message, customTag, logLevel);
#endif

#endif
    }

    [Conditional("LOG_ON")]
#if UNITY
    private static void print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default, Color color = default, UnityEngine.Object context = null)
#else
    private static void print(object message, string customTag = null, LogLevel logLevel = LogLevel.Default)
#endif
    {
#if LOG_ON
        _stringBuilder.Clear();
#if UNITY
        _stringBuilder.Append(BracketTag(customTag));
#else
        _stringBuilder.Append(BracketTag(customTag, logLevel));
#endif

#if UNITY
        if (color != default)
        {
            _stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
            _stringBuilder.Append(message);
            _stringBuilder.Append($"</color>");
        } else
#endif
        _stringBuilder.Append(message);
        _stringBuilder.AppendLine();

#if UNITY
        switch (logLevel)
        {
            default:
            case LogLevel.Default: 
                Debug.Log(_stringBuilder, context);
                break;
            case LogLevel.Warning: 
                Debug.LogWarning(_stringBuilder, context);
                break;
            case LogLevel.Error: 
                Debug.LogError(_stringBuilder, context);
                break;
        }
#else
        Console.WriteLine(_stringBuilder.ToString());
#endif

#endif
    }

#if UNITY
    public static string BracketTag(string customTag = null)
    {
        if (string.IsNullOrEmpty(customTag))
        {
            return DefaultTag;
        }
        
        return $"[{customTag}] : ";
    }
#else
    public static string BracketTag(string customTag = null, LogLevel logLevel = LogLevel.Default)
    {
        if (string.IsNullOrEmpty(customTag))
        {
            switch (logLevel)
            {
                case LogLevel.Default:
                    return "I: ";
                case LogLevel.Warning:
                    return "W: ";
                case LogLevel.Error:
                    return "E: ";
            }
        }
        
        return DefaultTag;
    }
#endif

    public string Tag = DefaultTag;
    
    public LogLevel LogLevel = LogLevel.Default;
    public int Priority = 0;

#if UNITY
    public LogInterval LogInterval = LogInterval.Default;
    public Color Color;
#endif
    
    public Printer()
    {
    }

    [Conditional("LOG_ON")]
    public void Print(object message, UnityEngine.Object context = null, [CallerMemberName] string callerMethod = "")
    {
#if UNITY
        Print(message, Tag, LogLevel, Priority, LogInterval, Color, context, callerMethod);
#else
        Print(message, Tag, LogLevel, Priority);
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.Callbacks.OnOpenAsset(1)]
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
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(activeTextValue, @"\(at (.+)\)");
        int finiteLoop = 30;
        
        while (match.Success)
        {
            if (!match.Value.Contains("Printer"))
            {
                break;
            }
            
            match = match.NextMatch();
            finiteLoop--;

            if (finiteLoop < 0)
            {
                break;
            }
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
