using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> sessionList = new List<ClientSession>();
        JobQueue jobQueue = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach(ClientSession s in sessionList)
            {
                 s.Send(pendingList);
            }
            Console.WriteLine($"서버에서 클라소켓에 넘기는 pendingList의 개수 : {pendingList.Count}");
            pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            pendingList.Add(segment);
        }
        public void Enter(ClientSession session)
        {
            //플레이어를 추가한다
            sessionList.Add(session);
            session.Room = this;

            // 새로들어온 클라이언트에게 모든 플레이어 목록 전송
            S_PlayerList playerList = new S_PlayerList();
            foreach(ClientSession s in sessionList)
            {
                playerList.players.Add( new S_PlayerList.Player() 
                { 
                    isSelf = (s == session) ,
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(playerList.Write());

            // 새로들어온 클라이언트 입장을 모두에게 알린다
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());

        }
        public void Leave(ClientSession session)
        {
            //클라이언트 제거
            sessionList.Remove(session);

            //모두에게 알린다
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session , C_Move packet)
        {
            //좌표를 바꾼다
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            //모두에게 알린다
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }

        
    }
}
