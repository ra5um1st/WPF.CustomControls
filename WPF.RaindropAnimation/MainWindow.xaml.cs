using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF.RaindropAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Point elementMousePosition;
        private double magnetazedBoundX;
        private bool isMagnetized;

        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;

            elementMousePosition = Mouse.GetPosition(element);
            magnetazedBoundX = 15;
            raindropEllipse.Visibility = Visibility.Visible;

            rect.CaptureMouse();
        }

        private void rect_MouseMove(object sender, MouseEventArgs e)
        {
            var element = (FrameworkElement)sender;

            if (element.IsMouseCaptured)
            {
                var newElementPosition = Mouse.GetPosition(canvas) - elementMousePosition;
                var magnetazedElementPosition = new Point(Canvas.GetLeft(line), Canvas.GetTop(line));

                if(Math.Abs(newElementPosition.X - magnetazedElementPosition.X) < magnetazedBoundX)
                {
                    Canvas.SetTop(element, newElementPosition.Y);
                    Canvas.SetLeft(element, magnetazedElementPosition.X);

                    //Анимация
                    if (!isMagnetized)
                    {
                        Canvas.SetLeft(raindropEllipse, Canvas.GetLeft(element) - raindropEllipse.Width / 2);
                        Canvas.SetTop(raindropEllipse, Canvas.GetTop(element) - element.Height / 2);

                        var storyboard = (Storyboard)raindropEllipse.Resources["storyboard"];
                        storyboard.Begin();
                    }

                    isMagnetized = true;
                }
                else
                {
                    Canvas.SetTop(element, newElementPosition.Y);
                    Canvas.SetLeft(element, newElementPosition.X);

                    isMagnetized = false;
                }
            }
        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.ReleaseMouseCapture();
        }
    }
}
