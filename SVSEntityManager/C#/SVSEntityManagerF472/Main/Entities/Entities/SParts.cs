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
// using Ansys.Mechanical.DataModel.Converters;
// using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
// using Ansys.Mechanical.DataModel.Enums;
// using Ansys.ACT.Interfaces.Mesh;
// using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
// using Ansys.ACT.Mechanical.Tools;
// using Ansys.ACT.Common.Graphics;

using Ansys.ACT.Mechanical;
using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Geometry;
using System.ComponentModel;
using Ansys.Core.Commands.DiagnosticCommands;
using Ansys.Mechanical.DataModel.Enums;


namespace SVSEntityManagerF472
{ 
    /// <summary>
    /// Class/instance of SParts is collection of SPart objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SParts : SEntities<SPart, SParts>, IEnumerable
    { 
         /// <summary>
        /// gets list of internal (ACT) part objects (IGeoPart)
        /// </summary>
        public List<IGeoPart>  iParts          { get => entities.Select(x => x.iPart).ToList();  }
        /// <summary>
        /// gets geometry type of the entities => SType.Part
        /// </summary>
        public override SType   type            { get => SType.Part; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool    isGeom          { get => true; } 
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets the other parts in the model
        /// </summary>
        public SParts           invert          { get => em.parts - this; } 
        /// <summary>
        /// gets list of part names  
        /// </summary>
        public List<string>    names           { get => Get(x => x.name); }
        /// <summary>
        /// gets list of part volumes  
        /// </summary>
        public List<double>    volumes         { get => Get(x => x.volume); }
        /// <summary>
        /// gets list of part areas 
        /// </summary>
        public List<double>    areas           { get => Get(x => x.area); }
        /// <summary>
        /// gets list of part length 
        /// </summary>
        public List<double>    lengths         { get => Get(x => x.length); }
        /// <summary>
        /// gets list of part masses 
        /// </summary>
        public List<double>    masses          { get => Get(x => x.mass); }
        /// <summary>
        /// sets all part names  
        /// </summary>
        public string          name            { set => ForEach((x) => x.name = value); }
        /// <summary>
        /// sets all part material name 
        /// </summary>
        public string          materialName    { set => bodies.ForEach((x) => x.materialName = value); }
        /// <summary>
        /// sets all part transparency 
        /// </summary>
        public double          transparency    { set { using (em.api.Graphics.Suspend()) bodies.ForEach((x) => x.transparency = value); } }
        /// <summary>
        /// sets all part color 
        /// </summary>
        public int             color           { set => __SetColors(value); } 
        /// <summary>
        /// sets all part color by red, green, blue
        /// </summary>
        public void            SetRGB(int r, int g, int b) => bodies.ForEach((x) => x.color = SColorUtils.FromRGB(b, g, r)); // ANSYS ma prehozeno RGB->BGR
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts             { get => this; }  
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
        // public SParts(SEntityManager em, IGeoPart part) : base(em, new List<SPart>() { new SPart(em, part) })
        // { 
        // }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SParts(SEntityManager em, IEnumerable<SEntity> parts) : base(em, parts.Select(x => (SPart)x))
        {
            if (!(parts.FirstOrDefault() is SPart)) throw new Exception("SParts(...): SEntity is not SPart");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SParts(SEntityManager em, IEnumerable<SPart> parts) : base(em, parts) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SParts(SEntityManager em, SPart part) : base(em, part) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SParts(SEntityManager em, SParts parts) : base(em, parts.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SParts(SEntityManager em) : base(em) { } // empty 
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
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one part by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SPart this[int key] { get => entities[key]; }  
        /// <summary>
        /// gets the others parts in the model
        /// </summary>
        public static SParts operator -(SParts a) => a.invert; 
        // -------------------------------------------------------------------------------------------
        //
        //      IfName:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filters by body name (by containing sub-string)
        /// </summary>
        public override SParts IfName(string nameContains,
                                      bool caseSensitive = false) => If(e => caseSensitive ? e.name.Contains(nameContains) :
                                                                                             e.name.ToLower().Contains(nameContains.ToLower())); 
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string  ToString() => $"EntityManager.SBodies({count} bodies)"; 
        // -------------------------------------------------------------------------------------------
        //
        //      tree: 
        //
        // ------------------------------------------------------------------------------------------- 
        private void __SetColors(int c)
        {
            em.tree.Save();  
            //
            //  sel all:
            //
            Activate(); 
            //
            //  color:
            //
            using (new Transaction())
                using (em.api.Graphics.Suspend())
                    ForEach((x) => x["Color"].InternalValue = c);
            //
            //  sel prev:
            //
            em.tree.Resume();  
        }
        // -------------------------------------------------------------------------------------------
        //
        //      tree: 
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// goes (selects) tree nodes attached to the parts 
        /// </summary>
        public void Activate() => em.tree.tree.Activate(Get(x => x.nodePart));
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
        public SParts Extend(Func<SPart, SPart, bool> func)
        {
            if (isEmpty) return SNew.EmptySParts(em);
            List<SPart> r = new List<SPart>();
            foreach (SPart c in parts) r.AddRange(em.parts.If(x => func(c, x)));
            return parts + r; 
        }
    }
}
