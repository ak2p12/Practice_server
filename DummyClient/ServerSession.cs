using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace DummyClient
{

	class ServerSession : Session
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속승인 : {_endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerID = 1001 , name = "ABCD"};

			var skill = new PlayerInfoReq.Skill() { id = 001, level = 3, duration = 1.5f };
			skill.attributes.Add(new PlayerInfoReq.Skill.Attribute() { att = 25 });
			packet.skills.Add(skill);
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 002 , level = 6 , duration = 1.6f});
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 003 , level = 9 , duration = 1.7f});
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 004 , level = 12 , duration = 1.8f});
            //서버에 보낼 데이터 생성
            //for (int i = 0; i < 5; ++i)
            {

                ArraySegment<byte> s  = packet.Write();

                //서버에 데이터 전송
                if (s != null)
                    Send(s);
            }

            //RemoteEndPoint
            //소켓에 연결되어 있는 IP와 포트 정보
        }

        public override void OnDisconnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속종료 : {_endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"Server : {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"바이트 수 : {numOfBytes}");
        }
    }
}
