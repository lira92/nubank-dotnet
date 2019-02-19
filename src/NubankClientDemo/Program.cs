using System;
using System.Threading.Tasks;
using ConsoleTables;
using NubankClient;
using NubankClient.Model;

namespace NubankClientDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Nubank Client");
            Console.WriteLine("Please, type your login (CPF):");
            var login = Console.ReadLine().Trim();
            Console.WriteLine("Type your password:");
            var password = Console.ReadLine().Trim();
            var nubankClient = new Nubank(login, password);
            await nubankClient.Login();
            var events = await nubankClient.GetEvents();

            ConsoleTable
                .From<Event>(events)
                .Write(Format.Alternative);

            Console.ReadKey();
        }
    }
}
