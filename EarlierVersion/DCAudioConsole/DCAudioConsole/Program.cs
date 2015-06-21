using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace DCAudioConsole
{
    class Program
    {
        // Network variables
        private static List<Thread> threadList;
        private static TcpListener server;
        private static Byte[] byteBuffer;
        private static String serverIp = "10.96.134.50";
        private static int port = 554;

        static void Main(string[] args)
        {
            try
            {
                // Initialise server and variables.
                server = new TcpListener(System.Net.IPAddress.Parse(serverIp), port);
                server.Start();
                byteBuffer = new Byte[2048];
                threadList = new List<Thread>();

                // Keep running. Hopefully we recieve information from listening to the stream.
                while (true)
                {
                    try
                    {
                        // Establish the streams we are listening to. 
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Client connected...");

                        if (client != null)
                        {
                            // Make a new thread to write out the bytes.
                            Thread newThread = new Thread(new ThreadStart(Listen));
                            newThread.IsBackground = true;
                            newThread.Start();
                            threadList.Add(newThread);

                            Console.WriteLine("New worker thread created.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                Console.WriteLine("Frame: " + frame.ToString());
                Console.WriteLine("line: " + line.ToString());
            }

        }

        //
        // Listen thread to print all of the contents of the stream to the console application.
        //
        private static void Listen()
        {
            try
            {
                // Keep running. Hopefully we recieve information from listening to the stream.
                // Establish the stream to listen to. 
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...\n");

                // Get the tcp connection.
                NetworkStream serverStream = client.GetStream();

                /*while (client.Connected)
                {
                    Byte[] readBuffer = new byte[serverStream.Length];

                    // Read the amount of bytes.
                    int bytesRead = serverStream.Read(readBuffer, 0, readBuffer.Length);

                    var bufferCommand = System.Text.Encoding.UTF8.GetString(readBuffer);

                    Console.WriteLine("Command: " + serverStream.Length + "\n");

                    if (bytesRead == 0) break;
                } */

                Console.WriteLine("Thread disconnected.");
            }
            catch (Exception e)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                Console.WriteLine("Frame: " + frame.ToString());
                Console.WriteLine("line: " + line.ToString());
            }
        }

        //
        // Closure event. Thread is disposed here. 
        //
        private static bool ConsoleEventCallback(int eventType)
        {
            for(int i = 0; i < threadList.Count; i++)
                if (eventType == 2)
                    threadList[i].Abort(); 

            return false;
        }
    }
}
