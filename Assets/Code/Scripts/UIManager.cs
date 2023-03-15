using UnityEngine;

namespace DokiDokiFightClub
{
    /// <summary>
    /// Class handles user input on UI elements.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private DdfcNetworkManager _networkManager;

        private void Start()
        {
            _networkManager = FindObjectOfType<DdfcNetworkManager>();
        }

        #region Offline Scene UI
        /// <summary>
        /// Start Network Client.
        /// </summary>
        public void OnPlayButton()
        {
            _networkManager.StartClient();
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
