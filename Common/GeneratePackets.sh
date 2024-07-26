#!/bin/bash

# Create the protocol C# files
protoc -I=./ --csharp_out=./ Protocol.proto

# Copy the protocol C# file to the folder
cp Protocol.cs ../Server/Packet
cp Protocol.cs ../Client/Packet

# Remove the original protocol C# file
rm ./Protocol.cs