#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq; 
using System.Collections;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mesh; 


namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SElems is collection of SElem objects.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SElems : SEntities<SElem, SElems>, IEnumerable
    { 
        /// <summary>
        /// gets list of internal (ACT) element objects (IElement)
        /// </summary>
        public List<IElement>                      iElems            { get => entities.Select(x => x.iElem).ToList(); }  // Ansys.ACT.Common.Mesh.NodeWrapper
        /// <summary>
        /// gets geometry type of the entities => SType.Elem
        /// </summary>
        public override SType                       type              { get => SType.Elem; }
        /// <summary>
        /// gets true if element type of the entities => true
        /// </summary>
        public override bool                        isElem            { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets the other elements int the model
        /// </summary>
        public SElems                               invert            { get => em.elems - this; }
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.elems.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        public override SInfo                       info              { get => SInfo.NewElemInfo(ids); }
        // -------------------------------------------------------------------------------------------
        //
        //      shape:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// filters elements which are any solid shape type
        /// </summary> 
        public SElems                               solids          { get => If(e => e.isHex || e.isTet || e.isWedge || e.isPyramid); }
        /// <summary>
        /// filters elements which are any shell shape type
        /// </summary> 
        public SElems                               shells          { get => If(e => e.isTri || e.isQuad); }
        /// <summary>
        /// filters elements which are any line shape type
        /// </summary>

        public SElems                               lines            { get => If(e => e.isLine); } 
        /// <summary>
        /// filters elements which are any beam shape type
        /// </summary>

        public SElems                               beams            { get => If(e => e.isBeam); } 
        /// <summary>
        /// filters elements which are any tri shape type
        /// </summary>

        public SElems                               tris             { get => If(e => e.isTri); } 
        /// <summary>
        /// filters elements which are any quad shape type
        /// </summary>

        public SElems                               quads            { get => If(e => e.isQuad); } 
        /// <summary>
        /// filters elements which are any tetra shape type
        /// </summary>

        public SElems                               tets             { get => If(e => e.isTet); }
        /// <summary>
        /// filters elements which are any hexa shape type
        /// </summary>
        public SElems                               hexs             { get => If(e => e.isHex); }
        /// <summary>
        /// filters elements which are any wedge shape type
        /// </summary>
        public SElems                               wedges           { get => If(e => e.isWedge); }  
        /// <summary>
        /// filters elements which are any pyramid shape type
        /// </summary>
        public SElems                               pyramids         { get => If(e => e.isPyramid); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kPoint0 shape type
        /// </summary>
        public SElems                               point0s          { get => If(e => e.elemType == ElementTypeEnum.kPoint0); }  
        /// <summary>
        /// filters elements which are ElementTypeEnum.kLine2 shape type
        /// </summary>
        public SElems                               line2s           { get => If(e => e.elemType == ElementTypeEnum.kLine2); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kLine3 shape type
        /// </summary>
        public SElems                               line3s           { get => If(e => e.elemType == ElementTypeEnum.kLine3); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kBeam3 shape type
        /// </summary>
        public SElems                               beam3s           { get => If(e => e.elemType == ElementTypeEnum.kBeam3); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kBeam4 shape type
        /// </summary>
        public SElems                               beam4s           { get => If(e => e.elemType == ElementTypeEnum.kBeam4); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kTri3 shape type
        /// </summary>
        public SElems                               tri3s            { get => If(e => e.elemType == ElementTypeEnum.kTri3); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kTri6 shape type
        /// </summary>
        public SElems                               tri6s            { get => If(e => e.elemType == ElementTypeEnum.kTri6); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kQuad4 shape type
        /// </summary>
        public SElems                               quad4s           { get => If(e => e.elemType == ElementTypeEnum.kQuad4); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kQuad8 shape type
        /// </summary>
        public SElems                               quad8s           { get => If(e => e.elemType == ElementTypeEnum.kQuad8); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kTet4 shape type
        /// </summary>
        public SElems                               tet4s            { get => If(e => e.elemType == ElementTypeEnum.kTet4); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kTet10 shape type
        /// </summary>
        public SElems                               tet10s           { get => If(e => e.elemType == ElementTypeEnum.kTet10); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kHex8 shape type
        /// </summary>
        public SElems                               hex8s            { get => If(e => e.elemType == ElementTypeEnum.kHex8); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kHex20 shape type
        /// </summary>
        public SElems                               hex20s           { get => If(e => e.elemType == ElementTypeEnum.kHex20); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kWedge6 shape type
        /// </summary>
        public SElems                               wedge6s          { get => If(e => e.elemType == ElementTypeEnum.kWedge6); }  
        /// <summary>
        /// filters elements which are ElementTypeEnum.kWedge15 shape type
        /// </summary>
        public SElems                               wedge15s         { get => If(e => e.elemType == ElementTypeEnum.kWedge15); }
        /// <summary>
        /// filters elements which are ElementTypeEnum.kPyramid5 shape type
        /// </summary>
        public SElems                               pyramid5s        { get => If(e => e.elemType == ElementTypeEnum.kPyramid5); } 
        /// <summary>
        /// filters elements which are ElementTypeEnum.kPyramid13 shape type
        /// </summary>
        public SElems                               pyramid13s       { get => If(e => e.elemType == ElementTypeEnum.kPyramid13); }
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes                     corners           { get => SConvertEntity.ToCorners(this); }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes                     mids              { get => SConvertEntity.ToMids(this); }
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts                      parts            { get => SConvertEntity.ToParts(this); }  
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies                     bodies           { get => SConvertEntity.ToBodies(this); } 
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces                      faces            { get => SConvertEntity.ToFaces(this); }  
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges                      edges            { get => SConvertEntity.ToEdges(this); }  
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts                      verts            { get => SConvertEntity.ToVerts(this); }  
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes                      nodes            { get => SConvertEntity.ToNodes(this); } 
        /// <summary>
        /// converts to attached elements
        /// </summary>
        public override SElems                      elems            { get => this; }  
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces                  elemFaces        { get => SConvertEntity.ToElemFaces(this);   } 
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElems(SEntityManager em, IEnumerable<SEntity> elems) : base(em, elems.Select(x => (SElem)x))
        {
            if (!(elems.FirstOrDefault() is SElem)) throw new Exception("SElems(...): SEntity is not SElem");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElems(SEntityManager em, IEnumerable<SElem> elems) : base(em, elems) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElems(SEntityManager em, SElems elems) : base(em, elems.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElems(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one element by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SElem this[int key] { get => entities[key]; }  
        /// <summary>
        /// gets the others elements in the model
        /// </summary>
        public static SElems operator -(SElems a) => a.invert;
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SElems({count} elems)"; 
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends elements by a function.  
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
        /// <param name="func">SElem, SElem, bool</param>
        /// <returns>new SElems</returns>
        public SElems Extend(Func<SElem, SElem, bool> func)
        {
            if (isEmpty) return SNew.EmptySElems(em);
            List<SElem> r = new List<SElem>();
            foreach (SElem c in elems) r.AddRange(em.elems.If(x => func(c, x)));
            return elems + r;   
        }
    }
}
