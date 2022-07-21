using System;
using System.Collections.Generic;
using System.Text;

namespace WPF_Timeline.Timeline
{
    internal interface ITimeline
    {
        public TimeSpan IntervalTimeSpan { get; set; }
        public TimeSpan MinTimeSpan { get; set; }
        public TimeSpan MaxTimeSpan { get; set; }
        public double IntervalWidth { get; set; }
        public int IntervalsCount { get; }
        public double TotalWidth { get; } 
    }
}
