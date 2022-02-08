﻿using System;
using AudioPlugSharp;

namespace SimpleExample
{
    public class SimpleExamplePlugin : AudioPluginBase
    {
        public SimpleExamplePlugin()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            PluginName = "Simple Gain Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // Unique 64bit ID for the plugin
            PluginID = 0xF57703946AFC4EF8;
        }

        AudioIOPort monoInput;
        AudioIOPort monoOutput;

        public override void Initialize()
        {
            base.Initialize();

            InputPorts = new AudioIOPort[] { monoInput = new AudioIOPort("Mono Input", EAudioChannelConfiguration.Mono) };
            OutputPorts = new AudioIOPort[] { monoOutput = new AudioIOPort("Mono Output", EAudioChannelConfiguration.Mono) };

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
        }

        public override void Process()
        {
            base.Process();

            double gain = GetParameter("gain").Value;
            double linearGain = Math.Pow(10.0, 0.05 * gain);

            monoInput.ReadData();

            double[] inSamples = monoInput.GetAudioBuffers()[0];
            double[] outSamples = monoOutput.GetAudioBuffers()[0];

            for (int i = 0; i < inSamples.Length; i++)
            {
                outSamples[i] = inSamples[i] * linearGain;
            }

            monoOutput.WriteData();
        }
    }
}
