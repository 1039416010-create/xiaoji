using System.Collections;
using GroundChickenKing.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GroundChickenKing.Tests.PlayMode
{
    public sealed class AspectRatioControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator AspectRatioController_OnAwake_AssignsValidViewport()
        {
            var gameObject = new GameObject("Camera", typeof(Camera), typeof(AspectRatioController));
            yield return null;
            var viewport = gameObject.GetComponent<Camera>().rect;
            Assert.That(viewport.width, Is.GreaterThan(0f));
            Assert.That(viewport.height, Is.GreaterThan(0f));
            Object.Destroy(gameObject);
        }
    }
}
