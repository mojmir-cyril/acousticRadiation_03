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


namespace SVSEntityManagerF472
{
    public static class SConvertUtils
    {
        public static SParts GeomToParts(SEntitiesBase s, IEnumerable<SEntity> entities)
            => SNew.FromList<SPart, SParts>(s.em, entities.Select(e => SNew.GeomsFromGeom(s.em, ((dynamic)e.iEntity).Part))
                                                          .Where(e => !e.isEmpty).SelectMany(e => (SParts)e));

        public static SBodies GeomToBodies(SEntitiesBase s, IEnumerable<SEntity> entities)
            => SNew.FromList<SBody, SBodies>(s.em, entities.Select(e => SNew.GeomsFromGeoms(s.em, ((dynamic)e.iEntity).Bodies))
                                                           .Where(e => !e.isEmpty).SelectMany(e => (SBodies)e));

        public static SFaces GeomToFaces(SEntitiesBase s, IEnumerable<SEntity> entities)
            => SNew.FromList<SFace, SFaces>(s.em, entities.Select(e => SNew.GeomsFromGeoms(s.em, ((dynamic)e.iEntity).Faces))
                                                          .Where(e => !e.isEmpty).SelectMany(e => (SFaces)e));

        public static SEdges GeomToEdges(SEntitiesBase s, IEnumerable<SEntity> entities)
            => SNew.FromList<SEdge, SEdges>(s.em, entities.Select(e => SNew.GeomsFromGeoms(s.em, ((dynamic)e.iEntity).Edges))
                                                          .Where(e => !e.isEmpty).SelectMany(e => (SEdges)e));

        public static SVerts GeomToVerts(SEntitiesBase s, IEnumerable<SEntity> entities)
            => SNew.FromList<SVert, SVerts>(s.em, entities.Select(e => SNew.GeomsFromGeoms(s.em, ((dynamic)e.iEntity).Vertices))
                                                          .Where(e => !e.isEmpty).SelectMany(e => (SVerts)e));

        public static SNodes GeomToNodes(SEntitiesBase s, IEnumerable<SEntity> geoms)
            => SNew.NodesFromList(s.em, s.em.mesh.GetNodes(geoms.Select(g => g.id)).Select(n => new SNode(s.em, n)));

        public static SElems GeomToElems(SEntitiesBase s, IEnumerable<SEntity> geoms)
            => SNew.ElemsFromList(s.em, s.em.mesh.GetElems(geoms.Select(g => g.id)).Select(e => new SElem(s.em, e)));

        public static SElemFaces ToElemFaces(SBodies s)
            => ToElemFaces(s.faces);
        // {
        //     // SBodies a = s.actives; 
        //     // SNodes n = a.faces.nodes + a.edges.nodes; 
        //     // return ToElemFaces(n, SConvertType.OnlyIfAllAttached).exts;
        //     // return SNew.ElemFacesFromList(s.em, a.faces.SelectMany(f => ToElemFaces(f.nodes, SConvertType.OnlyIfAllAttached).exts.ToList()));
        // }
        public static SElemFaces ToElemFaces(SFaces s)
        {
            using (s.em.logger.StartStop($"ToElemFaces(SFaces s)"))
            {
                Func<SFace, IEnumerable<SElemFace>> FromFace = face =>
                {
                    // SNodes     nodes = face.nodes.corners;
                    // IList<int> oks   = nodes.ids;
                    // SNodes     nodes = face.nodes;
                    // IList<int> oks   = nodes.ids;
                    //
                    // IEnumerable<SElemFace> efs = nodes.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.cornersIds.Contains(n.id))
                    //                                                                                     .Where(ef => ef.faceNodeIds.Intersect(oks).Count() >= ef.faceNodeCount));
                    // --------------------------------------------------------------------
                    //  Optim 1 :
                    // SNodes     nodes = face.nodes;
                    // IList<int> oks   = nodes.ids;
                    // IEnumerable<SElemFace> efs = nodes.entities.SelectMany(n => _ElemFacesList(n.elems).AsParallel() 
                    //                                                                                     .Where(ef => ef.cornersIds.Contains(n.id))
                    //                                                                                     .Where(ef => ef.faceNodeIds.Intersect(oks).Count() >= ef.faceNodeCount));
                    // --------------------------------------------------------------------
                    //  Optim 2 :
                    SNodes     nodes = face.nodes;
                    Dictionary<int, int> oks = nodes.ToDictionary(n => n.id, n => n.id);                   
                    IEnumerable<SElemFace> efs = _ElemFacesList(nodes.elems)
                                                      .AsParallel()
                                                      .Where(ef => oks.ContainsKey(ef.cornersIds[0]))
                                                      .Where(ef => ef.cornersIds.All(id => oks.ContainsKey(id))); 
                    return efs;
                };
                return SNew.ElemFacesFromList(s.em, s.SelectMany(f => FromFace(f)));
            }
        }

        public static SElemFaces ToElemFaces(SEdges s)
            => ToElemFaces(s.nodes, SConvertType.AnyAttached).exts;

