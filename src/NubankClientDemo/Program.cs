using ConsoleTables;
using NubankClient;
using NubankClient.Model;
using System;
using System.Threading.Tasks;

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
            var result = await nubankClient.Login();
            if (result.NeedsDeviceAuthorization)
            {
                Console.WriteLine("You must authenticate with your phone to be able to access your data.");
                Console.WriteLine("Scan the QRCode below with you Nubank application on the following menu:");
                Console.WriteLine("Nu(Seu Nome) > Perfil > Acesso pelo site");
                Console.WriteLine();
                Console.Write(result.GetQrCodeAsAscii());
                Console.WriteLine();
                Console.ReadKey();

                await nubankClient.AutenticateWithQrCode(result.Code);
            }
            var events = await nubankClient.GetEvents();

            ConsoleTable
                .From<Event>(events)
                .Write(Format.Alternative);

            Console.ReadKey();
        }
    }
}
