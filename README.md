# NAudio-Pitchshifter

A realtime C# audio pitch shifter based on S. M. Bernsees phase vocoder that implements an NAudio SampleProvider.

Test application:


![Example Screenshot](Example.bmp)


Usage:




// Choose FFTSize and Osamp. (recommended are 4096 and 8)

// Define Pitch shifting factor. (0.5f pitches one octave down, 2f would pitch one octave up)

SMB = new SMBPitchShiftingSampleProvider(new AudioFileReader(@"C:\Test.wav"), 4096, 8L, 0.5f);

WaveOutEvent wo = new WaveOutEvent
{
  DesiredLatency = 150,
  NumberOfBuffers = 3
};

wo.Init(new SampleToWaveProvider16(SMB));

wo.Play();
