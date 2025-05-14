using PetSimulator.Enums;

namespace PetSimulator.Models
{
    public class Item
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public int Value { get; set; }
        public PetType[] CompatiblePets { get; set; }
        public PetStat[] AffectedStats { get; set; }

        public Item(string name, ItemType type, int value, PetType[] compatiblePets, PetStat[] affectedStats)
        {
            Name = name;
            Type = type;
            Value = value;
            CompatiblePets = compatiblePets;
            AffectedStats = affectedStats;
        }
    }
} 