using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true , //주석 무시
                IgnoreWhitespace = true //띄어쓰기 무시
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent(); //헤더를 건너띄고 본내용으로 이동
            }
        }
    }
}
