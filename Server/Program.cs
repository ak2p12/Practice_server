using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
   
    class Program
    {
        static Listener listener = new Listener(); //서버 메인 (?)
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 1000);
        }

        static void Main(string[] args)
        {

            //DSN (Domain Name System)
            string host = Dns.GetHostName(); //로컬pc의 호스트 이름을 얻는다
            IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트이름으로 IP 정보를 얻는다.

            //AddressList 는 IP주소 배열이다.
            //하나일 수도 있지만 여러 개일 수도 있음
            //여러사람이 하나의 IP주소에 접근하면 엄청난 트래픽이 발생
            //트래픽 방지를 위해 여러개의 IP주소가 존재 할 수있음
            IPAddress ipAddr = ipHost.AddressList[0];

            //외부에서 최종적으로 접속할 IP 주소 생성
            //(호스트의 IP , 포트번호)
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //
            listener.Init(endPoint, () => { return SessionManager.Instance.Generate();  });
            Console.WriteLine("서버개방");

            FlushRoom();

            while (true)
            {
                JobTimer.Instance.Flush();
            }


        }
    }
}
