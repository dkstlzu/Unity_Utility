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
    /// Physics2D 의 종류별 cast에 대해서 gizmo기능을 제공합니다.
    /// 내부적으로 NonAlloc 종류의 cast를 최대한 사용하며(<see cref="RadialCast"/>>는 제외)
    /// hit일 시에 빨간색, un-hit일 시에 초록색으로 표시합니다.
    /// </summary>
    public static class Physics2DHelper
    {
        
#if UNITY_EDITOR
        public struct LineGizmoData
        {
            public Vector3 Origin;
            public Vector2 EndPoint;
            public bool Hit;
        }
#endif
        
#if UNITY_EDITOR
        public static bool DrawGizmo = false;

        public static Color HitColor = Color.red;
        public static Color UnHitColor = Color.green;
        public static Color TrajectoryColor = Color.grey;

        public static int CircleLineSegmentNumber = 20;
#endif

        #region RayCast

        public static ReadOnlySpan<RaycastHit2D> RayCast(Vector3 origin, Vector3 direction, RaycastHit2D[] results, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            int foundNum = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask, minDepth, maxDepth);

#if UNITY_EDITOR
            if (DrawGizmo) DrawRayCastGizmo(origin, direction, foundNum > 0, distance);
#endif

            var result = new ReadOnlySpan<RaycastHit2D>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        static void DrawRayCastGizmo(Vector3 origin, Vector3 direction, bool found, float distance = Mathf.Infinity)
        {
            Debug.DrawLine(origin, origin + direction * distance, found ? HitColor : UnHitColor);
        }
#endif

        #endregion
        
        #region BoxCast

        public static ReadOnlySpan<RaycastHit2D> BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction,
            RaycastHit2D[] results, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers,
            float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            int foundNum = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, minDepth, maxDepth);

#if UNITY_EDITOR
            if (DrawGizmo) DrawBoxCastGizmo(origin, size, angle, direction, foundNum > 0, distance);
#endif

            var result = new ReadOnlySpan<RaycastHit2D>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        static void DrawBoxCastGizmo(Vector2 origin, Vector2 size, float angle, Vector2 direction, bool found, float distance = Mathf.Infinity)
        {
            if (size.magnitude == 0)
            {
                return;    
            }
            
            //Setting up the start points to draw the cast
            Vector2 p1, p2, p3, p4;
            float w = size.x * 0.5f;
            float h = size.y * 0.5f;
            p1 = new Vector2(-w, h);
            p2 = new Vector2(w, h);
            p3 = new Vector2(w, -h);
            p4 = new Vector2(-w, -h);
        
            Quaternion q = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
            p1 = q * p1;
            p2 = q * p2;
            p3 = q * p3;
            p4 = q * p4;
        
            p1 += origin;
            p2 += origin;
            p3 += origin;
            p4 += origin;
        
            DebugExtensions.DrawBox(origin, size, angle, found ? HitColor : UnHitColor);

            // End points version
            if (!direction.Equals(Vector2.zero) && distance > 0)
            {
                Vector2 p5, p6, p7, p8;
                Vector2 vectorDistance = direction.normalized * distance;
                p5 = p1 + vectorDistance;
                p6 = p2 + vectorDistance;
                p7 = p3 + vectorDistance;
                p8 = p4 + vectorDistance; 
                
                DebugExtensions.DrawBox(origin + vectorDistance, size, angle, found ? HitColor : UnHitColor);
        
                Debug.DrawLine(p1, p5, TrajectoryColor);
                Debug.DrawLine(p2, p6, TrajectoryColor);
                Debug.DrawLine(p3, p7, TrajectoryColor);
                Debug.DrawLine(p4, p8, TrajectoryColor);
            }
        }
#endif
        
        #endregion

        #region CircleCast

        public static ReadOnlySpan<RaycastHit2D> CircleCast(Vector2 origin, float radius, Vector2 direction,
            RaycastHit2D[] results, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers,
            float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            int foundNum = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, minDepth, maxDepth);

#if UNITY_EDITOR
            if (DrawGizmo) DrawCircleCastGizmo(origin, radius, direction, foundNum > 0, distance);
