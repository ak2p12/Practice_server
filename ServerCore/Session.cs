using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Session
    {
        Socket socket;
        int disconnected = 0; //Disconnect 함수를 중복호출을 방지하기 위한 변수

        public void Init(Socket _socket)
        {
            socket = _socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            recvArgs.SetBuffer(new byte[1024] , 0 , 1024);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] _sendBuff)
        {
            socket.Send(_sendBuff);
        }

        void RegisterSend(SocketAsyncEventArgs _args)
        {

        }

        void RegisterRecv(SocketAsyncEventArgs _args)
        {
            bool pending = socket.ReceiveAsync(_args);
            if (pending == false)
            {
                OnRecvCompleted(null , _args);
            }
        }

        void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
        {
            //BytesTransferred 
            //받은 문자열의 크기를 반환해준다.
            //상대방이 연결을 끊을경우 가끔 0바이트로 반환 
            if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
            {
                try
                {
                    //클라이언트 -> 서버
                    //클라리언트에서 정보를 받는다.
                    //접속승인한 클라소켓에서 정보를 받는다
                    //byte[] recbuff = new byte[1024]; //데이터를 받을 변수

                    string recvData = Encoding.UTF8.GetString(_args.Buffer, _args.Offset, _args.BytesTransferred);
                    Console.WriteLine($"Client : {recvData}");
                    RegisterRecv(_args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref disconnected, 1) == 1)
                return;

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
