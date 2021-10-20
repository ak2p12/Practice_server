using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        //{0} 패킷이름
        //{1} 멤버변수
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write
        public static string packetFormat =
@"
class {0}
{{
    {1}

    public void Read(ArraySegment<byte> _arraySegment) //서버에서 받은 패킷을 역직렬화
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(_arraySegment.Array , _arraySegment.Offset , _arraySegment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
       {2}
    }}

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        Span<byte> s = new Span<byte>(segment.Array , segment.Offset , segment.Count);

        ushort count = 0;
        bool success = true; //데이터 변환 성공 여부

        //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)
            
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count , s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerID);
        count += sizeof(long);
        {3}
        success &= BitConverter.TryWriteBytes(s, count); //최종 패킷 크기

        if (success == false)
            return null;

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1}";

        // {0} 변수 이름
        // {1} To~ 변수형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(_s.Slice(_count, _s.Length - _count));
_count += sizeof({2});";

        // {0} 변수이름
        public static string readStringFormat =
@"ushort {0} = BitConverter.ToUInt16(s.Slice(count , s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count , {0}Len));
count += {0}Len;";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0} , 0 , this.{0}.Length , segment.Array , segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len); //string 의 크기를 넣어준다
count += sizeof(ushort);
count += {0}Len;";
    }
}
