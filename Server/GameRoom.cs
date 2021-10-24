using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> session = new List<ClientSession>();
        JobQueue jobQueue = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach(ClientSession s in session)
            {
                 s.Send(pendingList);
            }
            Console.WriteLine($"서버에 넘기는 pendingList의 개수 : {pendingList.Count}");
            pendingList.Clear();
        }

        public void Broadcast(ClientSession _session,string _chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = _session.SessionId;
            packet.chat = $"{_chat} 저는 {packet.playerId} 입니다 ";
            ArraySegment<byte> segment = packet.Write();

            pendingList.Add(segment);
            
        }
        public void Enter(ClientSession _session)
        {
            session.Add(_session);
            _session.Room = this;
        }
        public void Leave(ClientSession _session)
        {
            session.Remove(_session);
        }

        
    }
}
