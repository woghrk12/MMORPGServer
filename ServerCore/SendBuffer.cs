namespace ServerCore
{
    public class SendBufferHelper
    {
        #region Properties

        public static ThreadLocal<SendBuffer> CurrentBuffer { set; get; } = new(() => { return null; });

        public static int ChunckSize { set; get; } = 4096;

        #endregion Properties

        #region Methods

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (ReferenceEquals(CurrentBuffer.Value, null))
            {
                CurrentBuffer.Value = new SendBuffer(ChunckSize);
            }

            if (CurrentBuffer.Value.FreeSize < reserveSize)
            {
                CurrentBuffer.Value = new SendBuffer(ChunckSize);
            }

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }

        #endregion Methods
    } 

    public class SendBuffer
    {
        #region Variables

        private byte[] buffer = null;

        private int usedSize = -1;

        #endregion Variables

        #region Properties

        public int FreeSize => buffer.Length - usedSize;

        #endregion Properties

        #region Constructor

        public SendBuffer(int chunckSize)
        {
            buffer = new byte[chunckSize];
        }

        #endregion Constructor

        #region Methods

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize) return null;

            return new ArraySegment<byte>(buffer, usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, this.usedSize, usedSize);

            this.usedSize += usedSize;

            return segment;
        }

        #endregion Methods 
    }
}