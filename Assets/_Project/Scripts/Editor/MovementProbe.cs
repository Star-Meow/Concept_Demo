using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ConceptDemo.Player;
using UnityEditor;
using UnityEngine;

namespace ConceptDemo.Editor
{
    /// <summary>
    /// Play Mode 量測工具：依時間軸透過 ScriptedPlayerInput 送入輸入，逐幀記錄角色狀態到 CSV。
    /// 量測期間 Time.captureFramerate = 50（每幀固定 0.02 s）並開啟 runInBackground，
    /// Editor 沒有焦點時時序也精確；結束後還原。兩者都只在該次 Play Mode 生效。
    /// </summary>
    /// <example>
    /// MovementProbe.Run("walk", "0 respawn; 0.5 move 1; 1.5 move 0", 2.5f);
    /// 指令：respawn、move -1|0|1、jump、roll、teleport x y、pause（暫停 Play Mode 並結束量測，供截圖）。
    /// </example>
    public static class MovementProbe
    {
        public const string OutputFolder = "Temp/MovementProbe";
        public const int CaptureFramerate = 50;

        private const string StatusKey = "ConceptDemo.MovementProbe.Status";

        private static Session _session;

        public static bool IsRunning => _session != null;

        /// <summary>最近一次量測的狀態：running、done:&lt;csv 路徑&gt; 或 error:&lt;訊息&gt;。</summary>
        public static string Status => SessionState.GetString(StatusKey, "idle");

        public static string Run(string name, string timeline, float duration)
        {
            if (!EditorApplication.isPlaying)
            {
                return SetStatus("error: 需要在 Play Mode 執行");
            }

            if (_session != null)
            {
                return SetStatus("error: 已有量測在執行");
            }

            PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                return SetStatus("error: 場景中找不到 PlayerController");
            }

            List<Step> steps;
            try
            {
                steps = ParseTimeline(timeline);
            }
            catch (FormatException e)
            {
                return SetStatus("error: " + e.Message);
            }

            _session = new Session(name, steps, duration, player);
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            return SetStatus("running");
        }

        public static void Cancel()
        {
            Finish("error: 已取消");
        }

        private static void Tick()
        {
            Session session = _session;
            if (session == null)
            {
                return;
            }

            if (session.Player == null)
            {
                Finish("error: PlayerController 已不存在");
                return;
            }

            float elapsed = Time.time - session.StartTime;
            while (session.NextStep < session.Steps.Count && session.Steps[session.NextStep].Time <= elapsed + 1e-4f)
            {
                Step step = session.Steps[session.NextStep++];
                if (!session.Apply(step))
                {
                    session.Sample(elapsed);
                    Finish("done:" + session.WriteCsv());
                    return;
                }
            }

            session.Sample(elapsed);

            if (elapsed >= session.Duration)
            {
                Finish("done:" + session.WriteCsv());
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                Finish("error: Play Mode 已結束");
            }
        }

        private static void Finish(string status)
        {
            if (_session == null)
            {
                return;
            }

            _session.Restore();
            _session = null;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SetStatus(status);
        }

        private static string SetStatus(string status)
        {
            SessionState.SetString(StatusKey, status);
            return status;
        }

        private static List<Step> ParseTimeline(string timeline)
        {
            var steps = new List<Step>();
            foreach (string raw in (timeline ?? string.Empty).Split(';'))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !TryParse(parts[0], out float time))
                {
                    throw new FormatException("無法解析時間軸指令：" + line);
                }

                var args = new float[parts.Length - 2];
                for (int i = 2; i < parts.Length; i++)
                {
                    if (!TryParse(parts[i], out args[i - 2]))
                    {
                        throw new FormatException("無法解析參數：" + line);
                    }
                }

