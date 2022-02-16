using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;

namespace Utility
{
    public class BezierCurve
    {
        public static List<BezierCurve> Curves = new List<BezierCurve>();
        public static GameObject CurvesObject;
        public static int Count
        {
            get {return Curves.Count;}
        }

        public BGCurvePointI this[int index]
        {
            get {return Curve[index];}
        }

        public GameObject gameObject
        {
            get {return Curve.gameObject;}
        }

        public BGCurve Curve;

        public BezierCurve(BGCurve curve)
        {
            Curve = curve;
        }

        public static BezierCurve ParabolaFromTo(Transform fromTransform, bool useFromTransform, Transform toTransform, bool useToTransform)
        {
            GameObject newCurveObject;
            (newCurveObject = new GameObject($"BezierCurve{Count}")).transform.SetParent(GetCurvesObject().transform);
            BezierCurve bezierCurve = new BezierCurve(newCurveObject.AddComponent<BGCurve>());

            if (useFromTransform)
            {
                bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, fromTransform, Vector3.zero, BGCurvePoint.ControlTypeEnum.Absent, Vector3.zero, Vector3.zero, true));
            } else
            {
                bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, fromTransform.position, BGCurvePoint.ControlTypeEnum.Absent, Vector3.zero, Vector3.zero, true));
            }

            if (useToTransform)
            {
                bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, toTransform, Vector3.zero, BGCurvePoint.ControlTypeEnum.Absent, Vector3.zero, Vector3.zero, true));
            } else
            {
                bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, toTransform.position, BGCurvePoint.ControlTypeEnum.Absent, Vector3.zero, Vector3.zero, true));
            }

            Vector3 distanceVector = toTransform.InverseTransformPoint(fromTransform.position);
            Vector3 dotPosition;
            int factor;
            if (Random.value > 0.5)
            {
                factor = 1;
            } else
            {
                factor = -1;
            }
            dotPosition = toTransform.position + distanceVector * 4/3 + factor * Vector3.Cross(distanceVector, Vector3.forward).normalized * distanceVector.magnitude/3;
            bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, dotPosition, BGCurvePoint.ControlTypeEnum.BezierSymmetrical, true), 1);

            Vector3 middlePoint = (toTransform.position + fromTransform.position) / 2;
            bezierCurve.Curve[1].ControlFirstLocal = -factor * Vector3.Cross(dotPosition - middlePoint, Vector3.forward)/4;
            var BGCcMath = newCurveObject.AddComponent<BGCcMath>();
            var BGCcCursor = newCurveObject.AddComponent<BGCcCursor>();
            var BGCcCursorObjectTranslate = newCurveObject.AddComponent<BGCcCursorObjectTranslate>();

            Curves.Add(bezierCurve);
            return bezierCurve;
        }

        static GameObject GetCurvesObject()
        {
            if (!CurvesObject)
            {
                CurvesObject = new GameObject("CurvesGameObject");
            }
            return CurvesObject;
        }
    }
}