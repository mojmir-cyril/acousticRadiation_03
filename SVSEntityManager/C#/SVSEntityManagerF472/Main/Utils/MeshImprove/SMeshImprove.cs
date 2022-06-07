#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using Ansys.ACT.Interfaces.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVSEntityManagerF472
{ 
    public class SMeshImprove
    {
        public Dictionary<int, SMeshImproveElem>    elems       { get; }
        public Dictionary<int, SMeshImproveNode>    nodes       { get; }
        public List<SMeshImproveNode>               probNodes   { get; }
        //
        //  
        //    em   = EntityManager.SEntityManager(ExtAPI)
        //    em.ClearGraphics()
        //    body = em.bodies[0]
        //    impr = EntityManager.SMeshImprove(ExtAPI,
        //           body.elems, body.faces.nodes,
        //           scale = 0.1, iters = 100,
        //           innerIters = 20, qualityLimit = 0.5,
        //           Log = ExtAPI.Log.WriteMessage)
        //    impr.DoMove(ExtAPI)
        //    body.nodes.morph.RedrawMesh()
        //
        //
        //
        //
        //    em    = EntityManager.SEntityManager(ExtAPI)
        //    em.ClearGraphics()
        //    elems = em.Elem(88385).elems
        //    impr  = EntityManager.SMeshImprove(ExtAPI,
        //            elems, elems.bodies.faces.nodes,
        //            scale = 0.1, iters = 100,
        //            innerIters = 20, qualityLimit = 0.5,
        //            Log = ExtAPI.Log.WriteMessage)
        //    impr.DoMove(ExtAPI)
        //    body.nodes.morph.RedrawMesh()
        //
        //
        public SMeshImprove(IMechanicalExtAPI   api, 
                            SElems              region, 
                            SNodes              fixedNodes, 
                            double              scale           = 0.1, 
                            int                 iters           = 100,   // over all nodes
                            int                 innerIters      = 20,    // iters for each node
                            double              qualityLimit    = 0.70,  // ignore elems over this quality
                            Action<string>      Log             = null,
                            bool                andDraw         = false)
        {
            if (Log is null) throw new ArgumentNullException(nameof(Log));
            //
            //  suspend:
            //
            using (api.Graphics.Suspend())
            {
                // api.Graphics.Scene.Clear();
                //
                //  do:
                //
                IList<int> fixIds = fixedNodes.ids;
                elems = region.ToDictionary(e => e.id, e => new SMeshImproveElem(e.iElem));
                nodes = region.nodes.ToDictionary(n => n.id, n => new SMeshImproveNode(fixIds.Contains(n.id), n.iNode)); 
                elems.Values.ToList().AsParallel().ForAll(e => e.AssignNodes(nodes));
                nodes.Values.ToList().AsParallel().ForAll(n => n.AssignElems(elems));
                elems.Values.ToList().AsParallel().ForAll(e => e.CreateLinks());
                //
                //  quality:
                //
                QualityCheck(Log, "00");
                //
                //  improve:
                //
                // for (int i = 0; i < iters; i++) nodes.Values.ToList().AsParallel().ForAll(n => n.Move(scale));
                // 
                Random R = new Random();
                for (int i = 0; i < iters; i++) nodes.Values.ToList().AsParallel().ForAll(n => n.MoveRandom(scale, R, innerIters, qualityLimit));   
                //
                //
                //
                elems.Values.ToList().AsParallel().ForAll(e => e.StraightMids());
                //
                //  draw:
                //
                if (andDraw) nodes.Values.ToList().ForEach(n => n.Draw(api)); 
                // 
                //  probs:
                // 
                probNodes = nodes.Values.Where(n => n.x == double.NaN ||  n.y == double.NaN || n.z == double.NaN).ToList();
                //
                //  log:
                //
                Log($" - count    : {nodes.Values.Count()}");
                Log($" - min dist : {nodes.Values.Where(n => !n.fix).Min(n => n.dist)}");
                Log($" - max dist : {nodes.Values.Max(n => n.dist)}");
                //
                //  quality:
                //
                QualityCheck(Log, "01");
            }
        }
        public void QualityCheck(Action<string> Log, string checkId = "00")
        {
                (double min, double avg, double max) = Quality(); 
                Log($" ");  
                Log($" ------------------------------------");
                Log($" ---      Mesh Quality Check      ---");
                Log($" ------------------------------------");
                Log($" - checkId     : {checkId}");
                Log($" - min quality : {min}");
                Log($" - avg quality : {avg}");
                Log($" - max quality : {max}");
                Log($" -------------------------------"); 
                Log($" "); 
        }
        public (double, double, double) Quality()
        {
            elems.Values.ToList().AsParallel().ForAll(e => e.EvalLinks());
            double[] qs  = elems.Values.Select(e => e.Quality()).ToArray(); 
            return (qs.Min(), qs.Average(), qs.Max());
        }
        public void DoMove(IMechanicalExtAPI api)
        {
            SMorphUtils.MoveNodesTo(api.DataModel.MeshDataByName("Global"), 
                                    nodes.Values.ToList().Select(n => new SSimplePoint(n.id, n.xyz)));
        }
    }
}
