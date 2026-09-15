using GroundChickenKing.Core;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class DisplayAspectUtilityTests
    {
        [Test]
        public void CalculateViewport_ExactTwoToOne_FillsScreen()
        {
            var viewport = DisplayAspectUtility.CalculateViewport(1920, 960);
            Assert.That(viewport.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(viewport.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(viewport.width, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(viewport.height, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void CalculateViewport_SixteenByNine_AddsHorizontalLetterbox()
        {
            var viewport = DisplayAspectUtility.CalculateViewport(1920, 1080);
            Assert.That(viewport.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(viewport.width, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(viewport.height, Is.EqualTo(8f / 9f).Within(0.0001f));
            Assert.That(viewport.y, Is.EqualTo(1f / 18f).Within(0.0001f));
        }

        [Test]
        public void CalculateViewport_Ultrawide_AddsVerticalPillarbox()
        {
            var viewport = DisplayAspectUtility.CalculateViewport(2560, 1080);
            Assert.That(viewport.height, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(viewport.width, Is.LessThan(1f));
            Assert.That(viewport.x, Is.GreaterThan(0f));
        }

        [Test]
        public void CalculateViewport_InvalidDimensions_ReturnsSafeFullViewport()
        {
            var viewport = DisplayAspectUtility.CalculateViewport(0, 0);
            Assert.That(viewport.width, Is.EqualTo(1f));
            Assert.That(viewport.height, Is.EqualTo(1f));
        }
    }
}
