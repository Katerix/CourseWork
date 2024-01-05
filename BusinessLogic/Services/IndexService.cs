namespace CourseWork.BusinessLogic.Services
{
    public class IndexService
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object _lock;

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
        /// Combines the dictionaries.
        /// </summary>
        static void CombineDictionaries() => Words = SubDictionaries.SelectMany(dict => dict)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(group => group.Key, group => group.First().Value);

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

            var allFiles = GetAllFiles();

            for (int i = 0; i < threads.Length; i++)
            {
                var start = i * (Constants.TOTAL_QUANTITY / threadAmount);
                var end = (i + 1) * (Constants.TOTAL_QUANTITY / threadAmount);

                threads[i] = new Thread(() =>
                {
                    var tempDictionary = InitIndexWithDirectory(ref allFiles, start, end);

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
        /// <returns>File paths array.</returns>
        private static string[] GetAllFiles()
        {
            List<string> files = new();

            foreach (var dir in Constants.DIRECTORIES)
            {
                files.AddRange(Directory.GetFiles(dir)
                    .Skip(Constants.START_COUNT)
                    .Take(Constants.SMALL_QUANTITY)
                    .ToArray());
            }

            files.AddRange(Directory.GetFiles(Constants.BIG_DIRECTORY)
                .Skip(Constants.START_COUNT_BIG)
                .Take(Constants.BIG_QUANTITY)
                .ToArray());

            return files.ToArray();
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="files">The files.</param>
        private static Dictionary<string, HashSet<string>> InitIndexWithDirectory(ref string[] files, int start, int end)
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
            //Console.WriteLine("chunk inited");

            return tempDictionary;
        }
    }
}