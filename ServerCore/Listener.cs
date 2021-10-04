using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Listener
    {
        Socket listenSocket;
        Action<Socket> onAcceptHandler;

        public void Init(IPEndPoint endPoint , Action<Socket> handler)
        {
            //문지기
            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            onAcceptHandler += handler;
            listenSocket.Bind(endPoint);

            listenSocket.Listen(10); //최대 대기수

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); //입장시도가 생기면 호출할 콜백함수 등록
            RegisterAccept(args); //최초로 호출
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = listenSocket.AcceptAsync(args); //입장하면 false 반환
            if (pending == false)
            {
                OnAcceptCompleted(null,args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args); //다음 클라이언트의 입장 대기
        }

        public Socket Accept()
        {
            //Accept 동기
            //AcceptAsync 비동기
            return listenSocket.Accept();
        }
    }
}
