using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession session = new ServerSession();

    public void Send (ArraySegment<byte> sendBuff)
    {
        session.Send(sendBuff);
    }
    void Start()
    {
        //DSN (Domain Name System)
        string host = Dns.GetHostName(); //로컬pc의 호스트 이름을 얻는다 (서버pc)
        IPHostEntry ipHost = Dns.GetHostEntry(host); //호스트 정보를 얻는다.
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        //Connect(주소 ,함수 , 사용자 수 (클라이언트 수) ) 
        connector.Connect(endPoint, () => { return session; }, 1);

        
    }

    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();

        foreach(IPacket packet in list)
        {
            PacketManager.Instance.HandlePacket(session, packet);
        }
    }
}
