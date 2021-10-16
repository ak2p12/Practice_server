﻿using ServerCore;
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
            string host = Dns.GetHostName(); //로컬pc의 호스트 이름을 얻는다 (서버pc)
            IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트 정보를 얻는다.
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint , () => { return new ServerSession();  });
            while (true)
            {
                try
                {
                    ////문지기에게 입장 문의
                    //socket.Connect(endPoint);

                    ////서버에서 보낸 데이터를 받기 위해 생성
                    //byte[] recvBuff = new byte[1024];

                    ////서버에서 보낸 데이터를 받는다.
                    //int recvbytes = socket.Receive(recvBuff);

                    //string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvbytes);
                    //Console.WriteLine($"서버에서 한말 : {recvData}");

                    //소켓종료
                    //Shutdown
                    //소켓을 종료하지만 매개변수에 따라 송신,수신 차단
                    //SocketShutdown.Receive (수신 차단)
                    //SocketShutdown.Send (송신 차단)
                    //SocketShutdown.Both (둘다 차단)
                    //socket.Shutdown(SocketShutdown.Both);

                    //소켓종료
                    //송신과 수신 둘다 차단 후 종료한다.
                    //socket.Close();
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
