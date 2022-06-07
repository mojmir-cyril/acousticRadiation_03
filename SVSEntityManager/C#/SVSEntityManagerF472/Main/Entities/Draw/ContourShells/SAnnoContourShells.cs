#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Drawing;


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

using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Core;
using System.IO;

namespace SVSEntityManagerF472
{

    public class SAnnoContourShells : SAnnoObject
    {  
        public SColors                                     colorBands               { get; private set; } 
        public Dictionary<int, SAnnoPoint.SNode>           pointNodes               { get; private set; } 
        public Dictionary<(int, int), SBorder>             borders                  { get; private set; }
        public Dictionary<(int, int, int), STriangle>      triangles                { get; private set; }
        public List<SAnnoPoint.SNode>                      allPointNodes            { get => pointNodes.Values.ToList(); }
        public List<SAnnoPoint.SIntersect>                 allPointIntersects       { get => allBorders.SelectMany(b => b.pointIntersects).ToList(); }
        public List<SBorder>                               allBorders               { get => borders.Values.ToList();  }
        public List<STriangle>                             allTriangles             { get => triangles.Values.ToList();  } 
        public List<SOneSegment>                           allContourSegments       { get => allTriangles.SelectMany(t => t.contourSegments).ToList(); } 
        public SNodes                                      sNodes                   { get => SNew.NodesFromIds(allTriangles[0].elemFace.em, allPointNodes.Select(n => n.id)); }   // pouze z testovacich duvodu
        //
        //  one-color-region:
        //
        public List<SAnnoPoint.SBase>                      allPoints                { get; private set; }
        public double[]                                    allVerts                 { get; private set; }
        public double[]                                    allNorms                 { get; private set; }  
        public Dictionary<int, int[]>                      colorBandConnectivities  { get; private set; }  //  bandConss[colorBand.index] = int[] { 0,1,2, ... }  body co se maji spojit
        // --------------------------------------------------------------------------------------------------------
        // 
        //      ctor:
        //
        // --------------------------------------------------------------------------------------------------------
        public SAnnoContourShells(SEntityManager em) : base(em)
        {  
            logger.Msg("SAnnoContourShells(...)"); 
            //
            //  dicts:
            //
            pointNodes = new Dictionary<int, SAnnoPoint.SNode>();             // pointNodes[nodeId]
            borders    = new Dictionary<(int, int), SBorder>();         // borders[nodeId1, nodeId2]
            triangles  = new Dictionary<(int, int, int), STriangle>();  // triangles[nodeId1, nodeId2, nodeId3]
        }
        // -------------------------------------------------------------------------------------------
        //
        //      DrawContourShells:
        //
        // ------------------------------------------------------------------------------------------- 
        public void DrawContourShells(SElemFaces          elemFaces, 
                                      Func<int, double>   GetNodalValue         = null, 
                                      Func<int, double[]> GetMorphCoords        = null, 
                                      SColors             colorBands            = null, 
                                      double              sphereSize            = 0.0,       // 0.0 ---> no spheres
                                      int                 wireLineWeight        = 2,      
                                      int                 wireColor             = 0xAAAAAA,
                                      double              translucencyWireframe = 0.0,
                                      double              translucencyContour   = 0.0,
                                      bool                showContours          = true,
                                      bool                showWireframe         = true,
                                      bool                showAllBorderWires    = false,
                                      bool                debugDraw             = false,
                                      bool                useParallel           = true,    
                                      string              exportVTKDataToFile   = "N/A")    
        {
            using (logger.StartStopLog(nameof(DrawContourShells)))
            {
                try
                { 
                    Null(elemFaces,         nameof(elemFaces));   // if (elemFaces  == null)   throw new Exception($"Null error: elemFaces == null. ");
                    Null(colorBands,        nameof(colorBands));  // if (colorBands == null)   throw new Exception($"Null error: colorBands == null. ");
                    NullAndCount(elemFaces, nameof(elemFaces));   // if (elemFaces.count <= 0) throw new Exception($"Count 0 error: elemFaces.count <= 0. ");
                    if (!showContours && !showWireframe) throw new Exception($"Nothing to show (!showContours && !showWireframe). ");
                    //
                    //  color bands:
                    //
                    this.colorBands = colorBands;
                    //
                    //  struct (elemFaces-triangles-borders-nodes):
                    // 
                    elemFaces.ForEach(ef => CreateTriangles(ef));  
                    //
                    //  morphed or original coords:
                    //
                    IMeshData mesh = em.api.DataModel.MeshDataByName("Global");
                    double[] GetCoords(int id)
                    {
                        INode n = mesh.NodeById(id);
                        return new double[] { n.X, n.Y, n.Z };
                    }
                    void UpdateNode(SAnnoPoint.SNode n)
                    {
                        n.xyz         = GetMorphCoords != null ? GetMorphCoords(n.id) : GetCoords(n.id);
                        n.resultValue = Convert.ToDouble(n.id);//GetNodalValue(n.id); 
                        n.colorBand   = colorBands.GetBand(n.resultValue);
                    }
                    if (useParallel) allPointNodes.AsParallel()
                                                  .WithDegreeOfParallelism(em.CPUs)
                                                  .ForAll(UpdateNode);
                    else             allPointNodes.ForEach(UpdateNode);
                    //
                    //  intersect points:
                    // 
                    if (useParallel) allBorders.AsParallel()
                                               .WithDegreeOfParallelism(em.CPUs)
                                               .ForAll(b => b.CreatePointIntersects());
                    else             allBorders.ForEach(b => b.CreatePointIntersects());
                    //
                    //  wrn:
                    //
                    string wrn = string.Join("; ", allBorders.Where(i => i.wrn != ""));
                    if (wrn != "") logger.Wrn($"DrawContourShells(...): {wrn}");
                    //
                    //  log:
                    //  
                    logger.Msg($" - colorBands.borderValues : {string.Join(", ", colorBands.borderValues)} ");
                    logger.Msg($" - allPointNodes(n => n.resultValue): ");
                    logger.Msg($"   - .Count()   : {allPointNodes.Count()}");
                    logger.Msg($"   - .Min()     : {allPointNodes.Min(n => n.resultValue)}");
                    logger.Msg($"   - .Average() : {allPointNodes.Average(n => n.resultValue)}");
                    logger.Msg($"   - .Max()     : {allPointNodes.Max(n => n.resultValue)}");
                    logger.Msg($" - allBorders.Count()    : {allBorders.Count()}");
                    logger.Msg($" - allTriangles.Count()  : {allTriangles.Count()}");
                    //
                    //  contour data:
                    //  
                    if (useParallel) allTriangles.AsParallel()
                                                 .WithDegreeOfParallelism(em.CPUs)
                                                 .ForAll(t => t.CreateSegmentsDatas()); 
                    else             allTriangles.ForEach(t => t.CreateSegmentsDatas()); 
                    //
                    //  DEBUG: spheres & wires:
                    //
                    if (debugDraw)
                    {
                        DrawSpheres(sphereSize, translucencyContour);
                        DrawLabels(wireColor,translucencyContour, firstCount: 10, onlyWithMinPointCount: 5); 
                    }
                    if (showWireframe) DrawWires(wireLineWeight, wireColor, translucencyWireframe, showAllBorders: showAllBorderWires);  
                    //
                    //  one-color-region:
                    //          
                    allPoints = allPointNodes.Cast<SAnnoPoint.SBase>().Union(allPointIntersects).ToList();
                    foreach ((int i, SAnnoPoint.SBase p) in allPoints.Enumerate()) p.indexInAllPoints = i; 
                    if (useParallel) allPoints.AsParallel()
                                              .WithDegreeOfParallelism(em.CPUs)
                                              .ForAll(p => p.EvalAvgTriangleNormal()); 
                    else             allPoints.ForEach(p => p.EvalAvgTriangleNormal()); 
                    allVerts                = allPoints.SelectMany(p => p.xyz).ToArray();
                    allNorms                = allPoints.SelectMany(p => p.avgTriangleNormal).ToArray(); 
                    colorBandConnectivities = new Dictionary<int, int[]>();
                    int minBandIndex = 0;
                    int maxBandIndex = colorBands.maxBand.index;
                    for (int bandIndex = minBandIndex; bandIndex <= maxBandIndex; bandIndex++)
                        colorBandConnectivities[bandIndex] = allContourSegments.Where(cs => cs.colorBand.index == bandIndex)
                                                                               .SelectMany(cs => cs.points.Select(p => p.indexInAllPoints))
                                                                               .ToArray();
                    //
                    //  log:
                    // 
                    logger.Msg($" - minBandIndex           : {minBandIndex} ");
                    logger.Msg($" - maxBandIndex           : {maxBandIndex} ");
                    logger.Msg($" - translucencyContour    : {translucencyContour} ");
                    logger.Msg($" - translucencyWireframe  : {translucencyWireframe} ");
                    //
                    //  draw:
                    //
                    if (showContours) DrawShells(minBandIndex, maxBandIndex, translucencyContour);  
                    //
                    //  export VTK data:
                    //
                    string path = System.IO.Path.GetDirectoryName(exportVTKDataToFile);
                    logger.Msg($" - exportVTKDataToFile      : {exportVTKDataToFile} ");
                    logger.Msg($" - path                     : {path} ");
                    logger.Msg($" - Directory.Exists(path)   : {Directory.Exists(path)} ");
                    if (Directory.Exists(path))
                    {  
                        logger.Msg($" - allPoints.Count          : {allPoints.Count} ");
                        logger.Msg($" - allContourSegments.Count : {allContourSegments.Count} ");
                        string L(IEnumerable<string> v) => string.Join("\n", v);
                        string S(IEnumerable<double> v) => string.Join(";", v);
                        string I(IEnumerable<int> v)    => string.Join(";", v);
                        string allData = $"[ALL-POINTS]\n{L(allPoints.Select(p => S(p.xyz)))}\n";
                        for (int bandIndex = minBandIndex; bandIndex <= maxBandIndex; bandIndex++)
                        {
                            (int r, int g, int b) = colorBands.bands[bandIndex].rgb;
                            SOneSegment[] sgs = allContourSegments.Where(cs => cs.colorBand.index == bandIndex).ToArray();
                            allData += $"\n[COLOR-{r}-{g}-{b}]\n{L(sgs.Select(one => I(one.points.Select(p => p.indexInAllPoints))))}"; 
                        }
                        allData += $"\n[END]";
                        File.WriteAllText(exportVTKDataToFile, allData);
                    }
                    //
                    //  end!
                    //
                }
                catch (Exception err) { Throw(err, nameof(DrawContourShells)); }  // catch (Exception err) { throw new Exception($"DrawContourShells(...): {err.Message}", err); }
            }
        }


