#pragma warning disable IDE1006                         // Naming Styles


 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SVSExceptionBase;

//
//  Ansys:
// 
using Ansys.ACT.Mechanical; 
using Ansys.ACT.Automation.Mechanical; 


namespace SVSEntityManagerF472
{
    /// <summary>
    /// Parent object of all entities (SParts, SPart, SBodies, SBody, SNodes, SNode, ...)
    /// </summary>
    public abstract class SEntitiesBase : SExceptionBase
    {
        /// <summary>
        /// The SEntityManager object created by SVS FEM s.o.r. for fast/easy work with geometrical entitites.
        /// The main instance (em) genarally keeps all necessary settings for selecting.
        /// </summary>
        public SEntityManager               em            { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SType               type          { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract List<int>           ids           { get; } 
        // -------------------------------------------------------------------------------------------
        //
        //      type:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets true if geometry type of the entities => false
        /// </summary>
        public virtual bool                 isGeom        { get => false; }
        /// <summary>
        /// gets true if nodal type of the entities => false
        /// </summary>
        public virtual bool                 isNode        { get => false; }
        /// <summary>
        /// gets true if elemental type of the entities => false
        /// </summary>
        public virtual bool                 isElem        { get => false; }
        /// <summary>
        /// gets true if element face type of the entities => false
        /// </summary> 
        public virtual bool                 isElemFace    { get => false; } 
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SInfo               info          { get; } // location 
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SNodes              corners       { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SNodes              mids          { get; } 
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SParts              parts         { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SBodies             bodies        { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SFaces              faces         { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SEdges              edges         { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SVerts              verts         { get; }
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SNodes              nodes         { get; } 
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SElems              elems         { get; } 
        /// <summary>
        /// abstract
        /// </summary>
        public abstract SElemFaces          elemFaces     { get; } 
        /// <summary>
        /// defines if graphics redraw will be call after show/hide operations
        /// </summary>
        public bool                         autoRedraw    { get; set; } = true;
        /// <summary>
        /// gets true if collection is empty (count == 0)
        /// </summary>
        public abstract bool                isEmpty       { get; }  
        /// <summary>
        /// gets count of entities for the collection
        /// </summary>
        public abstract int                 count         { get; } 
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// abstract
        /// </summary>
        public SEntitiesBase(SEntityManager em) // : base(em)
        {
            this.em = em; 
            //
            //  po dokonceni If je treba odstranit prev_ objekty aby nahodou nebyl vzit stary CS pro dalsi vybery:
            //
            em.math.ClearTransMatrix();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      equal:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// returns hash code
        /// </summary> 
        public override int GetHashCode() => (type, ids).GetHashCode();
        /// <summary>
        /// compares single entities by ids
        /// compares entinty collections by ids
        /// </summary> 
        public static bool operator ==(SEntitiesBase a, SEntitiesBase b) => __Same(a, b);
        /// <summary>
        /// compares single entities by ids
        /// compares entinty collections by ids
        /// </summary> 
        public static bool operator !=(SEntitiesBase a, SEntitiesBase b) => !__Same(a, b);
         /// <summary>
        /// compares single entities by ids
        /// compares entinty collections by ids
        /// </summary> 
        public override bool Equals(object o)
        {
            if (this is null && o is null) return true;
            if (this is null)              return false;
            if (o is null)                 return false;
            if (!(o is SEntitiesBase))     return false;
            return __Same(this, (SEntitiesBase)o); 
        }
        private static bool __Same(SEntitiesBase a, SEntitiesBase b)
        {
            //
            //  srovnani z pohledu selekce!
            //    SFaces(ids = [1]) == SFace(id = 1) ----> True
            //
            if (a is null && b is null)      return true;
            if (a is null)                   return false;
            if (b is null)                   return false;
            if (a.type  != b.type)           return false;
            if (a.count != b.count)          return false;
            if (!a.ids.SequenceEqual(b.ids)) return false;
            return true;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      show, hide, visible:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// works with all entities in the collection
        /// returns true if all entities are visible
        /// returns false if any entity is hidden
        /// sets visibility for all entities
        /// </summary>
        public bool               visible           { get => bodies.ToList().All(x => x.nodeBody.Visible); 
                                                      set => __ShowHide(value); }
        /// <summary>
        /// shows all entities in the collection
        /// </summary>                                             
        public SEntitiesBase Show()              => __ShowHide(true);
        /// <summary>
        /// shows all entities in the collection and hide all others (same as: .Show().HideOthers())
        /// </summary>
        public SEntitiesBase ShowOnly()          => Show().HideOthers();
        /// <summary>
        /// hides all entities in the collection
        /// </summary>    
        public SEntitiesBase Hide()              => __ShowHide(false);
        /// <summary>
        /// hides all entities in the collection and shows all others
        /// </summary>    
        public SEntitiesBase HideOnly()          => (em.bodies - Hide().bodies).Show();
        /// <summary>
        /// hides others entities than are in the collection
        /// </summary> 
        public SEntitiesBase HideOthers()        => (em.bodies - bodies).Hide();
        private SEntitiesBase __ShowHide(bool show)
        {
            int c = 0;
            using (new Transaction())
                using (em.api.Graphics.Suspend()) 
                    foreach (SBody b in bodies) 
                        if (b.nodeBody.Visible != show)
                        {
                            b.nodeBody.Visible = show;
                            c++;
                        }
            em.logger.Msg($"__ShowHide(...): {c} ---> {show}");
            return autoRedraw ? Redraw() : this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      suppress, unuppress, suppressed:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// works with all entities in the collection
        /// returns true if all entities are suppresed
        /// returns false if any entity is suppresed
        /// sets suppress state for all entities
        /// </summary>
        public bool               suppressed         { get => bodies.ToList().All((x) => x.nodeBody.Suppressed); 
                                                       set => __SuppressUnsuppress(value); }
        /// <summary>
        /// unsuppress all entities in the collection
        /// </summary>   
        public SEntitiesBase Unsuppress()        => __SuppressUnsuppress(false);
        /// <summary>
        /// unsuppress all entities in the collection
        /// </summary>  
        public SEntitiesBase Suppress()          => __SuppressUnsuppress(true);
        /// <summary>
        /// suppress others entities than are in the collection
        /// </summary>
        public SEntitiesBase SuppressOthers()    => (em.bodies - bodies).Suppress();
        private SEntitiesBase __SuppressUnsuppress(bool sup)
        {
            int c = 0;
            using (new Transaction()) 
                using (em.api.Graphics.Suspend()) 
                    foreach (SBody b in bodies)
                        if (b.nodeBody.Suppressed != sup)
                        {
                            b.nodeBody.Suppressed = sup;
                            c++;
                        }
            em.logger.Msg($"__SuppressUnsuppress(...): {c} ---> {sup}");
            return autoRedraw ? Redraw() : this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Redraw:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// redraws the Mechanical graphics.
        /// </summary>
        /// <example>
        /// <code>
        /// em.Redraw()
        /// </code>
        /// </example> 
        /// <param name="byDS">true ... more powerfull</param>
        /// <returns>SEntityManager ---> self</returns>
        public SEntitiesBase Redraw(bool byDS = true)
        {
            em.Redraw(byDS); 
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      python:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// see ToString()
        /// </summary>
        public string __str__() => ToString();
        /// <summary>
        /// see count
        /// </summary>
        public int __len__()    => count;
        // -------------------------------------------------------------------------------------------
        //
        //      units:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// sets unit strings
        /// default:
        ///  lengthUnit = "mm" 
        ///  angleUnit  = "deg" 
        ///  massUnit   = "kg"
        /// </summary>
        /// <example>
        /// <code>
        /// em.bodies.Units("mm", "deg", "kg")
        /// em.bodies.Units("m", "rad", "t")
        /// </code>
        /// </example>
        public SEntitiesBase Units(string length, string angle, string mass)
        {
            em.Units(length, angle, mass);
            return this;
        }
        /// <summary>
        /// sets default unit strings
        /// default:
        ///  lengthUnit = "mm" 
        ///  angleUnit  = "deg" 
        ///  massUnit   = "kg"
        /// </summary>
        /// <example>
        /// <code>
        /// em.bodies.Units("mm", "deg", "kg")
        /// em.bodies.Units("m", "rad", "t")
        /// </code>
        /// </example>
        public SEntitiesBase Units() => Units("mm", "deg", "kg");
        // -------------------------------------------------------------------------------------------
        //
        //      AccurDigits:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// sets rounding of numbers for keeping of accuracy,
        /// it is necessary for selecting by coordinates,
        /// the function sets the digits accuracy level lengthAccurDigits, massAccurDigitis, angleAccurDigits together
        /// default:
        ///   lengthAccurDigits = 10
        ///   angleAccurDigits  = 10
        ///   massAccurDigitis  = 10 
        /// </summary>
        public SEntitiesBase AccurDigits(int length, int angle, int mass)
        {
            em.AccurDigits(length, angle, mass);
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      CS:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// sets global coordinate system
        /// </summary>
        public SEntitiesBase CS()
        {
            em.CS();
            return this;
        }
        /// <summary>
        /// sets coordinate system by tree node id
        /// </summary>
        public SEntitiesBase CS(int id)
        {
            em.CS(id);
            return this;
        }
        /// <summary>
        /// sets coordinate system by tree node name
        /// </summary>
        public SEntitiesBase CS(string name)
        {
            em.CS(name);
            return this;
        }
        /// <summary>
        /// sets coordinate system by CoordinateSystem object
        /// </summary>
        public SEntitiesBase CS(CoordinateSystem cs)
        {
            em.CS(cs);
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Sel & Add & Filter & Remove:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// adds entities to current selection
        /// </summary> 
        public abstract SEntitiesBase Add();
        /// <summary>
        /// selects entities (current selection is lost)
        /// </summary> 
        public abstract SEntitiesBase Sel(); 
        /// <summary>
        /// filters entities in current selection
        /// </summary> 
        public SEntitiesBase Filter() => SEntityManager.Intersect(em.current, this, "Filter.Intersect").Sel(); 
        /// <summary>
        /// removes entities from current selection
        /// </summary> 
        public SEntitiesBase Remove() => SEntityManager.Substract(em.current, this, "Remove.Substract").Sel();

        // -------------------------------------------------------------------------------------------
        //
        //      __GetSEntityList:
        //
        // -------------------------------------------------------------------------------------------  
        internal abstract List<SEntity> __GetSEntityList();
    }
}
