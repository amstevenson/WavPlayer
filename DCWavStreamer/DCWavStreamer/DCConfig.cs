using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCWavStreamer
{
    /// <summary>
    /// Struct to detail any configuration constants for the server
    /// application.  This can be added to as we further expand the application
    /// </summary>
    struct DCConfig
    {
        // Configuration constants for server information
        public string SERVER_ADDR;
        public int    SERVER_PORT;

        // Describe the inbound buffer size, defaults to 256. 
        public int    INBOUND_BUFFER_SIZE;

        // Set the backlog - details the amount of pending connections that can be handled at once
        public int    BACK_LOG_SIZE;

        // Configuration constants for 
        public string SERVER_TITLE;

        // Configuration constants for miscelaneous functionality
        public List<String> SUPPORTED_CODECS;
    }
}
