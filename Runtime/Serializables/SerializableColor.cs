using UnityEngine;

namespace dkstlzu.Utility.Serializables
{
    [System.Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(){}
        public SerializableColor(float r, float g, float b)
        {
            this.r = Mathf.Clamp01(r);
            this.g = Mathf.Clamp01(g);
            this.b = Mathf.Clamp01(b);
            this.a = 1;
        }
        public SerializableColor(float r, float g, float b, float a)
        {
            this.r = Mathf.Clamp01(r);
            this.g = Mathf.Clamp01(g);
            this.b = Mathf.Clamp01(b);
            this.a = Mathf.Clamp01(a);
        }

        public static implicit operator SerializableColor(Color color) => new SerializableColor(color.r, color.g, color.b, color.a);
        public static implicit operator Color(SerializableColor color) => new Color(color.r, color.g, color.b, color.a);        
    }
}