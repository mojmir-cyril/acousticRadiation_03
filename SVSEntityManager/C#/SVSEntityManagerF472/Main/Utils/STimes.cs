#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 


namespace SVSEntityManagerF472
{
    /// <summary>
    /// STimes object which contains time points from an Analysis
    /// which is assgined by SSolution object
    /// </summary>
    /// <exmple>
    /// <code>
    /// em = EM() 
    /// s = em.solution.Assign(1)
    /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
    /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
    /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
    /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
    /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
    /// </code>
    /// </exmple>
    public class STimes : IEnumerable<double>
    {
        /// <summary>
        /// gets time values
        /// </summary>
        public double[]     values          { get; private set; }
        private double      time1           { get; set;}
        private int         index1          { get; set;}
        private int         index2          { get; set;}
        private double      time2           { get; set;}
        private double      ratio           { get; set; }  
        public double       first           { get => values.FirstOrDefault(); }
        public double       min             { get => values.Min(); }
        public double       max             { get => values.Max(); }
        public double       last            { get => values.Last(); }
        public int          count           { get => values.Count(); }
        public double       this[int index] { get => values[index]; }
        public STimes(IEnumerable<double> times)
        {
            values = times.ToArray();
        }
        public void Dim(int count)
        {
            values = new double[count];
        }
        public bool Contains(double time) => values.Contains(time);
        public int  IndexOf(double time)  => values.ToList().IndexOf(time); 
        public void Interpolate(double time)
        {
            if (time < min) time = min;
            if (time > max) time = max;
            if (values.Contains(time))
            {
                time1  = time;
                time2  = time;
                index1 = IndexOf(time);
                index2 = index1;
                ratio  = 0.0;
                return;
            }
            time1  = values.OrderBy((x) => x < time ? time - x : 1e123).ToList()[0];
            index1 = IndexOf(time1);
            index2 = index1 + 1;
            time2  = values[index2]; 
            ratio  = (time - time1) / (time2 - time1);
            if (ratio > 1.0 || ratio < 0.0) throw new Exception($"ratio > 1.0 || ratio < 0.0 !!! -> ratio = '{ratio}'");
        }
        public int GetNearestIndex(double time) => IndexOf(values.OrderBy((x) => Math.Abs(x - time)).ToList()[0]); 
        public override string ToString() => $"STimes(count = {count}, min = {min}, max = {max})"; 
        public IEnumerator<double> GetEnumerator() => (IEnumerator<double>)values.GetEnumerator(); 
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }  
}
