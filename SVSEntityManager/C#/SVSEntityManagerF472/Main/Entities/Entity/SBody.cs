#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
//  Ansys:
// 
using Ansys.ACT.Automation.Mechanical; 
using Ansys.ACT.Interfaces.Geometry; 


using SVSConvertF472;

namespace SVSEntityManagerF472
{
    /// <summary>
    /// Class/instance of SBody is single body object.
    /// It is SVSEntityManager basic object for working with ANSYS Mechanical model. 
    /// It is wrapper of several ACT object(s).
    /// </summary>
    public class SBody : SEntity
    { 
        /// <summary>
        /// gets internal (ACT) object of the body (IGeoBody)
        /// </summary>
        public IGeoBody        iBody            { get => (IGeoBody)iEntity; }
        /// <summary>
        /// gets internal (ACT) object of the entity (IBaseGeoEntity)
        /// </summary>
        public IBaseGeoEntity iGeoEntity        { get => (IBaseGeoEntity)iEntity; }
        /// <summary>
        /// gets geometry type of the entities => SType.Body
        /// </summary>
        public override SType  type             { get => SType.Body; }
        /// <summary>
        /// gets true if geometry type of the entities => true
        /// </summary>
        public override bool   isGeom           { get => true; }
        // -------------------------------------------------------------------------------------------
        //
        //      props:
        //
        // -------------------------------------------------------------------------------------------
        // public double          volume           { get => __ConvertVolume(internalBody.Volume); }  // nefunguje na skorepinach ---> nodeBody
        /// <summary>
        /// gets total area of faces which wrap body (in areaUnit)
        /// </summary>
        public double          area             { get => em.ConvertArea(iBody.Area); }
        /// <summary>
        /// gets total length of edges of the body (in lengthUnit)
        /// </summary>
        public double          length           { get => em.ConvertLength(iBody.Length); }
        /// <summary>
        /// gets body type (ACT)
        /// </summary> 
        public GeoBodyTypeEnum bodyType         { get => iBody.BodyType; }
        /// <summary>
        /// gets cross-section (ACT) object if body type is line/wire
        /// </summary> 
        public object          crossSection     { get => iBody.CrossSection; }
        /// <summary>
        /// gets cross-section name if body type is line/wire
        /// </summary> 
        public string          crossSectionName { get => ((dynamic)iBody.CrossSection)?.Name ?? "N/A"; }
        /// <summary>
        /// gets material (ACT) object
        /// </summary> 
        public object          material         { get => iBody.Material; }
        // -------------------------------------------------------------------------------------------
        //
        //      nodeBody:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets Mechanical Tree node of the body
        /// </summary>
        public Body            nodeBody         { get => em.tree.model.Geometry.GetBody(iBody); }
        /// <summary>
        /// gets Mechanical Tree node property (by string name PropertyByName(v)) of the body
        /// </summary>
        /// <example>
        /// <code>
        /// print this["MaterialName"].InternalValue
        /// print this["Color"].InternalValue
        /// print this["Transparency"].InternalValue
        /// </code>
        /// </example>
        public Property        this[string v]   { get { try   { return nodeBody.PropertyByName(v); } 
                                                        catch { throw new Exception($"nodeBody.PropertyByName(...): A problem to find property '{v}'. " +
                                                                                    $"This ACT is optimalized for Ansys 2021R1. ");  } } }
        /// <summary>
        /// gets Mechanical Tree node name of the body
        /// </summary>
        public string          name             { get => nodeBody.Name; 
                                                  set => nodeBody.Name = value; }
        /// <summary>
        /// gets material name of the body
        /// </summary>
        public string          materialName     { get => this["MaterialName"].InternalValue.ToString();
                                                  set => this["MaterialName"].InternalValue = value; }
        /// <summary>
        /// gets transparency of the body 
        /// </summary> 
        public double          transparency     { get => SConvert.ToDouble(this["Transparency"].InternalValue);    // podporovano od 2021R1
                                                  set => this["Transparency"].InternalValue = value; }             // podporovano od 2021R1
        /// <summary>
        /// gets color (long integer) of the body
        /// </summary> 
        public int             color            { get => SConvert.ToInt32(this["Color"].InternalValue);            // podporovano od 2021R1
                                                  set => __SetColor(value); }                                      // podporovano od 2021R1
        /// <summary>
        /// gets mass of the body in current unit (em.massUnit)
        /// </summary> 
        public double          mass             { get => em.ConvertMass(nodeBody.Mass); }
        /// <summary>
        /// gets volume of the body in current unit (em.volumeUnit )
        /// </summary> 
        public double          volume           { get => em.ConvertVolume(nodeBody.Volume); }  // nefunguje na skorepinach
        /// <summary>
        /// gets first principal moment of inertia of the body in current unit (em.lengthUnit)
        /// </summary> 
        public double          Ip1              { get => SShape1D2D3D.GetMomentOfInertiaMax(this); }
        /// <summary>
        /// gets second principal moment of inertia of the body in current unit (em.lengthUnit)
        /// </summary> 
        public double          Ip2              { get => SShape1D2D3D.GetMomentOfInertiaMid(this); }
        /// <summary>
        /// gets third principal moment of inertia of the body in current unit (em.lengthUnit)
        /// </summary> 
        public double          Ip3              { get => SShape1D2D3D.GetMomentOfInertiaMin(this); }
        /// <summary>
        /// gets shape index 1D of the body (TO-DO: tune the value)
        /// </summary> 
        public double          shapeIndex1D     { get => SShape1D2D3D.GetShape1DIndex(this); }
        /// <summary>
        /// gets shape index 2D of the body (TO-DO: tune the value)
        /// </summary> 
        public double          shapeIndex2D     { get => SShape1D2D3D.GetShape2DIndex(this); }
        /// <summary>
        /// gets shape index 3D of the body (TO-DO: tune the value)
        /// </summary> 
        public double          shapeIndex3D     { get => SShape1D2D3D.GetShape3DIndex(this); }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts (multi-body parts) 
        /// </summary>
        public override SParts          parts          { get => bodies.parts; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        public override SBodies         bodies         { get => SNew.FromSingle<SBody, SBodies>(this); }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        public override SFaces          faces          { get => bodies.faces; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        public override SEdges          edges          { get => bodies.edges; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        public override SVerts          verts          { get => bodies.verts; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        public override SNodes          nodes          { get => bodies.nodes; }
        /// <summary>
        /// converts to attached elems
        /// </summary>
        public override SElems          elems          { get => bodies.elems; }
        /// <summary>
        /// converts to attached element faces
        /// </summary>
        public override SElemFaces      elemFaces      { get => bodies.elemFaces; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// creates new object
        /// </summary> 
        public SBody(SEntityManager em, IGeoBody internalBody) : base(em, internalBody) { }        
        // -------------------------------------------------------------------------------------------
        //
        //      color:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// sets color by red, green, blue (0 up to 255)
        /// </summary>
        public void SetRGB(int r, int g, int b) => __SetColor(SColorUtils.FromRGB(b, g, r)); // ANSYS ma prehozeno RGB->BGR
        private void __SetColor(int col, bool saveResume = true)
        { 
            if (saveResume) em.tree.Save(); 
            nodeBody.Activate(); 
            this["Color"].InternalValue = col; 
            if (saveResume) em.tree.Resume();
        }
        /// <summary>
        /// gets string of the object 
        /// </summary> 
        public override string ToString() => $"EntityManager.SBody(name = '{name}')"; 
        // -------------------------------------------------------------------------------------------
        //
        //      Extend:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// extends bodies by a function.  
        /// </summary>
        /// <example>
        /// <code>
        /// em.current.Extend(lambda cur, ver: cur.x == ver.x).Sel()
        /// em.current.Extend(lambda cur, ver: cur.x == ver.x).showns.Sel()
        /// #
        /// # where:
        /// #   cur .. is currently selected object (em.current)
        /// #   any .. is any object in the model
        /// #
        /// </code>
        /// </example>
        /// <param name="func">SBody, SBody, bool</param>
        /// <returns>new SBodies</returns>
        public SBodies Extend(Func<SBody, SBody, bool> func) => bodies.Extend(func);
        // -------------------------------------------------------------------------------------------
        //
        //      Activate: 
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// goes (selects) tree nodes attached to the bodies 
        /// </summary>
        public void Activate() => nodeBody.Activate();
    }
}


// f = Tree.FirstActiveObject
// f.Properties
// Hidden, Glow, Shininess, Transparency, Color, Specularity, Suppressed, ID, IsInfluenceBody, Dimension, MaterialName, 
// HomogeneousMaterialName, Phase, BoundingBoxLengthX, BoundingBoxLengthY, BoundingBoxLengthZ, CellsAlongCircumference, 
// CellsThroughThickness, Volume, Mass, MassCentroidX, MassCentroidY, MassCentroidZ, MassMomentOfInertiaP1, 
// MassMomentOfInertiaP2, MassMomentOfInertiaP3, SheetSurfaceArea, WireLength, StiffnessBehavior, BrickIntegrationScheme, 
// NonlinearEffects, ThermalStrainEffects, CoordinateSystemSelection, VirtualBodyDefineByMeshGroup, MaterialPointSelection, 
// UseReferenceTemperature, ReferenceTemperature, Model2DBehavior, Thickness, HomogeneousThickness, Nodes, Elements, 
// CrossSectionName, CrossSectionArea, CrossSectionType, CrossSectionIYY, CrossSectionIZZ, PipeInternalDiameter, 
// PipeExternalDiameter, PipeThickness, MeshReadOnly, MeshMetric, MeshMetricMin, MeshMetricMax, MeshMetricAverage, 
// MeshMetricSTDV, SurfaceArea, MeshVolume, VolumeRatio, NumSurfaces, MeshThickness, MeshType, Version, Refined, MeshOnPart, 
// XCentroid, YCentroid, ZCentroid, XXInertia, YYInertia, ZZInertia, XYInertia, YZInertia, ZXInertia, XXCInertia, YYCInertia, 
// ZZCInertia, XYCInertia, YZCInertia, ZXCInertia, Principal00, Principal01, Principal02, Principal10, Principal11, Principal12, 
// Principal20, Principal21, Principal22, NumTurns, MaterialPolarizationDirection, ThicknessMode, AutomaticOrManual, OffsetType, 
// ShellOffsetType, HomogeneousShellOffsetType, OffsetX, OffsetY, HomogeneousMembraneOffset, Behavior, ModelType, 
// HomogeneousMembrane, FiberCrossSectionArea, FiberSpacing, FiberAngle, LinkTrussBehavior, BeamSolverCrossSectionType, 
// FluidCrossArea, FluidDiscretization, ReferenceFrame, GroupThisBody, 
// BodyGroupPriority, GasketInitialGap, ShellThermalVariation, Source, ReadOnly