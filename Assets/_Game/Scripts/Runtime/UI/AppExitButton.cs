using UnityEngine;

namespace GroundChickenKing.UI
{
    public sealed class AppExitButton : MonoBehaviour
    {
        [SerializeField] private ConfirmationDialog _confirmationDialog;
        public void Configure(ConfirmationDialog confirmationDialog) => _confirmationDialog = confirmationDialog;

        public void ExitApplication()
        {
            if (_confirmationDialog != null) { _confirmationDialog.Show(UiTextCatalog.ConfirmExit, ExitConfirmed); return; }
            ExitConfirmed();
        }

        private static void ExitConfirmed()
        {
#if UNITY_EDITOR
            Debug.Log("[Application] Exit requested from the visible UI button.");
#else
            Application.Quit();
#endif
        }
    }
}
