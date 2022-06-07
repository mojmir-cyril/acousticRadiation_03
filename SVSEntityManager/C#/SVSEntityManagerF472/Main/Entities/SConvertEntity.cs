#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member
 
using System.Linq; 


namespace SVSEntityManagerF472 
{ 
    public static class SConvertEntity
    {
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToParts:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SParts ToParts(SParts s)      => s; 
        public static SParts ToParts(SBodies s)     => SConvertUtils.GeomToParts(s, s.entities); 
        public static SParts ToParts(SFaces s)      => SConvertUtils.GeomToParts(s, s.entities); 
        public static SParts ToParts(SEdges s)      => SConvertUtils.GeomToParts(s, s.entities); 
        public static SParts ToParts(SVerts s)      => SConvertUtils.GeomToParts(s, s.entities); 
        public static SParts ToParts(SNodes s)      => SConvertUtils.MeshToGeoms<SBody, SBodies>(s).parts;
        public static SParts ToParts(SElems s)      => SConvertUtils.MeshToGeoms<SBody, SBodies>(s).parts;
        public static SParts ToParts(SElemFaces s)  => SConvertUtils.MeshToGeoms<SBody, SBodies>(s).parts; 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToBodies:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SBodies ToBodies(SParts s)     => SConvertUtils.GeomToBodies(s, s.entities);
        public static SBodies ToBodies(SBodies s)    => s;
        public static SBodies ToBodies(SFaces s)     => SConvertUtils.GeomToBodies(s, s.entities);
        public static SBodies ToBodies(SEdges s)     => SConvertUtils.GeomToBodies(s, s.entities);
        public static SBodies ToBodies(SVerts s)     => SConvertUtils.GeomToBodies(s, s.entities);
        public static SBodies ToBodies(SNodes s)     => SConvertUtils.MeshToGeoms<SBody, SBodies>(s);
        public static SBodies ToBodies(SElems s)     => SConvertUtils.MeshToGeoms<SBody, SBodies>(s);
        public static SBodies ToBodies(SElemFaces s) => SConvertUtils.MeshToGeoms<SBody, SBodies>(s); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToFaces:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SFaces ToFaces(SParts s)      => SConvertUtils.GeomToFaces(s, s.entities);
        public static SFaces ToFaces(SBodies s)     => SConvertUtils.GeomToFaces(s, s.entities);
        public static SFaces ToFaces(SFaces s)      => s;
        public static SFaces ToFaces(SEdges s)      => SConvertUtils.GeomToFaces(s, s.entities);
        public static SFaces ToFaces(SVerts s)      => SConvertUtils.GeomToFaces(s, s.entities);
        public static SFaces ToFaces(SNodes s)      => SConvertUtils.MeshToGeoms<SFace, SFaces>(s);
        public static SFaces ToFaces(SElems s)      => SConvertUtils.MeshToGeoms<SFace, SFaces>(s);
        public static SFaces ToFaces(SElemFaces s)  => SConvertUtils.MeshToGeoms<SFace, SFaces>(s); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToEdges:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SEdges ToEdges(SParts s)      => SConvertUtils.GeomToEdges(s, s.entities);
        public static SEdges ToEdges(SBodies s)     => SConvertUtils.GeomToEdges(s, s.entities);
        public static SEdges ToEdges(SFaces s)      => SConvertUtils.GeomToEdges(s, s.entities);
        public static SEdges ToEdges(SEdges s)      => s;
        public static SEdges ToEdges(SVerts s)      => SConvertUtils.GeomToEdges(s, s.entities);
        public static SEdges ToEdges(SNodes s)      => SConvertUtils.MeshToGeoms<SEdge, SEdges>(s);
        public static SEdges ToEdges(SElems s)      => SConvertUtils.MeshToGeoms<SEdge, SEdges>(s);
        public static SEdges ToEdges(SElemFaces s)  => SConvertUtils.MeshToGeoms<SEdge, SEdges>(s); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToVerts:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SVerts ToVerts(SParts s)      => SConvertUtils.GeomToVerts(s, s.entities);
        public static SVerts ToVerts(SBodies s)     => SConvertUtils.GeomToVerts(s, s.entities);
        public static SVerts ToVerts(SFaces s)      => SConvertUtils.GeomToVerts(s, s.entities);
        public static SVerts ToVerts(SEdges s)      => SConvertUtils.GeomToVerts(s, s.entities);
        public static SVerts ToVerts(SVerts s)      => s;
        public static SVerts ToVerts(SNodes s)      => SConvertUtils.MeshToGeoms<SVert, SVerts>(s);
        public static SVerts ToVerts(SElems s)      => SConvertUtils.MeshToGeoms<SVert, SVerts>(s);
        public static SVerts ToVerts(SElemFaces s)  => SConvertUtils.MeshToGeoms<SVert, SVerts>(s);
        // -------------------------------------------------------------------------------------------
        //
        //      corners & mids:
        //
        // -------------------------------------------------------------------------------------------
        public static SNodes ToCorners(SElems s)    => SNew.NodesFromIds(s.em, s.iElems.SelectMany(ie => ie.CornerNodeIds)); 
        public static SNodes ToMids(SElems s)       => SNew.NodesFromIds(s.em, s.iElems.SelectMany(ie => ie.NodeIds.Where(id => !ie.CornerNodeIds.Contains(id)))); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToNodes:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SNodes ToNodes(SParts s)      => SConvertUtils.GeomToNodes(s, s.entities);
        public static SNodes ToNodes(SBodies s)     => SConvertUtils.GeomToNodes(s, s.entities);
        public static SNodes ToNodes(SFaces s)      => SConvertUtils.GeomToNodes(s, s.entities);
        public static SNodes ToNodes(SEdges s)      => SConvertUtils.GeomToNodes(s, s.entities);
        public static SNodes ToNodes(SVerts s)      => SConvertUtils.GeomToNodes(s, s.entities);
        public static SNodes ToNodes(SNodes s)      => s;
        public static SNodes ToNodes(SElems s)      => SNew.NodesFromIds(s.em, s.entities.SelectMany(x => x.iElem.NodeIds));
        public static SNodes ToNodes(SElemFaces s)  => SNew.NodesFromIds(s.em, s.entities.SelectMany(x => x.faceNodeIds)); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToElems:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SElems ToElems(SParts s)      => SConvertUtils.GeomToElems(s, s.entities);
        public static SElems ToElems(SBodies s)     => SConvertUtils.GeomToElems(s, s.entities);
        public static SElems ToElems(SFaces s)      => SConvertUtils.GeomToElems(s, s.entities);
        public static SElems ToElems(SEdges s)      => SConvertUtils.GeomToElems(s, s.entities);
        public static SElems ToElems(SVerts s)      => SConvertUtils.GeomToElems(s, s.entities);
        public static SElems ToElems(SNodes s)      => SNew.ElemsFromIds(s.em, s.entities.SelectMany(n => n.iNode.ConnectedElementIds));
        public static SElems ToElems(SElems s)      => s;
        public static SElems ToElems(SElemFaces s)  => SNew.ElemsFromIds(s.em, s.entities.SelectMany(x => x.ids)); 
        // -------------------------------------------------------------------------------------------
        //
        //      SConvertEntity.ToElemFaces:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SElemFaces ToElemFaces(SParts s)      => SConvertUtils.ToElemFaces(s.bodies);
        public static SElemFaces ToElemFaces(SBodies s)     => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFaces(SFaces s)      => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFaces(SEdges s)      => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFaces(SVerts s)      => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFaces(SNodes s)      => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFacesIn(SNodes s)    => SConvertUtils.ToElemFaces(s, cType: SConvertType.OnlyIfAllAttached);
        public static SElemFaces ToElemFaces(SElems s)      => SConvertUtils.ToElemFaces(s);
        public static SElemFaces ToElemFaces(SElemFaces s)  => s;  
    }
}
