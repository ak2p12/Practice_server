using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; //실행 시간
        public Action action; //시간이 되면 실행할 함수
        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick; //상대방 틱에서 내틱을 뻰다
        }
    }
    class JobTimer
    {
        PriorityQueue<JobTimerElem> pq = new PriorityQueue<JobTimerElem>();
        object mylock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action _action, int tickAfter = 0)
        {
            JobTimerElem job;

            //System.Environment.TickCount 현재 시간
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = _action;

            lock(mylock)
            {
                pq.Push(job);
            }


        }

        public void Flush()
        {
            while(true)
            {
                int now = System.Environment.TickCount;
                JobTimerElem job;

                lock (mylock)
                {
                    if (pq.Count == 0)
                    {
                        break; //lock 이 아닌 while문 탈출
                    }

                    job = pq.Peek();
                    if (job.execTick > now)
                    {
                        break;
                    }

                    job.action.Invoke(); 

                }
            }
        }
    }
}
