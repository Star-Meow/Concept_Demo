using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class AttackTimerTests
    {
        private const float FixedStep = 0.02f;
        private const float Duration = 0.5f;

        private AttackTimer _attack;

        [SetUp]
        public void SetUp()
        {
            _attack = new AttackTimer();
        }

        /// <summary>執行到揮砍結束，回傳總時間。</summary>
        private float RunToEnd(float deltaTime)
        {
            float time = 0f;
            for (int i = 0; i < 1000 && _attack.IsActive; i++)
            {
                _attack.Tick(deltaTime);
                time += deltaTime;
            }

            return time;
        }

        [Test]
        public void NotStarted_IsInactive()
        {
            Assert.IsFalse(_attack.IsActive);
        }

        [TestCase(0.02f)]
        [TestCase(0.0166667f)]
        [TestCase(0.033f)]
        public void LastsForDuration(float deltaTime)
        {
            _attack.Begin(Duration);

            float time = RunToEnd(deltaTime);

            Assert.GreaterOrEqual(time, Duration - 1e-4f);
            Assert.Less(time, Duration + deltaTime);
        }

        [Test]
        public void ActiveUntilDurationReached()
        {
            _attack.Begin(Duration);

            for (int i = 0; i < 24; i++)
            {
                _attack.Tick(FixedStep);
            }

            Assert.IsTrue(_attack.IsActive);
            _attack.Tick(FixedStep);
            Assert.IsFalse(_attack.IsActive);
        }

        [Test]
        public void AfterEnd_StaysInactive()
        {
            _attack.Begin(Duration);
            RunToEnd(FixedStep);

            _attack.Tick(FixedStep);

            Assert.IsFalse(_attack.IsActive);
        }

        [Test]
        public void Cancel_EndsImmediately()
        {
            _attack.Begin(Duration);
            _attack.Tick(FixedStep);

            _attack.Cancel();

            Assert.IsFalse(_attack.IsActive);
        }

        [Test]
        public void Progress_GoesFromZeroToOne()
        {
            _attack.Begin(Duration);
            Assert.AreEqual(0f, _attack.Progress, 1e-5f);

            for (int i = 0; i < 12; i++)
            {
                _attack.Tick(FixedStep);
            }

            Assert.AreEqual(0.48f, _attack.Progress, 1e-4f);

            RunToEnd(FixedStep);
            Assert.AreEqual(1f, _attack.Progress, 1e-5f);
        }

        [Test]
        public void Begin_RestartsTimer()
        {
            _attack.Begin(Duration);
            RunToEnd(FixedStep);

            _attack.Begin(Duration);

            Assert.IsTrue(_attack.IsActive);
            Assert.AreEqual(0f, _attack.Elapsed, 1e-5f);
        }
    }
}
