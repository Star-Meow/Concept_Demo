using UnityEngine;
using UnityEngine.InputSystem;

namespace ConceptDemo.Player
{
    /// <summary>
    /// 玩家移動：Move、Sprint、Jump、Roll、Respawn。
    /// 輸入在 Update 鎖存，在 FixedUpdate 消耗。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        private const float GroundCheckHeight = 0.1f;
        private const float GroundCheckWidthRatio = 0.9f;
        private const float RisingThreshold = 0.01f;
        private const float MovingThreshold = 0.01f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
        private static readonly int IsRollingHash = Animator.StringToHash("IsRolling");

        [SerializeField] private PlayerMovementSettings _settings = new PlayerMovementSettings();

        [Header("References")]
        [Tooltip("場景物件，Prefab 上留空，由場景實例指定")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _visual;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private LayerMask _groundLayers;

        [Header("Placeholder Colors")]
        [SerializeField] private Color _idleColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color _walkColor = new Color(0.4f, 0.85f, 0.4f);
        [SerializeField] private Color _sprintColor = new Color(1f, 0.6f, 0.15f);
        [SerializeField] private Color _jumpColor = new Color(0.3f, 0.85f, 1f);
        [SerializeField] private Color _fallColor = new Color(0.55f, 0.5f, 1f);
        [SerializeField] private Color _rollColor = new Color(1f, 0.35f, 0.7f);

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _rollAction;

        private readonly RollMotion _roll = new RollMotion();
        private SprintIntentDetector _sprint;

        private Vector2 _fallbackSpawnPosition;
        private int _moveInput;
        private bool _jumpHeld;
        private bool _jumpPressedLatched;
        private bool _jumpReleasedLatched;
        private bool _rollPressedLatched;

        private float _lastGroundedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;
        private bool _hasGroundJump;
        private float _rollCooldownEndTime;
        private int _rollDirection = 1;

        public PlayerMovementSettings Settings => _settings;
        public PlayerState State { get; private set; } = PlayerState.Idle;
        public Vector2 Velocity => _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;
        public bool IsGrounded { get; private set; }
        public int Facing { get; private set; } = 1;
        public bool IsSprinting => _sprint != null && _sprint.IsSprinting;
        public float SprintHoldTime => _sprint != null ? _sprint.HoldTime : 0f;
        public float RollCooldownRemaining => Mathf.Max(0f, _rollCooldownEndTime - Time.time);
        public float ColliderWidth => _collider.size.x * Mathf.Abs(transform.lossyScale.x);
        public float VisualRotationZ => _visual != null ? _visual.localEulerAngles.z : 0f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _rigidbody.gravityScale = _settings.GravityScale;
            _sprint = new SprintIntentDetector(_settings.SprintHoldTime, _settings.DoubleTapWindow);
            _fallbackSpawnPosition = _rigidbody.position;
            CreateInputActions();
        }

        private void Start()
        {
            if (_spawnPoint == null)
            {
                Debug.LogWarning($"{name}: 沒有指定 SpawnPoint，重生時會回到初始位置。", this);
            }
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _jumpAction?.Enable();
            _rollAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _rollAction?.Disable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _jumpAction?.Dispose();
            _rollAction?.Dispose();
        }

        private void CreateInputActions()
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");

            _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");

