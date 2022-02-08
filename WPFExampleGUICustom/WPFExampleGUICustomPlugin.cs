using System;
using System.Windows;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;

// for graphics
using System.Windows.Media;
using System.Linq;
using System.Windows.Input;

namespace WPFExampleGUICustom
{
    public class WPFExampleGUICustomPlugin: AudioPluginWPF
    {
        AudioIOPort stereoInput;
        AudioIOPort stereoOutput;

        public WPFExampleGUICustomPlugin()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            PluginName = "WPF Example GUI Custom Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // Unique 64bit ID for the plugin
            PluginID = 0x1E93658E510B19A7;

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

        System.Windows.Controls.TextBlock newText;
        System.Windows.Controls.Button    newBtn;
        System.Windows.Controls.ComboBox comboBox;

        WpfEditorView.UserControl1 TheView;
        Canvas Canvas;

        public override UserControl GetEditorView()
        { 
            TheView = new WpfEditorView.UserControl1();

            InitializeTheGUIManualPlacement();

            return TheView;
        }

        void newBtn_Click(object sender, RoutedEventArgs e)
        {
 //          System.Windows.MessageBox.Show(newBtn.Content.ToString());
           System.Windows.MessageBox.Show(comboBox.SelectedValue.ToString());
        }

        public override void Process()
        {
            base.Process();

            //           double gain = GetParameter("gain").Value;
            double gain = 1.2;
            double linearGain = Math.Pow(10.0, 0.05 * gain);

            //           double pan = GetParameter("pan").Value;
            double pan = 0.0;

            stereoInput.ReadData();

            double[] inSamplesLeft = stereoInput.GetAudioBuffers()[0];
            double[] inSamplesRight = stereoInput.GetAudioBuffers()[1];

            double[] outLeftSamples = stereoOutput.GetAudioBuffers()[0];
            double[] outRightSamples = stereoOutput.GetAudioBuffers()[1];

            for (int i = 0; i < inSamplesLeft.Length; i++)
            {
                outLeftSamples[i] = inSamplesLeft[i] * linearGain * (1 - pan);
                outRightSamples[i] = inSamplesRight[i] * linearGain * (1 + pan);
            }

            stereoOutput.WriteData();
        }

        void InitializeTheGUIManualPlacement()
        {
            Canvas = TheView.MainCanvas;

            // add controls to new canvas
            newText = new TextBlock();
            newText.Text = "Setup For Added Controls";
            newText.Width = 150;
            newText.FontWeight = FontWeights.Bold;
            newText.Margin = new System.Windows.Thickness(5, 5, 5, 5);
            Canvas.SetTop(newText, 0.0);
            Canvas.SetLeft(newText, (EditorWidth - newText.Width)/ 2.0);
            Canvas.Children.Add(newText);

            comboBox = new ComboBox();
            ComboBoxItem cboxitem1 = new ComboBoxItem();
            cboxitem1.Content = "Value 1";
            comboBox.Items.Add(cboxitem1);
            ComboBoxItem cboxitem2 = new ComboBoxItem();
            cboxitem2.Content = "Value 2";
            comboBox.Items.Add(cboxitem2);
            ComboBoxItem cboxitem3 = new ComboBoxItem();
            cboxitem3.Content = "Value 3";
            comboBox.Items.Add(cboxitem3);
            comboBox.Width = 110;
            comboBox.SelectedIndex = 0;
            comboBox.Margin = new System.Windows.Thickness(5, 5, 5, 5);
            Canvas.SetTop(comboBox, 0.25 * EditorHeight);
            Canvas.SetLeft(comboBox, (EditorWidth - comboBox.Width) / 2.0); // doesn't work
            Canvas.Children.Add(comboBox);

            newBtn = new Button();
            newBtn.Content = "The Added Button";
            newBtn.Width = 110;
            newBtn.Margin = new System.Windows.Thickness(5, 5, 5, 5);
            newBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(newBtn_Click)); ;
            Canvas.SetTop(newBtn, 0.6 * EditorHeight);
            Canvas.SetLeft(newBtn, (EditorWidth - newBtn.Width) / 2.0);
            Canvas.Children.Add(newBtn);
        }
    }
}