        public static SElemFaces ToElemFaces(SVerts s)
            // => ToElemFaces(s.nodes, SConvertType.AnyAttached);
            => ToElemFaces(s.nodes, SConvertType.AnyAttached).exts;

        public static SElemFaces ToElemFaces(SNodes s, SConvertType cType = SConvertType.AnyAttached)
        { 
            using (s.em.logger.StartStop($"ToElemFaces (cType = {cType})"))
            {
                if (cType == SConvertType.OnlyIfAllAttached)
                {
                    // SElemFaces efs = ToElemFaces(s.elems);
                    // 
                    // var w = e
                    // SElemFaces wrong = (efs.nodes - s).elemFaces; // * efs;
                    // SElemFaces ef2   = efs - wrong; 
                    // return ef2;
                    //
                    // IList<int> oks    = s.ids;
                    // IEnumerable<SElemFace> efs = s.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.faceNodeIds.Contains(n.id)).Where(ef => ef.faceNodeIds.Intersect(oks).Count() >= ef.faceNodeCount));
                    // return SNew.ElemFacesFromList(s.em, efs);
                    //
                    // SNodes     corners = s.corners;
                    // SElemFaces efs1    = corners.elemFaces;
                    // SElemFaces efs2    = (corners.elems.corners - corners).elemFaces;
                    // return efs1 - efs2;
                    // 
                    // -----------------------------------------------------------------------------------------------
                    // IList<int> oks = s.ids;
                    // IEnumerable<SElemFace> efs = s.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.faceNodeIds.Contains(n.id))
                    //                                                                                 .Where(ef => ef.faceNodeIds.Intersect(oks).Count() >= ef.faceNodeCount)); 
                    // return SNew.ElemFacesFromList(s.em, efs);
                    // -----------------------------------------------------------------------------------------------
                    // IList<int> oks = s.ids;
                    // IEnumerable<SElemFace> efs = s.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.HasNode(n.id))
                    //                                                                                 .Where(ef => ef.faceNodeIds.Intersect(oks).Count() >= ef.faceNodeCount)); 
                    // return SNew.ElemFacesFromList(s.em, efs);
                    // -----------------------------------------------------------------------------------------------
                    Dictionary<int, int> oks = s.ids.ToDictionary(i => i, i => i);
                    List<SElemFace>      efs = _ElemFacesList(s.elems)
                                                        .AsParallel()
                                                        .Where(ef => ef.faceNodeIds.All(id => oks.ContainsKey(id)))
                                                        .ToList();
                    return SNew.ElemFacesFromList(s.em, efs); 
                }
                else if (cType == SConvertType.AnyAttached)
                { 
                    // IEnumerable<SElemFace> efs = s.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.faceNodeIds.Contains(n.id)));
                    // IEnumerable<SElemFace> efs = s.entities.SelectMany(n => __ElemFacesList(n.elems).Where(ef => ef.HasNode(n.id)));
                    // -----------------------------------------------------------------------------------------------
                    Dictionary<int, int> oks = s.ids.ToDictionary(i => i, i => i);
                    List<SElemFace>      efs = _ElemFacesList(s.elems)
                                                    .AsParallel()
                                                    .Where(ef => ef.faceNodeIds.Any(id => oks.ContainsKey(id)))
                                                    .ToList();
                    return SNew.ElemFacesFromList(s.em, efs);
                } 
                else throw new Exception($"ToElemFaces(...): cType = {cType}");
            } 
        }

        public static SElemFaces ToElemFaces(SElems es)
            => SNew.ElemFacesFromList(es.em, _ElemFacesList(es));

