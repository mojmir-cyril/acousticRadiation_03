#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  



namespace SVSEntityManagerF472
{
    public class SDisps : IEnumerable<double>
    {
        /// <summary>
        /// gets name (label) of the displacement curve
        /// </summary>
        public string       label           { get; }
        /// <summary>
        /// gets STime object with time values
        /// </summary>
        public STimes       times           { get; }
        /// <summary>
        /// gets displacement values of the curve
        /// </summary>
        public double[]     values          { get; private set; }
        private double      time1           { get; set; }
        private int         index1          { get; set; }
        private int         index2          { get; set; }
        private double      time2           { get; set; }
        private double      ratio           { get; set; }   
        public double       first           { get => values.FirstOrDefault(); }
        public double       min             { get => values.Min(); }
        public double       max             { get => values.Max(); }
        public double       last            { get => values.LastOrDefault(); }
        public int          count           { get => values.Count(); }
        public double       this[int index] { get => values[index]; set => values[index] = value; }
        public SDisps(STimes times, string label)
        {
            this.label = label;
            this.times = times;
            values     = new double[times.count];
        } 
        public double InTime(double time)
        {
            if (time < min) time = double.NaN;
            if (time > max) time = double.NaN;
            if (times.Contains(time))
            {
                time1  = time;
                time2  = time;
                index1 = times.IndexOf(time);
                index2 = index1;
                ratio  = 0.0;
                return values[index1];
            }
            time1  = times.values.OrderBy((x) => x < time ? time - x : 1e123).FirstOrDefault();
            index1 = times.IndexOf(time1);
            index2 = index1 + 1;
            time2  = times.values[index2]; 
            ratio  = (time - time1) / (time2 - time1);
            if (ratio > 1.0 || ratio < 0.0) throw new Exception($"ratio > 1.0 || ratio < 0.0 !!! -> ratio = '{ratio}'");
            return (values[index2] - values[index1]) * ratio + values[index1];
        }
        public SDisps Dim(int count)
        {
            values = new double[count];
            return this;
        }
        public override string ToString() => $"SDisps(label = {label}, count = {count}, min = {min}, max = {max})";
        public IEnumerator<double> GetEnumerator() => (IEnumerator<double>)values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }  
}
