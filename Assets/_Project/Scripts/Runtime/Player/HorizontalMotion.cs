using UnityEngine;

namespace ConceptDemo.Player
{
    /// <summary>
    /// 帶慣性的水平移動：以加速度趨近目標速度，並決定面向。
    /// 不依賴 Unity 的輸入與時間，由呼叫端傳入 deltaTime。
    /// </summary>
    public class HorizontalMotion
    {
        private float _groundAcceleration;
        private float _groundDeceleration;
        private float _airAcceleration;
        private float _airDeceleration;

        public HorizontalMotion(float groundAcceleration, float groundDeceleration, float airAcceleration, float airDeceleration)
        {
            Configure(groundAcceleration, groundDeceleration, airAcceleration, airDeceleration);
        }

        public void Configure(float groundAcceleration, float groundDeceleration, float airAcceleration, float airDeceleration)
        {
            _groundAcceleration = Mathf.Max(0f, groundAcceleration);
            _groundDeceleration = Mathf.Max(0f, groundDeceleration);
            _airAcceleration = Mathf.Max(0f, airAcceleration);
            _airDeceleration = Mathf.Max(0f, airDeceleration);
        }

        /// <param name="velocity">目前的水平速度。</param>
        /// <param name="input">-1、0、1。</param>
        /// <param name="maxSpeed">目前的目標速度大小（走路或 Sprint）。</param>
        /// <returns>這一步之後的水平速度。</returns>
        public float Step(float velocity, int input, float maxSpeed, bool grounded, float deltaTime)
        {
            float target = input * maxSpeed;
            return Mathf.MoveTowards(velocity, target, GetRate(velocity, input, target, grounded) * deltaTime);
        }

        private float GetRate(float velocity, int input, float target, bool grounded)
        {
            if (!grounded)
            {
                // 空中有輸入（含反向）用空中加速度；沒有輸入時幾乎保留衝力。
                return input != 0 ? _airAcceleration : _airDeceleration;
            }

            bool sameDirection = velocity == 0f || Mathf.Sign(velocity) == input;
            bool speedingUp = input != 0 && sameDirection && Mathf.Abs(target) >= Mathf.Abs(velocity);
            return speedingUp ? _groundAcceleration : _groundDeceleration;
        }

        /// <summary>
        /// 地面上面向跟隨輸入；空中按反方向時，水平速度越過 0、往輸入方向移動後才轉向。
        /// </summary>
        public static int ResolveFacing(int facing, int input, float velocity, bool grounded)
        {
            if (input == 0)
            {
                return facing;
            }

            if (grounded)
            {
                return input;
            }

            return velocity * input > 0f ? input : facing;
        }
    }
}
