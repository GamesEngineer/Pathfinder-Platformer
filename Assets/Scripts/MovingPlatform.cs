using UnityEngine;

namespace GameU
{
    public class MovingPlatform : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public Vector3 Velocity => Body.velocity;
    }
}
