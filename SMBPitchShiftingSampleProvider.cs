using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using NAudio.Wave;

///
/// Author: Freefall
/// Date: 05.08.16
/// Based on: the port of Stephan M. Bernsee´s pitch shifting class
/// Port site: https://sites.google.com/site/mikescoderama/pitch-shifting
/// Test application and github site: https://github.com/Freefall63/NAudio-Pitchshifter
/// 
/// NOTE: I strongly advice to add a Limiter for post-processing.
/// For my needs the FastAttackCompressor1175 provides acceptable results:
/// https://github.com/Jiyuu/SkypeFX/blob/master/JSNet/FastAttackCompressor1175.cs
///
public class SMBPitchShiftingSampleProvider : ISampleProvider
{

    private ISampleProvider SourceStream = null;
    private WaveFormat WFormat = null;
    private float Pitch = 1f;
    private int _FFTSize;
    private long _osamp;
    private SMBPitchShifter ShifterLeft = new SMBPitchShifter();
    private SMBPitchShifter ShifterRight = new SMBPitchShifter();
    private float volscale = 1f; //Recommended to scale volume down, as SMB seems to clip with pitching

    private object PitchLock = new object();
    public SMBPitchShiftingSampleProvider(ISampleProvider SourceProvider, int FFTSize, long osamp, float InitialPitch)
    {
        SourceStream = SourceProvider;
        WFormat = SourceProvider.WaveFormat;
        _FFTSize = FFTSize;
        _osamp = osamp;
        PitchFactor = InitialPitch;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (PitchLock) {
            int SampRead = SourceStream.Read(buffer, offset, count);
            if (Pitch == 1f)    
                //Nothing to do.
                return SampRead;
            if (WFormat.Channels == 1) {
                float[] Mono = new float[SampRead];
                int index = 0;
                for (int sample = offset; sample <= SampRead - 1; sample++) {
                    Mono[index] = buffer[sample];
                    index += 1;
                }
                ShifterLeft.PitchShift(Pitch, SampRead, _FFTSize, _osamp, WFormat.SampleRate, Mono);
                index = 0;
                for (int sample = offset; sample <= SampRead - 1; sample++) {
                    buffer[sample] = Mono[index] * volscale * 0.707f;
                    index += 1;
                }
                return SampRead;
            } else if (WFormat.Channels == 2) {
                float[] Left = new float[(SampRead >> 1)];
                float[] Right = new float[(SampRead >> 1)];
                int index = 0;
                for (int sample = offset; sample <= SampRead - 1; sample += 2) {
                    Left[index] = buffer[sample];
                    Right[index] = buffer[sample + 1];
                    index += 1;
                }
                ShifterLeft.PitchShift(Pitch, SampRead >> 1, _FFTSize, _osamp, WFormat.SampleRate, Left);
                ShifterRight.PitchShift(Pitch, SampRead >> 1, _FFTSize, _osamp, WFormat.SampleRate, Right);
                index = 0;
                for (int sample = offset; sample <= SampRead - 1; sample += 2) {
                    buffer[sample] = Left[index] * volscale * 0.707f;
                    buffer[sample + 1] = Right[index] * volscale * 0.707f;
                    index += 1;
                }
                return SampRead;
            } else {
                throw new Exception("Shifting of more than 2 channels is currently not supported.");
            }
        }
    }

    public NAudio.Wave.WaveFormat WaveFormat {
        get { return WFormat; }
    }

    public float PitchFactor {
        get { return Pitch; }
        set {
            lock (PitchLock) {
                Pitch = value;
                ScaleVolume(); // A Limiter would be better than linear downscaling...
            }
        }
    }

    private void ScaleVolume()
    {
        if (Pitch > 1f) {
            volscale = 1f / Pitch;
        } else {
            volscale = Pitch;
        }
    }
}
