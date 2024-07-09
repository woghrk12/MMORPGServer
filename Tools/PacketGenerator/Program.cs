using System.Xml;

namespace PacketGenerator
{
    public class Program
    {
        private static string generatedPacket = string.Empty;
        private static ushort packetId = 0;
        private static string generatedEnumList = string.Empty;

        private static string generatedClientRegister = string.Empty;
        private static string generatedServerRegister = string.Empty;

        private static string generatedClientPacketHandler = string.Empty;
        private static string generatedServerPacketHandler = string.Empty;

        static void Main(string[] args)
        {
            string pdlPath = "PDL.xml";

            XmlReaderSettings settings = new()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            if (args.Length >= 1)
            {
                Console.WriteLine(args[0]);
                pdlPath = args[0];
            }

            using (XmlReader reader = XmlReader.Create(pdlPath, settings))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(reader);
                    }
                }

                string fileText = string.Format(PacketFormat.FILE_FORMAT, generatedEnumList, generatedPacket);
                File.WriteAllText("GeneratedPackets.cs", fileText);

                string clientManagerText = string.Format(PacketFormat.MANAGER_FORMAT, "DummyClient", generatedClientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
                string serverManagerText = string.Format(PacketFormat.MANAGER_FORMAT, "Server", generatedServerRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);

                string clinetHandlerText = string.Format(PacketFormat.HANDLER_FORMAT, "DummyClient", generatedClientPacketHandler);
                File.WriteAllText("ClientPacketHandler.cs", clinetHandlerText);
                string serverHandlerText = string.Format(PacketFormat.HANDLER_FORMAT, "Server", generatedServerPacketHandler);
                File.WriteAllText("ServerPacketHandler.cs", serverHandlerText);
            }
        }

        private static void ParsePacket(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.EndElement) return;

            if (reader.Name.ToLower().Equals("packet") == false)
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = reader["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            string usage = reader["usage"];
            if (string.IsNullOrEmpty(usage))
            {
                throw new Exception($"There is no \"usage\" element. You need to add a \"usage\" element as either server or client. Packet : {packetName}");
            }

            Tuple<string, string, string> tuple = ParseMembers(reader);
            generatedPacket += string.Format(PacketFormat.PACKET_FORMAT, packetName, tuple.Item1, ConvertStringToEnumElementName(packetName), tuple.Item2, tuple.Item3) + "\n";
            generatedEnumList += string.Format(PacketFormat.PACKET_ENUM_FORMAT, ConvertStringToEnumElementName(packetName), ++packetId) + "\n\t";

            if (usage.Equals("Client") == true || usage.Equals("client") == true)
            {
                generatedServerRegister += string.Format(PacketFormat.MANAGER_CREATE_HANDLER_FORMAT, packetName, ConvertStringToEnumElementName(packetName)) + "\n\t\t\t";
                generatedServerRegister += string.Format(PacketFormat.MANAGER_REGISTER_HANDLER_FORMAT, packetName, ConvertStringToEnumElementName(packetName)) + "\n\t\t\t";
                generatedServerPacketHandler += string.Format(PacketFormat.HANDLE_PACKET_FORMAT, packetName);
            }
            else if (usage.Equals("Server") == true || usage.Equals("server") == true)
            {
                generatedClientRegister += string.Format(PacketFormat.MANAGER_CREATE_HANDLER_FORMAT, packetName, ConvertStringToEnumElementName(packetName)) + "\n\t\t\t";
                generatedClientRegister += string.Format(PacketFormat.MANAGER_REGISTER_HANDLER_FORMAT, packetName, ConvertStringToEnumElementName(packetName)) + "\n\t\t\t";
                generatedClientPacketHandler += string.Format(PacketFormat.HANDLE_PACKET_FORMAT, packetName);
            }
            else
            {
                throw new Exception($"You need to specify the usage purpose as either server or client. Packet : {packetName}");
            }

        }

        private static Tuple<string, string, string> ParseMembers(XmlReader reader)
        {
            string packetName = reader["name"];
            string memberCode = string.Empty;
            string readCode = string.Empty;
            string writeCode = string.Empty;

            int depth = reader.Depth + 1;
            while (reader.Read())
            {
                if (reader.Depth != depth) break;

                string memberName = reader["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                {
                    memberCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(readCode) == false)
                {
                    readCode += Environment.NewLine;
                }
                if (string.IsNullOrEmpty(writeCode) == false)
                {
                    writeCode += Environment.NewLine;
                }

                string memberType = reader.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.MEMBER_FORMAT, memberType, memberName);
                        readCode += string.Format(PacketFormat.READ_BYTE_FORMAT, memberName, memberType);
                        writeCode += string.Format(PacketFormat.WRITE_BYTE_FORMAT, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.MEMBER_FORMAT, memberType, memberName);
                        readCode += string.Format(PacketFormat.READ_FORMAT, memberName, GenerateToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.WRITE_FORMAT, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.MEMBER_FORMAT, memberType, memberName);
                        readCode += string.Format(PacketFormat.READ_STRING_FORMAT, memberName);
                        writeCode += string.Format(PacketFormat.WRITE_STRING_FORMAT, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> tuple = ParseList(reader);
                        memberCode += tuple.Item1;
                        readCode += tuple.Item2;
                        writeCode += tuple.Item3;
                        break;

                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        private static Tuple<string, string, string> ParseList(XmlReader reader)
        {
            string listName = reader["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> tuple = ParseMembers(reader);
            string memberCode = string.Format(PacketFormat.MEMBER_LIST_FORMAT,
                ChangeFirstCharToUpper(listName),
                ChangeFirstCharToLower(listName),
                tuple.Item1,
                tuple.Item2,
                tuple.Item3);
            string readCode = string.Format(PacketFormat.READ_LIST_FORMAT,
                ChangeFirstCharToUpper(listName),
                ChangeFirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.WRITE_LIST_FORMAT,
                ChangeFirstCharToUpper(listName),
                ChangeFirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        private static string GenerateToMemberType(string memberType)
        {
            switch (memberType)
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
            }

            Console.WriteLine($"Cannot find a matching type. {memberType}");
            return "";
        }

        private static string ChangeFirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        private static string ChangeFirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input.Substring(0, 1).ToLower() + input.Substring(1);
        }

        private static string ConvertStringToEnumElementName(string input)
        {
            string newString = string.Empty;
            int length = input.Length;

            for (int index = 0; index < length; index++)
            {
                if (index != 0 && char.IsUpper(input[index]) == true)
                {
                    newString += "_";
                }

                newString += char.ToUpper(input[index]);
            }

            return newString;
        }
    }
}