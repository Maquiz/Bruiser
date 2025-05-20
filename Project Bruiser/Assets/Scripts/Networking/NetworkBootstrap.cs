using UnityEngine;
using Unity.Netcode;

namespace Bruiser.Networking
{
    /// <summary>
    /// Simple bootstrap script that hosts a session automatically
    /// when running the game in the editor.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (!NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.StartHost();
            }
        }
    }
}
