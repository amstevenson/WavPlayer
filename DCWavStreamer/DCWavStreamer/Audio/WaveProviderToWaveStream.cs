using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace DCWavStreamer.Audio
{
    class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider source;
        private long position;
        private int bytesRead;
        private readonly int maxBytesToRead;
        private string readType;

        /// <summary>
        /// Default constructor for creating a sound file in a specified format
        /// for the purpose of reading all of the bytes. 
        /// </summary>
        /// <param name="source">The provider or stream for the wav file.</param>
        public WaveProviderToWaveStream(IWaveProvider source)
        {
            this.source = source;
        }

        /// <summary>
        /// Default constructor for creating a sound file in a specified format
        /// for the purpose of reading a specified chunk of the sound file. 
        /// For example three seconds. 
        /// </summary>
        /// <param name="source">The provider or stream for the wav file.</param>
        /// <param name="maxBytesToRead">The maximum amount of bytes to read. Which is usually
        ///                              the average amount of bytes per second times by the desired duration of the file in seconds.
        /// </param>
        public WaveProviderToWaveStream(IWaveProvider source, int maxBytesToRead)
        {
            this.source = source;
            this.maxBytesToRead = maxBytesToRead;
            this.readType = "average";
        }

        public override WaveFormat WaveFormat
        {
            // Return the format of the wave file.
            get { return source.WaveFormat; }
        }

        public override long Length
        {
            // This methods implementation is required.
            // So I have returned a random large number. 
            // This value however is configured later on.
            get { return Int32.MaxValue; }
        }

        public override long Position
        {
            // Return the number of bytes read so far.
            get { return position; }

            // The position is set in the main application, but the set methods
            // implementation is required.
            set { this.position = Position; }
        }

        public override int Read(byte[] buffer, int offset, int bytesToRead)
        {
            switch (readType)
            {
                case "average":
                    {
                        // Return a specific amount of bytes that is the equivalent of the amount of bytes per second
                        // times the desired duration of the file.
                        int bytesToReadThisTime = Math.Min(bytesToRead, maxBytesToRead - bytesRead);
                        int bytesReadThisTime = source.Read(buffer, offset, bytesToReadThisTime);
                        bytesRead += bytesReadThisTime;
                        return bytesReadThisTime;
                    }
                default:
                    {
                        // This can be removed later on once we are sure the application works. As it will not be needed if the test
                        // methods are removed.
                        int read = source.Read(buffer, offset, bytesToRead);
                        position += read;
                        return read;
                    }
            }
        }
    }
}
