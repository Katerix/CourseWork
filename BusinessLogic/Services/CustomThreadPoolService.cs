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
        /// Gets or sets the task queue.
        /// </summary>
        public Queue<Task> TaskQueue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomThreadPoolService"/> class.
        /// </summary>
        /// <param name="workingThreadAmount">The working thread amount.</param>
        public CustomThreadPoolService(Queue<Task> taskQueue, int workingThreadAmount = 4)
        {
            _workingThreadAmount = workingThreadAmount;
            TaskQueue = taskQueue;

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
        }
    }
}