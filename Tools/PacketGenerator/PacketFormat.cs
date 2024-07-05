namespace PacketGenerator
{
    public class PacketFormat
    {
        /// <summary>
        /// {0} : The name of the packet to be generated. <br/>
        /// {1} : Member variables. <br/>
        /// {2} : The logic to read the member variables from the packet. <br/>
        /// {3} : The logic to write the member variables to the packet. <br/>
        /// </summary>
        public static string PACKET_FORMAT =
@"
public class {0}
{{
    #region Variables

    {1}

    #endregion Variables

    #region Methods

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool isSuccess = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}

        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (isSuccess == false) return false;

        return SendBufferHelper.Close(count);
    }

    #endregion Methods
}}
";

        /// <summary>
        /// {0} : The type of the member variable. <br/>
        /// {1} : The name of the member variable. <br/>
        /// </summary>
        public static string MEMBER_FORMAT =
@"public {0} {1};";

        /// <summary>
        /// {0} : The name of the member variable to read from the packet. <br/>
        /// {1} : The name of the method to convert a byte array to the type of the member variable. <br/>
        /// {2} : The type of the member variable to read from the packet.
        /// </summary>
        public static string READ_FORMAT =
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

        /// <summary>
        /// {0} : The name of the member variable of type string to read from the packet.
        /// </summary>
        public static string READ_STRING_FORMAT =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";

        /// <summary>
        /// {0} : The name of the member variable to write to the packet. <br/>
        /// {1} : The type of the member variable to write to the packet.
        /// </summary>
        public static string WRITE_FORMAT =
@"isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0}));
count += sizeof({1});";

        /// <summary>
        /// {0} : The name of the member variable of type string to write to the packet.
        /// </summary>
        public static string WRITE_STRING_FORMAT =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count, {0}Len));
count += sizeof(ushort);
count += {0}Len;";
    }
}