namespace CourseWork.BusinessLogic.Services
{
    public class CustomThreadPoolService
    {
        /// <summary>
        /// The lock
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// The working threads
        /// </summary>
        private Thread[] workingThreads;

        /// <summary>
        /// The working thread amount
        /// </summary>
        private readonly int _workingThreadAmount;

        /// <summary>
        /// Gets or sets the task queue.
        /// </summary>
        public Queue<Action> TaskQueue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomThreadPoolService"/> class.
        /// </summary>
        /// <param name="workingThreadAmount">The working thread amount.</param>
        public CustomThreadPoolService(int workingThreadAmount = 4)
        {
            _workingThreadAmount = workingThreadAmount;
            TaskQueue = new();
            _lock = new object();

            workingThreads = new Thread[_workingThreadAmount];
        }

        /// <summary>
        /// Adds to queue.
        /// </summary>
        /// <param name="task">The task to add.</param>
        public void AddToQueue(Action task) 
        {
            lock (_lock)
            {
                TaskQueue.Enqueue(task);
                Monitor.Pulse(_lock);
            }
        }

        /// <summary>
        /// Initializes the threads.
        /// </summary>
        public void InitThreads()
        {
            for (int i = 0; i < _workingThreadAmount; i++)
            {
                workingThreads[i] = new(() =>
                {
                    while (true)
                    {
                        Action task;

                        lock (_lock)
                        {
                            while (TaskQueue.Count == 0)
                            {
                                Monitor.Wait(_lock);
                            }

                            task = TaskQueue.Dequeue();
                        }

                        task.Invoke();
                    }
                });

                Console.WriteLine($"Thread {workingThreads[i].ManagedThreadId} is created");
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