using CourseWork.BusinessLogic.Services.Contracts;

namespace CourseWork.BusinessLogic.Services.Implementations
{
    public class IndexService : IIndexService
    {
        const int START_COUNT = 7500;
        const int END_COUNT = 7750;
        const int START_COUNT_BIG = 30000;
        const int END_COUNT_BIG = 31000;

        static string BIG_DIRECTORY = " ";
        static string[] Directories = { "", "" };

        public Dictionary<string, List<string>> Words { get; private set; }

        /// <inheritdoc />
        public void AddToIndex(string keyword, string fileName) => Words.Add(keyword, new List<string> { fileName });

        /// <inheritdoc />
        public bool Contains(string keyword) => Words.ContainsKey(keyword) && Words[keyword].Count > 0;

        /// <inheritdoc />
        public IEnumerable<string> FindFiles(string keyword)
        {
            foreach (var key in Words.Keys)
            {
                if (key == keyword) return Words[key];
            }
            
            return new List<string>();
        }

        /// <inheritdoc />
        public void InitIndex()
        {
            Words = new Dictionary<string, List<string>>();

            foreach (var dir in Directories)
            {
                InitIndexWithDirectory(dir, START_COUNT, END_COUNT);
            }

            InitIndexWithDirectory(BIG_DIRECTORY, START_COUNT_BIG, END_COUNT_BIG);
        }

        /// <summary>
        /// Initializes the index with directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        private void InitIndexWithDirectory(string directory, int start, int end)
        {
            var files = Directory.GetFiles(directory);

            for (int i = start; i <= end && i < files.Length; i++)
            {
                var wordsInFile = File.ReadAllText(files[i]).Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in wordsInFile)
                {
                    if (!Contains(word))
                    {
                        Words.Add(word, new List<string> { files[i] });
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