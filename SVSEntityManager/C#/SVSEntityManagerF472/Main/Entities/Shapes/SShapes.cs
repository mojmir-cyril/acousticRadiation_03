#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member
#pragma warning disable PropertyDocumentationHeader     // The property must have a documentation header.
#pragma warning disable ConstructorDocumentationHeader  // The constructor must have a documentation header.
#pragma warning disable MethodDocumentationHeader       // The method must have a documentation header.
#pragma warning disable ClassDocumentationHeader        // The class must have a documentation header.
#pragma warning disable EnumDocumentationHeader         // The enum must have a documentation header.


//
//  Ansys:
//
using Ansys.ACT.Interfaces.Mesh;

namespace SVSEntityManagerF472
{
    public static class SShapes
    { 
        public class SHex20 : SFaceShapeData
        {
            // 3-D 20-Node Structural Solid
            // face 1 (J-I-L-K), face 2 (I-J-N-M), face 3 (J-K-O-N), face 4 (K-L-P-O), face 5 (L-I-M-P), face 6 (M-N-O-P) 
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kHex20; }  
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(0, 1, 5, 4,  8, 16, 17, 12),      // 0: [0, 1, 5, 4, 8, 16, 17, 12 ]
                                                                                      new SFaceIndexes(2, 1, 5, 6,  9, 18, 17, 13),      // 1: [2, 1, 5, 6, 9, 18, 17, 13 ]
                                                                                      new SFaceIndexes(3, 2, 6, 7, 10, 19, 18, 14),      // 2: [3, 2, 6, 7, 10, 19, 18, 14]
                                                                                      new SFaceIndexes(0, 3, 7, 4, 11, 16, 19, 15),      // 3: [0, 3, 7, 4, 11, 16, 19, 15]
                                                                                      new SFaceIndexes(0, 3, 2, 1, 11,  8, 10,  9),      // 4: [0, 3, 2, 1, 11, 8, 10, 9  ]
                                                                                      new SFaceIndexes(5, 6, 7, 4, 13, 12, 14, 15) }; }  // 5: [5, 6, 7, 4, 13, 12, 14, 15]
            public SHex20(IElement elem, int faceId) : base(elem, faceId) { }  
        }
        public class SHex8 : SFaceShapeData
        {
            // 3-D 8-Node Structural Solid
            // face 1 (J-I-L-K), face 2 (I-J-N-M), face 3 (J-K-O-N), face 4 (K-L-P-O), face 5 (L-I-M-P), face 6 (M-N-O-P) 
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kHex8; }  
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(0, 1, 5, 4),      // 0: [0, 1, 5, 4]
                                                                                      new SFaceIndexes(2, 1, 5, 6),      // 1: [2, 1, 5, 6]
                                                                                      new SFaceIndexes(3, 2, 6, 7),      // 2: [3, 2, 6, 7]
                                                                                      new SFaceIndexes(0, 3, 7, 4),      // 3: [0, 3, 7, 4]
                                                                                      new SFaceIndexes(0, 3, 2, 1),      // 4: [0, 3, 2, 1]
                                                                                      new SFaceIndexes(5, 6, 7, 4) }; }  // 5: [5, 6, 7, 4]
            public SHex8(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class SPyramid13 : SFaceShapeData
        { 
            public override ElementTypeEnum   elemShape { get => ElementTypeEnum.kPyramid13; }
            public override SFaceIndexes[]    faces     { get => new SFaceIndexes[] { new SFaceIndexes(1, 0, 3, 2, 5, 6, 8, 7),      //  0: [1, 0, 3, 2, 5, 6, 8, 7]
                                                                                      new SFaceIndexes(4, 10, 9, 1, 0, 5),           //  1: [4, 10, 9, 1, 0, 5]
                                                                                      new SFaceIndexes(4, 10, 11, 1, 2, 6),          //  2: [4, 10, 11, 1, 2, 6]
                                                                                      new SFaceIndexes(4, 12, 11, 3, 2, 7),          //  3: [4, 12, 11, 3, 2, 7]
                                                                                      new SFaceIndexes(4, 9, 12, 0, 3, 8) }; }       //  4: [4, 9, 12, 0, 3, 8]
            public SPyramid13(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class SPyramid5 : SFaceShapeData
        { 
            public override ElementTypeEnum   elemShape { get => ElementTypeEnum.kPyramid5; }
            public override SFaceIndexes[]    faces     { get => new SFaceIndexes[] { new SFaceIndexes(0, 3, 1, 2),        //  0: [0, 3, 1, 2]
                                                                                      new SFaceIndexes(4, 0, 1),           //  1: [4, 0, 1]
                                                                                      new SFaceIndexes(4, 1, 2),           //  2: [4, 1, 2]
                                                                                      new SFaceIndexes(4, 3, 2),           //  3: [4, 3, 2]
                                                                                      new SFaceIndexes(4, 0, 3) }; }       //  4: [4, 0, 3]
            public SPyramid5(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class SQuad4 : SFaceShapeData
        {
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kQuad4; }  
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(1, 0, 2, 3) }; }
            public SQuad4(IElement elem, int faceId) : base(elem, faceId) { }  
        }
        public class SQuad8 : SFaceShapeData
        {
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kQuad8; }  
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(3, 2, 0, 1, 6, 7, 5, 4) }; }
            public SQuad8(IElement elem, int faceId) : base(elem, faceId) { }  
        }
        public class STet10 : SFaceShapeData
        {
            // 3-D 10-Node Tetrahedral Structural Solid
            // face 1 (J-I-K), face 2 (I-J-L), face 3 (J-K-L), face 4 (K-I-L)
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kTet10; }  
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(0, 3, 1, 7, 4, 8),      //  0: [0, 3, 1, 7, 4, 8]
                                                                                      new SFaceIndexes(3, 2, 1, 9, 8, 5),      //  1: [3, 2, 1, 9, 8, 5]
                                                                                      new SFaceIndexes(0, 3, 2, 7, 6, 9),      //  2: [0, 3, 2, 7, 6, 9]
                                                                                      new SFaceIndexes(0, 2, 1, 6, 4, 5) }; }  //  3: [0, 2, 1, 6, 4, 5]
            public STet10(IElement elem, int faceId) : base(elem, faceId) { }  
        }
        public class STet4 : SFaceShapeData
        {
            // 3-D 4-Node Tetrahedral Structural Solid
            // face 1 (J-I-K), face 2 (I-J-L), face 3 (J-K-L), face 4 (K-I-L)
            public override ElementTypeEnum   elemShape { get => ElementTypeEnum.kTet4; }
            public override SFaceIndexes[]    faces     { get => new SFaceIndexes[] { new SFaceIndexes(0, 3, 1),      //  0: [0, 3, 1]
                                                                                      new SFaceIndexes(3, 2, 1),      //  1: [3, 2, 1]
                                                                                      new SFaceIndexes(0, 3, 2),      //  2: [0, 3, 2]
                                                                                      new SFaceIndexes(0, 2, 1) }; }  //  3: [0, 2, 1]
            public STet4(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class STri3 : SFaceShapeData
        {
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kTri3; }
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(1, 2, 0) }; }
            public STri3(IElement elem, int faceId) : base(elem, faceId) { }
        } 
        public class STri6 : SFaceShapeData
        {
            public override ElementTypeEnum  elemShape  { get => ElementTypeEnum.kTri6; }
            public override SFaceIndexes[]   faces      { get => new SFaceIndexes[] { new SFaceIndexes(1, 2, 0, 4, 3, 5) }; }
            public STri6(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class SWedge15 : SFaceShapeData
        { 
            public override ElementTypeEnum   elemShape { get => ElementTypeEnum.kWedge15; }
            public override SFaceIndexes[]    faces     { get => new SFaceIndexes[] { new SFaceIndexes(0, 2, 1,  8, 6,  7),              //  0: [0, 2, 1, 8, 6, 7]
                                                                                      new SFaceIndexes(4, 5, 3, 10, 9, 11),              //  1: [4, 5, 3, 10, 9, 11]
                                                                                      new SFaceIndexes(0, 1, 4,  3, 6, 12, 13,  9),      //  2: [0, 1, 4, 3, 6, 12, 13, 9]
                                                                                      new SFaceIndexes(2, 1, 4,  5, 7, 14, 13, 10),      //  3: [2, 1, 4, 5, 7, 14, 13, 10]
                                                                                      new SFaceIndexes(0, 2, 5,  3, 8, 12, 14, 11) }; }  //  4: [0, 2, 5, 3, 8, 12, 14, 11]
            public SWedge15(IElement elem, int faceId) : base(elem, faceId) { }
        }
        public class SWedge6 : SFaceShapeData
        { 
            public override ElementTypeEnum   elemShape { get => ElementTypeEnum.kWedge6; }
            public override SFaceIndexes[]    faces     { get => new SFaceIndexes[] { new SFaceIndexes(0, 2, 1),              //  0: [0, 2, 1]
                                                                                      new SFaceIndexes(4, 5, 3),              //  1: [4, 5, 3]
                                                                                      new SFaceIndexes(0, 1, 4, 3),           //  2: [0, 1, 4, 3]
                                                                                      new SFaceIndexes(2, 1, 4, 5),           //  3: [2, 1, 4, 5]
                                                                                      new SFaceIndexes(0, 2, 5, 3) }; }       //  4: [0, 2, 5, 3]
            public SWedge6(IElement elem, int faceId) : base(elem, faceId) { }
        }
    }
}
// kUnknown = -1,
// kPoint0 = 0,
// kLine2 = 1,
// kLine3 = 2,
// kBeam3 = 3,
// kBeam4 = 4,
// kTri3 = 5,
// kTri6 = 6,
// kQuad4 = 7,
// kQuad8 = 8,
// kTet4 = 9,
// kTet10 = 10,
// kHex8 = 11,
// kHex20 = 12,
// kWedge6 = 13,
// kWedge15 = 14,
// kPyramid5 = 15,
// kPyramid13 = 16

