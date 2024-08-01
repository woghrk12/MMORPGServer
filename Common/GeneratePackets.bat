@echo off

:: Create the protocol C# files
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto

:: Copy the protocol C# file to the folder
copy "./Protocol.cs" "../Server/Packet"
copy "./Protocol.cs" "../Client/Packet"

:: Remove the original protocol C# file
del "./Protocol.cs"

pause