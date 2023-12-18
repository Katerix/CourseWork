using CourseWork.BusinessLogic.Services.Contracts;
using CourseWork.BusinessLogic.Services.Implementations;
using System.Net;
using System.Net.Sockets;

class Server
{
    private readonly IPAddress _ipAddress;

    private readonly int _port;

    public readonly TcpListener _listener;

    public bool IsActive { get; set; }

    protected readonly IIndexService _indexService;

    public Server()
    {
        _ipAddress = IPAddress.Parse("127.0.0.1");
        _port = 6666;
        _listener = new TcpListener(_ipAddress, _port);

        _indexService = new IndexService();
        _indexService.InitIndex();
        
        IsActive = true;
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Server is started and ready to receive calls...\n");
    }

    public void ShutDown()
    {
        _listener.Stop();
        Console.WriteLine("Server is shut down.\n");
    }

    public TcpClient AcceptClient() => _listener.AcceptTcpClient();

    static void Main(string[] args)
    {
        Server server = new Server();
        
        int numerator = 0;

        while (server.IsActive)
        {
            server.Start();

            var client = server.AcceptClient();

            Thread worker = new Thread(() => WorkerThread.HandleClient(client));
            worker.Start();
        }
    }
}