//
//  model: elemShapes
//
//   f = DataModel.Project.Model.NamedSelections.Children[0]
//   n = DataModel.Project.Model.NamedSelections.Children[1]
//   
//   mesh = ExtAPI.DataModel.MeshDataByName("Global")
//   ids = [10, 8, 2, 6, 9, 1, 5, 3]   # ... solids
//   ids = [38, 48, 49, 50]            # ... shells
//   elems = [mesh.ElementById(id) for id in ids]
//   
//   for e in elems:
//       print e.Type
//       print[id for id in e.NodeIds] 
//       for face in range(8):
//           try:
//               i = ExtAPI.SelectionManager.CreateSelectionInfo(SelectionTypeEnum.MeshElementFaces)
//               i.Ids = [e.Id]
//               i.ElementFaceIndices = [face]
//               f.Location = i
//               n.Generate()
//               if len(n.Location.Ids) <= 0: continue
//               print "{}: {}".format(face, [list(e.NodeIds).index(id) for id in n.Location.Ids])
//                   except: pass
// 
// kWedge15
// [83, 85, 84, 88, 86, 87, 90, 92, 89, 96, 95, 97, 91, 94, 93]
// 0: [0, 2, 1, 8, 6, 7]
// 1: [4, 5, 3, 10, 9, 11]
// 2: [0, 1, 4, 3, 6, 12, 13, 9]
// 3: [2, 1, 4, 5, 7, 14, 13, 10]
// 4: [0, 2, 5, 3, 8, 12, 14, 11]
// kHex20
// [55, 58, 57, 56, 62, 59, 60, 61, 64, 68, 66, 63, 72, 71, 73, 74, 65, 70, 69, 67]
// 0: [0, 1, 5, 4, 8, 16, 17, 12]
// 1: [2, 1, 5, 6, 9, 18, 17, 13]
// 2: [3, 2, 6, 7, 10, 19, 18, 14]
// 3: [0, 3, 7, 4, 11, 16, 19, 15]
// 4: [0, 3, 2, 1, 11, 8, 10, 9]
// 5: [5, 6, 7, 4, 13, 12, 14, 15]
// kTet10
// [5, 8, 7, 6, 11, 14, 10, 9, 13, 12]
// 0: [0, 3, 1, 7, 4, 8]
// 1: [3, 2, 1, 9, 8, 5]
// 2: [0, 3, 2, 7, 6, 9]
// 3: [0, 2, 1, 6, 4, 5]
// kPyramid13
// [33, 32, 35, 34, 38, 44, 45, 49, 47, 48, 46, 53, 51]
// 0: [1, 0, 3, 2, 5, 6, 8, 7]
// 1: [4, 10, 9, 1, 0, 5]
// 2: [4, 10, 11, 1, 2, 6]
// 3: [4, 12, 11, 3, 2, 7]
// 4: [4, 9, 12, 0, 3, 8]
// kHex8
// [75, 78, 77, 76, 82, 79, 80, 81]
// 0: [0, 1, 5, 4]
// 1: [2, 1, 5, 6]
// 2: [3, 2, 6, 7]
// 3: [0, 3, 7, 4]
// 4: [0, 3, 2, 1]
// 5: [5, 6, 7, 4]
// kTet4
// [1, 4, 3, 2]
// 0: [0, 3, 1]
// 1: [3, 2, 1]
// 2: [0, 3, 2]
// 3: [0, 2, 1]
// kWedge6
// [24, 26, 25, 29, 27, 28]
// 0: [0, 2, 1]
// 1: [4, 5, 3]
// 2: [0, 1, 4, 3]
// 3: [2, 1, 4, 5]
// 4: [0, 2, 5, 3]
// kPyramid5
// [17, 19, 20, 18, 23]
// 0: [0, 3, 1, 2]
// 1: [4, 0, 1]
// 2: [4, 1, 2]
// 3: [4, 3, 2]
// 4: [4, 0, 3]
// 
// ------------------------------------------------------------------------------------------
// 
// kQuad4
// [51, 50, 52, 53]
// 0: [1, 0, 2, 3]
// 1: [1, 0, 2, 3]
// kTri6
// [75, 73, 74, 77, 76, 78]
// 0: [1, 2, 0, 4, 3, 5]
// 1: [1, 2, 0, 4, 3, 5]
// kTri3
// [81, 79, 80]
// 0: [1, 2, 0]
// 1: [1, 2, 0]
// kQuad8
// [84, 85, 83, 82, 89, 88, 86, 87]
// 0: [3, 2, 0, 1, 6, 7, 5, 4]
// 1: [3, 2, 0, 1, 6, 7, 5, 4]
// 




