#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using Ansys.ACT.Interfaces.Mesh;


using SVSExceptionBase;

//
//  Ansys:
//
using Ansys.ACT.Mechanical.Tools;

namespace SVSEntityManagerF472
{
    public static class SMorphUtils
    {
        // -------------------------------------------------------------------------------------------
        //
        //      single node:
        //
        // ------------------------------------------------------------------------------------------- 
        public static void MoveNode(IMeshData mesh, int bodyId, int id, IEnumerable<double> dXYZ)
        {
            INode    n = mesh.NodeById(id);
            double[] p = dXYZ.ToArray();
            Helpers.MoveNodeByBody(mesh, bodyId, id, new double[] { n.X + p[0], n.Y + p[1], n.Z + p[2] });
        }
        public static void MoveNodeTo(IMeshData mesh, int bodyId, int id, IEnumerable<double> xyz)
            => Helpers.MoveNodeByBody(mesh, bodyId, id, xyz);
        // -------------------------------------------------------------------------------------------
        //
        //      nodes:
        //
        // ------------------------------------------------------------------------------------------- 
        public static void MoveNodesTo(IMeshData mesh, int bodyId, IEnumerable<SSimplePoint> nodes)
        {
            IEnumerable<int>    ids  = nodes.Select(n => n.id);
            IEnumerable<double> xyzs = nodes.SelectMany(n => n.xyz);
            Helpers.MoveNodesByBody(mesh, bodyId, ids, xyzs);
        } 
        public static void MoveNodesTo(IMeshData mesh, int bodyId, IEnumerable<SMorphNode> nodes, bool useMXYZ)
        {
            IEnumerable<int>    ids  = nodes.Select(n => n.nodeId);
            IEnumerable<double> xyzs = nodes.SelectMany(n => useMXYZ ? n.nxyz : n.xyz);
            Helpers.MoveNodesByBody(mesh, bodyId, ids, xyzs);
        } 
        public static void MoveNodesTo(IMeshData mesh, IEnumerable<SMorphDXYZ> dxyzs, bool useNXYZ = true)
        {
            SExceptionBase.Null(mesh, nameof(mesh), nameof(MoveNodesTo), nameof(SMorphUtils)); // if (mesh == null) throw new Exception($"SMorphUtils.MoveNodes(...): Null error: mesh == null. ");
            Dictionary<int, List<SMorphDXYZ>> bodies = SplitToBodies(dxyzs);
            foreach (int bodyId in bodies.Keys)
                MoveNodesTo(mesh, bodyId, bodies[bodyId].Select(d => new SSimplePoint(d.nodeId, useNXYZ ? d.nxyz : d.xyz)));
        } 
        public static void MoveNodesTo(IMeshData mesh, IEnumerable<SSimplePoint> nodes)
        {
            Dictionary<int, List<SSimplePoint>> bodies = SplitToBodies(mesh, nodes);
            foreach (int bodyId in bodies.Keys)
                MoveNodesTo(mesh, bodyId, bodies[bodyId].Select(p => new SSimplePoint(p.id, p.xyz)));
        } 
        public static void MoveNodesTo(IMeshData mesh, IEnumerable<SMorphNode> nodes, bool useMXYZ)
        {
            Dictionary<int, List<SMorphNode>> bodies = SplitToBodies(nodes);
            foreach (int bodyId in bodies.Keys)
                MoveNodesTo(mesh, bodyId, bodies[bodyId].Select(n => new SSimplePoint(n.nodeId, useMXYZ ? n.nxyz : n.xyz)));
        } 
        public static Dictionary<int, List<SMorphDXYZ>> SplitToBodies(IEnumerable<SMorphDXYZ> dxyzs)
        { 
            Dictionary<int, List<SMorphDXYZ>> bodies = new Dictionary<int, List<SMorphDXYZ>>();
            foreach (SMorphDXYZ d in dxyzs)
            {
                int bodyId = d.iNode.BodyIds.First();
                if (!bodies.ContainsKey(bodyId)) bodies[bodyId] = new List<SMorphDXYZ>();
                else                             bodies[bodyId].Add(d);
            }
            return bodies;
        }
        public static Dictionary<int, List<SSimplePoint>> SplitToBodies(IMeshData mesh, IEnumerable<SSimplePoint> nodes)
        { 
            Dictionary<int, List<SSimplePoint>> bodies = new Dictionary<int, List<SSimplePoint>>();
            foreach (SSimplePoint p in nodes)
            {
                int bodyId = mesh.NodeById(p.id).BodyIds.First();
                if (!bodies.ContainsKey(bodyId)) bodies[bodyId] = new List<SSimplePoint>();
                else                             bodies[bodyId].Add(p);
            }
            return bodies;
        } 
        public static Dictionary<int, List<SMorphNode>> SplitToBodies(IEnumerable<SMorphNode> nodes)
        { 
            Dictionary<int, List<SMorphNode>> bodies = new Dictionary<int, List<SMorphNode>>();
            foreach (SMorphNode n in nodes)
            {
                int bodyId = n.bodyId;
                if (!bodies.ContainsKey(bodyId)) bodies[bodyId] = new List<SMorphNode>();
                else                             bodies[bodyId].Add(n);
            }
            return bodies;
        } 
        // public static double SmoothingRBF(SNode node, SNodes sourcePoints, 
        //                                   List<double> sourceValues,
        //                                   SMorphSmoothingFunction function = SMorphSmoothingFunction.Triangular,
        //                                   int maxPoints = 20, double radius = 0.010)
        // {  
        //     List<N> points = new List<N>();
        //     foreach ((int i, SNode b) in sourcePoints.Enumerate())
        //     {
        //         double dist = node.DistTo(b); 
        //         points.Add(new N(b, dist, sourceValues[i])); 
        //     }
        //     List<N> bests  = points.OrderBy(p => p.dist).ToList();
        //     List<N> bests2 = maxPoints <= 0 ? bests : bests.Take(maxPoints).ToList();
        //     //
        //     //  weight:
        //     //
        //     double W(N n) => function == SMorphSmoothingFunction.Constant ? 1.0 : n.Weight(radius);
        //     //  
        //     //  return:
        //     //
        //     if (bests2.Count() == 1) return bests2.First().val;
        //     if (bests2.Count() >= 2) return bests2.Sum(b => b.val * W(b)) / bests2.Sum(b => W(b));
        //     throw new Exception("Neco se stalo spatne!!!"); 
        // }        
        public static double SmoothingRBF(SNode node, SNodes sourcePoints, 
                                          List<double>            sourceValues,
                                          SNodes                  outerPointsWithZeroValues = null,
                                          double                  outerPointsZeroValue      = 0.0,
                                          SMorphSmoothingFunction function                  = SMorphSmoothingFunction.Triangular,
                                          int                     maxPoints                 = 20, 
                                          double                  radius                    = 0.010)
        {  
            List<N> points = new List<N>();
            foreach ((int i, SNode b) in sourcePoints.Enumerate())
            {
                double dist = node.DistTo(b); 
                points.Add(new N(b, dist, sourceValues[i])); 
            } 
            if (outerPointsWithZeroValues != null)
            {
                foreach (SNode zero in outerPointsWithZeroValues)
                {
                    double dist = node.DistTo(zero); 
                    points.Add(new N(zero, dist, outerPointsZeroValue)); 
                } 
            }
            List<N> bests  = points.OrderBy(p => p.dist).ToList();
            List<N> bests2 = maxPoints <= 0 ? bests : bests.Take(maxPoints).ToList();
            //
            //  weight:
            //
            double W(N n) => function == SMorphSmoothingFunction.Constant ? 1.0 : n.Weight(radius);
            //  
            //  return:
            //
            if (bests2.Count() == 1) return bests2.First().val;
            if (bests2.Count() >= 2) return bests2.Sum(b => b.val * W(b)) / bests2.Sum(b => W(b));
            throw new Exception("SmoothingRBF(...): Neco se stalo spatne!!!"); 
        }
        private class N
        { 
            public SNode   n       { get; } 
            public double  dist    { get; }
            public double  val     { get; }
            public N(SNode n, double dist, double val)
            {
                this.n    = n;
                this.dist = dist;
                this.val  = val;
            }
            public double Weight(double radius) => SMorphDXYZ.TriangleWeight(dist, radius);  
        }
//        public static double SmoothingRBF2(SMorphDXYZ node, List<SMorphDXYZ> sourcePoints, 
//                                          List<double> sourceValues, 
//                                          int maxPoints = 20, double radius = 0.010)
//        {
//            if (sourcePoints.Count() != sourceValues.Count) throw new Exception($"SmoothingRBF(...): sourcePoints.Count() != sourceValues.Count, {sourcePoints.Count()} != {sourceValues.Count}");
//            //
//            //  dicts:
//            //
//            Dictionary<int, double> dists   = new Dictionary<int, double>();
//            Dictionary<int, double> vals    = new Dictionary<int, double>();
//            Dictionary<int, double> weights = new Dictionary<int, double>();
//            foreach ((int i, SMorphDXYZ b) in sourcePoints.Enumerate())
//            {
//                int id = b.nodeId;
//                double d = node.DistTo(b);
//                dists.Add(id, d);
//                vals.Add(id, sourceValues[i]);
//                weights.Add(id, SMorphDXYZ.TriangleWeight(d, radius));
//            }
//            //  
//            //  bests:
//            //
//            List<SMorphDXYZ> bests = maxPoints <= 0 ? sourcePoints.OrderBy(b => dists[b.nodeId]).ToList() :
//                                                      sourcePoints.OrderBy(b => dists[b.nodeId]).Take(maxPoints).ToList();
//            //  
//            //  return:
//            //
//            if (bests.Count() == 1) return vals[bests.First().nodeId];
//            if (bests.Count() >= 2) return bests.Sum(b => vals[b.nodeId] * weights[b.nodeId]) / bests.Sum(b => weights[b.nodeId]);
//            throw new Exception("Neco se stalo spatne!!!");
//        }

