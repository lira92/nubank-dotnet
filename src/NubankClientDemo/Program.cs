using ConsoleTables;
using NubankClient;
using NubankClient.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NubankClientDemo
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Nubank Client");
            Console.WriteLine("Please, type your login (CPF):");
            var login = Console.ReadLine().Trim();
            Console.WriteLine("Type your password:");
            var password = Console.ReadLine().Trim();
            var nubankClient = new Nubank(login, password);
            var result = await nubankClient.LoginAsync();
            if (result.NeedsDeviceAuthorization)
            {
                Console.WriteLine("You must authenticate with your phone to be able to access your data.");
                Console.WriteLine("Scan the QRCode below with you Nubank application on the following menu:");
                Console.WriteLine("Nu(Seu Nome) > Perfil > Acesso pelo site");
                Console.WriteLine();
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "qrcodes");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var filename = Path.Combine(directory, DateTime.Now.ToString("yyyyMMddHHmm") + ".jpg");
                result
                    .GetQrCodeAsBitmap()
                    .Save(filename);

                Console.WriteLine($"Open the file '{filename}' and use your phone to scan and after this press any key to continue...");
                Console.ReadKey();

                await nubankClient.AutenticateWithQrCodeAsync(result.Code);
            }
            var events = await nubankClient.GetEventsAsync();

            ConsoleTable
                .From<Event>(events)
                .Write(Format.Alternative);

            Console.ReadKey();
        }
    }
}
