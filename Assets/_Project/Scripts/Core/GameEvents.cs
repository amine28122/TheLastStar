using System;
using UnityEngine;

namespace TheLastStar.Core
{
    /// <summary>
    /// Static Event Bus for decoupled game events.
    /// </summary>
    public static class GameEvents
    {
        // Player & Starlight Events
        public static event Action<Vector3, float> OnLightPulseTriggered;
        public static event Action<int, int> OnStarlightMotesChanged; // current, max
        public static event Action<int, int> OnPlayerHealthChanged;

        // Exploration & Collectibles
        public static event Action<int, string> OnStarShardCollected; // shardIndex, regionName
        public static event Action<string> OnMemoryRecovered; // memoryTitle

        // Dialogue & Quests
        public static event Action<string> OnDialogueStarted; // speakerName
        public static event Action OnDialogueEnded;
        public static event Action<string, string> OnQuestUpdated; // questId, objectiveText

        public static void TriggerLightPulse(Vector3 position, float radius)
        {
            OnLightPulseTriggered?.Invoke(position, radius);
        }

        public static void UpdateStarlightMotes(int current, int max)
        {
            OnStarlightMotesChanged?.Invoke(current, max);
        }

        public static void UpdatePlayerHealth(int current, int max)
        {
            OnPlayerHealthChanged?.Invoke(current, max);
        }

        public static void CollectStarShard(int shardIndex, string regionName)
        {
            OnStarShardCollected?.Invoke(shardIndex, regionName);
        }

        public static void RecoverMemory(string memoryTitle)
        {
            OnMemoryRecovered?.Invoke(memoryTitle);
        }

        public static void StartDialogue(string speakerName)
        {
            OnDialogueStarted?.Invoke(speakerName);
        }

        public static void EndDialogue()
        {
            OnDialogueEnded?.Invoke();
        }

        public static void UpdateQuest(string questId, string objectiveText)
        {
            OnQuestUpdated?.Invoke(questId, objectiveText);
        }
    }
}
