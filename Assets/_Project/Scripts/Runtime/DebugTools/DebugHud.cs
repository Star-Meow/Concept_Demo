using ConceptDemo.Player;
using UnityEngine;

namespace ConceptDemo.DebugTools
{
    /// <summary>在 Game view 左上角顯示玩家的除錯資訊。</summary>
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        [SerializeField] private PlayerController _player;
        [SerializeField, Min(8)] private int _fontSize = 18;

        private GUIStyle _style;

        private void OnGUI()
        {
            if (!_visible || _player == null)
            {
                return;
            }

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = _fontSize,
                    padding = new RectOffset(10, 10, 8, 8),
                };
                _style.normal.textColor = Color.white;
            }

            Vector2 velocity = _player.Velocity;
            string text =
                $"State: {_player.State}\n" +
                $"Velocity: ({velocity.x:F2}, {velocity.y:F2})\n" +
                $"Grounded: {_player.IsGrounded}\n" +
                $"Facing: {(_player.Facing > 0 ? "Right" : "Left")}\n" +
                $"Sprint: {(_player.IsSprinting ? "ON" : "off")}  hold {_player.SprintHoldTime:F2}s\n" +
                $"Roll cooldown: {_player.RollCooldownRemaining:F2}s";

            GUIContent content = new GUIContent(text);
            Vector2 size = _style.CalcSize(content);
            GUI.Box(new Rect(10f, 10f, size.x, size.y), content, _style);
        }
    }
}
