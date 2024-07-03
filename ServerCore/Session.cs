using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks.Sources;

namespace ServerCore
{
    public abstract class Session
    {
        #region  Variables

        private Socket socket = null;

        private int isDisconnected = 0;

        private object sendLock = new();
        private SocketAsyncEventArgs sendArgs = new();
        private Queue<byte[]> sendQueue = new();
        private List<ArraySegment<byte>> pendingList = new();

        private SocketAsyncEventArgs recvArgs = new();

        #endregion Variables

        #region Methods

        public void Init(Socket socket)
        {
            this.socket = socket;

            SocketAsyncEventArgs recvArgs = new();

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff)
        {
            lock (sendLock)
            {
                sendQueue.Enqueue(sendBuff);
                
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
        }

        private void RegisterSend()
        {
            while (sendQueue.Count > 0)
            {
                byte[] buff = sendQueue.Dequeue();
                pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            if (socket.SendAsync(sendArgs)) return;

            OnSendCompleted(null, sendArgs);
        }

        private void RegisterRecv()
        {
            if (socket.ReceiveAsync(recvArgs)) return;

            OnRecvCompleted(null, recvArgs);
        }

        #region Events

        public abstract void OnConnected(EndPoint endPoint);
       
        public abstract void OnSend(int numBytes);

        public abstract void OnReceive(ArraySegment<byte> buffer);

        public abstract void OnDisconnected(EndPoint endPoint);

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (sendLock)
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
                    OnReceive(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
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