using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager session = new SessionManager();
        public static SessionManager Instance { get { return session; } }

        List<ServerSession> sessions = new List<ServerSession>();
        object mylock = new object();

        public void SendForEach()
        {
            lock(mylock)
            {
                foreach(ServerSession s in sessions)
                {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = "안녕하세요 서버님";
                    ArraySegment<byte> segment = chatPacket.Write();
                    s.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock(mylock)
            {
                ServerSession s = new ServerSession();
                sessions.Add(s);
                return s;
            }
        }
    }
}
