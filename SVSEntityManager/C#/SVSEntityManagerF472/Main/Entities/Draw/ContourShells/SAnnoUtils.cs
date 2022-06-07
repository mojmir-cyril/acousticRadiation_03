#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using System.Collections.Generic;  
using Ansys.ACT.Interfaces.Mesh;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public abstract class SAnnoUtils : SLoggerBase
    {
        public SAnnoUtils(SEntityManager em) : base(em) { } 
        public static (int, int, int) Ids3(IEnumerable<SNode> nodes)
        {
            if (nodes.Count() != 3) throw new Exception($"Ids3(...): Count error: iNodes.Count() != 3. ");
            int[] ids = nodes.Select(n => n.id).OrderBy(id => id).ToArray();
            return (ids[0], ids[1], ids[2]);
        }
        public static (int, int, int) Ids3(IEnumerable<SAnnoPoint.SNode> nodes)
        {
            if (nodes.Count() != 3) throw new Exception($"Ids3(...): Count error: iNodes.Count() != 3. ");
            int[] ids = nodes.Select(i => i.id).OrderBy(id => id).ToArray();
            return (ids[0], ids[1], ids[2]);
        }
        public static (int, int, int) Ids3(IEnumerable<INode> iNodes)
        {
            if (iNodes.Count() != 3) throw new Exception($"Ids3(...): Count error: iNodes.Count() != 3. ");
            int[] ids = iNodes.Select(i => i.Id).OrderBy(id => id).ToArray();
            return (ids[0], ids[1], ids[2]);
        }
        public static (int, int) Ids2(IEnumerable<INode> iNodes)
        {
            if (iNodes.Count() != 2) throw new Exception($"Ids2(...): Count error: iNodes.Count() != 2. ");
            int[] ids = iNodes.Select(i => i.Id).OrderBy(id => id).ToArray();
            return (ids[0], ids[1]);
        }
        public static (int, int) Ids2(SAnnoPoint.SNode node1, SAnnoPoint.SNode node2)
        {  
            int id1 = node1.id < node2.id ? node1.id : node2.id;
            int id2 = node1.id < node2.id ? node2.id : node1.id; 
            return (id1, id2);
        }
    }
}
