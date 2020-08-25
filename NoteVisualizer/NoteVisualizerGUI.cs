using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design;
using System.Timers;

[assembly : InternalsVisibleTo("UnitTestSoundVisualizer")]

/// <summary>
/// Credit Programm for the C# and .NET I subject 2020
/// Made by Tomas Celko
/// MFF UK Prague
/// </summary>
namespace NoteVisualizer
{
    /// <summary>
    /// Graphical User interface of the whole application
    /// </summary>
    public partial class SoundVisualizer : Form 
    {
        public SoundVisualizer()
        {
            InitializeComponent();
            loadingLabel.Visible = false;
            chooseTuningBox.SelectedItem = "C(0)"; //set the default value

            
        }
        public Queue<string> filesToProcess { get; private set; }
        #region event handlers
        /// <summary>
        /// Event Handler for clicking the browse button which opens file dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseButtonClick(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog();
            FD.Multiselect = true;
            FD.Filter = "Sound files (*.wav)|*.wav";
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputSampleTextBox.Text = String.Concat(FD.FileNames.Select(str => str + ';'));
            }

        }
        /// <summary>
        /// Starts processing of given text file(s) selected in inputTextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processButtonClicked(object sender, EventArgs e)
        {

            StartLoading();
            var tuningChosen = GetNumTuning(chooseTuningBox.SelectedItem.ToString());
            if (tuningChosen > 0)
                NoteDetector.TransposeUp(tuningChosen);
            else
                NoteDetector.TransposeDown(-tuningChosen);

            var threadCount = CalculateThreadCount(inputSampleTextBox.Text);

            if (threadCount == 1)
            {
                //cutting away .wav and adding _notes.ly
                new MainProcessor().ProcessSoundFile(inputSampleTextBox.Text.Trim(';'), inputSampleTextBox.Text.Trim(';').Substring(0, inputSampleTextBox.Text.Length - 4) + "_notes.ly", this);
            }
            else
            {
                FillQueue(inputSampleTextBox.Text);
                var sideThreads = CreateThreads(threadCount);
                Array.ForEach(sideThreads, thread => thread.Start());
                new MainProcessor().ProcessSoundFiles(this);
                Array.ForEach(sideThreads, thread => thread.Join());
            }
            Done();

        }
        #endregion
        private int CalculateThreadCount(string inputFiles)
        {
            const int maxThreadCount = 4;
            return Math.Min(inputFiles.Split(';', StringSplitOptions.RemoveEmptyEntries).Length, maxThreadCount);
        }
        private Thread[] CreateThreads(int threadCount)
        {
            var threads = new Thread[threadCount - 1];
            
            for (int i = 0; i < threadCount - 1; i++)
            {
                threads[i] = new Thread(() => new MainProcessor().ProcessSoundFiles(this));  
            }
            return threads;  
        }
        /// <summary>
        /// Enqueues all files selected in textBox to the queue
        /// </summary>
        /// <param name="fileNames">users choice of fileNames separated by semicolon</param>
        private void FillQueue(string fileNames)
        {
            filesToProcess = new Queue<string>();
            foreach (string fileName in fileNames.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                filesToProcess.Enqueue(fileName);
            }
        }
        public void Done()
        {
            loadingLabel.Visible = false; 
            MessageBox.Show("You can now view processed files", "Successfully Done");
        }
        private void StartLoading()
        {
            loadingLabel.Visible = true;
        }
        /// <summary>
        /// Parses tuning selected in textBox to find relative semi-tone distance of transposition
        /// </summary>
        /// <param name="tuning">user's choice of tuning</param>
        private int GetNumTuning(string tuning)
        {
            var numberStartIndex = tuning.IndexOf('(');
            var numberEndIndex = tuning.IndexOf(')');
            return int.Parse(tuning.Substring(numberStartIndex + 1, numberEndIndex - numberStartIndex - 1));
        }
        
    }
    
}
