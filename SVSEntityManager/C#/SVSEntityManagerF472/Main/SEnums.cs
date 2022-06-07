#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVSEntityManagerF472
{
    // public enum SLoggingType                { Text, HTML, ACT, TextAndHTML, TextAndACT}
    public enum SCurrentUnit                { Current }
    public enum SDir                        { x, y, z }
    public enum STransformType              { OnlyRotate, FullTransform }
    public enum SSystemType                 { Polar, Cartesian }
    public enum SType                       { Part, Body, Face, Edge, Vert, Node, Elem, ElemFace }
    public enum SFaceType                   { Quad8, Quad4, Tri6, Tri3 }
    public enum SConvertType                { AnyAttached, OnlyIfAllAttached }
    public enum SMidNodeOrderType           { ClassicIJKLMNOP, Workbench01234675 }
    public enum SMorphThresholdMethod       { ManualValue, LowestValue, HighestValue, Quantile }
    public enum SMorphSmoothingFunction     { Off, Triangular, Constant }
    public enum SMorphStepSizeMethod        { ExtraAgressive, Agressive, Normal, Fine, ExtraFine, Manual }
    public enum SMorphTypicalSize           { AverageElementSize, Manual }
    public enum SMorphMinimalThickness      { NodalDistance, Off }
    public enum SResultProbeTypes           { None, GlobalMax, GlobalMin, LocalMax, LocalMin } 
}
