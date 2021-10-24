using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;
        static ushort packetId;
        static string packetEnums;

        static string clientRigister;
        static string serverRigister;
        
        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true , //주석 무시
                IgnoreWhitespace = true //띄어쓰기 무시
            };

            if (args.Length >= 1)
            {
                pdlPath = args[0];
            }

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent(); //헤더를 건너띄고 본내용으로 이동
                while (r.Read())
                {
                    //
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }

                string fileText = string.Format(PacketFormat.fileFormat , packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText); //파일로 저장 및 생성

                string clientManagerText = string.Format(PacketFormat.managerFormat , clientRigister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText); //파일로 저장 및 생성

                string serverManagerText = string.Format(PacketFormat.managerFormat, serverRigister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText); //파일로 저장 및 생성
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

            Tuple<string,string,string> t = ParseMemvers(_r);
            genPackets += string.Format(PacketFormat.packetFormat,packetName, t.Item1, t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.packetEnumFormat , packetName , ++packetId) + Environment.NewLine + "\t";

            if (packetName.StartsWith("S_") || packetName.StartsWith("_s"))
            {
                clientRigister += string.Format(PacketFormat.managerRigisgerFormat, packetName) + Environment.NewLine;
            }
            else
            {
                serverRigister += string.Format(PacketFormat.managerRigisgerFormat, packetName) + Environment.NewLine;
            }
                
        }

        public static Tuple<string,string,string> ParseMemvers(XmlReader _r)
        {
            string packetName = _r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = _r.Depth + 1;
            while(_r.Read())
            {
                if (_r.Depth != depth)
                    break;

                string memberName = _r["name"];

                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("mamberName 이름 없음");
                    return null;
                }

                //IsNullOrEmpty 매게변수가 비어있다면 true반환
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;

                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;

                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                string memberType = _r.Name.ToLower();

                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat , memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat , memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat , memberName,memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat , memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(_r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n" ,"\n\t");
            readCode = readCode.Replace("\n" ,"\n\t\t");
            writeCode = writeCode.Replace("\n" ,"\n\t\t");

            return new Tuple<string, string, string>(memberCode , readCode , writeCode);
        }

        public static Tuple<string,string,string> ParseList(XmlReader _r)
        {
            string listName = _r["name"];

            if(string.IsNullOrEmpty(listName)) //listName이 비어있다면
            {
                Console.WriteLine("listName 이름 없음");
                return null;
            }

            Tuple<string, string, string> t = ParseMemvers(_r);

            string memberCode = string.Format(PacketFormat.memberListFormat, FirstCharToUpper(listName),FirstCharToLower(listName),t.Item1 , t.Item2 , t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat , FirstCharToUpper(listName), FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string _memberType)
        {
            switch(_memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string _input) //첫번째 글자를 대문자로 바꿔주는 함수
        {
            if (string.IsNullOrEmpty(_input))
            {
                return "";
            }

            //Substring 특정한구간의 문자열 추출
            //Substring(1) 두번째 문자열부터 추출 [0] [1] [2] [3] [4] 

            return _input[0].ToString().ToUpper() + _input.Substring(1); //첫번째 글자를 대문자로 바꾼후 나머지 부분을 반환한다.
        }

        public static string FirstCharToLower(string _input) //첫번째 글자를 소문자로 바꿔주는 함수
        {
            if (string.IsNullOrEmpty(_input))
            {
                return "";
            }

            //Substring 특정한구간의 문자열 추출
            //Substring(1) 두번째 문자열부터 추출 [0] [1] [2] [3] [4] 

            return _input[0].ToString().ToLower() + _input.Substring(1); //첫번째 글자를 소문자로 바꾼후 나머지 부분을 반환한다.
        }
    }
}
