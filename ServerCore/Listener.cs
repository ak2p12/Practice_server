using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Listener
    {
        //Socket
        //데이터를 통신 할 수 있도록 해주는 연결부
        //클라에서 서버에 접속 요청을 하면 서버 소켓에서 판단하여 승인 여부 결정
        //반대로 서버에서 클라이언트에 접속요청을 하면 클라이언트소켓에서 판단하여 승연 여부 결정
        //창구 또는 문지기 역할
        Socket listenSocket; //서버의 소켓
        Func<Session> sessionFactory; //서버 컨텐츠(서버기능) 생성기

        public void Init(IPEndPoint _endPoint , Func<Session> _sessionFactory)
        {
            //문지기
            //서버 소켓 생성
            listenSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            sessionFactory += _sessionFactory;

            //Bind
            //매개변수로 들어온 IP와 포트정보를 소켓에 묶는다 (연결 , 묶음)
            listenSocket.Bind(_endPoint);

            //대기열 수 지정
            //대기열 수를 초과한다면 접속요청을 하자마자 거부당함
            listenSocket.Listen(10); //최대 대기수

            //SocketAsyncEventArgs
            //비동기 작업에 필요한 이벤트 핸들러
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            //콜백함수 등록
            //접속승인이 되었다면 실행할 함수연결
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); //입장시도가 생기면 호출할 콜백함수 등록

            //멀티쓰레드 환경에서 서버 생성과 동시에 클라이언트에서 접속 승인 요청이 들어올수 있음 
            //에러 방지를 위해 최초 함수호출 or 접속 승인을 비동기로 실행하기 위한 최초 함수호출
            RegisterAccept(args); //최초로 호출
        }

        void RegisterAccept(SocketAsyncEventArgs _args)
        {
            //접속승인후 다시 호출될때 새로운 접속승인을 위해 비워준다.
            _args.AcceptSocket = null;

            //AcceptAsync
            //접속승인이 없을때 무한루프가 아닌 비동기로 진행됨
            //접속승인이 들어오면 접속승인 해주고 arge에 연결된 함수실행
            bool pending = listenSocket.AcceptAsync(_args); //입장하면 false 반환

            //pending == 보류중인가?
            //접속승인을 비동기로 바꿈과 동시에 접속승인이 들어올경우
            //접속승인이 들어오면 false 반환
            if (pending == false)
            {
                OnAcceptCompleted(null, _args);
            }
        }

        void OnAcceptCompleted(object _sender, SocketAsyncEventArgs _args)
        {
            //접속승인을 했다면
            if (_args.SocketError == SocketError.Success)
            {
                //접속승인 후 처리

                //session
                //보내고 받는 역할 클래스
                Session session = sessionFactory.Invoke();

                //AcceptSocket 연결된 소켓
                session.Init(_args.AcceptSocket);

                //RemoteEndPoint
                session.OnConnected(_args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(_args.SocketError.ToString());
            }

            //다음 접속승인을 위해 호출
            RegisterAccept(_args); //다음 클라이언트의 입장 대기
        }

        public Socket Accept()
        {
            //Accept
            //들어온 접속 요청을 승인해준다
            //하지만 접속 요청이 없다면 접속요청이 있을때까지 대기 (무한루프)
            return listenSocket.Accept();
        }
    }
}
