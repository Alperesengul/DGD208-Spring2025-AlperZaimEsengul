using System;
using System.Threading.Tasks;

namespace PetSimulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Pet Simulator";
            var game = new Game();
            await game.Run();
        }
    }
}
