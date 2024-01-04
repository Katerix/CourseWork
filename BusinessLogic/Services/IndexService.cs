namespace CourseWork.BusinessLogic.Services
{
    public class IndexService
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the words.
        /// </summary>
        public static Dictionary<string, HashSet<string>> Words { get; private set; }

        /// <summary>
        /// Combines the dictionaries.
        /// </summary>
        /// <param name="dictionaries">The dictionaries.</param>
        static void CombineDictionaries(List<Dictionary<string, HashSet<string>>> dictionaries) => Words = dictionaries.SelectMany(dict => dict)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(group => group.Key, group => group.First().Value);

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="threadAmount">The thread amount.</param>
        /// <returns>Found words which match the input value.</returns>
        public static IEnumerable<string> PerformSearch(string keyword) => Words.ContainsKey(keyword) ? Words[keyword] : new List<string>();

        /// <summary>
        /// Initializes the index.
        /// </summary>
        /// <param name="threadAmount">The thread amount.</param>
        public static void InitIndex(int threadAmount = 1)
        {
            Words = new();

            Thread[] threads = new Thread[threadAmount];

            List<Dictionary<string, HashSet<string>>> tempDictionaryForAllThreads = new();

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    List<Dictionary<string, HashSet<string>>> tempDictionary = new();

                    foreach (var dir in Constants.DIRECTORIES)
                    {
                        tempDictionary.Add(InitIndexWithDirectory(
                            Directory.GetFiles(dir)
                            .Skip(Constants.START_COUNT + i * (Constants.END_COUNT - Constants.START_COUNT) / threadAmount)
                            .Take(Constants.SMALL_QUANTITY).ToArray()));
                    }

                    tempDictionary.Add(InitIndexWithDirectory(
                        Directory.GetFiles(Constants.BIG_DIRECTORY)
                        .Skip(Constants.START_COUNT_BIG + i * (Constants.END_COUNT_BIG - Constants.START_COUNT_BIG) / threadAmount)
                        .Take(Constants.BIG_QUANTITY).ToArray()));

                    lock (_lock)
                    {
                        tempDictionaryForAllThreads.AddRange(tempDictionary);
                    }
                });
            }
                
            foreach (var thread in threads)
                thread.Start();

           foreach (var thread in threads)
                thread.Join();

            CombineDictionaries(tempDictionaryForAllThreads);
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="files">The files.</param>
        private static Dictionary<string, HashSet<string>> InitIndexWithDirectory(string[] files)
        {
            Dictionary<string, HashSet<string>> tempDictionary = new();

            for (int i = 0; i < files.Length; i++)
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
    }
}