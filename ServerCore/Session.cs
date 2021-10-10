using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class Session
    {
        Socket socket;
        int disconnected = 0; //Disconnect 함수를 중복호출을 방지하기 위한 변수

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        Queue<byte[]> sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();
        object myLock = new object();

        public abstract void OnConnected(EndPoint _endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint _endPoint);

        public void Init(Socket _socket)
        {
            socket = _socket;
            
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            recvArgs.SetBuffer(new byte[1024] , 0 , 1024);

            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        #region 보낸다
        public void Send(byte[] _sendBuff)
        {
            //socket.Send(_sendBuff);
            //sendArgs.SetBuffer(_sendBuff , 0 , _sendBuff.Length);
             
            lock (myLock)
            {
                sendQueue.Enqueue(_sendBuff);

                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }
        void RegisterSend()
        {
            while (sendQueue.Count > 0)
            {
                byte[] buff = sendQueue.Dequeue();
                pendingList.Add(new ArraySegment<byte>(buff , 0 , buff.Length));
            }

            sendArgs.BufferList = pendingList;
            
            //sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = socket.SendAsync(sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null , sendArgs);
            }
        }
        void OnSendCompleted(object _sender , SocketAsyncEventArgs _args)
        {
            lock(myLock)
            {
                if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSend(sendArgs.BytesTransferred);
                        

                        if (sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
           
        }

        #endregion

        #region 받는다
        void RegisterRecv()
        {
            bool pending = socket.ReceiveAsync(recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, recvArgs);
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

                    OnRecv(new ArraySegment<byte>(_args.Buffer, _args.Offset, _args.BytesTransferred));


                    RegisterRecv();
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
        #endregion

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref disconnected, 1) == 1)
                return;

            OnDisconnected(socket.RemoteEndPoint);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
