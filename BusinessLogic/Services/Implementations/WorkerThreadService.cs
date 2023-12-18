using System.Net.Sockets;
using System.Text;

namespace CourseWork.BusinessLogic.Services.Implementations
{
    public static class WorkerThread
    {
        public static async Task HandleClient(TcpClient client)
        {
            int N, R, threadAmount;

            NetworkStream stream = client.GetStream();

            WriteToStream(client, stream, "connected");
            Console.WriteLine("Client connected \n");

            byte[] inputArray; string message;

            var сonfiguration = ReadFromStream(client, stream);

            GetConfigs(сonfiguration ?? string.Empty, out N, out R, out threadAmount);

            WriteToStream(client, stream, "received configs");

            inputArray = ReadInput(client, stream, N);

            WriteToStream(client, stream, "received data");

            while (true)
            {
                message = ReadFromStream(client, stream);

                if (message.Contains("start calculation"))
                {
                    Console.WriteLine("Starting calculation... \n");

                    Task<string> calculationTask = Task.Run(() =>
                    {
                        string calculationResults = string.Empty;
                        PerformSearch(inputArray, R, threadAmount, out calculationResults);
                        return calculationResults;
                    });

                    while (!calculationTask.IsCompleted)
                    {
                        message = ReadFromStream(client, stream);

                        if (message.Contains("get status"))
                        {
                            WriteToStream(client, stream, "in progress");
                        }
                    }

                    string results = await calculationTask;
                    SendResults(client, stream, results);
                    Console.WriteLine("Finished and sent results!\n");

                    break;
                }
            }
        }

        public static void GetConfigs(string configurations, out int N, out int R, out int threadAmount)
        {
            int i = 0; string tempStringForNumber = string.Empty;

            if (configurations[i] == 'T')
            {
                i += 2;

                while (configurations[i] != 'N')
                {
                    tempStringForNumber += configurations[i++];
                }

                threadAmount = Parse(tempStringForNumber);

                if (configurations[i] == 'N')
                {
                    i += 2; tempStringForNumber = string.Empty;

                    while (configurations[i] != 'R')
                    {
                        tempStringForNumber += configurations[i++];
                    }

                    N = Parse(tempStringForNumber);

                    if (configurations[i] == 'R')
                    {
                        i += 2; tempStringForNumber = string.Empty;

                        while (i < configurations.Length)
                        {
                            tempStringForNumber += configurations[i++];
                        }

                        R = Parse(tempStringForNumber);

                        return;
                    }
                }
            }

            throw new Exception("BAD DATA");
        }

        private static int Parse(string tempStringForNumber)
        {
            try
            {
                return int.Parse(tempStringForNumber);
            }
            catch (Exception)
            {
                throw new Exception("BAD DATA");
            }
        }

        public static void PerformSearch(byte[] data, int range, int threadAmount, out string results)
        {
            results = string.Empty; // call index service method FindFiles
        }

        public static void SendResults(TcpClient client, NetworkStream stream, string result)
        {
            stream = client.GetStream();
            byte[] resultBytes = Encoding.ASCII.GetBytes(result);
            stream.Write(resultBytes, 0, resultBytes.Length);
        }

        public static byte[] ReadInput(TcpClient client, NetworkStream stream, int N)
        {
            stream = client.GetStream();
            byte[] data = new byte[4 * N];
            stream.Read(data, 0, data.Length);

            return data;
        }

        public static string ReadFromStream(TcpClient client, NetworkStream stream)
        {
            stream = client.GetStream();
            byte[] data = new byte[20];
            stream.Read(data, 0, data.Length);
            return Encoding.ASCII.GetString(data);
        }

        public static void WriteToStream(TcpClient client, NetworkStream stream, string message)
        {
            stream = client.GetStream();
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}