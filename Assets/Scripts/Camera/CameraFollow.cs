using UnityEngine;

namespace CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform target;
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        private void Start()
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 desiredPosition = target.position + offset;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
            }
        }
    }
} 