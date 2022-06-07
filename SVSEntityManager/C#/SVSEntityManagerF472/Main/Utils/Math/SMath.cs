#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Drawing;

//
//  Math.NET:
//
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Complex; 

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical; 
using SVSLoggerF472;

namespace SVSEntityManagerF472
{
    internal class SMath  
    {
        internal SUnitsUtils          units     { get; }
        internal STree                tree      { get; }
        internal SLogger              logger    { get; }
        internal int                  CPUs      { get; }
        internal IMechanicalExtAPI    api       { get => units.api; }
        // -------------------------------------------------------------------------------------------
        //
        //      previous:
        //
        // -------------------------------------------------------------------------------------------  
        private CoordinateSystem prev_csSource = null;
        private CoordinateSystem prev_csTarget = null;
        private STransformType   prev_tranType = STransformType.FullTransform;
        private double           prev_scale = 1.0;
        private Vector<double>   OS, OT, MTxMSxOS, MTxOT, MTxOT_MTxMSxOS; // MSxOS, 
        private Matrix<double>   MS, MT, MTxMS;
        internal void ClearTransMatrix()
        {
            // logger.Msg("ClearTransMatrix(...)");
            prev_csSource = null;
            prev_csTarget = null;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        internal SMath(SUnitsUtils units, STree tree, SLogger logger, int CPUs = 1)
        {
            this.units = units;
            this.tree  = tree; 
            this.logger = logger;
            this.CPUs  = CPUs; 
        } 
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    Prevzato z VNT-Mapping:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        internal List<SPoint> ToLocal(List<SPoint> globals, CoordinateSystem csLocal,
                                    STransformType tranType = STransformType.FullTransform,
                                    double scale = 1.0) 
        {
            return Transform(globals, csLocal, null, tranType, scale);  // SMath.Transform(globals, cylCS, globCS); // otoceno
        }
        internal SPoint ToLocal(SPoint global, CoordinateSystem csLocal,
                              STransformType tranType = STransformType.FullTransform,
                              double scale = 1.0) 
        {
            return Transform(global, csLocal, null, tranType, scale); 
        }
        internal List<SPoint> ToGlobal(List<SPoint> locals, CoordinateSystem csLocal,
                                            STransformType tranType = STransformType.FullTransform,
                                            double scale = 1.0) 
        {
            return Transform(locals, null, csLocal, tranType, scale);  
        }
        internal SPoint ToGlobal(SPoint local, CoordinateSystem csLocal,
                                STransformType tranType = STransformType.FullTransform,
                                double scale = 1.0)
        {
            return Transform(local, null, csLocal, tranType, scale);  // SMath.Transform(globals, cylCS, globCS); // otoceno
        }

        internal SPoint Transform(SPoint point,
                                CoordinateSystem csSource = null, // null = global CS
                                CoordinateSystem csTarget = null, // null = global CS
                                STransformType tranType = STransformType.FullTransform,
                                double scale = 1.0) 
        {
            try
            {
                if (units == null) throw new Exception("SMath.Transform(...): units == null. "); 
                return Transform(new List<SPoint> { point }, csSource, csTarget, tranType, scale)[0];
            }
            catch (Exception e) { throw new Exception("SMath.Transform(): " + e.ToString(), e); }
        }
        internal List<SPoint> Transform(List<SPoint> points,
                                      CoordinateSystem csSource = null, // null = global CS 
                                      CoordinateSystem csTarget = null, // null = global CS
                                      STransformType tranType = STransformType.FullTransform,
                                      double scale = 1.0) 
        {
            try
            {
                if (csSource == prev_csSource && csTarget == prev_csTarget && tranType == prev_tranType && scale == prev_scale) 
                {
                    //
                    //  reuse old:
                    //
                    return EvaluateTransform(points, null, null, null, null);
                } 
                //
                //  save prev:
                //
                prev_csSource = csSource; prev_csTarget = csTarget; prev_tranType = tranType; prev_scale = scale;
                //
                //  new:
                //
                if (units == null) throw new Exception("SMath.Transform(...): units == null. ");
                double[] originSource = ToDoubleArray(csSource != null ? tree.GetOrigin(csSource, units) : new SPoint(0, 0, 0, 0, "m")); // null = global CS 
                double[] originTarget = ToDoubleArray(csTarget != null ? tree.GetOrigin(csTarget, units) : new SPoint(0, 0, 0, 0, "m")); // null = global CS
                for (int i = 0; i < 3; i++)
                {
                    originSource[i] = originSource[i] * scale;
                    originTarget[i] = originTarget[i] * scale;
                }
                List<double> matrixSource = csSource != null ? csSource.Matrix.ToList() : new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };  // null = global CS 
                List<double> matrixTarget = csTarget != null ? csTarget.Matrix.ToList() : new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };  // null = global CS
                double[,] arrMatrixSource = new double[3, 3];
                double[,] arrMatrixTarget = new double[3, 3];
                int k = 0;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        arrMatrixSource[i, j] = matrixSource[k];
                        arrMatrixTarget[i, j] = matrixTarget[k];
                        k++;
                    }
                //
                //  eval:
                //
                return EvaluateTransform(points, originSource, arrMatrixSource, originTarget, arrMatrixTarget, tranType);
            }
            catch (Exception e) { throw new Exception("SMath.Transform(): " + e.ToString(), e); }
        }
        private List<SPoint> EvaluateTransform(List<SPoint> points,
                                               double[] originSource,
                                               double[,] matrixSource,
                                               double[] originTarget,
                                               double[,] matrixTarget,
                                               STransformType tranType = STransformType.FullTransform)
        {
            try
            {
                //  
                //  bool onlyRotate = tranType == STransformType.OnlyRotate;
                //  
                List<SPoint> newPoints = new List<SPoint>();
                //
                //  new or old:
                //
                if (originSource != null)
                { 
                    //  
                    //  kontroluje zda je spravne zadana dimenze
                    //  
                    if (!(originTarget.Length == 3 && originSource.Length       == 3 &&
                          matrixTarget.Length == 9 && matrixTarget.GetLength(0) == 3 &&
                          matrixSource.Length == 9 && matrixSource.GetLength(0) == 3))
                    { throw new Exception("Incorrect dimension of argument(s)."); }
                    //
                    //   [3]  = [3x3]x[3x3]x[3x1] - [3x3]x[3x3]x[3x1] + [3x3]x[3x1]      
                    //    R   =  MT  x MS  x P    -  MT  x MS  x OS   +  MT  x OT     ---> MTx(MSx(P-OS)+OT)
                    // 
                    //   MT ... matrix target, 
                    //   MS ... matrix source, 
                    //   P  ... point, 
                    //   OS ... origin vector source, 
                    //   OT ... origin vector target,
                    //
                    // Allocation:
                    //
                    // Vector<double> OS, OT, MTxMSxOS, MSxOS, MTxOT, MTxOT_MTxMSxOS;
                    // Matrix<double> MS, MT, MTxMS;
                    OS = NewVector(originSource);
                    OT = NewVector(originTarget);
                    MS = NewMatrix(matrixSource);
                    MT = NewMatrix(matrixTarget);
                    //
                    // Calculation of matrix and vector:
                    //
                    MTxMS          = MT.Inverse().Multiply(MS);
                    MTxMSxOS       = MTxMS.Multiply(OS);
                    // MSxOS          = MS.Multiply(OS);
                    MTxOT          = MT.Inverse().Multiply(OT);
                    MTxOT_MTxMSxOS = MTxOT - MTxMSxOS;
                }
                //
                //  Worker:
                //
                Func<SPoint, SPoint> Worker = point =>
                {
                    Vector<double> R, P;
                    double[] arrPoint = { point.x, point.y, point.z };
                    P = NewVector(arrPoint);
                    if      (tranType == STransformType.OnlyRotate)    R = MTxMS.Multiply(P);
                    else if (tranType == STransformType.FullTransform) R = MTxMS.Multiply(P) + OT - MTxMSxOS;     // oprava: divny ale takto to funguje
                    else                                               R = MTxMS.Multiply(P) + MTxOT_MTxMSxOS;    //original Marek ale nehraje tam translace OT
                    return new SPoint(point.id, R[0], R[1], R[2], point.parent, point.lengthUnit);
                };
                //
                //  work:
                //
                if (CPUs == 1) newPoints = points.Select(Worker).ToList();
                else           newPoints = points.AsParallel().AsOrdered().WithDegreeOfParallelism(CPUs).Select(Worker).ToList();
                //
                //  return
                //
                return newPoints;
            }
            catch (Exception e) { throw new Exception("SMath.EvaluateTransform(): " + e.ToString(), e); }
        }
        internal static double[] ToDoubleArray(SPoint p)
        {
            List<double> n = new List<double>() { p.x, p.y, p.z };
            return n.ToArray();
        }
        internal List<SPoint> ToPolar(List<SPoint> points)
        {
            //
            //  p.X ... radial coord
            //  p.Y ... angle coord
            //  p.Z ... axial coord
            //
            // List<SPoint> newPoints = new List<SPoint>();
            // foreach (SPoint p in points) newPoints.Add(ToPolar(p));
            // return newPoints;
            if (CPUs == 1) return points.Select(p => ToPolar(p)).ToList();
            else           return points.AsParallel().AsOrdered().WithDegreeOfParallelism(CPUs).Select(p => ToPolar(p)).ToList();
        }
        internal SPoint ToPolar(SPoint point)
        {
            //
            //  p.X ... radial coord
            //  p.Y ... angle coord
            //  p.Z ... axial coord
            //
            return new SPoint(point.id, Math.Sqrt(point.x * point.x + point.y * point.y), Math.Atan2(point.y, point.x), point.z, point.lengthUnit);
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    Scale:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        public List<SPoint> Scale(List<SPoint> points, CoordinateSystem cs, double scale)
        {
            SPoint o = tree.GetOrigin(cs, units);
            return points.Select((p) => (p - o) * scale + o).ToList();
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    New Vector:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        public Vector<double> NewVector(List<double> listVector)
        {
            return Vector<double>.Build.DenseOfArray(listVector.ToArray());
        }
        public Vector<double> NewVector(double[] arrayVector)
        {
            return Vector<double>.Build.DenseOfArray(arrayVector);
        }
        public Vector<double> NewVector(double x, double y, double z)
        {
            double[] v = { x, y, z };
            return Vector<double>.Build.DenseOfArray(v);
        }
        public Vector<double> NewVector(SPoint a, SPoint b)
        {
            double[] v = { b.x - a.x, b.y - a.y, b.z - a.z };
            return Vector<double>.Build.DenseOfArray(v);
        }
        public Vector<double> NewVector(int length, double value)  // [1,1,1,1,1,...]
        {
            return Vector<double>.Build.Dense(length, value);
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    New Matrix:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        public Matrix<double> NewMatrix(int rows, int columns)
        {
            return Matrix<double>.Build.Dense(rows, columns);
        }
        public Matrix<double> NewMatrix(double[,] arrayMatrix)
        {
            return Matrix<double>.Build.DenseOfArray(arrayMatrix);
        }
        public Matrix<double> NewMatrix(Vector<double> diagonalVector)
        {
            return Matrix<double>.Build.DiagonalOfDiagonalVector(diagonalVector);
        }
        public Matrix<double> NewMatrix(IEnumerable<IEnumerable<double>> data)
        {
            return Matrix<double>.Build.DenseOfRows(data);  // DenseOfRows(IEnumerable<IEnumerable<T>> data);
        }
        public Matrix<double> NewMatrix(double x) // simple matrix [x]
        {
            double[] v = { x };
            return Matrix<double>.Build.DiagonalOfDiagonalVector(NewVector(v)); 
        }
        public Matrix<double> NewMatrix(IEnumerable<Vector<double>> listOfVectors)
        {
            return Matrix<double>.Build.DenseOfColumnVectors(listOfVectors);
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    Scalar Product:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        public static double ScalarProduct(Vector<double> v1, Vector<double> v2)
        {
            //
            // m = NMain.SMath(ExtAPI, None)
            // v1 = m.NewVector(1,2,3)
            // v2 = m.NewVector(1,2,3)
            // v3 = m.NewVector(3,2,1)
            // v4 = m.NewVector(-1,-2,-3)
            // v5 = m.NewVector(5,0,1)
            // v6 = m.NewVector(0,10,0)
            // 
            // def ScalarProduct(v1, v2): return v1 * v2 / (v1.L2Norm() * v2.L2Norm())
            //     
            // print ScalarProduct(v1, v2) ---> 1.0
            // print ScalarProduct(v1, v3) ---> 0.714285714286
            // print ScalarProduct(v1, v4) ---> -1.0
            // print ScalarProduct(v5, v6) ---> 0.0
            //
            return v1 * v2 / (v1.L2Norm() * v2.L2Norm());
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        //
        //    Complex:
        //
        // ----------------------------------------------------------------------------------------------------------------------------------
        public Complex32 NewComplexScalar(float r, float i)
        {
            return new Complex32(r, i);
        }
        public Vector<Complex32> NewComplexVector(Complex32[] arrayVector)
        {
            return Vector<Complex32>.Build.DenseOfArray(arrayVector);
        }
        public Vector<Complex32> NewComplexVector(int length, Complex32 value)
        {
            return Vector<Complex32>.Build.Dense(length, value);
        }
        public Matrix<Complex32> NewComplexMatrix(Vector<Complex32> diagonalVector)
        {
            return Matrix<Complex32>.Build.DiagonalOfDiagonalVector(diagonalVector);
        }
    }
}
