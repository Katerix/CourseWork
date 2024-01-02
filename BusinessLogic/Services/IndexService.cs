namespace CourseWork.BusinessLogic.Services
{
    public static class IndexService
    {
        /// <summary>
        /// Gets the words.
        /// </summary>
        public static List<KeyValuePair<string, List<string>>> Words { get; private set; }

        /// <summary>
        /// Adds to index.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void AddToIndex(string keyword, string fileName) => Words.Add(new KeyValuePair<string, List<string>>(keyword, new List<string> { fileName }));

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified keyword]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string keyword)
        {
            foreach (var kvp in Words)
            {
                if (kvp.Key == keyword && kvp.Value.Count > 0) return true;
            }

            return false;
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
                        (Constants.END_COUNT - Constants.START_COUNT) / threadAmount * i,
                        (Constants.END_COUNT - Constants.START_COUNT) / threadAmount * (i + 1));

                    lock (result)
                    {
                        results.AddRange(result);
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
        public static void InitIndex(int threadAmount = 1)
        {
            Words = new List<KeyValuePair<string, List<string>>>();

            Thread[] threads = new Thread[threadAmount];

            foreach (var dir in Constants.DIRECTORIES)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() => InitIndexWithDirectory(
                        dir,
                        (Constants.END_COUNT - Constants.START_COUNT) / threadAmount * i, 
                        (Constants.END_COUNT - Constants.START_COUNT) / threadAmount * (i + 1)));
                }
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() => InitIndexWithDirectory(
                    Constants.BIG_DIRECTORY,
                    (Constants.END_COUNT_BIG - Constants.START_COUNT_BIG) / threadAmount * i,
                    (Constants.END_COUNT_BIG - Constants.START_COUNT_BIG) / threadAmount * (i + 1)));
            }
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        private static void InitIndexWithDirectory(string directory, int start, int end)
        {
            var files = Directory.GetFiles(directory);

            for (int i = start; i <= end && i < files.Length; i++)
            {
                var wordsInFile = File.ReadAllText(files[i]).Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordsInFile)
                {
                    if (!Contains(word))
                    {
                        lock (Words) // or other lock var
                        {
                            AddToIndex(word, files[i]);
                        }
                    }
                    else
                    {
                        Words.Find(x => x.Key == word).Value.Add(files[i]);
                    }
                }
            }
        }
    }
}