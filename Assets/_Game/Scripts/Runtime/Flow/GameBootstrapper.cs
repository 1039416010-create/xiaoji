using UnityEngine;
using UnityEngine.SceneManagement;

namespace GroundChickenKing.Flow
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private string _mainSceneName = "SCN_Main";

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(_mainSceneName))
                SceneManager.LoadScene(_mainSceneName, LoadSceneMode.Single);
        }
    }
}
