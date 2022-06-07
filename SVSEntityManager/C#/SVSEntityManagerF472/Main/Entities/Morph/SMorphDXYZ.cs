#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


using SVSExceptionBase;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mesh;

namespace SVSEntityManagerF472
{

    public class SMorphDXYZ : SExceptionBase, ISMorphNode
    {
        public INode                iNode             { get; }
        public int                  nodeId            { get => iNode.Id; }
        public bool                 isFix             { get; }
        public bool                 isMove            { get; }
        public bool                 isBC              { get => isFix || isMove;   }
        public bool                 isFree            { get => !isBC;   }
        public double               x                 { get => iNode.X; }
        public double               y                 { get => iNode.Y; }
        public double               z                 { get => iNode.Z; }
        public double               dx                { get; set; }
        public double               dy                { get; set; }
        public double               dz                { get; set; }
        public double               lastChange        { get; set; }
        public double[]             xyz               { get => new double[3] { x, y, z }; }
        public double[]             dxyz              { get => new double[3] { dx, dy, dz }; }
        public double               dmax              { get => dxyz.Max(); }
        public double               dist              { get => SRSS(dxyz); }
        public double[]             nxyz              { get => new double[3] { x + dx, y + dy, z + dz }; }
        private  List<SMorphDXYZ>   connetedUseds     { get; set;}
        public List<int>            connectedNodeIds  { get; set; }  
        public List<SMorphDXYZ>     connectedDXYZs    { get; private set; }
        public SMorphDXYZ(INode node, bool isFix, bool isMove)
        {
            this.iNode = node;
            this.isFix = isFix;
            this.isMove = isMove;
            lastChange = 0;
        }
        public void Set(SNormal globalNormal, double dist) => Set(new SNormal(globalNormal).Norm(dist).xyz);
        public void Set(double[] dxyz) => Set(dxyz[0], dxyz[1], dxyz[2]);
        public void Set(double dx, double dy, double dz)
        {
            this.dx = dx;
            this.dy = dy;
            this.dz = dz;
        } 
        public void AssignConnected(Dictionary<int, SMorphDXYZ> regionIds)  // ids ... zduvodu rychlosti
        {
            lock (this)
            {
                //
                //  assign SMorphDXYZ:
                // 
                connectedNodeIds = iNode.ConnectedElements
                                        .SelectMany(e => e.CornerNodeIds)
                                        .Distinct()
                                        .Where(id => regionIds.ContainsKey(id))
                                        .ToList(); 
                connectedDXYZs    = connectedNodeIds.Select(id => regionIds[id])
                                                    .ToList();  
                //
                //
                //
                // connectedNodeIds  = connectedNodeIds.Intersect(regionIds.Keys)
                //                                     .ToList();
                // //
                // //  connectid node ids:
                // //
                // // List<int> regionIds = region.Select(n => n.nodeId)
                // //                          .ToList(); 
                // connectedNodes   = iNode.ConnectedElements
                //                         .SelectMany(e => e.CornerNodes)
                //                         .Distinct(new SEqualityComparerINode())  
                //                         .ToList();
                // List<int> cnIds  = connectedNodes.Select(n => n.Id)
                //                                  .Intersect(regionIds)
                //                                  .ToList();
                // connectedDXYZs   = cnIds.Select(id => region[regionIds.IndexOf(id)])
                //                         .ToList(); 
                // // connectedDXYZs   = region.FindAll(d => cnIds.Contains(d.nodeId))
                // //                          .ToList();  

                // //
                // //  connectid node ids:
                // //
                // connectedNodes   = iNode.ConnectedElements.SelectMany(e => e.CornerNodes).Distinct(new SEqualityComparerINode()).ToList();
                // List<int> allIds = region.Select(n => n.nodeId).ToList();
                // List<int> cnIds  = connectedNodes.Select(n => n.Id).Where(id => allIds.Contains(id)).ToList();
                // //
                // //  check:
                // // 
                // if (cnIds.Count() <= 0) throw new Exception($"AssignConnectedDXYZs(...): Count 0 error: cnIds.Count() <= 0. ");
                // // 
                // //  connected DXYZs:
                // //  
                // connectedDXYZs = region.FindAll(d => cnIds.Contains(d.nodeId)).ToList(); // connectedDXYZs = cnIds.Where(d => allIds.Contains(d.nodeId)).ToList(); 
                //
                //  check:
                // 
                NullAndCount(connectedDXYZs, nameof(connectedDXYZs), nameof(AssignConnected)); // if (connectedDXYZs.Count() <= 0) throw new Exception($"AssignConnectedDXYZs(...): Count 0 error: connectedDXYZs.Count() <= 0. ");
            }
        }
        public double EvalAverage(int maxPoints = 0)
        {
            lock (this)
            { 
                if (isBC) return 0;
                //
                //  checks:
                //
                NullAndCount(connectedDXYZs, nameof(connectedDXYZs), nameof(EvalAverage), nameof(SMorphUtils)); // if (connectedDXYZs == null)      throw new Exception($"EvalAverage(...): Null error: connectedDXYZs == null. "); if (connectedDXYZs.Count() <= 0) throw new Exception($"EvalAverage(...): Count 0 error: connectedDXYZs.Count() <= 0. ");
                //
                //  eval:
                // 
                if (connetedUseds == null) connetedUseds = maxPoints == 0 ? connectedDXYZs : connectedDXYZs.OrderBy(d => DistTo(d)).Take(maxPoints).ToList();
                int    c = connetedUseds.Count();
                double x = connetedUseds.Sum(d => d.dx) / c;
                double y = connetedUseds.Sum(d => d.dy) / c;
                double z = connetedUseds.Sum(d => d.dz) / c;
                lastChange = Math.Max(Math.Max(Math.Abs(dx - x), Math.Abs(dy - y)), Math.Abs(dz - z));
                dx = x;
                dy = y;
                dz = z;
                return lastChange;
            }
        }
        public void EvalRBF(List<SMorphDXYZ> sourcePoints, int maxPoints = 0, double radius = 0.010)
        {
            Dictionary<SMorphDXYZ, double> dists = new Dictionary<SMorphDXYZ, double>();
            foreach (SMorphDXYZ b in sourcePoints) dists.Add(b, DistTo(b)); //  Math.Sqrt(Math.Pow(b.x - x, 2) + Math.Pow(b.y - y, 2) + Math.Pow(b.z - z, 2))
            IEnumerable<SMorphDXYZ> bests = maxPoints <= 0 ? sourcePoints.OrderBy(b => dists[b]) :
                                                             sourcePoints.OrderBy(b => dists[b]).Take(maxPoints); 
            //
            //  bests:
            //
            if (bests.Count() >= 1)
            { 
                double tot = bests.Sum(b => TriangleWeight(dists[b], radius)); 
                dx = bests.Sum(b => b.dx * TriangleWeight(dists[b], radius)) / tot;
                dy = bests.Sum(b => b.dy * TriangleWeight(dists[b], radius)) / tot;
                dz = bests.Sum(b => b.dz * TriangleWeight(dists[b], radius)) / tot;
            }
            else { dx = 0.0; dy = 0.0; dz = 0.0; }
        }
        internal static double TriangleWeight(double dist, double radius = 0.010, double maxWeight = 1.0)
            => Math.Max(0.0, 1.0 - Math.Abs(dist / radius)) * maxWeight;
        private double SRSS(double[] vec) => Math.Sqrt(vec.Sum(v => Math.Pow(v, 2)));
        internal double DistTo(SMorphDXYZ d) => Math.Sqrt(Math.Pow(d.x - x, 2) + Math.Pow(d.y - y, 2) + Math.Pow(d.z - z, 2));
        internal void Bisection()
        {
            dx = dx / 2.0;
            dy = dy / 2.0;
            dz = dz / 2.0;
        }
    }
}
