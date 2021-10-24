using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace DummyClient
{

	class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속승인 : {_endPoint}");
        }

        public override void OnDisconnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속종료 : {_endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this,buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"클라에서 서버로 {numOfBytes} 이만큼 보내기 성공");
            //Console.WriteLine($"바이트 수 : {numOfBytes}");
        }
    }
}
