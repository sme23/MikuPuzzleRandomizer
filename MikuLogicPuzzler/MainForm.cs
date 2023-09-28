using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuLogicPuzzler
{

    public partial class MainForm : Form
    {
        int seed = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Int32.TryParse(textBox1.Text, out seed);
            textBox1.Text = seed.ToString(); // this either does nothing or undoes the new text based on whether TryParse is successful
        }

        private void runExternalProgram(String commandText)
        {

            System.Diagnostics.Process runExt = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo runInfo = new System.Diagnostics.ProcessStartInfo();
            runInfo.FileName = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\MikuPuzzleShuffler.exe");
            runInfo.Arguments = commandText;
            runInfo.UseShellExecute = false;
            runInfo.RedirectStandardOutput = true;
            runInfo.RedirectStandardError = true;
            runInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            runInfo.CreateNoWindow = true;

            runExt.StartInfo = runInfo;

            StreamReader extOutputStream;

            runExt.Start();
            extOutputStream = runExt.StandardError;
            runExt.WaitForExit();

            Console.WriteLine(commandText);

            //start a message box containing the run output
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(extOutputStream.ReadToEnd(), "Output", buttons);




        }

        private void button2_Click(object sender, EventArgs e)
        {
            //roll a new seed
            Random r = new Random();
            seed = r.Next(0, Int32.MaxValue);
            textBox1.Text = seed.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //execute the program with the given information
            runExternalProgram(seed.ToString());


        }
    }
}
