#pragma warning disable IDE1006                         // Naming Styles

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
    /// Class/instance of SFaces is collection of SFace objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SFaces : SEntities<SFace, SFaces>, IEnumerable
    {
        /// <summary>
        /// gets list of internal (ACT) face objects (IGeoFace)
        /// </summary>
        public List<IGeoFace>          iFaces              { get => Get(x => x.iFace); }
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
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceBSpline type
        /// </summary>
        public SFaces                   bsplines            { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceBSpline);  }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceCone type
        /// </summary>
        public SFaces                   cones               { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceCone);  }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceCylinder type
        /// </summary>
        public SFaces                   cyls                { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceCylinder);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceEllipticalCone type
        /// </summary>
        public SFaces                   ellipCones          { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceEllipticalCone);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceEllipticalCylinder type
        /// </summary>
        public SFaces                   ellipCyls           { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceEllipticalCylinder);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceFaceted type
        /// </summary>
        public SFaces                   facets              { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceFaceted);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfacePlane type
        /// </summary>
        public SFaces                   planes              { get => __Where(GeoSurfaceTypeEnum.GeoSurfacePlane);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceSphere type
        /// </summary>
        public SFaces                   spheres             { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceSphere);   }  
        /// <summary>
        /// filters faces which are GeoSurfaceTypeEnum.GeoSurfaceTorus type
        /// </summary>
        public SFaces                   toruses             { get => __Where(GeoSurfaceTypeEnum.GeoSurfaceTorus);   }   
        private SFaces __Where(GeoSurfaceTypeEnum t) => If((x) => x.surfaceType == t); 
        /// <summary>
        /// gets the other faces int the model
        /// </summary>
        public SFaces                   invert             { get => em.faces - this; }
        /// <summary>
        /// filters faces which are shared across bodies
        /// </summary>
        public SFaces                   shareds           { get => If(f => f.iFace.Bodies.Count >= 2); }
        /// <summary>
        /// filters faces which are shared across bodies
        /// </summary>
        public SFaces                   notShareds        { get => If(f => f.iFace.Bodies.Count == 1); }
        /// <summary>
        /// gets list of areas for the faces
        /// </summary>                                           
        public List<double>             areas             { get => Get(x => x.area); }
        /// <summary>
        /// gets list of normals (in current CS) for the faces
        /// </summary>                           
        public List<SNormal>            normals           { get => Get(x => x.avgNormal); }
        /// <summary>
        /// gets average normal (in current CS) of the faces
        /// </summary>                           
        public SNormal                  avgNormal         { get => SNormal.Avg(normals).Norm(1); } // new SNormal(normals.Average(n => n.x), normals.Average(n => n.y), normals.Average(n => n.z))
        /// <summary>
        /// gets list of global normals for the faces
        /// </summary>                           
        public List<SNormal>            globNormals       { get => Get(x => x.avgGlobalNormal); }
        /// <summary>
        /// gets average global normal of the faces
        /// </summary>                           
        public SNormal                  globNormal        { get => SNormal.Avg(globNormals).Norm(1); }
        /// <summary>
        /// gets list of average polar normal (in current cylindrical CS) of the faces
        /// </summary>                     
        public List<SNormal>            polarNormals      { get => Get(x => x.polarNormal); }
        /// <summary>
        /// gets average polar normal (in current cylindrical CS) of the faces
        /// </summary>    
        public SNormal                  avgPolarNormal      { get => SNormal.Avg(polarNormals).Norm(1); }
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
        public override SFaces          faces             { get => this; }  
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges             { get => SConvertEntity.ToEdges(this); }  
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts             { get => SConvertEntity.ToVerts(this); }  
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes             { get => SConvertEntity.ToNodes(this); } 
        /// <summary>
        /// converts to attached elements
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
        public SFaces(SEntityManager em, IEnumerable<SEntity> faces) : base(em, faces.Select(x => (SFace)x))
        {
            if (!(faces.FirstOrDefault() is SFace)) throw new Exception("SFaces(...): SEntity is not SFace");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SFaces(SEntityManager em, IEnumerable<SFace> faces) : base(em, faces) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SFaces(SEntityManager em, SFaces faces) : base(em, faces.ToList())  { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SFaces(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one face by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SFace this[int key] { get => entities[key]; }      
        /// <summary>
        /// gets the others faces in the model
        /// </summary>
        public static SFaces operator -(SFaces a) => a.invert;
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SFaces({count} faces)";
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
        public SFaces Extend(Func<SFace, SFace, bool> func)
        {
            if (isEmpty) return SNew.EmptySFaces(em);
            List<SFace> r = new List<SFace>();
            foreach (SFace c in faces) r.AddRange(em.faces.If(x => func(c, x)));
            return faces + r; 
        }
    }
}
