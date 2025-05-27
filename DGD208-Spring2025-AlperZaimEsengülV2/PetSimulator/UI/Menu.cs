using System;
using System.Collections.Generic;

namespace PetSimulator.UI
{
    public class Menu
    {
        private List<string> options;
        private string title;

        public Menu(string title, List<string> options)
        {
            this.title = title;
            this.options = options;
        }

        public int Display()
        {
            Console.Clear();
            Console.WriteLine($"\n{title}\n");
            
            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }

            Console.Write("\nEnter your choice: ");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= options.Count)
                {
                    return choice;
                }
                Console.Write("Invalid choice. Please try again: ");
            }
        }
    }
} 