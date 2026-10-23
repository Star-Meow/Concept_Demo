namespace ConceptDemo.Player
{
    /// <summary>一幀的玩家輸入。</summary>
    public readonly struct PlayerInputFrame
    {
        public PlayerInputFrame(int moveDirection, bool jumpPressed, bool rollPressed, bool attackPressed = false)
        {
            MoveDirection = moveDirection > 0 ? 1 : moveDirection < 0 ? -1 : 0;
            JumpPressed = jumpPressed;
            RollPressed = rollPressed;
            AttackPressed = attackPressed;
        }

        /// <summary>-1、0、1。</summary>
        public int MoveDirection { get; }

        /// <summary>這一幀按下 Jump。</summary>
        public bool JumpPressed { get; }

        /// <summary>這一幀按下 Roll。</summary>
        public bool RollPressed { get; }

        /// <summary>這一幀按下 Attack。</summary>
        public bool AttackPressed { get; }
    }
}
