using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        //{0} 패킷등록
        public static string managerFormat =
 @"using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{{
    #region Singleton
    static PacketManager instance = new PacketManager();
    public static PacketManager Instance{{ get {{ return instance; }} }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte> , IPacket>> makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
        {0}
    }}

    public void OnRecvPacket(PacketSession session , ArraySegment<byte> buffer , Action<PacketSession , IPacket> onRecvCallback = null)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort packetID = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;

        if (makeFunc.TryGetValue(packetID , out func)) 
        {{
            IPacket packet = func.Invoke(session, buffer); //패킷ID에 따라 handler에 등록 되어 있는 메소스 실행
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }}
    }}

    T MakePacket<T>(PacketSession session , ArraySegment<byte> buffer) where T : IPacket , new()
    {{
        T pkt = new T();
        pkt.Read(buffer);

        return pkt;
    }}

    public void HandlePacket(PacketSession session , IPacket packet)
    {{
        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(packet.Protocol, out action))
        {{
            action.Invoke(session, packet);//패킷 ID별 등록되어 있는 메소드 실행
        }}
    }}
}}";
        //{0} 패킷 이름
        public static string managerRigisgerFormat =
@"
        //makeFunc 받은 패킷 보관
        //handler 패킷 ID 별 실제기능 함수 보관
        makeFunc.Add((ushort)PacketID.{0} , MakePacket<{0}>);
        handler.Add((ushort)PacketID.{0} , PacketHandler.{0}Handler); ";

        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public enum PacketID
{{
    {0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        //{0} 패킷 이름 (ID)
        //{1} 멤버변수
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write
        public static string packetFormat =
@"
public class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {{
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.{0}) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        {3}

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버변수
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"public class {0}
{{
    {2}

    public void Read(ArraySegment<byte> segment, ref ushort count)
    {{
        {3}
    }}

    public bool Write(ArraySegment<byte> segment, ref ushort count)
    {{
        bool success = true;

        {4}

        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();";

        // {0} 변수 이름
        // {1} To~ 변수형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(segment.Array , segment.Offset + count);
count += sizeof({2});
";
        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});
";

        // {0} 변수이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(segment.Array , segment.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(segment.Array , segment.Offset + count, {0}Len);
count += {0}Len;
";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"{1}s.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
ushort {1}Len = BitConverter.ToUInt16(segment.Array , segment.Offset + count);
count += sizeof(ushort);

for (int i = 0; i < {1}Len; ++i)
{{
    {0} {1} = new {0}();
    {1}.Read(segment , ref count);
    {1}s.Add({1} );
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes(this.{0}), 0, segment.Array, segment.Offset + count, sizeof({1}));
count += sizeof({1});
";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});
";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length , segment.Array, segment.Offset + count + sizeof(ushort));
Array.Copy(BitConverter.GetBytes({0}Len), 0, segment.Array, segment.Offset + count, sizeof(ushort)); //string 의 크기를 넣어준다
count += sizeof(ushort);
count += {0}Len;
";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"Array.Copy(BitConverter.GetBytes((ushort)this.{1}s.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));//Skill List의 크기를 넣어준다
count += sizeof(ushort);

foreach({0} {1} in this.{1}s)
    {1}.Write(segment , ref count);
";
    }
}
