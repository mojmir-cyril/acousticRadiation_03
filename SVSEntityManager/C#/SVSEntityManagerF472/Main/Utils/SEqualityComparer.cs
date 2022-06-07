using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

//
//  Ansys:
//
using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Geometry;



namespace SVSEntityManagerF472
{
    class SEqualityComparerSElemFace : IEqualityComparer<SElemFace>
    {
        bool IEqualityComparer<SElemFace>.Equals(SElemFace item1, SElemFace item2) { return item1.id == item2.id && item1.elemFaceId == item2.elemFaceId; }
        public int GetHashCode(SElemFace obj) { return $"{obj.id}-{obj.elemFaceId}".GetHashCode(); }
    }
    class SEqualityComparerINode : IEqualityComparer<INode>
    {
        bool IEqualityComparer<INode>.Equals(INode item1, INode item2) { return item1.Id == item2.Id; }
        public int GetHashCode(INode obj) { return obj.Id.GetHashCode(); }
    }
    class SEqualityComparerIElement : IEqualityComparer<IElement>
    {
        bool IEqualityComparer<IElement>.Equals(IElement item1, IElement item2) { return item1.Id == item2.Id; }
        public int GetHashCode(IElement obj) { return obj.Id.GetHashCode(); }
    }
    class SEqualityComparerIGeoFace : IEqualityComparer<IGeoFace>
    {
        bool IEqualityComparer<IGeoFace>.Equals(IGeoFace item1, IGeoFace item2) { return item1.Id == item2.Id; }
        public int GetHashCode(IGeoFace obj) { return obj.Id.GetHashCode(); }
    }
    class SEqualityComparerSEntity : IEqualityComparer<SEntity>
    {
        bool IEqualityComparer<SEntity>.Equals(SEntity item1, SEntity item2) { return item1.id == item2.id && item1.elemFaceId == item2.elemFaceId; }
        public int GetHashCode(SEntity obj) { return obj.id.GetHashCode(); }
    }
    class SEqualityComparerSEntity<TEnt> : IEqualityComparer<TEnt> where TEnt : SEntity
    {
        bool IEqualityComparer<TEnt>.Equals(TEnt item1, TEnt item2) { return item1.id == item2.id && item1.elemFaceId == item2.elemFaceId; }
        public int GetHashCode(TEnt obj) { return obj.id.GetHashCode(); }
    }
    // class SEqualityComparerSWire : IEqualityComparer<SWire>
    // {
    //     bool IEqualityComparer<SWire>.Equals(SWire w1, SWire w2)
    //     {
    //         return (w1.point1.id == w2.point1.id && w1.point2.id == w2.point2.id) ||
    //                (w1.point1.id == w2.point2.id && w1.point2.id == w2.point1.id);
    //     }
    //     public int GetHashCode(SWire obj) { return base.GetHashCode(); }
    // }
}
