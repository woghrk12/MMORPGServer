#!/bin/bash

# Navigate to the directory containing the built executable
cd ../Tools/PacketGenerator/bin

# Execute the C# program
./PacketGenerator ../PDL.xml

# Copy the C# file to DummyClient folder
cp GeneratedPackets.cs ../../../DummyClient/Packet

# Copy the C# file to Server folder
cp GeneratedPackets.cs ../../../Server/Packet