using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;
#endif

namespace dkstlzu.Utility
{
    /// <summary>
    /// Physics 의 종류별 cast에 대해서 gizmo기능을 제공합니다.
    /// 내부적으로 NonAlloc 종류의 cast를 최대한 사용하며(<see cref="RadialCast"/>>는 제외)
    /// hit일 시에 빨간색, un-hit일 시에 초록색으로 표시합니다.
    /// </summary>
    public static class PhysicsHelper
    {
#if UNITY_EDITOR
        public static bool DrawGizmo = false;

        public static Color HitColor = Color.red;
        public static Color UnHitColor = Color.green;
        public static Color TrajectoryColor = Color.grey;

        public static int SphereCircleSegmentNumber = 5;
        public static float FiniteDistanceWhenInfinity = 10000;

        public static float GetDrawableDistance(float distance)
        {
            return float.IsPositiveInfinity(distance) ? FiniteDistanceWhenInfinity : distance;
        }
#endif

        #region RayCast

        public static ReadOnlySpan<RaycastHit> RayCast(Vector3 origin, Vector3 direction, RaycastHit[] results, float distance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers)
        {
            distance = direction == default ? 0 : distance;
            direction = direction == default ? Vector3.forward : direction.normalized;
            int foundNum = Physics.RaycastNonAlloc(origin, direction, results, distance, layerMask);

#if UNITY_EDITOR
            if (DrawGizmo) DrawRayCastGizmo(origin, direction, foundNum > 0, distance, foundNum > 0 ? results[0] : default);
#endif

            var result = new ReadOnlySpan<RaycastHit>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        static void DrawRayCastGizmo(Vector3 origin, Vector3 direction, bool found, float distance = Mathf.Infinity, RaycastHit hit = default)
        {
            Debug.DrawLine(origin, found ? hit.point : origin + direction * GetDrawableDistance(distance), found ? HitColor : UnHitColor);
        }
#endif

        #endregion
        
        #region BoxCast

        public static ReadOnlySpan<RaycastHit> BoxCast(Vector3 origin, Vector3 size, Vector3 direction, RaycastHit[] results, 
            Quaternion orientation = default, float distance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            distance = direction == default ? 0 : distance;
            direction = direction == default ? Vector3.forward : direction.normalized;
            int foundNum = Physics.BoxCastNonAlloc(origin, size, direction, results, orientation, distance, layerMask);

#if UNITY_EDITOR
            if (DrawGizmo) DrawBoxCastGizmo(origin, size, direction, orientation, foundNum > 0, distance);
#endif

            var result = new ReadOnlySpan<RaycastHit>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        static void DrawBoxCastGizmo(Vector3 origin, Vector3 size, Vector3 direction, Quaternion orienatation, bool found, float distance = Mathf.Infinity)
        {
            if (size.magnitude == 0)
            {
                return;    
            }
            
            //Setting up the start points to draw the cast
            Vector3 s1, s2, s3, s4, s5, s6, s7, s8;
            float x = size.x;
            float y = size.y;
            float z = size.z;
            
            s1 = new Vector3(x, y, z);
            s2 = new Vector3(x, y, -z);
            s3 = new Vector3(x, -y, z);
            s4 = new Vector3(x, -y, -z);
            s5 = new Vector3(-x, y, z);
            s6 = new Vector3(-x, y, -z);
            s7 = new Vector3(-x, -y, z);
            s8 = new Vector3(-x, -y, -z);
        
            s1 = orienatation * s1;
            s2 = orienatation * s2;
            s3 = orienatation * s3;
            s4 = orienatation * s4;
            s5 = orienatation * s5;
            s6 = orienatation * s6;
            s7 = orienatation * s7;
            s8 = orienatation * s8;
        
            s1 += origin;
            s2 += origin;
            s3 += origin;
            s4 += origin;
            s5 += origin;
            s6 += origin;
            s7 += origin;
            s8 += origin;
        
            DebugExtensions.DrawBox(origin, size, orienatation, found ? HitColor : UnHitColor);

            if (!direction.Equals(Vector2.zero) && distance != 0)
            {
                Vector3 d1, d2, d3, d4, d5, d6, d7, d8;
                Vector3 vectorDistance = direction.normalized * GetDrawableDistance(distance);
                
                d1 = s1 + vectorDistance; 
                d2 = s2 + vectorDistance; 
                d3 = s3 + vectorDistance; 
                d4 = s4 + vectorDistance; 
                d5 = s5 + vectorDistance; 
                d6 = s6 + vectorDistance; 
                d7 = s7 + vectorDistance; 
                d8 = s8 + vectorDistance; 
                
                DebugExtensions.DrawBox(origin + vectorDistance, size, orienatation, found ? HitColor : UnHitColor);
        
                Debug.DrawLine(s1, d1, TrajectoryColor);
                Debug.DrawLine(s2, d2, TrajectoryColor);
                Debug.DrawLine(s3, d3, TrajectoryColor);
                Debug.DrawLine(s4, d4, TrajectoryColor);
                Debug.DrawLine(s5, d5, TrajectoryColor);
                Debug.DrawLine(s6, d6, TrajectoryColor);
                Debug.DrawLine(s7, d7, TrajectoryColor);
                Debug.DrawLine(s8, d8, TrajectoryColor);
            }
        }
#endif
        
        #endregion

        #region Sphere

        public static ReadOnlySpan<RaycastHit> SphereCast(Vector3 origin, float radius, Vector3 direction,
            RaycastHit[] results, float distance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            distance = direction == default ? 0 : distance;
            direction = direction == default ? Vector3.forward : direction.normalized;
            int foundNum = Physics.SphereCastNonAlloc(origin, radius, direction, results, distance, layerMask);

#if UNITY_EDITOR
            if (DrawGizmo) DrawSphereCastGizmo(origin, radius, direction, foundNum > 0, distance);
#endif

            var result = new ReadOnlySpan<RaycastHit>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        public static List<SphereDrawData> SphereHandleInfoList = new List<SphereDrawData>();
        
        [Serializable]
        public struct SphereDrawData
        {
            public Vector3 Origin;
            public float Radius;
            public Color Color;
        }
            
        private static void DrawSphereCastGizmo(Vector3 origin, float radius, Vector3 direction, bool found, float distance = Mathf.Infinity)
        {
            if (radius == 0)
            {
                return;
            }

            SphereHandleInfoList.Add(new SphereDrawData(){Origin = origin, Radius = radius, Color = found ? HitColor : UnHitColor});
            if (direction != Vector3.zero && distance != 0)
            {
                Vector3 vectorDistance = direction.normalized * GetDrawableDistance(distance);

                SphereHandleInfoList.Add(new SphereDrawData(){Origin = origin + vectorDistance, Radius = radius, Color = found ? HitColor : UnHitColor});
                
                Vector3 s1, s2, d1, d2;
                var vec = Vector3.Cross(direction, SceneView.lastActiveSceneView.camera.transform.forward).normalized;

                s1 = origin + vec * radius;
                s2 = origin + -vec * radius;

                d1 = origin + vec * radius + vectorDistance;
                d2 = origin + -vec * radius + vectorDistance;
            
                Debug.DrawLine(s1, d1, TrajectoryColor);
                Debug.DrawLine(s2, d2, TrajectoryColor);
            }
        }
#endif

        #endregion
    }
}