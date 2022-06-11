using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class PhysicsHelper
    {
        public static RaycastHit2D BoxCast( Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int mask ) 
        {
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, angle, direction, distance, mask);

            //Setting up the points to draw the cast
            Vector2 p1, p2, p3, p4, p5, p6, p7, p8;
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

            Vector2 realDistance = direction.normalized * distance;
            p5 = p1 + realDistance;
            p6 = p2 + realDistance;
            p7 = p3 + realDistance;
            p8 = p4 + realDistance;


            //Drawing the cast
            Color castColor = hit ? Color.red : Color.green;
            Debug.DrawLine(p1, p2, castColor);
            Debug.DrawLine(p2, p3, castColor);
            Debug.DrawLine(p3, p4, castColor);
            Debug.DrawLine(p4, p1, castColor);

            Debug.DrawLine(p5, p6, castColor);
            Debug.DrawLine(p6, p7, castColor);
            Debug.DrawLine(p7, p8, castColor);
            Debug.DrawLine(p8, p5, castColor);

            Debug.DrawLine(p1, p5, Color.grey);
            Debug.DrawLine(p2, p6, Color.grey);
            Debug.DrawLine(p3, p7, Color.grey);
            Debug.DrawLine(p4, p8, Color.grey);
            if(hit) {
            Debug.DrawLine(hit.point, hit.point + hit.normal.normalized * 0.2f, Color.yellow);
            }

            return hit;
        }

        public static RaycastHit2D[] RadialCast( Vector2 origin, float fromDegree, float toDegree, int rayNumber, float distance, int layerMask )
        {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();

            float fromRadius = fromDegree * Mathf.Deg2Rad;
            float radiusCycle = ((toDegree - fromDegree) / (rayNumber - 1)) * Mathf.Deg2Rad;

            for (int i = 0; i < rayNumber; i++)
            {
                float currentRadius = fromRadius + radiusCycle * i;
                RaycastHit2D hit;
                if (hit = Physics2D.Raycast(origin, new Vector2(Mathf.Cos(currentRadius), Mathf.Sin(currentRadius)), distance, layerMask)) hits.Add(hit);
            }

            return hits.ToArray();
        }
    }
}