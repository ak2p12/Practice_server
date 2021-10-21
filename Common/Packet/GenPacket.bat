start ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
xcopy /y GenPackets.cs "../../DummyClient/Packet"
xcopy /y GenPackets.cs "../../Server/Packet"