        // -------------------------------------------------------------------------------------------
        //
        //      private:
        //
        // ------------------------------------------------------------------------------------------- 

        private void DrawWires(int wireLineWeight, int wireColor, double translucency, bool showAllBorders = false)
        {
            if (wireLineWeight != 0)
            { 
                List<IPolyline<IWorldPoint>> wires = new List<IPolyline<IWorldPoint>>();
                List<SBorder> bs = showAllBorders ? allBorders : allBorders.Where(b => b.isExternal).ToList();
                foreach (SBorder b in bs)
                { 
                    List<IWorldPoint> points = new List<IWorldPoint>();
                    IWorldPoint point1 = api.Graphics.CreateWorldPoint(b.node1.x, b.node1.y, b.node1.z);
                    IWorldPoint point2 = api.Graphics.CreateWorldPoint(b.node2.x, b.node2.y, b.node2.z);
                    points.Add(point1);
                    points.Add(point2);
                    IPolyline<IWorldPoint> wire = api.Graphics.Scene.Factory3D.CreatePolyline(points);
                    wire.Translucency = translucency;
                    wire.Color        = wireColor; 
                    wires.Add(wire); 
                }
                graphicsEntities.AddRange(wires);
            }   
        } 
        private void DrawSpheres(double sphereSize = 0.0, double translucency = 0.0)
        {
            if (sphereSize != 0.0)
            { 
                List<ISphere3D> ss = new List<ISphere3D>();
                foreach (SAnnoPoint.SNode n in allPointNodes)
                { 
                    ISphere3D s    = api.Graphics.Scene.Factory3D.CreateSphere(sphereSize);
                    s.Color        = n.color;
                    s.Translucency = translucency;
                    s.Transformation3D.Translate(api.Graphics.CreateVector3D(n.x, n.y, n.z));  
                    ss.Add(s); 
                }
                graphicsEntities.AddRange(ss);
            } 
        } 
        private void DrawLabels(int textColor, double translucency, int firstCount = 10, int onlyWithMinPointCount = 5)
        {  
             List<IGraphicsEntity> txs = new List<IGraphicsEntity>();
             foreach (STriangle t in allTriangles.Where(t => t.allPoints.Count() >= onlyWithMinPointCount).Take(firstCount))
             {
                 foreach ((int i, SAnnoPoint.SBase p) in t.allPoints.Enumerate())
                 {
                     List<double> p1   = p.xyz.ToList(); 
                     IText2D      tx   = api.Graphics.Scene.Factory2D.CreateText(SDrawUtils.WP(api, p1), $"{i}...{p.GetLabel()}");
                     tx.Color          = textColor;
                     tx.Translucency   = translucency;
                     txs.Add(tx); 
                 } 
             } 
             graphicsEntities.AddRange(txs);
        }
        private void DrawShells(int minBandIndex, int maxBandIndex, double translucency)
        {  
            List<IShell3D> shells = new List<IShell3D>();
            for (int bandIndex = minBandIndex; bandIndex <= maxBandIndex; bandIndex++)
            {
                int[] bandCons = colorBandConnectivities[bandIndex];
                if (bandCons.Count() <= 0) continue;
                IShell3D s     = api.Graphics.Scene.Factory3D.CreateShell(allVerts, allNorms, bandCons);
                s.Color        = colorBands.bands[bandIndex].color;
                s.Translucency = translucency;
                shells.Add(s);
            } 
            graphicsEntities.AddRange(shells);
        }

