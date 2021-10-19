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

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> _s, ref ushort _count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(_s.Slice(_count, _s.Length - _count), id);
                _count += sizeof(int);

                success &= BitConverter.TryWriteBytes(_s.Slice(_count, _s.Length - _count), level);
                _count += sizeof(short);

                success &= BitConverter.TryWriteBytes(_s.Slice(_count, _s.Length - _count), duration);
                _count += sizeof(float);
                return success;
            }

            public void Read(ReadOnlySpan<byte> _s, ref ushort _count)
            {
                id = BitConverter.ToInt32(_s.Slice(_count, _s.Length - _count));
                _count += sizeof(int);

                level = BitConverter.ToInt16(_s.Slice(_count, _s.Length - _count));
                _count += sizeof(short);

                duration = BitConverter.ToSingle(_s.Slice(_count, _s.Length - _count));
                _count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> _arraySegment) //서버에서 받은 패킷을 역직렬화
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(_arraySegment.Array, _arraySegment.Offset, _arraySegment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            //string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            //Skill List
            skills.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; ++i)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);

            }
        }

        public override ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
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

            //string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen); //string 의 크기를 넣어준다
            count += sizeof(ushort);
            count += nameLen;

            //Skill List
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count); //Skill List의 크기를 넣어준다
            count += sizeof(ushort);

            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref count);
            }

            success &= BitConverter.TryWriteBytes(s, count); //최종 패킷 크기

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
                        Console.WriteLine($"이름 : {p.name} \nPlayerID : {p.playerID} ");
                        foreach (PlayerInfoReq.SkillInfo skill in p.skills)
                        {
                            Console.WriteLine($"skill ({skill.id})({skill.level})({skill.duration})");
                        }
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
