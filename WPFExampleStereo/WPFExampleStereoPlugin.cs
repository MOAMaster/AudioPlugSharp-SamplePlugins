using System;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;

namespace WPFExampleStereo
{
    public class WPFExampleStereoPlugin : AudioPluginWPF
    { 
        AudioIOPort stereoInput;
        AudioIOPort stereoOutput;   

        public WPFExampleStereoPlugin()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            PluginName = "WPF Example Stereo Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // Unique 64bit ID for the plugin
            PluginID = 0x2E92718E714B49D7;

            HasUserInterface = true;
            EditorWidth = 200;
            EditorHeight = 100;
        }

        public override void Initialize()
        {
            base.Initialize();

            InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
            OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };

            AddParameter(new AudioPluginParameter
            {
                ID = "gain",
                Name = "Gain",
                Type = EAudioPluginParameterType.Float,
                MinValue = -20,
                MaxValue = 20,
                DefaultValue = 0,
                ValueFormat = "{0:0.0}dB"
            });

            AddParameter(new AudioPluginParameter
            {
                ID = "pan",
                Name = "Pan",
                Type = EAudioPluginParameterType.Float,
                MinValue = -1,
                MaxValue = 1,
                DefaultValue = 0,
                ValueFormat = "{0:0.0}"
            });
        }

        public override void Process()
        {
            base.Process();

            double gain = GetParameter("gain").Value;
            double linearGain = Math.Pow(10.0, 0.05 * gain);

            double pan = GetParameter("pan").Value;

            stereoInput.ReadData();

            double[] inLeftSamples = stereoInput.GetAudioBuffers()[0];
            double[] inRightSamples = stereoInput.GetAudioBuffers()[1];

            double[] outLeftSamples = stereoOutput.GetAudioBuffers()[0];
            double[] outRightSamples = stereoOutput.GetAudioBuffers()[1];

            // if empty buffers sent, skip processing, just copy buffer of zeros
            try
            {
                for (int i = 0; i < inLeftSamples.Length; i++)
            {
                outLeftSamples[i] = inLeftSamples[i] * linearGain * (1 - pan);
                outRightSamples[i] = inRightSamples[i] * linearGain * (1 + pan);
            }
            }
            catch (Exception e)
            {
                Array.Copy(inLeftSamples, outLeftSamples, inLeftSamples.Length);
                Array.Copy(inRightSamples, outRightSamples, inRightSamples.Length);
            }

            stereoOutput.WriteData();
        }
    }
}
