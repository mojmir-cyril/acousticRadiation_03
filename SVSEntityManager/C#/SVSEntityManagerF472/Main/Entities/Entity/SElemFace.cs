#pragma warning disable IDE1006                         // Naming Styles

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Mechanical;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mesh;
using MathNet.Numerics.LinearAlgebra;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SElemFace is single element face object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// Note, ACT has not any similar object.
    /// </summary>
    public class SElemFace : SEntity
    {
        /// <summary>
        /// The SEntityManager object created by SVS FEM s.o.r. for fast/easy work with geometrical entitites.
        /// The main instance (em) genarally keeps all necessary settings for selecting.
        /// </summary>

        public IMechanicalExtAPI    api                 { get; }
        /// <summary>
        /// gets internal (ACT) object of the attached element (IElement)
        /// </summary>
        public IElement             iElem               { get => (IElement)iEntity; }
        /// <summary>
        /// gets attached element
        /// </summary>
        public SElem                elem                { get => SNew.SElem(em, iElem); }
        /// <summary>
        /// gets list of reference unique Ids
        /// </summary> 
        public List<int>            geoEntityIds        { get => iElem.CornerNodes.SelectMany(n => n.GeoEntityIds).ToList(); }
        /// <summary>
        /// gets geometry type of the entities => SType.ElemFace
        /// </summary>
        public override SType       type                { get => SType.ElemFace; }
        /// <summary>
        /// gets true if elemental face type of the entities
        /// </summary> 
        public override bool        isElemFace          { get => true; }
        /// <summary>
        /// gets element type (ACT) ElementTypeEnum { kTet10, kHex8, kHex20, ... }
        /// </summary> 
        public ElementTypeEnum      elemType            { get => iElem.Type; }
        /// <summary>
        /// gets element face shape data (important for drawing of faces ...)
        /// </summary> 
        public SFaceShapeData       faceShapeData       { get; }
        /// <summary>
        /// gets face shape type SFaceType { Quad8, Quad4, Tri6, Tri3 }
        /// </summary> 
        public SFaceType            faceShapeType       { get; }
        /// <summary>
        /// gets face (ACT) nodes INode
        /// </summary> 
        public List<INode>         iFaceNodes          { get; }
        /// <summary>
        /// gets face (ACT) corner nodes INode
        /// </summary> 
        public List<INode>         iConnerNodes        { get; }
        /// <summary>
        /// gets face (ACT) node Ids
        /// </summary> 
        public List<int>            faceNodeIds         { get; }
        public Dictionary<int, int> __faceNodeIds       { get; }
        /// <summary>
        /// gets face (ACT) corner node Ids
        /// </summary> 
        public List<int>            cornersIds          { get; }
        /// <summary>
        /// gets face node count
        /// </summary> 
        public int                  faceNodeCount       { get => faceNodeIds.Count(); }
        /// <summary>
        /// gets face corner node count
        /// </summary> 
        public int                  cornersCount        { get => cornersIds.Count(); }
        /// <summary>
        /// gets face shell datas (vertices, connectivity, normal indexes)
        /// </summary> 
        public SElemFaceShellDatas  elemFaceShellDatas  { get => __efsd != null ? __efsd : __Efsd(); }
        public List<double> vertices { get => elemFaceShellDatas.vertices; }
        public List<int> connectivities { get => elemFaceShellDatas.connectivity; }
        //
        //  private:
        //
        private IMeshData           __mesh              { get => em.api.DataModel.MeshDataByName("Global"); }
        private SNodes              __faceNodes         { get; }
        private SNodes              __connerNodes       { get; }
        private SElemFaceShellDatas __efsd              { get; set; }
        private SElemFaceShellDatas __Efsd()            { __efsd = new SElemFaceShellDatas(faceShapeData.faceShape, faceNodeIds, elemType, elemFaceId, __mesh); return __efsd; }

        // -------------------------------------------------------------------------------------------
        //
        //      normal:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets face normal (average)
        /// </summary> 
        public SNormal              normal              { get => __GetNormal(); }
        /// <summary>
        /// gets face list of normal for each node
        /// </summary> 
        public List<SNormal>        normals             { get => __GetNormals(); }
        /// <summary>
        /// gets face normal (average) in polar coordinates
        /// </summary> 
        public SNormal              polarNormal         { get => __GetNormal(polarNormal: true); }
        /// <summary>
        /// gets face list of normal for each node in polar coordinates
        /// </summary> 
        public List<SNormal>        polarNormals        { get => __GetNormals(polarNormal: true); }
        /// <summary>
        /// gets face normal (average) in global cartesian coordinates
        /// </summary> 
        public SNormal              globalNormal        { get => __GetGlobalNormal(); }
        /// <summary>
        ///  gets face list of normal for each node in global cartesian coordinates
        /// </summary> 
        public List<SNormal>        globalNormals       { get => __GetGlobalNormals(); }
        public IEnumerable<double>  globalNormalsIEnum  { get => globalNormals.SelectMany(gn => gn.xyz.ToList()); }
        /// <summary>
        /// gets areas of element subFaces (triangles) 
        /// </summary>
        private List<double>        partialElFaceAreas  { get => __GetPartialAreas(); }
        /// <summary>
        /// gets area of element face
        /// </summary>
        public double               elemFaceArea          { get => partialElFaceAreas.Sum(); }
        /// <summary>
        /// gets area of element face divided by number of its corner nodes -> 
        /// </summary>
        public double               elemFaceAreaPerNode   { get => elemFaceArea / corners.count; }

        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets element unique Id
        /// </summary>
        public override int         id                  { get => iElem.Id; }
        /// <summary>
        /// gets face Id on the element (e.g.: 1, 2, 3, ...)
        /// </summary>
        public override int         elemFaceId          { get; }
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.elemFaces.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        public override SInfo       info                { get => SInfo.NewElemFaceInfo(id, elemFaceId); }
        // -------------------------------------------------------------------------------------------
        //
        //      shape/type groups:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets true if element face is internal 
        /// </summary>
        public bool                 isInt               { get => !isExt; }                 // internal
        /// <summary>
        /// gets true if element face is external 
        /// </summary>
        public bool                 isExt               { get => _IsExternal(); } //  => __IsExternal(); }  // external
        /// <summary>
        /// gets true if element face is quad shape type 
        /// </summary>
        public bool                 isQuad              { get => faceShapeType == SFaceType.Quad4 || faceShapeType == SFaceType.Quad8; }
        /// <summary>
        /// gets true if element face is tri shape type 
        /// </summary>
        public bool                 isTri               { get => faceShapeType == SFaceType.Tri3  || faceShapeType == SFaceType.Tri6; }
        /// <summary>
        /// gets true if element face is linear shape type 
        /// </summary>
        public bool                 isLinear            { get => faceShapeType == SFaceType.Tri3  || faceShapeType == SFaceType.Quad4; }
        /// <summary>
        /// gets true if element face is quadratic shape type 
        /// </summary>
        public bool                 isQuadratic         { get => faceShapeType == SFaceType.Quad8 || faceShapeType == SFaceType.Tri6; }
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes      corners             { get => SNew.NodesFromIds(em, iElem.CornerNodeIds) * nodes; }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes      mids                { get => SNew.NodesFromIds(em, iElem.NodeIds.Where(id => !iElem.CornerNodeIds.Contains(id))) * nodes; }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts      parts               { get => nodes.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies     bodies              { get => nodes.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces      faces               { get => nodes.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges      edges               { get => nodes.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts      verts               { get => nodes.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>

        public override SNodes      nodes               { get => __faceNodes; }
        /// <summary>
        /// converts to attached elements
        /// </summary> 
        public override SElems      elems               { get => elem.elems; } //  __connerNodes.elems
        /// <summary>
        /// converts to attached element faces
        /// </summary> 
        public override SElemFaces  elemFaces           { get => SNew.ElemFacesFromIds(em, id, elemFaceId); }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElemFace(SEntityManager em, int id, int elemFaceId) : base(em, em.mesh.GetElem(id))
        {
            this.elemFaceId = elemFaceId; 
            IElement elem   = em.mesh.GetElem(id);
            //
            //  node data:
            //
            faceShapeData  = SFaceShapeData.NewFaceShapeData(elem, elemFaceId);
            Null(faceShapeData, nameof(faceShapeData), nameof(SElemFace));  // if (faceShapeData == null) throw new Exception($"SElemFace(...): Null error: faceShapeData == null. ");
            faceShapeType  = faceShapeData.faceShape;
            iFaceNodes     = faceShapeData.faceNodes.ToList();
            iConnerNodes   = faceShapeData.corners.ToList(); 
            faceNodeIds    = iFaceNodes.Select(n => n.Id).ToList(); 
            cornersIds     = iConnerNodes.Select(n => n.Id).ToList(); 
            __faceNodes    = SNew.NodesFromIds(em, faceNodeIds);
            __connerNodes  = SNew.NodesFromIds(em, cornersIds);
            __faceNodeIds  = faceNodeIds.ToDictionary(i => i, i => i);  
        }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SElemFace(elemId = '{id}', elemFaceId = '{elemFaceId}')";
        // -------------------------------------------------------------------------------------------
        //
        //      __IsExternal:
        //
        // ------------------------------------------------------------------------------------------- 
        private bool? __isExt { get; set; }
        private bool _IsExternal()
        {
            if (__isExt != null) return (bool)__isExt;
            using (em.logger.StartStop(nameof(_IsExternal)))
            {
                int c = nodes.count; // return elems.If(e => e.id != id).If(e => (nodes * e.nodes).count == c).count <= 0;  
                // __isExt = nodes.elems.Where(e => e.id != id).Where(e => nodes.ids.Intersect(e.nodes.ids).Count() == c).Count() <= 0;  
                __isExt = nodes.elems          
                               .Where(e => e.id != id)
                               .Where(e => nodes.ids.Intersect(e.nodes.ids).Count() == c)
                               .Count() <= 0;  
                return (bool)__isExt;
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends element faces by a function.  
        /// </summary>
        /// <example>
        /// <code>
        /// em.current.Extend(lambda cur, any: cur.x == any.x).Sel()
        /// em.current.Extend(lambda cur, any: cur.x == any.x).showns.Sel()
        /// #
        /// # where:
        /// #   cur .. is currently selected object (em.current)
        /// #   any .. is any object in the model
        /// #
        /// </code>
        /// </example>
        /// <param name="func">SElemFace, SElemFace, bool</param>
        /// <returns>new SElemFace</returns>
        public SElemFaces Extend(Func<SElemFace, SElemFace, bool> func) => elemFaces.Extend(func); 
        // -------------------------------------------------------------------------------------------
        //
        //      update (e.g. coords after morphing):
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// updates (re-creates) element faces (e.g. coords after morphing)
        /// </summary>
        public override SEntity Update()
        {
            em.usedElemFaces.Clear();
            iEntity = em.mesh.GetElem(id);  
            __Efsd();
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      normal & normals:
        //
        // -------------------------------------------------------------------------------------------
        private SNormal __GetNormal(bool polarNormal = false)
        { 
            SNormal norm = __GetGlobalNormal(); 
            //
            //  local & polar:
            //
            return SNormalUtils.ToLocalPolar(em, norm, polarNormal);
        }
        private SNormal __GetGlobalNormal()
        {
            List<SNormal> nrms = __GetGlobalNormals();
            SNormal       norm = new SNormal(nrms.Average(n => n.x), nrms.Average(n => n.y), nrms.Average(n => n.z)).Norm(1);
            return norm;
        }
        private List<SNormal> __GetGlobalNormals()
        {
            var normals = new List<SNormal>();
            foreach (SNorm n123 in elemFaceShellDatas.normalIndexes)
            {
                INode n1 = __mesh.NodeById(faceNodeIds[n123.c1]);
                INode n2 = __mesh.NodeById(faceNodeIds[n123.c2]);
                INode n3 = __mesh.NodeById(faceNodeIds[n123.c3]);
                normals.Add(new SNormal(SVectorUtils.Normal(n1, n2, n3)));
            }
            bool          flip = false; // (elemFaceId == 3 || elemFaceId == 2) && faceShapeType == SFaceType.Tri6;
            double        sign = flip ? -1.0 : 1.0;
            List<SNormal> nrms = normals.Select(x => x.Norm(1) * sign).ToList();
            //
            //  ret:
            //
            return nrms;
        }
        private List<double> __GetPartialAreas()
        {
            var normals = new List<SNormal>();
            List<double> areas = new List<double>();
            List<SNorm> normalIndexesForAreaComp = new List<SNorm>();

            foreach (SNorm n123 in elemFaceShellDatas.normalIndexesForAreaComp)
            {
                //if (elemFaceShellDatas.areaNormalInds.Contains(elemFaceShellDatas.normalIndexesForAreaComp.IndexOf(n123)))
                //{
                    INode n1 = __mesh.NodeById(faceNodeIds[n123.c1]);
                    INode n2 = __mesh.NodeById(faceNodeIds[n123.c2]);
                    INode n3 = __mesh.NodeById(faceNodeIds[n123.c3]);
                    normals.Add(new SNormal(SVectorUtils.NormalOrig(n1, n2, n3)));
                    areas.Add(SVectorUtils.SRSS(SVectorUtils.NormalOrig(n1, n2, n3)) / 2);
                //}
            }
            bool          flip = false; // (elemFaceId == 3 || elemFaceId == 2) && faceShapeType == SFaceType.Tri6;
            double        sign = flip ? -1.0 : 1.0;
            //
            //  ret:
            //
            return areas.Select(a => em.ConvertArea(a)).ToList();
        }

        private List<SNormal> __GetNormals(bool polarNormal = false)
        { 
            //
            //  local & polar:
            //
            return __GetGlobalNormals().Select(norm => SNormalUtils.ToLocalPolar(em, norm, polarNormal)).ToList();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      HasNode:
        //
        // -------------------------------------------------------------------------------------------
        internal bool HasNode(int nodeId) => __faceNodeIds.ContainsKey(nodeId);
        public void DrawElemFace(int color = 0x000000)
        {
            DrawShells(color);
        }
        private void DrawShells(int color)
        {
            IMechanicalExtAPI api = em.api;
            List<IShell3D> shells = new List<IShell3D>();
            IShell3D s = api.Graphics.Scene.Factory3D.CreateShell(vertices, globalNormalsIEnum, connectivities);
            s.Color = color; //colorBands.bands[bandIndex].color;
            //s.Translucency = translucency;
            shells.Add(s);
                                    
        }

        /// <summary>
        /// gets averaged results for element face in solver unit system unit, each component (eg. Ux, Uy, Uz)
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="ResultName"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public List<double> GetElemFaceResult(double freq, string ResultName, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            //DateTime time1 = DateTime.Now;
            IMechanicalExtAPI api = em.api;
            //DateTime time2 = DateTime.Now;
            //Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            //DateTime time3 = DateTime.Now;
                  
            resData.CurrentTimeFreq = freq;

            if (!resData.ResultNames.Contains(ResultName)) { throw new Exception(string.Format("Variable ResultName doesn't contain proper values. Proper values are: {0}", resData.ResultNames.ToString())); }
            Ansys.ACT.Interfaces.Post.IResult res = resData.GetResult(ResultName);
            
            //DateTime time4 = DateTime.Now;

            List<double>        resVals     = res.GetNodeValues(nodes.info.Ids.AsEnumerable()).ToList();
            List<List<double>>  resValComps = new List<List<double>>();
            for (int i = 0; i < res.Components.Count(); i++)
            {
                resValComps.Add(resVals.Where((x, j) => j-i % res.Components.Count() == 0).ToList());
            }
            //DateTime time5 = DateTime.Now;

            List<double> elemFaceresValComps = resValComps.Select(x=>x.Average()).ToList();
            //DateTime time6 = DateTime.Now;

            //var elemFaceResult

            //SResult res = nodes.result.Assign(ResultName);
            //List<double> ResVals = res.values;
            //DateTime time7 = DateTime.Now;

            //em.logger.Msg($"timeDiff21   : {(time2 - time1).TotalMilliseconds  } ms");
            //em.logger.Msg($"timeDiff32   : {(time3 - time2).TotalMilliseconds  } ms");
            //em.logger.Msg($"timeDiff43   : {(time4 - time3).TotalMilliseconds  } ms");
            //em.logger.Msg($"timeDiff54   : {(time5 - time4).TotalMilliseconds  } ms");
            //em.logger.Msg($"timeDiff65   : {(time6 - time5).TotalMilliseconds  } ms");
            //em.logger.Msg($"timeDiff76   : {(time7 - time6).TotalMilliseconds  } ms");
            return elemFaceresValComps;
        }
        /// <summary>
        /// gets value of element face normal displacement U in meters [m]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceNormalU(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            List<double> efU = GetElemFaceResult(freq, "U", analysis, resData);

            Vector<double> normalVect = Vector<double>.Build.DenseOfArray(globalNormal.xyz);
            Vector<double> efUVect = Vector<double>.Build.DenseOfArray(efU.ToArray());
            double efNormalU = normalVect * efUVect;
            string solverUnit = analysis.CurrentConsistentUnitFromQuantityName("Length");
            double efNormalUinMeters = Ansys.Core.Units.UnitsManager.ConvertUnit(efNormalU, solverUnit, "m", "Length");


            return efNormalUinMeters;
        }
        /// <summary>
        /// gets value of element face normal velocity V in meters per second [m/s]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceNormalV(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            double          omega   = 2 * Math.PI * freq;

            return GetElemFaceNormalU(freq, analysis, resData) * omega;
        }
        /// <summary>
        /// gets value of element face specific ERP (euquivalent radiated power) in watts per meters squared [W/m**2]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceSpecERP(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            double ro = 1.2041; // air density [kg/m**3]
            double c = 343.25; // speed of sound [m/s]
            
            return ro * c / 2.0 * Math.Pow(GetElemFaceNormalV(freq, analysis, resData), 2);
        }
        /// <summary>
        /// gets value of element face ERP (euquivalent radiated power) in watts [W]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceERP(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            double specERP = GetElemFaceSpecERP(freq, analysis, resData);

            return specERP * elemFaceArea;
        }
        /// <summary>
        /// gets value of element face specific ERP Level (euquivalent radiated power level) in decibells [dB]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceSpecERPLevel(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            double speERP = GetElemFaceSpecERP(freq, analysis, resData);
            double WRef = 1e-12;
            if (speERP <= WRef) { return 0; }
            else { return 10 * Math.Log10(speERP / WRef); }
        }
        /// <summary>
        /// gets value of element face ERP Level (euquivalent radiated power level) in decibells [dB]
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="analysis"></param>
        /// <param name="resData"></param>
        /// <returns></returns>
        public double GetElemFaceERPLevel(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, Ansys.ACT.Interfaces.Post.IResultReader resData)
        {
            double ERP = GetElemFaceERP(freq, analysis, resData);
            double WRef = 1e-12;
            if(ERP <= WRef) { return 0; }
            else { return 10 * Math.Log10(ERP / WRef); }
            
        }
    }
}
// kUnknown = -1,
// kPoint0 = 0,
// kLine2 = 1,
// kLine3 = 2,
// kBeam3 = 3,
// kBeam4 = 4,
// kTri3 = 5,
// kTri6 = 6,
// kQuad4 = 7,
// kQuad8 = 8,
// kTet4 = 9,
// kTet10 = 10,
// kHex8 = 11,
// kHex20 = 12,
// kWedge6 = 13,
// kWedge15 = 14,
// kPyramid5 = 15,
// kPyramid13 = 16

//
//  model: elemShapes
//
//   f = DataModel.Project.Model.NamedSelections.Children[0]
//   n = DataModel.Project.Model.NamedSelections.Children[1]
//   
//   mesh = ExtAPI.DataModel.MeshDataByName("Global")
//   ids = [10, 8, 2, 6, 9, 1, 5, 3]   # ... solids
//   ids = [38, 48, 49, 50]            # ... shells
//   elems = [mesh.ElementById(id) for id in ids]
//   
//   for e in elems:
//       print e.Type
//       print[id for id in e.NodeIds] 
//       for face in range(8):
//           try:
//               i = ExtAPI.SelectionManager.CreateSelectionInfo(SelectionTypeEnum.MeshElementFaces)
//               i.Ids = [e.Id]
//               i.ElementFaceIndices = [face]
//               f.Location = i
//               n.Generate()
//               if len(n.Location.Ids) <= 0: continue
//               print "{}: {}".format(face, [list(e.NodeIds).index(id) for id in n.Location.Ids])
//                   except: pass
// 
// kWedge15
// [83, 85, 84, 88, 86, 87, 90, 92, 89, 96, 95, 97, 91, 94, 93]
// 0: [0, 2, 1, 8, 6, 7]
// 1: [4, 5, 3, 10, 9, 11]
// 2: [0, 1, 4, 3, 6, 12, 13, 9]
// 3: [2, 1, 4, 5, 7, 14, 13, 10]
// 4: [0, 2, 5, 3, 8, 12, 14, 11]
// kHex20
// [55, 58, 57, 56, 62, 59, 60, 61, 64, 68, 66, 63, 72, 71, 73, 74, 65, 70, 69, 67]
// 0: [0, 1, 5, 4, 8, 16, 17, 12]
// 1: [2, 1, 5, 6, 9, 18, 17, 13]
// 2: [3, 2, 6, 7, 10, 19, 18, 14]
// 3: [0, 3, 7, 4, 11, 16, 19, 15]
// 4: [0, 3, 2, 1, 11, 8, 10, 9]
// 5: [5, 6, 7, 4, 13, 12, 14, 15]
// kTet10
// [5, 8, 7, 6, 11, 14, 10, 9, 13, 12]
// 0: [0, 3, 1, 7, 4, 8]
// 1: [3, 2, 1, 9, 8, 5]
// 2: [0, 3, 2, 7, 6, 9]
// 3: [0, 2, 1, 6, 4, 5]
// kPyramid13
// [33, 32, 35, 34, 38, 44, 45, 49, 47, 48, 46, 53, 51]
// 0: [1, 0, 3, 2, 5, 6, 8, 7]
// 1: [4, 10, 9, 1, 0, 5]
// 2: [4, 10, 11, 1, 2, 6]
// 3: [4, 12, 11, 3, 2, 7]
// 4: [4, 9, 12, 0, 3, 8]
// kHex8
// [75, 78, 77, 76, 82, 79, 80, 81]
// 0: [0, 1, 5, 4]
// 1: [2, 1, 5, 6]
// 2: [3, 2, 6, 7]
// 3: [0, 3, 7, 4]
// 4: [0, 3, 2, 1]
// 5: [5, 6, 7, 4]
// kTet4
// [1, 4, 3, 2]
// 0: [0, 3, 1]
// 1: [3, 2, 1]
// 2: [0, 3, 2]
// 3: [0, 2, 1]
// kWedge6
// [24, 26, 25, 29, 27, 28]
// 0: [0, 2, 1]
// 1: [4, 5, 3]
// 2: [0, 1, 4, 3]
// 3: [2, 1, 4, 5]
// 4: [0, 2, 5, 3]
// kPyramid5
// [17, 19, 20, 18, 23]
// 0: [0, 3, 1, 2]
// 1: [4, 0, 1]
// 2: [4, 1, 2]
// 3: [4, 3, 2]
// 4: [4, 0, 3]
// 
// ------------------------------------------------------------------------------------------
// 
// kQuad4
// [51, 50, 52, 53]
// 0: [1, 0, 2, 3]
// 1: [1, 0, 2, 3]
// kTri6
// [75, 73, 74, 77, 76, 78]
// 0: [1, 2, 0, 4, 3, 5]
// 1: [1, 2, 0, 4, 3, 5]
// kTri3
// [81, 79, 80]
// 0: [1, 2, 0]
// 1: [1, 2, 0]
// kQuad8
// [84, 85, 83, 82, 89, 88, 86, 87]
// 0: [3, 2, 0, 1, 6, 7, 5, 4]
// 1: [3, 2, 0, 1, 6, 7, 5, 4]
// 




// string faceShapeType = "N/A";
// string elemType = elem.Type.ToString();
// Dictionary<int, string> faceShapeTypes;
// switch (elemType)
// {
//     case "kHex20": faceShapeTypes = new Dictionary<int, string>() { { 1, "8-node face" }, { 2, "8-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "8-node face" }, { 6, "8-node face" } }; break;
//     case "kTet10": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "6-node face" }, { 3, "6-node face" }, { 4, "6-node face" } }; break;
//     // case "kWedge15": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "8-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "6-node face" } }; break;
//     case "kWedge15": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "6-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "8-node face" } }; break;
//     case "kHex8": faceShapeTypes = new Dictionary<int, string>() { { 1, "4-node face" }, { 2, "4-node face" }, { 3, "4-node face" }, { 4, "4-node face" }, { 5, "4-node face" }, { 6, "4-node face" } }; break;
//     case "kTet4": faceShapeTypes = new Dictionary<int, string>() { { 1, "3-node face" }, { 2, "3-node face" }, { 3, "3-node face" }, { 4, "3-node face" } }; break;
//     case "kWedge6": faceShapeTypes = new Dictionary<int, string>() { { 1, "3-node face" }, { 2, "4-node face" }, { 3, "4-node face" }, { 4, "4-node face" }, { 3, "3-node face" } }; break;
//     default: throw new Exception("SMappingSimpleSurface.GetShellDatas(...): switch (elemType), elemType = " + elemType.ToString());
// }
// try { faceShapeType = faceShapeTypes[elemFaceId + 1]; }  // indexovani od 0 => 1 !!
// catch (Exception e) { throw new Exception("SMappingSimpleSurface.GetShellDatas(...): faceShapeTypes[this.elemFaceId + 1], this.elemFaceId = " + this.elemFaceId.ToString() + ", Exception: " + e.ToString()); }
// // 
// //  check:
// // 
// if (Convert.ToInt32(faceShapeType.Substring(0, 2).Replace("-", "")) != faceShape)
//     throw new Exception($"SMappingSimpleSurface.GetShellDatas(...): \n " +
//                         $"faceShapeType = {faceShapeType} != faceShape = {faceShape}  \n" +
//                         $"for elem.Id = {elem.Id}, elemType = {elemType}, elemFaceId = {elemFaceId}. ");
// //
// //  return:
// //
// return faceShapeType;

// public (List<double>, List<double>, List<int>) GetShellDatas(Action<object> Msg, IMeshData mesh)
// {
//     elem = mesh.ElementById(elementId);
//     //
//     //  check:
//     //
//     if (elem == null) throw new Exception($"GetShellDatas(...): Element cannot be obtained from id = {elementId}. ");
//     //
//     //  type:
//     //
//     string faceShapeType = __GetFaceShapeType();
//     switch (faceShapeType)
//     {
//         case "8-node face":
//             this.connectivity = new List<int>() { 1, 4, 5, 4, 0, 7, 7, 6, 4, 4, 6, 5, 7, 3, 6, 6, 2, 5 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 7, 4), NewCon(1, 4, 5), NewCon(2, 5, 6), NewCon(3, 6, 7), NewCon(4, 7, 5), NewCon(5, 4, 6), NewCon(6, 5, 7), NewCon(7, 6, 4) };
//             break;
//         case "6-node face":
//             this.connectivity = new List<int>() { 1, 3, 4, 3, 0, 5, 5, 2, 4, 3, 5, 4 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 5, 3), NewCon(1, 3, 4), NewCon(2, 4, 5), NewCon(3, 5, 4), NewCon(4, 3, 5), NewCon(5, 4, 3) };
//             break;
//         case "4-node face":
//             this.connectivity = new List<int>() { 1, 0, 3, 3, 2, 1 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 2, 1), NewCon(1, 0, 2), NewCon(2, 1, 3), NewCon(3, 2, 1) };
//             break;
//         case "3-node face":
//             this.connectivity = new List<int>() { 1, 0, 2 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 2, 1), NewCon(1, 0, 2), NewCon(2, 1, 0) };
//             break;
//         default: throw new Exception("SMappingSimpleSurface.GetShellDatas(...): switch (faceShapeType), faceShapeType = " + faceShapeType.ToString());
//     }
//     //
//     //  Vysledne vertices
//     //
//     this.vertices = new List<double>();
//     //
//     //  Id uzlu jednoho faceElem
//     // 
//     List<INode> nodes = (from id in this.faceNodeIds select mesh.NodeById(id)).ToList();
//     foreach (INode n in nodes)
//     {
//         this.vertices.Add(n.X);
//         this.vertices.Add(n.Y);
//         this.vertices.Add(n.Z);
//     }
//     //
//     //  Normals:
//     //
//     this.normals = new List<double>();
//     foreach (List<int> n123 in this.normalIndexes)
//     {
//         INode n1 = mesh.NodeById(this.faceNodeIds[n123[0]]);
//         INode n2 = mesh.NodeById(this.faceNodeIds[n123[1]]);
//         INode n3 = mesh.NodeById(this.faceNodeIds[n123[2]]);
//         this.normals.AddRange(Normal(n1, n2, n3));
//     }
//     return (this.vertices, this.normals, this.connectivity);
// }