using UnityEngine;

namespace dkstlzu.Utility
{
    public static class DebugExtensions
    {
        public static void DrawBox(Vector3 center, Vector3 size, Quaternion orientation, Color color)
        {
            Vector3 p1, p2, p3, p4, p5, p6, p7, p8;
            float x = size.x;
            float y = size.y;
            float z = size.z;
            
            p1 = new Vector3(x, y, z);
            p2 = new Vector3(x, y, -z);
            p3 = new Vector3(x, -y, -z);
            p4 = new Vector3(x, -y, z);
            p5 = new Vector3(-x, y, z);
            p6 = new Vector3(-x, y, -z);
            p7 = new Vector3(-x, -y, -z);
            p8 = new Vector3(-x, -y, z);
        
            p1 = orientation * p1 + center;
            p2 = orientation * p2 + center;
            p3 = orientation * p3 + center;
            p4 = orientation * p4 + center;
            p5 = orientation * p5 + center;
            p6 = orientation * p6 + center;
            p7 = orientation * p7 + center;
            p8 = orientation * p8 + center;
        
            // Drawing the cast of start points
            Debug.DrawLine(p1, p2, color);
            Debug.DrawLine(p2, p3, color);
            Debug.DrawLine(p3, p4, color);
            Debug.DrawLine(p4, p1, color);
            
            Debug.DrawLine(p5, p6, color);
            Debug.DrawLine(p6, p7, color);
            Debug.DrawLine(p7, p8, color);
            Debug.DrawLine(p8, p5, color);
            
            Debug.DrawLine(p1, p5, color);
            Debug.DrawLine(p2, p6, color);
            Debug.DrawLine(p3, p7, color);
            Debug.DrawLine(p4, p8, color);
        }
        
        public static void DrawBox(Vector2 center, Vector2 size, float angle, Color color)
        {
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
        
            p1 += center;
            p2 += center;
            p3 += center;
            p4 += center;
        
            // Drawing the cast of start points
            Debug.DrawLine(p1, p2, color);
            Debug.DrawLine(p2, p3, color);
            Debug.DrawLine(p3, p4, color);
            Debug.DrawLine(p4, p1, color);
        }
        
        public static void DrawCircle(Vector2 point, float radius, int lineSegmentNum, Color color)
        {
            // Setting up the points to draw the cast
            Vector2[] startCirclePoints = new Vector2[lineSegmentNum];

            float anglePerSegment = (float)System.Math.PI * 2 / lineSegmentNum;

            float angle = 0;
            
            for (int i = 0; i < lineSegmentNum; i++, angle += anglePerSegment)
            {
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                startCirclePoints[i] = new Vector2(x * radius, y * radius);
            }            
            
            for (int i = 0; i < lineSegmentNum; i++)
            {
                startCirclePoints[i] += point;
            }
            
            for (int i = 0; i < lineSegmentNum - 1; i++)
            {
                Debug.DrawLine(startCirclePoints[i], startCirclePoints[i+1], color);
            }
            Debug.DrawLine(startCirclePoints[lineSegmentNum-1], startCirclePoints[0], color);
        }
    }
}