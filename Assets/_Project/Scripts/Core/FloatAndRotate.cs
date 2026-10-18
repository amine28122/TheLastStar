using UnityEngine;

namespace TheLastStar.Core
{
    public class FloatAndRotate : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 35f;
        [SerializeField] private float floatAmplitude = 0.25f;
        [SerializeField] private float floatFrequency = 1.4f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.localPosition;
        }

        private void Update()
        {
            // Smooth Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Gentle Floating Bobbing
            float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}
