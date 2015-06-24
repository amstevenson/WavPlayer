using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using DCWavStreamer.Audio;
using System.IO;

namespace DCWavStreamer.Audio
{
    class ALawWaveStream
    {
        private WaveStream pcmStream;
        private WaveFormat aLawFormat;
        private FileStream fileStream;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="waveFileName">The name of the wave file that is currently in the .bin directory
        ///                            of the application. 
        /// </param>
        public ALawWaveStream(String waveFileName)
        {
            // Take the audio file and create an ALaw formatted stream.
            // A WaveStream needs to be created to construct the appropriate headers.
            fileStream = new FileStream(waveFileName, FileMode.Open);
            aLawFormat = WaveFormat.CreateALawFormat(8000, 1);
            RawSourceWaveStream reader = new RawSourceWaveStream(fileStream, aLawFormat);
            pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
        }

        /// <summary>
        /// Converts a PCM stream into ALaw and retrieves a set amount of bytes depending on the
        /// start position and duration. 
        /// </summary>
        /// <param name="startPositionInSeconds">The starting position of the stream.</param>
        /// <param name="durationInSeconds">How many seconds of byte reading there will be from the starting position.</param>
        /// <returns>A byte chunk containing all of the audio data.</returns>
        public byte[] getByteChunk(int startPositionInSeconds, int durationInSeconds)
        {
            int averageBytesPerSecond = getAverageBytesPerSecond();

            // Start and end positions - byte boundary allocations.
            // Out of bounds for the last second of an audio file does not need to be a concern,
            // as the read operation takes it as a 'max' and therefore does not go over the end.
            pcmStream.Position = averageBytesPerSecond * startPositionInSeconds;

            int maxBytesToRead;
            if ((averageBytesPerSecond * durationInSeconds) > pcmStream.Length)
                maxBytesToRead = (int)pcmStream.Length;
            else
                maxBytesToRead = (int)averageBytesPerSecond * durationInSeconds;

            // Convert from PCM to ALaw at the specified start point, capturing the bytes up until the duration period has expired.
            WaveProviderToWaveStream aLawStream = new WaveProviderToWaveStream(
                new WaveFormatConversionStream(aLawFormat, pcmStream),
                maxBytesToRead);

            // Return the byte data for the selected chunk.
            byte[] chunkBytes = new byte[maxBytesToRead];
            aLawStream.Read(chunkBytes, 0, maxBytesToRead);

            // Return the byte array.
            return chunkBytes;
        }

        /// <summary>
        /// Plays a chunk of byte data. For testing purposes only to ensure the application works as planned.
        /// </summary>
        /// <param name="audioByteChunk">The byte array containing the audio data in an ALaw format.</param>
        public void playTestAudio(byte[] audioByteChunk)
        {
            // Create an Alaw provider.
            IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(audioByteChunk), aLawFormat);

            // Read all the bytes from the provider.
            WaveProviderToWaveStream testStream = new WaveProviderToWaveStream(provider);

            // Create the Alaw file.
            WaveStream aLaw = WaveFormatConversionStream.CreatePcmStream(testStream);

            // Play the file.
            BlockAlignReductionStream pcmStream = new NAudio.Wave.BlockAlignReductionStream(aLaw);
            DirectSoundOut output = new NAudio.Wave.DirectSoundOut();
            output.Init(pcmStream);
            output.Play();
        }

        /// <summary>
        /// Saves the audio file for testing purposes only. ".wav" is appended on the end.
        /// </summary>
        /// <param name="audioByteChunk">The byte array containing the audio data in an ALaw format.</param>
        /// <param name="fileName"></param>
        public void saveTestAudio(byte[] audioByteChunk, String fileName)
        {
            // Create an Alaw provider.
            IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(audioByteChunk), aLawFormat);

            // Read all the bytes from the provider.
            WaveProviderToWaveStream testStream = new WaveProviderToWaveStream(provider);

            // Create the Alaw file.
            using (WaveStream aLaw = WaveFormatConversionStream.CreatePcmStream(testStream))
            {
                WaveFileWriter.CreateWaveFile(fileName, testStream);
            }
        }

        public double getDurationInSeconds()
        {
            return pcmStream.TotalTime.TotalSeconds;
        }

        /// <summary>
        /// The length of the PCM stream. This could be removed, but provides an easy way to
        /// access the amount of bytes in the file for testing purposes outside the scope of the class.
        /// To calculate the amount of ALaw bytes, divide by 2.
        /// </summary>
        /// <returns>The amount of bytes in the non-converted stream.</returns>
        public long getLength()
        {
            return pcmStream.Length;
        }

        /// <summary>
        /// Get the average bytes per second. This will be used in conjunction with the
        /// converted ALaw Stream. Therefore it should be noted that the sample rate is half that
        /// of a PCM, and as a result the average is going to involve dividing the initial average of the PCM stream
        /// by two. 
        /// </summary>
        /// <returns></returns>
        public int getAverageBytesPerSecond()
        {
            if (pcmStream.TotalTime.Seconds > 0)
            {
                // Average bytes per second for Wave does not exceed the size of an integer,
                // but if we find it does, the casts and return types will need to be changed.
                int totalTime = (int)pcmStream.TotalTime.TotalSeconds;
                int totalLength = (int)pcmStream.Length;
                return (totalLength / totalTime) / 2;
            }
            else
            {
                // A less accurate average that does not take into account different
                // formats, but this will only be used for audio files of 0:00, so it will 
                // not have an effect on performance. 
                return pcmStream.WaveFormat.AverageBytesPerSecond / 2;
            }
        }

        /// <summary>
        /// Deconstructor. Deletes the file stream that is open throughout the byte reading process.
        /// </summary>
        ~ALawWaveStream()
        {
            fileStream.Close();
        }
    }
}
