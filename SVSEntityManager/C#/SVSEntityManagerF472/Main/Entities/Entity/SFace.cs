#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq; 

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Geometry;  



namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SFace is single face object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s). 
    /// </summary>
    public class SFace : SEntity
    { 
        /// <summary>
        /// gets internal (ACT) object of the face (IGeoFace)
        /// </summary>
        public IGeoFace                 iFace               { get => (IGeoFace)iEntity; }
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity)
        /// </summary>
        public IBaseGeoEntity           iGeoEntity          { get => (IBaseGeoEntity)iEntity; }
        /// <summary>
        /// gets geometry type of the entities => SType.Face
        /// </summary>
        public override SType           type                { get => SType.Face; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool            isGeom              { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets area of face (in areaUnit)
        /// </summary>
        public double                   area                { get => em.ConvertArea(iFace.Area); }   
        /// <summary>
        /// gets type of face (ACT)
        /// </summary>
        public GeoSurfaceTypeEnum       surfaceType         { get => iFace.SurfaceType; }
        /// <summary>
        /// gets list of normals (in current CS) for the face
        /// </summary>                            
        public List<SNormal>            normals             { get => __GetNormals(); }
        /// <summary>
        /// gets list of global normals for the face
        /// </summary>                           
        public List<SNormal>            globalNormals       { get => __GetGlobalNormals(); }
        /// <summary>
        /// gets average normal of the face
        /// </summary>  
        public SNormal                  avgNormal           { get => __GetAvgNormal(); }
        /// <summary>
        /// gets average global normal of the face
        /// </summary>  
        public SNormal                  avgGlobalNormal     { get => __GetGlobAvgNormal(); }
        /// <summary>
        /// gets average polar normal of the face
        /// </summary>                     
        public SNormal                  polarNormal         { get => __GetAvgNormal(polarNormal: true); }
        /// <summary>
        /// gets radius of cylindrical face, otherwise, 0 is returned
        /// </summary>                                    
        public double                   radius              { get => em.ConvertLength(__GetRadius()); }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts               { get => faces.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies              { get => faces.bodies ; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces               { get => SNew.FromSingle<SFace, SFaces>(this); }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges               { get => faces.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts               { get => faces.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes               { get => faces.nodes; }
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems          elems               { get => faces.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces      elemFaces           { get => faces.elemFaces; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SFace(SEntityManager em, IGeoFace internalFace) : base(em, internalFace) { }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SFace(id = '{id}')";

        // -------------------------------------------------------------------------------------------
        //
        //      private:
        //
        // -------------------------------------------------------------------------------------------
        private SRadiusMgr radiusMgr { get => em.radiusMgr; set => em.radiusMgr = value; }
        private double __GetRadius()
        {
            double r = double.NaN;
            try
            {
                if (radiusMgr == null) radiusMgr = new SRadiusMgr(em);
                r = radiusMgr.radiuses[id];
            } catch { }
            return r;
        }
        private List<SNormal> __GetNormals(bool polarNormal = false)
        {
            var norms = __GetGlobalNormals();
            //
            //  local & polar:
            //
            return norms.Select(norm => SNormalUtils.ToLocalPolar(em, norm, polarNormal)).ToList();
        }
        private List<SNormal> __GetGlobalNormals()
        {
            if (iFace.Normals.Count() <= 0) throw new Exception($"__GetNormals(...): Count 0 error: internalFace.Normals.Count() <= 0. ");
            //
            //  def:
            //
            int count = iFace.Normals.Count() / 3;
            List<SNormal> norms = new List<SNormal>();
            //
            //  split:
            //
            for (int i = 0; i < count; i++) norms.Add(new SNormal(iFace.Normals.Skip(i * 3).Take(3).ToList()));
            //   
            //  return:   
            //
            return norms;
        }
        private SNormal __GetGlobAvgNormal()
        {
            List<SNormal> norms = normals.Select((x) => x.Norm(1)).ToList();
            return new SNormal(norms.Average((n) => n.x), norms.Average((n) => n.y), norms.Average((n) => n.z));
        }
        private SNormal __GetAvgNormal(bool polarNormal = false)
        { 
            SNormal nn   = __GetGlobAvgNormal();
            //
            //  local & polar:
            //
            return SNormalUtils.ToLocalPolar(em, nn, polarNormal);
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        // public static SFaces operator +(SFace a, SFace b) => new SFaces(a.em, new List<SFace>() { a, b });
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends faces by a function.  
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
        /// <param name="func">SFace, SFace, bool</param>
        /// <returns>new SFaces</returns>
        public SFaces Extend(Func<SFace, SFace, bool> func) => faces.Extend(func);
    }
}
