using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Task10
{
    public partial class Form1 : Form
    {

        private FileStream fileStream;
        private const int bufferSize = 102400;


        public Form1()
        {
            InitializeComponent();
            SourceTextBox.Text = "D:\\ALL MOVIES\\Asur.mkv";
            DestinationTextBox.Text = "C:\\Users\\raviv\\Desktop\\New folder\\Asur.mkv";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        #region source and destination button click

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

        #endregion





        #region start stop button


        //start button code
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
            fileStream.Position = totalBytesRead;

            while (fileStream != null && (bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
            {
                //write byte to destination
                using (FileStream destinationStream = new FileStream(DestinationTextBox.Text, FileMode.Append, FileAccess.Write))
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                }

                //progressbar
                totalBytesRead += bytesRead;  // Update total bytes read
                progressBar.Invoke((MethodInvoker)delegate
                {
                    long progressScale = totalFileSize / progressBar.Maximum;
                    int progressValue = (int)(totalBytesRead / progressScale);

                    progressBar.Value = Math.Min(progressValue, progressBar.Maximum);

                    double percentage = ((double)progressBar.Value / progressBar.Maximum) * 100;
                    TransferPercentage.Text = $"{percentage:F2}%";

                });
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


        #endregion
    }
}

