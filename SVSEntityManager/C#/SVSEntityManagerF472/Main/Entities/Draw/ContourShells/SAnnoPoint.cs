#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq;

using Ansys.ACT.Interfaces.Mesh;


namespace SVSEntityManagerF472
{
    public static class SAnnoPoint
    { 
        // -------------------------------------------------------------------------------------------
        //
        //      XYZ:
        //
        // -------------------------------------------------------------------------------------------
        public class SXYZ // : SAnnoUtils
        { 
            public double                x              { get => xyz[0]; }
            public double                y              { get => xyz[1]; }
            public double                z              { get => xyz[2]; }
            public double[]              xyz            { get; set; }
            public SXYZ()                          => xyz = new double[] { double.NaN, double.NaN, double.NaN };
            public SXYZ(IEnumerable<double> xyz)   => this.xyz = xyz.ToArray();
            public static SXYZ operator +(SXYZ a, SXYZ b) => new SXYZ(SVectorUtils.Add(a.xyz, b.xyz)); 
            public static SXYZ operator -(SXYZ a, SXYZ b) => new SXYZ(SVectorUtils.Substract(a.xyz, b.xyz)); 
            public static SXYZ operator *(SXYZ a, double c)        => new SXYZ(SVectorUtils.Scale(a.xyz, c)); 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Base:
        //
        // -------------------------------------------------------------------------------------------
        public abstract class SBase : SXYZ
        {
            public List<SAnnoContourShells.STriangle>   triangles              { get; set; }
            public List<SElemFace>                      elemFaces              { get; set; }
            public double                               resultValue            { get; set; }
            public List<double>                         avgTriangleNormal      { get; set; }  
            public int indexInAllPoints { get; internal set; }

            public SBase(SAnnoContourShells.STriangle triangle, SElemFace elemFace)
            {
                //
                //  connections:
                //
                triangles = new List<SAnnoContourShells.STriangle> { triangle };
                elemFaces = new List<SElemFace> { elemFace };
                //
                //  defaults:
                //
                Default();
            }
            public SBase(List<SAnnoContourShells.STriangle> triangles, List<SElemFace> elemFaces)
            {
                //
                //  connections:
                //
                this.triangles = triangles;
                this.elemFaces = elemFaces;
                //
                //  defaults:
                //
                Default();
            }
            public virtual void Default()
            { 
                resultValue = double.NaN; 
            }
            public abstract bool HasBand(int bandIndex);
            public abstract string GetLabel();
            public void EvalAvgTriangleNormal()
            {
                List<List<double>> normals = triangles.Select(t => t.normal).ToList();
                int    c  = normals.Count();
                double ax = normals.Select(n => n[0]).Sum() / c;
                double ay = normals.Select(n => n[1]).Sum() / c;
                double az = normals.Select(n => n[2]).Sum() / c;
                avgTriangleNormal = SVectorUtils.Normalize(new List<double> { ax, ay, az });
            }
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Intersect:
        //
        // -------------------------------------------------------------------------------------------
        public class SIntersect : SBase
        {
            public SAnnoContourShells.SBorder    border            { get; set; }
            public SColors.SColor         colorBandHigh     { get; set; }
            public SColors.SColor         colorBandLow      { get; set; }
            public bool           isIntermediate    { get => colorBandHigh.index != colorBandLow.index; }
            public string         wrn               { get; } = "";

            public SIntersect(SAnnoContourShells.SBorder border, double resultValue, SXYZ pointXYZ) : base(border.triangles, border.elemFaces)
            {
                this.border         = border; 
                this.resultValue    = resultValue; 
                this.xyz            = pointXYZ.xyz;
                //
                //  colors:
                //
                try
                {
                    if (resultValue <= border.colorBands.borderValues.First())
                    {
                        colorBandLow  = border.colorBands.minBand;
                        colorBandHigh = border.colorBands.minBand;
                    }
                    if (resultValue >= border.colorBands.borderValues.Last())
                    {
                        colorBandLow  = border.colorBands.maxBand;
                        colorBandHigh = border.colorBands.maxBand; 
                    }
                    else
                    {
                        int bIndex     = border.colorBands.borderValues.IndexOf(resultValue);
                        colorBandLow   = border.colorBands.bands[bIndex - 1];
                        colorBandHigh  = border.colorBands.bands[bIndex]; 
                    } 
                }
                catch (Exception err) 
                { 
                    wrn = $"SAnnoPointIntersect(...): {err}";
                    colorBandLow  = border.colorBands.bands.First();
                    colorBandHigh = border.colorBands.bands.First();
                } 
            }
            public override bool HasBand(int bandIndex) => colorBandHigh.index == bandIndex || colorBandLow.index == bandIndex;
            public override string GetLabel() => $"X : {colorBandLow.index}-{colorBandHigh.index}";
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Node:
        //
        // -------------------------------------------------------------------------------------------
        public class SNode : SBase
        {
            public bool                  isCorner       { get; }
            public int                   id             { get => iNode.Id; }
            public INode                 iNode          { get; private set; }
            public SNodes                sNodes         { get => SNew.NodesFromIds(elemFaces[0].em, new int[] { iNode.Id }); }   // pouze z testovacich duvodu 

            public SColors               colorBands     { get => colorBand.colorBands; }
            public SColors.SColor        colorBand      { get; set; }
            public int                   color          { get => colorBand.color; }

            public SNode(INode iNode, SAnnoContourShells.STriangle triangle, SElemFace elemFace, bool isCorner) : base(triangle, elemFace)
            {
                this.isCorner = isCorner; 
                this.iNode    = iNode; 
                Default();
            } 
            public override void Default()
            {
                resultValue = double.NaN;  
                colorBand   = null;
                resultValue = double.NaN; 
            }
            public override bool HasBand(int bandIndex) => colorBand.index == bandIndex;
            public override string GetLabel() => $"N{id} : {colorBand.index}";
        }





    }

}