using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> sessionFactory;
        public void Connect(IPEndPoint _endPoint , Func<Session> _sessionFactory)
        {
            
            Socket socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //서버 생성.

            sessionFactory = _sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = _endPoint;
            args.UserToken = socket;

            RigesterConnect(args);
        }

        void RigesterConnect(SocketAsyncEventArgs _args)
        {
            Socket socket = _args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(_args);
            if (pending == false)
                OnConnectCompleted(null, _args);

        }

        void OnConnectCompleted(object _sender , SocketAsyncEventArgs _args)
        {
            if (_args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory.Invoke();
                session.Init(_args.ConnectSocket);
                session.OnConnected(_args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Failed : {_args.SocketError}");
            }
        }


    }
}