#endif

            var result = new ReadOnlySpan<RaycastHit2D>(results);
            return result.Slice(0, foundNum);
        }

#if UNITY_EDITOR
        private static void DrawCircleCastGizmo(Vector2 origin, float radius, Vector2 direction, bool found, float distance = Mathf.Infinity)
        {
            if (radius == 0)
            {
                return;
            }
            
            DebugExtensions.DrawCircle(origin, radius, CircleLineSegmentNumber, found ? HitColor : UnHitColor);
            // Setting up the points to draw the cast
            Vector2[] startCirclePoints = new Vector2[CircleLineSegmentNumber];

            float anglePerSegment = (float)System.Math.PI * 2 / CircleLineSegmentNumber;

            float angle = 0;
            
            for (int i = 0; i < CircleLineSegmentNumber; i++, angle += anglePerSegment)
            {
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                startCirclePoints[i] = new Vector2(x * radius, y * radius);
            }            
            
            for (int i = 0; i < CircleLineSegmentNumber; i++)
            {
                startCirclePoints[i] += origin;
            }

            if (!direction.Equals(Vector2.zero) && distance > 0)
            {
                Vector2[] endCirclePoints = new Vector2[CircleLineSegmentNumber];
                
                Vector2 vectorDistance = direction.normalized * distance;
                DebugExtensions.DrawCircle(origin + vectorDistance, radius, CircleLineSegmentNumber, found ? HitColor : UnHitColor);

                for (int i = 0; i < CircleLineSegmentNumber; i++)
                {
                    endCirclePoints[i] = startCirclePoints[i] + vectorDistance;
                }
                
                for (int i = 0; i < CircleLineSegmentNumber; i++)
                {
                    Debug.DrawLine(startCirclePoints[i], endCirclePoints[i], TrajectoryColor);
                }
            }
        }
#endif

        #endregion

        #region RadialCast
        
        /// <summary>
        /// NonAlloc 버전의 구현이 아닙니다. 
        /// </summary>
        public static RaycastHit2D[] RadialCast(Vector2 origin, float fromDegree, float toDegree, int rayNumber, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
#if UNITY_EDITOR
            // Only for gizmos
            List<LineGizmoData> gizmoDatas = new List<LineGizmoData>();
#endif

            float fromRadius = fromDegree * Mathf.Deg2Rad;
            float radiusCycle = ((toDegree - fromDegree) / (rayNumber - 1)) * Mathf.Deg2Rad;

            for (int i = 0; i < rayNumber; i++)
            {
                RaycastHit2D hit;

                float currentRadius = fromRadius + radiusCycle * i;
                var direction = new Vector2(Mathf.Cos(currentRadius), Mathf.Sin(currentRadius));
                
                if (hit = Physics2D.Raycast(origin, direction, distance, layerMask))
                {
                    hits.Add(hit);
#if UNITY_EDITOR
                    gizmoDatas.Add(new LineGizmoData()
                    {
                        Origin = origin,
                        EndPoint = hit.point,
                        Hit = true,
                    });
                }
                else
                {
                    gizmoDatas.Add(new LineGizmoData()
                    {
                        Origin = origin,
                        EndPoint = origin + direction * distance,
                        Hit = false,
                    });
#endif
                }
            }

#if UNITY_EDITOR
            if (DrawGizmo) DrawRadialCastGizmo(gizmoDatas);
#endif

            return hits.ToArray();
        }
        
#if UNITY_EDITOR
        private static void DrawRadialCastGizmo(List<LineGizmoData> datas)
        {
            if (datas.Count == 0)
            {
                return;
            }

            for (int i = 0; i < datas.Count; i++)
            {
                Debug.DrawLine(datas[i].Origin, datas[i].EndPoint, datas[i].Hit ? HitColor : UnHitColor);

                if (i > 0)
                {
                    Debug.DrawLine(datas[i].EndPoint, datas[i-1].EndPoint, datas[i].Hit || datas[i-1].Hit ? HitColor : UnHitColor);                    
                }
            }
        }
#endif

        #endregion
    }
}