#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 


using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections;

//
//  Ansys:
// 
using Ansys.Core.Units;
using Ansys.Mechanical.DataModel.Enums;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Mechanical.Tools;
using Ansys.ACT.Automation.Mechanical;

using Ansys.ACT.Automation.Mechanical.MeshControls;

namespace SVSEntityManagerF472
{

    public class SMorph : SLoggerBase
    {
        public string                                    meshName                { get; }
        public IMeshData                                 mesh                    { get; }
        public SNodes                                    source                  { get; set; }
        public SMorphData                                this[string dataId]     { get => savedData[dataId]; }
        private Dictionary<string, SMorphData>           savedData               { get; set; }  // dataId : coordinates
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        internal SMorph(SEntityManager em, string meshName = "Global") : base(em, nameof(SMorph))
        {
            this.meshName = meshName; 
            //
            //  mesh:
            //
            mesh = em.api.DataModel.MeshDataByName(meshName);
            //
            //  data:
            //
            savedData = new Dictionary<string, SMorphData>();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      __SetSource:
        //
        // -------------------------------------------------------------------------------------------  
        internal SMorph __SetSource(SNodes nodes)
        {
            source = nodes;
            return this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Save & Restore:
        //
        // -------------------------------------------------------------------------------------------
        public SMorph SaveCoords(string dataId, string fileName = "N/A", char decimalSeparator = '.', char cellSeparator = ';') 
            => SaveCoords(source, dataId, fileName, decimalSeparator, cellSeparator);
        public SMorph SaveCoords(SNodes nodes, string dataId, string fileName = "N/A", char decimalSeparator = '.', char cellSeparator = ';')
        {
            logger.Msg("SaveCoords(...)"); 
            savedData[dataId] = new SMorphData(this, nodes, dataId).Save(fileName, decimalSeparator, cellSeparator);
            return this;
        }
        public SMorph LoadCoords(string[] dataIds, string[] fileNames, char decimalSeparator = '.', char cellSeparator = ';')
            => LoadCoords(source, dataIds, fileNames, decimalSeparator, cellSeparator);
        public SMorph LoadCoords(SNodes nodes, string[] dataIds, string[] fileNames, char decimalSeparator = '.', char cellSeparator = ';')
        {
            logger.Msg("LoadCoords(...)");
            for (int i = 0; i < dataIds.Count(); i++)
            {
                logger.Msg($" - loading : {dataIds[i]} : {fileNames[i]}");
                savedData[dataIds[i]] = new SMorphData(this, nodes, dataIds[i]).Load(fileNames[i], decimalSeparator, cellSeparator); 
            }
            return this;
        }
        public SMorph RestoreCoords(string dataId, bool doRedrawMesh = true) => RestoreCoords(source, dataId, doRedrawMesh);
        public SMorph RestoreCoords(SNodes nodes, string dataId, bool doRedrawMesh = true)
        {
            logger.Msg("RestoreCoords(...)");
            if (!savedData.Keys.Contains(dataId)) throw new Exception($"SMorph.RestoreCoords(...): dataId = '{dataId}' is not found. ");
            savedData[dataId].Restore(nodes);
            if (doRedrawMesh) nodes.RedrawMesh();
            return this;
        } 
        public SNodes DoMove(double dx = 0.0, double dy = 0.0, double dz = 0.0,
                             bool doRedrawMesh            = true,
                             bool doUpdateInputSelections = true) // update coords for input selections ...
        {
            Null(source, nameof(source), nameof(DoMove));  // if (source == null) throw new Exception($"SetMoveAndFix(...): Null error: current == null. ");
            //
            //  move:
            //
            List<int> partIds = new List<int>();
            foreach (var n in source)
            {
                SMorphUtils.MoveNode(mesh, n.iNode.BodyIds.First(), n.id, new double[] { dx, dy, dz });
                partIds.AddRange(n.bodies.parts.ids);
            }
            //
            //  redraw & update:
            //
            if (doRedrawMesh)            RedrawMesh(partIds); // em.Redraw(byDS: true); 
            if (doUpdateInputSelections) source.Update();
            //
            //  return source nodes:
            //
            return source;
        }
        public void RedrawMesh()             => RedrawMesh(source);
        public void RedrawMesh(SNodes nodes) => RedrawMesh(nodes.SelectMany(n => n.bodies.parts.ids));
        public void RedrawMesh(IEnumerable<int> partIds, bool byDisplayStyle = true)
        {
            using (logger.StartStop($"RedrawMesh"))
            { 
                partIds.Distinct().ToList().ForEach(pId => Helpers.RedrawPartMesh(pId, updateMeshState: false));
                if (byDisplayStyle)
                {
                    Mesh m = em.api.DataModel.Project.Model.Mesh;
                    if (em.api.DataModel.Tree.FirstActiveObject == m) em.api.DataModel.Project.Activate(); 
                    MeshDisplayStyle o = m.DisplayStyle;
                    m.DisplayStyle = MeshDisplayStyle.AspectRatio; 
                    m.Activate(); 
                    m.DisplayStyle = o;
                }
                else em.Redraw(byDS: true); 
            }
        }
        public SNodes DoMorphAveraging(SNodes fix, SNodes move, double dx = 0.0, double dy = 0.0, double dz = 0.0,
                                      bool doRedrawMesh            = true,
                                      bool doUpdateInputSelections = true) // update coords for input selections ...
        {
            using (logger.StartStop($"DoMorphAveraging"))
            {
                //
                //
                //     em   = EM()
                //     reg  = em.NS("reg").nodes
                //     move = em.NS("move").nodes
                //     fix  = em.NS("fix").nodes
                //     reg.morph.SaveCoords(dataId = "initial") 
                //     reg.morph.DoMorphAveraging(fix, move, dx = 0.05)  
                //
                //
                //
                logger.Msg($"DoMorphAveraging(...)");
                logger.Msg($" - region     : {source}");
                logger.Msg($" - fix        : {fix}");
                logger.Msg($" - move       : {move}");
                logger.Msg($" - dx, dy, dz : {dx}, {dy}, {dz}");
                //
                //  do:
                //
                SMorphField field = new SMorphField(em, mesh, source.ids, fix.ids, move.ids);
                field.SetMove(dx, dy, dz);
                field.NeighborAverage();  // SMorphUtils.MorphNodes(mesh, dxyzs);
                field.MoveNodes();        // SMorphUtils.MoveNodes(mesh, dxyzs);  
                //
                //  redraw & update:
                //
                if (doRedrawMesh)            RedrawMesh(source); // em.Redraw(byDS: true);  
                if (doUpdateInputSelections) source.Update();
                //
                //  return source nodes:
                //
                return source;
            }
        }
        public SNodes DoMorphSHELL(SNodes fix, SNodes move, SNodes shell, SNodes shellFix,
                                   double dx = 0.0, double dy = 0.0, double dz = 0.0,
                                   bool doRedrawMesh            = true,
                                   bool doUpdateInputSelections = true) // update coords for input selections ...
        {
            using (logger.StartStop($"DoMorphSHELL"))
            {
                //
                //
                //     em       = EM()
                //     reg      = em.NS("reg").nodes
                //     move     = em.NS("move").nodes
                //     fix      = em.NS("fix").nodes
                //     shell    = em.NS("shell").nodes
                //     shellFix = em.NS("shellFix").nodes
                //     reg.morph.SaveCoords(dataId = "initial") 
                //     reg.morph.DoMorphSHELL(fix, move, shell, shellFix, dx = 0.05)  
                //     # -----
                //     reg.morph.RestoreCoords(dataId = "initial") 
                //
                logger.Msg($"DoMorphSHELL(...)");
                logger.Msg($" - region/current : {source}");
                logger.Msg($" - fix            : {fix}");
                logger.Msg($" - move           : {move}");
                logger.Msg($" - shell          : {shell}");
                logger.Msg($" - shellFix       : {shellFix}");
                logger.Msg($" - dx, dy, dz     : {dx}, {dy}, {dz}");
                //
                //  external:
                // 
                SMorphField field = new SMorphField(em, mesh, shell.ids, shellFix.ids, move.ids);
                field.SetMove(dx, dy, dz);
                field.NeighborAverage();  
                field.Stats(); 
                //
                //  internal:
                //
                SMorphField field2 = new SMorphField(em, mesh, source.ids, fix.ids, shell.ids);
                field2.SetMoveFrom(field);
                field2.NeighborAverage();
                field2.Stats(); 
                //
                //  move:
                //
                field2.MoveNodes();        // SMorphUtils.MoveNodes(mesh, dxyzs);  
                //
                //  redraw & update:
                //
                if (doRedrawMesh)            RedrawMesh(source);
                if (doUpdateInputSelections) source.Update();
                //
                //  return source nodes:
                // 
                return source;
            }
        } 
        public SNodes DoMorphByResults(SNodes                           fix, 
                                       SNodes                           move, 
                                       SNodes                           shell, 
                                       SNodes                           shellFix, 
                                       SElemFaces                       elemFacesForNorms, 
                                       SNodes                           shellFreeZero,                              // NS name for export shellFreeZero set of nodes (smoothing) 
                                       //
                                       //  ignores:
                                       //
                                       bool                             ignoreMoveNodesWithoutNormals  = true,      // uzly v Region of Interest ale nelezi na Internal/External ...
                                       //
                                       //  result:
                                       //
                                       Func<int, double>                ResultValueByIdFunc            = null,      // overrides both resultName and fileName
                                       string                           resultNameOrFileName           = "N/A",     // FileName ... restart !!!
                                       bool                             absResultValue                 = true, 
                                       double                           resultPower                    = 1.0,       // 1.0 ... no power 
                                       //                                                              
                                       //  morph:                                                      
                                       //                                                              
                                       bool                             onlyPreview                    = false,
                                       //                                                              
                                       //  threshold:                                                  
                                       //                                                              
                                       SMorphThresholdMethod            threshold                      = SMorphThresholdMethod.Quantile,
                                       double                           quantileThresholdPercentage    = 50,     // [%]           if SMorphThresholdMethod.Quantile
                                       double                           manualThresholdValue           = 1e-5,   // [result unit] if SMorphThresholdMethod.Manual
                                       //                                                              
                                       //  step size:                                                  
                                       //                                                              
                                       SMorphStepSizeMethod             stepSize                       = SMorphStepSizeMethod.Normal,
                                       double                           manualStepSize                 = 0.0020,
                                       double                           extraAgressiveStepSizeRatio    = 1.50,
                                       double                           agressiveStepSizeRatio         = 1.00,
                                       double                           normalStepSizeRatio            = 0.50,
                                       double                           fineStepSizeRatio              = 0.25,
                                       double                           extraFineStepSizeRatio         = 0.12,
                                       //                                                              
                                       //  typical size:                                               
                                       //                                                              
                                       SMorphTypicalSize                typicalSize                    = SMorphTypicalSize.AverageElementSize,
                                       double                           manualTypicalSize              = 0.0020,
                                       //                                                              
                                       //  smoothing:                                                  
                                       //                                                              
                                       SMorphSmoothingFunction          smoothingFunction              = SMorphSmoothingFunction.Triangular,
                                       double                           smoothingAvgRadiusRatio        = 5, 
                                       int                              smoothingAvgPoints             = 20,
                                       //                                                              
                                       //  minimal thickness:                                          
                                       //                                                              
                                       SMorphMinimalThickness           minThickness                   = SMorphMinimalThickness.NodalDistance, // SMorphMinimalThickness { NodalDistance, Off }
                                       Quantity                         minThicknessValue              = null, // e.g.: '5 [mm]'
                                       SNodes                           minThicknessTarget             = null, 
                                       //                                                              
                                       //  error-quality named selection:                              
                                       //                                                              
                                       NamedSelection                   errorQualityNamedSelection     = null,
                                       int                              errorQualityBisectionCount     = 10,
                                       //
                                       //  export normals & distances:
                                       //
                                       string                           exportDistsFileName            = "N/A",  // file name where normals and distances will be exported
                                       //
                                       //  misc:
                                       //
                                       bool                             doRedrawMesh                   = true,  
                                       bool                             doUpdateInputSelections        = true, // update coords for input selections ...
                                       Action<string>                   Log                            = null,
                                       Action<double, double, double>   SendMinAvgMax                  = null,
                                       int                              previewColor                   = 0x0000FF)
        {
            try
            {
                using (logger.StartStopLogLog($"DoMorphByResults", Log))
                { 
                    //
                    //     Model.Mesh.ClearGeneratedData()
                    //     Model.Analyses[1].Solve()
                    //     # -----------------------------------------
                    //     em         = EM()
                    //     reg        = em.NS("reg").nodes
                    //     move       = em.NS("move").nodes
                    //     fix        = em.NS("fix").nodes
                    //     shell      = em.NS("shell").nodes
                    //     shellFix   = em.NS("shellFix").nodes
                    //     norms      = em.NS("norms").elemFaces    # normals
                    //     resultName = "NLEPEQ - move"
                    //     em.ClearGraphics()
                    //     reg.morph.SaveCoords(dataId = "initial") 
                    //     reg.morph.DoMorphByResults(fix, move, shell, shellFix, norms, resultName,
                    //                                onlyPreview = True,
                    //                                threshold = 1e-5, stepSize  = 0.001,
                    //                                doSmoothing = True)  
                    //     # -----------------------------------------
                    //     reg.morph.RestoreCoords(dataId = "initial") 
                    //
                    //
                    //
                    SNodes moveNodes; 
                    using (logger.StartStopLogLog($"move nodes", Log)) moveNodes = move; // .corners; moveMids  = move.mids;
                    //
                    //  log:
                    // 
                    logger.Msg($"<hr>");
                    logger.Msg($"DoMorphByResults(...)");
                    logger.Msg($" - region/current             : {source                   }");
                    logger.Msg($" - fix                        : {fix                      }");
                    logger.Msg($" - move                       : {move                     }");
                    logger.Msg($" - moveNodes                  : {moveNodes                }");
                    logger.Msg($" - shell                      : {shell                    }");
                    logger.Msg($" - shellFix                   : {shellFix                 }");
                    logger.Msg($" - norms                      : {elemFacesForNorms        }");
                    logger.Msg($" - resultNameOrFileName       : {resultNameOrFileName     }"); 
                    logger.Msg($" - onlyPreview                : {onlyPreview              }"); 
                    logger.Msg($" - SendMinAvgMax              : {SendMinAvgMax            }");
                    logger.Msg($" - previewColor               : {previewColor             }"); 
                    logger.Msg($"<hr>");
                    //
                    //  check:
                    //
                    NullAndCount(fix,   nameof(fix));    // if (fix   == null) throw new Exception($"Null error: fix == null. ");
                    NullAndCount(move,  nameof(move));   // if (move  == null) throw new Exception($"Null error: move == null. ");
                    NullAndCount(shell, nameof(shell));  // if (shell == null) throw new Exception($"Null error: shell == null. ");
                    //
                    //  check:
                    //
                    if (ignoreMoveNodesWithoutNormals)
                    { 
                        moveNodes = (moveNodes.elemFaces * elemFacesForNorms).nodes * moveNodes;
                        Log($" - REPAIR (ignoreMoveNodesWithoutNormals): ");
                        Log($"   - move       : {move      }");
                        Log($"   - moveNodes  : {moveNodes }");
                    }
                    Log($"<hr>");
                    //
                    //  moving field:
                    // 
                    SMorphField fieldMov;
                    using (logger.StartStopLogLog($"creating moving field ... ", Log))
                    { 
                        fieldMov = new SMorphField(em, mesh, shell.ids, shellFix.ids, moveNodes.ids, Log);
                    }
                    //
                    //  results:
                    //  
                    Log($" - results ...");
                    Log($" - ResultValueByIdFunc  : {ResultValueByIdFunc}");
                    Log($" - resultNameOrFileName : {resultNameOrFileName}");
                    Log($" - moveNodes.count      : {moveNodes.count}");
                    Log($" - moveNodes.ids.Min()  : {moveNodes.ids.Min()}");
                    Log($" - moveNodes.ids.Max()  : {moveNodes.ids.Max()}");
                    Log($" - absResultValue       : {absResultValue}");
                    Log($" - resultPower          : {resultPower}");
                    //
                    using (logger.StartStopLogLog($"assign result", Log))
                    {
                        if (ResultValueByIdFunc == null)
                        {
                            if (File.Exists(resultNameOrFileName)) moveNodes.result.LoadFromFile(resultNameOrFileName);
                            else                                   moveNodes.result.Assign(resultNameOrFileName);
                            string stats = moveNodes.result.Stats(Log);
                            if (Log != null) Log(stats);
                        }
                    }
                    Log($"<hr>");
                    //
                    //  decl:
                    //
                    List<double>  values  = new List<double>();
                    List<SNormal> normals = new List<SNormal>();
                    //
                    //  normals, distances:
                    //  
                    using (logger.StartStopLogLog($"normals, distances", Log))
                    { 
                        foreach (SNode n in moveNodes)
                        {
                            //
                            //  value:
                            //
                            double value = double.NaN;
                            if (ResultValueByIdFunc == null) value = Math.Pow(absResultValue ? Math.Abs(n.resultValue) : n.resultValue, resultPower);
                            else                             value = ResultValueByIdFunc(n.id);
                            //
                            //  norm:
                            //
                            SElemFaces fcs1 = n.elemFaces;
                            if (fcs1.count <= 0) throw new Exception($"Count 0 error: fcs1.count <= 0. ");
                            SElemFaces fcs = fcs1 * elemFacesForNorms;
                            if (fcs.count <= 0) throw new Exception($"No normal for node '{n?.id}'. Count 0 error: fcs.count <= 0, " +
                                                                    $"fcs1 = {fcs1}, elemFacesForNorms = {elemFacesForNorms}. ");
                            //
                            //  add:m
                            //
                            if (fcs.avgGlobalNormal == null) throw new Exception($"Average global normal for element faces cannot be obtained (fcs.avgGlobalNormal == null). ");
                            normals.Add(fcs.avgGlobalNormal);
                            values.Add(value);
                        }
                    }
                    Log($"   - values (result or ResultValueByIdFunc):");
                    Log($"     - .Count()   : {values.Count()}");
                    Log($"     - .Min()     : {values.Min()}");
                    Log($"     - .Average() : {values.Average()}");
                    Log($"     - .Max()     : {values.Max()}");  
                    //
                    //  check:
                    //
                    if (values.Min() == double.NaN || values.Max() == double.NaN) 
                        throw new Exception("Result values contain incorrect value NaN. " +
                                            $"This is not supported in this version of software. " +
                                            "Morphing has been stopped. ");
                    if (values.Min() == values.Max()) 
                        throw new Exception($"Result values are constant '{values.Min()}'. " +
                                            $"This is not supported in this version of software. " +
                                            $"Morphing has been stopped. ");
                    //
                    //  typical size:
                    // 
                    double typicalSizeValue = double.NaN; 
                    using (logger.StartStopLogLog($"typical size", Log))
                    { 
                        if (typicalSize == SMorphTypicalSize.AverageElementSize)
                        {
                            SElems solids    = moveNodes.Units(em.meshUnit, "deg", "kg").elems.solids;
                            if (solids.isEmpty) throw new Exception("No overlay solid element on 'Region of Interest' location. ");
                            SElems tetPyrs   = solids.tets + solids.pyramids;
                            double avg       = solids.Stats(e => e.volume).avg;
                            typicalSizeValue = Math.Pow(avg, 0.333) * (tetPyrs.count / solids.count + 1.0);
                        }
                        else if (typicalSize == SMorphTypicalSize.Manual)         
                        { 
                            typicalSizeValue = manualTypicalSize;
                        }
                        else throw new Exception($"TO-DO: typicalSizeForStepSize = {typicalSize}");
                    }
                    Log($"   - em.meshUnit            : {em.meshUnit}");
                    Log($"   - typicalSizeForStepSize : {typicalSize}");
                    Log($"   - typicalSizeValue       : {typicalSizeValue}");
                    //
                    //  threshold method:
                    //
                    double GetThresholdValue(double[] vs)
                    { 
                        double tValue = double.NaN;

                        using (logger.StartStopLogLog($"threshold", Log))
                        { 
                            if (threshold == SMorphThresholdMethod.ManualValue) tValue = manualThresholdValue;
                            else
                            {
                                double Quantile()
                                { 
                                    int c = (int)Math.Round(quantileThresholdPercentage / 100.0 * vs.Count());
                                    double val = vs.OrderBy(x => x).Take(c).LastOrDefault();
                                    logger.Msg($"   - quantileThresholdPerce : {quantileThresholdPercentage}");
                                    logger.Msg($"   - count                  : {c}");
                                    logger.Msg($"   - value                  : {val}");
                                    return val;
                                }
                                tValue = threshold == SMorphThresholdMethod.LowestValue  ? vs.Min() :
                                         threshold == SMorphThresholdMethod.HighestValue ? vs.Max() : 
                                         threshold == SMorphThresholdMethod.Quantile     ? Quantile() : double.NaN;
                            }
                        }
                        return tValue;
                    }
                    //
                    //  smoothing of result values:
                    //
                    double[] valuesSmooth = new double[moveNodes.Count()]; 
                    //
                    using (logger.StartStopLogLog($"smoothing", Log))
                    { 
                        bool doSmoothing = smoothingFunction != SMorphSmoothingFunction.Off;
                        Log($"   - smoothingFunction       : {smoothingFunction}");
                        Log($"   - doSmoothing             : {doSmoothing}");
                        if (doSmoothing)
                        {
                            double smoothingAvgRadius = smoothingAvgRadiusRatio * typicalSizeValue;
                            Log($"   - smoothingAvgRadiusRatio : {smoothingAvgRadiusRatio}");
                            Log($"   - smoothingAvgPoints      : {smoothingAvgPoints}");
                            Log($"   - typicalSizeValue        : {typicalSizeValue}");
                            Log($"   - smoothingAvgRadius      : {smoothingAvgRadius}");
                            if (smoothingAvgRadius <= 0) throw new Exception($"Resulting radius cannot be '{smoothingAvgRadius}'. ");
                            if (moveNodes.count != values.Count()) throw new Exception("moveNodes.count != values.Count()");
                            //
                            //  shell free zero:
                            //
                            if (shellFreeZero == null)
                            { 
                                shellFreeZero = shell - (shellFix + moveNodes);
                                shellFreeZero = moveNodes.elems.nodes.elems.nodes.elems.nodes * shellFreeZero;  // two layers of free nodes;
                            } 
                            double zeroValue = GetThresholdValue(values.ToArray());
                            Log($"   - shellFreeZero.count     : {shellFreeZero.count}");
                            Log($"   - zeroValue               : {zeroValue}");
                            //
                            //
                            //
                            foreach ((int i, SNode n) in moveNodes.Enumerate())
                            { 
                                //
                                //  with outers:
                                //
                                valuesSmooth[i] = SMorphUtils.SmoothingRBF(n, moveNodes, values,  
                                                                           outerPointsWithZeroValues:  shellFreeZero, 
                                                                           outerPointsZeroValue:       zeroValue,              // nejlepe threshold aby to zahlazovalo do threshold !!!
                                                                           function:                   smoothingFunction, 
                                                                           maxPoints:                  smoothingAvgPoints, 
                                                                           radius:                     smoothingAvgRadius);
                            }
                        }
                        else valuesSmooth = values.ToArray();
                    } 
                    //
                    //  threshold:
                    //
                    double thresholdValue = GetThresholdValue(valuesSmooth);
                    //
                    //  moving distances:
                    //
                    List<double> dists;
                    //
                    using (logger.StartStopLogLog($"moving distances", Log))
                    { 
                        dists = valuesSmooth.Select(v => v - thresholdValue).ToList();
                    }
                    //
                    //  log:
                    //
                    Log($"   - threshold              : {threshold}");
                    Log($"   - thresholdValue         : {thresholdValue}");
                    Log($"   - valuesSmooth: ");
                    Log($"     - .Count()   : {valuesSmooth.Count()}");
                    Log($"     - .Min()     : {valuesSmooth.Min()}");
                    Log($"     - .Average() : {valuesSmooth.Average()}");
                    Log($"     - .Max()     : {valuesSmooth.Max()}");
                    Log($"   - dists:");
                    Log($"     - .Count()   : {dists.Count()}");
                    Log($"     - .Min()     : {dists.Min()}");
                    Log($"     - .Average() : {dists.Average()}");
                    Log($"     - .Max()     : {dists.Max()}");
                    //
                    //  step size:
                    // 
                    double stepSizeValue    = double.NaN; 
                    //
                    using (logger.StartStopLogLog($"step size", Log))
                    { 
                        if (stepSize == SMorphStepSizeMethod.Manual)
                        {
                            stepSizeValue = manualStepSize;
                        }
                        else
                        {  
                            //
                            //  log:
                            //
                            Log($"   - agressiveStepSizeRatio : {agressiveStepSizeRatio}");
                            Log($"   - normalStepSizeRatio    : {normalStepSizeRatio}");
                            Log($"   - fineStepSizeRatio      : {fineStepSizeRatio}");
                            //
                            //  stepSizeValue:
                            // 
                            if      (stepSize == SMorphStepSizeMethod.ExtraAgressive) stepSizeValue = extraAgressiveStepSizeRatio * typicalSizeValue;
                            if      (stepSize == SMorphStepSizeMethod.Agressive)      stepSizeValue = agressiveStepSizeRatio      * typicalSizeValue;
                            else if (stepSize == SMorphStepSizeMethod.Normal)         stepSizeValue = normalStepSizeRatio         * typicalSizeValue;
                            else if (stepSize == SMorphStepSizeMethod.Fine)           stepSizeValue = fineStepSizeRatio           * typicalSizeValue; 
                            else if (stepSize == SMorphStepSizeMethod.ExtraFine)      stepSizeValue = extraFineStepSizeRatio      * typicalSizeValue; 
                            else throw new Exception($"TO-DO: stepSize = {stepSize}");
                        } 
                    }
                    Log($"   - stepSizeValue          : {stepSizeValue}");
                    //
                    //  normalize:
                    //
                    List<double> distsApply;
                    //
                    using (logger.StartStopLogLog($"normalize", Log))
                    { 
                        double norm = stepSizeValue / dists.Max(d => Math.Abs(d));
                        distsApply  = dists.Select(d => d * norm).ToList(); 
                    }
                    //
                    //  minimal thickness (isMinThk):
                    // 
                    List<double> distsApply2;
                    //
                    using (logger.StartStopLogLog($"min thickness", Log))
                    { 
                        distsApply2 = distsApply; 
                        bool isMinThk = minThickness != SMorphMinimalThickness.Off;
                        Log($"   - minThickness           : {minThickness}");
                        Log($"   - isMinThk               : {isMinThk}");
                        if (isMinThk)
                        {
                            em.unitUtils.UpdateGeomMeshUnit();
                            //
                            //  log:
                            //
                            Log($"   - em.meshUnit            : {em.meshUnit}");
                            Log($"   - minThicknessValue      : {minThicknessValue}"); 
                            //
                            //  do:
                            //
                            double       minT = minThicknessValue.ConvertUnit(em.meshUnit).Value;
                            List<double> thks = moveNodes.AsParallel()
                                                         .AsOrdered()
                                                         .Select(n1 => n1.DistsTo(minThicknessTarget).Min())
                                                         .ToList();  //  moveNodes.Select(n1 => minThicknessTarget.Select(n2 => n1.DistTo(n2)).Min()).ToList(); 
                            //
                            //  modif of distance:
                            // 
                            distsApply2 = new List<double>();
                            foreach ((int i, double d) in distsApply.Enumerate()) distsApply2.Add(d + Math.Max(0, minT - thks[i] - d));
                            //
                            //  log:
                            //
                            Log($"   - thks (min, avg, max)   : {thks.Min()}, {thks.Average()}, {thks.Max()}");  
                            Log($"   - minT                   : {minT}");  
                        } 
                    }
                    //
                    //  log:
                    //
                    Log($" - final values:");
                    Log($"   - values (result) ---> min, avg, max : {values.Min()}, {values.Average()}, {values.Max()}");
                    Log($"   - valuesSmooth    ---> min, avg, max : {valuesSmooth.Min()}, {valuesSmooth.Average()}, {valuesSmooth.Max()}");
                    Log($"   - dists           ---> min, avg, max : {dists.Min()}, {dists.Average()}, {dists.Max()}");
                    Log($"   - distsApply      ---> min, avg, max : {distsApply.Min()}, {distsApply.Average()}, {distsApply.Max()}");
                    Log($"   - distsApply2     ---> min, avg, max : {distsApply2.Min()}, {distsApply2.Average()}, {distsApply2.Max()}"); 
                    Log($"<hr>");  
                    //
                    //  check:
                    //
                    if (distsApply2.Min() == double.NaN || distsApply2.Max() == double.NaN)
                    {
                        throw new Exception("Morphing displacement contain incorrect value NaN. Morphing has been stopped. ");
                    }
                    //
                    //  appling morphing:
                    //
                    using (logger.StartStopLogLog($"appling morphing ...", Log))
                    { 
                        foreach ((int i, SNode n) in moveNodes.Enumerate())
                            fieldMov.SetMove(n.id, normals[i], distsApply2[i]); // id;nx;ny;nz;dist
                    }
                    //
                    //  shell smoothing:
                    //
                    using (logger.StartStopLogLog($"shell smoothing ...", Log))
                    { 
                        fieldMov.NeighborAverage(maxPoints: smoothingAvgPoints);
                        fieldMov.Stats(Log);
                    }
                    Log($"<hr>");
                    //
                    //  mids:
                    // 
                    SMorphField fieldMid = fieldMov;
                    Log($"<hr>");
                    //
                    //  internal nodes:
                    //
                    SMorphField fieldVol;
                    //
                    using (logger.StartStopLogLog($"creating internal node field ...", Log))
                    { 
                        fieldVol = new SMorphField(em, mesh, source.ids, fix.ids, shell.ids, Log);
                        fieldVol.SetMoveFrom(fieldMid);
                        fieldVol.NeighborAverage(maxIters: 2500, maxChangeLimit: 0.0001, maxPoints: 20, Log: Log);
                        fieldVol.Stats(Log);
                    }
                    //
                    //  update:
                    // 
                    if (doUpdateInputSelections)
                    {
                        using (logger.StartStopLogLog($"updating input selections ...", Log))
                        {
                            logger.Msg($"<hr>");
                            logger.Msg($" - updating (coords) selections ...");
                            fix.Update();
                            move.Update(); 
                            shell.Update(); 
                            shellFix.Update();
                            elemFacesForNorms.Update();
                        }
                    }
                    //
                    //  log:
                    // 
                    if (ResultValueByIdFunc == null)
                    {
                        Log($"<hr>");
                        Log($" - Select(m => m.resultValue).Min()     : {moveNodes.Select(m => m.resultValue).Min()}");
                        Log($" - Select(m => m.resultValue).Max()     : {moveNodes.Select(m => m.resultValue).Max()}");
                        Log($" - Select(m => m.resultValue).Average() : {moveNodes.Select(m => m.resultValue).Average()}");
                    }
                    //
                    //  redraw & move:
                    // 
                    using (logger.StartStopLog($"moving nodes & preview ..."))
                    {
                        if (onlyPreview)
                        {
                            fieldVol.Preview(move.ids, addDist: true, addNodeId: false);
                        }
                        else
                        {
                            fieldVol.Preview(move.ids, addDist: false, addNodeId: false, color: previewColor);
                            fieldVol.MoveNodes();
                            if (doRedrawMesh) RedrawMesh(source); 
                        }
                    }
                    Log($"<hr>");
                    //
                    //  element quality:
                    //
                    Log($" - ELEMENT QUALITY ..."); 
                    double scale = 1.0;
                    if (errorQualityNamedSelection != null && !onlyPreview)
                    {
                        NamedSelection n = errorQualityNamedSelection;
                        Log($"   - NS : {n.Name}");
                        int count0 = n.Location?.Ids?.Count ?? 0;
                        if (count0 >= 1) throw new Exception("Wrong element quality!!! ");
                        //
                        //  generate:
                        // 
                        for (int i = 0; i < errorQualityBisectionCount; i++)
                        { 
                            n.Activate();
                            n.Generate();
                            n.Generate();
                            n.Generate();
                            n.Activate(); 
                            //
                            //  count:
                            //
                            int count1 = n.Location?.Ids?.Count ?? 0;
                            Log($"   - ");
                            Log($"   - ");
                            Log($"   - bisection : {i} ---> count : {count1}");
                            if (count1 <= 0)
                            {
                                Log($"   - ------------");
                                Log($"   - NO-BISECTION");
                                Log($"   - ------------");  
                                break;
                            } 
                            //
                            //  bisection:
                            // 
                            Log($" *********************************************************** ");
                            Log($" *********************************************************** ");
                            Log($" *********************** BISECTION ************************* ");
                            Log($" *********************************************************** ");
                            Log($" *********************************************************** "); 
                            fieldVol.Bisection();
                            Log($" - move max dist   : {fieldVol.move.Max(d => d.dist)}");
                            Log($" - move first xyz  : {string.Join(", ", fieldVol.move.First().xyz)}");
                            Log($" - move first dxyz : {string.Join(", ", fieldVol.move.First().dxyz)}");
                            Log($" - move first nxyz : {string.Join(", ", fieldVol.move.First().nxyz)}");
                            Log($" - free max dist   : {fieldVol.free.Max(d => d.dist)}");
                            Log($" - free first xyz  : {string.Join(", ", fieldVol.free.First().xyz)}");
                            Log($" - free first dxyz : {string.Join(", ", fieldVol.free.First().dxyz)}");
                            Log($" - free first nxyz : {string.Join(", ", fieldVol.free.First().nxyz)}");
                            fieldVol.Preview(move.ids, addDist: false, addNodeId: false, color: 0xFF0000);
                            fieldVol.MoveNodes();
                            scale = scale / 2.0;
                        }
                    }
                    else Log($"   - errorQualityNamedSelection is null or onlyPreview ... ");
                    //
                    //  finish:
                    //
                    if (SendMinAvgMax != null) SendMinAvgMax(distsApply2.Min() * scale, distsApply2.Average() * scale, distsApply2.Max() * scale);
                    //
                    //  export *.dist:
                    //
                    using (logger.StartStopLogLog($"appling morphing ...", Log))
                    { 
                        Log($" - exportDistsFileName : {exportDistsFileName}");
                        if (exportDistsFileName != "N/A")
                        {
                            List<string> exportDists = new List<string>() { $"[{mesh.Unit}]" }; // [m]
                            foreach ((int i, SNode n) in moveNodes.Enumerate())
                                exportDists.Add($"{n.id};{string.Join(";", normals[i].xyz)};{distsApply2[i] * scale}");  // id;nx;ny;nz;dist
                            File.WriteAllText(exportDistsFileName, string.Join("\n", exportDists));
                        }
                    } 
                    //
                    //  finish:
                    //
                    Log($"<hr>");
                    return source;
                } 
            }
            catch (Exception err) { Throw(err, nameof(DoMorphByResults)); }  // catch (Exception err) { throw new Exception($"SMorph.DoMorphByResults(...): {err.Message}", err); }
            return null;
        }
    }
}
 
