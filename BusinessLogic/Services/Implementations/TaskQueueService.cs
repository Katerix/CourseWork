namespace CourseWork.BusinessLogic.Services.Implementations
{
    public class TaskQueueService
    {
        public Queue<Task> taskQueue;
        public int _queueTimeLimit;

        public TaskQueueService() => taskQueue = new Queue<Task>();

        public void FillQueue()
        {
            
        }

        public static int GetRandomExecutionTime() => 6 + new Random().Next() % 6;

        public void Work(Task task)
        {
            //Console.WriteLine($"{Thread.CurrentThread.Name} started to execute task ({time}s)...\n");

            task.Start(); //can be wrong

            //Console.WriteLine($"{Thread.CurrentThread.Name} finished task ({time}s) execution.\n");
        }

        public void StartQueue()
        {
            while (true)
            {
                if (!taskQueue.Any())
                {
                    lock (taskQueue)
                    {
                        Thread.Sleep(1000);

                        //Console.WriteLine("Queue is currently empty! New tasks will be added soon! \n");

                        FillQueue();

                        //Console.WriteLine("Queue has been filled! \n");
                    }
                }
            }
        }
    }
}
