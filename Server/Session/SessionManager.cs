using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class SessionManager //세션을 관리하는 클래스
    {
        #region Singleton
        static SessionManager session = new SessionManager();
        public static SessionManager Instance { get { return session; } }
        #endregion

        int sessionId = 0; //세션을 구별하기 위한 id
        Dictionary<int, ClientSession> sessions = new Dictionary<int, ClientSession>(); //세션을 담을 자료구조
        object myLock = new object(); //락을 사용하기 위한 

        public ClientSession Generate() //세션 생성 함수
        {
            lock(myLock)
            {
                int id = ++sessionId; //세션을 구별하기 위한 id
                ClientSession clientSession = new ClientSession();
                clientSession.SessionId = sessionId;

                sessions.Add(id , clientSession);// 생성한 세션을 담는다.

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
