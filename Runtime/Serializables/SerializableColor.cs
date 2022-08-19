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

        public override string ToString()
        {
            return string.Format("r : {0}, g : {1}, b : {2}, a : {3}", r, g, b, a);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            SerializableColor colorObj = obj as SerializableColor;
            
            return r == colorObj.r && g == colorObj.g && b == colorObj.b && a == colorObj.a;
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator SerializableColor(Color color) => new SerializableColor(color.r, color.g, color.b, color.a);
        public static implicit operator Color(SerializableColor color) => new Color(color.r, color.g, color.b, color.a);
    }
}