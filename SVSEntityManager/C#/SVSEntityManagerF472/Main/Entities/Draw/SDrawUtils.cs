#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 


using System;
using System.Collections.Generic;
using System.Linq; 

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical; 
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities; 


namespace SVSEntityManagerF472
{
    public static class SDrawUtils
    {
        public static List<IGraphicsEntity> DrawNormals(SElemFaces elemFaces, IMechanicalExtAPI api, 
                                                        double scale = 1.0, int color = 0x0000FF, 
                                                        int lineWeight = 2, bool alls = true, 
                                                        bool addText = false)
        {
            IFactory2D f2d = api.Graphics.Scene.Factory2D;
            IFactory3D f3d = api.Graphics.Scene.Factory3D;
            List<IGraphicsEntity> pls = new List<IGraphicsEntity>();
            using (api.Graphics.Suspend())
            {
                foreach (SElemFace ef in elemFaces)
                {
                    List<SNormal> ns = alls ? ef.globalNormals.Select(n => n * scale).ToList() : new List<SNormal>() { ef.globalNormal * scale };
                    foreach ((int i, SNormal n) in ns.Enumerate())
                    { 
                        List<double>    p1 = alls ? new List<double>() { ef.iFaceNodes[i].X, ef.iFaceNodes[i].Y, ef.iFaceNodes[i].Z } : ef.globalXYZ; 
                        List<double>    p2 = new List<double>() { p1[0] + n.x, p1[1] + n.y, p1[2] + n.z }; // .Select(p => p * scale).ToList();
                        IGraphicsEntity pl = f3d.CreatePolyline(new IWorldPoint[] { WP(api, p1), WP(api, p2) });
                        //
                        //  props:
                        //
                        pl.Color      = color;
                        pl.LineWeight = lineWeight;
                        //
                        //  add:
                        //
                        pls.Add(pl); 
                        //
                        //  ids:
                        //
                        if (addText)
                        {
                            string  t  = alls ? ToStr(i) : ToStr(n);
                            IText2D tx = f2d.CreateText(WP(api, p1), $"... {t}");
                            tx.Color = 0x000000;
                            pls.Add(tx);
                        }
                    }
                }
            }
            return pls;
        }
        public static List<IGraphicsEntity> DrawEntIds(SEntitiesBase ents, IMechanicalExtAPI api, int color = 0x0000FF, bool addIndex = true)
        {
            IFactory2D f2d = api.Graphics.Scene.Factory2D;
            List<IGraphicsEntity> txs = new List<IGraphicsEntity>();
            using (api.Graphics.Suspend())
            {
                foreach ((int i, SEntity ent) in ents.__GetSEntityList().Enumerate())
                {
                    List<double> p1   = ent.globalXYZ;
                    string       t    = ToStr(ent);
                    string       text = addIndex ? $"... {i} : {t}" : $"... {t}";
                    IText2D      tx   = f2d.CreateText(WP(api, p1), text);
                    tx.Color          = color;
                    txs.Add(tx); 
                }
            }
            return txs;
        }       
        public static List<IGraphicsEntity> DrawNodesResultProbes(SNodes                  nodes, 
                                                                  int[]                   onlyIds, 
                                                                  IMechanicalExtAPI       api, 
                                                                  string                  withMorph, 
                                                                  Dictionary<int, double> nodalValues   = null, 
                                                                  string                  unit          = "N/A",
                                                                  int                     color         = 0x0000FF, 
                                                                  Func<double, string>    ValueToString = null, 
                                                                  bool                    addUnit       = true,
                                                                  bool                    addIndex      = true) 
        {
            IFactory2D            f2d   = api.Graphics.Scene.Factory2D;
            List<IGraphicsEntity> txs   = new List<IGraphicsEntity>();
            SMorphData            mData = nodes.morph[withMorph];
            using (api.Graphics.Suspend())
            {
                foreach (SNode node in nodes)
                {  
                    int          id   = node.id;
                    if (!onlyIds.Contains(id)) continue; 
                    SMorphNode   nn   = mData[id];
                    double       v    = nodalValues   == null ? node.resultValue : nodalValues[node.id];
                    string       t    = ValueToString == null ? v.ToString()     : ValueToString(v); 
                    string       u    = addUnit  ? $"[{(unit == "N/A" ? node.result.unit : unit)}]" : "";
                    string       text = addIndex ? $"... {t} {u} ({id})" : $"... {t} {u}";      // string text = addIndex ? $"... {id} : {t} {u}" : $"... {t} {u}"; 
                    SSimplePoint p2   = new SSimplePoint(id, nn.x, nn.y, nn.z);  
                    IText2D      tx   = f2d.CreateText(WP(api, p2), text);
                    tx.Color          = color;
                    txs.Add(tx); 
                }
            }
            return txs; 
        }
        public static List<IGraphicsEntity> Points(IEnumerable<SSimplePoint> points, IMechanicalExtAPI api, int color = 0x0000FF, int size = 5)
        {
            IFactory3D f3d = api.Graphics.Scene.Factory3D; 
            List<IGraphicsEntity> grs = new List<IGraphicsEntity>();
            using (api.Graphics.Suspend())
            {
                foreach (SSimplePoint p in points)
                {
                    List<double> p1 = p.xyz.ToList(); 
                    IWorldPoint wp1 = WP(api, p1); 
                    IPoint3D pt1    = f3d.CreatePoint(wp1, size);  
                    pt1.Color       = color; 
                    grs.Add(pt1);  
                }
            }
            return grs;
        }
        public static List<IGraphicsEntity> Nodes(SNodes nodes, IMechanicalExtAPI api, int color = 0x0000FF, int size = 5, string withMorph = "N/A")
        { 
            if (withMorph != "N/A")
            {
                List<SSimplePoint> pts = new List<SSimplePoint>();
                foreach (SNode n in nodes)
                { 
                    SMorphNode nn = nodes.morph[withMorph][n.id];
                    pts.Add(new SSimplePoint(n.id, nn.x, nn.y, nn.z));
                }
                return Points(pts, api, color, size);
            }
            else return Points(nodes.Select(n => new SSimplePoint(n.id, n.iNode.X, n.iNode.Y, n.iNode.Z)), api, color, size);
        } 
        public static List<IGraphicsEntity> DrawMovingPoints(IEnumerable<ISMorphNode> points, IMechanicalExtAPI api, 
                                                             Func<int, int> ColorById = null, 
                                                             Func<int, double[]> Point1XYZs = null, // use node xyz if null 
                                                             int pointSize1 = 0, 
                                                             int pointSize2 = 5, 
                                                             int lineWeight = 1,
                                                             bool addDistText = true, bool addNodeId = true)
        {
            IFactory2D f2d = api.Graphics.Scene.Factory2D;
            IFactory3D f3d = api.Graphics.Scene.Factory3D;
            List<IGraphicsEntity> grs = new List<IGraphicsEntity>();
            using (api.Graphics.Suspend())
            {
                foreach (ISMorphNode p in points)
                {
                    int color = ColorById == null ? 0x0000FF : ColorById(p.nodeId);
                    List<double> p1 = Point1XYZs == null ? p.nxyz.ToList() : Point1XYZs(p.nodeId).ToList();
                    List<double> p2 = p.xyz.ToList();
                    IWorldPoint wp1 = WP(api, p1);
                    IWorldPoint wp2 = WP(api, p2); 
                    if (pointSize1 >= 1)
                    {
                        IPoint3D pt1 = f3d.CreatePoint(wp1, pointSize1);
                        pt1.Color    = color;
                        grs.Add(pt1); 
                    }
                    if (pointSize2 >= 1)
                    {
                        IPoint3D pt2 = f3d.CreatePoint(wp2, pointSize2);
                        pt2.Color    = color;
                        grs.Add(pt2); 
                    } 
                    if (lineWeight >= 1)
                    { 
                        IPolyline<IWorldPoint> pl = f3d.CreatePolyline(new IWorldPoint[] { wp1, wp2 });
                        pl.Color                  = color;
                        pl.LineWeight             = lineWeight; 
                        grs.Add(pl);
                    } 
                    if (addDistText || addNodeId)
                    {
                        string text = addNodeId ? $"... {p.nodeId} : {p.dist}" : $"... {p.dist}";
                        IText2D  tx = f2d.CreateText(wp2, text);
                        tx.Color    = color;
                        grs.Add(tx);
                    }
                }
            }
            return grs;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Create World Point:
        //
        // -------------------------------------------------------------------------------------------
        public static IWorldPoint WP(IMechanicalExtAPI api, List<double> xyz) => api.Graphics.CreateWorldPoint(xyz[0], xyz[1], xyz[2]);
        public static IWorldPoint WP(IMechanicalExtAPI api, SSimplePoint p)   => api.Graphics.CreateWorldPoint(p.x, p.y, p.z);
        // -------------------------------------------------------------------------------------------
        //
        //      ToStr:
        //
        // ------------------------------------------------------------------------------------------- 
        private static string ToStr(object o) => o.ToString().Replace("EntityManager.", "");
    }
}
 
