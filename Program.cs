using System;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Security;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        private static ConcurrentQueue<string> group1_q = new ConcurrentQueue<string>();
        private static ConcurrentQueue<string> group2_q = new ConcurrentQueue<string>();
        private static ConcurrentQueue<string> group3_q = new ConcurrentQueue<string>();
        private static readonly object queueLock = new object();
        private static string inputFilePath = @"C:\New folder\testfiles";
        private static int numThreads = 3;
        private static int waitTimeForQueue = 5000; //milliseconds

        private static int currentg = 0;
        static void Main(string[] args)
        {
            Thread thr = new Thread(DequeueProcessor);
            thr.Start();
            watcher();

        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void watcher()
        {
            MonitorDirectory();

        }
        private static void MonitorDirectory()
        {
            Console.WriteLine("Hello World!");
            using var watcher = new FileSystemWatcher();
            watcher.Path = inputFilePath;
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
            Console.ReadKey();
        }

        // This function is like a callback which is called every time a file is created in the given folder
        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.Name}";
            Console.WriteLine(value);
            string groupName = e.Name.Split('.')[0];
            if (groupName == "group1")
            {
                Console.WriteLine("Enqueue in group1");
                EnqueueFile(e.Name, group1_q);
            }
            else if (groupName == "group2")
            {
                Console.WriteLine("Enqueue in group2");
                EnqueueFile(e.Name, group2_q);
            }
            else if (groupName == "group3")
            {
                Console.WriteLine("Enqueue in group3");
                EnqueueFile(e.Name, group3_q);
            }
        }

        private static void EnqueueFile(string path, ConcurrentQueue<string> q)
        {
            q.Enqueue(path);
        }

        // gets the queue structure corresponding to the queue number
        private static ConcurrentQueue<string> getQueue(int queueNum)
        {
            if (queueNum == 0)
            {
                return group1_q;
            }
            else if (queueNum == 1)
            {
                return group2_q;
            }
            else
            {
                return group3_q;
            }
        }

        private static void DequeueProcessor()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // as of now just running the dequeue processor for 90 seconds. can be changed if needed
            while (stopWatch.ElapsedMilliseconds < 90000)
                DequeueFile();

        }

        // this function dequeues the filenames from the 3 queues in a round robin fashion.        
        private static void DequeueFile()
        {
            if (group1_q.IsEmpty && group2_q.IsEmpty && group3_q.IsEmpty)
            {
                return;
            }

            Task<string>[] tasks = new Task<string>[numThreads];
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            // go through the queues in round robin , fetch the head element filename and and push the trasform algo task 
            // into the tasks array (consists fo 3 elements max, which is configurable)
            // In case there arent enough elelments or files to be fill up the array, then wait for a timer and then
            // just break out of the loop to go ahead and execute whatever tasks are available.
            for (int j = 0; j < numThreads;)
            {
                var q = getQueue(currentg);
                if (!q.IsEmpty)
                {
                    string file = "";
                    q.TryDequeue(out file);
                    Console.WriteLine($"file dequeued for group {currentg + 1} is {file}");
                    tasks[j] = Task<string>.Factory.StartNew(() => Processing.transformAlgoWrapper(file, inputFilePath + @"\", TransformAlgoType.LowerCase));
                    j++;
                }
                currentg = (currentg + 1) % 3;

                if (stopWatch.ElapsedMilliseconds > 5000)
                    break;

            }

            stopWatch.Stop();
            int threads = numThreads;
            // in case any of the tasks element in the array is not filled, then make sure the next time, the queue group corresponding 
            // to the currentg integer value is chosen first inthe round robin to attain fairness.
            while (threads > 0)
            {
                if (tasks[threads - 1] == null)
                {
                    currentg = threads - 1;

                    Console.WriteLine($"Setting next group to {currentg}");
                }
                else
                {
                    tasks[threads - 1].Wait();
                }
                threads--;
            }


            for (int j = 0; j < numThreads; j++)
            {
                if (tasks[j] != null)
                {
                    if (!tasks[j].Result.Contains("processed"))
                    {
                        Console.WriteLine($"file {tasks[j].Result} processing failed");
                    }
                    else
                    {
                        Console.WriteLine($"file {tasks[j].Result} processing Succeeded");
                    }
                }
            }
            
         
        }

        
    }
}
