using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;

namespace DCWavStreamer
{
    class DCServer
    {
        /// <summary>
        /// Member variables
        /// 
        /// m_dataBuffer   : retrieves the inbound request from the connecting client
        /// m_clients      : list of connected sockets to serve
        /// m_serverSocket : reference to the socket that maintains a connection to the server via TCP
        /// 
        /// </summary>
        private static byte[]       m_dataBuffer;
        private static List<Socket> m_clients;
        private static Socket       m_serverSocket;

        /// <summary>
        /// Constructor for a new DCServer, accepts a DCConfig structure as a parameters.
        /// The DCConfig structure dictates where the server should run, the port it should run on,
        /// the servers title and other information, such as the size of the input buffer.
        /// 
        /// The default value for an inbound buffer is 256 (default GET request len), however a larger
        /// size can be provided if you know you will be serving larger packets.
        /// 
        /// </summary>
        /// <param name="config">Configuration constants</param>
        public DCServer(DCConfig config)
        {
            // Ensure we can create a connection, if any of the fundamental components are missing
            // then we log an error to the console.
            if (config.SERVER_ADDR == null || config.SERVER_PORT <= 0 || config.INBOUND_BUFFER_SIZE == 0)
            {
                Console.WriteLine("Terminating app, configuration constants not set.");
                return;
            }
            if (config.SERVER_TITLE != null)
                Console.Title = config.SERVER_TITLE;

            // Perform any initialisation of member variables, dependent on values set in 
            // the configuration struct
            m_clients      = new List<Socket>();
            m_dataBuffer   = new byte[config.INBOUND_BUFFER_SIZE];
            m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Attempt to initialise the server by binding the server to the socket, this will be our listener
            // for inbound request.  We also set a backlog of 5 pending connections here, meaning the 6th
            // connection will be rejected if the server queue is full, finally we tell the server to start
            // accepting connections and then perform the recursive server logic in the AcceptCallback func.
            Console.WriteLine("Initialising the server.");
            try
            {
                m_serverSocket.Bind(new IPEndPoint(IPAddress.Parse(config.SERVER_ADDR), config.SERVER_PORT));
                m_serverSocket.Listen(config.BACK_LOG_SIZE); 
                m_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        private static void AcceptCallback(IAsyncResult res)
        {
            try
            {
                var socket = m_serverSocket.EndAccept(res);
                m_clients.Add(socket);
                socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                m_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        private static void ReceiveCallback(IAsyncResult res)
        {
            try
            {
                var socket = (Socket)res.AsyncState;
                int data = socket.EndReceive(res);
                var tmpBuffer = new byte[data];

                Array.Copy(m_dataBuffer, tmpBuffer, data);

                string txt = Encoding.ASCII.GetString(tmpBuffer);
                Console.WriteLine("Request from client: " + txt);

                for (int i = 0; i < 2000000; i++)
                { }

                if (txt.Contains("DESCRIBE"))
                {
                    Console.WriteLine("Fetching the resource");
                    SendChunk("playing to client", socket);
                }
                else
                {
                    Console.WriteLine("Invalid resource :(");
                    SendChunk("invalid request", socket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        private static void SendCallback(IAsyncResult res)
        {
            try
            {
                var socket = (Socket)res.AsyncState;
                socket.EndSend(res);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socket"></param>
        private static void SendChunk(string message, Socket socket)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
