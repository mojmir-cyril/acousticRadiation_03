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
using System.Linq;

namespace SVSEntityManagerF472
{
    public class SNormal
    {
        public double       x        { get; set; }
        public double       y        { get; set; }
        public double       z        { get; set; }
        public double[]     xyz      { get => new double[3] { x, y, z }; }
        public double       length   { get { return Math.Sqrt(x * x + y * y + z * z); } }
        // --------------------------------------------------------------------------------------------------------
        // 
        //    ctor:
        //
        // --------------------------------------------------------------------------------------------------------
        public SNormal() { }
        public SNormal(SNormal n)                      => Set(n.x, n.y, n.z);  // copy
        public SNormal(double X, double Y, double Z)   => Set(X, Y, Z);
        public SNormal(SPoint p1, SPoint p2)           => Set(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
        public SNormal(INode n1, INode n2)             => Set(n2.X - n1.X, n2.Y - n1.Y, n2.Z - n1.Z);
        public SNormal(IList<double> l)                => Set(l[0], l[1], l[2]);
        public SNormal(double[] l)                     => Set(l[0], l[1], l[2]);
        public SNormal(IGeoVertex v1, IGeoVertex v2)   => Set(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z); 
        // ---------------------------------------------------------------------------------------------
        //
        //   Methods:
        //
        // --------------------------------------------------------------------------------------------- 
        public override string ToString() => $"EntityManager.SNormal(x = {x}, y = {y}, z = {z}, length = {length})";
        // ---------------------------------------------------------------------------------------------
        //
        //   Norm:
        //
        // ---------------------------------------------------------------------------------------------
        public SNormal Norm(double length) => this * (length / this.length);
        // ---------------------------------------------------------------------------------------------
        //
        //   Set:
        //
        // ---------------------------------------------------------------------------------------------
        public SNormal Set(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
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
            throw new Exception("SNormal.__GetCoord(): Wrong direction (dir). ");
        }
        public double GetCoord(int dir)
        {
            switch (dir)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
            }
            throw new Exception($"SNormal.__GetCoord(): Wrong direction (dir = '{dir}'). ");
        }   
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Scale:
        //
        // --------------------------------------------------------------------------------------------------------
        public SNormal Scale(double scale)
        {
            x = x * scale;
            y = y * scale;
            z = z * scale;
            return this;
        } 
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Operators:
        //
        // --------------------------------------------------------------------------------------------------------
        public static SNormal operator +(SNormal a, SNormal b)        => new SNormal(a.x + b.x, a.y + b.y, a.z + b.z);
        public static SNormal operator -(SNormal a, SNormal b)        => new SNormal(a.x - b.x, a.y - b.y, a.z - b.z);

        public static double  operator *(SNormal a, SNormal b)        => (a.x * b.x + a.y * b.y + a.z * b.z) / (a.length * b.length);  // scalar product
        public static SNormal operator *(SNormal a, double scale)     => new SNormal(a.x * scale, a.y * scale, a.z * scale);
        public static SNormal operator *(double scale, SNormal a)     => new SNormal(a.x* scale, a.y* scale, a.z* scale);
        public static bool operator ==(SNormal obj1, SNormal obj2)    => __Same(obj1, obj2);
        public static bool operator !=(SNormal obj1, SNormal obj2)    => !__Same(obj1, obj2);
        /// <summary>
        /// gets average normal from the normal list
        /// </summary> 
        public static SNormal Avg(List<SNormal> normals)              => new SNormal(normals.Average(n => n.x), normals.Average(n => n.y), normals.Average(n => n.z));

        private static bool __Same(SNormal obj1, SNormal obj2)
        {
            if (obj1 is null && obj2 is null) return true;
            if (obj1 is null) return false;
            if (obj2 is null) return false;
            return (obj1.x == obj2.x && obj1.y == obj2.y && obj1.z == obj2.z);
        }
        public override bool Equals(object o)
        {
            if (this is null && o is null) return true;
            if (this is null) return false;
            if (!(o is SNormal)) return false;
            return __Same(this, (SNormal)o);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      GetHashCode:
        //
        // ------------------------------------------------------------------------------------------- 
        public override int GetHashCode() => base.GetHashCode(); 
    }
}
