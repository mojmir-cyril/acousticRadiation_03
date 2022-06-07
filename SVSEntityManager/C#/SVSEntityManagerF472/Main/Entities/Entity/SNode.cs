#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

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
    /// Class/instance of SNode is single node object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s). 
    /// </summary>
    public class SNode : SEntity
    {
        /// <summary>
        /// gets internal (ACT) object of the node (INode)
        /// </summary>
        public INode                            iNode           { get => (INode)iEntity; } 
        /// <summary>
        /// gets list of reference unique Ids
        /// </summary> 
        public List<int>                       geoEntityIds    { get => iNode.GeoEntityIds.ToList(); }
        /// <summary>
        /// gets geometry type of the entities => SType.Node
        /// </summary>
        public override SType                   type            { get => SType.Node; }
        /// <summary>
        /// gets true if node type of the entities => true
        /// </summary>
        public override bool                    isNode          { get => true; } 
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets node id (same id as in ACT)
        /// </summary>
        public override int                     id              { get => iNode.Id; }
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
        public override SInfo                   info            { get => SInfo.NewNodeInfo(id); }
        // -------------------------------------------------------------------------------------------
        //
        //      solution & results (displacements):
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets SSolution object which keeps solution data from assigned analysis/solution (direct access to RST file),
        /// only one solution can bee assigned to Entity Manager,
        /// ACT result reader is used:
        /// IResultReader reader = analysis.GetResultsData();
        /// </summary>
        public SSolution                        solution        { get => em.solution; }  
        /// <summary>
        /// gets STimes object which contains time points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public STimes                           times           { get => solutionNode.isOk ? solutionNode.times : throw new Exception(_noRes2) ; }
        /// <summary>
        /// gets SDisps object which contains displacement points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public SDisps                           uxs             { get => solutionNode.isOk ? solutionNode.uxs   : throw new Exception(_noRes); }
        /// <summary>
        /// gets SDisps object which contains displacement points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public SDisps                           uys             { get => solutionNode.isOk ? solutionNode.uys   : throw new Exception(_noRes); }
        /// <summary>
        /// gets SDisps object which contains displacement points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public SDisps                           uzs             { get => solutionNode.isOk ? solutionNode.uzs   : throw new Exception(_noRes); }
        /// <summary>
        /// gets SDisps object which contains displacement points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public SDisps                           usums           { get => solutionNode.isOk ? solutionNode.usums : throw new Exception(_noRes); }
        /// <summary>
        /// gets SSolution.SNode object which keeps solution data for the node from assigned analysis/solution object 
        /// </summary>
        public SSolution.SNode                    solutionNode    { get => em.solution[id]; }  
        /// <summary>
        /// gets SResult object which keeps result data from assigned analysis/solution 
        /// </summary>
        public SResult                          result          { get => em.result; }
        /// <summary>
        /// gets SResultNode object which keeps result data for the node from assigned result object
        /// </summary>
        public SResult.SNode                    resultNode      { get => em.result[id]; }
        /// <summary>
        /// gets currently retrieved time in result object
        /// </summary>
        public double                           resultTime      { get => resultNode.time; }
        /// <summary>
        /// gets result value for the node in the result
        /// </summary>
        public double                           resultValue     { get => resultNode.value; }
        /// <summary>
        /// gets unit of result value
        /// </summary>
        public string                           resultUnit      { get => resultNode.unit; }
        //
        //  private:
        //
        private static string                   _noRes          { get => "No result presents, try to get results via em.solution.Assign(...) function. "; }
        private static string                   _noRes2         { get => "No result presents, try to get results via em.solution.Assign(...) function. "; }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts                  parts           { get => nodes.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies                 bodies          { get => nodes.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces                  faces           { get => nodes.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges                  edges           { get => nodes.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts                  verts           { get => nodes.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes                  nodes           { get => SNew.FromSingle<SNode, SNodes>(this); } 
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems                  elems           { get => nodes.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces              elemFaces       { get => nodes.elemFaces; }
        public SNormal                          avgNormal       { get => elemFaces.avgNormal; }
        /// <summary>
        /// gets proportional part of surrounding external element face areas, if the node is corner node. If its midside node, returns 0
        /// </summary>
        public double                           proportionalArea  { get => corners.Contains(this) ? elemFaces.exts.Sum(ef => ef.elemFaceAreaPerNode) : 0; } 
        // python equivalent: sum([ef.elFaceArea for ef in elemFaces if ef.isExt])


        // void X(List<SNodes> nodes)
        // {
        //     // [ef.elFaceArea for ef in elemFaces]
        //     // 
        //     // str(s) for s in [a for a in [ef.elFaceArea for ef in elemFaces] if a > 5]]
        //     // 
        //     // nodes.Select(n => n.X).Where(x => x > 5).Select(s => s.ToString())
        //     //      
        //     //     Min
        //     //     Max
        //     //     Average
        //     //     Sum
        //     //     Count
        //     // 
        //     var x = elems.nodes.Get(n => n.Y);
        // 
        // 
        //     // elemFaces.If(ef => ef.elFaceArea > 10)
        // }


        //{
        //    get => Get(x => x.elem);  
        //}         


        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNode(SEntityManager em, INode node) : base(em, node) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SNode(SEntityManager em, int id) : base(em, em.mesh.GetNode(id)) { }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SNode(id = '{id}')"; 
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
        public SNodes Extend(Func<SNode, SNode, bool> func) => nodes.Extend(func);
        // -------------------------------------------------------------------------------------------
        //
        //      update (e.g. coords after morphing):
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// updates (re-creates) node (e.g. coords after morphing)
        /// </summary>
        public override SEntity Update()
        {
            iEntity = em.mesh.GetNode(id); 
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      DistTo:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// gets distance in current lengthUnit
        /// </summary>
        public double DistTo(SNode n) 
            => em.ConvertLength(Math.Sqrt(Math.Pow(n.iNode.X - iNode.X, 2) + Math.Pow(n.iNode.Y - iNode.Y, 2) + Math.Pow(n.iNode.Z - iNode.Z, 2)));
        /// <summary>
        /// gets list of distances in current lengthUnit
        /// </summary>
        public List<double> DistsTo(SNodes ns) 
            => ns.Select(n => DistTo(n)).ToList(); 
        
    }
}
