#pragma warning disable IDE1006                         // Naming Styles

using Ansys.ACT.Automation.Mechanical;
//
//  Ansys:
//
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Geometry;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;



namespace SVSEntityManagerF472
{
    /// <summary>
    /// The SEntityManager object created by SVS FEM s.o.r. for fast/easy work with geometrical entitites.
    /// The main instance (em) genarally keeps all necessary settings for selecting.
    /// </summary>

    public class SEntityManager : SUnitsBase
    {
        internal const string   LICENCE_FREE   = "free license";
        internal const string   LICENCE_MORPH  = "123456 lincese morph license morph 123456 lincese morph license morph 123456";
        /// <summary>
        /// License key, please, contact SVS FEM s.r.o.
        /// </summary>
        public string           licenseKey          { get; set; }

        /// <summary>
        /// number of CPUs for multi-threading operations
        /// </summary>
        public int               CPUs               { get; set; } = 8;
        internal STree           tree               { get; }
        internal SMath           math               { get; }
        internal SMesh           mesh               { get; }
        internal SSendPY         sendPY             { get; }
        internal SRadiusMgr      radiusMgr          { get; set; }
        /// <summary>
        /// gets SSolution object which keeps solution data from assigned analysis/solution (direct access to RST file),
        /// only one solution can bee assigned to Entity Manager,
        /// ACT result reader is used:
        /// IResultReader reader = analysis.GetResultsData();
        /// </summary>
        /// <example>
        /// <code>
        /// em = EM()  
        /// s = em.solution.Assign("Static Structural") # by analysis name in the Mechanical tree 
        /// </code>
        /// </example>
        internal SSolution       solution           { get; }

