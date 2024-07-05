namespace ServerCore
{
    public class RecvBuffer
    {
        #region Variables

        private ArraySegment<byte> buffer = null;

        private int readPos = -1;
        private int writePos = -1;

        #endregion Variables

        #region Properties

        public int DataSize => writePos - readPos;

        public int FreeSize => buffer.Count - writePos;

        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(buffer.Array, buffer.Offset + readPos, DataSize);

        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(buffer.Array, buffer.Offset + writePos, FreeSize);

        #endregion Properties

        #region Constructor

        public RecvBuffer(int bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        #endregion Constructor

        #region Methods

        public void Clear()
        {
            if (DataSize == 0)
            {
                readPos = writePos = 0;
            }
            else
            {
                Array.Copy(buffer.Array, buffer.Offset + readPos, buffer.Array, buffer.Offset, DataSize);

                readPos = 0;
                writePos = DataSize;
            }
        }

        #region Events

        public bool OnRead(int numBytes)
        {
            if (numBytes > DataSize) return false;

            readPos += numBytes;

            return true;
        }

        public bool OnWrite(int numBytes)
        {
            if (numBytes > FreeSize) return false;

            writePos += numBytes;

            return true;
        }

        #endregion Events

        #endregion Methods
    }
}