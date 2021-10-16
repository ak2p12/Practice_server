using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
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

        public override void Read(ArraySegment<byte> _arraySegment) //서버에서 받은 패킷을 역직렬화
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

        public override ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성

            ushort count = 0;
            bool success = true; //데이터 변환 성공 여부

            //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)
            
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetID);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerID);
            count += 8;

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
        }
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"접속승인 : {_endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerID = 1001 };

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
