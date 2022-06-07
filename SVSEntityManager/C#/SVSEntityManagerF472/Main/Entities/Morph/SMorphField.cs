#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 


using System;
using System.Collections.Generic;
using System.Linq;

//
//  Ansys:
//
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics.Entities;


using SVSLoggerF472;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public class SMorphField : SExceptionBase
    { 
        public SEntityManager                   em              { get; }
        public IMechanicalExtAPI                api             { get => em.api; }
        public Action<string>                   Log             { get; }
        public Func<string, SStartStop>         StartStop       { get => em.logger.StartStop; }
        public Func<string, SStartStop>         StartStopLog    { get => em.logger.StartStopLog; }
        public IMeshData                        mesh            { get; }
        public Dictionary<int, SMorphDXYZ>      regionIds       { get; }
        public List<SMorphDXYZ>                 region          { get; }
        public List<SMorphDXYZ>                 fix             { get; private set; }  // constrained: dx = dy = dz = 0
        public List<SMorphDXYZ>                 move            { get; private set; }  // constrained
        public List<SMorphDXYZ>                 bcs             { get; private set; }  // constrained: fix + move
        public List<SMorphDXYZ>                 free            { get; private set; }  // free moving/morphing: region - bcs
        public SMorphDXYZ                       this[int id]    { get => region.Find(n => n.nodeId == id); }
        private List<IGraphicsEntity>           previewPoints   { get; set; }
        public SMorphField(SEntityManager em, IMeshData mesh,
                           IEnumerable<int> regionIds,     // all nodes in morphing region
                           IEnumerable<int> fixIds,        // fixed nodes
                           IEnumerable<int> moveIds,
                           Action<string>   Log = null)    // move nodes 
        {
            try
            {
                this.em   = em;
                this.mesh = mesh;
                this.Log  = Log;
                //
                //  log:
                //
                if (Log != null) Log($"SMorphField(...)");
                //
                //  bools:
                //
                Func<int, bool> IsMove = id => moveIds == null ? false : moveIds.Contains(id);
                Func<int, bool> IsFix  = id => fixIds  == null ? false : fixIds.Contains(id);
                //
                //  new:
                //
                Func<int, SMorphDXYZ> New = id
                    => new SMorphDXYZ(mesh.NodeById(id),
                                      isFix: IsFix(id),
                                      isMove: IsMove(id));
                //
                //  region:
                //
                this.regionIds = new Dictionary<int, SMorphDXYZ>();
                foreach (int id in regionIds) this.regionIds[id] = New(id);
                region = this.regionIds.Values.ToList();
                // this.regionIds = regionIds.ToList();
                // region         = regionIds.Select(id => New(id)).ToList();
                UpdateListsByBools();
                AssignConnectedDXYZs();
                //
                //  log:
                // 
                if (Log != null)
                { 
                    Log($" - region : {region.Count()}");
                    Log($" - fix    : {fix.Count()}");
                    Log($" - move   : {move.Count()}");
                    Log($" - bcs    : {bcs.Count()}");
                    Log($" - free   : {free.Count()}");
                } 
            }
            catch (Exception err) { Throw(err, nameof(SMorphField)); }  // catch (Exception err) { throw new Exception($"SMorphField(...): {err.Message}", err); }
        }
        public void UpdateListsByBools()
        {
            if (Log != null) Log($"UpdateListsByBools(...)");
            fix   = region.Where(d => d.isFix).ToList();
            move  = region.Where(d => d.isMove).ToList();
            bcs   = region.Where(d => d.isBC).ToList();
            free  = region.Where(d => d.isFree).ToList(); 
        }
        public void SetMove(double dx, double dy, double dz)
            => move.ForEach(m => m.Set(dx, dy, dz));

        public void SetMove(int nodeId, SNormal globalNormal, double dist)
            => this[nodeId]?.Set(globalNormal, dist);

        public void SetMoveFrom(SMorphField field)
            => move.ForEach(m => m.Set(field[m.nodeId].dxyz));

        public void AssignConnectedDXYZs()
        {
            DateTime now = DateTime.Now;
            if (Log != null)
            {
                Log($"AssignConnectedDXYZs(...)");
                Log($" - region.Count : {region.Count} ");
                Log($" - em.CPUS      : {em.CPUs}      ");
                Log($" - now          : {now}          ");
                Log($" - connect ...");
            }
            //
            //  connected nodes:
            //
            // region.ForEach(d => d.ConnectedNodes());
            // if (Log != null)
            // {
            //     Log($" ************************************************************ ");  
            //     Log($" - elapsed 1    : {DateTime.Now - now} "); 
            //     Log($" ************************************************************ "); 
            //     Log($" - assigning ...");
            // }
            //
            //  assign:
            // 
            if (region.Count < 100) region.ForEach(d => d.AssignConnected(regionIds));
            else                    region.AsParallel().WithDegreeOfParallelism(em.CPUs).ForAll(d => d.AssignConnected(regionIds));
            if (Log != null)
            {
                Log($" ************************************************************ ");  
                Log($" - elapsed 2    : {DateTime.Now - now} "); 
                Log($" ************************************************************ "); 
                Log($" - checks ...");
            }
            //
            //  checks:
            //
            List<int> ccs  = region.Select(d => d.connectedNodeIds.Count()).ToList();
            int       err1 = ccs.Where(c => c <= 0).Count();
            int       err2 = region.Where(d => d.connectedDXYZs == null).Count();
            if (err1 >= 1) throw new Exception($"SMorphField.AssignConnectedDXYZs(...): err1 = {err1} >= 1, region.Count() = {region.Count()}");
            if (err2 >= 1) throw new Exception($"SMorphField.AssignConnectedDXYZs(...): err2 = {err2} >= 1, region.Count() = {region.Count()}"); 
            if (Log != null)
            {
                Log($" - connected    : {ccs.Min()}, {ccs.Average()}, {ccs.Max()}     ");  
            }
        }
        public void MoveNodes()     => SMorphUtils.MoveNodesTo(mesh, region);
        public void UndoMoveNodes() => SMorphUtils.MoveNodesTo(mesh, region, useNXYZ: false);
        public void NeighborAverage(int minIters = 3, int maxIters = 2500, double maxChangeLimit = 0.0001, int maxPoints = 0, Action<string> Log = null)
        {
            using (StartStop($"NeighborAverage"))
            { 
                if (Log != null) Log($"NeighborAverage(...):");
                Stats();
                //
                //  iters:
                // 
                int c = free.Count();
                if (c >= 1)
                { 
                    for (int i = 0; i < maxIters; i++)
                    {
                        DateTime now = DateTime.Now;
                        List<SMorphDXYZ> s = free.OrderBy(d => d.dmax).Reverse().ToList(); 
                        double maxChange = c < 100 ? s.Select(d => d.EvalAverage(maxPoints)).Max() :
                            s.AsParallel().WithDegreeOfParallelism(8).Select(d => d.EvalAverage(maxPoints)).Max();  // double maxChange = free.Max(d => d.lastChange);
                        if (Log != null) Log($" - iter : {i} : {maxChange} <= {maxChangeLimit} (ella: {DateTime.Now - now})");
                        if (i >= minIters && maxChange <= maxChangeLimit) break;
                    }
                }
            }
        }
        public void RBF(int maxPoints = 10, double radius = 0.010)
        {
            using (StartStop($"RBF"))
            {
                if (Log != null) Log($"RBF(...):");
                if (free.Count() >= 1) free.ForEach(d => d.EvalRBF(bcs, maxPoints, radius));
            }
        }
        public void RBF(List<SMorphDXYZ> sourcePoints, int maxPoints = 10, double radius = 0.010)
        {
            using (StartStop($"RBF"))
            {
                if (Log != null) Log($"RBF(...):");
                if (free.Count() >= 1) free.ForEach(d => d.EvalRBF(sourcePoints, maxPoints, radius));
            }
        }
        public void RBFSmoothnig(int maxIters = 5)
        {
            using (StartStop($"RBFSmoothnig"))
            {
                if (Log != null) Log($"RBFSmoothnig(...):");
                if (free.Count() >= 1) 
                { 
                    free.ForEach(d => d.EvalRBF(bcs));
                    for (int i = 0; i < maxIters; i++) free.ForEach(d => d.EvalAverage());
                }
            }
        }
        public void Stats(Action<string> Log = null)
        { 
            Log = Log ?? this.Log;
            if (Log != null) 
            {
                Log($"SMorphField(...)");
                Log($" - region : {region.Count()}");
                Log($" - fix    : {fix.Count()}");
                Log($" - move   : {move.Count()}");
                Log($" - bcs    : {bcs.Count()}");
                Log($" - free   : {free.Count()}");
                //
                Log($" - region --> count           : {region.Count()}");
                if (region.Count() >= 1)
                {
                    Log($" - region --> max dx, dy, dz  : {region.Max(f => f.dx)}, {region.Max(f => f.dy)}, {region.Max(f => f.dz)}");
                    Log($" - region --> min dx, dy, dz  : {region.Min(f => f.dx)}, {region.Min(f => f.dy)}, {region.Min(f => f.dz)}");
                    Log($" - region --> avg dx, dy, dz  : {region.Average(f => f.dx)}, {region.Average(f => f.dy)}, {region.Average(f => f.dz)}");
                }
                //
                Log($" - fix --> count              : {fix.Count()}");
                if (fix.Count() >= 1)
                {
                    Log($" - fix --> max dx, dy, dz     : {fix.Max(f => f.dx)}, {fix.Max(f => f.dy)}, {fix.Max(f => f.dz)}");
                    Log($" - fix --> min dx, dy, dz     : {fix.Min(f => f.dx)}, {fix.Min(f => f.dy)}, {fix.Min(f => f.dz)}");
                    Log($" - fix --> avg dx, dy, dz     : {fix.Average(f => f.dx)}, {fix.Average(f => f.dy)}, {fix.Average(f => f.dz)}");
                }
                //
                Log($" - move --> count             : {move.Count()}");
                if (move.Count() >= 1)
                {
                    Log($" - move --> max dx, dy, dz    : {move.Max(f => f.dx)}, {move.Max(f => f.dy)}, {move.Max(f => f.dz)}");
                    Log($" - move --> min dx, dy, dz    : {move.Min(f => f.dx)}, {move.Min(f => f.dy)}, {move.Min(f => f.dz)}");
                    Log($" - move --> avg dx, dy, dz    : {move.Average(f => f.dx)}, {move.Average(f => f.dy)}, {move.Average(f => f.dz)}");
                }
                //
                Log($" - bcs --> count              : {bcs.Count()}");
                if (bcs.Count() >= 1)
                {
                    Log($" - bcs --> max dx, dy, dz     : {bcs.Max(f => f.dx)}, {bcs.Max(f => f.dy)}, {bcs.Max(f => f.dz)}");
                    Log($" - bcs --> min dx, dy, dz     : {bcs.Min(f => f.dx)}, {bcs.Min(f => f.dy)}, {bcs.Min(f => f.dz)}");
                    Log($" - bcs --> avg dx, dy, dz     : {bcs.Average(f => f.dx)}, {bcs.Average(f => f.dy)}, {bcs.Average(f => f.dz)}");
                }
                //
                Log($" - free --> count             : {free.Count()}");
                if (free.Count() >= 1)
                {
                    Log($" - free --> max dx, dy, dz    : {free.Max(f => f.dx)}, {free.Max(f => f.dy)}, {free.Max(f => f.dz)}");
                    Log($" - free --> min dx, dy, dz    : {free.Min(f => f.dx)}, {free.Min(f => f.dy)}, {free.Min(f => f.dz)}");
                    Log($" - free --> avg dx, dy, dz    : {free.Average(f => f.dx)}, {free.Average(f => f.dy)}, {free.Average(f => f.dz)}");
                }
            }
        }
        public void Preview(int color = 0x0000FF)
        {
            if (previewPoints == null) previewPoints = new List<IGraphicsEntity>();
            previewPoints.AddRange(SDrawUtils.DrawMovingPoints(move, api, id => color));
        }
        public void Preview(IEnumerable<int> onlyNodeIds, int color = 0x0000FF, bool addDist = true, bool addNodeId = true)
        {
            if (previewPoints == null) previewPoints = new List<IGraphicsEntity>();
            previewPoints.AddRange(SDrawUtils.DrawMovingPoints(region.Where(p => onlyNodeIds.Contains(p.nodeId)), api, id => color, addDistText: addDist, addNodeId: addNodeId));
        } 
        internal void Bisection() => region.ForEach(f => f.Bisection());
    }
}
 
