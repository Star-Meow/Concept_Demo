using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class SprintIntentDetectorTests
    {
        private const float HoldTime = 0.3f;
        private const float DoubleTapWindow = 0.3f;
        private const float Step = 0.02f;

        private SprintIntentDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _detector = new SprintIntentDetector(HoldTime, DoubleTapWindow);
        }

        /// <summary>從 start 到 end（含）每 Step 秒以相同方向呼叫一次 Tick，回傳最後的時間。</summary>
        private float Hold(float start, float end, int direction)
        {
            float time = start;
            for (; time <= end + 1e-4f; time += Step)
            {
                _detector.Tick(time, direction);
            }

            return time - Step;
        }

        [Test]
        public void HoldShorterThanHoldTime_IsNotSprinting()
        {
            Hold(0f, 0.25f, 1);

            Assert.IsFalse(_detector.IsSprinting);
        }

        [Test]
        public void HoldAtLeastHoldTime_IsSprinting()
        {
            Hold(0f, 0.3f, 1);

            Assert.IsTrue(_detector.IsSprinting);
        }

        [Test]
        public void DoubleTapWithinWindow_IsSprintingOnSecondPress()
        {
            Hold(0f, 0.05f, 1);
            Hold(0.06f, 0.15f, 0);
            _detector.Tick(0.16f, 1);

            Assert.IsTrue(_detector.IsSprinting);
        }

        [Test]
        public void DoubleTapOutsideWindow_IsNotSprinting()
        {
            Hold(0f, 0.05f, 1);
            Hold(0.06f, 0.6f, 0);
            _detector.Tick(0.62f, 1);

            Assert.IsFalse(_detector.IsSprinting);
        }

        [Test]
        public void DoubleTapInOppositeDirection_IsNotSprinting()
        {
            Hold(0f, 0.05f, 1);
            Hold(0.06f, 0.15f, 0);
            _detector.Tick(0.16f, -1);

            Assert.IsFalse(_detector.IsSprinting);
        }

        [Test]
        public void Reverse_EndsSprint()
        {
            Hold(0f, 0.4f, 1);
            Assume.That(_detector.IsSprinting);

            _detector.Tick(0.42f, -1);

            Assert.IsFalse(_detector.IsSprinting);
        }

        [Test]
        public void Release_EndsSprint()
        {
            Hold(0f, 0.4f, 1);
            Assume.That(_detector.IsSprinting);

            _detector.Tick(0.42f, 0);

            Assert.IsFalse(_detector.IsSprinting);
        }

        [Test]
        public void Reset_RestartsHoldTimerWhileStillHeld()
        {
            Hold(0f, 0.4f, 1);
            _detector.Reset();

            Hold(0.42f, 0.62f, 1);
            Assert.IsFalse(_detector.IsSprinting);

            Hold(0.64f, 0.72f, 1);
            Assert.IsTrue(_detector.IsSprinting);
        }

        [Test]
        public void HoldTime_TracksContinuousHold()
        {
            Hold(0f, 0.2f, 1);

            Assert.AreEqual(0.2f, _detector.HoldTime, 1e-3f);

            _detector.Tick(0.22f, 0);
            Assert.AreEqual(0f, _detector.HoldTime);
        }
    }
}
