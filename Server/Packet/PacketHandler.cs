using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"이름 : {p.name} \nPlayerID : {p.playerID} ");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"skill ({skill.id})({skill.level})({skill.duration})");
            }
        }
    }
}
