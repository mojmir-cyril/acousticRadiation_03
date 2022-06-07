#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Geometry; 


namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SVerts is collection of SVert objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SVerts : SEntities<SVert, SVerts>, IEnumerable
    { 
        /// <summary>
        /// gets list of internal (ACT) vertex objects (IGeoVertex)
        /// </summary>
        public List<IGeoVertex> iVerts         { get => entities.Select(x => x.iVert).ToList(); }
        /// <summary>
        /// gets geometry type of the entities => SType.Vert
        /// </summary>
        public override SType    type           { get => SType.Vert; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool     isGeom         { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets the other verticles in the model
        /// </summary>
        public SVerts            invert         { get => em.verts - this; }
        /// <summary>
        /// filters verticles which are shared across bodies
        /// </summary>
        public SVerts            shareds        { get => If((f) => f.iVert.Bodies.Count >= 2); }
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts             { get => SConvertEntity.ToParts(this); }  
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies            { get => SConvertEntity.ToBodies(this); } 
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces             { get => SConvertEntity.ToFaces(this); }  
        /// <summary>
        /// converts to attached faces
        /// </summary> 
        public override SEdges          edges             { get => SConvertEntity.ToEdges(this); }  
        /// <summary>
        /// converts to attached verticles
        /// </summary> 
        public override SVerts          verts             { get => this; }  
        /// <summary>
        /// converts to attached nodes
        /// </summary> 
        public override SNodes          nodes             { get => SConvertEntity.ToNodes(this); } 
        /// <summary>
        /// converts to attached elemens
        /// </summary> 
        public override SElems          elems             { get => SConvertEntity.ToElems(this); }
        /// <summary>
        /// converts to attached element faces
        /// </summary> 
        public override SElemFaces      elemFaces         { get => SConvertEntity.ToElemFaces(this); }

        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// creates new object
        /// </summary> 
        public SVerts(SEntityManager em, IEnumerable<SEntity> verts) : base(em, verts.Select(x => (SVert)x))
        {
            if (!(verts.FirstOrDefault() is SVert)) throw new Exception("SVerts(...): SEntity is not SVert");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SVerts(SEntityManager em, IEnumerable<SVert> verts) : base(em, verts) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SVerts(SEntityManager em, SVerts verts) : base(em, verts.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SVerts(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one vertex by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SVert this[int key] { get => entities[key]; }  
        /// <summary>
        /// gets the others verticles in the model
        /// </summary>
        public static SVerts operator -(SVerts a) => a.invert; 
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SVerts({count} verts)";
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends verts by a function.  
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
        public SVerts Extend(Func<SVert, SVert, bool> func)    
        {
            if (isEmpty) return SNew.EmptySVerts(em);
            List<SVert> r = new List<SVert>();
            foreach (SVert c in verts) r.AddRange(em.verts.If(x => func(c, x)));
            return verts + r; 
        }
    }
}
