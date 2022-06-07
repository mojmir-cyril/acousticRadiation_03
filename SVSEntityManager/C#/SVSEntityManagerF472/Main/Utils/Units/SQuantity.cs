#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Text;
using System.Linq; 
using System.Globalization;


//
//  Ansys:
//
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.Core.Units;
using Ansys.ACT.Interfaces.UserObject;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public abstract class SQuantity : Quantity
    {
        public abstract string quantityName     { get; }
        public abstract string supportedUnits   { get; }
        public SQuantity(string value) : base(value)
        {
            if (!Check()) throw new Exception($"SQuantity(...): Wrong unit = {Unit}, type = {GetType()}! ");
        }
        public SQuantity(double value, string unit) : base(value, unit)
        {
            if (!Check()) throw new Exception($"SQuantity(...): Wrong unit = {Unit}, type = {GetType()}! ");
        }
        public bool Check() => supportedUnits.Split(',').Contains(Unit);
        public double ToInternal()
        {
            if      (this is SLength)       return ConvertUnit(SUnitsUtils.internalLengthUnit).Value;
            else if (this is STemperature)  return ConvertUnit(SUnitsUtils.internalTemperatureUnit).Value;
            else if (this is SConvection)   return ConvertUnit(SUnitsUtils.internalConvectionUnit).Value;
            else if (this is SAngle)        return ConvertUnit(SUnitsUtils.internalAngleUnit).Value;
            else if (this is SFrequency)    return ConvertUnit(SUnitsUtils.internalFrequencyUnit).Value;
            else if (this is SAcceleration) return ConvertUnit(SUnitsUtils.internalAccelerationUnit).Value;
            else throw new Exception($"SQuantity.ToInternal(...): Unknow type = {GetType()}! ");
        }
        public double ToCurrent(SUnitsUtils units) => ConvertUnit(units.GetCurrentUnit(quantityName)).Value; 
        public double ToSolver(SUnitsUtils units, Analysis anal)  => ConvertUnit(units.GetSolverUnits(quantityName, anal)).Value; 
        protected static double ToDouble(object o) => ToDouble(o.ToString());
        protected static double ToDouble(string s)
        {
            try
            {
                CultureInfo c = new CultureInfo("en-US");
                s = s.Replace(",", c.NumberFormat.NumberDecimalSeparator);
                s = s.Replace(".", c.NumberFormat.NumberDecimalSeparator);
                return Convert.ToDouble(s, c);
            }
            catch (Exception err) { SExceptionBase.Throw(err, nameof(SQuantity), nameof(ToDouble)); } // catch (Exception err) { throw new Exception($"SQuantity.ToDouble(...): {err.Message}", err); }
            return double.NaN;
        }
    }
    public class SLength : SQuantity
    {
        public override string quantityName   { get => "Length"; }        // public static readonly override string quantityName   = "Length";
        public override string supportedUnits { get => "m,cm,mm,um"; }    // public static readonly override string supportedUnits = "m,cm,mm,um";
        public SLength(string value) : base(value) { }
        public SLength(double value, string unit) : base(value, unit) { }
        public SLength(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Length")) { }
        public SLength(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
        public double ToMeshUnit(SUnitsUtils units) => ConvertUnit(units.meshUnit).Value;
    }
    public class STemperature : SQuantity
    {
        public override string quantityName   { get =>  "Temperature"; }
        public override string supportedUnits { get =>  "K,C,R,F"; }
        public STemperature(string value) : base(value) { }
        public STemperature(double value, string unit) : base(value, unit) { }
        public STemperature(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Temperature")) { }
        public STemperature(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
    }
    public class SConvection : SQuantity
    {
        public override string quantityName   { get => "Heat Transfer Coefficient"; }
        public override string supportedUnits { get => "W m^-2 K^-1,W m^-1 m^-1 K^-1,W m^-2 C^-1,W m^-1 m^-1 C^-1," +
                                                       "W mm^-2 K^-1,W mm^-1 mm^-1 K^-1,W mm^-2 C^-1,W mm^-1 mm^-1 C^-1," +
                                                       "W um^-2 K^-1,W um^-1 um^-1 K^-1,W um^-2 C^-1,W um^-1 um^-1 C^-1," +
                                                       "tonne s^-3 C^-1,tonne s^-3 K^-1,tonne s^-1 s^-1 s^-1 C^-1,tonne s^-1 s^-1 s^-1 K^-1"; }
        public SConvection(string value) : base(value) { }
        public SConvection(double value, string unit) : base(value, unit) { }
        public SConvection(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Heat Transfer Coefficient")) { }
        public SConvection(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
    }
    public class SAngle : SQuantity  // Phase
    {
        public override string quantityName   { get =>  "Angle"; }
        public override string supportedUnits { get =>  "radian,rad,degree,deg"; }
        public SAngle(string value) : base(value) { }
        public SAngle(double value, string unit) : base(value, unit) { }
        public SAngle(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Angle")) { }
        public SAngle(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
    }
    public class SAcceleration : SQuantity
    {
        public override string quantityName   { get =>  "Accelartion"; }
        public override string supportedUnits { get =>  "m s^-2,ft s^-2,in s^-2," +
                                                        "cm s^-2,mm s^-2,um s^-2,mm ms^-2,cm us^-2,um ms^-2," +
                                                        "cm sec^-1 sec^-1,mm sec^-1 sec^-1,um sec^-1 sec^-1," +
                                                        "mm ms^-2,cm us^-2,um ms^-2," +
                                                        "m sec^-1 sec^-1,mm sec^-1 sec^-1";}
        public SAcceleration(string value) : base(value) { }
        public SAcceleration(double value, string unit) : base(value, unit) { }
        public SAcceleration(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Accelartion")) { }
        public SAcceleration(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
    }
    public class SFrequency : SQuantity
    {
        public override string quantityName     { get =>  "Frequency"; }
        public override string supportedUnits   { get =>  "Hz,MHz"; }
        public SFrequency(string value) : base(value) { }
        public SFrequency(double value, string unit) : base(value, unit) { }
        public SFrequency(double value, SUnitsUtils units, SCurrentUnit cur) : base(value, units.GetCurrentUnit("Frequency")) { }
        public SFrequency(ISimProperty p) : base(ToDouble(p.Value), p.UnitString) { }
    } 
}
