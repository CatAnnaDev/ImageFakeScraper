using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GScraperExample.uselessCode
{
    internal class pool
    {
        public static void start(int nbThread = 1)
        {
            for (int i = 0; i < nbThread; i++)
            {
                worker();
            }
        }

        static Queue myQ = new Queue();
        public static Queue mySyncdQ = Queue.Synchronized(myQ);
        static List<Task> tasks = new List<Task>();

        public static void addtrucenQueue(string value) => mySyncdQ.Enqueue(value);

        public static void worker()
        {
            tasks.Add(Task.Run(() =>
            {
                while (mySyncdQ.Count != 0)
                {
                    lock (mySyncdQ.SyncRoot)
                    {
                        ShowThreadInformation(mySyncdQ.Dequeue().ToString()); // ici thread pool qui marche

                    }
                }
            }));
        }

        private static object lockObj = new object();

        private static void ShowThreadInformation(string taskName)
        {
            string msg = null;
            Thread thread = Thread.CurrentThread;
            lock (lockObj)
            {
                msg = string.Format("{0} thread information\n", taskName) +
                      string.Format("   Background: {0}\n", thread.IsBackground) +
                      string.Format("   Thread Pool: {0}\n", thread.IsThreadPoolThread) +
                      string.Format("   Thread ID: {0}\n", thread.ManagedThreadId);
            }
            Console.WriteLine(msg);
        }
    }
}
