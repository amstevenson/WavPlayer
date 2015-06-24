using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using DCWavStreamer.Audio;

namespace DCWavStreamer
{
    class DCServer
    {
        /// <summary>
        /// 
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
        /// 
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
        /// Defines a Callback to Accept a new connection to the server.  This works similar to how mutex locks
        /// work in multi threaded applications.  The server will acquire the lock by ending its accept state, 
        /// then it will add the new socket to the managed socket pool, start listening to that client for a request
        /// and then it will finally allow new sockets to connect, this will fire recursively and shall listen
        /// to all clients on each loop.
        /// 
        /// </summary>
        /// <param name="res">Pointer to the socket we are listening to</param>
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
        /// Defines a callback to receive data from the connected client socket.  The pointer passed in the Accept method
        /// tells the server which socket it is listening on (this runs asynchronously as we listen to multiple sockets at once)
        /// we are then able to pull the request from that socket into the readbuffer, allowing us to see what type 
        /// of request is sent. This is normally an RTSP DESCRIBE request, from here we can determine what stream the
        /// listening socket should read from.
        /// 
        /// </summary>
        /// <param name="res"></param>
        private static void ReceiveCallback(IAsyncResult res)
        {
            try
            {
                // Get the pointer to our socket from the result, and then create a temporary buffer
                // to pull the information from 
                var socket    = (Socket)res.AsyncState;
                int data      = socket.EndReceive(res);
                var tmpBuffer = new byte[data];

                // Copy the request data into the temp buffer (this is written to in accept callback from the connecting socket)
                Array.Copy(m_dataBuffer, tmpBuffer, data);

                string request = Encoding.ASCII.GetString(tmpBuffer);
                Console.WriteLine("Request from client: " + request);

                // See what type of RTSP request we recieve on our end, DESCRIBE means that the client is asking for
                // the data at the specified resource i.e. rtsp://server.com/streams/streamX would respond with a PLAY request
                // with a chunk of data from that stream at the current position.
                if (request.Contains("DESCRIBE"))
                {
                    Console.WriteLine("Fetching chunk from the requested stream...");
                    try
                    {
                        // Convert the WAV chunk to a byte array and send it to the current socket, once the data
                        // has been transmitted the socket will automatically end its sending state and start listening
                        // for more data, this is how we set up our continuous stream from client to server...
                        // Collect byte information from audio file
                        ALawWaveStream waveStream = new ALawWaveStream("holdmusic.wav");
                        byte[] byteChunk = waveStream.getByteChunk(0, 3);
                        socket.BeginSend(byteChunk, 0, byteChunk.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                        socket.BeginReceive(m_dataBuffer, 0, m_dataBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid resource :(");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// Defines a callback for when a data is sent to a connected socket. This tells the server socket that the data in the buffer
        /// has been sent and that the socket can start recieving data again. This will allow us to consitently send chunks to the
        /// listening SIP application.
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
    }
}
