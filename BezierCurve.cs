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
        public BGCcCursor Cursor
        {
            get 
            {
                BGCcCursor cursor = null;
                if (!gameObject.TryGetComponent<BGCcCursor>(out cursor))
                {
                    Debug.LogWarning($"{gameObject.name} do not have BGCcCursor");
                }
                return cursor;
            }
        }

        public BezierCurve(BGCurve curve)
        {
            Curve = curve;
        }

        public BGCcMath AddMath()
        {
            return AddBGCc<BGCcMath>() as BGCcMath;
        }

        public BGCcCursor AddCursor()
        {
            return AddBGCc<BGCcCursor>() as BGCcCursor;
        }

        public BGCcSplitterPolyline AddSplitPolyline()
        {
            return AddBGCc<BGCcSplitterPolyline>() as BGCcSplitterPolyline;
        }

        public BGCcSweep2D AddSweep2D()
        {
            return AddBGCc<BGCcSweep2D>() as BGCcSweep2D;
        }

        public BGCcTriangulate2D AddTriangulate2D()
        {
            return AddBGCc<BGCcTriangulate2D>() as BGCcTriangulate2D;
        }

        public BGCcTrs AddTRS(Transform transform)
        {
            var c = AddBGCc<BGCcTrs>() as BGCcTrs;
            c.ObjectToManipulate = transform;
            return c;
        }

        public BGCcCursorChangeLinear AddCursorChangeLinear()
        {
            return AddBGCc<BGCcCursorChangeLinear>() as BGCcCursorChangeLinear;
        }

        public BGCcCursorObjectRotate AddCursorRotate(Transform transform = null)
        {
            var c = AddBGCc<BGCcCursorObjectRotate>() as BGCcCursorObjectRotate;
            c.ObjectToManipulate = transform;
            return c;
        }

        public BGCcCursorObjectScale AddCursorScale(Transform transform)
        {
            var c = AddBGCc<BGCcCursorObjectScale>() as BGCcCursorObjectScale;
            c.ObjectToManipulate = transform;
            return c;
        }

        public BGCcCursorObjectTranslate AddCursorTranslate(Transform transform)
        {
            var c = AddBGCc<BGCcCursorObjectTranslate>() as BGCcCursorObjectTranslate;
            c.ObjectToManipulate = transform;
            return c;
        }

        private BGCc AddBGCc<T>() where T : BGCc
        {
            T Component = null;
            if (gameObject.TryGetComponent<T>(out Component))
            {
                Debug.LogWarning($"{gameObject.name} already has {Component.GetType()}");
            } else
            {
                Component = gameObject.AddComponent<T>();
            }

            return Component;
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

            Vector3 distanceVector = fromTransform.position - toTransform.position;
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
            // MonoBehaviour.print($"FromPosition {fromTransform.position}, ToPosition {toTransform.position}, DotPosition {dotPosition}");
            bezierCurve.Curve.AddPoint(new BGCurvePoint(bezierCurve.Curve, dotPosition, BGCurvePoint.ControlTypeEnum.BezierSymmetrical, true), 1);

            Vector3 middlePoint = (toTransform.position + fromTransform.position) / 2;
            bezierCurve.Curve[1].ControlFirstLocal = -factor * Vector3.Cross(dotPosition - middlePoint, Vector3.forward)/4;
            // var BGCcMath = newCurveObject.AddComponent<BGCcMath>();
            // var BGCcCursor = newCurveObject.AddComponent<BGCcCursor>();
            // var BGCcCursorObjectTranslate = newCurveObject.AddComponent<BGCcCursorObjectTranslate>();

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