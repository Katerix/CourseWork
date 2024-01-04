using System.Collections.Concurrent;

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
        /// Combines lists
        /// </summary>
        /// <param name="arrayOfLists"></param>
        /*static void CombineLists(List<List<KeyValuePair<string, HashSet<string>>>> arrayOfLists) => Words = arrayOfLists
                .SelectMany(list => list)
                .GroupBy(kvp => kvp.Key)
                .Select(group => new KeyValuePair<string, HashSet<string>>(
                    group.Key,
                    group.SelectMany(kvp => kvp.Value).ToHashSet()))
                .ToList();*/

        /// <summary>
        /// Smart add
        /// </summary>
        /// <param name="words"></param>
        /// <param name="keyword"></param>
        /// <param name="fileName"></param>
        public static void SmartAdd(List<KeyValuePair<string, HashSet<string>>> words, string keyword, string fileName)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Key == keyword)
                {
                    words[i].Value.Add(fileName);
                    return;
                }
            }

            words.Add(new KeyValuePair<string, HashSet<string>>(keyword, new HashSet<string> { fileName }));
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
                //if (Words[i].Key == keyword) return Words[i].Value;
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
            Words = new();

            Thread[] threads = new Thread[threadAmount];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    foreach (var dir in Constants.DIRECTORIES)
                    {
                        //if (i >= threadAmount) return;

                        InitIndexWithDirectory(
                            Directory.GetFiles(dir)
                            .Skip(Constants.START_COUNT + i * (Constants.END_COUNT - Constants.START_COUNT) / threadAmount)
                            .Take(Constants.SMALL_QUANTITY).ToArray());

                        /*lock (_lock)
                        {
                            Words.AddOrUpdate(r);
                        }*/

                    }

                    InitIndexWithDirectory(
                        Directory.GetFiles(Constants.BIG_DIRECTORY)
                        .Skip(Constants.START_COUNT_BIG + i * (Constants.END_COUNT_BIG - Constants.START_COUNT_BIG) / threadAmount)
                        .Take(Constants.BIG_QUANTITY).ToArray());

                    /*lock (_lock)
                    {
                        results.Add(t);
                    }*/
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
        private static void InitIndexWithDirectory(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                var wordsInFile = File.ReadAllText(files[i]).Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordsInFile)
                {
                    lock(_lock)
                    {
                        if (!Words.ContainsKey(word))
                        {
                            Words.Add(word, new HashSet<string> { files[i] });
                        }
                        else
                        {
                            Words[word].Add(files[i]);
                        }
                    }
                }
            }
        }
    }
}