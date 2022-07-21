using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using WPF_Timeline.Timeline.TypeConverters;

namespace WPF_Timeline.Timeline
{
    internal class TimelineControl : ContentControl, ITimeline
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
            DependencyProperty.Register(nameof(MinTimeSpan), typeof(TimeSpan), typeof(TimelineControl), 
                new PropertyMetadata(TimeSpan.FromHours(0), OnMinTimeSpanPropertyChanged));

        private static void OnMinTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;
        }

        public TimeSpan MaxTimeSpan
        {
            get { return (TimeSpan)GetValue(MaxTimeSpanProperty); }
            set { SetValue(MaxTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty MaxTimeSpanProperty =
            DependencyProperty.Register(nameof(MaxTimeSpan), typeof(TimeSpan), typeof(TimelineControl), 
                new PropertyMetadata(TimeSpan.FromHours(1), OnMaxTimeSpanPropertyChanged));

        private static void OnMaxTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;
        }

        public double IntervalWidth
        {
            get { return (double)GetValue(IntervalWidthProperty); }
            set { SetValue(IntervalWidthProperty, value); }
        }

        public static readonly DependencyProperty IntervalWidthProperty =
            DependencyProperty.Register(nameof(IntervalWidth), typeof(double), typeof(TimelineControl), 
                new PropertyMetadata(25.0, OnIntervalWidthPropertyChanged, CoerceIntervalWidthPropertyValue));

        private static object CoerceIntervalWidthPropertyValue(DependencyObject d, object baseValue)
        {
            TimelineControl timelineControl = (TimelineControl)d;

            if ((double)baseValue < timelineControl.MinIntervalWidth)
            {
                return timelineControl.MinIntervalWidth;
            }
            if ((double)baseValue > timelineControl.MaxIntervalWidth)
            {
                return timelineControl.MaxIntervalWidth;
            }

            return baseValue;
        }

        private static void OnIntervalWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;

            timelineControl.InvalidateVisual();
        }

        public double MinIntervalWidth
        {
            get { return (double)GetValue(MinIntervalWidthProperty); }
            set { SetValue(MinIntervalWidthProperty, value); }
        }

        public static readonly DependencyProperty MinIntervalWidthProperty =
            DependencyProperty.Register(nameof(MinIntervalWidth), typeof(double), typeof(TimelineControl), new PropertyMetadata(10.0));

        public double MaxIntervalWidth
        {
            get { return (double)GetValue(MaxIntervalWidthProperty); }
            set { SetValue(MaxIntervalWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxIntervalWidthProperty =
            DependencyProperty.Register(nameof(MaxIntervalWidth), typeof(double), typeof(TimelineControl), new PropertyMetadata(100.0));


        public TimeSpan IntervalTimeSpan
        {
            get { return (TimeSpan)GetValue(IntervalTimeSpanProperty); }
            set { SetValue(IntervalTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty IntervalTimeSpanProperty =
            DependencyProperty.Register(nameof(IntervalTimeSpan), typeof(TimeSpan), typeof(TimelineControl), 
                new FrameworkPropertyMetadata(TimeSpan.FromSeconds(10), OnIntervalTimeSpanPropertyChanged, CoerceIntervalTimeSpanValue));

        private static void OnIntervalTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;

            timelineControl.InvalidateVisual();
        }

        private static object CoerceIntervalTimeSpanValue(DependencyObject d, object baseValue)
        {
            TimelineControl timelineControl = (TimelineControl)d;

            if ((TimeSpan)baseValue < timelineControl.MinIntervalTimeSpan)
            {
                return timelineControl.MinIntervalTimeSpan;
            }

            return baseValue;
        }

        public TimeSpan MinIntervalTimeSpan
        {
            get { return (TimeSpan)GetValue(MinIntervalTimeSpanProperty); }
            set { SetValue(MinIntervalTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty MinIntervalTimeSpanProperty =
            DependencyProperty.Register(nameof(MinIntervalTimeSpan), typeof(TimeSpan), typeof(TimelineControl),
                new PropertyMetadata(TimeSpan.FromMinutes(1), OnMinIntervalTimeSpanPropertyChanged));

        private static void OnMinIntervalTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;
        }

        public TimeSpan MaxIntervalTimeSpan
        {
            get { return (TimeSpan)GetValue(MaxIntervalTimeSpanProperty); }
            set { SetValue(MaxIntervalTimeSpanProperty, value); }
        }

        public static readonly DependencyProperty MaxIntervalTimeSpanProperty =
            DependencyProperty.Register(nameof(MaxIntervalTimeSpan), typeof(TimeSpan), typeof(TimelineControl), 
                new PropertyMetadata(TimeSpan.FromMinutes(10), OnMaxIntervalTimeSpanPropertyChanged));

        private static void OnMaxIntervalTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineControl timelineControl = (TimelineControl)d;
        }


        #endregion

        #region Private Fields

        private ScrollViewer PART_ScrollViewer;
        private ContentPresenter PART_ContentPresenter;

        #endregion

        #region Constructor

        public TimelineControl()
        {
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            var text = new FormattedText(
                "00:00:00",
                defaultCultureInfo,
                FlowDirection.LeftToRight,
                defaultTypeface,
                fontSize,
                foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            MinIntervalWidth = text.Width + textMargin.Left + textMargin.Right;
        }
        #endregion

        #region Static Constructor

        static TimelineControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineControl), new FrameworkPropertyMetadata(typeof(TimelineControl)));
        }

        #endregion

        public override void OnApplyTemplate()
        {
            PART_ScrollViewer = (ScrollViewer)GetTemplateChild(nameof(PART_ScrollViewer));
            PART_ContentPresenter = (ContentPresenter)GetTemplateChild(nameof(PART_ContentPresenter));

            PART_ScrollViewer.ScrollChanged += PART_ScrollViewer_ScrollChanged;
        }

        private void PART_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        private Pen emptyPen = new Pen();
        private Pen markupLinePen = new Pen(Brushes.SlateGray, 0.5);
        private Pen underlinePen = new Pen(Brushes.DarkSlateGray, 1);
        private Brush foreground = Brushes.Black;
        private CultureInfo defaultCultureInfo = new CultureInfo("ru");
        private Typeface defaultTypeface = new Typeface(new FontFamily("Segou UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        private int fontSize = 12;
        private Thickness textMargin = new Thickness(5);

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(PART_ScrollViewer == null)
            {
                return;
            }

            var visibleBounds = new Rect(
                PART_ScrollViewer.HorizontalOffset,
                PART_ScrollViewer.VerticalOffset,
                PART_ScrollViewer.RenderSize.Width,
                PART_ScrollViewer.RenderSize.Height);

            var firstIntervalIndex = (int)(visibleBounds.Left / IntervalWidth);
            var lastIntervalIndex = (int)Math.Ceiling(visibleBounds.Right / IntervalWidth);

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, visibleBounds.Width, visibleBounds.Height)));
            drawingContext.DrawRectangle(Background, emptyPen, new Rect(0, 0, ActualWidth, ActualHeight));

            for (int i = firstIntervalIndex + 1; i <= lastIntervalIndex; i++)
            {
                var offsetX = visibleBounds.Left % IntervalWidth;
                var markupLinePositionX = IntervalWidth * (i - firstIntervalIndex) - offsetX;

                //Вертикальные линии разметки
                var markupLinePoint1 = new Point(markupLinePositionX, 0);
                var markupLinePoint2 = new Point(markupLinePositionX, this.ActualHeight);

                drawingContext.PushGuidelineSet(
                    new GuidelineSet(
                        new double[] { markupLinePositionX + markupLinePen.Thickness / 2 },
                        new double[] { }));

                drawingContext.DrawLine(markupLinePen, markupLinePoint1, markupLinePoint2);
                drawingContext.Pop();

                //Тект с временем
                var currentInterval = IntervalTimeSpan * i;

                var text = new FormattedText(
                    currentInterval.ToString(@"hh\:mm\:ss"),
                    defaultCultureInfo,
                    FlowDirection.LeftToRight,
                    defaultTypeface,
                    fontSize,
                    foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                var origin = new Point(markupLinePositionX - text.Width / 2, 0);

                //Прямоугольный фон для текста
                drawingContext.DrawRectangle(
                    Background,
                    emptyPen,
                    new Rect(origin,
                    new Size(text.Width + textMargin.Right + textMargin.Left, text.Height + textMargin.Top + textMargin.Bottom)));
                drawingContext.DrawText(text, new Point(origin.X + textMargin.Left, origin.Y + textMargin.Top));
            }

            var underlinePoint1 = new Point(0, defaultTypeface.FontFamily.LineSpacing * fontSize + textMargin.Top + textMargin.Bottom);
            var underlinePoint2 = new Point(this.ActualWidth, defaultTypeface.FontFamily.LineSpacing * fontSize + textMargin.Top + textMargin.Bottom);

            if (this.Content != null)
            {
                var element = (UIElement)this.Content;
                element.Arrange(new Rect(underlinePoint1.X, underlinePoint1.Y, PART_ContentPresenter.ActualWidth, PART_ContentPresenter.ActualHeight - underlinePoint1.Y));
            }

            //Линия, ограничивающая текст
            drawingContext.PushGuidelineSet(new GuidelineSet(new double[] { }, new double[] { underlinePoint1.Y + underlinePen.Thickness / 2}));
            drawingContext.DrawLine(underlinePen, underlinePoint1, underlinePoint2);
            drawingContext.Pop();
        }
    }
}
