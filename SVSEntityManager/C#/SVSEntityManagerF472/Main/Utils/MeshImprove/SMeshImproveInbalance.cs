#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;

namespace SVSEntityManagerF472
{
    public class SMeshImproveInbalance
    {
        public double   len   { get; }
        public double   Fx    { get; }
        public double   Fy    { get; }
        public double   Fz    { get; }
        public SMeshImproveInbalance(SMeshImproveNode node, SMeshImproveLink link, double avg)
        {
            if (node != link.node1 && node != link.node2) throw new Exception("Neco je spatne !!!");
            double f = node == link.node2 ? 1.0 : -1.0;
            double r = (link.len - avg) / avg;
            Fx  = r * link.dx / link.len * f;
            Fy  = r * link.dy / link.len * f;
            Fz  = r * link.dz / link.len * f;
            len = link.len;
        } 
    }
}
