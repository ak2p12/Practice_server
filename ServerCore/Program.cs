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
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"Client : {recvData}");

                //보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("환영합니다");
                clientSocket.Send(sendBuff);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
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
            IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트 정보를 얻는다.
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr , 7777);

            listener.Init(endPoint , OnAcceptHandler);
            Console.WriteLine("서버개방");

            while (true)
            {
                    
            }

            
        }
    }
}
