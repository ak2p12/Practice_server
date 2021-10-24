using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    
    public class SendBufferHelper //나만 사용하는 스레드
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>( ()=> { return null; } );
        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int _reserveSize)  //사용할 데이터 크기
        {
            //SendBuffer 가 비어있다면 새로 할당
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            //기존에 데이터 크기가 새로운 데이터 크기보다 작으면 더 크게 새로 할당
            if (CurrentBuffer.Value.FreeSize < _reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(_reserveSize);
        }
        public static ArraySegment<byte> Close(int _usedSize) //최종적으로 사용한 데이터 크기
        {
            return CurrentBuffer.Value.Close(_usedSize);
        }
    }

    public class SendBuffer
    {
        //[] [u] [] [] [] [] [] [] [] [] 
        byte[] buffer;
        int usedSize;

        public int FreeSize { get { return buffer.Length - usedSize; } } //현재 사용할수 있는 데이터 크기

        public SendBuffer(int _chunkSize)
        {
            buffer = new byte[_chunkSize];
        }

        public ArraySegment<byte> Open(int _reserveSize) //_reserveSize 얼마만큼 사용할건지
        {
            if (_reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(buffer , usedSize , _reserveSize);
        }
        public ArraySegment<byte> Close(int _usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer ,usedSize , _usedSize);
            usedSize += _usedSize;
            return segment;
        }

    }
}
