using UnityEngine;

namespace dkstlzu.Utility
{
    public class AnimationCurveHelper 
    {
        public static AnimationCurve ReverseCurve(AnimationCurve c)
        {
            // TODO: check c is strictly monotonic and Piecewise linear, log error otherwise
            var rev = new AnimationCurve();
            for(int i = c.length - 1; i >= 0; i--) {
                var kf=c.keys[i];
                var rkf = new Keyframe(c[c.length-1].time - kf.time, kf.value);
                rkf.inTangent = -kf.outTangent;
                rkf.outTangent = -kf.inTangent;
                rev.AddKey(rkf);
            }
            return rev;
        }        
    }
}