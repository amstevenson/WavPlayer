using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCWavStreamer
{
    class DCStream
    {
        /// <summary>
        /// 
        /// Member variables for a DCStream object
        /// 
        /// </summary>
        private string m_streamName;
        private int    m_numSongs;
        private int    m_currSong;
        private int    m_currPos;
        private byte[] m_songs;

        /// <summary>
        /// 
        /// Creates a new DCStream object, the DCStream class is responsible for creating
        /// a listening interface between DCServer and DCStream to get WAV chunks from 
        /// the stream at the current point.
        /// 
        /// This class exists as multiple streams can exist on the server, we want to be 
        /// able to point users to the correct stream object and then pull samples out
        /// of that to serve the correct content.
        /// 
        /// </summary>
        /// <param name="streamName"></param>
        public DCStream(string streamName)
        {
            m_streamName = steamName;
        }

        /// <summary>
        /// 
        /// Returns the WAV chunk based on the current position in the current song 
        /// that is playing.  This chunk is then returned to DCServer from which 
        /// it is played back to the connected socket(s)
        /// 
        /// </summary>
        /// <returns>a byte array contains raw WAV data to be piped to the client</returns>
        public static byte[] getChunk()
        {
            
        }
    }
}
