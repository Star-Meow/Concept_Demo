namespace ConceptDemo.Player
{
    /// <summary>由程式指定的輸入來源，供量測工具或測試使用。</summary>
    public class ScriptedPlayerInput : IPlayerInputSource
    {
        private bool _jumpQueued;
        private bool _rollQueued;

        /// <summary>按住的方向：-1、0、1。</summary>
        public int MoveDirection { get; set; }

        /// <summary>下一次 Read 回報按下 Jump。</summary>
        public void PressJump()
        {
            _jumpQueued = true;
        }

        /// <summary>下一次 Read 回報按下 Roll。</summary>
        public void PressRoll()
        {
            _rollQueued = true;
        }

        public PlayerInputFrame Read()
        {
            var frame = new PlayerInputFrame(MoveDirection, _jumpQueued, _rollQueued);
            _jumpQueued = false;
            _rollQueued = false;
            return frame;
        }
    }
}
