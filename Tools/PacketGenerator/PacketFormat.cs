namespace PacketGenerator
{
    public class PacketFormat
    {
        /// <summary>
        /// {0} : The usage of the manager class. (server or client) <br/>
        /// {1} : The logic to register the packet handler to the manager. <br/>
        /// </summary>
        public static readonly string MANAGER_FORMAT =
@"using System;
using System.Collections.Generic;
using ServerCore;

namespace {0}
{{
    public class PacketManager
    {{
        #region Variables

        private static PacketManager instance = new();

        private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> receivedPacketHandlerDict = new();
        private Dictionary<ushort, Action<PacketSession, IPacket>> handlerDict = new();

        #endregion Variables

        #region Properties

        public static PacketManager Instance => instance;

        #endregion Properties   

        #region Constructor

        public PacketManager()
        {{
            Register();
        }}

        #endregion Constructor

        #region Methods

        public void Register()
        {{
            {1}
        }}

        private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {{
            T packet = new();
            packet.Read(buffer);

            if (handlerDict.TryGetValue(packet.Protocol, out Action<PacketSession, IPacket> action))
            {{
                action.Invoke(session, packet);
            }}
        }}

        #region Events

        public void OnReceivePacket(PacketSession session, ArraySegment<byte> buffer)
        {{
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            if (receivedPacketHandlerDict.TryGetValue(id, out Action<PacketSession, ArraySegment<byte>> action))
            {{
                action.Invoke(session, buffer);
            }}
        }}
    
        #endregion Events

        #endregion Methods
    }}
}}
";

        /// <summary>
        /// {0} : The classes representing the packet. <br/>
        /// {1} : The enum elements representing the index of the packet. <br/>
        /// </summary>
        public static readonly string MANAGER_CREATE_HANDLER_FORMAT =
@"receivedPacketHandlerDict.Add((ushort)EPacketID.{1}, MakePacket<{0}>);";

        /// <summary>
        /// {0} : The classes representing the packet. <br/>
        /// {1} : The enum elements representing the index of the packet. <br/>
        /// </summary>
        public static readonly string MANAGER_REGISTER_HANDLER_FORMAT =
@"handlerDict.Add((ushort)EPacketID.{1}, PacketHandler.Handle{0});";

        /// <summary>
        /// {0} : The usage of the manager class. (server or client) <br/>
        /// {1} : The logic to handle the registered packets. <br/>
        /// </summary>
        public static readonly string HANDLER_FORMAT =
@"using System;
using ServerCore;

namespace {0}
{{
    public class PacketHandler
    {{
        {1}
    }}
}}
";

        /// <summary>
        /// {0} : The name of the packet to be handled. <br/>
        /// </summary>
        public static readonly string HANDLE_PACKET_FORMAT =
@"  
        public static void Handle{0}(PacketSession session, IPacket packet)
        {{
            throw new NotImplementedException();
        }}
";

        /// <summary>
        /// {0} : The enum elements representing the index of the packet. <br/>
        /// {1} : The classes representing the packet. <br/>
        /// </summary>
        public static readonly string FILE_FORMAT =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum EPacketID
{{
    {0}
}}

public interface IPacket
{{
    public ushort Protocol {{ get; }}

    public void Read(ArraySegment<byte> segment);
    public ArraySegment<byte> Write();
}}

{1}
";

        /// <summary>
        /// {0} : The packet name with all letters in uppercase and spaces replaced by underscores. <br/>
        /// {1} : The index of the packet. <br/>
        /// </summary>
        public static readonly string PACKET_ENUM_FORMAT =
@"{0} = {1},";

        /// <summary>
        /// {0} : The name of the packet to be generated. <br/>
        /// {1} : Member variables. <br/>
        /// {2} : The element of the enum representing the packet. <br/>
        /// {3} : The logic to read the member variables from the packet. <br/>
        /// {4} : The logic to write the member variables to the packet. <br/>
        /// </summary>
        public static readonly string PACKET_FORMAT =
@"public class {0} : IPacket
{{
    #region Variables

    {1}

    #endregion Variables

    #region Properties

    public ushort Protocol => (ushort)EPacketID.{2};

    #endregion Properties

    #region Methods

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {3}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool isSuccess = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)EPacketID.{2});
        count += sizeof(ushort);

        {4}

        isSuccess &= BitConverter.TryWriteBytes(span, count);

        if (isSuccess == false) return null;

        return SendBufferHelper.Close(count);
    }}

    #endregion Methods
}}
";

        /// <summary>
        /// {0} : The type of the member variable. <br/>
        /// {1} : The name of the member variable. <br/>
        /// </summary>
        public static readonly string MEMBER_FORMAT =
@"public {0} {1};";

        /// <summary>
        /// {0} : The name of the class, starting with a capital letter, to be used as the element type of the list. <br/>
        /// {1} : The name of the class, starting with a lowercase letter, to be used as the element type of the list. <br/>
        /// {2} : Member variables. <br/>
        /// {3} : The logic to read the member variables from the packet. <br/>
        /// {4} : The logic to write the member variables to the packet. <br/>
        /// </summary>
        public static readonly string MEMBER_LIST_FORMAT =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> span, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort count)
    {{
        bool isSuccess = true;

        {4}

        return isSuccess;
    }}
}}

public List<{0}> {1}List {{ private set; get; }} = new();";

        /// <summary>
        /// {0} : The name of the member variable to read from the packet. <br/>
        /// {1} : The name of the method to convert a byte array to the type of the member variable. <br/>
        /// {2} : The type of the member variable to read from the packet. <br/>
        /// </summary>
        public static readonly string READ_FORMAT =
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

        /// <summary>
        /// {0} : The name of the member variable to read from the packet. <br/>
        /// {1} : The type of the member variable to read from the packet. <br/>
        /// </summary>
        public static readonly string READ_BYTE_FORMAT =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1}});";

        /// <summary>
        /// {0} : The name of the member variable of type string to read from the packet. <br/>
        /// </summary>
        public static readonly string READ_STRING_FORMAT =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";


        /// <summary>
        /// {0} : The name of the struct, starting with a capital letter, to be used as the element type of the list. <br/>
        /// {1} : The name of the struct, starting with a lowercase letter, to be used as the element type of the list. <br/>
        /// </summary>
        public static readonly string READ_LIST_FORMAT =
@"this.{1}List.Clear();
ushort {1}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
for (int index = 0; index < {1}Len; index++)
{{
    {0} {1} = new();
    {1}.Read(span, ref count);
    {1}List.Add({1});
}}";

        /// <summary>
        /// {0} : The name of the member variable to write to the packet. <br/>
        /// {1} : The type of the member variable to write to the packet. <br/>
        /// </summary>
        public static readonly string WRITE_FORMAT =
@"isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        /// <summary>
        /// {0} : The name of the member variable to write to the packet. <br/>
        /// {1} : The type of the member variable to write to the packet. <br/>
        /// </summary>
        public static readonly string WRITE_BYTE_FORMAT =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1}});";

        /// <summary>
        /// {0} : The name of the member variable of type string to write to the packet. <br/>
        /// </summary>
        public static readonly string WRITE_STRING_FORMAT =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        /// <summary>
        /// {0} : The name of the struct, starting with a capital letter, to be used as the element type of the list. <br/>
        /// {1} : The name of the struct, starting with a lowercase letter, to be used as the element type of the list. <br/>
        /// </summary>
        public static readonly string WRITE_LIST_FORMAT =
@"isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}List.Count);
count += sizeof(ushort);
foreach ({0} {1} in {1}List)
{{
    isSuccess &= {1}.Write(span, ref count);
}}";
    }
}