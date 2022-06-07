#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  
using SVSExceptionBase;  

namespace SVSEntityManagerF472
{
    /// <summary>
    /// SStats object which provides statistical functions for a property defined by function "func" 
    /// </summary>
    /// <example><code>
    /// v = em.bodies.Stats(lambda e: e.volume)
    /// print "v.min   : " + str(v.min)     # minimal value
    /// print "v.max   : " + str(v.max)     # maximal value
    /// print "v.avg   : " + str(v.avg)     # average
    /// print "v.srss  : " + str(v.srss)    # square root of sum of squares 
    /// print "v.sum   : " + str(v.sum)     # sumation
    /// print "v.ssum  : " + str(v.ssum)    # sum of squares 
    /// print "v.count : " + str(v.count)   # count of items
    /// print "v.mean  : " + str(v.mean)    # mean = avg
    /// print "v.stdv  : " + str(v.stdv)    # standard deviation
    /// print "v.rms   : " + str(v.rms)     # root mean square  
    /// </code></example>
    public class SStats<TEnt> : SExceptionBase where TEnt : SEntity
    {
        /// <summary>
        /// gets list of attached entities for the statistics
        /// </summary>
        public ISEntities<TEnt>      ents    { get; }
        /// <summary>
        /// gets function which defines property
        /// </summary>
        /// <example><code>
        /// func = lambda e: e.volume 
        /// </code></example>
        public Func<TEnt, double>    func    { get; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets SStats object which provides statistical functions for a property defined by function "func" 
        /// </summary>
        /// <example><code>
        /// v = em.bodies.Stats(lambda e: e.volume)
        /// print "v.min   : " + str(v.min)     # minimal value
        /// print "v.max   : " + str(v.max)     # maximal value
        /// print "v.avg   : " + str(v.avg)     # average
        /// print "v.srss  : " + str(v.srss)    # square root of sum of squares 
        /// print "v.sum   : " + str(v.sum)     # sumation
        /// print "v.ssum  : " + str(v.ssum)    # sum of squares 
        /// print "v.count : " + str(v.count)   # count of items
        /// print "v.mean  : " + str(v.mean)    # mean = avg
        /// print "v.stdv  : " + str(v.stdv)    # standard deviation
        /// print "v.rms   : " + str(v.rms)     # root mean square  
        /// </code></example>
        public SStats(ISEntities<TEnt> ents, Func<TEnt, double> func)
        {
            this.ents = ents;
            this.func = func;
            Update();
        } 
        /// <summary>
        /// reuses function for update value list (e.g. after change of coordinate system, ...)
        /// </summary> 
        public SStats<TEnt> Update()
        {
            try
            {
                double F(TEnt e)
                {
                    try { return func(e); } catch (Exception err) { Throw($"TEnt(id = {e.id}): ", err, nameof(Update)); }
                    return double.NaN;
                }
                List<TEnt> objs = ents.entities.Where(x => F(x) != double.NaN).ToList();
                ids             = objs.Select(i => i.id).ToList();
                values          = objs.Select(func).ToList();
            }
            catch (Exception err) { Throw(err, nameof(Update)); }
            return this; 
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      values:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets list of ids of entities pointed in "ents"
        /// </summary> 
        public List<int>      ids       { get; private set; } 
        /// <summary>
        /// gets list of evaluated values by function "func"
        /// </summary> 
        public List<double>   values    { get; private set; }  
        // -------------------------------------------------------------------------------------------
        //
        //      stats:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>gets minimal value</summary> 
        public double         min       { get => values.Min();      }
        /// <summary>gets maximal value</summary> 
        public double         max       { get => values.Max();      }
        /// <summary>gets average</summary> 
        public double         avg       { get => values.Average();  }
        /// <summary>gets sumation</summary> 
        public double         sum       { get => values.Sum();      }
        /// <summary>gets sum of squares</summary>  
        public double         ssum      { get => values.Sum(x => Math.Pow(x, 2)); }
        /// <summary>gets square root of sum of squares</summary>  
        public double         srss      { get => Math.Sqrt(ssum); }
        /// <summary>gets count of items</summary> 
        public double         count     { get => values.Count(); }
        /// <summary>gets mean = avg</summary> 
        public double         mean      { get => avg; }
        /// <summary>gets standard deviation</summary> 
        public double         stdv      { get { double a = avg; return Math.Sqrt(values.Sum(x => Math.Pow(x - a, 2)) / count); } }
        /// <summary>gets root mean square</summary>   
        public double         rms       { get => Math.Sqrt(ssum / count); }
        /// <summary>gets most commonly occurring values</summary>   
        public double         mode      { get => values.OrderBy(x => values.Count(y => x == y)).LastOrDefault(); }  
        public double Quantile(double percentage) => throw ToDo("Quantile", nameof(Quantile)); 
        // -------------------------------------------------------------------------------------------
        //
        //      save all values to CSV file:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>saves data into text file (CSV format)</summary>   
        public void SaveAllData(string fullFileName, string head)
        {
            string text = string.Join("\n", ents.entities.Select(i => $"{i.id};{func(i)}"));
            File.WriteAllText(fullFileName, head + "\n" + text);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      ToString:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>gets string</summary>   
        public override string ToString() => $"SStats(count = {count}, min = {min}, max = {max})";
    }
}
