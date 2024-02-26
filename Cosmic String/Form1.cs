using System;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Reloaded.Memory;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace PBB_Trainer
{
    public partial class Form1 : Form
    {
        private bool attached = false;
        private Process proc;

        private int zcoordOff = 0x018CBDC0;
        private int camOff = 0x18C3170;
        private int timerOff = 0xAC57D0;

        private IntPtr coordAddress;
        private IntPtr camAdd;
        private IntPtr timerAdd;

        private ExternalMemory gameMem;

        nint coordAdd;
        nint camCoordAdd;

        private float[] savedPos = new float[3];
        private float[] savedCamPos = new float[6];
        private float[] savedRot = new float[4];
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!attached)
                {
                    proc = Process.GetProcessesByName("PennysBigBreakaway").FirstOrDefault();

                    if (proc == null)
                    {
                        MessageBox.Show("PBB could not be found");
                        return;
                    }
                    coordAddress = IntPtr.Add(proc.MainModule.BaseAddress, zcoordOff);
                    camAdd = IntPtr.Add(proc.MainModule.BaseAddress, camOff);
                    timerAdd = IntPtr.Add(proc.MainModule.BaseAddress, timerOff);

                    gameMem = new ExternalMemory(proc);


                    //should give address to pointer. Not value!

                    attached = true;
                    button1.Text = "Dettach";

                    timer1.Start();
                    
                }
                else
                {
                    attached = false;
                    button1.Text = "Attach";
                    timer1.Stop();
                }

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
            gameMem.Read<nint>((nuint)camAdd, out camCoordAdd);

            gameMem.Read<float>((nuint)coordAdd + 0x20, out savedPos[0]);
            gameMem.Read<float>((nuint)coordAdd + 0x24, out savedPos[1]);
            gameMem.Read<float>((nuint)coordAdd + 0x28, out savedPos[2]);

            gameMem.Read<float>((nuint)camCoordAdd + 0x478, out savedCamPos[0]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x47C, out savedCamPos[1]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x480, out savedCamPos[2]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x484, out savedCamPos[3]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x488, out savedCamPos[4]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x48C, out savedCamPos[5]);

            gameMem.Read<float>((nuint)camCoordAdd + 0x49C, out savedRot[0]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x4A0, out savedRot[1]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x4A4, out savedRot[2]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x4A8, out savedRot[3]);



            label1.Text = "Saved X Pos: " + savedPos[0];

            label2.Text = "Saved Y Pos: " + savedPos[1];

            label3.Text = "Saved Z Pos: " + savedPos[2];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!attached)
            {
                MessageBox.Show("Attach program to PBB first");
                return;
            }
            gameMem.Write<float>((nuint)camCoordAdd + 0x478, savedCamPos[0]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x47C, savedCamPos[1]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x480, savedCamPos[2]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x484, savedCamPos[3]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x488, savedCamPos[4]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x48C, savedCamPos[5]);

            gameMem.Write<float>((nuint)camCoordAdd + 0x49C, savedRot[0]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x4A0, savedRot[1]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x4A4, savedRot[2]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x4A8, savedRot[3]);

            gameMem.Write<float>((nuint)coordAdd + 0x20, savedPos[0]);
            gameMem.Write<float>((nuint)coordAdd + 0x24, savedPos[1]);
            gameMem.Write<float>((nuint)coordAdd + 0x28, savedPos[2]);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //if is in a level
                float[] curPos = new float[3];

                gameMem.Read<nint>((nuint)coordAddress, out coordAdd);
                gameMem.Read<nint>((nuint)camAdd, out camCoordAdd);

                gameMem.Read<float>((nuint)coordAdd + 0x20, out curPos[0]);
                gameMem.Read<float>((nuint)coordAdd + 0x24, out curPos[1]);
                gameMem.Read<float>((nuint)coordAdd + 0x28, out curPos[2]);

                gameMem.Write<double>((nuint)timerAdd, 999999);

                label4.Text = "Current X Pos: " + curPos[0];
                label5.Text = "Current Y Pos: " + curPos[1];
                label6.Text = "Current Z Pos: " + curPos[2];
            }
            catch { 
                //Nothing happens here LOOOOL
            }

        }
    }
}
