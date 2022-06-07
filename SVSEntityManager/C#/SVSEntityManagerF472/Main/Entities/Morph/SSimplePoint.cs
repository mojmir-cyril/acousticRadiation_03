#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 



//
//  Ansys:
//

using System.Collections.Generic;
using System.Linq;

namespace SVSEntityManagerF472
{
    public class SSimplePoint
    {
        public int        id     { get; set; }
        public double     x      { get; set; }
        public double     y      { get; set; }
        public double     z      { get; set; }
        public double[]   xyz    { get => new double[3] { x, y, z }; }
        public SSimplePoint(int nodeId, double x, double y, double z)
        {
            this.id = nodeId;
            this.x  = x;
            this.y  = y;
            this.z  = z;
        } 
        public SSimplePoint(int nodeId, IEnumerable<double> xyz)
        {
            this.id    = nodeId;
            double[] d = xyz.ToArray();
            this.x     = d[0];
            this.y     = d[1];
            this.z     = d[2];
        } 
    }
}
