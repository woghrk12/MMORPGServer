using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks.Sources;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        #region Variables

        private static readonly int HEADER_SIZE = 2;

        #endregion Variables

        #region Methods

        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                if (buffer.Count < HEADER_SIZE) break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                OnReceivePacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnReceivePacket(ArraySegment<byte> buffer);

        #endregion Methods
    }

    public abstract class Session
    {
        #region  Variables

        private Socket socket = null;

        private int isDisconnected = 0;

        private object lockObj = new();

        private SocketAsyncEventArgs sendArgs = new();
        private Queue<ArraySegment<byte>> sendQueue = new();
        private List<ArraySegment<byte>> pendingList = new();

        private SocketAsyncEventArgs recvArgs = new();

        private RecvBuffer recvBuffer = new RecvBuffer(65535);

        #endregion Variables

        #region Methods

        public void Init(Socket socket)
        {
            this.socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (lockObj)
            {
                sendQueue.Enqueue(sendBuff);

                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0) return;

            lock (lockObj)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                {
                    sendQueue.Enqueue(sendBuff);
                }

                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref isDisconnected, 1) == 1) return;

            OnDisconnected(socket.RemoteEndPoint);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Clear();
        }

        private void Clear()
        {
            lock (lockObj)
            {
                sendQueue.Clear();
                pendingList.Clear();
            }
        }

        private void RegisterSend()
        {
            if (isDisconnected == 1) return;

            while (sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = sendQueue.Dequeue();
                pendingList.Add(buff);
            }

            sendArgs.BufferList = pendingList;

            try
            {
                if (socket.SendAsync(sendArgs)) return;

                OnSendCompleted(null, sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed. {e}");
            }
        }

        private void RegisterRecv()
        {
            if (isDisconnected == 1) return;

            recvBuffer.Clear();

            ArraySegment<byte> segment = recvBuffer.WriteSegment;
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                if (socket.ReceiveAsync(recvArgs)) return;

                OnRecvCompleted(null, recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed. {e}");
            }
        }

        #region Events

        public abstract void OnConnected(EndPoint endPoint);

        public abstract void OnSend(int numBytes);

        public abstract int OnReceive(ArraySegment<byte> buffer);

        public abstract void OnDisconnected(EndPoint endPoint);

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (lockObj)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSend(sendArgs.BytesTransferred);

                        if (sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed. {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    if (recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLen = OnReceive(recvBuffer.ReadSegment);
                    if (processLen < 0 || processLen > recvBuffer.DataSize)
                    {
                        Disconnect();
                        return;
                    }

                    if (recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed. {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }

        #endregion Events

        #endregion Methods
    }
}