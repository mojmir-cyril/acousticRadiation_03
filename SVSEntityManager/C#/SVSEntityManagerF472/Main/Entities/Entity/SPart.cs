#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
//  Ansys:
// 
using Ansys.ACT.Automation.Mechanical; 
using Ansys.ACT.Interfaces.Geometry;



namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SPart is single part object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s). 
    /// </summary>
    public class SPart  : SEntity
    { 
        /// <summary>
        /// gets internal (ACT) object of the part (IGeoPart)
        /// </summary>
        public  IGeoPart                iPart           { get => (IGeoPart)iEntity; } 
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity)
        /// </summary>
        public IBaseGeoEntity           iGeoEntity      { get => (IBaseGeoEntity)iEntity; }
        /// <summary>
        /// gets geometry type of the entities => SType.Part
        /// </summary>
        public override SType           type            { get => SType.Part; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool            isGeom          { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets total volume of the part in current unit (em.volumeUnit )
        /// </summary>
        public double                   volume           { get => bodies.volume; }
        /// <summary>
        /// gets total area of the part in current unit (em.areaUnit )
        /// </summary>
        public double                   area             { get => bodies.area; }
        /// <summary>
        /// gets total length of the part in current unit (em.lengthUnit )
        /// </summary>
        public double                   length           { get => bodies.length; }
        /// <summary>
        /// gets total mass of the part in current unit (em.massUnit )
        /// </summary>
        public double                   mass             { get => bodies.mass; }  
        /// <summary>
        /// gets Mechanical Tree node of the part
        /// </summary>
        public Part                     nodePart         { get => em.tree.model.Geometry.GetPart(iPart); }
        /// <summary>
        /// gets Mechanical Tree node property (by string name PropertyByName(v)) of the part
        /// </summary>
        /// <example>
        /// <code> 
        /// print this["Color"].InternalValue 
        /// </code>
        /// </example>
        public Property                 this[string v]   { get { try   { return nodePart.PropertyByName(v); } 
                                                                 catch { throw new Exception($"nodeBody.PropertyByName(...): A problem to find property '{v}'. " +
                                                                                             $"This ACT is optimalized for Ansys 2021R1. ");  } } }
        /// <summary>
        /// gets Mechanical Tree node name of the part
        /// </summary>
        public string                   name             { get => nodePart.Name; 
                                                           set => nodePart.Name = value; }  
        /// <summary>
        /// gets Mechanical Tree node names for inner bodies of the part
        /// </summary>
        public List<string>             names            { get => bodies.names; }
        /// <summary>
        /// gets material names for inner bodies of the part
        /// </summary>
        public List<string>             materialNames    { get => bodies.materialNames; }
        /// <summary>
        /// gets transparencies for inner bodies of the part
        /// </summary> 
        public List<double>             transparencies   { get => bodies.transparencies; }
        /// <summary>
        /// gets colors for inner bodies of the part
        /// </summary>
        public List<int>                colors           { get => bodies.colors; }
        /// <summary>
        /// sets material names for inner bodies of the part
        /// </summary>
        public string                   materialName     {  set => bodies.materialName = value; }
        /// <summary>
        /// sets transparencies for inner bodies of the part
        /// </summary>
        public double                   transparency     {  set => bodies.transparency = value; }
        /// <summary>
        /// sets colors for inner bodies of the part
        /// </summary>
        public int                      color            {  set => bodies.color = value; }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts          { get => SNew.FromSingle<SPart, SParts>(this); }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies         { get => parts.bodies; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces          { get => parts.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges          { get => parts.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts          { get => parts.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes          { get => parts.nodes; } 
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems          elems          { get => nodes.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces      elemFaces      { get => nodes.elemFaces; }

        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SPart(SEntityManager em, IGeoPart internalPart) : base(em, internalPart) { }    
        // -------------------------------------------------------------------------------------------
        //
        //      selection:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// adds bodies to current selection
        /// </summary> 
        public override SEntitiesBase Add()
        { 
            em.Add(bodies);     
            return this;
        }
        /// <summary>
        /// selects bodies (current selection is lost)
        /// </summary> 
        public override SEntitiesBase Sel()  
        { 
            em.Sel(bodies);     
            return this;
        }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SPart(name = '{name}')"; 
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends parts by a function.  
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
        /// <param name="func">SPart, SPart, bool</param>
        /// <returns>new SParts</returns>
        public SParts Extend(Func<SPart, SPart, bool> func) => parts.Extend(func); 
        // -------------------------------------------------------------------------------------------
        //
        //      Activate: 
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// goes (selects) tree nodes attached to the parts 
        /// </summary>
        public void Activate() => nodePart.Activate();
    }
}


// f = Tree.FirstActiveObject
// f.Properties
// 
// Hidden, Suppressed, MaterialName, CoordinateSystemSelection, 
// BoundingBoxLengthX, BoundingBoxLengthY, BoundingBoxLengthZ, 
// Volume, Mass, MassCentroidX, MassCentroidY, MassCentroidZ, 
// MassMomentOfInertiaP1, MassMomentOfInertiaP2, MassMomentOfInertiaP3, 
// SheetSurfaceArea, MeshReadOnly, Nodes, Elements, MeshMetric, 
// MeshMetricMin, MeshMetricMax, MeshMetricAverage, MeshMetricSTDV, Source, 
// ReadOnly