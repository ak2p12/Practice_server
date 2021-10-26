using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetID;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> arraySegment);
    }

	class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public override void OnConnected(EndPoint _endPoint)
        {
            Program.Room.Push( () => Program.Room.Enter(this));
            Console.WriteLine($"Server에서 한말 : 접속승인됨 {_endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> _buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, _buffer); //클라이언트에서 받은 패킷을 저리한다.
        }

        public override void OnDisconnected(EndPoint _endPoint)
        {
            SessionManager.Instance.Remove(this);

            if (Room != null)
            {
                GameRoom room = Room;
                room.Push( () => room.Leave(this));
                Room = null;
            }
            Console.WriteLine($"Server : 접속종료 : {_endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"바이트 수 : {numOfBytes}");
        }
    }
}
