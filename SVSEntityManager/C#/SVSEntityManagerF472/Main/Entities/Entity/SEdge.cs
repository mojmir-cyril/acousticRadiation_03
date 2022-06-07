#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System; 
//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Geometry;




namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SEdge is single edge object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SEdge  : SEntity
    { 
        /// <summary>
        /// gets internal (ACT) object of the body (IGeoEdge)
        /// </summary>
        public IGeoEdge                 iEdge          { get => (IGeoEdge)iEntity; }
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity)
        /// </summary>
        public IBaseGeoEntity           iGeoEntity     { get => (IBaseGeoEntity)iEntity; }
        /// <summary>
        /// gets geometry type of the entities => SType.Edge
        /// </summary>
        public override SType           type           { get => SType.Edge; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool            isGeom         { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets length of the edge in current unit (em.lengthUnit)
        /// </summary> 
        public double                   length         { get => em.ConvertLength(iEdge.Length); }
        /// <summary>
        /// gets curve/edge curve type (ACT)
        /// </summary> 
        public GeoCurveTypeEnum         curveType      { get => iEdge.CurveType; }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts          { get => edges.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies         { get => edges.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces          { get => edges.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges          { get => SNew.FromSingle<SEdge, SEdges>(this); }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts          { get => edges.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes          { get => edges.nodes; }
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems          elems          { get => edges.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces      elemFaces      { get => edges.elemFaces; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SEdge(SEntityManager em, IGeoEdge internalEdge) : base(em, internalEdge)
        { 
        }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SEdge(id = '{id}')";
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        // public static SEdges operator +(SEdge a, SEdge b) => new SEdges(a.em, new List<SEdge>() { a, b });
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends edges by a function.  
        /// </summary>
        /// <example>
        /// <code>
        /// em.current.Extend(lambda cur, ver: cur.x == ver.x).Sel()
        /// em.current.Extend(lambda cur, ver: cur.x == ver.x).showns.Sel()
        /// #
        /// # where:
        /// #   cur .. is currently selected object (em.current)
        /// #   any .. is any object in the model
        /// #
        /// </code>
        /// </example>
        /// <param name="func">SEdge, SEdge, bool</param>
        /// <returns>new SEdges</returns>
        public SEdges Extend(Func<SEdge, SEdge, bool> func) => edges.Extend(func);
    }
}
