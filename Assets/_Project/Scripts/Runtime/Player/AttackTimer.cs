using UnityEngine;

namespace ConceptDemo.Player
{
    /// <summary>
    /// 揮砍的持續時間：Begin 後經過 duration 結束。
    /// 不依賴 Unity 的時間，由呼叫端傳入 deltaTime。
    /// </summary>
    public class AttackTimer
    {
        private const float MinDuration = 0.01f;
        private const float TimeEpsilon = 1e-5f;

        private float _duration;

        public bool IsActive { get; private set; }

        public float Elapsed { get; private set; }

        /// <summary>0 到 1 的進度。</summary>
        public float Progress => _duration > 0f ? Mathf.Clamp01(Elapsed / _duration) : 0f;

        public void Begin(float duration)
        {
            _duration = Mathf.Max(MinDuration, duration);
            Elapsed = 0f;
            IsActive = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return;
            }

            Elapsed += deltaTime;
            if (Elapsed >= _duration - TimeEpsilon)
            {
                IsActive = false;
            }
        }

        public void Cancel()
        {
            IsActive = false;
        }
    }
}
