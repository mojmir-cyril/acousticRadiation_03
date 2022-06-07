#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Geometry; 


namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SVert is single vertex object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s). 
    /// </summary>
    public class SVert : SEntity
    {
        /// <summary>
        /// gets internal (ACT) object of the vertex (IGeoVertex)
        /// </summary>
        public IGeoVertex               iVert           { get => (IGeoVertex)iEntity; }
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity)
        /// </summary>
        public IBaseGeoEntity           iGeoEntity      { get => (IBaseGeoEntity)iEntity; }
        /// <summary>
        /// gets geometry type of the entities => SType.Vert
        /// </summary>
        public override SType           type            { get => SType.Vert; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool            isGeom          { get => true; } 
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts          { get => verts.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies         { get => verts.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces          { get => verts.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges          { get => verts.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts          { get => SNew.FromSingle<SVert, SVerts>(this); }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes          { get => verts.nodes; } 
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems          elems          { get => verts.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces      elemFaces      { get => verts.elemFaces; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SVert(SEntityManager em, IGeoVertex internalVertex) : base(em, internalVertex) {  }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SVert(id = '{id}')"; 
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends verticles by a function.  
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
        /// <param name="func">SVert, SVert, bool</param>
        /// <returns>new SVerts</returns>
        public SVerts Extend(Func<SVert, SVert, bool> func) => verts.Extend(func);
    }
}
