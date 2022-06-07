#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography;
using System.Xml;
 

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
using Ansys.Core.Units;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public class SUnitsUtils : SLoggerBase
    {
        // public Action<object>    Msg        { get; }
        // public IMechanicalExtAPI api        { get; }
        //                                  
        //  geom, mesh, mass units:                
        //                                  
        public string            geomUnit   { get =>_geomUnit;  } //  units.GeomUnit() --> "m"
        public string            meshUnit   { get =>_meshUnit;  } //  units.MeshUnit() --> "m"
        public string            massUnit   { get => GetCurrentUnit("mass"); } 
        //                                  
        //  osetreni InvokeUIThread chyby:  
        //                                  
        private string          _geomUnit   { get; set; }
        private string          _meshUnit   { get; set; }
        //
        //  fixed:
        //
        public static readonly string internalLengthUnit        = "m";
        public static readonly string internalTemperatureUnit   = "C";
        public static readonly string internalConvectionUnit    = "W m^-2 K^-1";
        public static readonly string internalAngleUnit         = "radian";
        public static readonly string internalFrequencyUnit     = "Hz";
        public static readonly string internalAccelerationUnit  = "m s^-2";
        public SUnitsUtils(SEntityManager em) : base(em, nameof(SUnitsUtils))
        { 
            logger.Msg("SUnits(...)");
            UpdateGeomMeshUnit();
        } 
        public void UpdateGeomMeshUnit()
        {
            try
            {
                _meshUnit = api.DataModel.MeshDataByName("Global").Unit;
                _geomUnit = api.DataModel.GeoData.Assemblies[0].Unit;
            }
            catch (Exception err) { Throw(err, nameof(UpdateGeomMeshUnit)); } // catch (Exception err) { throw new Exception($"UpdateGeomMeshUnit(...): {err.Message}", err); }
        } 
        //
        //  type:
        //
        public bool IsLength(Quantity q)        => new SLength(1, q.Unit).Check();  
        public bool IsTemperature(Quantity q)   => new STemperature(1, q.Unit).Check();
        public bool IsConvection(Quantity q)    => new SConvection(1, q.Unit).Check();
        public bool IsAngle(Quantity q)         => new SAngle(1, q.Unit).Check();
        public bool IsAcceleration(Quantity q)  => new SAcceleration(1, q.Unit).Check();
        public bool IsFrequency(Quantity q)     => new SFrequency(1, q.Unit).Check(); 
        public string GetCurrentUnit(string quantityName)
        {
            // 
            //  e.g.: GetCurrentUnit("Length") ---> "mm"
            // 
            return api.DataModel.CurrentUnitFromQuantityName(quantityName);
        }
        public string GetSolverUnits(string quantityName, Analysis anal)
        {
            // 
            //  e.g.: GetSolverUnits("Length") ---> "mm"
            // 
            return anal.CurrentConsistentUnitFromQuantityName(quantityName);
        }
    }
}
//
//  Ansys.Core.Units.UnitsManager.GetQuantityUnits("Acceleration"):
//
//
//
//  Ansys.Core.Units.UnitsManager.GetQuantityUnits("Angle"):
//  
//   radian
//   degree
//  
//  Ansys.Core.Units.UnitsManager.GetQuantityUnits("Heat Transfer Coefficient"):
//
//  'W m^-2 K^-1',             <---
//  'erg s^-1 cm^-2 K^-1', 
//  'BTU s^-1 ft^-2 F^-1', 
//  'lbf s^-1 ft^-1 R^-1', 
//  'slug s^-3 R^-1', 
//  'lb s^-3 R^-1', 
//  'W mm^-2 K^-1',            <---
//  'W um^-2 K^-1',            <---
//  'W m^-2 C^-1',             <---
//  'dyne s^-1 cm^-1 C^-1',    
//  'tonne s^-3 C^-1',         <---
//  'pW um^-2 C^-1', 
//  'slug s^-3 F^-1', 
//  'slinch s^-3 F^-1', 
//  'mg ms^-3 K^-1', 
//  'g us^-3 K^-1', 
//  'pg ms^-3 K^-1' 
//
// Ansys.Core.Units.UnitsManager.GetQuantityNames()
// ['A Weighted Sound Pressure Level', 'Acceleration', 'Angle', 'Angular Acceleration', 'Angular Velocity', 
// 'Area', 'Capacitance', 'Chemical Amount', 'Compressibility', 'Concentration', 'Contact Resistance', 'Current', 
// 'Current Transfer Coefficient', 'Decay Constant', 'Density', 'Density Derivative', 'Density Derivative wrt Pressure', 
// 'Density Derivative wrt Temperature', 'Dielectric Contact Resistance', 'Dynamic Viscosity', 'Electric Charge', 
// 'Electric Charge Density', 'Electric Charge Transfer Coefficient', 'Electric Conductance Per Unit Area', 
// 'Electric Current Density', 'Electric Current Source', 'Electric Flux Density', 'Electric Field', 
// 'Electrical Conductance', 'Electrical Conductivity', 'Electrical Contact Resistance', 
// 'Electrical Permittivity', 'Electrical Resistance', 'Electrical Resistivity', 'Energy', 
// 'Energy Density by Mass', 'Energy Source', 'Energy Source Coefficient', 'Enthalpy Variance', 'Epsilon', 
// 'Epsilon Flux', 'Epsilon Flux Coefficient', 'Epsilon Source', 'Epsilon Source Coefficient', 
// 'Flame Surface Density Source', 'Force', 'Force Intensity', 'Force Per Angular Unit', 'Fracture Energy', 
// 'Fracture Energy Rate', 'Frequency', 'Gasket Stiffness', 'Heat Flux', 'Heat Flux in', 'Heat Generation', 
// 'Heat Rate', 'Heat Transfer Coefficient', 'Impulse', 'Impulse Per Angular Unit', 'Inductance', 
// 'Interphase Transfer Coefficient', 'Inverse Angle', 'Inverse Area', 'Inverse Length', 'Inverse Stress', 
// 'k', 'k Flux', 'k Flux Coefficient', 'k Source', 'k Source Coefficient', 'Kinematic Diffusivity', 'Length', 
// 'Luminance', 'Luminous Intensity', 'Magnetic Field', 'Magnetic Field Intensity', 'Magnetic Flux', 
// 'Magnetic Flux Density', 'Magnetic Induction', 'Magnetic Potential', 'Magnetic Permeability', 'MAPDL Enthalpy', 
// 'Mass', 'Mass Concentration', 'Mass Concentration Rate', 'Mass Flow', 'Mass Flow in', 'Mass Flow Rate Per Area', 
// 'Mass Flow Rate Per Length', 'Mass Flow Rate Per Volume', 'Mass Flux', 'Mass Flux Coefficient', 
// 'Mass Flux Pressure Coefficient', 'Mass Fraction', 'Mass Per Area', 'Mass Per Length', 'Mass Source', 
// 'Mass Source Coefficient', 'Mass Source Pressure Coefficient', 'Total Mass Source Pressure Coefficient', 
// 'Material Impedance', 'Molar Concentration', 'Molar Concentration Henry Coefficient', 
// 'Molar Concentration Rate', 'Molar Energy', 'Molar Entropy', 'Molar Fraction', 'Molar Mass', 'Molar Volume', 
// 'Moment', 'Moment of Inertia of Area', 'Moment of Inertia of Mass', 'Momentum Source', 
// 'Momentum Source Lin Coeff', 'Momentum Source Quad Coeff', 'Number Source', 'Normalized Value', 
// 'Omega Source', 'Per Mass', 'Per Mass Flow', 'Per Time', 'Per Time Cubed', 'Per Time Squared', 
// 'Piezoelectric Strain Coefficient', 'Piezoelectric Stress Coefficient', 'Pressure', 
// 'Pressure Derivative wrt Temperature', 'Pressure Derivative wrt Volume', 'Pressure Gradient', 
// 'PSD Acceleration', 'PSD Acceleration Gravity', 'PSD Displacement', 'PSD Force', 'PSD Moment', 
// 'PSD Pressure', 'PSD Strain', 'PSD Stress', 'PSD Velocity', 'Power', 'Power Spectral Density', 
// 'Relative Permeability', 'Relative Permittivity', 'Rotational Damping', 'Rotational Stiffness', 
// 'RS Acceleration Gravity', 'Section Modulus', 'Seebeck Coefficient', 'Shear Strain', 'Shear Strain Rate', 
// 'Shock Velocity', 'Solid Angle', 'Soot Cross Coefficient', 'Soot PX Factor', 'Sound Pressure Level', 
// 'Specific', 'Specific Concentration', 'Specific Energy', 'Specific Enthalpy', 'Specific Entropy', 
// 'Specific Flame Surface Density', 'Specific Heat Capacity', 'Specific Volume', 'Specific Weight', 'Stiffness', 
// 'Strain', 'Strength', 'Stress', 'Stress Intensity Factor', 'Stress Per Temperature', 'Surface Charge Density', 
// 'Surface Tension', 'Temperature', 'Temperature Difference', 'Temperature Gradient', 'InvTemp1', 'InvTemp2', 
// 'InvTemp3', 'InvTemp4', 'Temperature Variance', 'Temperature Variance Source', 'Thermal Capacitance', 
// 'Thermal Conductance', 'Thermal Conductivity', 'Thermal Contact Resistance', 'Thermal Expansivity', 'Time', 
// 'Torque', 'Torsional Spring Constant', 'Total Radiative Intensity', 'Translational Damping', 'Turbulent Heat Flux', 
// 'Velocity', 'Voltage', 'Volume', 'Volumetric', 'Volumetric Flow', 'Volumetric Flow in', 'Warping Factor', 
// 'Surface Power Density', 'Force Density', 'Surface Force Density', 'Dimensionless']
//
