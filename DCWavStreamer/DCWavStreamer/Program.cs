using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCWavStreamer.Audio;

namespace DCWavStreamer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialise the application
            var conf = new DCConfig();
            conf.SERVER_ADDR  = "10.96.134.50";
            conf.SERVER_PORT  = 554;
            conf.SERVER_TITLE = "Datacom WAV Streaming Server";
            conf.INBOUND_BUFFER_SIZE = 256;
            conf.BACK_LOG_SIZE = 5;

            // Create the server and start listening
            var server = new DCServer(conf);
            Console.ReadLine();
        }
    }
}
