using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public enum PacketID
{
    PlayerInfoReq = 1,
	Test = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class PlayerInfoReq : IPacket
{
    public byte testByte;
	public long playerID;
	public string name;
	public class Skill
	{
	    public int id;
		public short level;
		public float duration;
		public class Attribute
		{
		    public int att;
		
		    public void Read(ReadOnlySpan<byte> s, ref ushort count)
		    {
		        this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
				count += sizeof(int);
				
		    }
		
		    public bool Write(Span<byte> s, ref ushort count)
		    {
		        bool success = true;
		
		        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
				count += sizeof(int);
				
		
		        return success;
		    }
		}
		
		public List<Attribute> attributes = new List<Attribute>();
	
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			
			this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
			count += sizeof(short);
			
			this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			
			attributes.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
			ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			
			for (int i = 0; i < attributeLen; ++i)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(s , ref count);
			    attributes.Add(attribute );
			}
	    }
	
	    public bool Write(Span<byte> s, ref ushort count)
	    {
	        bool success = true;
	
	        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
			count += sizeof(int);
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
			count += sizeof(short);
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
			count += sizeof(float);
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count); //Skill List의 크기를 넣어준다
			count += sizeof(ushort);
			
			foreach(Attribute attribute in this.attributes)
			    success &= attribute.Write(s , ref count);
			
	
	        return success;
	    }
	}
	
	public List<Skill> skills = new List<Skill>();

    public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		
		this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		
		ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
		count += nameLen;
		
		skills.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
		ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		
		for (int i = 0; i < skillLen; ++i)
		{
		    Skill skill = new Skill();
		    skill.Read(s , ref count);
		    skills.Add(skill );
		}
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        ushort count = 0;
        bool success = true; //데이터 변환 성공 여부

        //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)
            
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);

        segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerID);
		count += sizeof(long);
		
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length , segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen); //string 의 크기를 넣어준다
		count += sizeof(ushort);
		count += nameLen;
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count); //Skill List의 크기를 넣어준다
		count += sizeof(ushort);
		
		foreach(Skill skill in this.skills)
		    success &= skill.Write(s , ref count);
		

        success &= BitConverter.TryWriteBytes(s, count); //최종 패킷 크기

        if (success == false)
            return null;

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

class Test : IPacket
{
    public int TestInt;

    public ushort Protocol { get { return (ushort)PacketID.Test; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.TestInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        ushort count = 0;
        bool success = true; //데이터 변환 성공 여부

        //TryWriteBytes (추출한 데이터를 받을 배열 , 추출할 데이터)
            
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.TestInt);
		count += sizeof(int);
		

        success &= BitConverter.TryWriteBytes(s, count); //최종 패킷 크기

        if (success == false)
            return null;

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

