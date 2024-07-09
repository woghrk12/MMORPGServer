using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum EPacketID
{
    CLIENT_CHAT = 1,
	SERVER_CHAT = 2,
	
}

public interface IPacket
{
    public ushort Protocol { get; }

    public void Read(ArraySegment<byte> segment);
    public ArraySegment<byte> Write();
}

public class ClientChat : IPacket
{
    #region Variables

    public string chat;

    #endregion Variables

    #region Properties

    public ushort Protocol => (ushort)EPacketID.CLIENT_CHAT;

    #endregion Properties

    #region Methods

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool isSuccess = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)EPacketID.CLIENT_CHAT);
        count += sizeof(ushort);

        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        isSuccess &= BitConverter.TryWriteBytes(span, count);

        if (isSuccess == false) return null;

        return SendBufferHelper.Close(count);
    }

    #endregion Methods
}

public class ServerChat : IPacket
{
    #region Variables

    public int playerId;
	public string chat;

    #endregion Variables

    #region Properties

    public ushort Protocol => (ushort)EPacketID.SERVER_CHAT;

    #endregion Properties

    #region Methods

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(span.Slice(count, span.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool isSuccess = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)EPacketID.SERVER_CHAT);
        count += sizeof(ushort);

        isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		isSuccess &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        isSuccess &= BitConverter.TryWriteBytes(span, count);

        if (isSuccess == false) return null;

        return SendBufferHelper.Close(count);
    }

    #endregion Methods
}


