using UnityEngine;

namespace dkstlzu.Utility.Serializables
{
    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(){}
        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator SerializableVector3(Vector3 vector3) => new SerializableVector3(vector3.x, vector3.y, vector3.z);
        public static implicit operator Vector3(SerializableVector3 position) => new Vector3(position.x, position.y, position.z);
        public static implicit operator SerializableVector3(Vector2 vector2) => new SerializableVector3(vector2.x, vector2.y, 0);
        public static implicit operator Vector2(SerializableVector3 position) => new Vector2(position.x, position.y);

        public float this[int index]
        {
            get 
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else if (index == 2) return z;
                else return 0f;
            }
        }
    }
}