                steps.Add(new Step(time, parts[1].ToLowerInvariant(), args));
            }

            steps.Sort((a, b) => a.Time.CompareTo(b.Time));
            return steps;
        }

        private static bool TryParse(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private readonly struct Step
        {
            public Step(float time, string command, float[] args)
            {
                Time = time;
                Command = command;
                Args = args;
            }

            public float Time { get; }
            public string Command { get; }
            public float[] Args { get; }
        }

        private class Session
        {
            private readonly ScriptedPlayerInput _input = new ScriptedPlayerInput();
            private readonly Rigidbody2D _rigidbody;
            private readonly StringBuilder _csv = new StringBuilder();
            private readonly int _previousCaptureFramerate;
            private readonly bool _previousRunInBackground;
            private readonly string _name;
            private int _lastSampledFrame = -1;

            public Session(string name, List<Step> steps, float duration, PlayerController player)
            {
                _name = string.IsNullOrWhiteSpace(name) ? "probe" : name;
                Steps = steps;
                Duration = duration;
                Player = player;
                _rigidbody = player.GetComponent<Rigidbody2D>();

                _previousCaptureFramerate = Time.captureFramerate;
                _previousRunInBackground = Application.runInBackground;
                Time.captureFramerate = CaptureFramerate;
                Application.runInBackground = true;
                player.SetInputSource(_input);

                StartTime = Time.time;
                _csv.AppendLine("frame,t,x,y,vx,vy,state,grounded,facing,sprinting,sprintHold,rollCooldown,rotZ,move");
            }

            public List<Step> Steps { get; }
            public float Duration { get; }
            public PlayerController Player { get; }
            public float StartTime { get; }
            public int NextStep { get; set; }

            /// <returns>false 代表量測應該結束（pause）。</returns>
            public bool Apply(Step step)
            {
                switch (step.Command)
                {
                    case "respawn":
                        Player.Respawn();
                        break;
                    case "move":
                        _input.MoveDirection = step.Args.Length > 0 ? Mathf.RoundToInt(step.Args[0]) : 0;
                        break;
                    case "jump":
                        _input.PressJump();
                        break;
                    case "roll":
                        _input.PressRoll();
                        break;
                    case "teleport":
                        if (step.Args.Length >= 2)
                        {
                            var position = new Vector2(step.Args[0], step.Args[1]);
                            _rigidbody.position = position;
                            Player.transform.position = position;
                            _rigidbody.linearVelocity = Vector2.zero;
                        }

                        break;
                    case "pause":
                        EditorApplication.isPaused = true;
                        return false;
                    default:
                        Debug.LogWarning($"MovementProbe: 未知指令 {step.Command}");
                        break;
                }

                return true;
            }

            public void Sample(float elapsed)
            {
                if (Time.frameCount == _lastSampledFrame)
                {
                    return;
                }

                _lastSampledFrame = Time.frameCount;
                Vector2 position = _rigidbody.position;
                Vector2 velocity = Player.Velocity;
                _csv.Append(Time.frameCount).Append(',')
                    .Append(F(elapsed)).Append(',')
                    .Append(F(position.x)).Append(',')
                    .Append(F(position.y)).Append(',')
                    .Append(F(velocity.x)).Append(',')
                    .Append(F(velocity.y)).Append(',')
                    .Append(Player.State).Append(',')
                    .Append(Player.IsGrounded ? 1 : 0).Append(',')
                    .Append(Player.Facing).Append(',')
                    .Append(Player.IsSprinting ? 1 : 0).Append(',')
                    .Append(F(Player.SprintHoldTime)).Append(',')
                    .Append(F(Player.RollCooldownRemaining)).Append(',')
                    .Append(F(Player.VisualRotationZ)).Append(',')
                    .Append(_input.MoveDirection)
                    .AppendLine();
            }

            public string WriteCsv()
            {
                Directory.CreateDirectory(OutputFolder);
                string path = Path.Combine(OutputFolder, _name + ".csv").Replace('\\', '/');
                File.WriteAllText(path, _csv.ToString());
                return path;
            }

            public void Restore()
            {
                Time.captureFramerate = _previousCaptureFramerate;
                Application.runInBackground = _previousRunInBackground;
                if (Player != null)
                {
                    Player.SetInputSource(null);
                }
            }

            private static string F(float value)
            {
                return value.ToString("0.####", CultureInfo.InvariantCulture);
            }
        }
    }
}
