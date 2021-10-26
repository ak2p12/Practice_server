using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue //패킷을 저장하기 위한 자료구조 큐
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> packetQueue = new Queue<IPacket>();
    object myLock = new object();

    public void Push(IPacket _packet)
    {
        lock(myLock)
        {
            packetQueue.Enqueue(_packet);
        }
    }

    public IPacket Pop()
    {
        lock(myLock)
        {
            if (packetQueue.Count == 0)
                return null;

            return packetQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        List<IPacket> list = new List<IPacket>();

        lock(myLock)
        {
            while(packetQueue.Count > 0)
            {
                list.Add(packetQueue.Dequeue());
            }
        }

        return list;
    }
}
