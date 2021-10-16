using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    }

    public abstract class Packet
    {
        public ushort size;
        public ushort packetID;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> _arraySegment);
    }

    class PlayerInfoReq : Packet
    {
        public long playerID;

        public PlayerInfoReq()
        {
            this.packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> _arraySegment)
        {
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(_arraySegment.Array, _arraySegment.Offset);
            count += 2;
            //ushort packetID = BitConverter.ToUInt16(_arraySegment.Array, _arraySegment.Offset + count);
            count += 2;

            this.playerID = BitConverter.ToInt64(_arraySegment.Array, _arraySegment.Offset + count);
            count += 8;
            //Console.WriteLine($"PlayerID : {playerID} ");
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;


            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetID);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerID);
            count += 8;

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속승인 : {_endPoint}");

            //Packet packet = new Packet() { size = 100, packetID = 10 };

            ////byte[] sendBuff = Encoding.UTF8.GetBytes("환영합니다");

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetID);
            //Array.Copy(buffer , 0 , openSegment.Array , openSegment.Offset ,buffer.Length );
            //Array.Copy(buffer2 , 0 , openSegment.Array , openSegment.Offset + buffer.Length ,buffer2.Length );
            //ArraySegment<byte> sendBuff =  SendBufferHelper.Close(buffer.Length + buffer2.Length);


            //Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> _buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
            count += 2;
            ushort packetID = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + count);
            count += 2;

            Console.WriteLine($"패킷 ID : {packetID}  패킷 크기 : {size}");

            switch((PacketID)packetID)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(_buffer);
                        Console.WriteLine($"PlayerID : {p.playerID} ");
                    }
                    break;

                case PacketID.PlayerInfoOK:
                    {

                    }
                    break;
            }
        }

        public override void OnDisconnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속종료 : {_endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"바이트 수 : {numOfBytes}");
        }
    }
}
