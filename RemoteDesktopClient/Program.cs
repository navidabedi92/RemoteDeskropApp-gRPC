using Grpc.Net.Client;
using RemoteDesktopClient;
using System.Drawing;
using System.Drawing.Imaging;


internal class Program
{
    static async Task Main(string[] args)
    {
        var serverAddress = "https://localhost:7000"; // Replace with your server address

        var clientA = new Client(serverAddress);
        await clientA.ConnectToServer("ClientAID", "ClientA");
        clientA.ListenToServerNotifications();


        var clientB = new Client(serverAddress);
        await clientB.ConnectToServer("clientBID", "clientB");
        clientB.ListenToServerNotifications();


        clientA.RequestPermission("clientBID");

        Console.ReadLine();
    }


}