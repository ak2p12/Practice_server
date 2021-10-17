using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public string name;

        public PlayerInfoReq()
        {
            this.packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> _arraySegment) 
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(_arraySegment.Array, _arraySegment.Offset, _arraySegment.Count);

            //ushort size = BitConverter.ToUInt16(_arraySegment.Array, _arraySegment.Offset);
            count += 2;
            //ushort packetID = BitConverter.ToUInt16(_arraySegment.Array, _arraySegment.Offset + count);
            count += 2;

            //this.playerID = BitConverter.ToInt64(_arraySegment.Array, _arraySegment.Offset + count);
            this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += 8;
            //Console.WriteLine($"PlayerID : {playerID} ");
        }

        public override ArraySegment<byte> Write() 
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            ushort count = 0;
            bool success = true; //데이터 변환 성공 여부

            //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetID);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerID);
            count += sizeof(long);

            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
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
