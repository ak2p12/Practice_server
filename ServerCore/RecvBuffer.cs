using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        //[] [r] [] [] [w] [] [] [] [] [] 
        ArraySegment<byte> buffer;

        int readPos; //현재 읽고 있는 데이터 위치
        int writePos; //현재 쓸수 있는 데이터 위치

        public RecvBuffer(int _bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[_bufferSize] , 0 , _bufferSize);
        }

        public int DataSize { get { return writePos - readPos; } } //현재 가지고 있는 데이터 크기
        public int FreeSize { get { return buffer.Count - writePos; } } //현재 데이터남은 공간크기

        public ArraySegment<byte> ReadSegment //어디부터 읽으면 되는지
        {
            get { return null; }
        }
        public ArraySegment<byte> WriteSegment
        {
            get { return null; }
        }
    }
}
