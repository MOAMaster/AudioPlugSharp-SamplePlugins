using System;
using System.Windows;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;

using System.Linq;
using System.Windows.Input;

namespace WPFExampleGUI
{
    public class WPFExamplePluginGUI : AudioPluginWPF
    {
        AudioIOPort stereoInput;
        AudioIOPort stereoOutput;

        // do not process sound data below this threshold
        // or plugin crashes

        double Threshold = 0.05;

        public WPFExamplePluginGUI()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            PluginName = "WPF Example GUI Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // Unique 64bit ID for the plugin
            PluginID = 0x1D93658E410B2947;

            HasUserInterface = true;
            EditorWidth = 500;
            EditorHeight = 100;
        }

        public override void Initialize()
        {
            base.Initialize();

            InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
            OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };
        }

        Canvas Canvas;
        WpfEditorView.UserControl1 TheView;
        Button newBtn;
        TextBlock newTxt;

        SliderMgr.SliderMgr Gain;
        
        public override UserControl GetEditorView()
        {
            TheView = new WpfEditorView.UserControl1();
            
            Canvas = TheView.MainCanvas;

            newTxt = new TextBlock();
            newTxt.Text = "Adding GUI Components";
            newTxt.Width = 135;
            newTxt.Margin = new System.Windows.Thickness(5, 5, 5, 5);
            Canvas.Children.Add(newTxt);
            Canvas.SetLeft(newTxt, (EditorWidth - newTxt.Width) / 2.0);
            Canvas.SetTop(newTxt, 15);

            newBtn = new Button();
            newBtn.Content = "Click On Me";
            newBtn.Width = 110;
            newBtn.Margin = new System.Windows.Thickness(5, 5, 5, 5);
            newBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(newBtn_Click)); 
            Canvas.Children.Add(newBtn);
            Canvas.SetLeft(newBtn,40);
            Canvas.SetTop(newBtn, 50);

            // "#." No Decimal Point, "#.##"  2 decimal points, "N@" numerice with 2 decimal points
            Gain = new SliderMgr.SliderMgr(Min: -20, Max: 20, SmallChange: 1, LargeChange: 5, Value: 0.0, Width: 150, Parent: Canvas,
                                         SliderLeft: 180, SliderTop: 55, TextWidth: 48, TextLeft: 350, TextTop: 51, Format: "N2");
  
            return TheView;
        }
        void newBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(newBtn.Content.ToString());
        }
 
        public override void Process()
        {
            base.Process();

            stereoInput.ReadData();

            double[] inSamplesLeft = stereoInput.GetAudioBuffers()[0];
            double[] inSamplesRight = stereoInput.GetAudioBuffers()[1];

            double[] outSamplesLeft = stereoOutput.GetAudioBuffers()[0];
            double[] outSamplesRight = stereoOutput.GetAudioBuffers()[1];

            // Prevent silence from being processed
            // Plugin can crash without this guard
            if ((inSamplesLeft.Max() < Threshold) || (inSamplesRight.Max() < Threshold))
                return;

            double gain = Gain.CurrentValue ;

            double linearGain = Math.Pow(10.0, 0.05 * gain);

            double pan = 0.0;

            for (int i = 0; i < inSamplesLeft.Length; i++)
            {
                    outSamplesLeft[i] = inSamplesLeft[i] * linearGain * (1 - pan);
                    outSamplesRight[i] = inSamplesRight[i] * linearGain * (1 + pan);
            }

            stereoOutput.WriteData();
        }
    }
}
