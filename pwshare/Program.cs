// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Welcome to pwshare!");
Console.WriteLine("Do you want to ");
Console.WriteLine("(R)eceive a secret");
Console.WriteLine("(S)end a secret");
var choice = Console.ReadLine() ?? string.Empty;
Console.Clear();

if(choice.Equals("r", StringComparison.InvariantCultureIgnoreCase))
{       
    // build and start listener to run asynchronously in the
    // background and wait for incoming connections
    
    Console.WriteLine("Please enter a number between 10.000 and 65.000");
    string? portText = Console.ReadLine();
    int port = int.Parse(string.IsNullOrWhiteSpace(portText) ? "10203": portText);
    Console.Clear();

    var hostName = Dns.GetHostName();
    var ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, port);
    TcpListener listener = new(ipEndPoint);
    
    try
    {    
        listener.Start();
        Console.WriteLine("Please tell the other person to connect to address");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine($"{hostName}:{port} (or the IP {ipEndPoint.Address}:{port})");

        using TcpClient handler = await listener.AcceptTcpClientAsync();
        await using NetworkStream stream = handler.GetStream();

        var buffer = new byte[1_024 * 100];
        int received = await stream.ReadAsync(buffer);

        var message = Encoding.UTF8.GetString(buffer, 0, received);        
        Console.Clear();
        Console.WriteLine($"Secret received:");
        Console.WriteLine($"{message}");
    }
    finally
    {
        listener.Stop();
    }
}
else if(choice.Equals("s", StringComparison.InvariantCultureIgnoreCase))
{
    Console.WriteLine("Enter the address the other person told you:");
    var remoteHost = (Console.ReadLine()??string.Empty).Split(':',2,StringSplitOptions.TrimEntries);

    IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(remoteHost[0]);
    IPAddress ipAddress = ipHostInfo.AddressList[0];
    var remoteIpEndPoint = new IPEndPoint(ipAddress.MapToIPv6(), int.Parse(remoteHost[1]));


    using TcpClient client = new();
    await client.ConnectAsync(remoteIpEndPoint);
    await using NetworkStream stream = client.GetStream();


    Console.Clear();
    Console.WriteLine("Enter your secret:");
    var secret = Encoding.UTF8.GetBytes(Console.ReadLine() ?? "No Input");

    await stream.WriteAsync(secret, 0, secret.Length);
    Console.WriteLine("Message sent");
}

Console.WriteLine("Thank you for using pwshare - Press any key to continue...");
Console.ReadKey();