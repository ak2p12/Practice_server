start ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
xcopy /y GenPackets.cs "../../Client/Assets/Scripts/Packet"
xcopy /y GenPackets.cs "../../DummyClient/Packet"
xcopy /y GenPackets.cs "../../Server/Packet"

xcopy /y ClientPacketManager.cs "../../Client/Assets/Scripts/Packet"
xcopy /y ClientPacketManager.cs "../../DummyClient/Packet"
xcopy /y ServerPacketManager.cs "../../Server/Packet"