#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using Ansys.ACT.Interfaces.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVSEntityManagerF472
{
    public abstract class SVectorUtils
    {  
        public static List<double> CrossProduct(IList<double> vec1, IList<double> vec2) => new List<double>() { vec1[1] * vec2[2] - vec1[2] * vec2[1], vec2[0] * vec1[2] - vec2[2] * vec1[0], vec1[0] * vec2[1] - vec1[1] * vec2[0] };
        public static List<double> Point(INode node) => new List<double>() { node.X, node.Y, node.Z };
        public static List<double> Normal(INode n1, INode n2, INode n3) => Normalize(CrossProduct(Vector(Point(n1), Point(n3)), Vector(Point(n1), Point(n2))));
        public static List<double> NormalOrig(INode n1, INode n2, INode n3) => CrossProduct(Vector(Point(n1), Point(n3)), Vector(Point(n1), Point(n2)));
        public static List<double> Add(IEnumerable<double> v1, IEnumerable<double> v2) => v1.Zip(v2, (i1, i2) => i1 + i2).ToList();
        public static List<double> Substract(IEnumerable<double> v1, IEnumerable<double> v2) => v1.Zip(v2, (i1, i2) => i1 - i2).ToList();
        public static List<double> Scale(IEnumerable<double> v, double c) => v.Select(i => i * c).ToList();
        public static double Dist(IEnumerable<double> v1, IEnumerable<double> v2) => SRSS(Substract(v1, v2));
        public static double SRSS(IEnumerable<double> vec) => Math.Sqrt(vec.Sum(v => Math.Pow(v, 2)));
        public static double TriangleArea(INode n1, INode n2, INode n3) => SRSS(CrossProduct(Vector(Point(n1), Point(n3)), Vector(Point(n1), Point(n2)))) / 2;

        public static List<double> Normalize(IEnumerable<double> vec)
        {
            double l = SRSS(vec);
            return (from v in vec select v / l).ToList();
        }
        public static List<double> Vector(IEnumerable<double> P1, IEnumerable<double> P2)
        {
            List<double> ret = new List<double>();
            for (int i = 0; i < P1.Count(); i++) ret.Add(P2.ToArray()[i] - P1.ToArray()[i]);
            return ret;
        } 
    }
}