        private static List<SElemFace> _ElemFacesList(SElems es)
        { 
            using (es.em.logger.StartStop(nameof(_ElemFacesList)))
            {
                SEqualityComparerSElemFace qualComp = new SEqualityComparerSElemFace();
                return es
                        // .AsParallel()
                         .Where(e => e.isSolid || e.isShell) 
                         .SelectMany(e => Enumerable.Range(0, SFaceShapeData.GetFaceCount(e.iElem))
                                                    .Select(i => SNew.ElemFace(es.em, e.id, i)))
                         .Distinct(qualComp)
                         .ToList();
            }
        }
        /// <summary>
        /// converts to geometry entities
        /// </summary>
        /// <example><code>
        /// import datetime
        /// Now = datetime.datetime.now
        /// n = Now()
        /// c = em.nodes.bodies.nodes.bodies.count 
        /// print c, Now() - n
        /// </code></example>
        public static TEnts MeshToGeoms<TEnt, TEnts>(SNodes s) where TEnts : SEntitiesBase where TEnt : SEntity
        {
            using (s.em.logger.StartStop("MeshToGeoms<TEnt, TEnts>(SNodes s)"))
            {
                // GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                // IEnumerable<IGeoEntity> ents = s.entities.SelectMany(x => x.geoEntityIds).Select(id => s.em.GetIEntity(id)).Where(e => e.Type == eTyp);
                // return (TEnts)SNew.GeomsFromGeoms(s.em, ents);
                // ----------------------------------------------------------------
                // optim 1:
                // GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                // SNodes ns = new SNodes(s.em, s);
                // List<IGeoEntity> rs = new List<IGeoEntity>();
                // int c = ns.Count();
                // int x = 0;
                // for (int i = 0; i < c; i++)
                // {
                //     List<IGeoEntity> es = ns[x]
                //                             .geoEntityIds
                //                             .Select(id => s.em.GetIEntity(id))
                //                             .Where(e => e.Type == eTyp)
                //                             .ToList();
                //     if (es.Count() <= 0)
                //     {
                //         x ++;
                //         continue;
                //     } 
                //     rs.AddRange(es);
                //     ns = ns - SNew.GeomsFromGeoms(s.em, es).nodes;
                //     x = 0;
                //     if (ns.isEmpty) break;
                // }
                // return (TEnts)SNew.GeomsFromGeoms(s.em, rs);
                // ----------------------------------------------------------------
                // optim 2:
                GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                IEnumerable<IGeoEntity> ents = s.entities
                                              //  .AsParallel()
                                                .SelectMany(x => x.geoEntityIds)
                                                .Distinct()
                                                .Select(id => s.em.GetIEntity(id))
                                                .Where(e => e.Type == eTyp);
                return (TEnts)SNew.GeomsFromGeoms(s.em, ents);
            }
        }
        /// <summary>
        /// converts to geometry entities
        /// </summary>
        /// <example><code>
        /// import datetime
        /// Now = datetime.datetime.now
        /// n = Now()
        /// c = em.elems.bodies.elems.bodies.count 
        /// print c, Now() - n
        /// # 0:01:08.377000
        /// </code></example>
        public static TEnts MeshToGeoms<TEnt, TEnts>(SElems s) where TEnts : SEntitiesBase where TEnt : SEntity
        {
            using (s.em.logger.StartStop("MeshToGeoms<TEnt, TEnts>(SElems s)"))
            {
                // GeoCellTypeEnum         eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                // IEnumerable<IGeoEntity> ents = s.entities.SelectMany(x => x.geoEntityIds).Select(id => s.em.GetIEntity(id)).Where(e => e.Type == eTyp);
                // return (TEnts)SNew.GeomsFromGeoms(s.em, ents);
                // ----------------------------------------------------------------
                // optim 1:
                // return MeshToGeoms<TEnt, TEnts>(s.nodes);
                // ----------------------------------------------------------------
                // optim 2: 
                // GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                // SElems xs = new SElems(s.em, s);
                // List<IGeoEntity> rs = new List<IGeoEntity>();
                // for (int i = 0; i < xs.Count(); i++)
                // {
                //     List<IGeoEntity> es = xs[0].geoEntityIds
                //                                .Select(id => s.em.GetIEntity(id))
                //                                .Where(e => e.Type == eTyp)
                //                                .ToList();
                //     rs.AddRange(es);
                //     xs = xs - SNew.GeomsFromGeoms(s.em, es).elems;
                //     if (xs.isEmpty) break;
                // }
                // return (TEnts)SNew.GeomsFromGeoms(s.em, rs);
                // ----------------------------------------------------------------
                // optim 3:
                GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                IEnumerable<IGeoEntity> ents = s.entities
                                             //   .AsParallel()
                                                .SelectMany(x => x.geoEntityIds)
                                                .Distinct()
                                                .Select(id => s.em.GetIEntity(id))
                                                .Where(e => e.Type == eTyp);
                return (TEnts)SNew.GeomsFromGeoms(s.em, ents); 
            }
        }
        /// <summary>
        /// converts to geometry entities
        /// </summary>
        /// <example><code>
        /// import datetime
        /// Now = datetime.datetime.now
        /// n = Now()
        /// c = em.elemFaces.bodies.elemFaces.bodies.count 
        /// print c, Now() - n
        /// </code></example>
        public static TEnts MeshToGeoms<TEnt, TEnts>(SElemFaces s) where TEnts : SEntitiesBase where TEnt : SEntity
        {
            using (s.em.logger.StartStop("MeshToGeoms<TEnt, TEnts>(SElemFaces s)"))
            {
                // GeoCellTypeEnum         eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                // IEnumerable<IGeoEntity> ents = s.entities.SelectMany(x => x.geoEntityIds).Select(id => s.em.GetIEntity(id)).Where(e => e.Type == eTyp);
                // return (TEnts)SNew.GeomsFromGeoms(s.em, ents);
                // ----------------------------------------------------------------
                // optim 1:
                // return MeshToGeoms<TEnt, TEnts>(s.nodes);
                // ----------------------------------------------------------------
                // optim 2:
                GeoCellTypeEnum eTyp = SEntityManager.typeEnumDict[typeof(TEnt)];
                IEnumerable<IGeoEntity> ents = s.entities
                                               // .AsParallel()
                                                .SelectMany(x => x.geoEntityIds)
                                                .Distinct()
                                                .Select(id => s.em.GetIEntity(id))
                                                .Where(e => e.Type == eTyp);
                return (TEnts)SNew.GeomsFromGeoms(s.em, ents); 
            }
        }

    }
}
