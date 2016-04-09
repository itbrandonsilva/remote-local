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

namespace DesktopDup
{
    public struct Client
    {
        public TcpClient tcp;
        public NetworkStream stream;
        public int playerId;
    }

    /*public struct Message
    {
        public byte[] bytes;
        public int size;
    }*/

    public struct RLocalIncomingMessage
    {
        public byte[] bytes;
        public int playerId;
        public int type;
    }

    class RLocalConnection
    {
        TcpListener m_server;
        Client m_client;
        public byte[] ReadBuffer;
        public byte[] m_writeBuffer;
        //public IntPtr m_readBufferPtr;
        //public IntPtr m_writeBufferPtr;
        List<Client> m_clients;
        public bool isServer;
        List<int> playerIds;

        public BlockingCollection<RLocalIncomingMessage> ReadHandlerQueue;

        //Action<Client> NewClientHandler;
        public int packetsSent = 0;

        public RLocalConnection()
        {
            ReadHandlerQueue = new BlockingCollection<RLocalIncomingMessage>();
            ReadBuffer = new Byte[8];
            m_writeBuffer = new Byte[9999999];

            playerIds = new List<int>();
            m_clients = new List<Client>();
        }

        public void StartServer(IPAddress address, int port, RunWorkerCompletedEventHandler ReceiveClient)
        {
            isServer = true;
            m_server = new TcpListener(address, port);
            m_server.Start();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs eargs) =>
            {
                Client client = ReceiveNextClient();
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
            TcpClient client = new TcpClient(host.ToString(), port);
            m_client = BuildClient(client, 0);

            StartReaderWorker(m_client, PacketHandler);
        }

        public void BroadcastBytes(byte[] bytes, int size)
        {
            if (bytes == null) bytes = m_writeBuffer;
            m_clients.ForEach(delegate (Client c)
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

        private Client ReceiveNextClient()
        {
            TcpClient tcpClient = m_server.AcceptTcpClient();
            NetworkStream stream = tcpClient.GetStream();

            Client client = BuildClient(tcpClient, GetAvailablePlayerId());
            m_clients.Add(client);

            return client;
        }

        public Client BuildClient(TcpClient tcpClient, int playerId)
        {
            Client client = new Client();
            client.tcp = tcpClient;
            client.stream = tcpClient.GetStream();
            client.playerId = playerId;
            return client;
        }

        public void StartReaderWorker(Client client, RunWorkerCompletedEventHandler PacketHandler)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs eargs) =>
            {
                byte[] bytes = ReadBytes(client.stream);

                RLocalIncomingMessage message = new RLocalIncomingMessage();
                message.bytes = bytes;
                message.playerId = client.playerId;
                message.type = BitConverter.ToInt32(bytes, 0);

                eargs.Result = message;
            });
            worker.RunWorkerCompleted += PacketHandler;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                worker.RunWorkerAsync();
            });
            worker.RunWorkerAsync();
        }

        public void HandlePacket()
        {

        }

        public byte[] ReadBytes(NetworkStream stream)
        {
            stream.Read(ReadBuffer, 0, 8);
            int sizeOfPacket = BitConverter.ToInt32(ReadBuffer, 4);

            if (ReadBuffer.Length < 8 + sizeOfPacket) Array.Resize<byte>(ref ReadBuffer, 8 + sizeOfPacket);

            int amountRead = 0;
            while (amountRead < sizeOfPacket)
            {
                amountRead += stream.Read(ReadBuffer, amountRead + 8, sizeOfPacket - amountRead);
            }

            byte[] bytes = new byte[8 + amountRead];
            Array.Copy(ReadBuffer, bytes, 8 + amountRead);

            return bytes;
        }
    }
}
