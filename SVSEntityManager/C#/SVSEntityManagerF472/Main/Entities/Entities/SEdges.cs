#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq; 
using System.Collections;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Geometry; 


namespace SVSEntityManagerF472
{
    /// <summary> 
    /// Class/instance of SEdges is collection of SEdge objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SEdges : SEntities<SEdge, SEdges>, IEnumerable
    { 
        /// <summary>
        /// gets list of internal (ACT) edge objects (IGeoEdge)
        /// </summary>
        public List<IGeoEdge>     iEdges         { get => Get(x => x.iEdge); }
        /// <summary>
        /// gets geometry type of the entities => SType.Edge
        /// </summary>
        public override SType      type           { get => SType.Edge; } 
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool       isGeom         { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveBSpline type
        /// </summary>
        public SEdges              bsplines       { get => __W(GeoCurveTypeEnum.GeoCurveBSpline);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveCircle type
        /// </summary>
        public SEdges              circles        { get => __W(GeoCurveTypeEnum.GeoCurveCircle);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveCircularArc type
        /// </summary>
        public SEdges              circleArcs     { get => __W(GeoCurveTypeEnum.GeoCurveCircularArc);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveEllipseFull type
        /// </summary>
        public SEdges              elliFulls      { get => __W(GeoCurveTypeEnum.GeoCurveEllipseFull);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveEllipticalArc type
        /// </summary>
        public SEdges              elliArcs       { get => __W(GeoCurveTypeEnum.GeoCurveEllipticalArc);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveFaceted type
        /// </summary>
        public SEdges              facets         { get => __W(GeoCurveTypeEnum.GeoCurveFaceted);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveLine type
        /// </summary>
        public SEdges              lines          { get => __W(GeoCurveTypeEnum.GeoCurveLine);  }  
        /// <summary>
        /// filters edges which are GeoSurfaceTypeEnum.GeoCurveLineSegment type
        /// </summary>
        public SEdges              lineSegments   { get => __W(GeoCurveTypeEnum.GeoCurveLineSegment);  } 
        private SEdges __W(GeoCurveTypeEnum t) => If(x => x.curveType == t);
        //
        //  invert:
        //
        /// <summary>
        /// gets the other edges in the model
        /// </summary>
        public SEdges              invert         { get => em.edges - this; }
        /// <summary>
        /// filters edges which are shared across bodies
        /// </summary>
        public SEdges              shareds         { get => If((e) => e.iEdge.Bodies.Count >= 2); }
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
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges             { get => this; }  
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts             { get => SConvertEntity.ToVerts(this); }  
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes             { get => SConvertEntity.ToNodes(this); }  
        /// <summary>
        /// converts to attached elems
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
        public SEdges(SEntityManager em, IEnumerable<SEntity> edges) : base(em, edges.Select(x => (SEdge)x))
        {
            if (!(edges.FirstOrDefault() is SEdge)) throw new Exception("SEdges(...): SEntity is not SEdge");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SEdges(SEntityManager em, IEnumerable<SEdge> edges) : base(em, edges) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SEdges(SEntityManager em, SEdges edges) : base(em, edges.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SEdges(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one edge by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SEdge this[int key] { get => entities[key]; }  
        /// <summary>
        /// gets the others edges in the model
        /// </summary>
        public static SEdges operator -(SEdges a) => a.invert;
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SEdges({count} edges)";
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
        /// em.current.Extend(lambda cur, any: cur.x == any.x).Sel()
        /// em.current.Extend(lambda cur, any: cur.x == any.x).showns.Sel()
        /// #
        /// # where:
        /// #   cur .. is currently selected object (em.current)
        /// #   any .. is any object in the model
        /// #
        /// </code>
        /// </example>
        /// <param name="func">SEdge, SEdge, bool</param>
        /// <returns>new SEdges</returns>
        public SEdges Extend(Func<SEdge, SEdge, bool> func)
        {
            if (isEmpty) return SNew.EmptySEdges(em);
            List<SEdge> r = new List<SEdge>();
            foreach (SEdge c in edges) r.AddRange(em.edges.If(x => func(c, x)));
            return edges + r;  
        }
    }
}
