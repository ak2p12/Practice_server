using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }
    public class JobQueue: IJobQueue
    {
        Queue<Action> jobQueue = new Queue<Action>();
        object mylock = new object();
        bool flush = false;
        public void Push(Action job)
        {
            bool f = false;
            lock (mylock)
            {
                jobQueue.Enqueue(job);
                if (flush == false)
                {
                    flush = f = true;
                }
            }

            if (f == true)
            {
                Flush();
            }
        }
        
        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock (mylock)
            {
                if (jobQueue.Count == 0)
                {
                    flush = false;
                    return null;
                }
                    
                return jobQueue.Dequeue();
            }
        }

    }

   
}
