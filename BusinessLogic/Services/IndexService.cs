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
        public static List<KeyValuePair<string, List<string>>> Words { get; private set; }

        /// <summary>
        /// Adds to index.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void AddToIndex(
            List<KeyValuePair<string, List<string>>> words, 
            string keyword, string fileName) => 
            words.Add(new KeyValuePair<string, List<string>>(keyword, new List<string> { fileName }));

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified keyword]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsWord(List<KeyValuePair<string, List<string>>> words, string keyword)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Key == keyword && words[i].Value.Count > 0) return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="words"></param>
        /// <param name="keyword"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ContainsFileInWordValues(List<KeyValuePair<string, List<string>>> words, string keyword, string fileName)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Key == keyword)
                {
                    foreach (var file in words[i].Value)
                    {
                        if (file == fileName) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="words"></param>
        /// <param name="keyword"></param>
        /// <param name="fileName"></param>
        public static void SmartAdd(List<KeyValuePair<string, List<string>>> words, string keyword, string fileName)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Key == keyword)
                {
                    foreach (var file in words[i].Value)
                    {
                        if (file == fileName) return;
                    }

                    words[i].Value.Add(fileName);
                }
            }

            words.Add(new KeyValuePair<string, List<string>>(keyword, new List<string> {fileName }));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="words"></param>
        /// <param name="keyword"></param>
        /// <param name="fileNames"></param>
        public static void SmartAddRange(List<KeyValuePair<string, List<string>>> words, string keyword, List<string> fileNames)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Key == keyword)
                {
                    words[i].Value.AddRange(fileNames);
                    words[i].Value.Distinct();
                }
            }

            words.Add(new KeyValuePair<string, List<string>>(keyword, fileNames));
        }

        /// <summary>
        /// Finds the files.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>Found words which match the input value.</returns>
        private static IEnumerable<string> FindFiles(string keyword, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                if (Words[i].Key == keyword) return Words[i].Value;
            }

            return new List<string>();
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="threadAmount">The thread amount.</param>
        /// <returns>Found words which match the input value.</returns>
        public static IEnumerable<string> PerformSearch(string keyword, int threadAmount = 1)
        {
            if (threadAmount == 1) return FindFiles(keyword, 0, Words.Count);

            Thread[] calculatingThreads = new Thread[threadAmount];

            List<string> results = new();

            for (int i = 0; i < calculatingThreads.Length; i++)
            {
                calculatingThreads[i] = new Thread(() =>
                {
                    var result = FindFiles(
                        keyword, 
                        Words.Count / threadAmount * i,
                        Words.Count / threadAmount * (i + 1)).ToList();

                    if (result.Count > 0)
                    {
                        results = result;
                    }
                });
            }

            foreach (var thread in calculatingThreads)
                thread.Start();

            foreach (var thread in calculatingThreads)
                thread.Join();

            return results;
        }

        /// <summary>
        /// Initializes the index.
        /// </summary>
        /// <param name="threadAmount">The thread amount.</param>
        public static void InitIndex(int threadAmount = 1)
        {
            Words = new List<KeyValuePair<string, List<string>>>();

            Thread[] threads = new Thread[threadAmount];

            foreach (var dir in Constants.DIRECTORIES)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        var result = InitIndexWithDirectory(
                            Directory.GetFiles(dir)
                            .Skip(Constants.START_COUNT + (Constants.END_COUNT - Constants.START_COUNT) / threadAmount * i)
                            .Take(Constants.SMALL_QUANTITY).ToArray());

                        lock (_lock)
                        {
                            foreach (var kvp in result)
                            {
                                SmartAddRange(Words, kvp.Key, kvp.Value);
                            }
                        }
                    });
                }

                foreach (var thread in threads)
                    thread.Start();

                foreach (var thread in threads)
                    thread.Join();
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    var result = InitIndexWithDirectory(
                        Directory.GetFiles(Constants.BIG_DIRECTORY)
                        .Skip(Constants.START_COUNT_BIG + (Constants.END_COUNT_BIG - Constants.START_COUNT_BIG) / threadAmount * i)
                        .Take(Constants.BIG_QUANTITY).ToArray());

                    lock (_lock)
                    {
                        foreach (var kvp in result)
                        {
                            SmartAddRange(Words, kvp.Key, kvp.Value);
                        }       
                    }
                });   
            }

            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads)
                thread.Join();
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="files">The files.</param>
        private static List<KeyValuePair<string, List<string>>> InitIndexWithDirectory(string[] files)
        {
            var result = new List<KeyValuePair<string, List<string>>>();

            for (int i = 0; i < files.Length; i++)
            {
                var wordsInFile = File.ReadAllText(files[i]).Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordsInFile)
                {
                    SmartAdd(result, word, files[i]);
                }
            }

            Console.WriteLine("Directory inited!");

            return result;
        }
    }
}