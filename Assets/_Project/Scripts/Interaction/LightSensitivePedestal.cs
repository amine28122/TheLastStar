using UnityEngine;
using UnityEngine.Events;

namespace TheLastStar.Interaction
{
    public class LightSensitivePedestal : MonoBehaviour, ILightSensitive, IInteractable
    {
        [Header("Visual Feedback")]
        [SerializeField] private MeshRenderer crystalRenderer;
        [SerializeField] private Light crystalLight;
        [SerializeField] private Color illuminatedColor = new Color(0.1f, 0.9f, 1.0f);
        [SerializeField] private float emissionIntensity = 3.5f;

        [Header("State")]
        [SerializeField] private bool startsActivated = false;
        [SerializeField] private bool canBeDeactivated = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onActivated;
        [SerializeField] private UnityEvent onDeactivated;

        private Material crystalMaterial;
        private bool isIlluminated = false;

        public bool IsIlluminated => isIlluminated;

        private void Awake()
        {
            if (crystalRenderer != null)
            {
                crystalMaterial = crystalRenderer.material;
            }

            if (startsActivated)
            {
                SetIlluminated(true);
            }
            else
            {
                SetIlluminated(false);
            }
        }

        public void OnHitByLightPulse(Vector3 pulseOrigin, float intensity)
        {
            if (!isIlluminated)
            {
                SetIlluminated(true);
                onActivated?.Invoke();
            }
            else if (canBeDeactivated)
            {
                SetIlluminated(false);
                onDeactivated?.Invoke();
            }
        }

        private void SetIlluminated(bool active)
        {
            isIlluminated = active;

            if (crystalLight != null)
            {
                crystalLight.enabled = active;
                crystalLight.color = illuminatedColor;
            }

            if (crystalMaterial != null)
            {
                if (active)
                {
                    crystalMaterial.EnableKeyword("_EMISSION");
                    crystalMaterial.SetColor("_EmissionColor", illuminatedColor * emissionIntensity);
                }
                else
                {
                    crystalMaterial.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        public string GetInteractionPrompt()
        {
            return isIlluminated ? "Ancient Starlight Core (Active)" : "Press [Q] near this pedestal to channel Luminescent Pulse";
        }

        public void Interact(GameObject interactor)
        {
            // Inspect prompt
            Debug.Log($"[TheLastStar] Inspected Ancient Pedestal. Illuminated: {isIlluminated}");
        }

        public bool CanInteract() => true;
    }
}
