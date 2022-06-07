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
    /// Class/instance of SNodes is collection of SNode objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SNodes : SEntities<SNode, SNodes>, IEnumerable
    { 
        /// <summary>
        /// gets list of internal (ACT) node objects (INode)
        /// </summary>
        public List<INode>                         iNodes            { get => entities.Select(x => x.iNode).ToList(); }  // Ansys.ACT.Common.Mesh.NodeWrapper
        /// <summary>
        /// gets geometry type of the entities => SType.Node
        /// </summary>
        public override SType                       type              { get => SType.Node; }
        /// <summary>
        /// gets true if nodal type of the entities => true
        /// </summary>
        public override bool                        isNode            { get => true; }
        /// <summary>
        /// gets SSolution object which keeps solution data from assigned analysis/solution (direct access to RST file),
        /// only one solution can bee assigned to Entity Manager,
        /// ACT result reader is used:
        /// IResultReader reader = analysis.GetResultsData();
        /// </summary>
        public SSolution                            solution          { get => em.solution.__SetSource(this); }
        /// <summary>
        /// gets object for moving of nodes in the model
        /// </summary>
        public SMorph                               morph             { get => em.licenseKey == SEntityManager.LICENCE_MORPH ? 
                                                                               em.morph.__SetSource(this) :
                                                                               throw Throw("Wrong license for using morph (SMorph) object. "); }
        /// <summary>
        /// gets object for reading of result data by ACT ResultDataTable object (access to currently evaluated result field)
        /// </summary>
        public SResult                              result            { get => em.result.__SetSource(this); }  // public SResult                              result            { get => em.result; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets the other nodes int the model
        /// </summary>
        public SNodes                               invert            { get => em.nodes - this; }
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.nodes.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        public override SInfo                       info              { get => SInfo.NewNodeInfo(ids); } 
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes                      corners           { get => nodes.elems.corners * this; }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes                      mids              { get => nodes.elems.mids * this; } 
        /// <summary>
        /// filters nodes which are shared across bodies
        /// </summary>
        public SNodes                               shareds           { get => this * (faces.If(f => f.bodies.count >= 2).nodes +
                                                                                       edges.If(e => e.bodies.count >= 2).nodes +
                                                                                       verts.If(v => v.bodies.count >= 2).nodes); } 
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts                      parts             { get => SConvertEntity.ToParts(this); }  
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies                     bodies            { get => SConvertEntity.ToBodies(this); } 
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces                      faces             { get => SConvertEntity.ToFaces(this); }  
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges                      edges             { get => SConvertEntity.ToEdges(this); }  
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts                      verts             { get => SConvertEntity.ToVerts(this); }  
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes                      nodes             { get => this; } 
        /// <summary>
        /// converts to attached elements
        /// </summary>
        public override SElems                      elems             { get => SConvertEntity.ToElems(this); }  
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces                  elemFaces         { get => SConvertEntity.ToElemFaces(this); }
        /// <summary>
        /// converts to attached element face in
        /// </summary>
        public SElemFaces                           elemFacesIn       { get => SConvertEntity.ToElemFacesIn(this); }
        /// <summary>
        /// gets proportional areas, if the node is corner node. If its midside node, returns 0
        /// </summary>
        public List<double>                         proportionalAreas { get => Get(x => x.proportionalArea); }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNodes(SEntityManager em, IEnumerable<SEntity> nodes) : base(em, nodes.Select(x => (SNode)x))
        {
            if (!(nodes.FirstOrDefault() is SNode)) throw new Exception("SNodes(...): SEntity is not SNode");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNodes(SEntityManager em, IEnumerable<SNode> nodes) : base(em, nodes) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNodes(SEntityManager em, SNodes nodes) : base(em, nodes.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNodes(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one node by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SNode this[int key] { get => entities[key]; }  
        /// <summary>
        /// gets the others nodes in the model
        /// </summary>
        public static SNodes operator -(SNodes a) => a.invert; 
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SNodes({count} nodes)"; 
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends nodes by a function.  
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
        /// <param name="func">SNode, SNode, bool</param>
        /// <returns>new SNodes</returns>
        public SNodes Extend(Func<SNode, SNode, bool> func)
        {
            if (isEmpty) return SNew.EmptySNodes(em);
            List<SNode> r = new List<SNode>();
            foreach (SNode c in nodes) r.AddRange(em.nodes.If(x => func(c, x)));
            return nodes + r;   
        }
        // -------------------------------------------------------------------------------------------
        //
        //      RedrawMesh:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// redraws mesh (useful after morphing)
        /// </summary>
        public SNodes RedrawMesh()
        {
            morph.RedrawMesh(this); // em.Redraw(byDS: true);
            return this;
        }
    }
}
