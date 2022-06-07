#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVSEntityManagerF472
{
    public class SMeshImproveNode
    { 
        public INode                    node         { get; }
        public int                      id           { get; }
        public bool                     fix          { get; }
        public double                   origX        { get; private set; }
        public double                   origY        { get; private set; }
        public double                   origZ        { get; private set; }
        public double                   x            { get; private set; }
        public double                   y            { get; private set; }
        public double                   z            { get; private set; } 
        public bool                     midDone      { get; set; } = false;
        public List<SMeshImproveElem>   elems        { get; private set; }
        public int                      elemsCount   { get; private set; }
        public double                   dist         { get => Math.Sqrt(Math.Pow(origX - x, 2) + Math.Pow(origY - y, 2) + Math.Pow(origZ - z, 2)); }
        public double[]                 xyz          { get => new double[3] { x, y, z }; }

        public SMeshImproveNode(bool fix, INode node)
        {
            this.fix  = fix;
            this.node = node;
            id        = node.Id;
            x         = node.X;
            y         = node.Y;
            z         = node.Z; 
            origX     = x;
            origY     = y;
            origZ     = z; 
        }
        public void AssignElems(Dictionary<int, SMeshImproveElem> d)
        {
            elems      = node.ConnectedElementIds.Select(id => d[id]).ToList();
            elemsCount = elems.Count();
        } 
        internal void MoveImbalance(double scale)
        {
            lock (this)
            { 
                if (fix) return;
                double Fx  = 0;
                double Fy  = 0;
                double Fz  = 0;
                double len = 0;
                foreach (SMeshImproveElem e in elems)
                {
                    e.links.ForEach(l => l.Eval());
                    double avg = e.links.Average(l => l.len);
                    List<SMeshImproveInbalance> bs = e.links.Where(l => l.node1 == this || l.node2 == this)
                                                            .Select(l => new SMeshImproveInbalance(this, l, avg))
                                                            .ToList();
                    Fx  += bs.Sum(b => b.Fx);
                    Fy  += bs.Sum(b => b.Fy);
                    Fz  += bs.Sum(b => b.Fz); 
                    len += bs.Sum(b => b.len) / bs.Count(); 
                }
                len = len / elemsCount;
                double F = Math.Sqrt(Math.Pow(Fx, 2) + Math.Pow(Fy, 2) + Math.Pow(Fz, 2));
                double dx = Fx / F * len * scale;
                double dy = Fy / F * len * scale;
                double dz = Fz / F * len * scale;
                x += dx; 
                y += dy; 
                z += dz;  
            }
        } 
        internal void Set(double x, double y, double z)
        {
            lock (this)
            {
                this.x = x; 
                this.y = y; 
                this.z = z;  
            }
        } 
        internal void MoveRandom(double scale, Random R, int innerIters, double qualityLimit)
        {
            lock (this)
            {
                if (fix) return;
                elems.ForEach(e => e.EvalLinks());
                double iniQ = elems.Min(e => e.Quality());
                double avgL = elems.Average(e => e.links.Average(l => l.len));
                if (iniQ >= qualityLimit) return;
                //
                //  iters:
                //
                double minQ, best = iniQ, bx = x, by = y, bz = z;
                for (int i = 0; i < innerIters; i++)
                {
                    double dx = (0.5 - R.NextDouble()) * avgL * scale;
                    double dy = (0.5 - R.NextDouble()) * avgL * scale;
                    double dz = (0.5 - R.NextDouble()) * avgL * scale;
                    x = origX + dx;
                    y = origY + dy;
                    z = origZ + dz;
                    elems.ForEach(e => e.EvalLinks());
                    minQ = elems.Min(e => e.Quality());
                    if (minQ > best)
                    {
                        bx   = x;
                        by   = y;
                        bz   = z;
                        best = minQ;
                    }
                }
                x = bx;
                y = by;
                z = bz; 
            }
        }
        public void Draw(IMechanicalExtAPI api)
        { 
            List<IWorldPoint> points = new List<IWorldPoint>();
            IWorldPoint point1 = api.Graphics.CreateWorldPoint(x, y, z);
            IWorldPoint point2 = api.Graphics.CreateWorldPoint(origX, origY, origZ);
            points.Add(point1);
            points.Add(point2);
            IPolyline<IWorldPoint> wire = api.Graphics.Scene.Factory3D.CreatePolyline(points);
            wire.Color      = midDone ? 0xFF0000 : 0x0;
            wire.LineWeight = 1;
        }
    }
}
