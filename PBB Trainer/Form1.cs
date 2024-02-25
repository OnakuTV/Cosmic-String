using System;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory;
using System.Xml.Serialization;

namespace PBB_Trainer
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("Kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        private bool attached = false;
        private Process proc;

        private int zcoordOff = 0x018CBDC0;
        private int xcoordOff = 0x00AA5088;


        private byte[] buffer;

        public IntPtr bytesRead;
        private IntPtr coordAddress;

        private ExternalMemory gameMem;

        nint coordAdd;

        public float[] savedPos = new float[3];
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                proc = Process.GetProcessesByName("PennysBigBreakaway").FirstOrDefault();

                if (proc == null)
                {
                    MessageBox.Show("PBB could not be found");
                    return;
                }
                MessageBox.Show(proc.MainModule.BaseAddress.ToString("X"));
                coordAddress = IntPtr.Add(proc.MainModule.BaseAddress, zcoordOff);
                MessageBox.Show(coordAddress.ToString("X"));

                gameMem = new ExternalMemory(proc);


                //should give address to pointer. Not value!

                attached = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!attached)
            {
                MessageBox.Show("Attach program to PBB first");
                return;
            }

            gameMem.Read<nint>((nuint)coordAddress, out coordAdd);
            gameMem.Read<float>((nuint)coordAdd + 0x20, out savedPos[0]);
            gameMem.Read<float>((nuint)coordAdd + 0x24, out savedPos[1]);
            gameMem.Read<float>((nuint)coordAdd + 0x28, out savedPos[2]);

            label1.Text = "Saved X Pos: " + savedPos[0];

            label2.Text = "Saved Y Pos: " + savedPos[1];

            label3.Text = "Saved Z Pos: " + savedPos[2];

            MessageBox.Show(savedPos[2].ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gameMem.Write<float>((nuint)coordAdd + 0x20, savedPos[0]);
            gameMem.Write<float>((nuint)coordAdd + 0x24, savedPos[1]);
            gameMem.Write<float>((nuint)coordAdd + 0x28, savedPos[2]);
        }
    }
}
