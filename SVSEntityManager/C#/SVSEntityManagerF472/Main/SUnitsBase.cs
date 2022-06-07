#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;  

//
//  Ansys:
// 
using Ansys.Core.Units; 
using Ansys.ACT.Interfaces.Mechanical; 


using SVSLoggerF472;
using SVSExceptionBase;
using System.IO;
using System.Reflection;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// abstract
    /// </summary>
    public abstract class SUnitsBase : SLoggerBase
    {
        // /// <summary>
        // /// logging events to ANSYS Extensions Log File and defined log file
        // /// </summary>
        // public SLogger              logger                      { get; }
        // /// <summary>
        // /// ANSYS Mechanical ExtAPI
        // /// </summary>
        // public IMechanicalExtAPI    api                         { get; }
        /// <summary>
        /// Unit Utilities
        /// </summary>
        public SUnitsUtils          unitUtils                   { get; set; }
        // -------------------------------------------------------------------------------------------
        //
        //      Units:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// internal geometry length unit based on import ("m" is typical for SpaceClaim, "mm" is typical for CATIA)
        /// </summary>
        public string               geomUnit                    { get => unitUtils.geomUnit;  } //  units.GeomUnit() --> "m"
        /// <summary>
        /// internal mesh length unit based on import (typically same geomUnit)
        /// </summary>
        public string               meshUnit                    { get => unitUtils.meshUnit;  } //  units.MeshUnit() --> "m"
        /// <summary>
        /// time unit used in EM (default: "sec")
        /// </summary>
        public string               timeUnit                    { get; set; } = "sec";
        /// <summary>
        /// length unit used in EM (default: geomUnit)
        /// </summary>
        public string               lengthUnit                  { get; set; }   // TO-DO: kontrola jednotek zda jsou OK, asi pres Quantity
        /// <summary>
        /// angle unit used in EM (default: "angle")
        /// </summary>
        public string               angleUnit                   { get; set; }   // TO-DO: kontrola jednotek zda jsou OK, asi pres Quantity
        /// <summary>
        /// mass unit used in EM (default: "kg")
        /// </summary>
        public string               massUnit                    { get; set; }   // TO-DO: kontrola jednotek zda jsou OK, asi pres Quantity
        /// <summary>
        /// area unit given from lengthUnit
        /// </summary>
        public string               areaUnit                    { get => $"{lengthUnit}^2";  }
        /// <summary>
        /// volume unit given from lengthUnit
        /// </summary>
        public string               volumeUnit                  { get => $"{lengthUnit}^3";  } 
        /// <summary>
        /// volume unit given from lengthUnit and massUnit
        /// </summary>
        public string               momentOfInertiaUnit         { get => $"{massUnit} {lengthUnit}^2";  } 
        // -------------------------------------------------------------------------------------------
        //
        //      accur:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// accuracy for length unit: 
        /// default: 10
        /// order  ---> 12345.12345
        ///  3    ---> 12345.123
        ///  2    ---> 12345.12
        ///  1    ---> 12345.1
        ///  0    ---> 12345
        /// -1    ---> 12350
        /// -2    ---> 11300
        /// -3    ---> 11000
        /// </summary>
        public int                  lengthAccurDigits           { get;  set; } = 10;
        /// <summary>
        /// accuracy for angle unit: 
        /// default: 10
        /// order  ---> 12345.12345
        ///  3    ---> 12345.123
        ///  2    ---> 12345.12
        ///  1    ---> 12345.1
        ///  0    ---> 12345
        /// -1    ---> 12350
        /// -2    ---> 11300
        /// -3    ---> 11000
        /// </summary>
        public int                  angleAccurDigits            { get;  set; } = 10;
        /// <summary>
        /// accuracy for mass unit: 
        /// default: 10
        /// order  ---> 12345.12345
        ///  3    ---> 12345.123
        ///  2    ---> 12345.12
        ///  1    ---> 12345.1
        ///  0    ---> 12345
        /// -1    ---> 12350
        /// -2    ---> 11300
        /// -3    ---> 11000
        /// </summary>
        public int                  massAccurDigitis            { get;  set; } = 10;
        /// <summary>
        /// accuracy for mass unit given from lengthAccurDigits
        /// </summary>
        public int                  areaAccurDigits             { get => 2 * lengthAccurDigits; set => throw new Exception("areaAccurDigits cannot be set. Use lengthAccurDigits istead. "); }
        /// <summary>
        /// accuracy for mass unit given from lengthAccurDigits
        /// </summary>
        public int                  volumeAccurDigits           { get => 3 * lengthAccurDigits; set => throw new Exception("volumeAccurDigits cannot be set. Use lengthAccurDigits istead. "); }
        /// <summary>
        /// accuracy for moment of inertia unit given from lengthAccurDigits and massAccurDigitis
        /// </summary>
        public int                  momentOfInertiaAccurDigitis { get => 2 * lengthAccurDigits * massAccurDigitis; set => throw new Exception("momentOfInertiaAccurDigitis cannot be set. Use lengthAccurDigits and massAccurDigitis istead. "); }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// abstract
        /// </summary>
        public SUnitsBase(IMechanicalExtAPI api,  
                          string lengthUnit, 
                          string angleUnit, 
                          string massUnit,
                          bool loggingToACT  = false,
                          bool loggingToText = false,
                          bool loggingToHtml = false) : base(api, nameof(SUnitsBase))
        { 
            Units(lengthUnit, angleUnit, massUnit); 
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Units:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// sets unit strings
        /// default:
        ///  lengthUnit = "mm" 
        ///  angleUnit  = "deg" 
        ///  massUnit   = "kg"
        /// </summary>
        /// <example>
        /// <code>
        /// em.Units("mm", "deg", "kg")
        /// em.Units("m", "rad", "t")
        /// </code>
        /// </example>
        public SUnitsBase Units(string length, string angle, string mass)
        {
            lengthUnit  = length;
            angleUnit   = angle;
            massUnit    = mass;
            return this;
        }
        /// <summary>
        /// sets default unit strings
        /// default:
        ///  lengthUnit = "mm" 
        ///  angleUnit  = "deg" 
        ///  massUnit   = "kg"
        /// </summary>
        /// <example>
        /// <code>
        /// em.Units("mm", "deg", "kg")
        /// em.Units("m", "rad", "t")
        /// </code>
        /// </example>
        public SUnitsBase Units() => Units("mm", "deg", "kg");
        // -------------------------------------------------------------------------------------------
        //
        //      AccurDigits:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// sets default rounding of numbers for keeping of accuracy,
        /// it is necessary for selecting by coordinates,
        /// the function sets the digits accuracy level lengthAccurDigits, massAccurDigitis, angleAccurDigits together
        /// default:
        ///   lengthAccurDigits = 10
        ///   angleAccurDigits  = 10
        ///   massAccurDigitis  = 10 
        /// </summary>
        public SUnitsBase AccurDigits()
        {
            lengthAccurDigits = 10;
            angleAccurDigits  = 10;
            massAccurDigitis  = 10;
            return this;
        }
        /// <summary>
        /// sets rounding of numbers for keeping of accuracy,
        /// it is necessary for selecting by coordinates,
        /// the function sets the digits accuracy level lengthAccurDigits, massAccurDigitis, angleAccurDigits together
        /// default:
        ///   lengthAccurDigits = 10
        ///   angleAccurDigits  = 10
        ///   massAccurDigitis  = 10 
        /// </summary>
        public SUnitsBase AccurDigits(int length, int angle, int mass)
        {
            //
            //
            //   order --> 12345.12345
            //       3 --> 12345.123
            //       2 --> 12345.12
            //       1 --> 12345.1
            //       0 --> 12345
            //      -1 --> 12350
            //      -2 --> 11300
            //      -3 --> 11000
            //
            //
            // 
            lengthAccurDigits = length;
            angleAccurDigits  = angle;
            massAccurDigitis  = mass;
            return this;
        }
        private double __R(double l, int digits)    { double s = Math.Pow(10.0, digits); return Math.Round(l / s) * s; }
        /// <summary>
        /// round a number
        /// </summary>
        public double Round(double l, int digits)   => digits > 15 ? l : digits >= 0 ? Math.Round(l, digits) : __R(l, -digits);
        /// <summary>
        /// round a length value
        /// </summary>
        public double RoundLength(double l)         => Round(l, lengthAccurDigits);
        /// <summary>
        /// round a area value
        /// </summary>
        public double RoundArea(double a)           => Round(a, areaAccurDigits);
        /// <summary>
        /// round a volume value
        /// </summary>
        public double RoundVolume(double v)         => Round(v, volumeAccurDigits);
        /// <summary>
        /// round a angle value
        /// </summary>
        public double RoundAngle(double a)          => Round(a, angleAccurDigits);
        /// <summary>
        /// round a mass value
        /// </summary>
        public double RoundMass(double m)           => Round(m, massAccurDigitis); 
        /// <summary>
        /// round a moment of inertia value
        /// </summary>
        public double RoundMomentOfInertia(double m) => Round(m, momentOfInertiaAccurDigitis); 
        // -------------------------------------------------------------------------------------------
        //
        //      __ConvertVolume, __ConvertArea, __ConvertLength:
        //
        // -------------------------------------------------------------------------------------------  
        /// <summary>
        /// convert a mass value to lengthUnit
        /// </summary>
        public double ConvertLength(double l)       => RoundLength(unitUtils.geomUnit != lengthUnit  ? new Quantity(l, unitUtils.geomUnit).ConvertUnit(lengthUnit).Value : l);
        /// <summary>
        /// convert a area value to areaUnit
        /// </summary>
        public double ConvertArea(double a)         => RoundArea(unitUtils.geomUnit   != lengthUnit  ? new Quantity(a, unitUtils.geomUnit + "^2").ConvertUnit(areaUnit).Value : a);
        /// <summary>
        /// convert a volume value to volumeUnit
        /// </summary>
        public double ConvertVolume(double v)       => RoundVolume(unitUtils.geomUnit != lengthUnit  ? new Quantity(v, unitUtils.geomUnit + "^3").ConvertUnit(volumeUnit).Value : v);
        /// <summary>
        /// convert a angle value to angleUnit
        /// </summary>
        public double ConvertAngle(double a)        => RoundAngle("rad" != angleUnit ? new Quantity(a, "rad").ConvertUnit(angleUnit).Value : a);
        /// <summary>
        /// convert a mass value to massUnit
        /// </summary>
        public double ConvertMass(double m)                 => RoundMass(unitUtils.massUnit != massUnit ? new Quantity(m, unitUtils.massUnit).ConvertUnit(massUnit).Value : m);
        /// <summary>
        /// convert a moment of intertia to momentOfInertiaUnit
        /// </summary>
        public double ConvertMomentOfInertia(double m)      => RoundMass(mOiUnit != momentOfInertiaUnit ? new Quantity(m, mOiUnit).ConvertUnit(momentOfInertiaUnit).Value : m); 
        private string mOiUnit { get => $"{unitUtils.massUnit} {unitUtils.geomUnit}^2"; } 
        /// <summary>
        /// convert a mass value to lengthUnit
        /// </summary>
        public double ConvertLength(Quantity l)             => RoundLength(l.ConvertUnit(lengthUnit).Value);
        /// <summary>
        /// convert a area value to areaUnit
        /// </summary>
        public double ConvertArea(Quantity a)               => RoundArea(a.ConvertUnit(areaUnit).Value);
        /// <summary>
        /// convert a volume value to volumeUnit
        /// </summary>
        public double ConvertVolume(Quantity v)             => RoundVolume(v.ConvertUnit(volumeUnit).Value);
        /// <summary>
        /// convert a angle value to angleUnit
        /// </summary>
        public double ConvertAngle(Quantity a)              => RoundAngle(a.ConvertUnit(angleUnit).Value);
        /// <summary>
        /// convert a mass value to massUnit
        /// </summary>
        public double ConvertMass(Quantity m)               => RoundMass(m.ConvertUnit(massUnit).Value);
        /// <summary>
        /// convert a moment of intertia value to massUnit and lengthUnit
        /// </summary>
        public double ConvertMomentOfInertia(Quantity m)    => RoundMomentOfInertia(m.ConvertUnit(momentOfInertiaUnit).Value);
    }
}
