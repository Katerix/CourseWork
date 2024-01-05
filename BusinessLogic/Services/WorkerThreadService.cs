using System.Net.Sockets;
using System.Text;

namespace CourseWork.BusinessLogic.Services
{
    public static class WorkerThreadService
    {
        /// <summary>
        /// Handles the client.
        /// </summary>
        /// <param name="client">The client.</param>
        public static async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            WriteToStream(client, stream, "connected");
            Console.WriteLine("Client connected \n");

            byte[] input; string message;

            var searchValue = CustomTrim(ReadFromStream(client, stream));

            WriteToStream(client, stream, "received data");

            while (true)
            {
                message = ReadFromStream(client, stream);

                if (message.Contains("start calculation"))
                {
                    Console.WriteLine("Starting calculation... \n");

                    Task<List<string>> calculationTask = Task.Run(() => 
                        IndexService.PerformSearch(searchValue.ToString()).ToList());

                    while (!calculationTask.IsCompleted)
                    {
                        message = ReadFromStream(client, stream);

                        if (message.Contains("get status"))
                        {
                            WriteToStream(client, stream, "in progress");
                        }
                    }

                    List<string> results = await calculationTask;
                    SendResults(client, stream, $"results:\n{string.Join('\n', results)}");
                    Console.WriteLine("Finished and sent results!\n");

                    break;
                }
            }
        }

        public static string CustomTrim(string input)
        {
            string result = string.Empty;

            foreach (var ch in input)
            {
                if (ch != '\0')  result += ch;

                else return result;
            }

            return result;
        }

        /// <summary>
        /// Sends the results.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="result">The result.</param>
        public static void SendResults(TcpClient client, NetworkStream stream, string result)
        {
            stream = client.GetStream();
            byte[] resultBytes = Encoding.ASCII.GetBytes(result);
            stream.Write(resultBytes, 0, resultBytes.Length);
        }

        /// <summary>
        /// Reads from stream.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>Client input string.</returns>
        public static string ReadFromStream(TcpClient client, NetworkStream stream)
        {
            stream = client.GetStream();
            byte[] data = new byte[20];
            stream.Read(data, 0, data.Length);
            return Encoding.ASCII.GetString(data);
        }

        /// <summary>
        /// Writes to stream.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="message">The message.</param>
        public static void WriteToStream(TcpClient client, NetworkStream stream, string message)
        {
            stream = client.GetStream();
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}