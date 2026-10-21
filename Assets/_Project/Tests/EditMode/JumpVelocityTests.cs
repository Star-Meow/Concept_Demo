using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class JumpVelocityTests
    {
        private const float Gravity = 9.81f * 3f;
        private const float FixedStep = 0.02f;

        /// <summary>模擬物理引擎的固定步長（先更新速度、再更新位置），回傳最高點。</summary>
        private static float SimulateApex(float velocity, float gravity, float deltaTime)
        {
            float height = 0f;
            float apex = 0f;
            for (int i = 0; i < 10000 && velocity > 0f; i++)
            {
                velocity -= gravity * deltaTime;
                height += velocity * deltaTime;
                if (height > apex)
                {
                    apex = height;
                }
            }

            return apex;
        }

        [TestCase(1f)]
        [TestCase(2.7f)]
        [TestCase(3f)]
        public void SimulatedApex_EqualsJumpHeight(float jumpHeight)
        {
            float velocity = PlayerController.GetJumpVelocity(jumpHeight, Gravity, FixedStep);

            float apex = SimulateApex(velocity, Gravity, FixedStep);

            Assert.AreEqual(jumpHeight, apex, 0.03f);
        }

        [Test]
        public void ZeroStep_MatchesContinuousFormula()
        {
            float velocity = PlayerController.GetJumpVelocity(3f, Gravity, 0f);

            Assert.AreEqual(3f, velocity * velocity / (2f * Gravity), 1e-3f);
        }

        [Test]
        public void ZeroOrNegativeHeight_GivesZeroVelocity()
        {
            Assert.AreEqual(0f, PlayerController.GetJumpVelocity(0f, Gravity, FixedStep));
            Assert.AreEqual(0f, PlayerController.GetJumpVelocity(-1f, Gravity, FixedStep));
        }
    }
}