// string faceShapeType = "N/A";
// string elemType = elem.Type.ToString();
// Dictionary<int, string> faceShapeTypes;
// switch (elemType)
// {
//     case "kHex20": faceShapeTypes = new Dictionary<int, string>() { { 1, "8-node face" }, { 2, "8-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "8-node face" }, { 6, "8-node face" } }; break;
//     case "kTet10": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "6-node face" }, { 3, "6-node face" }, { 4, "6-node face" } }; break;
//     // case "kWedge15": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "8-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "6-node face" } }; break;
//     case "kWedge15": faceShapeTypes = new Dictionary<int, string>() { { 1, "6-node face" }, { 2, "6-node face" }, { 3, "8-node face" }, { 4, "8-node face" }, { 5, "8-node face" } }; break;
//     case "kHex8": faceShapeTypes = new Dictionary<int, string>() { { 1, "4-node face" }, { 2, "4-node face" }, { 3, "4-node face" }, { 4, "4-node face" }, { 5, "4-node face" }, { 6, "4-node face" } }; break;
//     case "kTet4": faceShapeTypes = new Dictionary<int, string>() { { 1, "3-node face" }, { 2, "3-node face" }, { 3, "3-node face" }, { 4, "3-node face" } }; break;
//     case "kWedge6": faceShapeTypes = new Dictionary<int, string>() { { 1, "3-node face" }, { 2, "4-node face" }, { 3, "4-node face" }, { 4, "4-node face" }, { 3, "3-node face" } }; break;
//     default: throw new Exception("SMappingSimpleSurface.GetShellDatas(...): switch (elemType), elemType = " + elemType.ToString());
// }
// try { faceShapeType = faceShapeTypes[elemFaceId + 1]; }  // indexovani od 0 => 1 !!
// catch (Exception e) { throw new Exception("SMappingSimpleSurface.GetShellDatas(...): faceShapeTypes[this.elemFaceId + 1], this.elemFaceId = " + this.elemFaceId.ToString() + ", Exception: " + e.ToString()); }
// // 
// //  check:
// // 
// if (Convert.ToInt32(faceShapeType.Substring(0, 2).Replace("-", "")) != faceShape)
//     throw new Exception($"SMappingSimpleSurface.GetShellDatas(...): \n " +
//                         $"faceShapeType = {faceShapeType} != faceShape = {faceShape}  \n" +
//                         $"for elem.Id = {elem.Id}, elemType = {elemType}, elemFaceId = {elemFaceId}. ");
// //
// //  return:
// //
// return faceShapeType;

