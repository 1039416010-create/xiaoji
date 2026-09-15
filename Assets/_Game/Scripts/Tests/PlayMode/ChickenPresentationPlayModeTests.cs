using System.Collections;
using GroundChickenKing.Chickens;
using GroundChickenKing.Race;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GroundChickenKing.Tests.PlayMode
{
    public sealed class ChickenPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator ChickenController_AllPlannedBehaviors_EnterAndExitWithoutAnimator()
        {
            foreach (RaceEventType type in System.Enum.GetValues(typeof(RaceEventType)))
            {
                var controller = CreateController();
                controller.Play(CreatePlan(type));
                controller.Advance(2.1f);
                Assert.That(controller.VisualState, Is.EqualTo(ExpectedState(type)), type.ToString());
                controller.Advance(1f);
                Assert.That(controller.VisualState, Is.EqualTo(ChickenVisualState.Run), type.ToString());
                controller.Advance(7f);
                Assert.That(controller.IsPlaying, Is.False);
                Assert.That(controller.VisualState, Is.EqualTo(ChickenVisualState.Lose));
                Object.Destroy(controller.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChickenController_ResetCancelAndForceCompletion_AreSafeAndDeterministic()
        {
            var controller = CreateController(); var plan = CreatePlan(RaceEventType.Trip, true);
            controller.Play(plan); controller.Advance(4f); controller.Cancel();
            Assert.That(controller.IsPlaying, Is.False); Assert.That(controller.VisualState, Is.EqualTo(ChickenVisualState.Idle));
            controller.Play(plan); controller.ResetToStart(); Assert.That(controller.NormalizedProgress, Is.Zero);
            controller.Play(plan); controller.ForceSafeCompletion();
            Assert.That(controller.NormalizedProgress, Is.EqualTo(1f)); Assert.That(controller.VisualState, Is.EqualTo(ChickenVisualState.Celebrate));
            Assert.That(controller.UsesRootMotion, Is.False);
            Object.Destroy(controller.gameObject); yield return null;
        }

        [UnityTest]
        public IEnumerator ChickenController_LargeAndSmallFrameSteps_EndAtSamePlannedPosition()
        {
            var fine = CreateController(); var coarse = CreateController(); var plan = CreatePlan(RaceEventType.TurnAround, true);
            fine.Play(plan); coarse.Play(plan);
            for (var i = 0; i < 100; i++) fine.Advance(0.1f);
            coarse.Advance(10f);
            Assert.That(fine.NormalizedProgress, Is.EqualTo(plan.ExpectedEndProgress).Within(0.0001f));
            Assert.That(coarse.NormalizedProgress, Is.EqualTo(fine.NormalizedProgress).Within(0.0001f));
            Assert.That(fine.VisualState, Is.EqualTo(coarse.VisualState));
            Object.Destroy(fine.gameObject); Object.Destroy(coarse.gameObject); yield return null;
        }

        [UnityTest]
        public IEnumerator ChickenController_OneHundredSequentialPlans_ReusesSingleInstanceWithoutResidue()
        {
            var controller = CreateController();
            var instanceId = controller.GetInstanceID();
            for (var round = 0; round < 100; round++)
            {
                controller.ResetToStart(); controller.Play(CreatePlan((RaceEventType)(round % 6), round % 5 == 0)); controller.Advance(20f);
                Assert.That(controller.IsPlaying, Is.False, $"round={round}");
                Assert.That(controller.NormalizedProgress, Is.InRange(0f, 1f), $"round={round}");
            }
            Assert.That(controller.GetInstanceID(), Is.EqualTo(instanceId));
            Object.Destroy(controller.gameObject); yield return null;
        }

        [UnityTest]
        public IEnumerator ChickenRacePresenter_InterfereEvent_DrivesAdjacentPairedReactionOnly()
        {
            var host = new GameObject("RacePresenter", typeof(ChickenRacePresenter));
            var controllers = new ChickenController[5]; var plans = new ChickenRacePlan[5];
            for (var lane = 0; lane < 5; lane++)
            {
                controllers[lane] = CreateController($"chicken-{lane}", lane);
                plans[lane] = new ChickenRacePlan($"chicken-{lane}", lane, lane == 4, lane == 4 ? 10f : -1f, lane == 4 ? 1f : 0.8f,
                    new[] { new RaceSegment(0f, 10f, 0f, lane == 4 ? 1f : 0.8f, RaceInterpolation.Linear, true) },
                    lane == 0 ? new[] { new RaceEvent(RaceEventType.Interfere, 2f, 3f, 1f, 1) } : new RaceEvent[0]);
            }
            var presenter = host.GetComponent<ChickenRacePresenter>(); presenter.Configure(controllers);
            presenter.BindPlan(new RacePlan("test", "config", 1, new RaceSeed(1), "chicken-4", 10f, plans, "paired-test")); presenter.Play();
            foreach (var controller in controllers) controller.Advance(2.1f); presenter.RefreshPairedInterference();
            Assert.That(controllers[0].VisualState, Is.EqualTo(ChickenVisualState.Interfere));
            Assert.That(controllers[1].VisualState, Is.EqualTo(ChickenVisualState.Interfere));
            Assert.That(controllers[2].VisualState, Is.EqualTo(ChickenVisualState.Run));
            foreach (var controller in controllers) controller.Advance(1f); presenter.RefreshPairedInterference();
            Assert.That(controllers[1].VisualState, Is.EqualTo(ChickenVisualState.Run));
            foreach (var controller in controllers) Object.Destroy(controller.gameObject);
            Object.Destroy(host); yield return null;
        }

        private static ChickenController CreateController()
        {
            return CreateController("test-chicken", 0);
        }

        private static ChickenController CreateController(string chickenId, int lane)
        {
            var root = new GameObject("TestChicken", typeof(RectTransform), typeof(ChickenController));
            var visual = new GameObject("Visual", typeof(RectTransform)); visual.transform.SetParent(root.transform, false);
            var controller = root.GetComponent<ChickenController>();
            controller.Configure(chickenId, lane, root.GetComponent<RectTransform>(), visual.transform, null, -100f, 100f, lane * 10f);
            return controller;
        }

        private static ChickenRacePlan CreatePlan(RaceEventType type, bool champion = false)
        {
            var end = champion ? 1f : 0.82f;
            return new ChickenRacePlan("test-chicken", 0, champion, champion ? 10f : -1f, end,
                new[] { new RaceSegment(0f, 10f, 0f, end, RaceInterpolation.Linear, true) },
                new[] { new RaceEvent(type, 2f, 3f, 1f, type == RaceEventType.Interfere ? 1 : null) });
        }

        private static ChickenVisualState ExpectedState(RaceEventType type) => type switch
        {
            RaceEventType.Sprint => ChickenVisualState.Sprint,
            RaceEventType.Slowdown => ChickenVisualState.Stop,
            RaceEventType.Pause => ChickenVisualState.Stop,
            RaceEventType.Trip => ChickenVisualState.Fall,
            RaceEventType.TurnAround => ChickenVisualState.Turn,
            _ => ChickenVisualState.Interfere,
        };
    }
}
