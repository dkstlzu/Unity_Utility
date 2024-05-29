using UnityEngine;

namespace dkstlzu.Utility
{
    public static class AnimationCurveExtensions 
    {
        public static AnimationCurve ReverseCurve(this AnimationCurve curve)
        {
            // TODO: check c is strictly monotonic and Piecewise linear, log error otherwise
            var rev = new AnimationCurve();
            for(int i = curve.length - 1; i >= 0; i--) {
                var kf=curve.keys[i];
                var rkf = new Keyframe(curve[curve.length-1].time - kf.time, kf.value);
                rkf.inTangent = -kf.outTangent;
                rkf.outTangent = -kf.inTangent;
                rev.AddKey(rkf);
            }
            return rev;
        }        
    }
}