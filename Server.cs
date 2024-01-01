using CourseWork.BusinessLogic.Services;
using System.Net;
using System.Net.Sockets;

public class Server
{
    private readonly IPAddress _ipAddress;

    private readonly int _port;

    public readonly TcpListener _listener;

    public bool IsActive { get; set; }

    public Queue<Task> TaskQueue { get; set; }

    public CustomThreadPoolService CustomThreadPoolService;

    public Server()
    {
        // socket initialization
        _ipAddress = IPAddress.Parse("127.0.0.1");
        _port = 6666;
        _listener = new TcpListener(_ipAddress, _port);

        // prepering a queue for client requests
        TaskQueue = new Queue<Task>();

        int threadAmount = 2;

        // index initialization
        IndexService.InitIndex(threadAmount);

        // preparing thread pool to execute enqueued tasks
        CustomThreadPoolService = new CustomThreadPoolService(threadAmount);

        CustomThreadPoolService.InitThreads(TaskQueue);

        // activating server status
        IsActive = true;

        // starting the server
        Start();
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Server is started and ready to receive calls...\n");
    }

    public TcpClient AcceptClient() => _listener.AcceptTcpClient();

    public static void Main(string[] args)
    {
        Server server = new Server();

        while (server.IsActive)
        {
            server.CustomThreadPoolService.StartThreads();

            var client = server.AcceptClient();

            server.TaskQueue.Enqueue(WorkerThreadService.HandleClient(client));
        }
    }
}