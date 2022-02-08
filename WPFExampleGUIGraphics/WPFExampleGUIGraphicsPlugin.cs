using System;
using System.Windows;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;

// for graphics
using System.Windows.Media;
using System.Linq;
using System.Windows.Input;

namespace WPFExampleGUIGraphics
{
    public class WPFExamplePluginGUGraphicsI : AudioPluginWPF
    {
        AudioIOPort stereoInput;
        AudioIOPort stereoOutput;

        public WPFExamplePluginGUGraphicsI()
        {
            Company = "My Company";
            Website = "www.mywebsite.com";
            Contact = "contact@my.email";
            PluginName = "WPF Example GUI Graphics Plugin";
            PluginCategory = "Fx";
            PluginVersion = "1.0.0";

            // Unique 64bit ID for the plugin
            PluginID = 0x1D94658E210BD949;

            HasUserInterface = true;
            EditorWidth = 500;
            EditorHeight = 100;
        }

        public override void Initialize()
        {
            base.Initialize();

            InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Mono Input", EAudioChannelConfiguration.Stereo) };
            OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };
        }

        System.Windows.Controls.Border Border;

        WpfEditorView.UserControl1 TheView;
        Canvas Canvas;

        public override UserControl GetEditorView()
        { 
            TheView = new WpfEditorView.UserControl1();

            InitializeTheGUIGraphics();

            return TheView;
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

        double[] x;
        double[] y;
        double XMin ; double XWidth ;
        double YMin ; double YWidth ;
        Canvas NewCanvas;
       
            void InitializeTheGUIGraphics()
            {
                Canvas = TheView.MainCanvas;

                // add border to grid
                Border = new Border();
                Border.Margin = new System.Windows.Thickness(5, 5, 5, 5);
                Border.BorderBrush = new SolidColorBrush(Colors.Black);
                Border.BorderThickness = new Thickness(2, 2, 2, 2);
                Border.Width = 205.0;
                Border.Height = 90.0;
                Canvas.Children.Add(Border);
                Canvas.SetTop(Border, (EditorHeight - Border.Height)/2 );
                Canvas.SetLeft(Border, (EditorWidth - Border.Width) / 2);

                // Add new canvas to border
                NewCanvas = new Canvas();
                Border.Child = NewCanvas;
                NewCanvas.Background = new SolidColorBrush(Colors.LightGreen);
                NewCanvas.Width = 200.0;
                NewCanvas.Height = 85.0;

                x = new double[] {10.0, 20.0, 30.0, 40.0};
                y = new double[] {40.0, 30.0, 50.0, 10.0};

                XMin = x.Min(); XWidth = x.Max() - x.Min();
                YMin = y.Min(); YWidth = y.Max() - y.Min();
                double TheX; double TheY;
                PointCollection Points = new PointCollection();
                for (int i = 0; i < x.Length; i = i + 1)
                {
                    // Canvas Locations Go From Upper Left To Lower Right
                    // Data Locations Go From Lower Left To Upper Right
                    // Transform Data Locations To Canvas Locations
                    // Upper Left To Lower Left
                    TheX = ((x[i] - XMin) / XWidth) * NewCanvas.Width;
                    TheY = NewCanvas.Height * ( 1.0 - ((y[i] - YMin) / YWidth));
                    Points.Add(new Point(TheX, TheY));
                }

                System.Windows.Shapes.Polyline Polyline = new System.Windows.Shapes.Polyline();
                Polyline.StrokeThickness = 2;
                Polyline.Stroke = new SolidColorBrush(Colors.Red); ;
                Polyline.Points = Points;
                NewCanvas.Children.Add(Polyline);

                // Also need MouseLeftButtonUp, MouseMove, MouseDoubleClick to drag graphics such as line
                NewCanvas.MouseLeftButtonDown += new MouseButtonEventHandler(Canvas_MouseLeftButtonDown);
            }
            void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                System.Windows.Point dragStart = e.GetPosition(Canvas);
                System.Drawing.Point absDragStart = System.Windows.Forms.Cursor.Position;

                // Convert Canvas Location In dragStart To Data Location

                double TheX = ( (dragStart.X / NewCanvas.Width ) * XWidth) + XMin ;
                double TheY = (YWidth * (1.0 - (dragStart.Y / NewCanvas.Height))) + YMin ;
                string Msg = "";
    //  Canvas Coordinates
    //           Msg = Msg + "Mouse Loc-- X: " + dragStart.X.ToString() + ", Y: " + dragStart.Y.ToString();
    //  Data Coordinates
                Msg = Msg + "Mouse Loc-- X: " + TheX.ToString() + ", Y: " + TheY.ToString();
                System.Windows.MessageBox.Show(Msg);

                e.Handled = true;
            }
    }
}
