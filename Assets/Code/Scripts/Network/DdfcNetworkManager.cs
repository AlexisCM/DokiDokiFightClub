using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DokiDokiFightClub
{
    [AddComponentMenu("")]
    public class DdfcNetworkManager : NetworkManager
    {
        [Header("DDFC Settings")]
        public GameObject InGamePlayerPrefab; // Prefab to load when player spawns in game scene

        public int MatchInstances = 2; // Number of simultaneous match instances allowed

        [Scene]
        public string GameScene; // Name of game scene

        [Scene]
        public string UiScene; // Name of scene which holds UI

        public readonly List<GameManager> GameManagers = new();

        // This is set true after server loads all subscene instances
        bool _subscenesLoaded;

        // subscenes are added to this list as they're loaded
        readonly List<Scene> _subScenes = new();

        /// <summary>
        /// Called by the MatchMaker when two players are ready to be put into a match.
        /// </summary>
        /// <param name="matchPlayers"></param>
        public void AddPlayersToMatchScene(Match match)
        {
            foreach (var player in match.QueuedPlayers)
            {
                StartCoroutine(OnAddPlayersToMatch(match.MatchId, player.SpawnIndex, player.connectionToClient));
            }

            StartCoroutine(OnAllMatchPlayersReady(match.MatchId));
        }

        /// <summary>
        /// Replace prefab corresponding to the player's connection.
        /// </summary>
        /// <param name="conn"></param>
        public void ReplacePlayerPrefab(int spawnIndex, NetworkConnectionToClient conn)
        {
            // Cache a reference to the current player object
            GameObject oldPlayer = conn.identity.gameObject;
            Transform spawnPoint = startPositions[spawnIndex];
            GameObject newPlayer = Instantiate(InGamePlayerPrefab, spawnPoint.position, spawnPoint.localRotation);

            // Instantiate the new player object and broadcast to clients
            // Include true for keepAuthority paramater to prevent ownership change
            NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);

            // Remove the previous player object that's now been replaced
            // Delay is required to allow replacement to complete.
            Destroy(oldPlayer, 0.1f);
        }

        public IEnumerator RegisterGameManager()
        {
            yield return null;
        }

        IEnumerator OnAllMatchPlayersReady(int matchId)
        {
            var match = MatchMaker.Instance.Matches[matchId];
            while (!match.CanStart)
                yield return null;

            match.GameManager.StartMatch(match.GetPlayerObjects());
        }

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }

        /// <summary>
        /// Delay adding the player by a frame until the additive UI scene is loaded.
        /// Begin matchmaking once player finishes connecting.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // wait for server to async load all subscenes for game instances
            while (!_subscenesLoaded)
                yield return null;

            // Send Scene message to client to additively load the game scene
            conn.Send(new SceneMessage { sceneName = UiScene, sceneOperation = SceneOperation.LoadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            base.OnServerAddPlayer(conn);
            MatchMaker.Instance.AddPlayerToQueue(conn);

        }

        /// <summary>
        /// Additively load the game scene and move the player's prefab to spawn within it.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerator OnAddPlayersToMatch(int matchId, int spawnIndex, NetworkConnectionToClient conn)
        {
            // wait for server to async load all subscenes for game instances
            while (!_subscenesLoaded)
                yield return null;

            ReplacePlayerPrefab(spawnIndex, conn);

            // Send Scene message to client to additively load the game scene
            conn.Send(new SceneMessage { sceneName = GameScene, sceneOperation = SceneOperation.LoadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            conn.Send(new SceneMessage { sceneName = UiScene, sceneOperation = SceneOperation.UnloadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            Player player = conn.identity.GetComponent<Player>();
            player.PlayerId = spawnIndex;
            player.MatchId = matchId;
            //player.MatchMgrInstance = _matches[matchId].GameManager;

            MatchMaker.Instance.Matches[matchId].AddMatchPlayer(player.gameObject);

            // Do this only on server, not on clients
            // This is what allows the NetworkSceneChecker on player and scene objects
            // to isolate matches per scene instance on server.
            if (_subScenes.Count > 0)
                SceneManager.MoveGameObjectToScene(conn.identity.gameObject, _subScenes[matchId]);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            MatchMaker.Instance.RemovePlayerFromQueue(conn.identity.GetComponent<PlayerQueueIdentity>());
            base.OnServerDisconnect(conn);
        }

        #endregion

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            // Initialize GameManagers
            for (int i = 0; i < MatchInstances; ++i)
                GameManagers.Add(null);

            StartCoroutine(ServerLoadSubScenes());
        }

        // We're additively loading scenes, so GetSceneAt(0) will return the main "container" scene,
        // therefore we start the index at one and loop through instances value inclusively.
        // If instances is zero, the loop is bypassed entirely.
        IEnumerator ServerLoadSubScenes()
        {
            // Wait for scene to properly transition from OfflineScene to QueueScene
            yield return new WaitForEndOfFrame();

            for (int index = 1; index <= MatchInstances; index++)
            {
                yield return SceneManager.LoadSceneAsync(GameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

                Scene newScene = SceneManager.GetSceneAt(index);
                _subScenes.Add(newScene);
                // Spawn interactable objects here; ie., doors, powerups, etc.
            }

            MatchSetup();
            _subscenesLoaded = true;
        }

        /// <summary>
        /// Load preliminary information for each match.
        /// Set each match's id and corresponding GameManager.
        /// </summary>
        void MatchSetup()
        {
            // Loop through subscenes
            Debug.Log($"number of subscenes: {_subScenes.Count}");

            for (var i = 0; i < _subScenes.Count; ++i)
            {
                var rootObjects = _subScenes[i].GetRootGameObjects();
                // loop through each subscene's root objects to find the game manager
                for (var j = 0; j < rootObjects.Length; ++j)
                {
                    if (rootObjects[j] != null && rootObjects[j].name == "GameManager")
                    {
                        GameManagers[i] = rootObjects[j].GetComponent<GameManager>();
                        GameManagers[i].MatchId = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            NetworkServer.SendToAll(new SceneMessage { sceneName = GameScene, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
        }

        // Unload the subScenes and unused assets and clear the subScenes list.
        IEnumerator ServerUnloadSubScenes()
        {
            for (int index = 0; index < _subScenes.Count; index++)
                yield return SceneManager.UnloadSceneAsync(_subScenes[index]);

            _subScenes.Clear();
            _subscenesLoaded = false;

            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            // make sure we're not in host mode
            if (mode == NetworkManagerMode.ClientOnly)
            {
                StartCoroutine(ClientUnloadSubScenes());
            }
        }

        // Unload all but the active scene, which is the "container" scene
        IEnumerator ClientUnloadSubScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
            }
        }

        #endregion
    }
}
