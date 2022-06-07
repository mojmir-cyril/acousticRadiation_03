#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq; 

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mesh; 



namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SElem is single element object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SElem : SEntity
    {
        /// <summary>
        /// gets internal (ACT) object of the element (IElement)
        /// </summary>
        public IElement                         iElem           { get => (IElement)iEntity; }
        /// <summary>
        /// gets list of reference unique Ids
        /// </summary> 
        public List<int>                        geoEntityIds    { get => iElem.Nodes.SelectMany(n => n.GeoEntityIds).ToList(); }
        /// <summary>
        /// gets geometry type of the entities => SType.Elem
        /// </summary>
        public override SType                   type            { get => SType.Elem; }
        /// <summary>
        /// gets true if element type of the entities => true
        /// </summary>
        public override bool                    isElem          { get => true; } 
        /// <summary>
        /// gets element unique Id
        /// </summary>
        public override int                     id              { get => iElem.Id; }
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
        public override SInfo                   info            { get => SInfo.NewElemInfo(id); }
        /// <summary>
        /// gets element shape type { kHex20, kQuad4, ... }
        /// </summary>
        public ElementTypeEnum                  elemType        { get => iElem.Type; }
        /// <summary>
        /// gets element volume in current unit (em.volumeUnit)
        /// </summary>
        public double                           volume          { get => em.ConvertVolume(iElem.Volume); } 
        /// <summary>
        /// gets cross-section (ACT) object if body type is line/wire
        /// </summary> 
        public object                           crossSection     { get => bodies.FirstOrDefault()?.crossSection; }
        /// <summary>
        /// gets material (ACT) object
        /// </summary> 
        public object                           material         { get => bodies.FirstOrDefault()?.material; }
        // -------------------------------------------------------------------------------------------
        //
        //      shape groups:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets true if element is solid shape type
        /// </summary>
        public bool                             isSolid         { get => isHex || isTet || isWedge || isPyramid; }
        /// <summary>
        /// gets true if element is shell shape type
        /// </summary>
        public bool                             isShell         { get => isTri || isQuad; }
        /// <summary>
        /// gets true if element is beam shape type
        /// </summary>
        public bool                             isBeam          { get => elemType == ElementTypeEnum.kBeam3    || elemType == ElementTypeEnum.kBeam4; }
        /// <summary>
        /// gets true if element is line shape type
        /// </summary>
        public bool                             isLine          { get => elemType == ElementTypeEnum.kLine2    || elemType == ElementTypeEnum.kLine3; }
        /// <summary>
        /// gets true if element is hexa shape type
        /// </summary>
        public bool                             isHex           { get => elemType == ElementTypeEnum.kHex20    || elemType == ElementTypeEnum.kHex8; }
        /// <summary>
        /// gets true if element is wedge shape type
        /// </summary>
        public bool                             isWedge         { get => elemType == ElementTypeEnum.kWedge6   || elemType == ElementTypeEnum.kWedge15; }
        /// <summary>
        /// gets true if element is pyramid shape type
        /// </summary>
        public bool                             isPyramid       { get => elemType == ElementTypeEnum.kPyramid5 || elemType == ElementTypeEnum.kPyramid13; }
        /// <summary>
        /// gets true if element is tet shape type
        /// </summary>
        public bool                             isTet           { get => elemType == ElementTypeEnum.kTet4     || elemType == ElementTypeEnum.kTet10; }
        /// <summary>
        /// gets true if element is tri shape type
        /// </summary>
        public bool                             isTri           { get => elemType == ElementTypeEnum.kTri3     || elemType == ElementTypeEnum.kTri6; }
        /// <summary>
        /// gets true if element is quad shape type
        /// </summary>
        public bool                             isQuad          { get => elemType == ElementTypeEnum.kQuad4    || elemType == ElementTypeEnum.kQuad8; }
        /// <summary>
        /// gets true if element is quadratic shape type
        /// </summary>
        public bool                             isQuadratic     { get => !isLinear; }
        /// <summary>
        /// gets true if element is linear shape type
        /// </summary>
        public bool                             isLinear        { get => new ElementTypeEnum[]{ ElementTypeEnum.kHex8, 
                                                                                                ElementTypeEnum.kWedge6, 
                                                                                                ElementTypeEnum.kPyramid5, 
                                                                                                ElementTypeEnum.kTri3, 
                                                                                                ElementTypeEnum.kQuad4, 
                                                                                                ElementTypeEnum.kBeam3, 
                                                                                                ElementTypeEnum.kLine2, 
                                                                                                ElementTypeEnum.kTet4    }.Contains (elemType); }


        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes                  corners         { get => SNew.NodesFromIds(em, iElem.CornerNodeIds); }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes                  mids            { get => SNew.NodesFromIds(em, iElem.NodeIds.Where(id => !iElem.CornerNodeIds.Contains(id))); }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to parent body
        /// </summary>
        /// <example><code>
        /// em = EM()
        /// b  = em.current[0]
        /// print b.body
        /// </code></example>
        public SBody                            body            { get => bodies.FirstOrDefault(); }
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts                  parts           { get => elems.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies                 bodies          { get => elems.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces                  faces           { get => elems.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges                  edges           { get => elems.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts                  verts           { get => elems.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes                  nodes           { get => elems.nodes; }
        /// <summary>
        /// converts to attached elements
        /// </summary>
        public override SElems                  elems           { get => SNew.FromSingle<SElem, SElems>(this); }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces              elemFaces       { get => elems.elemFaces; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElem(SEntityManager em, IElement elem) : base(em, elem) {  }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SElem(SEntityManager em, int id) : base(em, em.mesh.GetElem(id)) { }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SElem(id = '{id}')"; 
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
        public SElems Extend(Func<SElem, SElem, bool> func) => elems.Extend(func);
        // -------------------------------------------------------------------------------------------
        //
        //      update (e.g. coords after morphing):
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// updates element data from ACT database
        /// </summary> 
        public override SEntity Update()
        {
           iEntity = em.mesh.GetElem(id);  
           return this;
        }
    }
}
