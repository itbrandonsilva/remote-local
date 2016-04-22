using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Concurrent;

using System.Net;
using System.Net.Sockets;

namespace RLocal
{
    public struct RLocalClient
    {
        public TcpClient tcp;
        public NetworkStream stream;
        public int playerId;
    }

    public struct RLocalIncomingMessage
    {
        public byte[] bytes;
        public int playerId;
        public int type;
    }

    class RLocalConnection
    {
        TcpListener m_server;
        RLocalClient m_client;
        public byte[] ReadBuffer;
        public byte[] m_writeBuffer;
        //public IntPtr m_readBufferPtr;
        //public IntPtr m_writeBufferPtr;
        List<RLocalClient> m_clients;
        public bool isServer;
        List<int> playerIds;

        public BlockingCollection<RLocalIncomingMessage> ReadHandlerQueue;

        //Action<Client> NewClientHandler;
        public int packetsSent = 0;
        //IntPtr m_writeBufferPtr;

        public RLocalConnection(int bufferSize=9999999)
        {
            ReadHandlerQueue = new BlockingCollection<RLocalIncomingMessage>();
            ReadBuffer = new Byte[8];
            m_writeBuffer = new Byte[bufferSize];

            //GCHandle pinned = GCHandle.Alloc(m_writeBuffer, GCHandleType.Pinned);
            //m_writeBufferPtr = pinned.AddrOfPinnedObject();

            playerIds = new List<int>();
            m_clients = new List<RLocalClient>();
        }

        public void StartServer(IPAddress address, int port, RunWorkerCompletedEventHandler ReceiveClient)
        {
            isServer = true;
            m_server = new TcpListener(address, port);
            m_server.Start();

            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs eargs) =>
            {
                RLocalClient client = ReceiveNextClient();
                eargs.Result = client;
            });
            worker.RunWorkerCompleted += ReceiveClient;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                worker.RunWorkerAsync();
            });
            worker.RunWorkerAsync();
        }

        public void StartClient(IPAddress host, int port, RunWorkerCompletedEventHandler PacketHandler)
        {
            isServer = false;
            var client = new TcpClient(host.ToString(), port);
            m_client = BuildClient(client, 0);

            StartReaderWorker(m_client, PacketHandler);
        }

        public void BroadcastBytes(byte[] bytes, int size)
        {
            if (bytes == null) bytes = m_writeBuffer;
            m_clients.ForEach(delegate (RLocalClient c)
            {
                c.stream.Write(bytes, 0, size);
            });
        }

        public void WriteBytes(byte[] bytes, int size)
        {
            if (bytes == null) bytes = m_writeBuffer;
            NetworkStream stream = m_client.stream;
            stream.Write(bytes, 0, size);
        }

        public int GetAvailablePlayerId()
        {
            int playerId = 1;
            while (playerIds.Contains(playerId))
            {
                ++playerId;
            }
            playerIds.Add(playerId);
            return playerId;
        }

        private RLocalClient ReceiveNextClient()
        {
            TcpClient tcpClient = m_server.AcceptTcpClient();
            NetworkStream stream = tcpClient.GetStream();

            var client = BuildClient(tcpClient, GetAvailablePlayerId());
            m_clients.Add(client);

            return client;
        }

        public RLocalClient BuildClient(TcpClient tcpClient, int playerId)
        {
            var client = new RLocalClient();
            client.tcp = tcpClient;
            client.stream = tcpClient.GetStream();
            client.playerId = playerId;
            return client;
        }

        public void StartReaderWorker(RLocalClient client, RunWorkerCompletedEventHandler PacketHandler)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs eargs) =>
            {
                byte[] bytes = null;
                ReadBytes(client, out bytes);
                if (bytes == null) return;

                RLocalIncomingMessage message = new RLocalIncomingMessage();
                message.bytes = bytes;
                message.playerId = client.playerId;
                message.type = BitConverter.ToInt32(bytes, 0);

                eargs.Result = message;
            });
            worker.RunWorkerCompleted += PacketHandler;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                if (e.Result == null) return;
                worker.RunWorkerAsync();
            });
            worker.RunWorkerAsync();
        }

        public void ReadBytes(RLocalClient client, out byte[] bytes)
        {
            try {
                client.stream.Read(ReadBuffer, 0, 8);
                int sizeOfPacket = BitConverter.ToInt32(ReadBuffer, 4);

                if (ReadBuffer.Length < 8 + sizeOfPacket) Array.Resize<byte>(ref ReadBuffer, 8 + sizeOfPacket);

                int amountRead = 0;
                while (amountRead < sizeOfPacket)
                {
                    amountRead += client.stream.Read(ReadBuffer, amountRead + 8, sizeOfPacket - amountRead);
                }

                bytes = new byte[8 + amountRead];
                Array.Copy(ReadBuffer, bytes, 8 + amountRead);
            } catch
            {
                bytes = null;
                m_clients.Remove(client);
            }
        }
    }
}
