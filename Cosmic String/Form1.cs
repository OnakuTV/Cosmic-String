using System;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using Reloaded.Memory;
using Reloaded.Memory.Sigscan;
using SharpHook;
using SharpHook.Native;

namespace PBB_Trainer
{
    public partial class Form1 : Form
    {
        private bool attached = false;
        private Process proc;

        private int zcoordOff = 0x018CF680;
        private int camOff = 0x02170148;
        private int timerOff = 0xACA678;

        private IntPtr coordAddress;
        private IntPtr camAdd;
        private IntPtr timerAdd;

        private ExternalMemory gameMem;

        nint coordAdd;
        nint camCoordAdd;

        private float[] savedPos = new float[3];
        private float[] savedCamPos = new float[9];
        private float[] savedCamRot = new float[12];

        float[] oldPos = {0,0,0};

        private SimpleGlobalHook kbHook;
        private Task task;
        
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


                    attached = true;
                    button1.Text = "Detach";

                    timer1.Start();
                    
                    kbHook = new SimpleGlobalHook(true);
                    kbHook.KeyPressed += handle_keys;
                    task = kbHook.RunAsync();
                }
                else
                {
                    attached = false;
                    button1.Text = "Attach";
                    timer1.Stop();
                    
                    //kbHook.Dispose();
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

            gameMem.Read<float>((nuint)camCoordAdd + 0x2E8, out savedCamPos[0]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x2EC, out savedCamPos[1]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x2F0, out savedCamPos[2]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x2F4, out savedCamPos[3]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x2F8, out savedCamPos[4]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x2FC, out savedCamPos[5]);

            gameMem.Read<float>((nuint)camCoordAdd + 0x30C, out savedCamRot[0]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x310, out savedCamRot[1]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x314, out savedCamRot[2]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x318, out savedCamRot[3]);
            gameMem.Read<float>((nuint)camCoordAdd + 0x31C, out savedCamRot[4]);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!attached)
            {
                MessageBox.Show("Attach program to PBB first");
                return;
            }
            gameMem.Write<float>((nuint)camCoordAdd + 0x2E8, savedCamPos[0]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x2EC, savedCamPos[1]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x2F0, savedCamPos[2]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x2F4, savedCamPos[3]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x2F8, savedCamPos[4]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x2FC, savedCamPos[5]);

            gameMem.Write<float>((nuint)camCoordAdd + 0x30C, savedCamRot[0]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x310, savedCamRot[1]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x314, savedCamRot[2]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x318, savedCamRot[3]);
            gameMem.Write<float>((nuint)camCoordAdd + 0x31C, savedCamRot[4]);

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
                float speedHorizontal;

                gameMem.Read<nint>((nuint)coordAddress, out coordAdd);
                gameMem.Read<nint>((nuint)camAdd, out camCoordAdd);

                gameMem.Read<float>((nuint)coordAdd + 0x20, out curPos[0]);
                gameMem.Read<float>((nuint)coordAdd + 0x24, out curPos[1]);
                gameMem.Read<float>((nuint)coordAdd + 0x28, out curPos[2]);

                speedHorizontal = (float)Math.Round(Math.Sqrt(Math.Pow(curPos[0] - oldPos[0],2) + Math.Pow(curPos[2] - oldPos[2], 2)), 1);

                gameMem.Write<double>((nuint)timerAdd, 999999);

                label1.Text = "Saved X Pos: " + savedPos[0];

                label2.Text = "Saved Y Pos: " + savedPos[1];

                label3.Text = "Saved Z Pos: " + savedPos[2];

                label4.Text = "Current X Pos: " + Math.Round(curPos[0], 1);
                label5.Text = "Current Y Pos: " + Math.Round(curPos[1], 1);
                label6.Text = "Current Z Pos: " + Math.Round(curPos[2], 1);
                label7.Text = "Speed: " + speedHorizontal;

                oldPos[0] = curPos[0];
                oldPos[1] = curPos[1];
                oldPos[2] = curPos[2];
            }
            catch { 
                //Nothing happens here LOOOOL
            }

        }
      
        private void handle_keys(Object sender, KeyboardHookEventArgs e)
        {
            if (attached)
            {
                KeyCode key = e.Data.KeyCode;
                if(key == KeyCode.VcF9)
                {
                    button2_Click(sender, e);
                }
                else if(key == KeyCode.VcF10)
                {
                    button3_Click(sender,e);
                }
            }
        }
    }
}
