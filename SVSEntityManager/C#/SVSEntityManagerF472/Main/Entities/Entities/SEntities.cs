#pragma warning disable IDE1006                         // Naming Styles


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
using Ansys.ACT.Mechanical;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// abstract
    /// </summary>
    public abstract class SEntities<TEnt, TEnts> : SEntitiesBase, IEnumerable<TEnt>, ISEntities<TEnt> 
        where TEnt  : SEntity 
        where TEnts : ISEntities<TEnt>
    {
        /// <summary>
        /// gets list of internal (ACT) objects 
        /// </summary>
        public List<TEnt>                       entities          { get; private set; } 
        /// <summary>
        /// gets list of object Ids
        /// </summary>
        public override List<int>               ids               { get => entities.Select(e => e.id).ToList(); }
        /// <summary>
        /// gets list of element face ids (used only for element faces)
        /// </summary>
        public virtual List<int>                elemFaceIds       { get => null; } // elem-face (ElementFaceIndices)
        /// <summary>
        /// gets true if collection is empty (count == 0)
        /// </summary>
        public override bool                    isEmpty           { get => entities.Count() <= 0; }  
        /// <summary>
        /// gets count of entities for the collection
        /// </summary>
        public override int                     count             { get => entities.Count(); } 
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        public override SInfo                   info              { get => SInfo.NewGeomInfo(ids); }
        // -------------------------------------------------------------------------------------------
        //
        //      graphics:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets draw object which allows to draw additional graphics
        /// </summary>
        public SDraw                            draw              { get => em.draw.__SetSource(this); }
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes                  corners           { get => SConvertEntity.ToCorners(elems) * nodes; }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes                  mids              { get => SConvertEntity.ToMids(elems) * nodes; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// abstract
        /// </summary>
        public SEntities(SEntityManager em, IEnumerable<int> ids) : base(em)
        {
            NullAndCount(ids, nameof(ids), nameof(SEntities<TEnt, TEnts>)); // if (ids == null)      throw new Exception($"SEntities(...): Null error: ids == null. "); if (ids.Count() <= 0) throw new Exception($"SEntities(...): Count 0 error: ids.Count() <= 0. ");
            entities = ids.Select((x) => (TEnt)SNew.SEntity(em, em.GetIEntity(x))).ToList();
            entities = entities.Distinct<TEnt>(new SEqualityComparerSEntity()).ToList();                 // delete duplicates
        }
        /// <summary>
        /// abstract
        /// </summary>
        public SEntities(SEntityManager em, TEnt ent) : base(em)
        {
            Null(ent, nameof(ent), nameof(SEntities<TEnt, TEnts>)); // if (ent == null) throw new Exception($"SEntities(...): Null error: ent == null. ");
            entities = new List<TEnt>() { ent };
        }
        /// <summary>
        /// abstract
        /// </summary>
        public SEntities(SEntityManager em, IEnumerable<TEnt> entities) : base(em)
        { 
            NullAndCount(entities, nameof(entities), nameof(SEntities<TEnt, TEnts>)); //  if (entities == null) throw new Exception($"SEntities(...): Null error: entities == null. "); if (entities.Count() <= 0) throw new Exception($"SEntities(...): Count 0 error: entities.Count() <= 0. ");
            this.entities = entities.Distinct<TEnt>(new SEqualityComparerSEntity()).ToList();                 // delete duplicates
        }
        /// <summary>
        /// abstract
        /// </summary>
        public SEntities(SEntityManager em) : base(em)
        {
            entities = new List<TEnt>();  // empty 
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      selection:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// adds entities to current selection
        /// </summary> 
        public override SEntitiesBase Add() => em.Add(this);     
        /// <summary>
        /// selects entities (current selection is lost)
        /// </summary> 
        public override SEntitiesBase Sel() => em.Sel(this);   
        // -------------------------------------------------------------------------------------------
        //
        //      try convert:
        //
        // ------------------------------------------------------------------------------------------- 
        // public SEntities<TEnt, TEnts> TryConvert(SEntitiesBase es)
        // {
        //     try
        //     {
        //         return (SEntities<TEnt, TEnts>)es;
        //     }
        //     catch 
        //     { 
        //         throw new Exception("TryConvert(...): Unable to ");
        //     }
        // }
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// gets union of two entity sets
        /// </summary> 
        public static TEnts operator +(SEntities<TEnt, TEnts> a, SEntities<TEnt, TEnts> b)    => Union(a, b); 
        /// <summary>
        /// gets union of two entity sets
        /// </summary> 
        public static TEnts operator +(SEntities<TEnt, TEnts> a, IEnumerable<TEnt> b)         => Union(a, SNew.FromList<TEnt, TEnts>(a.em, b)); 
        /// <summary>
        /// gets union of two entity sets
        /// </summary> 
        public static TEnts operator +(SEntities<TEnt, TEnts> a, TEnt b)                      => Union(a, SNew.FromList<TEnt, TEnts>(a.em, new TEnt[] { b }));  
        /// <summary>
        /// gets substract a - b
        /// </summary> 
        public static TEnts operator -(SEntities<TEnt, TEnts> a, SEntities<TEnt, TEnts> b)    => Substract(a, b);  
        /// <summary>
        /// gets substract a - b
        /// </summary> 
        public static TEnts operator -(SEntities<TEnt, TEnts> a, IEnumerable<TEnt> b)         => Substract(a, SNew.FromList<TEnt, TEnts>(a.em, b)); 
        /// <summary>
        /// gets substract a - b
        /// </summary> 
        public static TEnts operator -(SEntities<TEnt, TEnts> a, TEnt b)                      => Substract(a, SNew.FromList<TEnt, TEnts>(a.em, new TEnt[] { b }));  
        /// <summary>
        /// gets intersect of two entity sets
        /// </summary> 
        public static TEnts operator *(SEntities<TEnt, TEnts> a, SEntities<TEnt, TEnts> b)    => Intersect(a, b); 
        /// <summary>
        /// gets intersect of two entity sets
        /// </summary> 
        public static TEnts operator *(SEntities<TEnt, TEnts> a, IEnumerable<TEnt> b)         => Intersect(a, SNew.FromList<TEnt, TEnts>(a.em, b));
        /// <summary>
        /// gets intersect of two entity sets
        /// </summary> 
        public static TEnts operator *(SEntities<TEnt, TEnts> a, TEnt b)                      => Intersect(a, SNew.FromList<TEnt, TEnts>(a.em, new TEnt[] { b }));  
        // -------------------------------------------------------------------------------------------
        //
        //      showns, hiddens, suppresseds, actives:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// filters entities which are hidden
        /// </summary>
        public TEnts hiddens          { get => If(x => !x.suppressed && !x.visible); }
        /// <summary>
        /// filters entities which are shown (visible)
        /// </summary>
        public TEnts showns           { get => If(x => !x.suppressed &&  x.visible); }  
        /// <summary>
        /// filters entities which are suppressed
        /// </summary>
        public TEnts suppresseds      { get => If(x => x.suppressed);  }
        /// <summary>
        /// filters entities which are not suppressed
        /// </summary>
        public TEnts actives          { get => If(x => !x.suppressed); } 
        // -------------------------------------------------------------------------------------------
        //
        //      methods:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets enumerator of collection
        /// </summary>
        public IEnumerator<TEnt> GetEnumerator() => entities.GetEnumerator(); // public IEnumerator<TEnt> IEnumerator<TEnt>.GetEnumerator() => entities.GetEnumerator();  
        /// <summary>
        /// gets enumerator of collection
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()  => entities.GetEnumerator();
        /// <summary>
        /// gets single entity from collection with requested Id
        /// </summary>
        public TEnts                  Id(int id)                              => If(e => e.id == id);
        /// <summary>
        /// gets new collection of entities from collection with requested Ids
        /// </summary>
        /// <example>
        /// <code>
        /// bodies     = em.Entities([1, 2, 3, 4]) 
        /// intersect  = bodies.Contains([3, 4, 5])    # ---> returns bodies with ids 3 and 4
        /// </code>
        /// </example>
        public TEnts                  Ids(IEnumerable<int> ids)               => If(e => ids.Contains(e.id));
        /// <summary>
        /// gets true if collection contains specified Id
        /// </summary>
        /// <example>
        /// <code>
        /// bodies = em.Entities([1, 2, 3, 4]) 
        /// bodies.Contains(1)      # ---> returns True for this case
        /// </code>
        /// </example>
        public bool                   Contains(int id)                        => ids.Contains(id);
        /// <summary>
        /// gets true if collection contains specified entity (SPart, SBody, ...)
        /// </summary>
        /// <example>
        /// <code>
        /// bodies = em.Entities([1, 2, 3, 4])
        /// body   = bodies[0]
        /// bodies.Contains(body)      # ---> returns True for this case
        /// </code>
        /// </example>
        public bool                   Contains(TEnt e)                        => entities.Contains(e, new SEqualityComparerSEntity<TEnt>());
        /// <summary>
        /// filters entity collection by a function which return true
        /// </summary>
        /// <example>
        /// <code>
        /// nodes.If(lambda n: n.x > 10)
        /// edges.If(lambda e: 10 &lt;= e.x &lt;= 20)
        /// bodies.If(lambda b: 10 &lt;= b.x &lt;= 20 and "line" in b.name)
        /// </code>
        /// </example>
        public TEnts                  If(Func<TEnt, bool> func)               => SNew.FromList<TEnt, TEnts>(em, entities.Where(func));
        /// <summary>
        /// calls action for each entity in the collection
        /// </summary>
        /// <example>
        /// <code>
        /// def F(n): print n.id
        /// nodes.ForEach(lambda n: F(n))   # alternative 1
        /// nodes.ForEach(F)                # alternative 2
        /// or:
        /// </code>
        /// </example>
        public SEntities<TEnt, TEnts> ForEach(Action<TEnt> action)             { entities.ToList().ForEach(action); return this; } 
        /// <summary>
        /// returns count (integer) of entities which can be filtered by func function
        /// </summary>
        /// <example>
        /// <code>
        /// print nodes.Count(lambda n: n.x > 10)
        /// print edges.Count(lambda e: 10 &lt;= e.x &lt;= 20)
        /// print bodies.Count(lambda b: 10 &lt;= b.x &lt;= 20 and "line" in b.name)
        /// </code>
        /// </example>
        public int                    Count(Func<TEnt, bool> func)            => entities.Count(func);
        /// <summary>
        /// filters entity collection by attached body name
        /// </summary>
        /// <example>
        /// <code> 
        /// bodies.IfName("line")  
        /// bodies.IfName("line", caseSensitive = True)
        /// </code>
        /// </example>
        public virtual TEnts          IfName(string nameContains,
                                             bool caseSensitive = false)      => If(x => x.bodies.IfName(nameContains, caseSensitive).count >= 1);
        /// <summary>
        /// sorts entity collection by a property given by func function
        /// </summary>
        /// <example>
        /// <code> 
        /// worstOne = edges.Min(lambda e: e.x)        # sorts edges by x-coordinate and gets worst one
        /// worstOne = bodies.Min(lambda b: b.volume)  # sorts bodies by volume and gets worst one
        /// </code>
        /// </example>
        public TEnt                   Min(Func<TEnt, double> func)            => entities.OrderBy(func).FirstOrDefault();
        /// <summary>
        /// sorts entity collection by a property given by func function
        /// </summary>
        /// <example>
        /// <code> 
        /// bestOne = edges.Max(lambda e: e.x)        # sorts edges by x-coordinate and gets best one
        /// bestOne = bodies.Max(lambda b: b.volume)  # sorts bodies by volume and gets best one
        /// </code>
        /// </example>
        public TEnt                   Max(Func<TEnt, double> func)            => entities.OrderBy(func).Reverse().FirstOrDefault();
        /// <summary>
        /// sorts entity collection by a property given by func function
        /// </summary>
        /// <example>
        /// <code> 
        /// sorted = edges.Min(lambda e: e.x, count = 3)        # sorts edges by x-coordinate and gets only 3 worsts
        /// sorted = bodies.Min(lambda b: b.volume, count = 3)  # sorts bodies by volume and gets only 3 worsts
        /// </code>
        /// </example>
        public TEnts                  Min(Func<TEnt, double> func, int count) => SNew.FromList<TEnt, TEnts>(em, entities.OrderBy(func).Take(count));
        /// <summary>
        /// sorts entity collection by a property given by func function
        /// </summary>
        /// <example>
        /// <code> 
        /// sorted = edges.Max(lambda e: e.x, count = 3)        # sorts edges by x-coordinate and gets only 3 bests
        /// sorted = bodies.Max(lambda b: b.volume, count = 3)  # sorts bodies by volume and gets only 3 bests
        /// </code>
        /// </example>
        public TEnts                  Max(Func<TEnt, double> func, int count) => SNew.FromList<TEnt, TEnts>(em, entities.OrderBy(func).Reverse().Take(count));
        /// <summary>
        /// sorts entity collection by a property given by func function
        /// </summary>
        /// <example>
        /// <code> 
        /// sorted = edges.OrderBy(lambda n: n.x)        # sorts edges by x-coordinate 
        /// sorted = bodies.OrderBy(lambda b: b.volume)  # sorts bodies by volume
        /// </code>
        /// </example>
        public TEnts                  OrderBy(Func<TEnt, double> func)        => SNew.FromList<TEnt, TEnts>(em, entities.OrderBy(func));
        /// <summary>
        /// reveses order of items in entity collection
        /// </summary>
        /// <example>
        /// <code> 
        /// sorted = edges.OrderBy(lambda e: e.x).Reverse()        # sorts edges by x-coordinate and returns reveres
        /// sorted = bodies.OrderBy(lambda b: b.volume).Reverse()  # sorts bodies by volume and reveres
        /// </code>
        /// </example>
        public TEnts                  Reverse()                               => SNew.FromList<TEnt, TEnts>(em, entities.ToArray().Reverse());
        /// <summary>
        /// gets list of objects (properties) from entity collection
        /// </summary>
        /// <example>
        /// <code> 
        /// lengths = edges.Get(lambda e: e.length)     # ---> returns list of length values
        /// volumes = bodies.Get(lambda b: b.volume)    # ---> returns list of volume values
        /// </code>
        /// </example>
        public List<T>                Get<T>(Func<TEnt, T> func)              => entities.Select(func).ToList();
        /// <summary>
        /// gets SStats object for a property given by func
        /// </summary>
        /// <example>
        /// <code> 
        /// volumeStats = bodies.Stats(lambda b: b.volume)    
        /// print volumeStats.sum     # total volume
        /// print volumeStats.max     # maximal volume
        /// print volumeStats.avg     # average volume
        /// print volumeStats.min     # minimal volume
        /// print volumeStats.srss    # square root of sum of volume squres 
        /// print volumeStats.count   # count of entities
        /// print volumeStats.mean    # mean volume
        /// print volumeStats.stdv    # standard deviation volume
        /// </code>
        /// </example>
        public SStats<TEnt>           Stats(Func<TEnt, double> func)          => new SStats<TEnt>(this, func);
        /// <summary>
        /// gets string 
        /// </summary>
        public override string        ToString() => $"EntityManager.{nameof(SEntities<TEnt, TEnts>)}({count} entities)"; 
        /// <summary>
        /// gets list (List object) of entites in the collection
        /// </summary>
        public List<TEnt>             ToList() => entities.ToList();
        // -------------------------------------------------------------------------------------------
        //
        //      update (e.g. coords after morphing):
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// updates data of entities from ACT database
        /// </summary> 
        public SEntities<TEnt, TEnts> Update()
        {
            em.usedElemFaces.Clear();
            ForEach(e => e.Update());
            return this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      boolean:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// gets union of two collection sets
        /// </summary> 
        /// <example>
        /// <code> 
        /// bodiesC = em.Union(bodiesA, bodiesB)    
        /// bodiesC = bodiesA + bodiesB
        /// </code>
        /// </example>
        public static TEnts Union(ISEntities<TEnt> a, ISEntities<TEnt> b)  
        {
            using (a.em.logger.StartStop(nameof(Union)))
            {
                if (a.isEmpty && b.isEmpty) return SNew.Empty<TEnt, TEnts>(a.em);
                if (a.isEmpty) return SNew.FromList<TEnt, TEnts>(b.em, b.entities);
                if (b.isEmpty) return SNew.FromList<TEnt, TEnts>(a.em, a.entities);
                SEntityManager.CheckHomo(a, b, "Union");
                List<TEnt> ret = new List<TEnt>(a.entities); // ret.AddRange(a.entities);
                ret.AddRange(b.entities);
                ret.Distinct(new SEqualityComparerSEntity()).ToList();                     // overit !!!!
                return SNew.FromList<TEnt, TEnts>(a.em, ret);
            }
        }
        /// <summary>
        /// gets substract of two collection sets
        /// </summary>
        /// <example>
        /// <code> 
        /// bodiesC = em.Substract(bodiesA, bodiesB)    
        /// bodiesC = bodiesA - bodiesB
        /// # ----------------------------
        /// # check performance:
        /// import datetime
        /// now = datetime.datetime.now()
        /// em = EntityManager.SEntityManager(ExtAPI)
        /// n1 = em.bodies.nodes
        /// n2 = em.bodies.nodes
        /// n4 = em.Entity(63).nodes
        /// n3 = (n1 + n2) # - n1
        /// print datetime.datetime.now() - now, " ---> ", n3.count
        /// now = datetime.datetime.now()
        /// n3 = n3 - n4
        /// print datetime.datetime.now() - now, " ---> ", n3.count
        /// </code>
        /// </example>
        public static TEnts Substract(ISEntities<TEnt> a, ISEntities<TEnt> b) 
        {
            using (a.em.logger.StartStop(nameof(Substract)))
            {
                if (a.isEmpty && b.isEmpty) return SNew.Empty<TEnt, TEnts>(a.em);
                if (a.isEmpty) return SNew.Empty<TEnt, TEnts>(a.em);
                if (b.isEmpty) return SNew.FromList<TEnt, TEnts>(a.em, a.entities);
                SEntityManager.CheckHomo(a, b, "Substract");
                // ----------------------------------------------------------------------------
                // List<TEnt> ret   = a.entities.ToList();
                // List<int>  ids   = b.entities.Select(e => e.id).ToList();
                // List<TEnt> aes   = a.entities;
                // List<TEnt> its = aes.Intersect(b.entities, new SEqualityComparerSEntity<TEnt>()).ToList();
                // its.ForEach(aa => ret.Remove(aa));  // TO-DO: pomaly jak prase asi !!!!
                // ----------------------------------------------------------------------------
                //  optim:
                List<TEnt> aes = a.entities;
                List<TEnt> its = aes.Intersect(b.entities, new SEqualityComparerSEntity<TEnt>()).ToList();
                aes.AsParallel().ForAll(x => x._forSubstractHelpingBool = true);
                its.AsParallel().ForAll(x => x._forSubstractHelpingBool = false);
                List<TEnt> ret = aes.AsParallel()
                                    .Where(x => x._forSubstractHelpingBool)
                                    .ToList();  
                return SNew.FromList<TEnt, TEnts>(a.em, ret);
            }
        }
        /// <summary>
        /// gets intersect of two collection sets
        /// </summary> 
        /// <example>
        /// <code> 
        /// bodiesC = em.Intersect(bodiesA, bodiesB)    
        /// bodiesC = bodiesA * bodiesB
        /// </code>
        /// </example>
        public static TEnts Intersect(ISEntities<TEnt> a, ISEntities<TEnt> b) 
        {
            using (a.em.logger.StartStop(nameof(Intersect)))
            {
                if (a.isEmpty || b.isEmpty) return SNew.Empty<TEnt, TEnts>(a.em);
                SEntityManager.CheckHomo(a, b, "Intersect"); 
                List<TEnt> ret = new List<TEnt>();
                // List<int> ids = b.entities.Select(e => e.id).ToList();
                ret = a.entities.Intersect(b.entities, new SEqualityComparerSEntity<TEnt>()).ToList();
                // foreach (TEnt e in a.entities) if (ids.Contains(e.id)) ret.Add(e); 
                return SNew.FromList<TEnt, TEnts>(a.em, ret);
            }
        } 
        internal override List<SEntity> __GetSEntityList() => entities.Cast<SEntity>().ToList();
    }
}
