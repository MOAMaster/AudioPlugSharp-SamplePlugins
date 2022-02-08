using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SliderMgr
{
    public class SliderMgr
    {
        public Slider TheSlider;
        public Label TheText;

        public double CurrentValue;

        string TheFormat;

        public SliderMgr(double Min, double Max, double SmallChange, double LargeChange, double Value,
                         double Width, Canvas Parent, double SliderLeft, double SliderTop, double TextWidth, double TextLeft, double TextTop,  string Format)
        {
            TheSlider = new Slider();
            TheSlider.Minimum = Min;
            TheSlider.Maximum = Max;
            TheSlider.SmallChange = SmallChange;
            TheSlider.LargeChange = LargeChange;
            TheSlider.Value = Value;
            TheSlider.Width = Width;
            TheSlider.ValueChanged += Slider_ValueChanged;
            Parent.Children.Add(TheSlider);
            Canvas.SetLeft(TheSlider, SliderLeft);
            Canvas.SetTop(TheSlider, SliderTop);

            TheFormat = Format;
            TheText = new Label();
            TheText.Width = TextWidth;
            TheText.BorderBrush = new System.Windows.Media.SolidColorBrush(Colors.Black) ;
            TheText.BorderThickness = new Thickness(2,2,2,2);
            TheText.HorizontalContentAlignment = HorizontalAlignment.Center;
            TheText.Content = TheSlider.Value.ToString(TheFormat);
            Parent.Children.Add(TheText);
            Canvas.SetLeft(TheText, TextLeft);
            Canvas.SetTop(TheText, TextTop);

            CurrentValue = TheSlider.Value;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentValue = TheSlider.Value;
            TheText.Content = CurrentValue.ToString(TheFormat);
        }
    }
}
