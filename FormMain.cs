using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NAudio_Pitchshifter_Test
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        SMBPitchShiftingSampleProvider SMB;
        float Pitch = 1f;

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value > 11)
            {
                Pitch = (float)((trackBar1.Value - 1) / 10f);
            }
            else if (trackBar1.Value < 11)
            {
                Pitch = (float)(((trackBar1.Value - 1) / 10f * 0.5f) + 0.5f);
            }
            else
            {
                Pitch = 1f;
            }
            button1.Text = Pitch.ToString();
            if (!Object.ReferenceEquals(null, SMB))
            {
                SMB.PitchFactor = Pitch;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                SMB = new SMBPitchShiftingSampleProvider(new AudioFileReader(OFD.FileName), 4096, 8L, Pitch);
                WaveOutEvent wo = new WaveOutEvent
                {
                    DesiredLatency = 150,
                    NumberOfBuffers = 3
                };
                wo.Init(new SampleToWaveProvider16(SMB));
                wo.Play();
            }
        }
    }
}
