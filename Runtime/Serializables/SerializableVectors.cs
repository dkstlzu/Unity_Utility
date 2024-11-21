#nullable enable

using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace dkstlzu.Utility.Serialization
{
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = sizeof(float) * 2)]
    public partial struct Vector2 : IEquatable<Vector2>
    {
        public static Vector2 Up = new Vector2(0, 1);
        public static Vector2 Down = new Vector2(0, -1);
        public static Vector2 Left = new Vector2(-1, 0);
        public static Vector2 Right = new Vector2(1, 0);
        public static Vector2 Zero = new Vector2(0, 0);
        public static Vector2 One = new Vector2(1, 1);
        public Vector2 xy => new Vector2(x, y);
        public Vector2 yx => new Vector2(y, x);
        public Vector3 xy_ => new Vector3(x, y, 0);
        public Vector3 x_y => new Vector3(x, 0, y);
        public Vector3 _xy => new Vector3(0, x, y);
        public Vector4 xy__ => new Vector4(x, y, 0, 0);
        public Vector4 x_y_ => new Vector4(x, 0, y, 0);
        public Vector4 x__y => new Vector4(x, 0, 0, y);
        public Vector4 yx__ => new Vector4(y, x, 0, 0);
        public Vector4 _xy_ => new Vector4(0, x, y, 0);
        public Vector4 _x_y => new Vector4(0, x, 0, y);
        public Vector4 y_x_ => new Vector4(y, 0, x, 0);
        public Vector4 _yx_ => new Vector4(0, y, x, 0);
        public Vector4 __xy => new Vector4(0, 0, x, y);
        public Vector4 y__x => new Vector4(y, 0, 0, x);
        public Vector4 _y_x => new Vector4(0, y, 0, x);
        public Vector4 __yx => new Vector4(0, 0, y, x);
        
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(Vector2 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }
        
        public Vector2(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }
        
        public Vector2(Vector4 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }

        public static implicit operator Vector2(System.Numerics.Vector2 vector2) => new Vector2(vector2.X, vector2.Y);
        public static implicit operator System.Numerics.Vector2(Vector2 vector2) => new System.Numerics.Vector2(vector2.x, vector2.y);
        public static implicit operator Vector2(UnityEngine.Vector2 vector2) => new Vector2(vector2.x, vector2.y);
        public static implicit operator UnityEngine.Vector2(Vector2 vector2) => new UnityEngine.Vector2(vector2.x, vector2.y);
        public static implicit operator Vector2(UnityEngine.Vector2Int vector2) => new Vector2(vector2.x, vector2.y);
        public static explicit operator UnityEngine.Vector2Int(Vector2 vector2) => new UnityEngine.Vector2Int((int)vector2.x, (int)vector2.y);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y); 
        public static Vector2 operator /(Vector2 a, Vector2 b) => new Vector2(a.x / b.x, a.y / b.y); 
        public static Vector2 operator *(Vector2 a, float scalar) => new Vector2(a.x * scalar, a.y * scalar);
        public static Vector2 operator /(Vector2 a, float scalar) => new Vector2(a.x / scalar, a.y / scalar);
        public static Vector2 operator +(Vector2 a, System.Numerics.Vector2 b) => new Vector2(a.x + b.X, a.y + b.Y); 
        public static Vector2 operator -(Vector2 a, System.Numerics.Vector2 b) => new Vector2(a.x - b.X, a.y - b.Y); 
        public static Vector2 operator *(Vector2 a, System.Numerics.Vector2 b) => new Vector2(a.x * b.X, a.y * b.Y); 
        public static Vector2 operator /(Vector2 a, System.Numerics.Vector2 b) => new Vector2(a.x / b.X, a.y / b.Y); 
        public static Vector2 operator +(Vector2 a, UnityEngine.Vector2 b) => new Vector2(a.x + b.x, a.y + b.y); 
        public static Vector2 operator -(Vector2 a, UnityEngine.Vector2 b) => new Vector2(a.x - b.x, a.y - b.y); 
        public static Vector2 operator *(Vector2 a, UnityEngine.Vector2 b) => new Vector2(a.x * b.x, a.y * b.y); 
        public static Vector2 operator /(Vector2 a, UnityEngine.Vector2 b) => new Vector2(a.x / b.x, a.y / b.y);
        
        public float Magnitude() => MathF.Sqrt(x * x + y * y);
        public Vector2 Normalized => this / Magnitude();

        public void Normalize()
        {
            var magnitude = Magnitude();
            x = x / magnitude;
            y = y / magnitude;
        }
        public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
        public static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        public static float Distance(Vector2 a, Vector2 b) => (a - b).Magnitude();
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);

        public override string ToString() => $"({x}, {y})";
        public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);
        
        public bool Equals(Vector2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }
        public override bool Equals(object? obj)
        {
            return obj is Vector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }


    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = sizeof(float) * 3)]
    public partial struct Vector3 : IEquatable<Vector3>
    {
        public static Vector3 Up = new Vector3(0, 1, 0);
        public static Vector3 Down = new Vector3(0, -1, 0);
        public static Vector3 Left = new Vector3(-1, 0, 0);
        public static Vector3 Right = new Vector3(1, 0, 0);
        public static Vector3 Forward = new Vector3(0, 0, 1);
        public static Vector3 Backward = new Vector3(0, 0, -1);
        public static Vector3 Zero = new Vector3(0, 0, 0);
        public static Vector3 One = new Vector3(1, 1, 1);
        public Vector2 xy => new Vector2(x, y);
        public Vector2 yz => new Vector2(y, z);
        public Vector2 zx => new Vector2(z, x);
        public Vector2 zy => new Vector2(z, y);
        public Vector2 yx => new Vector2(y, x);
        public Vector2 xz => new Vector2(x, z);
        public Vector3 xyz => new Vector3(x, y, z);
        public Vector3 xzy => new Vector3(x, z, y);
        public Vector3 yxz => new Vector3(y, x, z);
        public Vector3 zxy => new Vector3(z, x, y);
        public Vector3 yzx => new Vector3(y, z, x);
        public Vector3 zyx => new Vector3(z, y, x);
        public Vector4 xyz_ => new Vector4(x, y, z, 0);
        public Vector4 xzy_ => new Vector4(x, y, z, 0);
        public Vector4 yxz_ => new Vector4(x, y, z, 0);
        public Vector4 zxy_ => new Vector4(x, y, z, 0);
        public Vector4 yzx_ => new Vector4(x, y, z, 0);
        public Vector4 zyx_ => new Vector4(x, y, z, 0);
        public Vector4 xy_z => new Vector4(x, y, z, 0);
        public Vector4 x_yz => new Vector4(x, y, z, 0);
        public Vector4 _xyz => new Vector4(x, y, z, 0);

        
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public Vector3(Vector2 vector, float z)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = z;
        }

        public Vector3(Vector4 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public static implicit operator Vector3(System.Numerics.Vector3 vector3) => new Vector3(vector3.X, vector3.Y, vector3.Z);
        public static implicit operator System.Numerics.Vector3(Vector3 vector3) => new System.Numerics.Vector3(vector3.x, vector3.y, vector3.z);
        public static implicit operator Vector3(UnityEngine.Vector3 vector3) => new Vector3(vector3.x, vector3.y, vector3.z);
        public static implicit operator UnityEngine.Vector3(Vector3 vector3) => new UnityEngine.Vector3(vector3.x, vector3.y, vector3.z);
        public static implicit operator Vector3(UnityEngine.Vector3Int vector3) => new Vector3(vector3.x, vector3.y, vector3.z);
        public static explicit operator UnityEngine.Vector3Int(Vector3 vector3) => new UnityEngine.Vector3Int((int)vector3.x, (int)vector3.y, (int)vector3.z);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator /(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        public static Vector3 operator /(Vector3 a, float scalar) => new Vector3(a.x / scalar, a.y / scalar, a.z / scalar);
        public static Vector3 operator +(Vector3 a, System.Numerics.Vector3 b) => new Vector3(a.x + b.X, a.y + b.Y, a.z + b.Z);
        public static Vector3 operator -(Vector3 a, System.Numerics.Vector3 b) => new Vector3(a.x - b.X, a.y - b.Y, a.z - b.Z);
        public static Vector3 operator *(Vector3 a, System.Numerics.Vector3 b) => new Vector3(a.x * b.X, a.y * b.Y, a.z * b.Z);
        public static Vector3 operator /(Vector3 a, System.Numerics.Vector3 b) => new Vector3(a.x / b.X, a.y / b.Y, a.z / b.Z);
        public static Vector3 operator +(Vector3 a, UnityEngine.Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, UnityEngine.Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, UnityEngine.Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator /(Vector3 a, UnityEngine.Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public float Magnitude() => MathF.Sqrt(x * x + y * y + z * z);
        public Vector3 Normalized => this / Magnitude();

        public void Normalize()
        {
            var magnitude = Magnitude();
            x = x / magnitude;
            y = y / magnitude;
            z = z / magnitude;
        }
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
        
        public static float Distance(Vector3 a, Vector3 b) => (a - b).Magnitude();
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        
        public override string ToString() => $"({x}, {y}, {z})";
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public bool Equals(Vector3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }
        public override bool Equals(object? obj)
        {
            return obj is Vector3 other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = sizeof(float) * 4)]
    public partial struct Vector4 : IEquatable<Vector4>
    {
        public static Vector4 Identity = new Vector4(0, 0, 0, 1);
        public static Vector4 Zero = new Vector4(0, 0, 0, 0);
        public static Vector4 One = new Vector4(1, 1, 1, 1);
        public Vector2 xy => new Vector2(x, y);
        public Vector2 xz => new Vector2(x, z);
        public Vector2 xw => new Vector2(x, w);
        public Vector2 yx => new Vector2(y, x);
        public Vector2 yz => new Vector2(y, z);
        public Vector2 yw => new Vector2(y, w);
        public Vector2 zx => new Vector2(z, x);
        public Vector2 zy => new Vector2(z, y);
        public Vector2 zw => new Vector2(z, w);
        public Vector2 wx => new Vector2(w, x);
        public Vector2 wy => new Vector2(w, y);
        public Vector2 wz => new Vector2(w, z);
        public Vector3 xyz => new Vector3(x, y, z);
        public Vector3 xzy => new Vector3(x, z, y);
        public Vector3 xyw => new Vector3(x, y, w);
        public Vector3 xwy => new Vector3(x, w, y);
        public Vector3 xzw => new Vector3(x, z, w);
        public Vector3 xwz => new Vector3(x, w, z);
        public Vector3 yxz => new Vector3(y, x, z);
        public Vector3 yzx => new Vector3(y, z, x);
        public Vector3 yxw => new Vector3(y, x, w);
        public Vector3 ywx => new Vector3(y, w, x);
        public Vector3 yzw => new Vector3(y, z, w);
        public Vector3 ywz => new Vector3(y, w, z);
        public Vector3 zxy => new Vector3(z, x, y);
        public Vector3 zyx => new Vector3(z, y, x);
        public Vector3 zxw => new Vector3(z, x, w);
        public Vector3 zwx => new Vector3(z, w, x);
        public Vector3 zyw => new Vector3(z, y, w);
        public Vector3 zwy => new Vector3(z, w, y);
        public Vector3 wxy => new Vector3(w, x, y);
        public Vector3 wyx => new Vector3(w, y, x);
        public Vector3 wxz => new Vector3(w, x, z);
        public Vector3 wzx => new Vector3(w, z, x);
        public Vector3 wyz => new Vector3(w, y, z);
        public Vector3 wzy => new Vector3(w, z, y);
        public Vector4 xyzw => new Vector4(x, y, z, w);
        public Vector4 xywz => new Vector4(x, y, w, z);
        public Vector4 xzyw => new Vector4(x, z, y, w);
        public Vector4 xwyz => new Vector4(x, w, y, z);
        public Vector4 xzwy => new Vector4(x, z, w, y);
        public Vector4 xwzy => new Vector4(x, w, z, y);
        public Vector4 yxzw => new Vector4(y, x, z, w);
        public Vector4 yxwz => new Vector4(y, x, w, z);
        public Vector4 yzxw => new Vector4(y, z, x, w);
        public Vector4 ywxz => new Vector4(y, w, x, z);
        public Vector4 yzwx => new Vector4(y, z, w, x);
        public Vector4 ywzx => new Vector4(y, w, z, x);
        public Vector4 zxyw => new Vector4(z, x, y, w);
        public Vector4 zxwy => new Vector4(z, x, w, y);
        public Vector4 zyxw => new Vector4(z, y, x, w);
        public Vector4 zwxy => new Vector4(z, w, x, y);
        public Vector4 zwyx => new Vector4(z, w, y, x);
        public Vector4 zywx => new Vector4(z, y, w, x);
        public Vector4 wxyz => new Vector4(w, x, y, z);
        public Vector4 wxzy => new Vector4(w, x, z, y);
        public Vector4 wyxz => new Vector4(w, y, x, z);
        public Vector4 wzxy => new Vector4(w, z, x, y);
        public Vector4 wyzx => new Vector4(w, y, z, x);
        public Vector4 wzyx => new Vector4(w, z, y, x);

        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;
        [FieldOffset(12)]
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector4 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
            this.w = vector.w;
        }
        
        public Vector4(Vector2 vector, float z, float w)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = z;
            this.w = w;
        }
        
        public Vector4(Vector3 vector, float w)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
            this.w = w;
        }

        public static implicit operator Vector4(System.Numerics.Vector4 vector4) => new Vector4(vector4.X, vector4.Y, vector4.Z, vector4.W);
        public static implicit operator System.Numerics.Vector4(Vector4 vector4) => new System.Numerics.Vector4(vector4.x, vector4.y, vector4.z, vector4.w);
        public static implicit operator Vector4(UnityEngine.Vector4 vector4) => new Vector4(vector4.x, vector4.y, vector4.z, vector4.w);
        public static implicit operator UnityEngine.Vector4(Vector4 vector4) => new UnityEngine.Vector4(vector4.x, vector4.y, vector4.z, vector4.w);
        
        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static Vector4 operator /(Vector4 a, Vector4 b) => new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        public static Vector4 operator *(Vector4 a, float scalar) => new Vector4(a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
        public static Vector4 operator /(Vector4 a, float scalar) => new Vector4(a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
        public static Vector4 operator +(Vector4 a, System.Numerics.Vector4 b) => new Vector4(a.x + b.X, a.y + b.Y, a.z + b.Z, a.w + b.W);
        public static Vector4 operator -(Vector4 a, System.Numerics.Vector4 b) => new Vector4(a.x - b.X, a.y - b.Y, a.z - b.Z, a.w - b.W);
        public static Vector4 operator *(Vector4 a, System.Numerics.Vector4 b) => new Vector4(a.x * b.X, a.y * b.Y, a.z * b.Z, a.w * b.W);
        public static Vector4 operator /(Vector4 a, System.Numerics.Vector4 b) => new Vector4(a.x / b.X, a.y / b.Y, a.z / b.Z, a.w / b.W);
        public static Vector4 operator +(Vector4 a, UnityEngine.Vector4 b) => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static Vector4 operator -(Vector4 a, UnityEngine.Vector4 b) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static Vector4 operator *(Vector4 a, UnityEngine.Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static Vector4 operator /(Vector4 a, UnityEngine.Vector4 b) => new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        
        public float Magnitude() => MathF.Sqrt(x * x + y * y + z * z + w * w);
        public Vector4 Normalized => this / Magnitude();

        public void Normalize()
        {
            var magnitude = Magnitude();
            x = x / magnitude;
            y = y / magnitude;
            z = z / magnitude;
            w = w / magnitude;
        }
        public static float Dot(Vector4 a, Vector4 b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        public static float Distance(Vector4 a, Vector4 b) => (a - b).Magnitude();
        public static Vector4 Lerp(Vector4 a, Vector4 b, float t) => new Vector4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);

        public override string ToString() => $"({x}, {y}, {z}, {w})"; 
        public static bool operator ==(Vector4 a, Vector4 b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w; 
        public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);
        public bool Equals(Vector4 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w);
        }
    }
}