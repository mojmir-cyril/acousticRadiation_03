#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq; 
using System.Runtime.Serialization;

using Ansys.ACT.Interfaces.Common; 
using Ansys.ACT.Mechanical;

namespace SVSEntityManagerF472
{
    public class SInfo : MechanicalSelectionInfo, ISelectionInfo
    {
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        public SInfo(ISelectionInfo info) : base(info, info.SelectionType) { }
        public SInfo(SelectionTypeEnum type) : base(type) { }
        public SInfo(ISelectionInfo info, SelectionTypeEnum type) : base(info, type) { }
        public SInfo(SerializationInfo info, StreamingContext context) : base(info, context) { } 
        // -------------------------------------------------------------------------------------------
        //
        //      NewInfo:
        //
        // -------------------------------------------------------------------------------------------
        public SInfo NewInfo(ISelectionInfo i) => new SInfo(i);
        public SInfo NewInfo()                 => new SInfo(SelectionTypeEnum.GeometryEntities);
        public SInfo NewInfo(ISEntities<SEntity> ents)
        {
            if      (ents.isEmpty)    return NewInfo();
            else if (ents.isGeom)     return NewGeomInfo(ents.ids);
            else if (ents.isNode)     return NewNodeInfo(ents.ids);
            else if (ents.isElem)     return NewElemInfo(ents.ids);
            else if (ents.isElemFace) return NewElemFaceInfo(ents.ids, ents.elemFaceIds); 
            else throw new Exception("NewInfo(...): unknown entity type. ");
        }  
        // -------------------------------------------------------------------------------------------
        //
        //      NewGeomInfo:
        //
        // ------------------------------------------------------------------------------------------- 
        public static SInfo NewGeomInfo()           => new SInfo(SelectionTypeEnum.GeometryEntities);
        public static SInfo NewGeomInfo(int refId)  => NewGeomInfo(new int[1] { refId });
        public static SInfo NewGeomInfo(IEnumerable<int> refIds)
        {
            SInfo info = NewGeomInfo();
            info.Ids = refIds.ToList();
            return info;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      NewNodeInfo:
        //
        // -------------------------------------------------------------------------------------------
        public static SInfo NewNodeInfo() => new SInfo(SelectionTypeEnum.MeshNodes);
        public static SInfo NewNodeInfo(int id) => NewNodeInfo(new int[1] { id });
        public static SInfo NewNodeInfo(IEnumerable<int> ids)
        {
            SInfo info = NewNodeInfo();
            info.Ids = ids.ToList();
            return info;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      NewElemInfo:
        //
        // -------------------------------------------------------------------------------------------
        public static SInfo NewElemInfo() => new SInfo(SelectionTypeEnum.MeshElements);
        public static SInfo NewElemInfo(int id) => NewElemInfo(new int[1] { id });
        public static SInfo NewElemInfo(IEnumerable<int> ids)
        {
            SInfo info = NewElemInfo();
            info.Ids = ids.ToList();
            return info;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      NewElemInfo:
        //
        // -------------------------------------------------------------------------------------------
        public static SInfo NewElemFaceInfo() => new SInfo(SelectionTypeEnum.MeshElementFaces);
        public static SInfo NewElemFaceInfo(int elemId, int faceId) => NewElemFaceInfo(new int[1] { elemId }, new int[1] { faceId });
        public static SInfo NewElemFaceInfo(IEnumerable<int> elemIds, IEnumerable<int> faceIds)
        {
            SInfo info = NewElemFaceInfo();
            info.Ids = elemIds.ToList();
            info.ElementFaceIndices = faceIds.ToList();
            return info;
        }
    }
}