            _rollAction = new InputAction("Roll", InputActionType.Button, "<Keyboard>/leftShift");
            _rollAction.AddBinding("<Keyboard>/rightShift");
        }

        private void Update()
        {
            float axis = _moveAction.ReadValue<float>();
            _moveInput = Mathf.Abs(axis) >= _settings.AxisDeadzone ? (int)Mathf.Sign(axis) : 0;

            _jumpHeld = _jumpAction.IsPressed();
            if (_jumpAction.WasPressedThisFrame())
            {
                _jumpPressedLatched = true;
            }

            if (_jumpAction.WasReleasedThisFrame())
            {
                _jumpReleasedLatched = true;
            }

            if (_rollAction.WasPressedThisFrame())
            {
                _rollPressedLatched = true;
            }

            // 翻滾中不累積 Sprint，翻滾結束後才重新開始計時。
            if (!_roll.IsActive)
            {
                _sprint.Configure(_settings.SprintHoldTime, _settings.DoubleTapWindow);
                _sprint.Tick(Time.time, _moveInput);
            }
        }

        private void FixedUpdate()
        {
            float now = Time.time;
            float deltaTime = Time.fixedDeltaTime;

            if (_rigidbody.position.y < _settings.KillY)
            {
                Respawn();
                return;
            }

            Vector2 velocity = _rigidbody.linearVelocity;
            UpdateGrounded(now, velocity);

            bool jumpPressed = ConsumeLatch(ref _jumpPressedLatched);
            bool jumpReleased = ConsumeLatch(ref _jumpReleasedLatched);
            bool rollPressed = ConsumeLatch(ref _rollPressedLatched);

            bool rolledThisStep = false;
            if (_roll.IsActive)
            {
                if (!IsGrounded)
                {
                    EndRoll(now);
                }
                else
                {
                    velocity.x = _roll.Tick(deltaTime);
                    rolledThisStep = true;
                    if (!_roll.IsActive)
                    {
                        EndRoll(now);
                    }
                }
            }

            // 翻滾中的 Jump 按壓直接丟棄，不進 buffer。
            if (jumpPressed && !rolledThisStep)
            {
                _lastJumpPressedTime = now;
            }

            if (!rolledThisStep)
            {
                velocity = ApplyMoveAndJump(now, velocity, jumpReleased);

                if (rollPressed && CanRoll(now))
                {
                    StartRoll();
                    velocity.x = _roll.Tick(deltaTime);
                }
            }

            _rigidbody.gravityScale = velocity.y < 0f
                ? _settings.GravityScale * _settings.FallGravityMultiplier
                : _settings.GravityScale;
            _rigidbody.linearVelocity = velocity;

            UpdateState(velocity);
            UpdatePresentation(velocity);
        }

        private Vector2 ApplyMoveAndJump(float now, Vector2 velocity, bool jumpReleased)
        {
            if (_moveInput != 0)
            {
                Facing = _moveInput;
            }

            float speed = _sprint.IsSprinting ? _settings.SprintSpeed : _settings.WalkSpeed;
            velocity.x = _moveInput * speed;

            bool buffered = now - _lastJumpPressedTime <= _settings.JumpBufferTime;
            bool inCoyote = now - _lastGroundedTime <= _settings.CoyoteTime;
            if (buffered && inCoyote && _hasGroundJump)
            {
                velocity.y = _settings.JumpVelocity;
                _lastJumpPressedTime = float.NegativeInfinity;
                _hasGroundJump = false;
                IsGrounded = false;

                // 按下與放開落在同一步（或 buffer 期間已放開）時，直接套用可變跳高。
                if (!_jumpHeld)
                {
                    velocity.y *= _settings.JumpCutMultiplier;
                }
            }
            else if (jumpReleased && velocity.y > 0f && !IsGrounded)
            {
                velocity.y *= _settings.JumpCutMultiplier;
            }

            return velocity;
        }

        private void UpdateGrounded(float now, Vector2 velocity)
        {
            Vector2 center = _groundCheck != null ? (Vector2)_groundCheck.position : _rigidbody.position;
            Vector2 size = new Vector2(ColliderWidth * GroundCheckWidthRatio, GroundCheckHeight);
            bool overlapping = Physics2D.OverlapBox(center, size, 0f, _groundLayers) != null;

            // 上升中不算著地，避免起跳當下 OverlapBox 仍碰到地面而誤判。
            IsGrounded = overlapping && velocity.y <= RisingThreshold;
            if (IsGrounded)
            {
                _lastGroundedTime = now;
                _hasGroundJump = true;
            }
        }

        private bool CanRoll(float now)
        {
            return IsGrounded && !_roll.IsActive && now >= _rollCooldownEndTime;
        }

        private void StartRoll()
        {
            _rollDirection = Facing;
            _sprint.Reset();
            _lastJumpPressedTime = float.NegativeInfinity;
            _roll.Begin(_rollDirection, _settings.RollDistanceInBodyWidths * ColliderWidth, _settings.RollDuration);
        }

        private void EndRoll(float now)
        {
            _roll.Cancel();
            _rollCooldownEndTime = now + _settings.RollCooldown;
            _sprint.Reset();
            ResetVisualRotation();
        }

        /// <summary>回到 SpawnPoint，並重置速度、狀態、面向、Roll 冷卻、Visual 旋轉與 Sprint 偵測器。</summary>
        public void Respawn()
        {
            Vector2 position = _spawnPoint != null ? (Vector2)_spawnPoint.position : _fallbackSpawnPosition;
            _rigidbody.position = position;
            transform.position = position;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.gravityScale = _settings.GravityScale;

            _roll.Cancel();
            _rollCooldownEndTime = 0f;
            _sprint.Reset();
            ResetVisualRotation();

            Facing = 1;
            IsGrounded = false;
            _hasGroundJump = false;
            _lastGroundedTime = float.NegativeInfinity;
            _lastJumpPressedTime = float.NegativeInfinity;
            _jumpPressedLatched = false;
            _jumpReleasedLatched = false;
            _rollPressedLatched = false;

            State = PlayerState.Idle;
            UpdatePresentation(Vector2.zero);
        }

        private void UpdateState(Vector2 velocity)
        {
            bool moving = Mathf.Abs(velocity.x) > MovingThreshold;

            if (_roll.IsActive)
            {
                State = PlayerState.Roll;
            }
            else if (!IsGrounded)
            {
                State = velocity.y > RisingThreshold ? PlayerState.Jump : PlayerState.Fall;
            }
            else if (moving && _sprint.IsSprinting)
            {
                State = PlayerState.Sprint;
            }
            else if (moving)
            {
                State = PlayerState.Walk;
            }
            else
            {
                State = PlayerState.Idle;
            }
        }

        private void UpdatePresentation(Vector2 velocity)
        {
            if (_visual != null && _roll.IsActive)
            {
                _visual.localRotation = Quaternion.Euler(0f, 0f, -360f * _roll.Progress * _rollDirection);
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = GetStateColor(State);
            }

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetFloat(SpeedHash, Mathf.Abs(velocity.x));
                _animator.SetFloat(VerticalSpeedHash, velocity.y);
                _animator.SetBool(IsGroundedHash, IsGrounded);
                _animator.SetBool(IsSprintingHash, _sprint.IsSprinting);
                _animator.SetBool(IsRollingHash, _roll.IsActive);
            }
        }

        private void ResetVisualRotation()
        {
            if (_visual != null)
            {
                _visual.localRotation = Quaternion.identity;
            }
        }

        private Color GetStateColor(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Walk:
                    return _walkColor;
                case PlayerState.Sprint:
                    return _sprintColor;
                case PlayerState.Jump:
                    return _jumpColor;
                case PlayerState.Fall:
                    return _fallColor;
                case PlayerState.Roll:
                    return _rollColor;
                default:
                    return _idleColor;
            }
        }

        private static bool ConsumeLatch(ref bool latch)
        {
            bool value = latch;
            latch = false;
            return value;
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null)
            {
                return;
            }

            BoxCollider2D box = GetComponent<BoxCollider2D>();
            float width = box != null ? box.size.x * Mathf.Abs(transform.lossyScale.x) : 1f;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(_groundCheck.position, new Vector3(width * GroundCheckWidthRatio, GroundCheckHeight, 0f));
        }
    }
}
