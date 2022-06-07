#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO;
using System.Collections;

//
//  Ansys:
//
// using Ansys.Mechanical.DataModel.Converters;
// using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
// using Ansys.Mechanical.DataModel.Enums;
// using Ansys.ACT.Interfaces.Mesh;
// using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
// using Ansys.ACT.Mechanical.Tools;
// using Ansys.ACT.Common.Graphics;
using Ansys.ACT.Interfaces.Mesh;



namespace SVSEntityManagerF472
{
    public class SMorphNode : ISMorphNode
    { 
        public SEntityManager  em       { get; }
        public INode           iNode    { get => em.mesh.GetNode(nodeId); }
        public int             nodeId   { get; }
        public int             bodyId   { get => iNode.BodyIds.First(); }
        public double          x        { get; set; }
        public double          y        { get; set; }
        public double          z        { get; set; } 
        public double[]        xyz      { get => new double[] { x, y, z }; }                                 
        public double[]        nxyz     { get { INode n = iNode; return new double[] { n.X, n.Y, n.Z }; } }  
        public double          dist     { get => GetDist(); } 
        public SMorphNode(SNode node)
        {
            em      = node.em;
            nodeId  = node.id;
            INode n = iNode;
            x       = n.X;
            y       = n.Y;
            z       = n.Z;
        }
        public SMorphNode(SEntityManager em, int nodeId, double x, double y, double z)
        {
            this.em      = em;
            this.nodeId  = nodeId; 
            this.x       = x;
            this.y       = y;
            this.z       = z;
        }
        private double GetDist()
        {
            INode n = iNode;
            return Math.Sqrt(Math.Pow(x - n.X, 2) + Math.Pow(y - n.Y, 2) + Math.Pow(z - n.Z, 2));
        }
        public double DistTo(SMorphNode m)
            => Math.Sqrt(Math.Pow(x - m.x, 2) + Math.Pow(y - m.y, 2) + Math.Pow(z - m.z, 2));
        public string GetCSVLine(char decimalSeparator = '.', char cellSeparator = ';')
        {
            string I(int i)    => i.ToString();
            string D(double v) => v.ToString("E").Replace(',' , decimalSeparator);
            string[] cells = new string[] { I(nodeId), D(iNode.X), D(iNode.Y), D(iNode.Z) };
            return string.Join(cellSeparator.ToString(), cells);
        }
    }
}
