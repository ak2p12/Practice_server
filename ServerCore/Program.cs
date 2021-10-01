using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //DSN (Domain Name System)
            string host = Dns.GetHostName(); //로컬pc의 호스트 이름을 얻는다
            IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트 정보를 얻는다.
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr , 7777);

            Socket listenSocket = new Socket(endPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(endPoint);

            listenSocket.Listen(10); //최대 대기수

            while(true)
            {
                Console.WriteLine("서버개방");

                Socket clientSocket = listenSocket.Accept();

                byte[] recvBuff = new byte[1024];
                clientSocket.Receive();
            }
        }
    }
}
