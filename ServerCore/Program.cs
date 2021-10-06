using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static Listener listener = new Listener();
        static void OnAcceptHandler(Socket _clientSocket)
        {
            try
            {
                //session
                //보내고 받는 역할 클래스
                Session session = new Session();
                session.Init(_clientSocket);

                //보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("환영합니다");
                session.Send(sendBuff);
                
                Thread.Sleep(1000);

                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
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
            IPEndPoint endPoint = new IPEndPoint(ipAddr , 7777);

            listener.Init(endPoint , OnAcceptHandler);
            Console.WriteLine("서버개방");

            while (true)
            {
                    
            }

            
        }
    }
}
