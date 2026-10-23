using System;
using UnityEngine;

namespace ConceptDemo.Player
{
    /// <summary>玩家移動的全部可調數值，集中在一個區塊方便在 Inspector 一次調整。</summary>
    [Serializable]
    public class PlayerMovementSettings
    {
        [Header("Move")]
        [SerializeField, Min(0f)] private float _walkSpeed = 6f;
        [SerializeField, Min(0f)] private float _axisDeadzone = 0.5f;

        [Header("Inertia")]
        [SerializeField, Min(0f)] private float _groundAcceleration = 40f;
        [Tooltip("地面減速與反向煞車")]
        [SerializeField, Min(0f)] private float _groundDeceleration = 50f;
        [Tooltip("空中有輸入時（含反向）")]
        [SerializeField, Min(0f)] private float _airAcceleration = 20f;
        [Tooltip("空中沒有輸入時")]
        [SerializeField, Min(0f)] private float _airDeceleration = 2f;

        [Header("Sprint")]
        [SerializeField, Min(0f)] private float _sprintSpeed = 10f;
        [SerializeField, Min(0f)] private float _sprintHoldTime = 0.3f;
        [SerializeField, Min(0f)] private float _doubleTapWindow = 0.3f;

        [Header("Jump")]
        [Tooltip("固定的跳躍高度（最高點離起跳點的距離），不做續力")]
        [SerializeField, Min(0f)] private float _jumpHeight = 3f;
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBufferTime = 0.1f;
        [SerializeField, Min(1f)] private float _fallGravityMultiplier = 1.3f;
        [Tooltip("套用到 Rigidbody2D.gravityScale")]
        [SerializeField, Min(0f)] private float _gravityScale = 3f;

        [Header("Roll")]
        [SerializeField, Min(0f)] private float _rollDistanceInBodyWidths = 4f;
        [SerializeField, Range(0.05f, RollMotion.MaxDuration)] private float _rollDuration = 0.3f;
        [SerializeField, Min(0f)] private float _rollCooldown = 0.3f;

        [Header("Attack")]
        [Tooltip("揮砍持續時間，對應 Attack clip 長度")]
        [SerializeField, Min(0.05f)] private float _attackDuration = 0.5f;

        [Header("Respawn")]
        [Tooltip("掉到這個高度以下就重生；預設為畫面底部再減一個身高")]
        [SerializeField] private float _killY = -8f;

        public float WalkSpeed => _walkSpeed;
        public float AxisDeadzone => _axisDeadzone;
        public float GroundAcceleration => _groundAcceleration;
        public float GroundDeceleration => _groundDeceleration;
        public float AirAcceleration => _airAcceleration;
        public float AirDeceleration => _airDeceleration;
        public float SprintSpeed => _sprintSpeed;
        public float SprintHoldTime => _sprintHoldTime;
        public float DoubleTapWindow => _doubleTapWindow;
        public float JumpHeight => _jumpHeight;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public float FallGravityMultiplier => _fallGravityMultiplier;
        public float GravityScale => _gravityScale;
        public float RollDistanceInBodyWidths => _rollDistanceInBodyWidths;
        public float RollDuration => _rollDuration;
        public float RollCooldown => _rollCooldown;
        public float AttackDuration => _attackDuration;
        public float KillY => _killY;
    }
}
