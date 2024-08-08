using UnityEditor;
using UnityEngine;

namespace dkstlzu.Utility
{
    [InitializeOnLoad]
    public static class PhysicsHelperEditor
    {
        public static bool DrawGizmo
        {
            get => PhysicsHelper.DrawGizmo;
            set => PhysicsHelper.DrawGizmo = value;
        }

        private const string _PREF_KEY = "PhysicsHelper.DrawGizmo";
        private const string _DISPLAY_TITLE = "물리 도우미 알림";
        private const string _DISPLAY_TEXT = @"PhysicsHelper 클래스의 Gizmo를 활성화 합니까?
메뉴 툴바 Tools/PhysicsHelper/ 에서 언제든지 키고 끌 수 있습니다.";

        static PhysicsHelperEditor()
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

            if (DrawGizmo)
            {
                SceneView.duringSceneGui += SphereHandleSceneGUI;
            }
        }

        private static PhysicsHelper.SphereDrawData[] _sphereHandleInfoList = new PhysicsHelper.SphereDrawData[0];

        private static void SphereHandleSceneGUI(SceneView view)
        {
            Color originalColor = Handles.color;

            foreach (var infoList in _sphereHandleInfoList)
            {
                Handles.color = infoList.Color;
                var positionDiff = infoList.Origin - SceneView.lastActiveSceneView.camera.transform.position;

                Vector3 normalOfPositionDiffUp = Vector3.Cross(positionDiff.normalized, Vector3.up);

                var rotationUp = Quaternion.AngleAxis(90, normalOfPositionDiffUp) * positionDiff.normalized;
                
                Quaternion initialRotation = Quaternion.AngleAxis(45, rotationUp);
                Quaternion q = Quaternion.AngleAxis(180f / PhysicsHelper.SphereCircleSegmentNumber, rotationUp);

                var discNormal = positionDiff.normalized;
                discNormal = initialRotation * discNormal;
                
                for (int i = 0; i < PhysicsHelper.SphereCircleSegmentNumber; i++)
                {
                    discNormal = q * discNormal;
                    Handles.DrawWireDisc(infoList.Origin, discNormal, infoList.Radius, 0.5f);
                }
            }
            
            _sphereHandleInfoList = PhysicsHelper.SphereHandleInfoList.ToArray();
            PhysicsHelper.SphereHandleInfoList.Clear();
            
            Handles.color = originalColor;
        }

        [MenuItem("Tools/PhysicsHelper/EnableGizmo")]
        public static void EnableGizmo()
        {
            DrawGizmo = true;
            SceneView.duringSceneGui += SphereHandleSceneGUI;
            EditorPrefs.SetBool(_PREF_KEY, DrawGizmo);
        }

        [MenuItem("Tools/PhysicsHelper/DisableGizmo")]
        public static void DisableGizmo()
        {
            DrawGizmo = false;
            SceneView.duringSceneGui -= SphereHandleSceneGUI;
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