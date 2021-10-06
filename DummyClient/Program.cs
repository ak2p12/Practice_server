using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {

            //DSN (Domain Name System)
            string host = Dns.GetHostName(); //로컬pc의 호스트 이름을 얻는다
            IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트 정보를 얻는다.
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while(true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //문지기에게 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine($"연결완료 : {socket.RemoteEndPoint.ToString()}");
                    
                    for (int i = 0; i < 5; ++i)
                    {
                        //보낸다
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"안녕하세요 클라입니다  {i}");
                        int sendbytes = socket.Send(sendBuff);
                    }
                    

                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvbytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvbytes);
                    Console.WriteLine($"서버 : {recvData}");

                    //나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }

            
           
        }
    }
}