        internal SResult         result             { get; }
        /// <summary>
        /// gets object allows morph (node moving)
        /// </summary>
        internal SMorph          morph              { get; }
        /// <summary>
        /// gets object allows draw additional graphics
        /// </summary>
        internal SDraw           draw               { get; } 
        // -------------------------------------------------------------------------------------------
        //
        //      NS & CS:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets all Named Selection (ACT) objects 
        /// </summary>
        public List<NamedSelection> allNSs => tree.nss; 
        /// <summary>
        /// gets all Coordinate System (ACT) objects
        /// </summary>
        public List<CoordinateSystem> allCSs => tree.css; 
        // -------------------------------------------------------------------------------------------
        //
        //      Multi Criteria Analysis:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Multi-criterion analysis -> new criterion for bodies
        /// </summary>
        public SMultiCriteria.SForBody NewBodyCriterion(Func<SBody, double> bodyFunc, double targetValue, double zeroDifference, double topScore) => new SMultiCriteria.SForBody(bodyFunc, targetValue, zeroDifference, topScore);
        /// <summary>
        /// Multi-criterion analysis -> new criterion for faces
        /// </summary>
        public SMultiCriteria.SForFace NewFaceCriterion(Func<SFace, double> faceFunc, double targetValue, double zeroDifference, double topScore) => new SMultiCriteria.SForFace(faceFunc, targetValue, zeroDifference, topScore);
        /// <summary>
        /// Multi-criterion analysis -> new criterion for edges
        /// </summary>
        public SMultiCriteria.SForEdge NewEdgeCriterion(Func<SEdge, double> edgeFunc, double targetValue, double zeroDifference, double topScore) => new SMultiCriteria.SForEdge(edgeFunc, targetValue, zeroDifference, topScore);
        /// <summary>
        /// Multi-criterion analysis -> new criterion for verticles
        /// </summary>
        public SMultiCriteria.SForVert NewVertCriterion(Func<SVert, double> vertFunc, double targetValue, double zeroDifference, double topScore) => new SMultiCriteria.SForVert(vertFunc, targetValue, zeroDifference, topScore);
        // -------------------------------------------------------------------------------------------
        //
        //      current selection:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// current selections
        /// </summary>
        public SEntitiesBase      current        { get => __GetCurrent(); 
                                                   set => Sel(value); } 
        // -------------------------------------------------------------------------------------------
        //
        //      bodies:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// all parts (multi-body parts) in the model
        /// </summary>
        public SParts             parts          { get => ((SParts)SNew.GeomsFromGeoms(this, internalParts)).If(e => e.bodies.count >= 2); }  // new SParts(this, internalParts);
        /// <summary>
        /// all bodies in the model
        /// </summary>
        public SBodies            bodies         { get => (SBodies)SNew.GeomsFromGeoms(this, internalBodies); } 
        /// <summary>
        /// all solid bodies in the model
        /// </summary>
        public SBodies            solids         { get => bodies.solids; } 
        /// <summary>
        /// all surfaces (shell) bodies in the model
        /// </summary>
        public SBodies            surfs          { get => bodies.surfs; } 
        /// <summary>
        /// all line bodies in the model
        /// </summary>
        public SBodies            lines          { get => bodies.lines; }
        /// <summary>
        /// all hidden bodies in the model
        /// </summary>
        public SBodies            hiddens        { get => bodies.hiddens; }
        /// <summary>
        /// all shown (visibled) bodies in the model
        /// </summary>
        public SBodies            showns         { get => bodies.showns; } 
        /// <summary>
        /// all suppressed bodies in the model
        /// </summary>
        public SBodies            suppresseds    { get => bodies.suppresseds; }
        /// <summary>
        /// all active (not-suppressed) bodies in the model
        /// </summary>
        public SBodies            actives        { get => bodies.actives;     }
        //
        //  entities:
        //
        /// <summary>
        /// all active faces in the model
        /// </summary>
        public SFaces             faces          { get => bodies.faces; } 
        /// <summary>
        /// all active edges in the model
        /// </summary>
        public SEdges             edges          { get => bodies.edges; } 
        /// <summary>
        /// all active vertices in the model
        /// </summary>
        public SVerts             verts          { get => bodies.verts; } 
        /// <summary>
        /// all active nodes in the model
        /// </summary>
        public SNodes             nodes          { get => actives.nodes; }         // suppressed nedavaji smysl protoze nemaji sit !!!
        /// <summary>
        /// all active elements in the model
        /// </summary>
        public SElems             elems          { get => actives.elems; }         // suppressed nedavaji smysl protoze nemaji sit !!!
        /// <summary>
        /// all active element faces in the model
        /// </summary>
        public SElemFaces         elemFaces      { get => actives.elemFaces; }     // suppressed nedavaji smysl protoze nemaji sit !!!
        //
        //  internal:
        //
        /// <summary>
        /// all internal (ACT) parts (list of IGeoPart)
        /// </summary>
        public List<IGeoPart>    internalParts  { get => api.DataModel.GeoData.Assemblies[0].Parts.ToList(); }
        /// <summary>
        /// all internal (ACT) parts (list of IGeoBody)
        /// </summary>
        public List<IGeoBody>    internalBodies { get => __GetAllBodies(); }
        // -------------------------------------------------------------------------------------------
        //
        //      GeoEntityById, GetSEntity, GetSEntities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// get internal (ACT) entity (IGeoBody, IGeoFace, ...) by refence id 
        /// </summary>
        public IGeoEntity         GetIEntity(int id)                                                => api.DataModel.GeoData.GeoEntityById(id);
        /// <summary>
        /// get list of internal (ACT) entity (IGeoBody, IGeoFace, ...) by refence ids 
        /// </summary>
        public List<IGeoEntity>   GetIEntity(IEnumerable<int> ids)                                  => ids.Select(GetIEntity).ToList();
        /// <summary>
        /// get EM entity (SBody, SFace, ...) by refence id
        /// </summary>
        public SEntity            Entity(int id)                                                    => SNew.SEntity(this, id); 
        /// <summary>
        /// get EM entities (SBodies, SFaces, ...) by refence ids 
        /// </summary>
        public SEntitiesBase      Entities(IEnumerable<int> ids)                                    => SNew.GeomsFromIds(this, ids); 
        /// <summary>
        /// get EM node (SNode) by node id 
        /// </summary>
        public SNode              Node(int id)                                                      => SNew.NodesFromIds(this, new int[] { id })[0];
        /// <summary>
        /// get EM nodes (SNodes) by node ids
        /// </summary>
        public SNodes             Nodes(IEnumerable<int> ids)                                       => SNew.NodesFromIds(this, ids);
        /// <summary>
        /// get EM element (SElem) by node id
        /// </summary>
        public SElem              Elem(int id)                                                      => SNew.ElemsFromIds(this, new int[] { id })[0];
        /// <summary>
        /// get EM elements (SElems) by node ids
        /// </summary>
        public SElems             Elems(IEnumerable<int> ids)                                       => SNew.ElemsFromIds(this, ids);
        /// <summary>
        /// get EM element face (SElemFace) by element id and face id
        /// </summary>
        public SElemFace          ElemFace(int elemId, int faceId)                                  => SNew.ElemFacesFromIds(this, elemId, faceId)[0];
        /// <summary>
        /// get EM element faces (ElemFaces) by element ids and face ids
        /// </summary>
        public SElemFaces         ElemFaces(IEnumerable<int> elemIds, IEnumerable<int> faceIds)     => SNew.ElemFacesFromIds(this, elemIds, faceIds);
        // -------------------------------------------------------------------------------------------
        //
        //      CS:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// currently assingned coordinate system used for selecting by location
        /// </summary>
        public CoordinateSystem   coordinateSystem  { get;  set; }
        // -------------------------------------------------------------------------------------------
        //
        //      dicts:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// mapping EM entites to ACT entities (SBody -> GeoCellTypeEnum.GeoBody, ...)
        /// </summary>
        public static Dictionary<Type, GeoCellTypeEnum>   typeEnumDict   { get => new Dictionary<Type, GeoCellTypeEnum>() { 
                                                                                      { typeof(SPart) , GeoCellTypeEnum.GeoPart   }, 
                                                                                      { typeof(SBody) , GeoCellTypeEnum.GeoBody   }, 
                                                                                      { typeof(SFace) , GeoCellTypeEnum.GeoFace   }, 
                                                                                      { typeof(SEdge) , GeoCellTypeEnum.GeoEdge   }, 
                                                                                      { typeof(SVert) , GeoCellTypeEnum.GeoVertex } }; }    // type --> geom enum
        /// <summary>
        /// saved (generated) element faces for fast reusing
        /// </summary>
        public Dictionary<(int, int), SElemFace>   usedElemFaces  { get; }    // saves elem faces for reusing: usedElemFaces[elemId, faceId] ---> SElemFace

        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// constructor of EM (main object keeps all necessary settings)
        /// </summary>
        public SEntityManager(IMechanicalExtAPI api, 
                              string lengthUnit  = "mm", 
                              string angleUnit   = "deg", 
                              string massUnit    = "kg", 
                              string licenseKey  = LICENCE_FREE, 
                              bool loggingToACT  = false,
                              bool loggingToText = false,
                              bool loggingToHtml = false) : base(api, lengthUnit, angleUnit, massUnit)
        {
            em = this;
            //
            //  objects:
            //
            unitUtils = new SUnitsUtils(this); 
            tree      = new STree(this);
            sendPY    = new SSendPY(api, logger);
            mesh      = new SMesh(api, logger, "Global");
            math      = new SMath(unitUtils, tree, logger, CPUs);
            solution  = new SSolution(this);
            result    = new SResult(this);
            morph     = new SMorph(this);
            draw      = new SDraw(this);
            //
            //  cs:
            //
            coordinateSystem = tree.globalCS; 
            //
            //  saves elem faces for reusing: usedElemFaces[elemId, faceId] ---> SElemFace:
            //
            usedElemFaces = new Dictionary<(int, int), SElemFace>();
            //
            //  license key:
            //
            this.licenseKey = licenseKey;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      InvertSuppressed, InvertVisiblity:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// inverts suppressed set of bodies
        /// </summary>
        public SEntityManager InvertSuppressed()
        {
            SBodies a = actives;
            suppresseds.Unsuppress();
            a.Suppress();
            return this;
        }
        /// <summary>
        /// inverts visibled set of bodies (suppressed bodies are ignored)
        /// </summary>
        public SEntityManager InvertVisiblity()
        {
            SBodies h = hiddens;
            showns.Hide();
            h.Show();
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      CS:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// sets coordinate system by tree node id
        /// </summary>
        public SEntityManager CS(int id)
        {
            DataModelObject o = tree.Obj(id);
            if (!o.GetType().ToString().EndsWith(".CoordinateSystem")) throw new Exception($"SEntityManager.CS(int id): System with id = {id} cannot be found. ");
            coordinateSystem = (CoordinateSystem)o; 
            return this;
        }
        /// <summary>
        /// sets coordinate system by tree node name
        /// </summary>
        public SEntityManager CS(string name)
        {
            List<CoordinateSystem> css = tree.css.Where((cs) => cs.Name == name).ToList();
            if (css.Count <= 0) throw new Exception($"SEntityManager.CS(string name): No Coordinate System found with name '{name}'. ");
            if (css.Count >= 2) throw new Exception($"SEntityManager.CS(string name): More than one Coordinate System found with name '{name}'. ");
            coordinateSystem = css[0]; 
            return this;
        }
        /// <summary>
        /// sets coordinate system by CoordinateSystem object
        /// </summary>
        public SEntityManager CS(CoordinateSystem cs)
        {
            coordinateSystem = cs; 
            return this;
        }
        /// <summary>
        /// sets global coordinate system
        /// </summary>
        public SEntityManager CS()
        {
            coordinateSystem = tree.globalCS; 
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      NS:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets EM (SBodies, SFaces, ...) selection from NamedSelection object given by id
        /// </summary>
        public SEntitiesBase  NS(int id)            => NS((NamedSelection)tree.Obj(id));
        /// <summary>
        /// gets EM (SBodies, SFaces, ...) selection from NamedSelection object  
        /// </summary>
        public SEntitiesBase  NS(NamedSelection ns) => FromInfo(ns.Location);
        /// <summary>
        /// gets EM (SBodies, SFaces, ...) selection from NamedSelection object given by name
        /// </summary>
        public SEntitiesBase  NS(string name)
        {
            List<NamedSelection> nss = tree.nss.Where((ns) => ns.Name == name).ToList();
            if (nss.Count <= 0) throw new Exception($"SEntityManager.NS(...): No Named Selection found with name '{name}'. ");
            if (nss.Count >= 2) throw new Exception($"SEntityManager.NS(...): More than one Named Selection found with name '{name}'. ");
            return NS(nss.First());
        }
        /// <summary>
        /// adds (creates) new NamedSelection object
        /// </summary>
        public NamedSelection AddNS(string name, SEntitiesBase eb, bool andActivate = true)
        {
            NamedSelection ns = tree.dataModel.Project.Model.AddNamedSelection();
            ns.Name           = name;
            ns.Location       = eb.info;
            if (andActivate) ns.Activate();
            return ns;
        }
        /// <summary>
        /// fills selection to NamedSelection object by EM entities (SBodies, SFaces, ...)
        /// </summary>
        public NamedSelection SetNS(string name, SEntitiesBase eb, bool andActivate = true)
        {
            List<NamedSelection> nss = tree.nss.Where((ns) => ns.Name == name).ToList();
            if      (nss.Count <= 0) return AddNS(name, eb, andActivate);
            else if (nss.Count == 1) 
            {
                NamedSelection ns = nss.First();
                ns.Location = eb.info;
                return ns;
            }
            else if (nss.Count >= 2) throw new Exception($"SEntityManager.NS(...): More than one Named Selection found with name '{name}'. ");
            throw new Exception($"SEntityManager.NS(...): Picovina!!!!. ");
        }
        // -------------------------------------------------------------------------------------------
        //
        //      FromInfo:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets EM entities (SBodies, SFaces, ...) from existing ACT selection info
        /// </summary>
        public SEntitiesBase FromInfo(ISelectionInfo info) => SNew.FromInfo(this, new SInfo(info));
        /// <summary>
        /// gets EM entities (SBodies, SFaces, ...) from existing EM SInfo object
        /// </summary>
        public SEntitiesBase FromInfo(SInfo info)          => SNew.FromInfo(this, info);
        // -------------------------------------------------------------------------------------------
        //
        //      __GetCurrent:
        //
        // -------------------------------------------------------------------------------------------
        private SEntitiesBase __GetCurrent()
        { 
            ISelectionInfo i = api?.SelectionManager?.CurrentSelection;
            if (i == null) throw new Exception($"SEntityManager.__GetCurrent(...): A problem get current selection via 'api.SelectionManager.CurrentSelection'! ");
            if (i.Ids.Count <= 0) return null;
            return FromInfo(i);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Add:
        //
        // -------------------------------------------------------------------------------------------
        private SEntitiesBase __Add(SInfo info)
        {
            api.SelectionManager.AddSelection(info);
            return SNew.FromInfo(this, info);
        }
        /// <summary>
        /// extends current selection in the model with entities given by EM SInfo object
        /// </summary>
        public SEntitiesBase Add(SInfo info)                        => __Add(info);
        /// <summary>
        /// extends current selection in the model with entities given by EM entity (SBody, SFace, ...)
        /// </summary>
        public SEntitiesBase Add(SEntity ent)                       => Add(ent.info);
        /// <summary>
        /// extends current selection in the model with entities given by EM entities (SBodies, SFaces, ...)
        /// </summary>
        public SEntitiesBase Add(SEntitiesBase ents)                => Add(ents.info);
        /// <summary>
        /// extends current selection in the model with entity given by ACT entity 
        /// </summary>
        public SEntitiesBase Add(IBaseGeoEntity e)                  => Add(SNew.GeomsFromGeom(this, e)); 
        /// <summary>
        /// extends current selection in the model with entities given by list of ACT entities 
        /// </summary>
        public SEntitiesBase Add(IEnumerable<IBaseGeoEntity> es)    => Add(SNew.GeomsFromGeoms(this, es));
        /// <summary>
        /// extends current selection in the model with entity given by reference id
        /// </summary>
        public SEntitiesBase AddEnt(int refId)                      => Add(SInfo.NewGeomInfo(refId));  
        /// <summary>
        /// extends current selection in the model with entities given by list of reference ids
        /// </summary>
        public SEntitiesBase AddEnts(IEnumerable<int> refIds)       => Add(SInfo.NewGeomInfo(refIds)); 
        /// <summary>
        /// extends current selection in the model with node given by list of ids
        /// </summary>
        public SEntitiesBase AddNode(int id)                        => Add(SInfo.NewNodeInfo(id));  
        /// <summary>
        /// extends current selection in the model with nodes given by EM nodes (SNodes)
        /// </summary>
        public SEntitiesBase AddNodes(SNodes nodes)                 => Add(nodes.info); 
        /// <summary>
        /// extends current selection in the model with nodes given by list of ids
        /// </summary>
        public SEntitiesBase AddNodes(IEnumerable<int> id)          => Add(SInfo.NewNodeInfo(id)); 
        /// <summary>
        /// extends current selection in the model with element given by id
        /// </summary>
        public SEntitiesBase AddElem(int id)                        => Add(SInfo.NewElemInfo(id));
        /// <summary>
        /// extends current selection in the model with elements given by EM elements (SElems)
        /// </summary>
        public SEntitiesBase AddElems(SElems elems)                 => Add(elems.info);
        /// <summary>
        /// extends current selection in the model with elements given by list of ids
        /// </summary>
        public SEntitiesBase AddElems(IEnumerable<int> ids)         => Add(SInfo.NewElemInfo(ids)); 
        /// <summary>
        /// extends current selection in the model with element face given by element id and face id
        /// </summary>
        public SEntitiesBase AddFaceElem(int id, int faceId)        => Add(SInfo.NewElemFaceInfo(id, faceId));  
        /// <summary>
        /// extends current selection in the model with elements faces given by ACT element faces (SElemFaces)
        /// </summary>
        public SEntitiesBase AddFaceElems(SElemFaces elemFaces)     => Add(elemFaces.info);  
        /// <summary>
        /// extends current selection in the model with element faces given by list of element ids and list of face ids
        /// </summary>
        public SEntitiesBase AddFaceElems(IEnumerable<int> ids, 
                                          IEnumerable<int> faceIds) => Add(SInfo.NewElemFaceInfo(ids, faceIds));  
        // -------------------------------------------------------------------------------------------
        //
        //      Sel:
        //
        // -------------------------------------------------------------------------------------------
        private SEntitiesBase __Sel(SInfo info)
        {
            api.SelectionManager.NewSelection(info);
            return SNew.FromInfo(this, info); 
        }
        /// <summary>
        /// creates new selection in the model with entities given by EM SInfo object
        /// </summary>
        public SEntitiesBase Sel(SInfo info)                        => __Sel(info); 
        /// <summary>
        /// creates new selection in the model with entities given by EM entity (SBody, SFace, ...)
        /// </summary>
        public SEntitiesBase Sel(SEntity ent)                       => Sel(ent.info);
        /// <summary>
        /// creates new selection in the model with entities given by EM entities (SBodies, SFaces, ...)
        /// </summary>
        public SEntitiesBase Sel(SEntitiesBase ents)                => Sel(ents.info); 
        /// <summary>
        /// creates new selection in the model with entity given by ACT entity 
        /// </summary>
        public SEntitiesBase Sel(IBaseGeoEntity e)                  => Sel(SNew.GeomsFromGeom(this, e)); 
        /// <summary>
        /// creates newt selection in the model with entities given by list of ACT entities 
        /// </summary>
        public SEntitiesBase Sel(IEnumerable<IBaseGeoEntity> es)    => Sel(SNew.GeomsFromGeoms(this, es)); 
        /// <summary>
        /// creates new selection in the model with entity given by reference id
        /// </summary>
        public SEntitiesBase SelEnt(int refId)                      => Sel(SInfo.NewGeomInfo(refId));  
        /// <summary>
        /// creates new selection in the model with entities given by list of reference ids
        /// </summary>
        public SEntitiesBase SelEnts(IEnumerable<int> refIds)       => Sel(SInfo.NewGeomInfo(refIds)); 
        /// <summary>
        /// creates new selection in the model with node given by list of ids
        /// </summary>
        public SEntitiesBase SelNode(int id)                        => Sel(SInfo.NewNodeInfo(id));
        /// <summary>
        /// creates new selection in the model with nodes given by EM nodes (SNodes)
        /// </summary>
        public SEntitiesBase SelNodes(SNodes nodes)                 => Sel(nodes.info);
        /// <summary>
        /// creates new selection in the model with nodes given by list of ids
        /// </summary>
        public SEntitiesBase SelNodes(IEnumerable<int> id)          => Sel(SInfo.NewNodeInfo(id)); 
        /// <summary>
        /// creates new selection in the model with element given by id
        /// </summary>
        public SEntitiesBase SelElem(int id)                        => Sel(SInfo.NewElemInfo(id));  
        /// <summary>
        /// creates new selection in the model with elements given by EM elements (SElems)
        /// </summary>
        public SEntitiesBase SelElems(SElems elems)                 => Sel(elems.info);
        /// <summary>
        /// creates new  selection in the model with elements given by list of ids
        /// </summary>
        public SEntitiesBase SelElems(IEnumerable<int> ids)         => Sel(SInfo.NewElemInfo(ids)); 
        /// <summary>
        /// creates new selection in the model with element face given by element id and face id
        /// </summary>
        public SEntitiesBase SelFaceElem(int id, int faceId)        => Sel(SInfo.NewElemFaceInfo(id, faceId));  
        /// <summary>
        /// creates new selection in the model with elements faces given by ACT element faces (SElemFaces)
        /// </summary>
        public SEntitiesBase SelFaceElems(SElemFaces elemFaces)     => Sel(elemFaces.info);  
        /// <summary>
        /// creates new selection in the model with element faces given by list of element ids and list of face ids
        /// </summary>
        public SEntitiesBase SelFaceElems(IEnumerable<int> ids, 
                                          IEnumerable<int> faceIds) => Sel(SInfo.NewElemFaceInfo(ids, faceIds));   
        // -------------------------------------------------------------------------------------------
        //
        //      Clear:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// clears current selection in the model
        /// </summary>
        public void Clear() => api.SelectionManager.ClearSelection();
        // -------------------------------------------------------------------------------------------
        //
        //      Clear:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// clears total all additional graphics
        /// </summary>
        public void ClearGraphics() => draw.Clear();
        // -------------------------------------------------------------------------------------------
        //
        //      private:
        //
        // ------------------------------------------------------------------------------------------- 
        private List<IGeoBody> __GetAllBodies()
        {
            List<IBaseGeoBody> ret = new List<IBaseGeoBody>();
            foreach (IGeoPart part in api.DataModel.GeoData.Assemblies[0].Parts) ret.AddRange(part.Bodies);
            return ret.Select((x) => (IGeoBody)x).ToList();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Normal:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets empty SNormal object 
        /// </summary>
        public SNormal Normal()                             => new SNormal();
        /// <summary>
        /// gets SNormal object from coordinates
        /// </summary>
        public SNormal Normal(double x, double y, double z) => new SNormal(x, y, z);
        /// <summary>
        /// duplicates SNormal object
        /// </summary>
        public SNormal Normal(SNormal n)                    => new SNormal(n);
        /// <summary>
        /// gets SNormal object from two points
        /// </summary>
        public SNormal Normal(SPoint p1, SPoint p2)         => new SNormal(p1, p2);
        /// <summary>
        /// gets SNormal object from two ACT nodes (INode)
        /// </summary>
        public SNormal Normal(INode n1, INode n2)           => new SNormal(n1, n2);
        /// <summary>
        /// gets SNormal object from list of coordinates (x, y, z)
        /// </summary>
        public SNormal Normal(IList<double> l)              => new SNormal(l);
        /// <summary>
        /// gets SNormal object from array of coordinates (x, y, z)
        /// </summary>
        public SNormal Normal(double[] l)                   => new SNormal(l);
        /// <summary>
        /// gets SNormal object from two ACT verticles (IGeoVertex)
        /// </summary>
        public SNormal Normal(IGeoVertex v1, IGeoVertex v2) => new SNormal(v1, v2); 
        // -------------------------------------------------------------------------------------------
        //
        //      Help:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// shows dialog with help and examples for SEntityManager object
        /// </summary>
        /// <example>
        /// <code>
        /// em = SVSEntityManagerF472.SEntityManager(ExtAPI)
        /// em.Help()
        /// </code>
        /// </example> 
        public void Help() => new Help.SFormHelp(this);
        // -------------------------------------------------------------------------------------------
        //
        //      Redraw:
        //
        // ------------------------------------------------------------------------------------------- 

        /// <summary>
        /// redraws the Mechanical graphics.
        /// </summary>
        /// <example>
        /// <code>
        /// em.Redraw()
        /// </code>
        /// </example> 
        /// <param name="byDS">true ... more powerfull</param>
        /// <returns>SEntityManager ---> self</returns>
        public SEntityManager Redraw(bool byDS = true)
        {
            logger.Msg($"Redraw(...)");
            if (byDS)
            {
                try
                {
                    // em.api.Graphics.InternalObject.Redraw(1);  // funguje pouze pro aktualni verzi ansys
                    // Could not load file or assembly 'Ansys.Common.Interop.202, Version=0.0.0.0, Culture=neutral, PublicKeyToken=fe7dbaff69e8d999' or one of its dependencies.The system cannot find the file specified.:line 2
                    // dynamic g = api.Graphics;
                    // g.InternalObject.Redraw(1);
                    // string cmd = $"DS.Graphics.Redraw(1);";
                    string cmd = $"DS.Graphics.Redraw(1);"; // string cmd = $"DS.Graphics.Redraw(0);";
                    sendPY.SendJS(cmd); // new SSendPY(api).SendJS(cmd);
                }
                catch { logger.Msg($"Redraw(...): problem to use em.api.Graphics.InternalObject.Redraw(1)!"); }
            }
            else api.Graphics.Redraw();
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      GetVersion:
        //
        // -------------------------------------------------------------------------------------------
        
        /// <summary>
        /// gets version SVSEntityManager module
        /// </summary>
        public string GetVersion() => GetVersionString(); 
        /// <summary>
        /// gets version SVSEntityManager module (same as GetVersion())
        /// </summary>
        public static string GetVersionString()
        {
            try
            { 
                var x = Assembly.GetAssembly(typeof(SEntityManager)); 
                return $"{x.GetName().Version}";
                // var x = Assembly.GetAssembly(typeof(SEntityManager)).Location; 
                // return $"{FileVersionInfo.GetVersionInfo(x)}";
                // return $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location)}";
                // return $"{Assembly.GetEntryAssembly().GetName().Version}";
                // Version a = new AssemblyName().Version;
                // return $"{a.Major}.{a.Minor}";
            }
            catch { return "N/A"; }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      CheckHomo:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// checks if both types are same
        /// </summary>
        /// <example>
        /// <code>
        /// a = em.current
        /// b = em.Entities([1,2,3])
        /// SEntityManager.CheckHomo(a, b, "MyFunction")
        /// #  True  ---> if same type (SBody and SBody, SBodies and SBody)
        /// #  False ---> if different type (SBodies and SFaces)
        /// </code>
        /// </example> 
        /// <returns>true if type is same</returns>
        public static bool CheckHomo(SEntitiesBase a, SEntitiesBase b, string funName)  
            => a.type == b.type ? true : throw new Exception($"SEntityManager.{funName}.CheckHomo(...): non-homogenic result is not supported. ");
        /// <summary>
        /// checks if both types are same
        /// </summary>
        /// <example>
        /// <code>
        /// a = em.current
        /// b = em.Entities([1,2,3])
        /// SEntityManager.CheckHomo(a, b, "MyFunction")
        /// #  True  ---> if same type (SBody and SBody, SBodies and SBody)
        /// #  False ---> if different type (SBodies and SFaces)
        /// </code>
        /// </example> 
        /// <returns>true if type is same</returns>
        public static bool CheckHomo<TEnt1, TEnt2>(ISEntities<TEnt1> a, ISEntities<TEnt2> b, string funName) 
            where TEnt1 : SEntity 
            where TEnt2 : SEntity
            => a.type == b.type ? true : throw new Exception($"SEntityManager.{funName}.CheckHomo(...): non-homogenic result is not supported. ");
        // -------------------------------------------------------------------------------------------
        //
        //      Union & Substract & Intersect:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// gets union two entity sets (exception is invoked if same type is not used)
        /// </summary>
        public static SEntitiesBase Union(SEntitiesBase a, SEntitiesBase b, string functionName = "N/A")
        {
            CheckHomo(a, b, functionName); 
            return a.type == SType.Part      ? (SEntitiesBase)(a.parts     + b.parts    ) :
                   a.type == SType.Body      ? (SEntitiesBase)(a.bodies    + b.bodies   ) :
                   a.type == SType.Face      ? (SEntitiesBase)(a.faces     + b.faces    ) :
                   a.type == SType.Edge      ? (SEntitiesBase)(a.edges     + b.edges    ) :
                   a.type == SType.Vert      ? (SEntitiesBase)(a.verts     + b.verts    ) :
                   a.type == SType.Elem      ? (SEntitiesBase)(a.elems     + b.elems    ) :
                   a.type == SType.Node      ? (SEntitiesBase)(a.nodes     + b.nodes    ) :
                   a.type == SType.ElemFace  ? (SEntitiesBase)(a.elemFaces + b.elemFaces) : 
                   throw ToDo($"a.type == {a.type}", nameof(SEntityManager), functionName == "N/A" ? functionName : nameof(Union));  
        }        
        /// <summary>
        /// gets substact two entity sets (exception is invoked if same type is not used)
        /// </summary>
        public static SEntitiesBase Substract(SEntitiesBase a, SEntitiesBase b, string functionName = "N/A")
        {
            CheckHomo(a, b, functionName); 
            return a.type == SType.Part      ? (SEntitiesBase)(a.parts     - b.parts    ) :
                   a.type == SType.Body      ? (SEntitiesBase)(a.bodies    - b.bodies   ) :
                   a.type == SType.Face      ? (SEntitiesBase)(a.faces     - b.faces    ) :
                   a.type == SType.Edge      ? (SEntitiesBase)(a.edges     - b.edges    ) :
                   a.type == SType.Vert      ? (SEntitiesBase)(a.verts     - b.verts    ) :
                   a.type == SType.Elem      ? (SEntitiesBase)(a.elems     - b.elems    ) :
                   a.type == SType.Node      ? (SEntitiesBase)(a.nodes     - b.nodes    ) :
                   a.type == SType.ElemFace  ? (SEntitiesBase)(a.elemFaces - b.elemFaces) : 
                   throw ToDo($"a.type == {a.type}", nameof(SEntityManager), functionName == "N/A" ? functionName : nameof(Substract));  
        }
        /// <summary>
        /// gets intersect two entity sets (exception is invoked if same type is not used)
        /// </summary>
        public static SEntitiesBase Intersect(SEntitiesBase a, SEntitiesBase b, string functionName = "N/A")
        {
            CheckHomo(a, b, functionName); 
            return a.type == SType.Part      ? (SEntitiesBase)(a.parts     * b.parts    ) :
                   a.type == SType.Body      ? (SEntitiesBase)(a.bodies    * b.bodies   ) :
                   a.type == SType.Face      ? (SEntitiesBase)(a.faces     * b.faces    ) :
                   a.type == SType.Edge      ? (SEntitiesBase)(a.edges     * b.edges    ) :
                   a.type == SType.Vert      ? (SEntitiesBase)(a.verts     * b.verts    ) :
                   a.type == SType.Elem      ? (SEntitiesBase)(a.elems     * b.elems    ) :
                   a.type == SType.Node      ? (SEntitiesBase)(a.nodes     * b.nodes    ) :
                   a.type == SType.ElemFace  ? (SEntitiesBase)(a.elemFaces * b.elemFaces) : 
                   throw ToDo($"a.type == {a.type}", nameof(SEntityManager), functionName == "N/A" ? functionName : nameof(Intersect));  
        }
    }
}
 