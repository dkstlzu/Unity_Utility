using UnityEditor;

namespace dkstlzu.Utility
{
    [InitializeOnLoad]
    public static class Physics2DHelperEditor
    {
        public static bool DrawGizmo
        {
            get => Physics2DHelper.DrawGizmo;
            set => Physics2DHelper.DrawGizmo = value;
        }

        private const string _PREF_KEY = "Physics2DHelper.DrawGizmo";
        private const string _DISPLAY_TITLE = "물리 도우미 알림";
        private const string _DISPLAY_TEXT = @"Physics2DHelper 클래스의 Gizmo를 활성화 합니까?
메뉴 툴바 Tools/PhysicsHelper/ 에서 언제든지 키고 끌 수 있습니다.";

        static Physics2DHelperEditor()
        {
            if (!EditorPrefs.HasKey(_PREF_KEY))
            {
                DrawGizmo = EditorUtility.DisplayDialog(_DISPLAY_TITLE, _DISPLAY_TEXT, "켜기", "끄기");
                EditorPrefs.SetBool(_PREF_KEY, DrawGizmo);
            }
            else
            {
                DrawGizmo = EditorPrefs.GetBool(_PREF_KEY);
            }
        }

        [MenuItem("Tools/PhysicsHelper/EnableGizmo")]
        public static void EnableGizmo()
        {
            DrawGizmo = true;
            EditorPrefs.SetBool(_PREF_KEY, DrawGizmo);
        }

        [MenuItem("Tools/PhysicsHelper/DisableGizmo")]
        public static void DisableGizmo()
        {
            DrawGizmo = false;
            EditorPrefs.SetBool(_PREF_KEY, DrawGizmo);
        }

        [MenuItem("Tools/PhysicsHelper/EnableGizmo", true)]
        static bool EnableGizmoValidation()
        {
            return !DrawGizmo;
        }

        [MenuItem("Tools/PhysicsHelper/DisableGizmo", true)]
        static bool DisableGizmoValidation()
        {
            return DrawGizmo;
        }

        [MenuItem("Tools/PhysicsHelper/ResetSettings")]
        public static void ResetSettings()
        {
            DrawGizmo = false;
            EditorPrefs.DeleteKey(_PREF_KEY);
        }


    }
}