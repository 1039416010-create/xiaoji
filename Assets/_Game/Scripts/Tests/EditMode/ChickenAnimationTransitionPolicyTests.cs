using GroundChickenKing.Chickens;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class ChickenAnimationTransitionPolicyTests
    {
        [TestCase(ChickenVisualState.Run, ChickenVisualState.Sprint)]
        [TestCase(ChickenVisualState.Sprint, ChickenVisualState.Run)]
        public void ShouldPreservePhase_LocomotionTransition_ReturnsTrue(
            ChickenVisualState from,
            ChickenVisualState to)
        {
            Assert.That(ChickenAnimationTransitionPolicy.ShouldPreservePhase(from, to), Is.True);
            Assert.That(ChickenAnimationTransitionPolicy.GetDuration(from, to), Is.EqualTo(0.1f));
        }

        [TestCase(ChickenVisualState.Run, ChickenVisualState.Fall)]
        [TestCase(ChickenVisualState.Fall, ChickenVisualState.Recover)]
        [TestCase(ChickenVisualState.Recover, ChickenVisualState.Run)]
        [TestCase(ChickenVisualState.Run, ChickenVisualState.Celebrate)]
        public void ShouldPreservePhase_OneShotTransition_ReturnsFalse(
            ChickenVisualState from,
            ChickenVisualState to)
        {
            Assert.That(ChickenAnimationTransitionPolicy.ShouldPreservePhase(from, to), Is.False);
            Assert.That(ChickenAnimationTransitionPolicy.GetDuration(from, to), Is.GreaterThan(0f));
        }

        [Test]
        public void GetDuration_SameState_ReturnsZero()
        {
            Assert.That(
                ChickenAnimationTransitionPolicy.GetDuration(ChickenVisualState.Run, ChickenVisualState.Run),
                Is.Zero);
        }
    }
}
