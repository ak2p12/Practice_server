using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // [Size(2)] [PacketID(2)] [Data]    [Size(2)] [PacketID(2)] [Data] 
        // |===========한 패킷===========|   |============한 패킷===========|

        public sealed override int OnRecv(ArraySegment<byte> _buffer)
        {
            //처리한 패킷 크기 
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                //최소한 패킷 크기를 가공할 수 있는지 (패킷Size)
                //한 패킷의 Size가 기준보다 작다면 탈출
                if (_buffer.Count < HeaderSize)
                    break;

                //패킷이 완전히 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(_buffer.Array , _buffer.Offset);

                if (_buffer.Count < dataSize)
                    break;

                //패킷 가공
                OnRecvPacket( new ArraySegment<byte>(_buffer.Array , _buffer.Offset , dataSize) );
                packetCount++;
                processLen += dataSize;

                //한 패킷을 처리했다면 그 다음 패킷처리
                _buffer = new ArraySegment<byte>(_buffer.Array , _buffer.Offset + dataSize, _buffer.Count - dataSize);
            }
            if (packetCount > 1)
                Console.WriteLine($"패킷모아보내기 : {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> _buffer);
    }

    public abstract class Session
    {
        Socket socket;
        int disconnected = 0; //Disconnect 함수를 중복호출을 방지하기 위한 변수

        RecvBuffer recvBuffer = new RecvBuffer(65535);

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();
        object myLock = new object();

        public abstract void OnConnected(EndPoint _endPoint);
        public abstract int OnRecv(ArraySegment<byte> _buffer);
        public abstract void OnSend(int _numOfBytes);
        public abstract void OnDisconnected(EndPoint _endPoint);

        void Clear()
        {
            lock(myLock)
            {
                sendQueue.Clear();
                pendingList.Clear();
            }
            
        }

        public void Init(Socket _socket)
        {
            socket = _socket;
            
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        #region 보낸다
        public void Send(ArraySegment<byte> _sendBuff)
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

        public void Send(List<ArraySegment<byte>> _sendBuffList)
        {
            if (_sendBuffList.Count == 0)
                return;
            //socket.Send(_sendBuff);
            //sendArgs.SetBuffer(_sendBuff , 0 , _sendBuff.Length);

            lock (myLock)
            {
                foreach(ArraySegment<byte> sendBuffer in _sendBuffList)
                    sendQueue.Enqueue(sendBuffer);

                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }
        void RegisterSend()
        {
            if (disconnected == 1)
                return;
            while (sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = sendQueue.Dequeue();
                pendingList.Add(buff);
            }

            sendArgs.BufferList = pendingList;

            try
            {
                bool pending = socket.SendAsync(sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, sendArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"ReigisterSend Failed {e} ");
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
            if (disconnected == 1)
                return; 

            recvBuffer.Clean();
            ArraySegment<byte> segmenat = recvBuffer.WriteSegment;

            recvArgs.SetBuffer(segmenat.Array, segmenat.Offset, segmenat.Count);

            try
            {
                bool pending = socket.ReceiveAsync(recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, recvArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"RigisterRecv Failed {e}");
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
                    //Write 커서 이동
                    if(recvBuffer.OnWrite(_args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    //클라이언트 -> 서버
                    //클라리언트에서 정보를 받는다.
                    //접속승인한 클라소켓에서 정보를 받는다
                    //byte[] recbuff = new byte[1024]; //데이터를 받을 변수

                    //OnRecv(new ArraySegment<byte>(_args.Buffer, _args.Offset, _args.BytesTransferred));

                    //컨텐츠 쪽에서 데이터를 넘겨주고 얼마나 처리 했는지 받는다.
                    int processLen = OnRecv(recvBuffer.ReadSegment);

                    if (processLen < 0  || recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    //Read 커서 이동
                    if (recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return; 
                    }


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
            Clear();
        }
    }
}
