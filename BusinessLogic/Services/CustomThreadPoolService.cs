namespace CourseWork.BusinessLogic.Services
{
    public class CustomThreadPoolService
    {
        /// <summary>
        /// The working threads
        /// </summary>
        private Thread[] workingThreads;

        /// <summary>
        /// The working thread amount
        /// </summary>
        public readonly int _workingThreadAmount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomThreadPoolService"/> class.
        /// </summary>
        /// <param name="workingThreadAmount">The working thread amount.</param>
        public CustomThreadPoolService(int workingThreadAmount = 4)
        {
            _workingThreadAmount = workingThreadAmount;
            workingThreads = new Thread[_workingThreadAmount];
        }

        /// <summary>
        /// Initializes the threads.
        /// </summary>
        /// <param name="taskQueue">The task queue.</param>
        public void InitThreads(Queue<Task> taskQueue)
        {
            for (int i = 0; i < _workingThreadAmount; i++)
            {
                workingThreads[i] = new Thread(() =>
                {
                    while (true)
                    {
                        Task task;

                        lock (taskQueue)
                        {
                            if (taskQueue.Count == 0) continue;

                            task = taskQueue.Dequeue();
                        }

                        task.RunSynchronously();
                    }
                });

                workingThreads[i].Name = $"Thread {i + 1}";
            }
        }

        /// <summary>
        /// Starts the threads.
        /// </summary>
        public void StartThreads()
        {
            foreach (var thread in workingThreads)
                thread.Start();

            foreach (var thread in workingThreads)
                thread.Join();
        }
    }
}