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
using System.Windows.Threading;
using WPF_Timeline.Timeline;

namespace WPF_Timeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private DoubleAnimation intervalWidthAnimation;
        private DoubleAnimation intervalTimeSpanAnimation;

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (Math.Abs(intervalWidthWheelAcceleration) < 0.05)
            {
                intervalWidthWheelAcceleration = 0;
                isIntervalWidthAnimationActive = false;
            }
            if (Math.Abs(intervalTimeSpanWheelAcceleration) < 0.05)
            {
                intervalTimeSpanWheelAcceleration = 0;
                isIntervalTimeSpanAnimationActive = false;
            }

            if (isIntervalWidthAnimationActive)
            {
                var animatedIntevalWidth = EaseOutExpo(1 - Math.Abs(intervalWidthWheelAcceleration)) + 0.15;
                var newIntervalWidth = Lerp(animatedIntevalWidth, startIntervalWidthAnimationValue, finishIntervalWidthAnimationValue);
                timelineControl.IntervalWidth = newIntervalWidth;
                intervalWidthWheelAcceleration += 0.02 * Math.Sign(-intervalWidthWheelAcceleration);
            }
            if (isIntervalTimeSpanAnimationActive)
            {
                var animatedIntevalTimeSpan = EaseOutExpo(1 - Math.Abs(intervalTimeSpanWheelAcceleration)) + 0.15;
                var newIntervalTimeSpan = Lerp(animatedIntevalTimeSpan, startIntervalTimeSpanAnimationValue, finishIntervalTimeSpanAnimationValue);
                timelineControl.IntervalTimeSpan = newIntervalTimeSpan;
                intervalTimeSpanWheelAcceleration += 0.02 * Math.Sign(-intervalTimeSpanWheelAcceleration);
            }
        }

        private double Lerp(double value, double minValue, double maxValue)
        {
            return minValue + (maxValue - minValue) * value;
        }
        private TimeSpan Lerp(double value, TimeSpan minValue, TimeSpan maxValue)
        {
            return minValue + (maxValue - minValue) * value;
        }
        private double Normalize(double value, double minValue, double maxValue)
        {
            return (value - minValue) / (maxValue - minValue);
        }
        private double Normalize(TimeSpan value, TimeSpan minValue, TimeSpan maxValue)
        {
            return (value - minValue) / (maxValue - minValue);
        }
        private double EaseOutExpo(double value)
        {
            if(value > 1)
            {
                return 1;
            }
            if(value < 0)
            {
                return 0;
            }

            return value == 1 ? 1 : 1 - Math.Pow(2, -10 * value);
        }

        private Point startElementMousePosition;
        private TimeSpan startElementDuration;
        private Rect leftEdgeRect;
        private Rect rightEdgeRect;
        private bool isRightEdgeCaptured;
        private bool isLeftEdgeCaptured;
        private bool isBodyCaptured;

        private void OnTimelinePanelChildMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            TimelinePanel timelinePanel = (TimelinePanel)element.Parent;

            startElementMousePosition = e.GetPosition(element);
            startElementDuration = TimelinePanel.GetFinishTimeSpan(element) - TimelinePanel.GetStartTimeSpan(element);

            if (leftEdgeRect.Contains(startElementMousePosition))
            {
                isLeftEdgeCaptured = true;
            }
            else if (rightEdgeRect.Contains(startElementMousePosition))
            {
                isRightEdgeCaptured = true;
            }
            else
            {
                isBodyCaptured = true;
            }

            element.CaptureMouse();
        }

        private void OnTimelinePanelChildMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            TimelinePanel timelinePanel = (TimelinePanel)element.Parent;

            leftEdgeRect = new Rect(new Point(-5, -5), new Point(5, 5 + element.ActualHeight));
            rightEdgeRect = new Rect(new Point(-5 + element.ActualWidth, -5), new Point(5 + element.ActualWidth, 5 + element.ActualHeight));

            var elementMousePosition = e.GetPosition(element);
            var timelinePanelMousePosition = e.GetPosition(timelinePanel);

            if (!element.IsMouseCaptured)
            {
                if (leftEdgeRect.Contains(elementMousePosition))
                {
                    element.Cursor = Cursors.SizeWE;
                }
                else if (rightEdgeRect.Contains(elementMousePosition))
                {
                    element.Cursor = Cursors.SizeWE;
                }
                else
                {
                    element.Cursor = Cursors.Hand;
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                element.CaptureMouse();

                var positionDifference = timelinePanelMousePosition - startElementMousePosition;
                var newStartTimeSpan = positionDifference.X / timelinePanel.IntervalWidth * timelinePanel.IntervalTimeSpan + timelinePanel.MinTimeSpan;
                var newFinishTimeSpan = newStartTimeSpan + startElementDuration;

                var nearestStartInterval = Math.Round(newStartTimeSpan / timelinePanel.IntervalTimeSpan) * timelinePanel.IntervalTimeSpan;
                var nearestFinishInterval = Math.Round(newFinishTimeSpan / timelinePanel.IntervalTimeSpan) * timelinePanel.IntervalTimeSpan;

                var magnetizeTimeSpanOffset = timelinePanel.IntervalTimeSpan * 0.1 / (timelinePanel.IntervalWidth * 0.025);

                if (isBodyCaptured)
                {
                    //Примагничивание к интервалам
                    if ((nearestStartInterval - newStartTimeSpan).Duration() <= magnetizeTimeSpanOffset)
                    {
                        TimelinePanel.SetStartTimeSpan(element, nearestStartInterval);
                        TimelinePanel.SetFinishTimeSpan(element, nearestStartInterval + startElementDuration);
                    }
                    else
                    {
                        TimelinePanel.SetStartTimeSpan(element, newStartTimeSpan);
                        TimelinePanel.SetFinishTimeSpan(element, newStartTimeSpan + startElementDuration);
                    }
                }
                if (isLeftEdgeCaptured)
                {
                    if ((nearestStartInterval - newStartTimeSpan).Duration() <= magnetizeTimeSpanOffset)
                    {
                        TimelinePanel.SetStartTimeSpan(element, nearestStartInterval);
                    }
                    else
                    {
                        TimelinePanel.SetStartTimeSpan(element, newStartTimeSpan);
                    }
                }
                if (isRightEdgeCaptured)
                {
                    if ((nearestFinishInterval - newFinishTimeSpan).Duration() <= magnetizeTimeSpanOffset)
                    {
                        TimelinePanel.SetFinishTimeSpan(element, nearestFinishInterval);
                    }
                    else
                    {
                        TimelinePanel.SetFinishTimeSpan(element, newFinishTimeSpan);
                    }
                }
            }
        }

        private void OnTimelinePanelChildMouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            isBodyCaptured = false;
            isLeftEdgeCaptured = false;
            isRightEdgeCaptured = false;

            element.ReleaseMouseCapture();
        }

        private double intervalWidthWheelAcceleration;
        private double startIntervalWidthAnimationValue;
        private double finishIntervalWidthAnimationValue;
        private bool isIntervalWidthAnimationActive;

        private double intervalTimeSpanWheelAcceleration;
        private TimeSpan startIntervalTimeSpanAnimationValue;
        private TimeSpan finishIntervalTimeSpanAnimationValue;
        private bool isIntervalTimeSpanAnimationActive;

        private void OnTimelineControlMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)sender;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                intervalWidthWheelAcceleration = e.Delta / Math.Abs(e.Delta);

                startIntervalWidthAnimationValue = timelineControl.IntervalWidth;
                finishIntervalWidthAnimationValue =
                    timelineControl.IntervalWidth
                    + 0.1 * (timelineControl.MaxIntervalWidth - timelineControl.MinIntervalWidth)
                    * Math.Sign(intervalWidthWheelAcceleration);
                isIntervalWidthAnimationActive = true;
            }
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                intervalTimeSpanWheelAcceleration = e.Delta / Math.Abs(e.Delta);
                intervalTimeSpanWheelAcceleration = -intervalTimeSpanWheelAcceleration;

                startIntervalTimeSpanAnimationValue = timelineControl.IntervalTimeSpan;
                finishIntervalTimeSpanAnimationValue = 
                    timelineControl.IntervalTimeSpan 
                    + 0.1 * (timelineControl.MaxIntervalTimeSpan - timelineControl.MinIntervalTimeSpan) 
                    * Math.Sign(intervalTimeSpanWheelAcceleration);
                isIntervalTimeSpanAnimationActive = true;
            }
        }
    }
}
