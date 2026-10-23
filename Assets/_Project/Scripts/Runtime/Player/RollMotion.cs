using UnityEngine;

namespace ConceptDemo.Player
{
    /// <summary>
    /// 翻滾的水平位移：在 duration 內以固定速度移動 distance，最後一步補齊剩餘距離。
    /// 不依賴 Unity 的時間，由呼叫端傳入 deltaTime。
    /// </summary>
    public class RollMotion
    {
        public const float MaxDuration = 0.5f;

        private const float MinDuration = 0.01f;
        private const float TimeEpsilon = 1e-5f;

        private int _direction;
        private float _duration;
        private float _speed;
        private float _remaining;

        public bool IsActive { get; private set; }

        public float Elapsed { get; private set; }

        /// <summary>0 到 1 的進度，給表現（例如 Visual 旋轉）使用。</summary>
        public float Progress => _duration > 0f ? Mathf.Clamp01(Elapsed / _duration) : 0f;

        public void Begin(int direction, float distance, float duration)
        {
            _direction = direction >= 0 ? 1 : -1;
            _duration = Mathf.Clamp(duration, MinDuration, MaxDuration);
            _remaining = Mathf.Max(0f, distance);
            _speed = _remaining / _duration;
            Elapsed = 0f;
            IsActive = true;
        }

        /// <returns>這一步的水平速度；沒有作用時為 0。</returns>
        public float Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return 0f;
            }

            Elapsed += deltaTime;

            float step;
            if (Elapsed >= _duration - TimeEpsilon)
            {
                step = _remaining;
                IsActive = false;
            }
            else
            {
                step = Mathf.Min(_speed * deltaTime, _remaining);
            }

            _remaining -= step;
            return _direction * step / deltaTime;
        }

        public void Cancel()
        {
            IsActive = false;
            _remaining = 0f;
        }
    }
}
