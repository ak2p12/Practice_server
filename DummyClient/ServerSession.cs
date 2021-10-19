using ServerCore;
using System;
using System.Collections.Generic;
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

    class PlayerInfoReq
    {
        public long playerID;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> _s , ref ushort _count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(_s.Slice(_count , _s.Length - _count) , id);
                _count += sizeof(int);

                success &= BitConverter.TryWriteBytes(_s.Slice(_count , _s.Length - _count) , level);
                _count += sizeof(short);

                success &= BitConverter.TryWriteBytes(_s.Slice(_count, _s.Length - _count), duration);
                _count += sizeof(float);
                return success;
            }

            public void Read(ReadOnlySpan<byte> _s , ref ushort _count)
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

        public void Read(ArraySegment<byte> _arraySegment) //서버에서 받은 패킷을 역직렬화
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(_arraySegment.Array , _arraySegment.Offset , _arraySegment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerID = BitConverter.ToInt64(s.Slice(count , s.Length - count));
            count += sizeof(long);

            //string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count , s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count , nameLen));
            count += nameLen;

            //Skill List
            skills.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; ++i)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read( s , ref count);
                skills.Add(skill);

            }
        }

        public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
            Span<byte> s = new Span<byte>(segment.Array , segment.Offset , segment.Count);

            ushort count = 0;
            bool success = true; //데이터 변환 성공 여부

            //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)
            
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count , s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerID);
            count += sizeof(long);

            //string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name , 0 , this.name.Length , segment.Array , segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen); //string 의 크기를 넣어준다
            count += sizeof(ushort);
            count += nameLen;

            //Skill List
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count); //Skill List의 크기를 넣어준다
            count += sizeof(ushort);

            foreach(SkillInfo skill in skills)
            {
                success &= skill.Write(s , ref count);
            }

            success &= BitConverter.TryWriteBytes(s, count); //최종 패킷 크기

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

            PlayerInfoReq packet = new PlayerInfoReq() { playerID = 1001 , name = "ABCD"};

            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 001 , level = 3 , duration = 1.5f});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 002 , level = 6 , duration = 1.6f});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 003 , level = 9 , duration = 1.7f});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 004 , level = 12 , duration = 1.8f});
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
