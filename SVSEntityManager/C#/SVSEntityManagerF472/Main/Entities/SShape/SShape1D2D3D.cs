#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  

namespace SVSEntityManagerF472
{
    public static class SShape1D2D3D
    {
        //
        //  Ip1, Ip2, Ip3 ---> sorted
        //
        public static double GetMomentOfInertiaMax(SBody b) => __Get(b, 0);
        public static double GetMomentOfInertiaMid(SBody b) => __Get(b, 1);
        public static double GetMomentOfInertiaMin(SBody b) => __Get(b, 2);
        private static double __Get(SBody b, int index)
        {
            if (b                             == null) throw new Exception($"__Get(...): Null error: (SBody)b == null. ");
            if (b.nodeBody                    == null) throw new Exception($"__Get(...): Null error: b.nodeBody == null. ");
            if (b.nodeBody.MomentOfInertiaIp1 == null) throw new Exception($"__Get(...): Null error: b.nodeBody.MomentOfInertiaIp1 == null. ");
            if (b.nodeBody.MomentOfInertiaIp2 == null) throw new Exception($"__Get(...): Null error: b.nodeBody.MomentOfInertiaIp2 == null. ");
            if (b.nodeBody.MomentOfInertiaIp3 == null) throw new Exception($"__Get(...): Null error: b.nodeBody.MomentOfInertiaIp3 == null. ");
            //
            //  get:
            //
            List<double> I = new List<double>
            {
                b.em.ConvertMomentOfInertia(b.nodeBody.MomentOfInertiaIp1), // b.nodeBody.MomentOfInertiaIp1.Value,
                b.em.ConvertMomentOfInertia(b.nodeBody.MomentOfInertiaIp2), // b.nodeBody.MomentOfInertiaIp2.Value,
                b.em.ConvertMomentOfInertia(b.nodeBody.MomentOfInertiaIp3), // b.nodeBody.MomentOfInertiaIp3.Value
            };
            //
            //  sort:
            //
            I.Sort();
            I.Reverse();
            //
            //  return:
            //
            return I[index];
        }
        //
        //  1D ... (beam)  => 2 moments are larger
        //  index = 1.0 ... perfect 1D, 0.0 ... for perfect 2D or 3D
        //
        public static double GetShape1DIndex(SBody b) => (b.Ip2 / b.Ip1) * (1.0 - b.Ip3 / b.Ip1);// Math.Min(b.Ip2 / b.Ip1, 1.0 - b.Ip3 / b.Ip1);
        //
        //  2D ... (plate) => 1 moment is larger
        //  index = 1.0 ... perfect 2D, 0.0 ... for perfect 1D or 3D
        //
        public static double GetShape2DIndex(SBody b) => (b.Ip3 / b.Ip2) * (1.0 - b.Ip3 / b.Ip1); // Math.Min(b.Ip3 / b.Ip2, 1.0 - b.Ip3 / b.Ip1);
        //
        //  3D ... (solid) => 3 similar moments of inertia (principal)
        //  index = 1.0 ... perfect 3D, 0.0 ... for perfect 1D or 2D
        //
        public static double GetShape3DIndex(SBody b) => (b.Ip3 / b.Ip1) * (b.Ip2 / b.Ip1); // Math.Min(b.Ip3 / b.Ip1, b.Ip2 / b.Ip1);
    }
}
