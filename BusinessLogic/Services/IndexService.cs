namespace CourseWork.BusinessLogic.Services
{
    public class IndexService
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object _lock;

        /// <summary>
        /// Gets the files.
        /// </summary>
        public static string[] Files { get; private set; }

        /// <summary>
        /// Gets the words.
        /// </summary>
        public static Dictionary<string, HashSet<string>> Words { get; private set; }

        /// <summary>
        /// Gets or sets the sub dictionaries.
        /// </summary>
        private static List<Dictionary<string, HashSet<string>>> SubDictionaries { get; set; }

        /// <summary>
        /// Initializes the <see cref="IndexService"/> class.
        /// </summary>
        static IndexService()
        {
            _lock = new object();
            Words = new();
            SubDictionaries = new();
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>Found words which match the input value.</returns>
        public static IEnumerable<string> PerformSearch(string keyword)
        {
            return Words.ContainsKey(keyword) ? Words[keyword] : new List<string>();
        }

        /// <summary>
        /// Initializes the index.
        /// </summary>
        /// <param name="threadAmount">The thread amount.</param>
        public static void InitIndex(int threadAmount = 1)
        {
            Words = new();

            Thread[] threads = new Thread[threadAmount];

            GetAllFilesParallel(threadAmount);

            for (int i = 0; i < threads.Length; i++)
            {
                var start = i * (Constants.TOTAL_QUANTITY / threadAmount);
                var end = (i + 1) * (Constants.TOTAL_QUANTITY / threadAmount);

                threads[i] = new Thread(() =>
                {
                    var tempDictionary = InitIndexWithDirectory(Files, start, end);

                    lock (_lock)
                    {
                        SubDictionaries.Add(tempDictionary);
                    }
                });
            }
                
            foreach (var thread in threads)
                thread.Start();

           foreach (var thread in threads)
                thread.Join();

            CombineDictionaries();
        }

        /// <summary>
        /// Gets all files.
        /// </summary>
        public static void GetAllFiles()
        {
            List<string> files = new();

            for (int i = 0; i < Constants.DIRECTORIES.Length; i++)
            {
                files.AddRange(Directory.GetFiles(Constants.DIRECTORIES[i])
                            .Skip(Constants.START_COUNT)
                            .Take(Constants.SMALL_QUANTITY)
                            .ToArray());
            }

            files.AddRange(Directory.GetFiles(Constants.BIG_DIRECTORY)
                                .Skip(Constants.START_COUNT_BIG)
                                .Take(Constants.BIG_QUANTITY)
                                .ToArray());

            Files = files.ToArray();
        }

        /// <summary>
        /// Gets all files parallel.
        /// </summary>
        /// <param name="threadAmount">The thread amount.</param>
        public static void GetAllFilesParallel(int threadAmount = 5)
        {
            if (threadAmount != 3) threadAmount = 5;

            List<string> files = new();

            Thread[] threads = new Thread[threadAmount];

            for (int i = 0; i < threadAmount - 1; i++)
            {
                int startRange = Constants.DIRECTORIES.Length / (threadAmount - 1) * i;
                int endRange = Constants.DIRECTORIES.Length / (threadAmount - 1) * (i + 1);

                threads[i] = new Thread(() => 
                {
                    for (int j = startRange; j < endRange; j++)
                    {
                        int index = j;

                        var filesChunk = Directory.GetFiles(Constants.DIRECTORIES[index])
                                        .Skip(Constants.START_COUNT)
                                        .Take(Constants.SMALL_QUANTITY)
                                        .ToArray();

                        lock (_lock)
                        {
                            files.AddRange(filesChunk);
                        }
                    }
                });
            }

            threads[threadAmount - 1] = new Thread(() =>
            {
                var filesChunk = Directory.GetFiles(Constants.BIG_DIRECTORY)
                                .Skip(Constants.START_COUNT_BIG)
                                .Take(Constants.BIG_QUANTITY)
                                .ToArray();

                lock (_lock)
                {
                    files.AddRange(filesChunk);
                }
            });

            foreach (var th in threads)
                th.Start();

            foreach (var th in threads)
                th.Join();
            

            Files = files.ToArray();
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="files">The files.</param>
        private static Dictionary<string, HashSet<string>> InitIndexWithDirectory(string[] files, int start, int end)
        {
            Dictionary<string, HashSet<string>> tempDictionary = new();

            for (int i = start; i < end; i++)
            {
                var wordsInFile = File.ReadAllText(files[i]).Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordsInFile)
                {
                    if (!tempDictionary.ContainsKey(word))
                    {
                        tempDictionary.Add(word, new HashSet<string> { files[i] });
                    }
                    else
                    {
                        tempDictionary[word].Add(files[i]);
                    }
                }
            }

            return tempDictionary;
        }

        /// <summary>
        /// Combines the dictionaries.
        /// </summary>
        static void CombineDictionaries() => Words = SubDictionaries.SelectMany(dict => dict)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(group => group.Key, group => group.First().Value);
    }
}