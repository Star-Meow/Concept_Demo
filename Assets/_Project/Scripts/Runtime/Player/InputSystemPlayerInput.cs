using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ConceptDemo.Player
{
    /// <summary>預設的輸入來源：鍵盤與滑鼠，使用程式內建立的 InputAction（不建立 .inputactions 資產）。</summary>
    public class InputSystemPlayerInput : IPlayerInputSource, IDisposable
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _rollAction;
        private readonly InputAction _attackAction;

        public InputSystemPlayerInput()
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

            _attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
        }

        /// <summary>小於此值的軸輸入視為 0。</summary>
        public float AxisDeadzone { get; set; } = 0.5f;

        public void Enable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _rollAction.Enable();
            _attackAction.Enable();
        }

        public void Disable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
            _rollAction.Disable();
            _attackAction.Disable();
        }

        public PlayerInputFrame Read()
        {
            float axis = _moveAction.ReadValue<float>();
            int move = Mathf.Abs(axis) >= AxisDeadzone ? (int)Mathf.Sign(axis) : 0;
            return new PlayerInputFrame(
                move,
                _jumpAction.WasPressedThisFrame(),
                _rollAction.WasPressedThisFrame(),
                _attackAction.WasPressedThisFrame());
        }

        public void Dispose()
        {
            _moveAction.Dispose();
            _jumpAction.Dispose();
            _rollAction.Dispose();
            _attackAction.Dispose();
        }
    }
}
