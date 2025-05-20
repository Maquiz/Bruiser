using UnityEngine;
using Unity.Netcode;

namespace Bruiser.Animals
{
    /// <summary>
    /// Base class for all animal controllers. Provides simple movement logic
    /// that can be extended by specific animals.
    /// </summary>
    public abstract class AnimalBase : NetworkBehaviour
    {
        [SerializeField]
        protected float moveSpeed = 3f;

        /// <summary>
        /// Default movement implementation that moves the object locally.
        /// </summary>
        /// <param name="direction">Normalized movement direction.</param>
        public virtual void Move(Vector3 direction)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
