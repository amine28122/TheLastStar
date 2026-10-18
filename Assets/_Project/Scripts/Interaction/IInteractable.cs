using UnityEngine;

namespace TheLastStar.Interaction
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(GameObject interactor);
        bool CanInteract();
    }

    public interface ILightSensitive
    {
        void OnHitByLightPulse(Vector3 pulseOrigin, float intensity);
        bool IsIlluminated { get; }
    }
}
