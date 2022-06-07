#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Mechanical;
using System;
using System.Collections.Generic;

namespace SVSEntityManagerF472
{
    public class SMeshImproveLink
    { 
        public SMeshImproveElem     elem    { get; }
        public SMeshImproveNode     node1   { get; }
        public SMeshImproveNode     node2   { get; }
        public double               dx      { get; private set; }
        public double               dy      { get; private set; }
        public double               dz      { get; private set; }
        public double               len     { get; private set; }
        public SMeshImproveLink(SMeshImproveElem elem, SMeshImproveNode node1, SMeshImproveNode node2)
        {
            this.elem  = elem;
            this.node1 = node1;
            this.node2 = node2;
        }
        public void Eval()
        {
            dx  = node1.x - node2.x;  
            dy  = node1.y - node2.y;  
            dz  = node1.z - node2.z;  
            len = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));  
        }
        public void Draw(IMechanicalExtAPI api) 
        {
            List<IWorldPoint> points = new List<IWorldPoint>();
            IWorldPoint point1 = api.Graphics.CreateWorldPoint(node1.x, node1.y, node1.z);
            IWorldPoint point2 = api.Graphics.CreateWorldPoint(node2.x, node2.y, node2.z);
            points.Add(point1);
            points.Add(point2);
            IPolyline<IWorldPoint> wire = api.Graphics.Scene.Factory3D.CreatePolyline(points);
            wire.Color      = 0x0;
            wire.LineWeight = 1;
        }

    }
}
