using ConceptDemo.Player;
using NUnit.Framework;

namespace ConceptDemo.Tests
{
    public class ScriptedPlayerInputTests
    {
        private ScriptedPlayerInput _input;

        [SetUp]
        public void SetUp()
        {
            _input = new ScriptedPlayerInput();
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void MoveDirection_IsReturnedEveryRead(int direction)
        {
            _input.MoveDirection = direction;

            Assert.AreEqual(direction, _input.Read().MoveDirection);
            Assert.AreEqual(direction, _input.Read().MoveDirection);
        }

        [Test]
        public void MoveDirection_IsClampedToSign()
        {
            _input.MoveDirection = 5;

            Assert.AreEqual(1, _input.Read().MoveDirection);
        }

        [Test]
        public void PressJump_IsReportedOnce()
        {
            _input.PressJump();

            Assert.IsTrue(_input.Read().JumpPressed);
            Assert.IsFalse(_input.Read().JumpPressed);
        }

        [Test]
        public void PressRoll_IsReportedOnce()
        {
            _input.PressRoll();

            Assert.IsTrue(_input.Read().RollPressed);
            Assert.IsFalse(_input.Read().RollPressed);
        }

        [Test]
        public void PressAttack_IsReportedOnce()
        {
            _input.PressAttack();

            Assert.IsTrue(_input.Read().AttackPressed);
            Assert.IsFalse(_input.Read().AttackPressed);
        }

        [Test]
        public void NoPress_ReportsNothing()
        {
            PlayerInputFrame frame = _input.Read();

            Assert.IsFalse(frame.JumpPressed);
            Assert.IsFalse(frame.RollPressed);
            Assert.IsFalse(frame.AttackPressed);
        }
    }
}
