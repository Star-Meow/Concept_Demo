using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class HorizontalMotionTests
    {
        private const float GroundAcceleration = 40f;
        private const float GroundDeceleration = 50f;
        private const float AirAcceleration = 20f;
        private const float AirDeceleration = 2f;
        private const float WalkSpeed = 6f;
        private const float SprintSpeed = 10f;
        private const float Step = 0.02f;

        private HorizontalMotion _motion;

        [SetUp]
        public void SetUp()
        {
            _motion = new HorizontalMotion(GroundAcceleration, GroundDeceleration, AirAcceleration, AirDeceleration);
        }

        /// <summary>重複 Step 直到速度等於 target，回傳花費的時間；超過 maxTime 則回傳 float.MaxValue。</summary>
        private float TimeToReach(ref float velocity, int input, float maxSpeed, bool grounded, float target, float maxTime = 5f)
        {
            float time = 0f;
            while (velocity != target)
            {
                velocity = _motion.Step(velocity, input, maxSpeed, grounded, Step);
                time += Step;
                if (time > maxTime)
                {
                    return float.MaxValue;
                }
            }

            return time;
        }

        [Test]
        public void Ground_Accelerates_ToWalkSpeed_WithGroundAcceleration()
        {
            float velocity = 0f;

            float time = TimeToReach(ref velocity, 1, WalkSpeed, true, WalkSpeed);

            Assert.AreEqual(WalkSpeed / GroundAcceleration, time, Step);
        }

        [Test]
        public void Ground_NeverOvershootsTarget()
        {
            float velocity = 0f;
            for (int i = 0; i < 100; i++)
            {
                velocity = _motion.Step(velocity, 1, WalkSpeed, true, Step);
                Assert.LessOrEqual(velocity, WalkSpeed);
            }

            Assert.AreEqual(WalkSpeed, velocity);
        }

        [Test]
        public void Ground_NoInput_DeceleratesToZero_WithGroundDeceleration()
        {
            float velocity = WalkSpeed;

            float time = TimeToReach(ref velocity, 0, WalkSpeed, true, 0f);

            Assert.AreEqual(WalkSpeed / GroundDeceleration, time, Step);
        }

        [Test]
        public void Ground_Reverse_BrakesWithDeceleration_ThenAccelerates()
        {
            float velocity = WalkSpeed;

            float brakeTime = TimeToReach(ref velocity, -1, WalkSpeed, true, 0f);
            float accelerateTime = TimeToReach(ref velocity, -1, WalkSpeed, true, -WalkSpeed);

            Assert.AreEqual(WalkSpeed / GroundDeceleration, brakeTime, Step);
            Assert.AreEqual(WalkSpeed / GroundAcceleration, accelerateTime, Step);
        }

        [Test]
        public void Ground_SprintToWalk_SlowsWithDeceleration()
        {
            float velocity = SprintSpeed;

            float time = TimeToReach(ref velocity, 1, WalkSpeed, true, WalkSpeed);

            Assert.AreEqual((SprintSpeed - WalkSpeed) / GroundDeceleration, time, Step);
        }

        [Test]
        public void Air_NoInput_KeepsMostMomentum()
        {
            float velocity = WalkSpeed;

            velocity = _motion.Step(velocity, 0, WalkSpeed, false, 0.5f);

            Assert.AreEqual(WalkSpeed - AirDeceleration * 0.5f, velocity, 1e-4f);
        }

        [Test]
        public void Air_Reverse_UsesAirAcceleration()
        {
            float velocity = WalkSpeed;

            float time = TimeToReach(ref velocity, -1, WalkSpeed, false, -WalkSpeed);

            Assert.AreEqual(2f * WalkSpeed / AirAcceleration, time, Step);
        }

        [Test]
        public void Facing_OnGround_FollowsInputImmediately()
        {
            Assert.AreEqual(-1, HorizontalMotion.ResolveFacing(1, -1, WalkSpeed, true));
        }

        [Test]
        public void Facing_NoInput_Unchanged()
        {
            Assert.AreEqual(1, HorizontalMotion.ResolveFacing(1, 0, -WalkSpeed, false));
        }

        [Test]
        public void Facing_InAir_FlipsOnlyAfterVelocityCrossesZero()
        {
            int facing = 1;
            float velocity = WalkSpeed;
            bool flippedBeforeCross = false;

            for (int i = 0; i < 100 && velocity > -WalkSpeed; i++)
            {
                velocity = _motion.Step(velocity, -1, WalkSpeed, false, Step);
                facing = HorizontalMotion.ResolveFacing(facing, -1, velocity, false);
                if (velocity >= 0f && facing == -1)
                {
                    flippedBeforeCross = true;
                }
            }

            Assert.IsFalse(flippedBeforeCross);
            Assert.AreEqual(-1, facing);
        }
    }
}
