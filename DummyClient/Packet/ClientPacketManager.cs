using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
    #region Singleton
    static PacketManager instance = new PacketManager();
    public static PacketManager Instance{ get { return instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte> , IPacket>> makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        
        //makeFunc 받은 패킷 보관
        //handler 패킷 ID 별 실제기능 함수 보관
        makeFunc.Add((ushort)PacketID.S_BroadcastEnterGame , MakePacket<S_BroadcastEnterGame>);
        handler.Add((ushort)PacketID.S_BroadcastEnterGame , PacketHandler.S_BroadcastEnterGameHandler); 

        //makeFunc 받은 패킷 보관
        //handler 패킷 ID 별 실제기능 함수 보관
        makeFunc.Add((ushort)PacketID.S_BroadcastLeaveGame , MakePacket<S_BroadcastLeaveGame>);
        handler.Add((ushort)PacketID.S_BroadcastLeaveGame , PacketHandler.S_BroadcastLeaveGameHandler); 

        //makeFunc 받은 패킷 보관
        //handler 패킷 ID 별 실제기능 함수 보관
        makeFunc.Add((ushort)PacketID.S_PlayerList , MakePacket<S_PlayerList>);
        handler.Add((ushort)PacketID.S_PlayerList , PacketHandler.S_PlayerListHandler); 

        //makeFunc 받은 패킷 보관
        //handler 패킷 ID 별 실제기능 함수 보관
        makeFunc.Add((ushort)PacketID.S_BroadcastMove , MakePacket<S_BroadcastMove>);
        handler.Add((ushort)PacketID.S_BroadcastMove , PacketHandler.S_BroadcastMoveHandler); 

    }

    public void OnRecvPacket(PacketSession session , ArraySegment<byte> buffer , Action<PacketSession , IPacket> onRecvCallback = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort packetID = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;

        if (makeFunc.TryGetValue(packetID , out func)) 
        {
            IPacket packet = func.Invoke(session, buffer); //패킷ID에 따라 handler에 등록 되어 있는 메소스 실행
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session , ArraySegment<byte> buffer) where T : IPacket , new()
    {
        T pkt = new T();
        pkt.Read(buffer);

        return pkt;
    }

    public void HandlePacket(PacketSession session , IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(packet.Protocol, out action))
        {
            action.Invoke(session, packet);//패킷 ID별 등록되어 있는 메소드 실행
        }
    }
}