using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class RollMotionTests
    {
        private const float FixedStep = 0.02f;
        private const float BodyWidth = 1f;
        private const float DistanceInBodyWidths = 2f;
        private const float Duration = 0.3f;

        private RollMotion _roll;

        [SetUp]
        public void SetUp()
        {
            _roll = new RollMotion();
        }

        /// <summary>執行到翻滾結束，回傳總位移與總時間。</summary>
        private (float displacement, float time) RunToEnd(float deltaTime)
        {
            float displacement = 0f;
            float time = 0f;
            for (int i = 0; i < 1000 && _roll.IsActive; i++)
            {
                displacement += _roll.Tick(deltaTime) * deltaTime;
                time += deltaTime;
            }

            return (displacement, time);
        }

        [TestCase(0.02f)]
        [TestCase(0.0166667f)]
        [TestCase(0.033f)]
        public void Displacement_EqualsBodyWidthsTimesColliderWidth(float deltaTime)
        {
            _roll.Begin(1, DistanceInBodyWidths * BodyWidth, Duration);

            (float displacement, _) = RunToEnd(deltaTime);

            Assert.AreEqual(DistanceInBodyWidths * BodyWidth, displacement, 0.01f);
        }

        [Test]
        public void Displacement_ScalesWithColliderWidth()
        {
            _roll.Begin(1, DistanceInBodyWidths * 1.5f, Duration);

            (float displacement, _) = RunToEnd(FixedStep);

            Assert.AreEqual(3f, displacement, 0.01f);
        }

        [TestCase(0.3f)]
        [TestCase(0.5f)]
        [TestCase(2f)]
        public void TotalTime_IsAtMostMaxDuration(float requestedDuration)
        {
            _roll.Begin(1, 2f, requestedDuration);

            (_, float time) = RunToEnd(FixedStep);

            Assert.LessOrEqual(time, RollMotion.MaxDuration + 1e-4f);
        }

        [Test]
        public void TotalTime_MatchesDuration()
        {
            _roll.Begin(1, 2f, Duration);

            (_, float time) = RunToEnd(FixedStep);

            Assert.AreEqual(Duration, time, 1e-4f);
        }

        [TestCase(1)]
        [TestCase(-1)]
        public void Velocity_FollowsFacing(int facing)
        {
            _roll.Begin(facing, 2f, Duration);

            float velocity = _roll.Tick(FixedStep);
            (float rest, _) = RunToEnd(FixedStep);

            Assert.AreEqual(facing, System.Math.Sign(velocity));
            Assert.AreEqual(2f * facing, velocity * FixedStep + rest, 0.01f);
        }

        [Test]
        public void AfterEnd_ReturnsZeroAndInactive()
        {
            _roll.Begin(1, 2f, Duration);
            RunToEnd(FixedStep);

            Assert.IsFalse(_roll.IsActive);
            Assert.AreEqual(0f, _roll.Tick(FixedStep));
        }

        [Test]
        public void Cancel_StopsMotion()
        {
            _roll.Begin(1, 2f, Duration);
            _roll.Tick(FixedStep);

            _roll.Cancel();

            Assert.IsFalse(_roll.IsActive);
            Assert.AreEqual(0f, _roll.Tick(FixedStep));
        }
    }
}
