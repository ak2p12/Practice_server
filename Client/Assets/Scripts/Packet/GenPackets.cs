using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public enum PacketID
{
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class S_BroadcastEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastEnterGame; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array , segment.Offset + count);
		count += sizeof(int);
		
		this.posX = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.S_BroadcastEnterGame) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		
		Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

public class C_LeaveGame : IPacket
{
    

    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.C_LeaveGame) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array , segment.Offset + count);
		count += sizeof(int);
		
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.S_BroadcastLeaveGame) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

public class S_PlayerList : IPacket
{
    public class Player
	{
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	    public void Read(ArraySegment<byte> segment, ref ushort count)
	    {
	        this.isSelf = BitConverter.ToBoolean(segment.Array , segment.Offset + count);
			count += sizeof(bool);
			
			this.playerId = BitConverter.ToInt32(segment.Array , segment.Offset + count);
			count += sizeof(int);
			
			this.posX = BitConverter.ToSingle(segment.Array , segment.Offset + count);
			count += sizeof(float);
			
			this.posY = BitConverter.ToSingle(segment.Array , segment.Offset + count);
			count += sizeof(float);
			
			this.posZ = BitConverter.ToSingle(segment.Array , segment.Offset + count);
			count += sizeof(float);
			
	    }
	
	    public bool Write(ArraySegment<byte> segment, ref ushort count)
	    {
	        bool success = true;
	
	        Array.Copy(BitConverter.GetBytes(this.isSelf), 0, segment.Array, segment.Offset + count, sizeof(bool));
			count += sizeof(bool);
			
			Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
			count += sizeof(int);
			
			Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			
			Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			
			Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			
	
	        return success;
	    }
	}
	
	public List<Player> players = new List<Player>();

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        players.Clear(); //리스트 안에 다른 데이터가 들어가 있을 경우
		ushort playerLen = BitConverter.ToUInt16(segment.Array , segment.Offset + count);
		count += sizeof(ushort);
		
		for (int i = 0; i < playerLen; ++i)
		{
		    Player player = new Player();
		    player.Read(segment , ref count);
		    players.Add(player );
		}
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.S_PlayerList) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)this.players.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));//Skill List의 크기를 넣어준다
		count += sizeof(ushort);
		
		foreach(Player player in this.players)
		    player.Write(segment , ref count);
		

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

public class C_Move : IPacket
{
    public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.posX = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.C_Move) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }

    public void Read(ArraySegment<byte> segment) //서버에서 받은 패킷을 역직렬화
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array , segment.Offset + count);
		count += sizeof(int);
		
		this.posX = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(segment.Array , segment.Offset + count);
		count += sizeof(float);
		
    }

    public ArraySegment<byte> Write() //서버에 보낼 데이터를 직렬화
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //패킷크기를 임시적으로 4096 만큼 생성
        ushort count = 0;
        count += sizeof(ushort); //배열의 시작부분은 배열전체크기를 담을 공간이라 건너뛰기위해 증가를 시켜준다.

        //GetBytes 매개변수로 들어온 데이터를 byte 배열로 반환해주는 함수
        Array.Copy( BitConverter.GetBytes((ushort)PacketID.S_BroadcastMove) , 0 , segment.Array , segment.Offset + count , sizeof(ushort) );
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		
		Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));//최종 패킷 크기

        return SendBufferHelper.Close(count); //임시로 생성한 패킷을 실제로 사용한 크기로 다시 생성
    }
}

