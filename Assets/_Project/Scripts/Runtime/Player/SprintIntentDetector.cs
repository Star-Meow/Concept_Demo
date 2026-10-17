namespace ConceptDemo.Player
{
    /// <summary>
    /// 判斷玩家是否要奔跑：同方向按住超過 holdTime，或放開後 doubleTapWindow 內再按同方向。
    /// 不依賴 Unity 的輸入與時間，由呼叫端傳入時間與方向。
    /// </summary>
    public class SprintIntentDetector
    {
        // 累加的浮點時間會有誤差，避免 0.3 s 剛好被算成 0.2999999。
        private const float TimeEpsilon = 1e-4f;

        private float _holdTime;
        private float _doubleTapWindow;

        private int _direction;
        private float _pressTime;
        private int _lastReleaseDirection;
        private float _lastReleaseTime = float.NegativeInfinity;

        public SprintIntentDetector(float holdTime, float doubleTapWindow)
        {
            Configure(holdTime, doubleTapWindow);
        }

        public bool IsSprinting { get; private set; }

        /// <summary>目前方向已連續按住的時間；沒有按住時為 0。</summary>
        public float HoldTime { get; private set; }

        public void Configure(float holdTime, float doubleTapWindow)
        {
            _holdTime = holdTime;
            _doubleTapWindow = doubleTapWindow;
        }

        /// <param name="time">目前時間（秒）。</param>
        /// <param name="directionSign">-1、0、1；0 代表沒有按方向。</param>
        public void Tick(float time, int directionSign)
        {
            directionSign = directionSign > 0 ? 1 : directionSign < 0 ? -1 : 0;

            if (directionSign != _direction)
            {
                if (_direction != 0)
                {
                    _lastReleaseDirection = _direction;
                    _lastReleaseTime = time;
                }

                _direction = directionSign;
                _pressTime = time;
                IsSprinting = directionSign != 0
                    && directionSign == _lastReleaseDirection
                    && time - _lastReleaseTime <= _doubleTapWindow;
            }
            else if (directionSign != 0 && !IsSprinting && time - _pressTime >= _holdTime - TimeEpsilon)
            {
                IsSprinting = true;
            }

            HoldTime = _direction != 0 ? time - _pressTime : 0f;
        }

        /// <summary>清除所有狀態；仍按著的方向會在下一次 Tick 被當成新的按下。</summary>
        public void Reset()
        {
            _direction = 0;
            _pressTime = 0f;
            _lastReleaseDirection = 0;
            _lastReleaseTime = float.NegativeInfinity;
            IsSprinting = false;
            HoldTime = 0f;
        }
    }
}
