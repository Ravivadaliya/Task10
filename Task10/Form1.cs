using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Task10
{
    public partial class Form1 : Form
    {
        private FileStream fileStream;
        private const int bufferSize = 102400;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }



        //select source and destination method
        private void SourceSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog FileDialog = new OpenFileDialog();

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                SourceTextBox.Text = FileDialog.FileName;
            }
        }

        private void DestinationSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == DialogResult.OK)
            {
                DestinationTextBox.Text = Path.Combine(fdb.SelectedPath, Path.GetFileName(SourceTextBox.Text));
            }
        }





        //start and stop button methods 

        //when start buttton click
        //private void Startbutton_Click(object sender, EventArgs e)
        //{

        //    if (string.IsNullOrEmpty(SourceTextBox.Text) || string.IsNullOrEmpty(DestinationTextBox.Text))
        //    {
        //        MessageBox.Show("Please select source and destination folders.");
        //        return;
        //    }

        //    if (fileStream == null)
        //    {
        //        fileStream = new FileStream(SourceTextBox.Text, FileMode.Open, FileAccess.Read);
        //        //progressBar.Value = 0;
        //        //progressBar.Maximum = (int)fileStream.Length;
        //        progressBar.Maximum = 100;
        //        progressBar.Minimum = 0;

        //        Thread transferThread = new Thread(StartTransfer);
        //        transferThread.Start();
        //    }
        //}
        private void Startbutton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SourceTextBox.Text) || string.IsNullOrEmpty(DestinationTextBox.Text))
            {
                MessageBox.Show("Please select source and destination folders.");
                return;
            }

            if (fileStream == null)
            {
                fileStream = new FileStream(SourceTextBox.Text, FileMode.Open, FileAccess.Read);

                //progressBar.Maximum = (int)fileStream.Length;
                progressBar.Maximum = 100;
                progressBar.Minimum = 0;

                Thread transferThread = new Thread(StartTransfer);
                transferThread.Start();
            }
        }



        //this method call inside in above Startbutton_Click method
        long totalBytesRead = 0;  // Track total bytes read
        private void StartTransfer()
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            long totalFileSize = fileStream.Length;

            while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
            {
                using (FileStream destinationStream = new FileStream(DestinationTextBox.Text, FileMode.Append, FileAccess.Write))
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                }

                totalBytesRead += bytesRead;  // Update total bytes read

                //progressBar.Invoke((MethodInvoker)delegate
                //{

                //    long progressScale = totalFileSize / progressBar.Maximum;
                //    progressBar.Value = (int)(totalBytesRead / progressScale);

                //    double percentage = ((double)progressBar.Value / progressBar.Maximum) * 100;
                //    TransferPercentage.Text = $"{percentage:F2}%";
                //});
                progressBar.Invoke((MethodInvoker)delegate
                {
                    // Update progress based on the total progress made
                    long progressScale = totalFileSize / progressBar.Maximum;
                    int progressValue = (int)(totalBytesRead / progressScale);

                    // Ensure the progress value does not exceed the maximum
                    progressBar.Value = Math.Min(progressValue, progressBar.Maximum);

                    double percentage = ((double)progressBar.Value / progressBar.Maximum) * 100;
                    TransferPercentage.Text = $"{percentage:F2}%";
                });

                if (fileStream == null)
                {
                    break;
                }
            }

            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }


        //when you click stop then this method calld
        private void Stopbutton_Click(object sender, EventArgs e)
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }

        }
    }
}




