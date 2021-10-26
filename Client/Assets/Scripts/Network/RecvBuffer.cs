using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        // 1   2   3   4   5   6   7   8   9  10
        //[ ] [ ] [r] [ ] [w] [ ] [ ] [ ] [ ] [ ] 
        ArraySegment<byte> buffer;

        int readPos; //현재 읽고 있는 데이터 위치
        int writePos; //현재 쓸수 있는 데이터 위치

        public RecvBuffer(int _bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[_bufferSize] , 0 , _bufferSize);
        }

        public int DataSize { get { return writePos - readPos; } } //현재 가지고 있는 데이터 크기
        public int FreeSize { get { return buffer.Count - writePos; } } //현재 받을 수 있는 데이터 크기

        public ArraySegment<byte> ReadSegment //어디부터 읽으면 되는지
        {
            get { return new ArraySegment<byte>(buffer.Array,buffer.Offset + readPos , DataSize); }
        }
        public ArraySegment<byte> WriteSegment //어디서 부터 받으면 되는지
        {
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + writePos, FreeSize); }
        }

        public void Clean() //readPos 와 writePos 를 초기화 ( 0위치 )
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                //남은 데이터가 없다면 위치 초기화
                readPos = writePos = 0;
            }
            else
            {
                //인자 (위치를 옴길 데이터 배열 , 옴길려고 하는 데이터 시작위치 , 옴길려고 하는 데이터배열을 받는 배열 , 받을 데이터배열 시작위치 , 크기);
                Array.Copy(buffer.Array , buffer.Offset + readPos ,buffer.Array , buffer.Offset , dataSize);
                readPos = 0;
                writePos = dataSize;
            }
        }

        public bool OnRead(int _numOfBytes) //커서 위치 이동 함수 , 데이터를 가공 후 성공했다면 호출 할 함수
        {
            //보낸 데이터가 가지고 있는 데이터보다 크다면 
            if (_numOfBytes > DataSize)
                return false;

            readPos += _numOfBytes;
            return true;
        }

        public bool OnWrite(int _numOfBytes)
        {
            if (_numOfBytes > FreeSize)
                return false;

            writePos += _numOfBytes;
                return true;
        }
    }
}
