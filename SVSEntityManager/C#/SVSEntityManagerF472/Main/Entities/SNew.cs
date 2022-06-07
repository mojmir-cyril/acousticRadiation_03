#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public static class SNew
    {
        // -------------------------------------------------------------------------------------------
        //
        //      multi-entities:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SEntitiesBase GeomsFromGeom(SEntityManager em, IBaseGeoEntity geo) => GeomsFromGeoms(em, new IBaseGeoEntity[] { geo });
        public static SEntitiesBase GeomsFromGeoms(SEntityManager em, IEnumerable<IBaseGeoEntity> geos)
        {
            try
            {
                SExceptionBase.Null(em,   nameof(em));    // if (em == null)   throw new Exception($"Null error: em == null. ");
                SExceptionBase.Null(geos, nameof(geos));  // if (geos == null) throw new Exception($"Null error: geos == null. ");
                //
                //  empty:
                //
                if (geos.Count() <= 0) return EmptySBodies(em);
                //
                //  non-empty:
                //
                GeoCellTypeEnum selType = geos.FirstOrDefault().Type;
                //
                //  new:
                //
                if (selType == GeoCellTypeEnum.GeoPart || selType == GeoCellTypeEnum.GeoBody || selType == GeoCellTypeEnum.GeoFace || selType == GeoCellTypeEnum.GeoVertex || selType == GeoCellTypeEnum.GeoEdge)
                {
                    IEnumerable<SEntity> entList = geos.Select(g => SEntity(em, g)).ToList();
                    //
                    //  first ---> type: 
                    //
                    SEntity e = entList.First();
                    //
                    //  list:
                    // 
                    if      (e is SPart) return new SParts(em, entList);
                    else if (e is SBody) return new SBodies(em, entList);
                    else if (e is SFace) return new SFaces(em, entList);
                    else if (e is SEdge) return new SEdges(em, entList);
                    else if (e is SVert) return new SVerts(em, entList);
                    else if (e is SNode) return new SNodes(em, entList);
                    else throw new Exception("TO-DO: Unknown entity type.");
                    //
                    // return FromList(es);
                }
                throw new Exception("Unsupported selection type. TO-DO. ");
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SNew), nameof(GeomsFromGeoms)); }  // catch (Exception err) { throw new Exception($"SNew.FromGeos(...): {err.Message}", err); }
            return null;
        }
        public static ISEntities<TEnt> GeomsFromIds<TEnt>(SEntityManager em, IEnumerable<int> ids) where TEnt : SEntity
        {
            return (ISEntities<TEnt>)GeomsFromGeoms(em, ids.Select(id => em.GetIEntity(id)).Where(g => g.Type == SEntityManager.typeEnumDict[typeof(TEnt)])); 
        }
        public static SEntitiesBase GeomsFromIds2<TEnt>(SEntityManager em, IEnumerable<int> ids) where TEnt : SEntity
        {
            return GeomsFromGeoms(em, ids.Select(id => em.GetIEntity(id)).Where(g => g.Type == SEntityManager.typeEnumDict[typeof(TEnt)])); 
        }
        public static SEntitiesBase GeomsFromIds(SEntityManager em, IEnumerable<int> ids) 
        {
            try
            {
                SExceptionBase.Null(em, nameof(em));           // if (em  == null) throw new Exception($"Null error: em == null. ");
                SExceptionBase.NullAndCount(ids, nameof(ids)); // if (ids == null) throw new Exception($"Null error: geos == null. ");
                //
                //  empty:
                //
                if (ids.Count() <= 0) return EmptySBodies(em);
                //
                //  non-empty:
                //
                //
                //
                GeoCellTypeEnum selType = em.GetIEntity(ids.FirstOrDefault()).Type;
                //
                //  new:
                //
                if (selType == GeoCellTypeEnum.GeoPart || selType == GeoCellTypeEnum.GeoBody || selType == GeoCellTypeEnum.GeoFace || selType == GeoCellTypeEnum.GeoVertex || selType == GeoCellTypeEnum.GeoEdge)
                {
                    IEnumerable<SEntity> entList = ids?.Select(id => SEntity(em, id)).ToList();
                    SExceptionBase.Null(entList, nameof(entList));  // if (entList == null) throw new Exception($"Null error: entList == null. "); 
                    //
                    //  first ---> type:
                    //
                    SEntity e = entList.First();
                    //
                    //  list:
                    // 
                    if      (e is SPart)     return new SParts(em, entList);
                    else if (e is SBody)     return new SBodies(em, entList);
                    else if (e is SFace)     return new SFaces(em, entList);
                    else if (e is SEdge)     return new SEdges(em, entList);
                    else if (e is SVert)     return new SVerts(em, entList);
                    else if (e is SNode)     return new SNodes(em, entList); 
                    // else if (e is SElem)     return new SElems(em, entList);
                    // else if (e is SElemFace) return new SElemFaces(em, entList);
                    else throw new Exception("TO-DO: Unknown entity type.");
                    // em.Msg("FromIds(...): 0002");
                    //
                    // return FromList(es);
                    //
                    //  new (Hron):
                    //
                    // return (TEnts)Activator.CreateInstance(typeof(TEnts), em, entList);
                }
                throw new Exception("Unsupported selection type. TO-DO. "); 
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SNew), nameof(GeomsFromIds)); }  // catch (Exception err) { throw new Exception($"SNew.FromIds(...): {err.Message}", err); }
            return null;
        }
        // public static SEntities<TEnt> FromSingle<TEnt>(TEnt ent) where TEnt : SEntity 
        //     => FromList(ent.em, new List<TEnt>() { ent });
        public static TEnts FromSingle<TEnt, TEnts>(TEnt ent) where TEnt : SEntity 
            => FromList<TEnt, TEnts>(ent.em, new List<TEnt>() { ent });
        public static TEnts FromList<TEnt, TEnts>(SEntityManager em, IEnumerable<TEnt> ents) where TEnt : SEntity
        {
            try
            {
                SExceptionBase.Null(ents, nameof(ents), nameof(SNew), nameof(FromList));
                SExceptionBase.Null(em,   nameof(em),   nameof(SNew), nameof(FromList)); 
                // if (ents == null) throw new Exception($"Null error: ents == null. ");
                // if (em == null)   throw new Exception($"Null error: em == null. ");
                //
                //  empty:
                //
                if (ents.Count() <= 0) return Empty<TEnt, TEnts>(em);
                //
                //  non-empty:
                //
                SExceptionBase.NullAndCount(ents, nameof(ents), nameof(SNew), nameof(FromList)); 
                // if (ents == null)       throw new Exception($"Null error: ents == null. ");
                // if (ents.Count() <= 0)  throw new Exception($"Count 0 error: ents.Count() <= 0. ");
                // if (em == null)         throw new Exception($"Null error: em == null. ");
                //
                //
                //
                // return (SEntities<TEnt>)Activator.CreateInstance(typeof(SEntities<TEnt>), em, ents);
                //
                //  first ---> type:
                //
                // SEntity e = ents.First();
                //
                //  list:
                //
                // List<SEntity> entList = ents.Select(x => (SEntity)x).ToList();
                // em.Msg("!!!! ZDE !!!!");
                // em.Msg($" - entList.Count() : {entList.Count()}");
                //
                //  new:
                //
                // if      (e is SPart) return (dynamic) new SParts(em,  entList);
                // else if (e is SBody) return (dynamic) new SBodies(em, entList);
                // else if (e is SFace) return (dynamic) new SFaces(em,  entList);
                // else if (e is SEdge) return (dynamic) new SEdges(em,  entList);
                // else if (e is SVert) return (dynamic) new SVerts(em,  entList);
                // else if (e is SNode) return (dynamic) new SNodes(em,  entList);
                // else throw new Exception("TO-DO: Unknown entity type.");
                //
                //  new (Hron):
                //
                return (TEnts)Activator.CreateInstance(typeof(TEnts), em, ents.Select(x => (SEntity)x));
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SNew), nameof(FromList)); }  // catch (Exception err) { throw new Exception($"SNew.FromList<TEnt>(...): {err.Message}", err); }
            return default(TEnts);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      NodesFromList:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SNodes NodesFromList(SEntityManager em, IEnumerable<SNode> nodes)
        {
            // if (nodes.Count() <= 0) throw new Exception($"NodesFromList(...): Count 0 error: nodes.Count() <= 0. ");
            // SExceptionBase.NullAndCount(nodes, )
            SExceptionBase.NullAndCount(nodes, nameof(nodes), nameof(NodesFromList), nameof(SNew)); 
            //
            //  new & return:
            //
            return new SNodes(em, nodes);
        }
        public static SNodes NodesFromIds(SEntityManager em, IEnumerable<int> ids)
        {
            if (ids.Count() <= 0) throw new Exception($"NodesFromIds(...): Count 0 error: ids.Count() <= 0. ");  
            //
            //  new & return:
            //
            return new SNodes(em, ids.Select(id => new SNode(em, id)));
        }        
        public static SElems ElemsFromList(SEntityManager em, IEnumerable<SElem> elems)
        {
            if (elems.Count() <= 0) throw new Exception($"ElemsFromList(...): Count 0 error: elems.Count() <= 0. ");
            //
            //  new & return:
            //
            return new SElems(em, elems);
        }
        public static SElems ElemsFromIds(SEntityManager em, IEnumerable<int> ids)
        {
            if (ids.Count() <= 0) throw new Exception($"ElemsFromIds(...): Count 0 error: ids.Count() <= 0. ");  
            //
            //  new & return:
            //
            return new SElems(em, ids.Select(id => new SElem(em, id)));
        }
        public static SElemFaces ElemFacesFromList(SEntityManager em, IEnumerable<SElemFace> ents) => new SElemFaces(em, ents);
        public static SElemFaces ElemFacesFromIds(SEntityManager em, int id, int elemFaceId) => new SElemFaces(em, new SElemFace[] { ElemFace(em, id, elemFaceId) });
        public static SElemFaces ElemFacesFromIds(SEntityManager em, IEnumerable<int> ids, IEnumerable<int> elemFaceIds)
        {
            if (ids.Count() <= 0)                   throw new Exception($"ElemFacesFromIds(...): Count 0 error: ids.Count() <= 0. ");
            if (elemFaceIds.Count() <= 0)           throw new Exception($"ElemFacesFromIds(...): Count 0 error: elemFaceIds.Count() <= 0. ");
            if (ids.Count() != elemFaceIds.Count()) throw new Exception($"ElemFacesFromIds(...): ids.Count() != elemFaceIds.Count(). "); 
            //
            //  new & return:
            // 
            return new SElemFaces(em, Enumerable.Range(0, ids.Count()).ToList().Select(i => ElemFace(em, ids.ToList()[i], elemFaceIds.ToList()[i])));
        }
        // public static SElemFaces ElemFacesOnElem(SElem e)
        // {
        //     Enumerable.Range(0, SFaceShapeData.GetFaceCount(e.iElem)).Select(i => new SElemFace(e.em, e.id, i))
        // } 
        // -------------------------------------------------------------------------------------------
        //
        //      one-entity:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SElemFace ElemFace(SEntityManager em, int id, int faceId)
        {
            //
            //  reuse:
            //
            if (em.usedElemFaces.Keys.Contains((id, faceId))) return em.usedElemFaces[(id, faceId)];
            //
            //  new:
            //
            SElemFace ef = new SElemFace(em, id, faceId);
            em.usedElemFaces[(id, faceId)] = ef;
            return ef;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      one-entity:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SEntity SEntity(SEntityManager em, int id)       => SEntity(em, em.GetIEntity(id));
        public static SNode   SNode(SEntityManager em, int nodeId)     => new SNode(em, nodeId);
        public static SNode   SNode(SEntityManager em, INode iNode)    => new SNode(em, iNode);
        public static SElem   SElem(SEntityManager em, int elemId)     => new SElem(em, elemId);
        public static SElem   SElem(SEntityManager em, IElement iElem) => new SElem(em, iElem);
        public static SEntity SEntity(SEntityManager em, IBaseGeoEntity iEnt)
        {   
            if      (iEnt is IGeoPart part)   return new SPart(em, part);
            else if (iEnt is IGeoBody body)   return new SBody(em, body);
            else if (iEnt is IGeoFace face)   return new SFace(em, face);
            else if (iEnt is IGeoEdge edge)   return new SEdge(em, edge);
            else if (iEnt is IGeoVertex vert) return new SVert(em, vert);
            else throw new Exception("TO-DO: Unknown entity type.");
        }
        // -------------------------------------------------------------------------------------------
        //
        //      one-entity:
        //
        // ------------------------------------------------------------------------------------------- 
        // public static SEntities<TEnt> Empty<TEnt>(SEntityManager em) where TEnt : SEntity 
        //     => typeof(TEnt) == typeof(SPart) ? (dynamic) new SParts(em)  : 
        //        typeof(TEnt) == typeof(SBody) ? (dynamic) new SBodies(em) : 
        //        typeof(TEnt) == typeof(SFace) ? (dynamic) new SFaces(em)  : 
        //        typeof(TEnt) == typeof(SEdge) ? (dynamic) new SEdges(em)  : 
        //        typeof(TEnt) == typeof(SVert) ? (dynamic) new SVerts(em)  : 
        //        typeof(TEnt) == typeof(SNode) ? (dynamic) new SNodes(em)  : throw new Exception("Empty<TEnt>(...): Unknown typeof(TEnt). ");
        public static TEnts      Empty<TEnt, TEnts>(SEntityManager em) where TEnt : SEntity => (TEnts)Activator.CreateInstance(typeof(TEnts), em);
        public static SParts     EmptySParts(SEntityManager em)      => new SParts(em);
        public static SBodies    EmptySBodies(SEntityManager em)     => new SBodies(em);
        public static SFaces     EmptySFaces(SEntityManager em)      => new SFaces(em);
        public static SEdges     EmptySEdges(SEntityManager em)      => new SEdges(em);
        public static SVerts     EmptySVerts(SEntityManager em)      => new SVerts(em);
        public static SNodes     EmptySNodes(SEntityManager em)      => new SNodes(em);
        public static SElems     EmptySElems(SEntityManager em)      => new SElems(em);
        public static SElemFaces EmptySElemFaces(SEntityManager em)  => new SElemFaces(em);

        // -------------------------------------------------------------------------------------------
        //
        //      FromInfo:
        //
        // -------------------------------------------------------------------------------------------
        public static SEntitiesBase FromInfo(SEntityManager em, SInfo info)
        {
            try
            {
                //
                //  count check:
                // 
                SExceptionBase.Null(info,     nameof(info),     nameof(SNew), nameof(FromInfo)); // if (info        == null) throw new Exception("Null error: info == null. ");
                SExceptionBase.Null(info.Ids, nameof(info.Ids), nameof(SNew), nameof(FromInfo)); // if (info.Ids    == null) throw new Exception("Null error: info.Ids == null. ");
                if (info.Ids.Count <= 0) throw new Exception("Location is empty. "); // return new SEmpty(this); 
                //
                //  geoms:
                // 
                if      (info.SelectionType == SelectionTypeEnum.GeometryEntities) return GeomsFromIds(em, info.Ids);
                else if (info.SelectionType == SelectionTypeEnum.MeshNodes)        return NodesFromIds(em, info.Ids);
                else if (info.SelectionType == SelectionTypeEnum.MeshElements)     return ElemsFromIds(em, info.Ids);
                else if (info.SelectionType == SelectionTypeEnum.MeshElementFaces) return ElemFacesFromIds(em, info.Ids, info.ElementFaceIndices);
                throw new Exception($"Unsupported selection type '{info.SelectionType}'. TO-DO. ");
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SNew), nameof(FromInfo)); }  // catch (Exception err) { throw new Exception($"SNew.FromInfo(...): {err.Message}", err); }
            return null;
        }
    }
}