// public (List<double>, List<double>, List<int>) GetShellDatas(Action<object> Msg, IMeshData mesh)
// {
//     elem = mesh.ElementById(elementId);
//     //
//     //  check:
//     //
//     if (elem == null) throw new Exception($"GetShellDatas(...): Element cannot be obtained from id = {elementId}. ");
//     //
//     //  type:
//     //
//     string faceShapeType = __GetFaceShapeType();
//     switch (faceShapeType)
//     {
//         case "8-node face":
//             this.connectivity = new List<int>() { 1, 4, 5, 4, 0, 7, 7, 6, 4, 4, 6, 5, 7, 3, 6, 6, 2, 5 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 7, 4), NewCon(1, 4, 5), NewCon(2, 5, 6), NewCon(3, 6, 7), NewCon(4, 7, 5), NewCon(5, 4, 6), NewCon(6, 5, 7), NewCon(7, 6, 4) };
//             break;
//         case "6-node face":
//             this.connectivity = new List<int>() { 1, 3, 4, 3, 0, 5, 5, 2, 4, 3, 5, 4 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 5, 3), NewCon(1, 3, 4), NewCon(2, 4, 5), NewCon(3, 5, 4), NewCon(4, 3, 5), NewCon(5, 4, 3) };
//             break;
//         case "4-node face":
//             this.connectivity = new List<int>() { 1, 0, 3, 3, 2, 1 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 2, 1), NewCon(1, 0, 2), NewCon(2, 1, 3), NewCon(3, 2, 1) };
//             break;
//         case "3-node face":
//             this.connectivity = new List<int>() { 1, 0, 2 };
//             this.normalIndexes = new List<List<int>>() { NewCon(0, 2, 1), NewCon(1, 0, 2), NewCon(2, 1, 0) };
//             break;
//         default: throw new Exception("SMappingSimpleSurface.GetShellDatas(...): switch (faceShapeType), faceShapeType = " + faceShapeType.ToString());
//     }
//     //
//     //  Vysledne vertices
//     //
//     this.vertices = new List<double>();
//     //
//     //  Id uzlu jednoho faceElem
//     // 
//     List<INode> nodes = (from id in this.faceNodeIds select mesh.NodeById(id)).ToList();
//     foreach (INode n in nodes)
//     {
//         this.vertices.Add(n.X);
//         this.vertices.Add(n.Y);
//         this.vertices.Add(n.Z);
//     }
//     //
//     //  Normals:
//     //
//     this.normals = new List<double>();
//     foreach (List<int> n123 in this.normalIndexes)
//     {
//         INode n1 = mesh.NodeById(this.faceNodeIds[n123[0]]);
//         INode n2 = mesh.NodeById(this.faceNodeIds[n123[1]]);
//         INode n3 = mesh.NodeById(this.faceNodeIds[n123[2]]);
//         this.normals.AddRange(Normal(n1, n2, n3));
//     }
//     return (this.vertices, this.normals, this.connectivity);
// }