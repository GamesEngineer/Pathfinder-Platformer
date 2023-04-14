using UnityEngine;

namespace GameU
{
    public class MovingPlatform : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.interpolation = RigidbodyInterpolation.Interpolate; // IMPORTANT to reduce jitter of Player
        }
    }
}
