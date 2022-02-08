using System;
using System.Windows.Controls;

// THIS SIMPLE EXAMPLE ILLUSTRATES:
// A: define a custom user interface by employing a UserControl, Canvas, and
//     overriding GetEditorView, libary component WpfEditorView
// B: place controls onto Canvas at specific locations using SetTop, SetLeft
// C: employ your own non-WPF user control in the custom interface (Rotary Control)
// D: ensure that your own non-WPF user control can be accessed across thread boundaries
// E:  mix a non-WPF user control with WPF controls
// F: ensure that your user interface is stable,
//    given that the host executes plugin methods in ways
//    that are not specified and arbitarily implemend by the host
// G: ensure that your plugin is stable if your algorithm fails
//    due to inappropriate parameter values set by the user
//     or when executing method Process during silence
// H: do not update local variables if user has not
//    changed them in user control
// 

// NOTE: vstsdk executes plugin in a separate thread
//      any user control implement in GetEditorView has thread affinity to UI thread
//      plugin access to value of user control crosses thread boundaries
//      user control implementation must wrap access to value in Dispatcher.Invoke
//
//  THE STEPS TO FOLLOW ARE NUMBERED THROUGHOUT THE CODE


// 1. add project dependencies to:
//    AudioPlugSharp, AudioPlugSharpWPF
//    DialMgr, RotaryControl, WpfEditorView
//    our your own versions for your application
// 2  add using declarations to simplify access to methods
using AudioPlugSharp;
using AudioPlugSharpWPF;
using System.Linq;
using System.Windows;
using DialMgr;

namespace WPFExampleGUIKnobs
{
    public class WPFExampleGUIKnobs : AudioPluginWPF
    {
        AudioIOPort stereoInput;
        AudioIOPort stereoOutput;

        // 3
        // do not process sound data below this threshold
        // or algorithm can crash plugin
        double Threshold = 0.01;

        public WPFExampleGUIKnobs()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            // 4: change this as a minimum
            //    change others also
            PluginName = "WPF Example GUI Knobs Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // 5:  Unique 64bit ID for the plugin
            PluginID = 0x8F93558E214B5F41;

            HasUserInterface = true;
            //6 Size to accomodate controls
            EditorWidth = 470;
            EditorHeight = 200;
        }

