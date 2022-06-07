#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member



using System;
using System.Collections.Generic;
using System.Linq; 
using System.Collections;

//
//  Ansys:
//  
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Mechanical;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SElemFaces is collection of SElemFace objects.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SElemFaces : SEntities<SElemFace, SElemFaces>, IEnumerable
    {
        public SColors colorsObj = new SColors();
        /// <summary>
        /// gets list of internal (ACT) attached element objects (IElement)
        /// </summary>
        public List<IElement> iElems { get => entities.Select(x => x.iElem).ToList(); }  // Ansys.ACT.Common.Mesh.NodeWrapper
        /// <summary>
        /// gets geometry type of the entities => SType.ElemFace
        /// </summary>
        public override SType type { get => SType.ElemFace; }
        /// <summary>
        /// gets true if elemental face type of the entities
        /// </summary> 
        public override bool isElemFace { get => true; }
        /// <summary>
        /// gets element face Ids (indices) { 0, 1, 2, 3, ... }
        /// </summary> 
        public override List<int> elemFaceIds { get => entities.Select(x => x.elemFaceId).ToList(); } // elem-face (ElementFaceIndices)
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets only external element faces in the model
        /// </summary>
        public SElemFaces exts { get => _GetExternals(); }  // external
        private SElemFaces _GetExternals()
        {
            using (em.logger.StartStop(nameof(_GetExternals)))
            {
                // return SNew.FromList<SElemFace, SElemFaces>(em, this.AsParallel()
                //                                                     .WithDegreeOfParallelism(em.CPUs) 
                //                                                     .Where(e => e.isExt));
                // --------------------------------------------------------------------------------------- 
                SNodes exs = faces.nodes;
                return (nodes * exs).elemFacesIn;
            }
        }
        /// <summary>
        /// gets the other element faces in the model
        /// </summary>
        public SElemFaces invert { get => em.elemFaces - this; }
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.faces.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        public override SInfo info { get => SInfo.NewElemFaceInfo(ids, elemFaceIds); }
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes corners { get => SNew.NodesFromList(em, entities.SelectMany(ef => ef.corners)); }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes mids { get => SNew.NodesFromList(em, entities.SelectMany(ef => ef.mids)); }
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        public List<SNormal> normals { get => Get(x => x.normal.Norm(1)); }
        public List<SNormal> globalNormals { get => Get(x => x.globalNormal.Norm(1)); }
        public SNormal avgNormal { get => SNormal.Avg(normals).Norm(1); } //  new SNormal(normals.Average(n => n.x), normals.Average(n => n.y), normals.Average(n => n.z));
        public SNormal avgGlobalNormal { get => SNormal.Avg(globalNormals).Norm(1); } //  new SNormal(globalNormals.Average(n => n.x), globalNormals.Average(n => n.y), globalNormals.Average(n => n.z)); }
        /// <summary>
        /// gets areas of element faces
        /// </summary>
        public List<double> areas { get => Get(x => x.elemFaceArea); }
        /// <summary>
        /// gets areas of element faces, each divided by number of its nodes
        /// </summary>
        public List<double> areasPerNodes { get => Get(x => x.elemFaceAreaPerNode); }
        /// <summary>
        /// gets sum of element face areas
        /// </summary>
        public double sumArea { get => areas.Sum(); }


        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts parts { get => SConvertEntity.ToParts(this); }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies bodies { get => SConvertEntity.ToBodies(this); }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces faces { get => SConvertEntity.ToFaces(this); }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges edges { get => SConvertEntity.ToEdges(this); }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts verts { get => SConvertEntity.ToVerts(this); }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes nodes { get => SConvertEntity.ToNodes(this); }
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems elems { get => SConvertEntity.ToElems(this); }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces elemFaces { get => this; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        public SElemFaces tri3s { get => If(e => e.faceShapeType == SFaceType.Tri3); }
        public SElemFaces tri6s { get => If(e => e.faceShapeType == SFaceType.Tri6); }
        public SElemFaces quad4s { get => If(e => e.faceShapeType == SFaceType.Quad4); }
        public SElemFaces quad8s { get => If(e => e.faceShapeType == SFaceType.Quad8); }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElemFaces(SEntityManager em, IEnumerable<SEntity> elemFaces) : base(em, elemFaces.Select(x => (SElemFace)x))
        {
            if (!(elemFaces.FirstOrDefault() is SElemFace)) throw new Exception("SElemFaces(...): SEntity is not SElemFace");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElemFaces(SEntityManager em, IEnumerable<SElemFace> elemFaces) : base(em, elemFaces) { } // em.Msg($"SElemFaces(...): ... elemFaces.Count() : {elemFaces.Count()}");
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElemFaces(SEntityManager em, SElemFaces elemFaces) : base(em, elemFaces.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElemFaces(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one element face by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SElemFace this[int key] { get => entities[key]; }
        /// <summary>
        /// gets the others element faces in the model
        /// </summary>
        public static SElemFaces operator -(SElemFaces a) => a.invert;
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SElemFaces({count} elem-faces)";
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
        /// <returns>new SElemFaces</returns>
        public SElemFaces Extend(Func<SElemFace, SElemFace, bool> func)
        {
            if (isEmpty) return SNew.EmptySElemFaces(em);
            List<SElemFace> r = new List<SElemFace>();
            foreach (SElemFace c in elemFaces) r.AddRange(em.elemFaces.If(x => func(c, x)));
            return elemFaces + r;
        }

        public void DrawElemFacesSpecERP(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, int numberOfColors = 7)
        {
            using (em.api.Graphics.Suspend())
            {
                em.api.Graphics.Scene.Clear();
                if (numberOfColors == null) { throw new ArgumentNullException("Parameter cannot be null", nameof(numberOfColors)); }

                Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();

                List<double> elemFacesResults = elemFaces.Select(x => x.GetElemFaceSpecERP(freq, analysis, resData)).ToList();

                colorsObj.CreateBands(numberOfColors, elemFacesResults.Min(), elemFacesResults.Max());

                foreach (SElemFace ef in elemFaces)
                {
                    double res = ef.GetElemFaceSpecERP(freq, analysis, resData);
                    ef.DrawElemFace(colorsObj.GetColor(res));
                }
                DrawLegend(freq, analysis, colorsObj, "Specific Equivalent Radiated Power", "W/m^2");
                resData.Dispose();
            }
        }
        public void DrawElemFacesSpecERPLevel(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, int numberOfColors = 7)
        {
            using (em.api.Graphics.Suspend())
            {
                em.api.Graphics.Scene.Clear();
                if (numberOfColors == null) { throw new ArgumentNullException("Parameter cannot be null", nameof(numberOfColors)); }

                Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();

                List<double> elemFacesResults = elemFaces.Select(x => x.GetElemFaceSpecERPLevel(freq, analysis, resData)).ToList();

                colorsObj.CreateBands(numberOfColors, elemFacesResults.Min(), elemFacesResults.Max());

                foreach (SElemFace ef in elemFaces)
                {
                    double res = ef.GetElemFaceSpecERPLevel(freq, analysis, resData);
                    ef.DrawElemFace(colorsObj.GetColor(res));
                }
                DrawLegend(freq, analysis, colorsObj, "Specific Equivalent Radiated Power Level", "dB");
                resData.Dispose();
            }
        }
        public void DrawElemFacesNormalV(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, int numberOfColors = 7)
        {
            using (em.api.Graphics.Suspend())
            {
                em.api.Graphics.Scene.Clear();
                if (numberOfColors == null) { throw new ArgumentNullException("Parameter cannot be null", nameof(numberOfColors)); }

                Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();

                List<double> elemFacesResults = elemFaces.Select(x => x.GetElemFaceNormalV(freq, analysis, resData)).ToList();

                colorsObj.CreateBands(numberOfColors, elemFacesResults.Min(), elemFacesResults.Max());

                foreach (SElemFace ef in elemFaces)
                {
                    double res = ef.GetElemFaceNormalV(freq, analysis, resData);
                    ef.DrawElemFace(colorsObj.GetColor(res));
                }
                DrawLegend(freq, analysis, colorsObj, "Normal Velocity", "m/s");
                resData.Dispose();
            }
        }

        public void DrawElemFacesResults(Dictionary<SElemFace, double> dictResults, Ansys.ACT.Automation.Mechanical.Analysis analysis, double freq = 0, int numberOfColors = 7, string type = "Result Type", string unit = "Result Unit")
        {
            using (em.api.Graphics.Suspend())
            {
                em.api.Graphics.Scene.Clear();

                colorsObj.CreateBands(numberOfColors, dictResults.Min(x => x.Value), dictResults.Max(x => x.Value));

                foreach (KeyValuePair<SElemFace, double> keyVal in dictResults)
                {
                    keyVal.Key.DrawElemFace(colorsObj.GetColor(keyVal.Value));
                }
                DrawLegend(freq, analysis, colorsObj, type, unit);
            }
        }

        private void DrawLegend(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis, SColors colorsObj, string type, string unit)
        {
            SAnnoLegend legend = new SAnnoLegend(em, y1: 20);
            List<string> head1 = new List<string> { analysis.Name, "Type: " + type, "Frequency: " + freq.ToString() + " Hz", "Unit: " + unit, "Deformation Scale Factor: 0.0 (Undeformed, not changable)", DateTime.Now.ToString() };

            legend.DrawAnnoLegend(colors: colorsObj.bands, head1: head1);
        }

        public Dictionary<Tuple<int, int>, double> GetDictNormalV(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        {
            Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            Dictionary<Tuple<int, int>, double> dictNormalV = new Dictionary<Tuple<int, int>, double>();

            foreach (SElemFace elemFace in elemFaces)
            {
                dictNormalV[new Tuple<int, int>(elemFace.id, elemFace.elemFaceId)] = elemFace.GetElemFaceNormalV(freq, analysis, resData);
            }
            resData.Dispose();

            return dictNormalV;
        }

        public Dictionary<Tuple<int, int>, double> GetDictSpecERP(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        {
            Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            Dictionary<Tuple<int, int>, double> dictSpecERP = new Dictionary<Tuple<int, int>, double>();
            
            foreach(SElemFace elemFace in elemFaces)
            {
                dictSpecERP[new Tuple<int, int>(elemFace.id, elemFace.elemFaceId)] = elemFace.GetElemFaceSpecERP(freq, analysis, resData);
            }
            //double erpSum = dictERP.Sum(x => x.Value);
            resData.Dispose();

            return dictSpecERP;
        }        
        public Dictionary<Tuple<int, int>, double> GetDictERP(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        {
            Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            Dictionary<Tuple<int, int>, double> dictERP = new Dictionary<Tuple<int, int>, double>();
            
            foreach(SElemFace elemFace in elemFaces)
            {
                dictERP[new Tuple<int, int>(elemFace.id, elemFace.elemFaceId)] = elemFace.GetElemFaceERP(freq, analysis, resData);
            }
            //double erpSum = dictERP.Sum(x => x.Value);
            resData.Dispose();

            return dictERP;
        }

        public Dictionary<Tuple<int, int>, double> GetDictSpecERPLevel(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        {
            Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            Dictionary<Tuple<int, int>, double> dictSpecERPLevel = new Dictionary<Tuple<int, int>, double>();

            foreach (SElemFace elemFace in elemFaces)
            {
                dictSpecERPLevel[new Tuple<int, int>(elemFace.id, elemFace.elemFaceId)] = elemFace.GetElemFaceSpecERPLevel(freq, analysis, resData);
            }
            resData.Dispose();

            return dictSpecERPLevel;
        }
        public Dictionary<Tuple<int, int>, double> GetDictERPLevel(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        {
            Ansys.ACT.Interfaces.Post.IResultReader resData = analysis.GetResultsData();
            Dictionary<Tuple<int, int>, double> dictERPLevel = new Dictionary<Tuple<int, int>, double>();

            foreach (SElemFace elemFace in elemFaces)
            {
                dictERPLevel[new Tuple<int, int>(elemFace.id, elemFace.elemFaceId)] = elemFace.GetElemFaceERPLevel(freq, analysis, resData);
            }
            resData.Dispose();

            return dictERPLevel;
        }


        //public void     DrawElemFacesSpecERPFast(double freq, Ansys.ACT.Automation.Mechanical.Analysis analysis)
        //{
        //    em.api.Graphics.Scene.Clear();
        //    if (numberOfColors == null) { throw new ArgumentNullException("Parameter cannot be null", nameof(numberOfColors)); }


        //    List<double> elemFacesResults = elemFaces.Select(x => x.GetElemFaceSpecERP(freq, analysis)).ToList();
        //    // List<double> elemFacesResults = elemFaces.Select(x=>x.GetElemFaceResult(resultName, analysis)).ToList(); 


        //    colorsObj.CreateBands(numberOfColors, elemFacesResults.Min(), elemFacesResults.Max()); //number of color bude v jako property objektu ve strome, default = 5
        //    List<List<SElemFace>> elemFaceColorGroups = new List<List<SElemFace>>();
        //    foreach (SColors.SColor color in colorsObj.bands)
        //    {
        //        try
        //        {
        //            SElemFaces          efsOneColor         = new SElemFaces(em, elemFaces.Where(x => colorsObj.GetColor(x.GetElemFaceSpecERP(freq, analysis)) == color.color)); // selects element faces with the same color
        //            List<double>        vertices            = this.SelectMany(x => x.vertices).ToList();
        //            IEnumerable<double> globalNormalsIEnum  = this.SelectMany(x => x.globalNormalsIEnum);
        //            //IEnumerable<double> globalNormalsIEnumNeg  = globalNormalsIEnum.Select(x => -x);

        //            List<int>           connectivities      = this.SelectMany(x => x.connectivities).ToList();
        //            efsOneColor.DrawShells(vertices, globalNormalsIEnum, connectivities, color.color);
        //            //efsOneColor.DrawShells(vertices, globalNormalsIEnumNeg, connectivities, color.color);
        //        }
        //        catch (Exception e)
        //        {
                                           
        //        }

        //    }
            


        //    SAnnoLegend legend = new SAnnoLegend(em);
        //    legend.DrawAnnoLegend(colors: colorsObj.bands);
        //}
        private void    DrawShells(List<double> vertices, IEnumerable<double> globalNormalsIEnum, List<int> connectivities, int color)
        {
            IMechanicalExtAPI api = em.api;
            List<IShell3D> shells = new List<IShell3D>();
            IShell3D s = api.Graphics.Scene.Factory3D.CreateShell(vertices, globalNormalsIEnum, connectivities);
            s.Color = color; //colorBands.bands[bandIndex].color;
            //s.Translucency = translucency;
            shells.Add(s);
        }
        //public void DrawElemFaces(Dictionary dict)
        //{
        //    em.api.Graphics.Scene.Clear();
        //    colorsObj.CreateBands(numberOfColors, elemFacesResults.Min(), elemFacesResults.Max()); //number of color bude v jako property objektu ve strome, default = 5
        //    foreach (SElemFace ef in elemFaces)
        //    {
        //        double res = ef.GetElemFaceResult(resultName);
        //        ef.DrawElemFace(colorsObj.GetColor(res));
        //    }
        //    SAnnoLegend legend = new SAnnoLegend(em);
        //    legend.DrawAnnoLegend(colors: colorsObj.bands);
        //}

    }
}
