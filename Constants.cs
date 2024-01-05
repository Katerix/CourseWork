namespace CourseWork
{
    public static class Constants
    {
        /// <summary>
        /// The small quantity
        /// </summary>
        public const int SMALL_QUANTITY = 250;

        /// <summary>
        /// The big quantity
        /// </summary>
        public const int BIG_QUANTITY = 1000;

        /// <summary>
        /// The total quantity
        /// </summary>
        public const int TOTAL_QUANTITY = 2000;

        /// <summary>
        /// The start count
        /// </summary>
        public const int START_COUNT = 7500;

        /// <summary>
        /// The end count
        /// </summary>
        public const int END_COUNT = 7750;

        /// <summary>
        /// The start count big
        /// </summary>
        public const int START_COUNT_BIG = 30000;

        /// <summary>
        /// The end count big
        /// </summary>
        public const int END_COUNT_BIG = 31000;

        /// <summary>
        /// The big directory
        /// </summary>
        public const string BIG_DIRECTORY = "C:/Users/kaate/Programming/Data/train/unsup";

        /// <summary>
        /// The directories
        /// </summary>
        public static string[] DIRECTORIES = new string[4]
        {
            "C:/Users/kaate/Programming/Data/test/neg",
            "C:/Users/kaate/Programming/Data/test/pos",
            "C:/Users/kaate/Programming/Data/train/neg",
            "C:/Users/kaate/Programming/Data/train/pos"
        };
    }
}