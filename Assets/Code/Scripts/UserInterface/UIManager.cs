using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    /// <summary> Class handles user input on UI elements. </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject FitbitNotLinkedUi;

        private DdfcNetworkManager _networkManager;

        private void Start()
        {
            _networkManager = NetworkManager.singleton as DdfcNetworkManager;
        }

        #region Offline Scene UI
        /// <summary> Start Network Client. </summary>
        public void OnPlayButton()
        {
            if (!PlayerPrefs.HasKey(FitbitApi.FITBIT_REFRESH_TOKEN_KEY) || 
                !PlayerPrefs.HasKey(FitbitApi.FITBIT_ACCESS_TOKEN_KEY))
            {
                FitbitNotLinkedUi.SetActive(true);
                return;
            }

            _networkManager.StartClient();
        }

        public void OnQuitButton()
        {
            Application.Quit();
        }

        public void OnLinkFitbitUiOkayButton()
        {
            FitbitNotLinkedUi.SetActive(false);
        }
        #endregion

        #region Room Scene UI
        /// <summary>
        /// Returns Client to OfflineScene.
        /// </summary>
        public void OnLeaveButton()
        {
            _networkManager.StopClient();
            // TODO: Handle Matchmaking/remove player from queue.
        }
        #endregion
    }
}
