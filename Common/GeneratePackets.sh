#!/bin/bash

# Navigate to the directory containing the project
cd ../Tools/PacketGenerator

# build the project
dotnet build

# Navigate to the directory containing the built executable
cd ./bin

# Execute the C# program
./PacketGenerator ../PDL.xml

# Copy the C# file to DummyClient folder
cp GeneratedPackets.cs ../../../DummyClient/Packet
cp ClientPacketManager.cs ../../../DummyClient/Packet
cp ClientPacketHandler.cs ../../../DummyClient/Packet

# Copy the C# file to Server folder
cp GeneratedPackets.cs ../../../Server/Packet
cp ServerPacketManager.cs ../../../Server/Packet
cp ServerPacketHandler.cs ../../../Server/Packet