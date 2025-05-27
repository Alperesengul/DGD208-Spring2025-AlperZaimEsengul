using System;
using PetSimulator.Enums;

namespace PetSimulator.Models
{
    public class SurvivalQuest
    {
        public string Description { get; private set; }
        public int TargetSeconds { get; private set; }
        public int CurrentSeconds { get; private set; }
        public PetType? RequiredPetType { get; private set; }
        public bool IsCompleted => CurrentSeconds >= TargetSeconds;
        public int Reward { get; private set; }

        public SurvivalQuest(int targetSeconds, PetType? requiredPetType = null, int reward = 0)
        {
            TargetSeconds = targetSeconds;
            CurrentSeconds = 0;
            RequiredPetType = requiredPetType;
            Reward = reward > 0 ? reward : targetSeconds; // Use provided reward or default to 1 point per second
            Description = GenerateDescription();
        }

        private string GenerateDescription()
        {
            string petTypeStr = RequiredPetType.HasValue ? $" with a {RequiredPetType.Value}" : "";
            return $"Keep your pet{petTypeStr} alive for {TargetSeconds} seconds";
        }

        public void AddSecond()
        {
            if (!IsCompleted)
            {
                CurrentSeconds++;
            }
        }

        public string GetProgress()
        {
            return $"{CurrentSeconds}/{TargetSeconds} seconds";
        }
    }
} 