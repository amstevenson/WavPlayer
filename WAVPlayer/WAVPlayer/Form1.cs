using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Media;
using NAudio.Wave;
using WAVPlayer.Audio;

namespace WAVPlayer
{
    public partial class Form1 : Form
    {
        private string[] allWavs;
        BlockAlignReductionStream stream = null;
        DirectSoundOut output = null;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            /***********************************************************************
            * 
            * Retrieve the contents of the bin folder, and place only the .wav files
            * into an array, so we can work with the files. 
            * 
            * *********************************************************************/

            // The location of the debug folder
            string executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            // Put all txt files in the root directory into an array
            string[] allFiles = Directory.GetFiles(executableLocation);

            int i = 0; // the amount of objects
            foreach (string name in allFiles)
                if (Path.GetExtension(name) == ".wav")
                {
                    allFiles[i] = Path.GetFileName(name);
                    i++;
                }

            allWavs = new string[i]; // private array - in the scope of the whole form

            lstWavFiles.Items.Clear();
            for (i = 0; i < allWavs.Length; i++)
            {
                allWavs[i] = allFiles[i];
                lstWavFiles.Items.Add(allWavs[i].Replace(".wav", ""));
            }

            try
            {
                lstWavFiles.SelectedIndex = 3; // default selected is the hold music (alphabetical order)
            }
            catch (ArgumentOutOfRangeException c)
            {
                lstWavFiles.Items.Add("There are no files");
                Console.WriteLine(c);
            }

        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            playFile(allWavs[lstWavFiles.SelectedIndex]);
        }

        private void playFile(string fileName)
        {
            // Read the contents of the file. 
            var reader = new WaveFileReader(fileName);
            
            // Format for PCM - thanks Google.
            var newFormat = new WaveFormat(8000, 16, 1); 
                
            // Convert Alaw to PCM
            var conversionStream = new WaveFormatConversionStream(newFormat, reader);
                
            // The conversion stream is essentially a WaveProvider after being converted.
            // Therefore we need to create a waveStream that is determined by this value
            // (an enumerator of sorts), so a class has been made for this: WaveProviderToWaveStream 
            WaveProviderToWaveStream waveStream = new WaveProviderToWaveStream(conversionStream);

            stream = new BlockAlignReductionStream(waveStream);
            output = new DirectSoundOut();
            output.Init(stream);
            output.Play();
        }

        //
        // Used to pause the file. 
        // Probably won't need this, but will keep it for now. Might be deleted later on.
        //
        private void DisposeWave()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }
    }
}
