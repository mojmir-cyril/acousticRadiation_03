#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVSEntityManagerF472
{ 
    public class SMeshImproveElem
    {
        public IElement                 elem        { get; }
        public List<SMeshImproveNode>   nodes       { get; private set; }
        public List<SMeshImproveNode>   corners     { get; private set; }
        public List<SMeshImproveNode>   mids        { get; private set; }
        public List<SMeshImproveLink>   links       { get; private set; }
        public SMeshImproveElem(IElement elem)
        { 
            this.elem = elem;
            if (elem.Type != ElementTypeEnum.kTet4 &&
                elem.Type != ElementTypeEnum.kTet10) 
                throw new Exception($"Only Tet4 and Tet10 shape are supported (elem.Type = {elem.Type}). ");
        } 
        public void AssignNodes(Dictionary<int, SMeshImproveNode> d)
        { 
            nodes   = elem.NodeIds.Select(id => d[id]).ToList();
            corners = nodes.Where(n => elem.CornerNodeIds.Contains(n.id)).ToList();
            mids    = nodes.Where(n => !elem.CornerNodeIds.Contains(n.id)).ToList();
        }
        public void CreateLinks()
        {
            links = new List<SMeshImproveLink>() 
            { 
                new SMeshImproveLink(this, corners[0], corners[1]),
                new SMeshImproveLink(this, corners[0], corners[2]),
                new SMeshImproveLink(this, corners[1], corners[2]),
                new SMeshImproveLink(this, corners[0], corners[3]),
                new SMeshImproveLink(this, corners[1], corners[3]),
                new SMeshImproveLink(this, corners[2], corners[3]), 
            }; 
        } 
        public double Quality()
        {
            //
            //  Element Quality (ANSYS Help 3.3.15.2)
            //
            double V = Volume();
            double C = 124.70765802;
            return C * V / Math.Sqrt(Math.Pow(links.Sum(l => l.len * l.len), 3)); 
        } 
        public void EvalLinks() => links.ForEach(l => l.Eval());
        public void Draw(IMechanicalExtAPI api) => links.ForEach(l => l.Draw(api));
        public double Volume()
        {
            SMeshImproveLink A = links[0];
            SMeshImproveLink B = links[1];
            SMeshImproveLink D = links[3];
            double V = 1.0 / 6.0 * ((A.dy * B.dz - A.dz * B.dy) * D.dx + (A.dz * B.dx - A.dx * B.dz) * D.dy + (A.dx * B.dy - A.dy * B.dx) * D.dz);
            return Math.Abs(V);
        }
        public void StraightMids()
        {
            void Mid(SMeshImproveNode m1, SMeshImproveNode c1, SMeshImproveNode c2)
            {
                if (m1.fix || m1.midDone) return; 
                m1.Set((c1.x + c2.x) / 2.0, (c1.y + c2.y) / 2.0, (c1.z + c2.z) / 2.0);
                m1.midDone = true;
            }
            if (elem.Type == ElementTypeEnum.kTet10)
            {
                Mid(mids[0], corners[0], corners[1]);
                Mid(mids[1], corners[1], corners[2]);
                Mid(mids[2], corners[0], corners[2]);
                Mid(mids[3], corners[0], corners[3]);
                Mid(mids[4], corners[1], corners[3]);
                Mid(mids[5], corners[2], corners[3]);
            }
        }
    }
}