        public override void Initialize()
        {
            base.Initialize();

            InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
            OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };
        }

        // 7: permantently store references to all controls
        // View and Canvas defined in wpfEditorView
        Canvas MainCanvas;
        WpfEditorView.UserControl1 TheView;
        // 8: user controls
        DialMgr.DialMgr DialMgr;
        RotaryControls.RotaryControl GainDial;
        RotaryControls.RotaryControl PanDial;
        // 9: WPF controls
        TextBlock GainLabel;
        TextBlock PanLabel;

        // GetEditorView called multiple times by Host
        // under undefined circustances
        // 10: variable to limit initialization to 1 time
        Boolean Initialized = false;

        // 11: Overide GetEditorView and create GUI
        public override UserControl GetEditorView()
        {
            // plugin host calls this under a number of circumstances:
            // Add, Open after close, move in chain
            // Limit initialization to once
            // 12: limit initialization to 1 time
            if (Initialized)
            {
                return TheView;
            }
            Initialized = true;

            // 13: define user interface including user control
            TheView = new WpfEditorView.UserControl1();
            MainCanvas = TheView.MainCanvas;

            DialMgr = new DialMgr.DialMgr();
            (GainLabel, GainDial) = DialMgr.CreateLabelDialCombination(LabelText: "Gain (dB)", LabelLeft: 100, LabelTop: 27,
                                                               DialMin: -20, DialMax: 20, DialCurrent: 0, DialTick: 4,
                                                               DangerZone: DangerZones.DangerLowHigh, DialLeft: 50, DialTop: 35,
                                                               Container: MainCanvas);

            (PanLabel, PanDial) = DialMgr.CreateLabelDialCombination(LabelText: "Pan(% Left / Right)", LabelLeft: 270, LabelTop: 27,
                                                               DialMin: -1, DialMax: 1, DialCurrent: 0, DialTick: 0.2,
                                                               DangerZone: DangerZones.DangerLow, DialLeft: 250, DialTop: 35,
                                                               Container: MainCanvas);
            // requires a method in RotaryControl to wrap
            // access in Dispatcher.Invoke to defeat cross
            // thread limitation
            // 14: Store the GUI interface values into class variables
            InitializeClassVariables();
            // 15: Use the class variables to update the algorithm if needed
            InitializeAlgorithmParameters();

             return TheView;
        }

        // 16: declare class variables to house current value
        //     of property of user control
        double gain;
        double pan;
        double linearGain;

        // 17: move outside the Process method
        //     for access by other methods in class
        double[] inSamplesLeft ;
        double[] inSamplesRight ;

        double[] outSamplesLeft ;
        double[] outSamplesRight ;

         public override void Process()
        {
            base.Process();

            stereoInput.ReadData();

            inSamplesLeft = stereoInput.GetAudioBuffers()[0];
            inSamplesRight = stereoInput.GetAudioBuffers()[1];

            outSamplesLeft = stereoOutput.GetAudioBuffers()[0];
            outSamplesRight = stereoOutput.GetAudioBuffers()[1];

            // Prevent silence from being processed
            // Plugin can crash without this guard
            // 18: ignore silence based on Theshold
            if ((inSamplesLeft.Max() < Threshold) || (inSamplesRight.Max() < Threshold))
                return;

            double TheGain = gain;
            double ThePan = pan;

            // 19: hangs on stop audio without try/catch protection
            try
            {
                // requires a method in RotaryControl to wrap
                // access in Dispatcher.Invoke to defeat cross
                // thread limitation
                // 17: access user control properties from plugin host thread
                //     to GUI thread using wrapper method
                //     store in local variables
                TheGain = GainDial.GetCurrentValue();
                ThePan = PanDial.GetCurrentValue();
            }
            catch (Exception e)
            {
                MessageBox.Show("Process, Reading Dials: " + e.Message);
                CopyInputToOutput();
                return;
            }

            // 20: if user control parameters do not change,
            //     do not update local variables
            if ((gain != TheGain) || (pan != ThePan))
            {
                gain = TheGain;
                linearGain = Math.Pow(10.0, 0.05 * TheGain);
                pan = ThePan;
                InitializeClassVariables();
            }

            // 21: embed algorithm in a try catch block
            //     in case algorithm fails
            try
            { 
            for (int i = 0; i < inSamplesLeft.Length; i++)
            {
                outSamplesLeft[i] = inSamplesLeft[i] * linearGain * (1 - pan);
                outSamplesRight[i] = inSamplesRight[i] * linearGain * (1 + pan);
            }
            }
            catch (Exception e)
            {
                // 22: if applying algorithm fails, copy input to output
                //     without applying algorithm
                MessageBox.Show("Process, Applying Effect: " + e.Message);
                CopyInputToOutput();
                return;
            }

            stereoOutput.WriteData();
        }

        // 23: store GUI values into class values 

        public void InitializeClassVariables()
        {
            // need try/catch block here in case
            // user opens/closes in the presence
            // of other vst plugins or adds during
            // playing of audio stream
            // 24: employ wrapped access to property
            //     wrap in try/catch block
            try
            {
                gain = GainDial.GetCurrentValue();
                linearGain = Math.Pow(10.0, 0.05 * gain);
                pan = PanDial.GetCurrentValue();
            }
            catch (Exception e)
            {
                MessageBox.Show("GetEditorView, Reading Dials: " + e.Message);
            }

        }

        // 25: Use the class variables to update the algorithm if needed
        public void InitializeAlgorithmParameters()
        {

        }

        // 26: simple method to copy input to output
        //     needed for exception handling during processing
        public void CopyInputToOutput()
        {
            Array.Copy(inSamplesLeft, outSamplesLeft, inSamplesLeft.Length);
            Array.Copy(inSamplesRight, outSamplesRight, inSamplesRight.Length);
        }

    }
}