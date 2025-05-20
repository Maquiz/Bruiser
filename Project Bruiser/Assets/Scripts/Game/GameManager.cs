using UnityEngine;
using Unity.Netcode;

namespace Bruiser.Game
{
    /// <summary>
    /// Singleton game manager responsible for high level state.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Server side initialization can go here
            }
        }
    }
}
