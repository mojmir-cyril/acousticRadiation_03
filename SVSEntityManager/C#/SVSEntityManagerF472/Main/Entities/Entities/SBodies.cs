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
    /// Class/instance of SBodies is collection of SBody objects. 
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SBodies : SEntities<SBody, SBodies>, IEnumerable
    {
        /// <summary>
        /// gets list of internal (ACT) body objects (IGeoBody)
        /// </summary>
        public List<IGeoBody>       iBodies             { get => Get(x => x.iBody); }
        /// <summary>
        /// gets geometry type of the entities => SType.Body
        /// </summary>
        public override SType       type                { get => SType.Body; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool        isGeom              { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filters bodies which are solid bodies (meshed with solid elements)
        /// </summary> 
        public SBodies              solids              { get => __W(GeoBodyTypeEnum.GeoBodySolid);  }
        /// <summary>
        /// filters bodies which are surface bodies (meshed with shell elements)
        /// </summary>
        public SBodies              surfs               { get => __W(GeoBodyTypeEnum.GeoBodySheet);  }
        /// <summary>
        /// filters bodies which are line bodies (meshed with beam elements)
        /// </summary>
        public SBodies              lines               { get => __W(GeoBodyTypeEnum.GeoBodyWire); }
        private SBodies __W(GeoBodyTypeEnum t) => If((x) => x.bodyType == t);
        //
        //  invert:
        //
        /// <summary>
        /// gets the other nodes int the model
        /// </summary>
        public SBodies              invert              { get => em.bodies - this; }
        /// <summary>
        /// gets total volume of the bodies (em.volumeUnit)
        /// </summary>
        public double               volume              { get => volumes.Sum(); }
        /// <summary>
        /// gets total area of the bodies (em.areaUnit)
        /// </summary>
        public double               area                { get => areas.Sum(); }
        /// <summary>
        /// gets total length of the bodies (em.lengthUnit)
        /// </summary>
        public double               length              { get => lengths.Sum(); }
        /// <summary>
        /// gets total mass of the bodies (em.massUnit)
        /// </summary>
        public double               mass                { get => masses.Sum(); }
        /// <summary>
        /// gets list of body names 
        /// </summary>
        public List<string>         names               { get => Get(x => x.name); }
        /// <summary>
        /// gets list of body material names 
        /// </summary>
        public List<string>         materialNames       { get => Get(x => x.materialName); }
        /// <summary>
        /// gets list of body cross-section names 
        /// </summary>
        public List<string>         crossSectionNames   { get => Get(x => x.crossSectionName); }
        /// <summary>
        /// gets list of body transparencies 
        /// </summary>
        public List<double>         transparencies      { get => Get(x => x.transparency); }
        /// <summary>
        /// gets list of body volumes 
        /// </summary>
        public List<int>            colors              { get => Get(x => x.color); }
        /// <summary>
        /// gets list of body volumes 
        /// </summary>
        public List<double>         volumes             { get => Get(x => x.volume); }
        /// <summary>
        /// gets list of body areas 
        /// </summary>
        public List<double>         areas               { get => Get(x => x.area); }
        /// <summary>
        /// gets list of body lengths 
        /// </summary>
        public List<double>         lengths             { get => Get(x => x.length); }
        /// <summary>
        /// gets list of body masses 
        /// </summary>
        public List<double>         masses              { get => Get(x => x.mass); }
        /// <summary>
        /// sets body name for the bodies
        /// </summary>
        public string               name                { set => ForEach(x => x.name = value); }
        /// <summary>
        /// sets material name for the bodies
        /// </summary>
        public string               materialName        { set => ForEach(x => x.materialName = value); }
        /// <summary>
        /// sets transparency for the bodies
        /// </summary> 
        public double               transparency        { set { using (em.api.Graphics.Suspend()) ForEach((x) => x.transparency = value); } }
        /// <summary>
        /// sets color for the bodies
        /// </summary> 
        public int                  color               { set => __SetColors(value); } //  em.tree.Save(); ForEach((x) => x.SetColor(value, saveResume: false)); em.tree.Resume(); }
        /// <summary>
        /// sets color (by: red, green, blue) for the bodies
        /// </summary> 
        public void                 SetRGB(int r, int g, int b) => ForEach(x => x.color = SColorUtils.FromRGB(b, g, r)); // ANSYS ma prehozeno RGB->BGR
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
        /// gets self
        /// </summary>
        public override SBodies         bodies            { get => this; } 
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
        // public SBodies(SEntityManager em, IGeoBody body) : base(em, new List<SBody>() { new SBody(em, body) })
        // { 
        // }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SBodies(SEntityManager em, IEnumerable<SEntity> bodies) : base(em, bodies.Select(x => (SBody)x))
        {
            if (!(bodies.FirstOrDefault() is SBody)) throw new Exception("SParts(...): SEntity is not SBody");
        }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SBodies(SEntityManager em, IEnumerable<SBody> bodies) : base(em, bodies) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SBodies(SEntityManager em, SBodies bodies) : base(em, bodies.ToList()) { }
        /// <summary>
        /// creates new object
        /// </summary> 
        public SBodies(SEntityManager em) : base(em) { } // empty 
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets one body by index in the list { 0, 1, 2, ... }
        /// </summary>
        public SBody this[int key]     { get => entities[key]; } 
        /// <summary>
        /// gets the others bodies in the model
        /// </summary>
        public static SBodies operator -(SBodies a) => a.invert;
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
        //      IfName:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filters by body name (by containing sub-string)
        /// </summary>
        public override SBodies IfName(string nameContains,
                                       bool caseSensitive = false) => If(e => caseSensitive ? e.name.Contains(nameContains) :
                                                                                                       e.name.ToLower().Contains(nameContains.ToLower()));
        // -------------------------------------------------------------------------------------------
        //
        //      tree: 
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// goes (selects) nodes attached to the bodies in the tree
        /// </summary>
        public void Activate() => em.tree.tree.Activate(ToList().Select((x) => x.nodeBody));
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// Extend element edges by a function.  
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
        public SBodies Extend(Func<SBody, SBody, bool> func)
        {
            if (isEmpty) return SNew.EmptySBodies(em);
            List<SBody> r = new List<SBody>();
            foreach (SBody c in bodies) r.AddRange(em.bodies.If(x => func(c, x))); 
            return bodies + r;  
        }
    }
}
