#pragma warning disable IDE1006                         // Naming Styles

using System;
using System.Collections.Generic;
using System.Linq; 

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mesh; 
using Ansys.ACT.Interfaces.Geometry;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// abstract
    /// </summary>
    public abstract class SEntity : SEntitiesBase, ISEntities<SEntity>
    {
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity, INode, IElement, ...)
        /// </summary>
        public object                            iEntity        { get; set; } // IBaseGeoEntity, INode, IElement, ...
        /// <summary>
        /// gets internal (ACT) object of the entity in the list (IBaseGeoEntity, INode, IElement, ...)
        /// </summary>
        public List<SEntity>                     entities       { get => new List<SEntity>() { this }; }
        internal IBaseGeoEntity                   __iGeoEntity  { get => isGeom ? (IBaseGeoEntity)iEntity : throw new Exception("SEntity is not SPart, SBody, SFace, Edge, Vert!"); }  
        /// <summary>
        /// returns always false
        /// </summary>
        public override bool                     isEmpty        { get => false; }
        /// <summary>
        /// gets entity id (same Id as in ACT)
        /// </summary>
        public virtual int                       id             { get => __iGeoEntity.Id; }
        /// <summary>
        /// gets face Id on the element (e.g.: 1, 2, 3, ...)
        /// </summary>
        public virtual int                       elemFaceId     { get => -1; }
        /// <summary>
        /// gets ids of entities (same Id as in ACT)
        /// </summary>
        public override List<int>                ids            { get => new List<int>() { id }; }
        /// <summary>
        /// gets face Ids on the element (e.g.: 1, 2, 3, ...)
        /// </summary>
        public virtual List<int>                 elemFaceIds    { get => new List<int>() { elemFaceId }; }
        /// <summary>
        /// gets x-coordinate of the entity,
        /// works in local cartesian coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary>  
        public double                            x              { get => SGetCoord._Local(this, SDir.x, SSystemType.Cartesian); }                  // cartesian coordinate
        /// <summary>
        /// gets y-coordinate of the entity,
        /// works in local cartesian coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary>  
        public double                            y              { get => SGetCoord._Local(this, SDir.y, SSystemType.Cartesian); }                  // cartesian coordinate
        /// <summary>
        /// gets z-coordinate of the entity,
        /// works in local cartesian coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary>  
        public double                            z              { get => SGetCoord._Local(this, SDir.z, SSystemType.Cartesian); }                  // cartesian coordinate
        /// <summary>
        /// gets x, y, z coordinate of the entity (SPoint object),
        /// works in local cartesian coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary>  
        public SPoint                            xyz            { get => SGetCoord._Locals(this); } 
        /// <summary>
        /// gets x, y, z coordinate of the entity (SPoint object),
        /// works in global cartesian coordinate system of the model,
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary>  
        public List<double>                      globalXYZ      { get => SGetCoord._Globals(this); } 
        /// <summary>
        /// gets r-coordinate (radial) of the entity,
        /// works in local polar coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary> 
        public double                            r              { get => SGetCoord._Local(this, SDir.x, SSystemType.Polar); }                      // radius coordinate
        /// <summary>
        /// gets a-coordinate (angle) of the entity,
        /// works in local polar coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.angleUnit 
        /// </summary> 
        public double                            a              { get => SGetCoord._Local(this, SDir.y, SSystemType.Polar); }                      // angle coordinate [deg/rad]
        /// <summary>
        /// gets c-coordinate (circumference = length) of the entity,
        /// works in local polar coordinate system defined via em.CS(...),
        /// works in unit system defined via em.Units(...) or em.lengthUnit 
        /// </summary> 
        public double                            c              { get => SGetCoord._Local(this, SDir.y, SSystemType.Polar, circumUnit: true); }    // angle coordinate [circumference = length]
        /// <summary>
        /// gets count of entities for the collection => 1
        /// </summary>
        public override int                      count          { get => 1; }
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
        public override SInfo                    info           { get => SInfo.NewGeomInfo(id); } 
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// filter to only corner nodes 
        /// </summary>
        public override SNodes                   corners       { get => SConvertEntity.ToCorners(elems); }
        /// <summary>
        /// filter to only mid-side nodes 
        /// </summary>
        public override SNodes                   mids          { get => SConvertEntity.ToMids(elems); }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// abstract
        /// </summary>
        public SEntity(SEntityManager em, IBaseGeoEntity internalEntity) : base(em)
        { 
            iEntity = internalEntity; 
        }
        /// <summary>
        /// abstract
        /// </summary>
        public SEntity(SEntityManager em, INode internalEntity) : base(em)
        { 
            iEntity = internalEntity; 
        } 
        /// <summary>
        /// abstract
        /// </summary>
        public SEntity(SEntityManager em, IElement internalEntity) : base(em)
        { 
            iEntity = internalEntity; 
        }  
        // -------------------------------------------------------------------------------------------
        //
        //      selection:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// adds entity to current selection
        /// </summary> 
        public override SEntitiesBase Add()    => em.Add(__iGeoEntity);     
        /// <summary>
        /// selects entity (current selection is lost)
        /// </summary> 
        public override SEntitiesBase Sel()    => em.Sel(__iGeoEntity);    
        // -------------------------------------------------------------------------------------------
        //
        //      opers:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets union of single two entities
        /// </summary>  
        public static SEntitiesBase operator +(SEntity a, SEntity b) => SEntityManager.Union(a, b, "operator +(SEntity a, SEntity b)"); 
        // -------------------------------------------------------------------------------------------
        //
        //      update (e.g. coords after morphing):
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// updates element data from ACT database
        /// </summary> 
        public virtual SEntity Update()
        {
           em.usedElemFaces.Clear();
           iEntity = em.GetIEntity(id); // for: part, body, face, edge, vert
           return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Multi Criteria Analysis:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// return score value from multi-criteria analysis
        /// </summary>
        public double Score(IEnumerable<SMultiCriteria.SCriterion> criteria) => SMultiCriteria.CalculateScore(this, criteria); 
        internal override List<SEntity> __GetSEntityList() => entities.ToList(); 
        // -------------------------------------------------------------------------------------------
        //
        //      SGetCoord:
        //
        // -------------------------------------------------------------------------------------------
        internal static class SGetCoord
        {
            internal static SPoint _Locals(SEntity ent)
            {
                SPoint outputSPoint = new SPoint();
                _Local(ent, SDir.x, SSystemType.Cartesian, false, outputSPoint);
                return outputSPoint;
            }
            internal static double _Local(SEntity ent, SDir c, SSystemType sType, bool circumUnit = false, SPoint outputSPoint = null) // x, y, z
            { 
                try
                {
                    if (ent == null) throw new Exception($"__GetCoord(...): Null error: ent == null. "); 
                    //
                    //
                    //
                    double ret;
                    //
                    SEntityManager em = ent.em;
                    string gUnit = em.unitUtils.geomUnit;
                    //
                    //  dir:
                    //
                    // int i = c == SDir.x ? 0 : 
                    //         c == SDir.y ? 1 : 2;
                    //
                    //  global:
                    //
                    List<double> cen = _Globals(ent);
                    //
                    //  debug:
                    //
                    // em.Msg($"{string.Join(", ", cen)}");
                    //
                    //  glob & cartesian:
                    //
                    // if (em.coordinateSystem.IsGlobal && sType == SSystemType.Cartesian) return em.ConvertLength(cen[i]);
                    //
                    //  transform:
                    // 
                    // SPoint loc = sType == SSystemType.Cartesian ? new SPoint(cen, gUnit).InLocal(em.math, em.coordinateSystem) : 
                    //                                               new SPoint(cen, gUnit).InPolar(em.math, em.coordinateSystem);
                    bool isGlb = em.coordinateSystem.IsGlobal;
                    bool isCar = sType == SSystemType.Cartesian;
                    SPoint glb = new SPoint(cen, gUnit); 
                    SPoint loc = isGlb && isCar ? glb :
                                          isCar ? glb.InLocal(em.math, em.coordinateSystem) :
                                                  glb.InPolar(em.math, em.coordinateSystem);
                    //
                    //  coordinate:
                    // 
                    ret = loc.GetCoord(c);
                    //
                    //  outputSPoint:
                    //
                    if (outputSPoint != null)
                    { 
                        outputSPoint.Set(loc).Scale(em.ConvertLength(1.0));
                        outputSPoint.lengthUnit = em.lengthUnit;
                    }
                    //
                    //  convert:
                    //  
                    if (isCar) return em.ConvertLength(ret);
                    else
                    { 
                        bool isAngle = c == SDir.y && sType == SSystemType.Polar;
                        if (!circumUnit) return isAngle ? em.ConvertAngle(ret) : em.ConvertLength(ret); 
                        else             return ret * em.ConvertLength(loc.x); 
                    }
                }
                catch (Exception e) 
                {
                    ent?.em?.logger?.Wrn($"__GetCoord(...): {e.Message}");
                    return double.NaN; 
                }
            }
            internal static List<double> _Globals(SEntity ent)
            {
                IGeoVertex V(SEntity v) => (IGeoVertex)ent.__iGeoEntity; 
                List<double> EFC(SElemFace efc)
                {
                    int count = efc.iConnerNodes.Count();
                    double nx = efc.iConnerNodes.Sum(nn => nn.X) / count;
                    double ny = efc.iConnerNodes.Sum(nn => nn.Y) / count;
                    double nz = efc.iConnerNodes.Sum(nn => nn.Z) / count;
                    return new List<double>() { nx, ny, nz };
                }; 
                List<double> PC(SPart pp)
                {
                    IGeoBody B(SBody b) => (IGeoBody)b.__iGeoEntity;
                    SBodies bs = pp.bodies; 
                    NullAndCount(bs, "bodies in part", nameof(SGetCoord), nameof(_Globals));
                    double  x  = bs.Sum(b => B(b).Centroid[0] * B(b).Volume) / bs.Sum(b => B(b).Volume);
                    double  y  = bs.Sum(b => B(b).Centroid[1] * B(b).Volume) / bs.Sum(b => B(b).Volume);
                    double  z  = bs.Sum(b => B(b).Centroid[2] * B(b).Volume) / bs.Sum(b => B(b).Volume);
                    return new List<double>() { x, y, z };
                } 
                //
                //  centroid:
                //
                List<double> cen = ent is SPart p      ? PC(p) :
                                   ent is SBody        ? ((IGeoBody)ent.__iGeoEntity).Centroid.ToList() :
                                   ent is SFace        ? ((IGeoFace)ent.__iGeoEntity).Centroid.ToList() :
                                   ent is SEdge        ? ((IGeoEdge)ent.__iGeoEntity).Centroid.ToList() :
                                   ent is SVert        ? new List<double>() { V(ent).X,  V(ent).Y,  V(ent).Z } :
                                   ent is SNode n      ? new List<double>() { n.iNode.X, n.iNode.Y, n.iNode.Z } :
                                   ent is SElem e      ? e.iElem.Centroid.ToList() : 
                                   ent is SElemFace ef ? EFC(ef) : 
                                   throw SExceptionBase.ToDo("ent type", nameof(SGetCoord), nameof(_Globals));
                return cen;
            }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      for Substract Helping Bool:
        //
        // ------------------------------------------------------------------------------------------- 
        internal bool _forSubstractHelpingBool { get; set; } 
    }
}
