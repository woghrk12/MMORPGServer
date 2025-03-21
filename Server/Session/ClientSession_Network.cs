using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Server.Data;
using Server.Game;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public partial class ClientSession
    {
        #region Variables

        private static readonly int HEADER_SIZE = 2;

        private Socket socket = null;

        private int isDisconnected = 0;

        private object lockObj = new();

        private SocketAsyncEventArgs sendArgs = new();
        private Queue<ArraySegment<byte>> sendQueue = new();
        private List<ArraySegment<byte>> pendingList = new();

        private SocketAsyncEventArgs recvArgs = new();
        private RecvBuffer recvBuffer = new(65535);

        #endregion Variables

        #region Methods

        private void Disconnect()
        {
            if (Interlocked.Exchange(ref isDisconnected, 1) == 1) return;

            OnDisconnected(socket.RemoteEndPoint);
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

        public void Send(IMessage packet)
        {
            EMessageID packetID = (EMessageID)Enum.Parse(typeof(EMessageID), packet.Descriptor.Name);
            ushort size = (ushort)packet.CalculateSize();

            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (lockObj)
            {
                sendQueue.Enqueue(sendBuffer);

                if (pendingList.Count == 0)
                {
                    RegisterSend();
                }
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
                if (socket.ReceiveAsync(recvArgs) == true) return;

                OnReceiveCompleted(null, recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed. {e}");
            }
        }

        private void RegisterSend()
        {
            if (isDisconnected == 1) return;

            while (sendQueue.Count > 0)
            {
                pendingList.Add(sendQueue.Dequeue());
            }

            sendArgs.BufferList = pendingList;

            try
            {
                if (socket.SendAsync(sendArgs) == true) return;

                OnSendCompleted(null, sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed. {e}");
            }
        }

        #region Events

        private void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Session ID : {SessionID}\nOnConnected : {endPoint}");

            ClientState = EClientState.Connected;

            ConnectedResponse packet = new();
            List<CharacterStat> characterStatList = [.. DataManager.CharacterStatDictionary.Values];
            packet.Stats.Add(new StatData() { StatType = EStatType.CharacterData, Data = JsonConvert.SerializeObject(characterStatList) });
            List<MonsterStat> monsterStatList = [.. DataManager.MonsterStatDictionary.Values];
            packet.Stats.Add(new StatData() { StatType = EStatType.MonsterData, Data = JsonConvert.SerializeObject(monsterStatList) });
            List<AttackStat> attackStatList = [.. DataManager.AttackStatDictionary.Values];
            packet.Stats.Add(new StatData() { StatType = EStatType.AttackData, Data = JsonConvert.SerializeObject(attackStatList) });
            List<ProjectileStat> projectileStatList = [.. DataManager.ProjectileStatDictionary.Values];
            packet.Stats.Add(new StatData() { StatType = EStatType.ProjectileData, Data = JsonConvert.SerializeObject(projectileStatList) });
            List<ItemStat> itemStatList = [.. DataManager.ItemStatDictionary.Values];
            packet.Stats.Add(new StatData() { StatType = EStatType.ItemData, Data = JsonConvert.SerializeObject(itemStatList) });

            Send(packet);
        }

        private void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"Session ID : {SessionID}\nOnDisconnected : {endPoint}");

            SessionManager.Instance.Remove(this);

            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.LeaveRoom, Character.ID);
        }

        private int OnReceive(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                if (buffer.Count < HEADER_SIZE) break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                PacketManager.Instance.OnReceivePacket(this, new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
            {
                Console.WriteLine($"OnReceiveCompleted Failed.\nBytes Transferred : {args.BytesTransferred}.\nState : {args.SocketError}");
                Disconnect();
                return;
            }

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
                Console.WriteLine($"OnReceiveCompleted Failed. {e}");
            }
        }

        private void OnSend(int numBytes)
        {

        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (lockObj)
            {
                if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
                {
                    Console.WriteLine($"OnSendCompleted Failed.\nBytes Transferred : {args.BytesTransferred}.\nState : {args.SocketError}");
                    Disconnect();
                    return;
                }

                try
                {
                    sendArgs.BufferList = null;
                    pendingList.Clear();

                    OnSend(args.BytesTransferred);

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
        }

        #endregion Events

        #endregion Methods
    }
}