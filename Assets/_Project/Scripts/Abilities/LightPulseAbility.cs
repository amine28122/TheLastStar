using System.Collections;
using UnityEngine;
using TheLastStar.Core;
using TheLastStar.Interaction;

namespace TheLastStar.Abilities
{
    public class LightPulseAbility : MonoBehaviour
    {
        [Header("Pulse Settings")]
        [SerializeField] private float pulseRadius = 7.5f;
        [SerializeField] private float pulseCooldown = 1.8f;
        [SerializeField] private float pulseIntensity = 1.0f;
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Header("Visual & Light Feedback")]
        [SerializeField] private Light pulseLight;
        [SerializeField] private ParticleSystem pulseParticles;
        [SerializeField] private float flashDuration = 0.35f;
        [SerializeField] private float peakLightIntensity = 6.0f;

        private float lastPulseTime = -10f;
        private bool isPulsing = false;

        public bool CanPulse => Time.time >= lastPulseTime + pulseCooldown && !isPulsing;
        public float CooldownProgress => Mathf.Clamp01((Time.time - lastPulseTime) / pulseCooldown);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
            {
                TryTriggerPulse();
            }
        }

        public bool TryTriggerPulse()
        {
            if (!CanPulse) return false;

            lastPulseTime = Time.time;
            StartCoroutine(ExecutePulseRoutine());
            return true;
        }

        private IEnumerator ExecutePulseRoutine()
        {
            isPulsing = true;
            Vector3 origin = transform.position + Vector3.up * 0.8f;

            // Broadcast globally
            GameEvents.TriggerLightPulse(origin, pulseRadius);

            // Play particles
            if (pulseParticles != null)
            {
                pulseParticles.Play();
            }

            // Animate Light flash
            if (pulseLight != null)
            {
                pulseLight.enabled = true;
                float elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / flashDuration;
                    pulseLight.intensity = Mathf.Lerp(peakLightIntensity, 0f, t);
                    pulseLight.range = Mathf.Lerp(1f, pulseRadius, t);
                    yield return null;
                }
                pulseLight.enabled = false;
            }

            // Detect and illuminate targets
            Collider[] hits = Physics.OverlapSphere(origin, pulseRadius, affectedLayers);
            foreach (var hit in hits)
            {
                var lightSensitive = hit.GetComponent<ILightSensitive>() ?? hit.GetComponentInParent<ILightSensitive>();
                if (lightSensitive != null)
                {
                    lightSensitive.OnHitByLightPulse(origin, pulseIntensity);
                }
            }

            isPulsing = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.8f, pulseRadius);
        }
    }
}
