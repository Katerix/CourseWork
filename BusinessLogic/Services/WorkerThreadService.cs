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

            var сonfigurations = ReadFromStream(client, stream);

            GetConfigs(сonfigurations ?? string.Empty, out int threadAmount, out string searchValue);

            WriteToStream(client, stream, "received configs and data");

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

        /// <summary>
        /// Gets the configs.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        /// <param name="threadAmount">The thread amount.</param>
        /// <param name="searchValue">The search value.</param>
        /// <exception cref="System.Exception">BAD DATA</exception>
        public static void GetConfigs(string configurations, out int threadAmount, out string searchValue)
        {
            int i = 0; string tempString = string.Empty;

            if (configurations[i] == 'T')
            {
                i += 2;

                while (configurations[i] != 'M')
                {
                    tempString += configurations[i++];
                }

                threadAmount = Parse(tempString);

                if (configurations[i] == 'M')
                {
                    i += 2; tempString = string.Empty;

                    while (!(configurations[i] == '\0'))
                    {
                        tempString += configurations[i++];
                    }

                    searchValue = tempString;

                    return;
                }
            }

            throw new Exception("BAD DATA");
        }

        /// <summary>
        /// Parses the specified temporary string for number.
        /// </summary>
        /// <param name="tempStringForNumber">The temporary string for number.</param>
        /// <returns>Parsed value.</returns>
        /// <exception cref="System.Exception">BAD DATA</exception>
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