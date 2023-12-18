namespace CourseWork.BusinessLogic.Services.Implementations
{
    public class CustomThreadPool
    {
        private Thread[] workingThreads;

        public readonly int _workingThreadAmount;

        public CustomThreadPool(int workingThreadAmount = 4)
        {
            _workingThreadAmount = workingThreadAmount;
            workingThreads = new Thread[_workingThreadAmount];
            //InitThreads();
        }

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
                            if (taskQueue.Count == 0) break;

                            task = taskQueue.Dequeue();
                        }

                        task.RunSynchronously();
                    }
                });

                workingThreads[i].Name = $"Thread {i + 1}";
            }
        }

        public void StartThreads()
        {
            foreach (var thread in workingThreads)
                thread.Start();

            foreach (var thread in workingThreads)
                thread.Join();
        }
    }
}