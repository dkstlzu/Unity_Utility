using UnityEngine;

namespace dkstlzu.Utility
{
    public static class Math
    {
        /// <summary>
        /// 인풋 값보다 크지만 가장 작은 2의 제곱수
        /// </summary>
        /// <param name="value">value > 0</param>
        public static int PowerOf2Ceiling(int value)
        {
            if (value <= 0)
            {
                return -1;
            }
            
            int result = 1;
            
            while (value > 1)
            {
                result <<= 1;
                value >>= 1;
            }

            return result;
        }

        /// <summary>
        /// 인풋 값보다 작거나 같지만 가장 큰 2의 제곱수
        /// </summary>
        /// <param name="value">value > 0</param>
        public static int PowerOf2Floor(int value)
        {
            if (value <= 0)
            {
                return -1;
            }

            return PowerOf2Ceiling(value) - 1;
        }

        /// <summary>
        /// 원형 범위에서의 균일 분포를 생성합니다.
        /// </summary>
        public static Vector2 GetCircleRandomPoint(Vector2 minMaxRadius) => GetCircleRandomPoint(minMaxRadius.x, minMaxRadius.y);
        
        /// <summary>
        /// 원형 범위에서의 균일 분포를 생성합니다.
        /// </summary>
        public static Vector2 GetCircleRandomPoint(float minRadius, float maxRadius)
        {
            var randomAngle = Random.Range(0, 2 * Mathf.PI);
            var randomDistance = Random.Range(minRadius * minRadius, maxRadius * maxRadius);
            float sqrtDistance = Mathf.Sqrt(randomDistance);
        
            float x = sqrtDistance * Mathf.Cos(randomAngle);
            float y = sqrtDistance * Mathf.Sin(randomAngle);

            return new Vector2(x, y);
        }

        public static float RandomWithIn(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }
    }
}