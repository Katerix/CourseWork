using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CourseWork.BusinessLogic.Services;

public class Server
{
    private readonly IPAddress _ipAddress;

    private readonly int _port;

    public readonly TcpListener _listener;

    public CustomThreadPoolService CustomThreadPool;

    public Server()
    {
        // socket initialization
        _ipAddress = IPAddress.Parse("127.0.0.1");
        _port = 6666;
        _listener = new TcpListener(_ipAddress, _port);

        int threadAmount = 4;

        var timer = new Stopwatch();
            
        timer.Start();

        // index initialization
        IndexService.InitIndex(threadAmount);

        timer.Stop();

        Console.WriteLine($"Index inited! {timer.ElapsedMilliseconds} ms.");
        Console.WriteLine($"Threads: {threadAmount}\n");
        
        // preparing thread pool to execute enqueued tasks
        CustomThreadPool = new CustomThreadPoolService(threadAmount);

        CustomThreadPool.InitThreads();

        // starting the server
        _listener.Start();
        Console.WriteLine("Server is started and ready to receive calls...\n");
    }

    public TcpClient AcceptClient() => _listener.AcceptTcpClient();

    public static void Main(string[] args)
    {
        Server server = new Server();

        server.CustomThreadPool.StartThreads();

        while (true)
        {
            var client = server.AcceptClient();

            server.CustomThreadPool.AddToQueue(async () => await WorkerThreadService.HandleClient(client));
        }
    }
}