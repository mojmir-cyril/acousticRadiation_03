#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
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


namespace SVSEntityManagerF472
{
    public static class SNormalUtils
    { 
        public static SNormal ToLocalPolar(SEntityManager em, SNormal nn, bool polarNormal = false)
        {
            //
            //  global + cartesian:
            //
            if (!polarNormal && em.coordinateSystem.IsGlobal) return nn;
            //
            //  to local CS or polar:
            //
            string gUnit = em.unitUtils.geomUnit;
            //
            //  centroid in global:
            //
            double x1 = 0.0; // Face.Centroid[0];
            double y1 = 0.0; // Face.Centroid[1];
            double z1 = 0.0; // iFace.Centroid[2];
            //
            //  delta:
            //
            if (polarNormal)
            {
                //
                //  dist:
                //
                double dist = Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
                //
                //  norm (scale) to small:
                //
                nn = nn.Norm(dist * 0.01);
                //
                //  focus point in global:
                //
                double x2 = x1 + nn.x;
                double y2 = y1 + nn.y;
                double z2 = z1 + nn.z;
                SPoint p1 = new SPoint(1, x1, y1, z1, gUnit).InPolar(em.math, em.coordinateSystem);
                SPoint p2 = new SPoint(2, x2, y2, z2, gUnit).InPolar(em.math, em.coordinateSystem);
                //
                //  nearest angle:
                //
                Func<double, double> A = (a) => Math.Abs(a);
                p2.y = A(p2.y + Math.PI * 2.0 - p1.y) < A(p2.y - p1.y) ? (p2.y + Math.PI * 2.0) : p2.y;
                p2.y = A(p2.y - Math.PI * 2.0 - p1.y) < A(p2.y - p1.y) ? (p2.y - Math.PI * 2.0) : p2.y;
                nn = new SNormal(p1, p2);
                nn.y *= p1.x; // rad --> circumference
                nn = nn.Norm(1);
            }
            else
            {
                //
                //  focus point in global:
                //
                double x2 = x1 + nn.x;
                double y2 = y1 + nn.y;
                double z2 = z1 + nn.z;
                SPoint p1 = new SPoint(1, x1, y1, z1, gUnit).InLocal(em.math, em.coordinateSystem);
                SPoint p2 = new SPoint(2, x2, y2, z2, gUnit).InLocal(em.math, em.coordinateSystem);
                nn = new SNormal(p1, p2).Norm(1);
            }
            //   
            //  return:   
            //
            return nn;
        }
    }
}
