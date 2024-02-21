using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Task10
{
    public partial class Form1 : Form
    {

        private const int bufferSize = 10240;
        public static long totalBytesRead = 0;  // Track total bytes read
        public static long totalFileSize = 0;
        public static bool flag = false;
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void Form1_FormOpen(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public Form1()
        {
            InitializeComponent();
            SourceTextBox.Text = "D:\\ALL MOVIES\\farzi";
            DestinationTextBox.Text = "C:\\Users\\raviv\\Desktop\\New folder\\farzi";
            ResumeButton.Enabled = false;
            Stopbutton.Enabled = false;
        }







        #region source and destination button click

        //select source 
        private void SourceSelect_Click(object sender, EventArgs e)
        {
            //OpenFileDialog FileDialog = new OpenFileDialog();

            FolderBrowserDialog FileDialog = new FolderBrowserDialog();
            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                SourceTextBox.Text = FileDialog.SelectedPath;
            }
        }

        //select destination
        private void DestinationSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == DialogResult.OK)
            {
                DestinationTextBox.Text = Path.Combine(fdb.SelectedPath, Path.GetFileName(SourceTextBox.Text));
            }
        }

        #endregion






        #region start, pause ,resume

        //start button code
        private async void Startbutton_Click(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            ResumeButton.Enabled = true;
            Stopbutton.Enabled = true;
            if (string.IsNullOrEmpty(SourceTextBox.Text) || string.IsNullOrEmpty(DestinationTextBox.Text))
            {
                MessageBox.Show("Please select source and destination folders.");
                return;
            }


            string[] files = Directory.GetFiles(SourceTextBox.Text);

            totalFileSize = files.Sum(file => new FileInfo(file).Length);



            foreach (string file in files)
            {
                string destinationFile = Path.Combine(DestinationTextBox.Text, Path.GetFileName(file));

                //if file already exist 
                if (File.Exists(destinationFile))
                {
                    long destinationfilesize = new FileInfo(destinationFile).Length;
                    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    if (destinationfilesize == fileStream.Length)
                    {
                        totalFileSize -= new FileInfo(file).Length;
                        MessageBox.Show($"The file {destinationFile} already exists. Skipping.");
                        continue;
                    }
                    else
                    {
                        fileStream.Position = destinationfilesize;
                        await Task.Run(() => StartTransfer(fileStream, file));
                    }
                }

                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    await Task.Run(() => StartTransfer(fileStream, file));
                }
            }
            this.FormClosing += Form1_FormOpen;
            Startbutton.Invoke((MethodInvoker)delegate
            {
                Startbutton.Enabled = false;
            });
        }


        //this method call inside in above Startbutton_Click method
        private void StartTransfer(FileStream fileStream, string sourceFile)
        {
            Startbutton.Invoke((MethodInvoker)delegate
            {
                Startbutton.Enabled = false;
            });
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            string destinationFile = Path.Combine(DestinationTextBox.Text, Path.GetFileName(sourceFile));
            string destinationdir = Path.GetDirectoryName(destinationFile);

            // Create the destination file if it doesn't exist
            if (!Directory.Exists(destinationdir))
            {
                Directory.CreateDirectory(destinationdir);
            }

            using (FileStream destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
            {
                while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {

                    //pause resume logic line 
                    pauseEvent.WaitOne();


                    destinationStream.Write(buffer, 0, bytesRead);

                    Interlocked.Add(ref totalBytesRead, bytesRead);

                    progressBar.Invoke((MethodInvoker)delegate
                    {
                        long progressScale = totalFileSize / progressBar.Maximum;
                        int progressValue = (int)(totalBytesRead / progressScale);

                        progressBar.Value = Math.Min(progressValue, progressBar.Maximum);


                        double percentage = ((double)progressBar.Value / progressBar.Maximum) * 100;
                        TransferPercentage.Text = $"{percentage:F2}%";
                    });

                }
            }
        }


        //when you click stop then this method calld
        private void Stopbutton_Click(object sender, EventArgs e)
        {
            pauseEvent.Reset();
        }


        //resume button
        private void ResumeButton_Click(object sender, EventArgs e)
        {
            pauseEvent.Set();
        }





        #endregion

    }
}

