using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        #region Singleton
        static SessionManager session = new SessionManager();
        public static SessionManager Instance { get { return session; } }
        #endregion

        List<ServerSession> sessions = new List<ServerSession>();
        object mylock = new object();
        Random random = new Random();
        public void SendForEach()
        {
            lock(mylock)
            {
                foreach(ServerSession s in sessions)
                {
                    C_Move movePacket = new C_Move();
                    movePacket.posX = random.Next(-50,50);
                    movePacket.posY = 0;
                    movePacket.posZ = random.Next(-50, 50);
                    s.Send(movePacket.Write());
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