        // public static List<SMorphDXYZ> MorphNodes(IMeshData mesh,
        //                                           // IEnumerable<int> regionIds,  // all nodes in morphing region
        //                                           // IEnumerable<int> fixIds,     // fixed nodes
        //                                           // IEnumerable<int> moveIds,    // 
        //                                           // double dx, double dy, double dz,
        //                                           SMorphField region,
        //                                           SMorphMethods method = SMorphMethods.RadialBaseFuncSmoothing,
        //                                           int maxIters = 2500, double maxChangeLimit = 0.0001,
        //                                           Action<object> Msg = null)
        // { 
        //     //
        //     //  checks:
        //     // 
        //     if (mesh      == null) throw new Exception($"SMorphUtils.MorphNodes(...): Null error: mesh == null. ");
        //     // if (fixIds    == null) throw new Exception($"SMorphUtils.MorphNodes(...): Null error: fixIds == null. ");
        //     // if (moveIds   == null) throw new Exception($"SMorphUtils.MorphNodes(...): Null error: moveIds == null. ");
        //     // if (regionIds == null) throw new Exception($"SMorphUtils.MorphNodes(...): Null error: nodeIds == null. ");
        //     //
        //     //
        //     //
        //     // List<int> nodeIds = mesh.MeshRegionById(bodyId).NodeIds.ToList();
        //     // List<SMorphDXYZ> dxyzs = regionIds.Select(id => new SMorphDXYZ(mesh.NodeById(id), isFix: fixIds.Contains(id) || moveIds.Contains(id))).ToList();
        //     //
        //     //  log:
        //     // 
        //     // if (Msg != null)
        //     // { 
        //     //     logger.Msg($"SMorphUtils.MorphNodes(...)");
        //     //     logger.Msg($" - regionIds : {string.Join(", ", regionIds)}");
        //     //     logger.Msg($" - fixIds    : {string.Join(", ", fixIds)}");
        //     //     logger.Msg($" - moveIds   : {string.Join(", ", moveIds)}"); 
        //     // }
        //     // CreateField
        //     //
        //     //  fixed & morphed:
        //     //
        //     // List<SMorphDXYZ> bcs = dxyzs.Where(d => d.isFix).ToList();
        //     // List<SMorphDXYZ> mvs = dxyzs.Where(d => !d.isFix).ToList();
        //     //
        //     //  set move & fix:
        //     //
        //     // bcs.Where(d => moveIds.Contains(d.id)).ToList().ForEach(d => d.Set(dx, dy, dz));
        //     //
        //     //  assign:
        //     //
        //     // foreach (SMorphDXYZ d in dxyzs) d.AssignConnectedDXYZs(dxyzs);
        //     //
        //     //  checks:
        //     //
        //     // int err1 = dxyzs.Where(d => d.connectedNodes.Count() <= 0).Count();
        //     // int err2 = dxyzs.Where(d => d.connectedDXYZs == null).Count();
        //     // if (err1 >= 1) throw new Exception($"SMorphUtils.MorphNodes(...): err1 = {err1} >= 1, dxyzs.Count() = {dxyzs.Count()}");
        //     // if (err2 >= 1) throw new Exception($"SMorphUtils.MorphNodes(...): err2 = {err2} >= 1, dxyzs.Count() = {dxyzs.Count()}");
        //     // if (method == SMorphMethods.NeighborAverage)
        //     // {
        //     //     //
        //     //     //  NeighborAverage:
        //     //     // 
        //     //     for (int i = 0; i < maxIters; i++)
        //     //     {
        //     //         mvs.ForEach(d => d.EvalAverage());
        //     //         double maxChange = mvs.Max(d => d.lastChange);
        //     //         logger.Msg($" - iter : {i} : {maxChange} <= 0.00001");
        //     //         if (maxChange <= maxChangeLimit) break;
        //     //     }
        //     // }
        //     // else if (method == SMorphMethods.RadialBaseFunc)
        //     // {
        //     //     //
        //     //     //  RadialBaseFunc:
        //     //     //
        //     //     mvs.ForEach(d => d.EvalRBF(bcs));
        //     // }
        //     // else if (method == SMorphMethods.RadialBaseFuncSmoothing)
        //     // {
        //     //     //
        //     //     //  RadialBaseFunc:
        //     //     //
        //     //     mvs.ForEach(d => d.EvalRBF(bcs));
        //     //     for (int i = 0; i < 5; i++) mvs.ForEach(d => d.EvalAverage());
        //     // }
        //     // //
        //     // //  move:
        //     // // 
        //     // return dxyzs.ToList();
        //     // dxyzs.ToList().ForEach(d => MoveNodes(mesh, d.iNode.BodyIds.First(), d.id, d.xyz));
        //     //
        //     //
        //     //
        //     // Helpers.RedrawPartMesh(partId);
        //     // ((IMechanicalExtAPI)api).DataModel.Project.Model.Mesh.InternalObject.NotifyMeshControlGroup();
        //     // ((IMechanicalExtAPI)api).DataModel.Project.Model.Mesh.Update();
        // }

    }
}
 
