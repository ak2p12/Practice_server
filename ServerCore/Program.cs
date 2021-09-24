using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {

        volatile int  locked = 0;

        public void Acquire()
        {
            while(true)
            {
                //int original = Interlocked.Exchange(ref locked, 1);

                //if (original == 0)
                //{
                //    break;
                //}

                int original = Interlocked.CompareExchange(ref locked , 1 , 0);

                if (original == 0)
                {
                    break;
                }
                    
            }
        }

        public void Release()
        {
            locked = 0;
        }
    }
    class Program
    {
        static int number = 0;
        static SpinLock spinlock = new SpinLock();

        static void Thread_1()
        {
            for(int i = 0; i < 100000; ++i)
            {
                spinlock.Acquire();
                number++;
                spinlock.Release();
            }
        }

        static void Thread_2()
        {
            for(int i = 0; i < 100000; ++i)
            {
                spinlock.Acquire();
                number--;
                spinlock.Release();
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1,t2);

            Console.WriteLine(number);

        }
    }
}
