using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;
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
                while (r.Read())
                {
                    //
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    //Console.WriteLine(r.Name + " " + r["name"]);
                }

                File.WriteAllText("GenPackets.cs", genPackets); //파일하나로 만들어줌
            }
        }

        public static void ParsePacket(XmlReader _r)
        {
            if (_r.NodeType == XmlNodeType.EndElement)
                return;

            //ToLower 모든 글자를 소문자로 바꿔준다.
            if (_r.Name.ToLower() != "packet")
            {
                Console.WriteLine("헤더가 packet가 아님");
                return;
            }
                

            string packetName = _r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("packetName 이름 없음");
            }

            ParseMemvers(_r);
        }

        public static void ParseMemvers(XmlReader _r)
        {
            string packetName = _r["name"];

            int depth = _r.Depth + 1;
            while(_r.Read())
            {
                if (_r.Depth != depth)
                    break;

                string mamberName = _r["name"];

                if (string.IsNullOrEmpty(mamberName))
                {
                    Console.WriteLine("mamberName 이름 없음");
                }

                string memberType = _r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                }
            }
        }
    }
}
