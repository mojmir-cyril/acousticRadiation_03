#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member



using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;
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
using System.Runtime.CompilerServices;

namespace SVSEntityManagerF472
{
    public class SPoint  
    {
        public int      id       { get; set; }
        public double   x        { get; set; }
        public double   y        { get; set; }
        public double   z        { get; set; }
        public object   parent   { get; set; }  // obecny objekt (pouziva se pro zapamatovani SMappingFaceElement)
        public double   length   { get { return Math.Sqrt(x * x + y * y + z * z); } }
        //
        //  fixed units:
        //
        public string lengthUnit = SUnitsUtils.internalLengthUnit;
        // --------------------------------------------------------------------------------------------------------
        // 
        //    ctor:
        //
        // --------------------------------------------------------------------------------------------------------
        public SPoint() { }
        public SPoint(SPoint p)                                           => Set(p.id, p.x, p.y, p.z, p.parent, p.lengthUnit);  // copy
        public SPoint(int Id, double X, double Y, double Z, string unit)  => Set(Id, X, Y, Z, unit);
        public SPoint(INode node, SUnitsUtils units)                      => Set(node.Id, node.X, node.Y, node.Z, node, units.meshUnit);
        public SPoint(IList<double> l, string unit)                       => Set(0, l[0], l[1], l[2], unit);
        public SPoint(double[] l, string unit)                            => Set(0, l[0], l[1], l[2], unit);
        public SPoint(IGeoVertex v, string unit)                          => Set(v.Id, v.X, v.Y, v.Z, unit);
        public SPoint(int Id, double X, double Y, double Z, object parent, string unit)
        {
            Set(Id, X, Y, Z, unit);
            this.parent = parent;  // obecny objekt (pouziva se pro zapamatovani SMappingFaceElement)
        } 
        // ---------------------------------------------------------------------------------------------
        //
        //   Methods:
        //
        // ---------------------------------------------------------------------------------------------
        public static List<SPoint> NewList() => new List<SPoint>();       // pro ucel Pythonu:   points = Mapping.SPoint.NewList()
        public override string ToString()    => $"EntityManager.SPoint(id = {id}, x = {x}, y = {y}, z = {z}, lengthUnit = {lengthUnit})";
        // ---------------------------------------------------------------------------------------------
        //
        //   Set:
        //
        // ---------------------------------------------------------------------------------------------
        public SPoint Set(SPoint p) 
        {
            id     = p.id;
            x      = p.x;
            y      = p.y;
            z      = p.z;
            parent = p.parent; 
            return this;
        }
        public SPoint Set(double X, double Y, double Z)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            return this;
        }
        public SPoint Set(int Id, double X, double Y, double Z)
        {
            this.id = Id;
            return Set(X, Y, Z);
        }
        public SPoint Set(int Id, double X, double Y, double Z, string unit)
        {
            this.id         = Id;
            this.lengthUnit = unit;
            return Set(X, Y, Z);
        }
        public SPoint Set(int Id, double X, double Y, double Z, object parent)
        {
            this.id     = Id;
            this.parent = parent;
            return Set(X, Y, Z);
        }
        public SPoint Set(int Id, double X, double Y, double Z, object parent, string unit)
        {
            this.parent = parent;
            return Set(Id, X, Y, Z, unit);
        }
        public SPoint Set(double val, SDir dir)
        {
            switch (dir)
            {
                case SDir.x: x = val; break;
                case SDir.y: y = val; break;
                case SDir.z: z = val; break;
            }
            return this;
        }
        public SPoint Set(int id, object p)
        {
            this.id = id;
            parent = p;
            return this;
        }
        public double GetCoord(SDir dir)
        {
            switch (dir)
            {
                case SDir.x: return x;
                case SDir.y: return y;
                case SDir.z: return z;
            }
            throw new Exception("SPoint.__GetCoord(): Wrong direction (dir). ");
        }
        public double GetCoord(int dir)
        {
            switch (dir)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
            }
            throw new Exception($"SPoint.__GetCoord(): Wrong direction (dir = '{dir}'). ");
        }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Dist:
        //
        // --------------------------------------------------------------------------------------------------------
        public SLength Dist(INode n, SUnitsUtils units) => Dist(new SPoint(n, units));
        public SLength Dist(SPoint p2)
        {
            (double x1, double y1, double z1) = CoordsInUnit("m");
            (double x2, double y2, double z2) = p2.CoordsInUnit("m");
            SLength d = new SLength(Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2)), "m");
            return (SLength)d.ConvertUnit(lengthUnit);
        }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    CoordsSLengths:
        //
        // --------------------------------------------------------------------------------------------------------
        public (SLength, SLength, SLength) CoordsSLengths() => (new SLength(x, lengthUnit), new SLength(y, lengthUnit), new SLength(z, lengthUnit));
        // --------------------------------------------------------------------------------------------------------
        // 
        //    CoordsInUnit:
        //
        // --------------------------------------------------------------------------------------------------------
        public (double, double, double) CoordsInUnit(string unit)
        {
            Func<double, double> C = (v) => new SLength(v, lengthUnit).ConvertUnit(unit).Value;
            return (C(x), C(y), C(z));
        }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Scale:
        //
        // --------------------------------------------------------------------------------------------------------
        public SPoint Scale(double scale)
        {
            x = x * scale;
            y = y * scale;
            z = z * scale;
            return this;
        }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Unit:
        //
        // --------------------------------------------------------------------------------------------------------
        public SPoint ToMeshUnit(SUnitsUtils units)
        {
            double s = new SLength(1, lengthUnit).ConvertUnit(units.meshUnit).Value;
            lengthUnit = units.meshUnit;
            return new SPoint(this).Scale(s);
        }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Operators:
        //
        // --------------------------------------------------------------------------------------------------------
        public static SPoint operator +(SPoint a, SNormal n) => new SPoint(a.id, a.x + n.x, a.y + n.y, a.z + n.z, a.parent, a.lengthUnit);
        public static SPoint operator +(SPoint a, SPoint b)
        {
            if (a.lengthUnit == b.lengthUnit) return new SPoint(a.id, a.x + b.x, a.y + b.y, a.z + b.z, a.parent, a.lengthUnit);
            double x = (new SLength(a.x, a.lengthUnit) + new SLength(b.x, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            double y = (new SLength(a.y, a.lengthUnit) + new SLength(b.y, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            double z = (new SLength(a.z, a.lengthUnit) + new SLength(b.z, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            return new SPoint(a.id, x, y, z, a.parent, a.lengthUnit);
        }
        public static SPoint operator -(SPoint a, SPoint b)
        {
            if (a.lengthUnit == b.lengthUnit) return new SPoint(a.id, a.x - b.x, a.y - b.y, a.z - b.z, a.parent, a.lengthUnit);
            double x = (new SLength(a.x, a.lengthUnit) - new SLength(b.x, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            double y = (new SLength(a.y, a.lengthUnit) - new SLength(b.y, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            double z = (new SLength(a.z, a.lengthUnit) - new SLength(b.z, b.lengthUnit)).ConvertUnit(a.lengthUnit).Value;
            return new SPoint(a.id, x, y, z, a.parent, a.lengthUnit);  
        }
        public static SPoint operator *(SPoint a, double scale)     => new SPoint(a.id, a.x * scale, a.y * scale, a.z * scale, a.parent, a.lengthUnit);
        public static SPoint operator *(double scale, SPoint a)     => new SPoint(a.id, a.x* scale, a.y* scale, a.z* scale, a.parent, a.lengthUnit);
        public static bool operator ==(SPoint obj1, SPoint obj2)    => __Same(obj1, obj2);
        public static bool operator !=(SPoint obj1, SPoint obj2)    => !__Same(obj1, obj2);
        private static bool __Same(SPoint obj1, SPoint obj2)
        {
            if (obj1 is null && obj2 is null) return true;
            if (obj1 is null) return false;
            if (obj2 is null) return false;
            return (obj1.x == obj2.x && obj1.y == obj2.y && obj1.z == obj2.z && obj1.id == obj2.id);
        }
        public override bool Equals(object o)
        {
            if (this is null && o is null) return true;
            if (this is null) return false;
            if (!(o is SPoint)) return false;
            return __Same(this, (SPoint)o);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      GetHashCode:
        //
        // ------------------------------------------------------------------------------------------- 
        public override int GetHashCode() => base.GetHashCode();
        // -------------------------------------------------------------------------------------------
        //
        //      Transform:
        //
        // -------------------------------------------------------------------------------------------  
        internal SPoint InLocal(SMath math, CoordinateSystem cs) => math.ToLocal(this, cs);
        internal SPoint InPolar(SMath math, CoordinateSystem cs) => math.ToPolar(InLocal(math, cs));
    }
}