        private SAnnoPoint.SNode GetPointNode(INode node, STriangle triangle, SElemFace elemFace, bool isCorner)
        { 
            if (pointNodes.ContainsKey(node.Id)) return pointNodes[node.Id]; 
            SAnnoPoint.SNode n = new SAnnoPoint.SNode(node, triangle, elemFace, isCorner);
            pointNodes[n.id] = n;
            return n;
        }
        private SBorder GetBorder(SAnnoPoint.SNode node1, SAnnoPoint.SNode node2, STriangle triangle, SElemFace elemFace)
        {
            (int, int) ids2 = Ids2(node1, node2);
            if (borders.ContainsKey(ids2)) return borders[ids2]; 
            SBorder b = new SBorder(node1, node2, triangle, elemFace); 
            borders[b.ids2] = b;
            return b;
        }
        private STriangle GetTriangle(SElemFace ef, IEnumerable<SNode> nodes, IEnumerable<SAnnoPoint.SXYZ> normals)
        {  
            if (triangles.ContainsKey(Ids3(nodes))) return triangles[Ids3(nodes)]; 
            STriangle t = new STriangle(ef, nodes, normals, GetPointNode);
            triangles[t.ids3] = t; 
            return t;
        }
        private List<STriangle> CreateTriangles(SElemFace ef)
        { 
            //   
            //   
            //   example:
            //   --------
            //       Msg = ExtAPI.Log.WriteMessage
            //       ExtAPI.Graphics.Scene.Clear()
            //       em = EntityManager.SEntityManager(ExtAPI)
            //       ef = em.current.elemFaces 
            //       an = EntityManager.SAnnoContourShells(ExtAPI, Msg, Msg, Msg) 
            //       an.DrawContourShells(ef) 
            //       # 
            //       #  pocty uzlu:
            //       # 
            //       print len(an.allTriangles) 
            //       print len(an.allBorders) 
            //       print len(an.allNodes) 
            //       # 
            //       #  zobrazeni uzlu:
            //       # 
            //       an.allTriangles[0].sNodes.Sel() 
            //       an.allBorders[0].sNodes.Sel()
            //       an.allNodes[0].sNodes.Sel()
            //       an.sNodes.Sel()
            //
            //
            //
            SNodes              nodes     = ef.nodes; // .iNodes.ToList()   // List<INode> nodes = ef.faceShapeData.faceNodes;
            List<List<int>>     indexss   = ef.elemFaceShellDatas.connectivity.ChunkBy(3);  
            List<double[]>      gNormals  = ef.globalNormals.Select(n => n.xyz).ToList();
            List<STriangle> triangles = indexss.Select(indexs => {
                                                                         IEnumerable<SNode>         ids  = indexs.Select(index => nodes[index]);
                                                                         IEnumerable<SAnnoPoint.SXYZ> nrms = indexs.Select(index => new SAnnoPoint.SXYZ(gNormals[index]));
                                                                         return GetTriangle(ef, ids, nrms);
                                                                     }).ToList();  
            //
            //  borders:
            //
            foreach (STriangle t in triangles)
            {
                t.borders.Add(GetBorder(t.nodes[0], t.nodes[1], t, ef));
                t.borders.Add(GetBorder(t.nodes[1], t.nodes[2], t, ef));
                t.borders.Add(GetBorder(t.nodes[2], t.nodes[0], t, ef)); 
                this.triangles[t.ids3] = t;
            }   
            //
            //  return:
            //
            return triangles;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Border:
        //
        // ------------------------------------------------------------------------------------------- 
        public class SBorder
        { 
            public bool                         isExternal          { get; set; } 
            public (int, int)                   ids2                { get => (id1, id2); }
            public int                          id1                 { get => node1.id; }
            public int                          id2                 { get => node2.id; }
            public SAnnoPoint.SNode             node1               { get => nodes[0]; }
            public SAnnoPoint.SNode             node2               { get => nodes[1]; } 
            public List<SAnnoPoint.SNode>       nodes               { get; set; }
            public SNodes                       sNodes              { get => SNew.NodesFromIds(elemFaces[0].em, nodes.Select(n => n.id)); }   // pouze z testovacich duvodu
            public List<STriangle>          triangles           { get; private set; }
            public List<SElemFace>              elemFaces           { get; private set; }
            public List<SAnnoPoint.SIntersect>  pointIntersects     { get; set; }
            public double                       length              { get => SVectorUtils.Dist(node1.xyz, node2.xyz); }
            public SColors                      colorBands          { get => node1.colorBands; }
            public string                       wrn                 { get; private set; } = "";

            public SBorder(SAnnoPoint.SNode node1, SAnnoPoint.SNode node2, STriangle triangle, SElemFace elemFace)
            { 
                nodes      = new List<SAnnoPoint.SNode>  { node1, node2 } .OrderBy(n => n.id).ToList();
                triangles  = new List<STriangle>   { triangle };
                elemFaces  = new List<SElemFace>       { elemFace }; 
                isExternal = nodes.Any(n => n.isCorner);
            } 
            public void CreatePointIntersects()
            {
                if (pointIntersects != null) throw new Exception("CreatePointIntersects(...): pointIntersects != null"); 
                if (node1.colorBand == null) throw new Exception("CreatePointIntersects(...): node1.colorBand == null"); 
                if (node2.colorBand == null) throw new Exception("CreatePointIntersects(...): node2.colorBand == null"); 
                //
                //  new (empty):
                //
                pointIntersects = new List<SAnnoPoint.SIntersect>();  
                //
                //  no point:
                //
                if (colorBands.bands.Count <= 1) return;
                if (node1.colorBand.index == node2.colorBand.index) return;     // stejna barva ---> nejsou inter body
                //
                //  new points:
                //
                //
                //   ================================================
                //                                      band border 
                //    hiVal
                //     o ----------                             
                //     |           ----------   pointItersect             
                //   ====================    ---X------          loVal
                //     |     band border        |      ---------- o       
                //     |                        |                 |
                //     |                        |                 |
                //     |                 ===============================================
                //     |                        |                 |     band border 
                //     |                        |                 |
                //     o -----------------------X---------------- o
                //    node1.xyz                                  node2.xyz
                //
                //     < ----------------- dist ----------------- >
                //
                //

                // double               d         = length; 
                double               val1      = node1.resultValue; 
                double               val2      = node2.resultValue; 
                double               hiVal     = Math.Max(val1, val2);
                double               loVal     = Math.Min(val1, val2);
                List<double>         interVals = colorBands.borderValues.Where(b => loVal < b && b < hiVal)
                                                                        .ToList();  
                List<SAnnoPoint.SXYZ>  txyzs     = interVals.Select(iVal => node1 + (node2 - node1) * ((iVal - val1) / (val2 - val1)))
                                                          .ToList(); 
                pointIntersects                = interVals.Zip(txyzs, (iVal, txyz) => new SAnnoPoint.SIntersect(this, iVal, txyz))
                                                          .Where(p => p.isIntermediate) 
                                                          .ToList();
                wrn                            = string.Join("; ", pointIntersects.Where(i => i.wrn != ""));
            } 
            internal List<SAnnoPoint.SIntersect> GetPointIntersectsSortedFrom(SAnnoPoint.SNode startPointNode)
                => pointIntersects.OrderBy(p => SVectorUtils.Dist(startPointNode.xyz, p.xyz)).ToList();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Triangle:
        //
        // ------------------------------------------------------------------------------------------- 
        public class STriangle : SAnnoUtils
        {
            public SElemFace                    elemFace          { get; }  
            public (int, int, int)              ids3              { get => Ids3(nodes); }  // sorted
            public int                          id1               { get => node1.id; }
            public int                          id2               { get => node2.id; }
            public int                          id3               { get => node3.id; }
            public int                          color1            { get => node1.color; }
            public int                          color2            { get => node2.color; }
            public int                          color3            { get => node3.color; }
            public SColors                      colorBands        { get => node1.colorBands; }
            public SColors.SColor               colorBand1        { get => node1.colorBand; }
            public SColors.SColor               colorBand2        { get => node2.colorBand; }
            public SColors.SColor               colorBand3        { get => node3.colorBand; }
            public SAnnoPoint.SNode             node1             { get => nodes[0]; } 
            public SAnnoPoint.SNode             node2             { get => nodes[1]; } 
            public SAnnoPoint.SNode             node3             { get => nodes[2]; } 
            public List<SAnnoPoint.SNode>       nodes             { get; } 
            public SNodes                       sNodes            { get => SNew.NodesFromIds(elemFace.em, nodes.Select(n => n.id)); }   // pouze z testovacich duvodu
            public List<SBorder>            borders           { get; } 
            public List<double>                 normal            { get; } 
            public List<SOneSegment> contourSegments   { get; set; } 
            public List<SAnnoPoint.SBase>       allPoints         { get; set; } 
            public STriangle(SElemFace elemFace, IEnumerable<SNode> nodes, IEnumerable<SAnnoPoint.SXYZ> normals, Func<INode, STriangle, SElemFace, bool, SAnnoPoint.SNode> GetNode) : base(elemFace.em)
            {
                Null(elemFace, nameof(elemFace), nameof(STriangle));    // if (elemFace == null)   throw new Exception($"SAnnoTriangle(...): Null error: elemFace == null. ");
                NullAndCount(nodes, nameof(nodes), nameof(STriangle));  // if (nodes == null) throw new Exception($"SAnnoTriangle(...): Null error: nodes == null. "); if (nodes.Count() != 3) throw new Exception($"SAnnoTriangle(...): Count 0 error: nodes.Count() != 3. ");
                List<int> iConnerNodes     = elemFace.iConnerNodes.Select(n => n.Id).ToList();
                this.elemFace              = elemFace;
                borders                    = new List<SBorder>(); 
                this.nodes                 = nodes.Select(n => GetNode(n.iNode, this, elemFace, iConnerNodes.Contains(n.id))).ToList();  // pozor nesmi se seradit jinak to rozhodi kresleni !!!        // .OrderBy(n => n.id). 
                List<SAnnoPoint.SXYZ> nrms   = normals.ToList();
                normal                     = ((nrms[0] + nrms[1] + nrms[2]) * 0.3333333).xyz.ToList();      // normal        = SVectorUtils.Normal(node1.iNode, node2.iNode, node3.iNode);
            } 
            public void CreateSegmentsDatas()
            {
                NullAndCount(nodes, nameof(nodes), nameof(STriangle), minCount: 3, maxCount: 3);  // if (nodes.Count() != 3) throw new Exception($"CreateSegmentsDatas(...): Count error: nodes.Count() != 3. ");
                Null(colorBands, nameof(colorBands), nameof(CreateSegmentsDatas)); // if (colorBands == null) throw new Exception($"CreateSegmentsDatas(...): Null error: colorBands == null. "); 
                //
                //  new:
                //
                contourSegments = new List<SOneSegment>();
                //
                //  types:
                //
                if (color1 == color2 && color2 == color3)
                { 
                    allPoints = nodes.Cast<SAnnoPoint.SBase>().ToList();
                    contourSegments.Add(new SOneSegment(colorBand1, allPoints, this));
                }
                else
                {
                    List<SColors.SColor> ccc = new List<SColors.SColor> { colorBand1, colorBand2, colorBand3 };  
                    int min = ccc.Min(c => c.index);
                    int max = ccc.Max(c => c.index); 
                    //
                    //  all points:
                    // 
                    allPoints = new List<SAnnoPoint.SBase>();
                    for (int i = 0; i < 3; i++)
                    {
                        allPoints.Add(nodes[i]);
                        allPoints.AddRange(borders[i].GetPointIntersectsSortedFrom(nodes[i]));
                    } 
                    //
                    //  segments:
                    //
                    for (int i = min; i <= max; i++)
                    {
                        SColors.SColor band = colorBands.bands[i]; 
                        List<SAnnoPoint.SBase> ps = allPoints.Where(p => p.HasBand(band.index)).ToList(); 
                        //
                        //  by count:
                        //
                        if (ps.Count() == 3)
                        {
                            contourSegments.Add(new SOneSegment(band, ps, this));
                        }
                        else if (ps.Count() == 4)
                        {
                            contourSegments.Add(new SOneSegment(band, new SAnnoPoint.SBase[] { ps[0], ps[1], ps[2] }, this));
                            contourSegments.Add(new SOneSegment(band, new SAnnoPoint.SBase[] { ps[2], ps[3], ps[0] }, this));
                        }
                        else if (ps.Count() == 5)
                        {
                            contourSegments.Add(new SOneSegment(band, new SAnnoPoint.SBase[] { ps[0], ps[1], ps[2] }, this));
                            contourSegments.Add(new SOneSegment(band, new SAnnoPoint.SBase[] { ps[0], ps[2], ps[3] }, this));
                            contourSegments.Add(new SOneSegment(band, new SAnnoPoint.SBase[] { ps[0], ps[3], ps[4] }, this));
                        }
                        else
                        {
                            throw new Exception($"CreateShellDatas(...): ps.Count() = {ps.Count()}");
                        }
                    } 
                }
            }
        }     
        
        // -------------------------------------------------------------------------------------------
        //
        //      OneContourSegment:
        //
        // ------------------------------------------------------------------------------------------- 
        public class SOneSegment
        {
            public STriangle                triangle     { get; } 
            public SElemFace                elemFace     { get => triangle.elemFace; }  
            public SColors                  colorBands   { get => colorBand.colorBands; }
            public SColors.SColor           colorBand    { get; set; }
            public int                      color        { get => colorBand.color; }
            public SAnnoPoint.SBase           point1       { get => points[0]; } 
            public SAnnoPoint.SBase           point2       { get => points[1]; } 
            public SAnnoPoint.SBase           point3       { get => points[2]; } 
            public List<SAnnoPoint.SBase>     points       { get; } 
            public List<double>             normal       { get => triangle.normal; } 
            public SOneSegment(SColors.SColor colorBand, IEnumerable<SAnnoPoint.SBase> points, STriangle triangle)  
            { 
                this.triangle   = triangle; 
                this.colorBand  = colorBand;
                this.points     = points.ToList();
            } 
        }
        
    }
}
