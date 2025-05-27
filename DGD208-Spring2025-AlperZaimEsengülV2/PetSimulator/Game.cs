using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using PetSimulator.Enums;
using PetSimulator.Models;
using PetSimulator.UI;

namespace PetSimulator
{
    public class Game
    {
        private List<Pet> pets;
        private List<Item> items;
        private Menu mainMenu;
        private bool isRunning;
        private Dictionary<Pet, int> statChangeCounters = new Dictionary<Pet, int>();
        private List<SurvivalQuest> activeQuests;
        private Random random;
        private int playerPoints;
        private CancellationTokenSource questCancellation;

        public Game()
        {
            pets = new List<Pet>();
            items = new List<Item>();
            activeQuests = new List<SurvivalQuest>();
            random = new Random();
            playerPoints = 0;
            InitializeItems();
            InitializeMainMenu();
            isRunning = true;
            StartQuestSystem();
        }

        private void InitializeItems()
        {
            items = new List<Item>
            {
                new Item("Dog Food", ItemType.Food, 20, new[] { PetType.Dog }, new[] { PetStat.Hunger }),
                new Item("Cat Food", ItemType.Food, 20, new[] { PetType.Cat }, new[] { PetStat.Hunger }),
                new Item("Bird Seed", ItemType.Food, 20, new[] { PetType.Bird }, new[] { PetStat.Hunger }),
                new Item("Fish Food", ItemType.Food, 20, new[] { PetType.Fish }, new[] { PetStat.Hunger }),
                new Item("Rabbit Food", ItemType.Food, 20, new[] { PetType.Rabbit }, new[] { PetStat.Hunger }),
                new Item("Ball", ItemType.Toy, 15, new[] { PetType.Dog, PetType.Cat }, new[] { PetStat.Fun }),
                new Item("Cat Bed", ItemType.Bed, 25, new[] { PetType.Cat }, new[] { PetStat.Sleep }),
                new Item("Medicine", ItemType.Medicine, 30, 
                    new[] { PetType.Dog, PetType.Cat, PetType.Bird, PetType.Fish, PetType.Rabbit },
                    new[] { PetStat.Hunger, PetStat.Sleep, PetStat.Fun })
            };
        }

        private void InitializeMainMenu()
        {
            var options = new List<string>
            {
                "Adopt a Pet",
                "View Pets",
                "Use Item",
                "View Quests",
                "View Points",
                "Display Creator Info",
                "Exit"
            };
            mainMenu = new Menu("Pet Simulator", options);
        }

        private void StartQuestSystem()
        {
            questCancellation = new CancellationTokenSource();
            _ = RunQuestSystem(questCancellation.Token);
        }

        private async Task RunQuestSystem(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var quest in activeQuests.ToList())
                {
                    quest.AddSecond();
                }
                CheckQuests();
                await Task.Delay(1000, cancellationToken);
            }
        }

        private void GenerateQuestForPet(PetType petType)
        {
            var quest = new SurvivalQuest(20, petType, 20); // 20 seconds, specific pet type, 20 points reward
            activeQuests.Add(quest);
        }

        private void CheckQuests()
        {
            var completedQuests = activeQuests.Where(q => q.IsCompleted).ToList();
            foreach (var quest in completedQuests)
            {
                Console.WriteLine($"\nQuest Completed: {quest.Description}");
                Console.WriteLine($"Reward: {quest.Reward} points!");
                playerPoints += quest.Reward;
                activeQuests.Remove(quest);
            }
        }

        public async Task Run()
        {
            try
            {
                while (isRunning)
                {
                    int choice = mainMenu.Display();
                    await HandleMenuChoice(choice);
                }
            }
            finally
            {
                questCancellation?.Cancel();
            }
        }

        private async Task HandleMenuChoice(int choice)
        {
            switch (choice)
            {
                case 1:
                    await AdoptPet();
                    break;
                case 2:
                    ViewPets();
                    break;
                case 3:
                    UseItem();
                    break;
                case 4:
                    ViewQuests();
                    break;
                case 5:
                    ViewPoints();
                    break;
                case 6:
                    DisplayCreatorInfo();
                    break;
                case 7:
                    isRunning = false;
                    break;
            }
        }

        private async Task AdoptPet()
        {
            var petTypes = Enum.GetValues(typeof(PetType)).Cast<PetType>().ToList();
            var options = petTypes.Select(p => p.ToString()).ToList();
            var menu = new Menu("Choose Pet Type", options);
            
            int choice = menu.Display();
            var selectedType = petTypes[choice - 1];

            Console.Write("Enter pet name: ");
            string name = Console.ReadLine();

            var pet = new Pet(name, selectedType);
            pet.PetDied += OnPetDied;
            pet.StatChanged += OnPetStatChanged;
            pets.Add(pet);

            // Generate quest for the adopted pet
            GenerateQuestForPet(selectedType);

            _ = pet.StartStatDecrease(); // Start the stat decrease task
            Console.WriteLine($"\n{name} has been adopted!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ViewPets()
        {
            if (!pets.Any())
            {
                Console.WriteLine("\nNo pets adopted yet!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("\nYour Pets:\n");
            foreach (var pet in pets)
            {
                Console.WriteLine($"{pet.Name} ({pet.Type})");
                foreach (var stat in pet.Stats)
                {
                    Console.WriteLine($"  {stat.Key}: {stat.Value}");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void UseItem()
        {
            if (!pets.Any())
            {
                Console.WriteLine("\nNo pets to use items on!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var petOptions = pets.Select(p => p.Name).ToList();
            var petMenu = new Menu("Select Pet", petOptions);
            int petChoice = petMenu.Display();
            var selectedPet = pets[petChoice - 1];

            var availableItems = items.Where(i => i.CompatiblePets.Contains(selectedPet.Type)).ToList();
            var itemOptions = availableItems.Select(i => i.Name).ToList();
            var itemMenu = new Menu("Select Item", itemOptions);
            int itemChoice = itemMenu.Display();
            var selectedItem = availableItems[itemChoice - 1];

            selectedPet.UseItem(selectedItem);
            Console.WriteLine($"\nUsed {selectedItem.Name} on {selectedPet.Name}!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ViewQuests()
        {
            Console.Clear();
            Console.WriteLine("\nActive Quests:\n");
            
            if (!activeQuests.Any())
            {
                Console.WriteLine("No active quests!");
            }
            else
            {
                foreach (var quest in activeQuests)
                {
                    Console.WriteLine($"{quest.Description} - Progress: {quest.GetProgress()}");
                    Console.WriteLine($"Reward: {quest.Reward} points");
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ViewPoints()
        {
            Console.Clear();
            Console.WriteLine($"\nYour Points: {playerPoints}");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void DisplayCreatorInfo()
        {
            Console.Clear();
            Console.WriteLine("\nCreator Information:");
            Console.WriteLine("Name: Alper Zaim Esengül");
            Console.WriteLine("Student Number: 225040079");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void OnPetDied(object sender, EventArgs e)
        {
            var pet = (Pet)sender;
            pets.Remove(pet);
            Console.WriteLine($"\n{pet.Name} has died!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void OnPetStatChanged(object sender, PetStat stat)
        {
            var pet = (Pet)sender;
            
            if (!statChangeCounters.ContainsKey(pet))
            {
                statChangeCounters[pet] = 0;
            }
            
            statChangeCounters[pet]++;
            
            if (statChangeCounters[pet] % 20 == 0)
            {
                Console.WriteLine($"{pet.Name}'s {stat} is now {pet.Stats[stat]}");
            }
        }
    }
} 