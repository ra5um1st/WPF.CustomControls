using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Timeline.Timeline.TypeConverters;

namespace WPF_Timeline.Timeline
{
    internal class TimelinePanel : Panel, ITimeline
    {
        public int IntervalsCount => (int)Math.Ceiling((MaxTimeSpan - MinTimeSpan) / IntervalTimeSpan);
        public double TotalWidth => IntervalsCount * IntervalWidth;

        #region Dependency Properties

        public TimeSpan MinTimeSpan
        {
            get { return (TimeSpan)GetValue(MinTimeSpanProperty); }
            set { SetValue(MinTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty MinTimeSpanProperty =
            DependencyProperty.Register(nameof(MinTimeSpan), typeof(TimeSpan), typeof(TimelinePanel), 
                new PropertyMetadata(TimeSpan.FromDays(0), OnMinTimeSpanPropertyChanged));

        private static void OnMinTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelinePanel timelinePanel = (TimelinePanel)d;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateVisual();
        }

        public TimeSpan MaxTimeSpan
        {
            get { return (TimeSpan)GetValue(MaxTimeSpanProperty); }
            set { SetValue(MaxTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty MaxTimeSpanProperty =
            DependencyProperty.Register(nameof(MaxTimeSpan), typeof(TimeSpan), typeof(TimelinePanel), 
                new PropertyMetadata(TimeSpan.FromDays(1), OnMaxTimeSpanPropertyChanged));

        private static void OnMaxTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelinePanel timelinePanel = (TimelinePanel)d;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateVisual();
        }

        public TimeSpan IntervalTimeSpan
        {
            get { return (TimeSpan)GetValue(IntervalTimeSpanProperty); }
            set { SetValue(IntervalTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty IntervalTimeSpanProperty =
            DependencyProperty.Register(nameof(IntervalTimeSpan), typeof(TimeSpan), typeof(TimelinePanel), 
                new PropertyMetadata(TimeSpan.FromDays(1), OnIntervalTimeSpanPropertyChanged));

        private static void OnIntervalTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelinePanel timelinePanel = (TimelinePanel)d;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateVisual();
        }

        public double IntervalWidth
        {
            get { return (double)GetValue(IntervalWidthProperty); }
            set { SetValue(IntervalWidthProperty, value); }
        }

        public static readonly DependencyProperty IntervalWidthProperty =
            DependencyProperty.Register(nameof(IntervalWidth), typeof(double), typeof(TimelinePanel), 
                new PropertyMetadata(25.0, OnIntervalWidthPropertyChanged));

        private static void OnIntervalWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelinePanel timelinePanel = (TimelinePanel)d;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateVisual();
        }

        #endregion

        #region Attached Properties

        public static TimeSpan GetStartTimeSpan(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(StartTimeSpanProperty);
        }
        public static void SetStartTimeSpan(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(StartTimeSpanProperty, value);
        }
        public static readonly DependencyProperty StartTimeSpanProperty = 
            DependencyProperty.RegisterAttached("StartTimeSpan", typeof(TimeSpan), typeof(TimelinePanel),
                new PropertyMetadata(TimeSpan.MinValue, OnStartTimeSpanPropertyChanged));

        private static void OnStartTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            if (!typeof(TimelinePanel).IsInstanceOfType(element.Parent))
            {
                return;
            }

            TimelinePanel timelinePanel = (TimelinePanel)element.Parent;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateArrange();
        }

        public static TimeSpan GetFinishTimeSpan(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(FinishTimeSpanProperty);
        }

        public static void SetFinishTimeSpan(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(FinishTimeSpanProperty, value);
        }

        public static readonly DependencyProperty FinishTimeSpanProperty = 
            DependencyProperty.RegisterAttached("FinishTimeSpan", typeof(TimeSpan), typeof(TimelinePanel),
                new PropertyMetadata(TimeSpan.MinValue.Add(TimeSpan.FromDays(1)), OnFinishTimeSpanPropertyChanged));

        private static void OnFinishTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            if (!typeof(TimelinePanel).IsInstanceOfType(element.Parent))
            {
                return;
            }

            TimelinePanel timelinePanel = (TimelinePanel)element.Parent;

            timelinePanel.InvalidateMeasure();
            timelinePanel.InvalidateArrange();
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            //if(TotalWidth > availableSize.Width || double.IsInfinity(availableSize.Width))
            //{
            //    availableSize.Width = TotalWidth;
            //}

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }

            //var lastChild = GetChildWithMaxDateTime();
            //var maxWidth = GetFinishTimeSpan(lastChild) / IntervalTimeSpan * IntervalWidth;
            var maxSize = new Size(TotalWidth, GetChildWithMaxHeight().DesiredSize.Height);

            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = maxSize.Width;
            }
            if (double.IsInfinity(availableSize.Height))
            {
                availableSize.Height = maxSize.Height;
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (FrameworkElement child in InternalChildren)
            {
                var childStartTime = TimelinePanel.GetStartTimeSpan(child);
                var childFinishTime = TimelinePanel.GetFinishTimeSpan(child);

                if(childStartTime == TimeSpan.MaxValue || childStartTime == TimeSpan.MinValue)
                {
                    return finalSize;
                }

                var startOffset = childStartTime - MinTimeSpan;
                var durationOffset = childFinishTime - childStartTime;

                var offsetX = (startOffset / IntervalTimeSpan) * IntervalWidth;
                var durationWidth = (durationOffset / IntervalTimeSpan) * IntervalWidth;

                var anchorPoint = new Point(offsetX, 0);

                if(durationWidth < 0 || offsetX < 0)
                {
                    return finalSize;
                }

                child.Arrange(new Rect(anchorPoint, new Size(durationWidth, child.DesiredSize.Height)));
            }

            return finalSize;
        }
        private UIElement GetChildWithMinDateTime()
        {
            var resultChild = new UIElement();

            if(InternalChildren.Count != 0)
            {
                resultChild = InternalChildren[0];
            }

            foreach (UIElement childItem in InternalChildren)
            {
                if (TimelinePanel.GetStartTimeSpan(resultChild) > TimelinePanel.GetFinishTimeSpan(childItem))
                {
                    resultChild = childItem;
                }
            }

            return resultChild;
        }
        private UIElement GetChildWithMaxDateTime()
        {
            var resultChild = new UIElement();

            if (InternalChildren.Count != 0)
            {
                resultChild = InternalChildren[0];
            }

            foreach (UIElement childItem in InternalChildren)
            {
                if (TimelinePanel.GetFinishTimeSpan(resultChild) < TimelinePanel.GetFinishTimeSpan(childItem))
                {
                    resultChild = childItem;
                }
            }

            return resultChild;
        }
        private UIElement GetChildWithMaxHeight()
        {
            var resultChild = new UIElement();

            if (InternalChildren.Count != 0)
            {
                resultChild = InternalChildren[0];
            }

            foreach (UIElement child in InternalChildren)
            {
                if(resultChild.DesiredSize.Height < child.DesiredSize.Height)
                {
                    resultChild = child;
                }
            }

            return resultChild;
        }
        private UIElement GetChildWithMaxWidth()
        {
            var resultChild = new UIElement();

            if (InternalChildren.Count != 0)
            {
                resultChild = InternalChildren[0];
            }

            foreach (UIElement child in InternalChildren)
            {
                if (resultChild.DesiredSize.Width < child.DesiredSize.Width)
                {
                    resultChild = child;
                }
            }

            return resultChild;
        }
    }
}
