using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class SessionManager
    {
        static SessionManager session = new SessionManager();
        public static SessionManager Instance { get { return session; } }


        int sessionId = 0;
        Dictionary<int, ClientSession> sessions = new Dictionary<int, ClientSession>();
        object myLock = new object();

        public ClientSession Generate()
        {
            lock(myLock)
            {
                int id = ++sessionId;
                ClientSession clientSession = new ClientSession();
                clientSession.SessionId = sessionId;

                sessions.Add(id , clientSession);

                Console.WriteLine($"연결 : {id}");

                return clientSession;

            }
        }

        public ClientSession Fine(int _Id)
        {
            lock(myLock)
            {
                ClientSession clientSession = null;
                sessions.TryGetValue(_Id, out clientSession) ;
                return clientSession;
            }
        }

        public void Remove(ClientSession _clientSession)
        {
            lock(myLock)
            {
                sessions.Remove(_clientSession.SessionId);
            }
        }
    }
}
