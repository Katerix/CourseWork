namespace CourseWork.BusinessLogic.Services.Contracts
{
    /// <summary>
    /// IIndex Interface
    /// </summary>
    interface IIndexService
    {
        /// <summary>
        /// Finds files containing a given keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>List of file names</returns>s
        IEnumerable<string> FindFiles(string keyword);

        /// <summary>
        /// Determines whether the index contains the keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>True or false</returns>
        bool Contains(string keyword);

        /// <summary>
        /// Adds a keyword to the index.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="fileName">Name of the file.</param>
        void AddToIndex(string keyword, string fileName);

        /// <summary>
        /// Initializes the index.
        /// </summary>
        void InitIndex();
    }
}