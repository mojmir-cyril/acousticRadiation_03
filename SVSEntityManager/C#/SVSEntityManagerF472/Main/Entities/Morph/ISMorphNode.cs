#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVSEntityManagerF472 
{
    public interface ISMorphNode
    {
        int         nodeId   { get; } 
        double[]    xyz      { get; }  // original location
        double[]    nxyz     { get; }  // morphed location (current nodal location)
        double      dist     { get; }  // distance
    }
}
