using UnityEngine;

namespace GameU
{
    static class VectorExtensions
    {
        public static Vector3 ToVector3(this Vector2 v, float y = 0f) => new(v.x, y, v.y);
        public static Vector2 ToVector2(this Vector3 v) => new(v.x, v.z);
    }
}
