#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq; 

using SVSLoggerF472;
using SVSExceptionBase;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical; 
using Ansys.ACT.Interfaces.Graphics.Entities; 

namespace SVSEntityManagerF472
{
    /// <summary>
    /// object allows draw additional graphics
    /// </summary>
    public class SDraw : SLoggerBase
    { 
        /// <summary>
        /// gets/sets source object 
        /// </summary>
        public SEntitiesBase            source               { get; set; }  
        /// <summary>
        /// gets all IGraphicsEntity objects 
        /// </summary>
        public List<IGraphicsEntity>    graphicsEntities     { get => annoObjects.graphicsEntities; }  
        /// <summary>
        /// gets/sets SAnnoObject dictionary: annoObjects[drawId] ... 
        /// </summary>
        public SAnnoObjects             annoObjects          { get; set; }   // annoObjects[drawId] ... 
        /// <summary>
        /// gets object allows draw additional graphics
        /// </summary>
        internal SDraw(SEntityManager em) : base(em, nameof(SDraw))
        {  
            annoObjects = new SAnnoObjects(em);
        } 
        internal SDraw __SetSource(SEntitiesBase ents)
        {
            source = ents;
            return this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Clear:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// clears total all additional graphics
        /// </summary>
        public SEntitiesBase Clear()
        {
            using (logger.StartStopLog("api.Graphics.Scene.Clear(...)"))
            { 
                api.Graphics.Scene.Clear();
            }
            return source;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Draw:
        //
        // -------------------------------------------------------------------------------------------
        public SEntitiesBase Normals(string drawId, double scale = 1.0, int color = 0x0000FF, int lineWeight = 2, bool allNormals = false, bool addText = false)
        {
            Null(source, nameof(source), nameof(Normals)); // if (source == null) throw new Exception($"SDraw.Normals(...): Null error: source == null. ");
            if (source is SElemFaces efs)
            {
                List<IGraphicsEntity> x = SDrawUtils.DrawNormals(efs, api, scale, color, lineWeight, allNormals, addText);
                annoObjects.Add(drawId, x); 
            }
            else throw new Exception($"SDraw.Normals(...): To draw normals is not supported for this type of entities. ");
            return source;
        }
        public SEntitiesBase Ids(string drawId, int color = 0x0000FF)
        {
            Null(source, nameof(source), nameof(Ids)); // if (source == null) throw new Exception($"SDraw.Ids(...): Null error: source == null. "); 
            annoObjects.Add(drawId, SDrawUtils.DrawEntIds(source, api, color)); 
            return source;
        }
        public SEntitiesBase ResultProbes(string drawId, int[] onlyIds, string withMorph = "N/A", int color = 0x0000FF)
        {

            Null(onlyIds, nameof(onlyIds), nameof(ResultProbes)); // if (onlyIds == null) throw new Exception($"SDraw.ResultProbes(...): Null error: onlyIds == null. "); 
            annoObjects.Add(drawId, SDrawUtils.DrawNodesResultProbes(source.nodes, onlyIds, api, withMorph, color: color)); 
            return source;
        } 
        public SEntitiesBase ResultProbes(string                    drawId, 
                                          Dictionary<int, double>   nodalValues, 
                                          SResultProbeTypes         type, 
                                          string                    withMorph     = "N/A", 
                                          string                    unit          = "N/A", 
                                          Func<double, string>      ValueToString = null, 
                                          int                       color         = 0x0000FF,
                                          bool                      addUnit       = true,
                                          bool                      addIndex      = true)
        {

            Null(nodalValues, nameof(nodalValues), nameof(ResultProbes)); // if (nodalValues == null) throw new Exception($"SDraw.ResultProbes(...): Null error: nodalValues == null. "); 
            List<int> onlyIds = new List<int>();
            if (type == SResultProbeTypes.GlobalMax)
            { 
                onlyIds.Add(nodalValues.Keys.OrderBy(id => nodalValues[id]).Last());
            }
            else if (type == SResultProbeTypes.GlobalMin)
            { 
                onlyIds.Add(nodalValues.Keys.OrderBy(id => nodalValues[id]).First());
            }
            else if (type == SResultProbeTypes.LocalMax)
            {
                List<int> ids = nodalValues.Keys.ToList();
                List<int> locals = source.nodes
                                         .AsParallel()
                                         .WithDegreeOfParallelism(em.CPUs)
                                         .Where(n => nodalValues[n.id] >= n.elems.nodes.ids.Intersect(ids).Max(id => nodalValues[id]))
                                         .Select(n => n.id)
                                         .ToList();
                onlyIds.AddRange(locals);
            }
            else if (type == SResultProbeTypes.LocalMin)
            {
                List<int> ids = nodalValues.Keys.ToList();
                List<int> locals = source.nodes
                                         .AsParallel()
                                         .WithDegreeOfParallelism(em.CPUs)
                                         .Where(n => nodalValues[n.id] <= n.elems.nodes.ids.Intersect(ids).Min(id => nodalValues[id]))
                                         .Select(n => n.id)
                                         .ToList();
                onlyIds.AddRange(locals);
            } 
            annoObjects.Add(drawId, SDrawUtils.DrawNodesResultProbes(source.nodes, onlyIds.ToArray(), api, withMorph, nodalValues, unit, color, ValueToString, addUnit, addIndex)); 
            return source;
        }
        public SEntitiesBase Nodes(string drawId, int color = 0x0000FF, int size = 5, string withMorph = "N/A")
        {
            Null(source, nameof(source), nameof(Nodes)); // if (source == null) throw new Exception($"SDraw.Ids(...): Null error: source == null. ");
            annoObjects.Add(drawId, SDrawUtils.Nodes(source.nodes, api, color, size, withMorph)); 
            return source;
        }
        public SColors DrawLegend(string drawId, SAnnoLegend.SContourSettings set, bool doDraw = true)
        {
            try
            {
                logger.Msg($"DrawLegend(...)");
                logger.Msg($" - drawId : {drawId}");
                set.WriteLog(logger.Msg);
                //
                //  legend object:
                //
                SAnnoLegend legend = new SAnnoLegend(em);
                SColors     colors = new SColors(); 
                //
                //  bands:
                //
                colors.CreateBands(set);
                if (doDraw)
                {
                    if (set == null)  legend.DrawAnnoLegend(head1: new List<string> { "Entity Manager" },
                                                            head2: new List<string> { "Result" },
                                                            head3: new List<string> { $"Unit : N/A" },
                                                            colors: colors.bands);
                    else              legend.DrawAnnoLegend(head1: new List<string> { set.label1 },
                                                            head2: new List<string> { set.label2 }, 
                                                            head3: new List<string> { $"Unit : {set.unit}" },
                                                            colors: colors.bands);
                    annoObjects.Add(drawId, legend, "legend");
                }
                return colors; 
            }
            catch (Exception err) { Throw(err, nameof(DrawLegend)); }  // catch (Exception err) { throw new Exception($"DrawLegend(...): {err.Message}", err); }
            return null;
        }
        public SEntitiesBase NodalResults(string drawId, int size = 5, string withMorph = "N/A", SAnnoLegend.SContourSettings contourSettings = null)
        {
            try
            {
                Null(source, nameof(source), nameof(NodalResults)); // if (source == null) throw new Exception($"Null error: current == null. ");
                logger.Msg("NodalResults(...)");
                //
                //  to draw:
                //
                SNodes toDrw = source.nodes;   
                //
                //  legend:
                // 
                if (contourSettings == null)
                {
                    (double min, double avg, double max) = toDrw.result.minAvgMax;
                    contourSettings = new SAnnoLegend.SContourSettings("M3Opti: Nodal Result", drawId, 9, min, max, toDrw.result.unit);
                }
                SColors colors = DrawLegend(drawId, contourSettings);
                //
                //  nodes in bands:
                //
                foreach (SColors.SColor band in colors.bands)
                {
                    SNodes b = toDrw.If(n => band.Contain(n.resultValue));
                    List<IGraphicsEntity> ents = SDrawUtils.Nodes(b, api, band.color, size, withMorph);
                    annoObjects.Add(drawId, ents);
                }
                //
                //  return:
                //
                return source; 
            }
            catch (Exception err) { Throw(err, nameof(NodalResults)); }  // catch (Exception err) { throw new Exception($"NodalResults(...): {err.Message}", err); }
            return null;
        }
        public SEntitiesBase Show(string drawId)
        {
            annoObjects[drawId].ForEach(a => a.Visible = true);
            return source;
        }
        public SEntitiesBase Hide(string drawId)
        {
            annoObjects[drawId].ForEach(a => a.Visible = false); 
            return source;
        } 
        /// <summary>
        /// draws nodal contour result with legend
        /// </summary>
        public SEntitiesBase ContourResults(string                        drawId, 
                                            string                        withMorph               = "N/A", 
                                            Dictionary<int, double>       nodalValues             = null,    
                                            SAnnoLegend.SContourSettings  contourSettings         = null,  
                                            int                           wireLineWeight          = 2,
                                            bool                          showWireframe           = false,
                                            bool                          showAllBorderWires      = false,
                                            int                           wireColor               = 0x9999FF, 
                                            bool                          showContours            = true,
                                            bool                          showLegend              = true,
                                            double                        translucencyContour     = 0.0,
                                            double                        translucencyWireframe   = 0.0,
                                            bool                          useParallel             = true)
        {
            using (logger.StartStopLog(nameof(ContourResults)))
            {
                try
                {
                    Null(source, nameof(source), nameof(ContourResults)); // if (source == null) throw new Exception($"Null error: current == null. ");
                    logger.Msg($"ContourResults(...)");
                    logger.Msg($" - drawId : {drawId}");
                    //
                    //  to draw:
                    //
                    SNodes     nodes  = source.nodes;
                    SElemFaces eFaces = nodes.elemFacesIn;
                    //
                    //  legend:
                    // 
                    if (contourSettings == null)
                    {
                        (double min, double avg, double max) = nodes.result.minAvgMax;
                        contourSettings = new SAnnoLegend.SContourSettings("M3Opti: Contour Result", drawId, 9, min, max, nodes.result.unit);
                    }
                    SColors colors = DrawLegend(drawId, contourSettings, showLegend);
                    //
                    //  nodes in bands:
                    //
                    if (nodalValues == null)
                    {
                        nodalValues = new Dictionary<int, double>();
                        foreach (SNode n in nodes) nodalValues[n.id] = n.resultValue; 
                    }
                    //
                    //  morph:
                    //
                    Dictionary<int, double[]> morphCoords = new Dictionary<int, double[]>();
                    foreach (SNode n in nodes)
                    {
                        SMorphNode nn     = nodes.morph[withMorph][n.id];
                        morphCoords[n.id] = nn.xyz;
                    }
                    //
                    //  draw:
                    //  
                    using (logger.StartStopLog($"creating moving field ... "))
                    {    
                        string errMsg = "It could by caused by changed mesh topology before post-processing. ";
                        SAnnoContourShells anno = new SAnnoContourShells(em); 
                        anno.DrawContourShells(eFaces, GetNodalValue:           (id) => nodalValues.ContainsKey(id) ? nodalValues[id] : throw new Exception($"GetNodalValue(...): Node id '{id}' cannot be found in dictionary 'nodalValues'. {errMsg} "),  
                                                       GetMorphCoords:          (id) => morphCoords.ContainsKey(id) ? morphCoords[id] : throw new Exception($"GetMorphCoords(...): Node id '{id}' cannot be found in dictionary 'morphCoords'. {errMsg} "),  
                                                       colorBands:              colors, 
                                                       showContours:            showContours,
                                                       sphereSize:              0.000125,
                                                       showWireframe:           showWireframe,
                                                       wireLineWeight:          wireLineWeight,
                                                       showAllBorderWires:      showAllBorderWires,
                                                       wireColor:               wireColor,         
                                                       translucencyContour:     translucencyContour,
                                                       translucencyWireframe:   translucencyWireframe,
                                                       useParallel:             useParallel); 
                        //
                        //  save objects:
                        //
                        annoObjects.Add(drawId, anno, "contour-shells");
                    }
                    //
                    //  return:
                    //
                    return source; 
                }
                catch (Exception err) { Throw(err, nameof(ContourResults)); }  // catch (Exception err) { throw new Exception($"ContourResults(...): {err.Message}", err); }
            }
            return null;
        }
        /// <summary>
        /// draws nodal contour result with legend
        /// </summary>
        /// <example><code>
        /// with ExtAPI.Graphics.Suspend():
        ///     em = EM()
        ///     em.ClearGraphics()
        ///     em.faces.nodes.result.Assign("Total Deformation")
        ///     em.faces.draw.ContourResults("c1", 0.0)
        /// # -----------------------    
        /// with ExtAPI.Graphics.Suspend():
        ///     em = EM()
        ///     em.ClearGraphics()
        ///     em.faces.nodes.result.Assign("Total Deformation")
        ///     em.faces.draw.ContourResults("c1", 0.0, exportVTKDataToFile = r"e:\Garrett - Augste\SIMDEV-16-Kinematics\pokus-vtk.contour")
        ///     em.logger.Show()
        /// </code></example>
        public SEntitiesBase ContourResults(string  drawId,
                                            double  deformationScale,
                                            int     wireLineWeight          = 2,
                                            bool    showWireframe           = false,
                                            bool    showAllBorderWires      = false,
                                            int     wireColor               = 0x9999FF, 
                                            bool    showContours            = true,
                                            bool    showLegend              = true,
                                            double  translucencyContour     = 0.0,
                                            double  translucencyWireframe   = 0.0,
                                            bool    useParallel             = true,
                                            string  exportVTKDataToFile     = "N/A")    
        {
            using (logger.StartStopLog(nameof(ContourResults)))
            { 
                try
                {
                    Null(source, nameof(source), nameof(ContourResults)); // if (source == null) throw new Exception($"Null error: current == null. ");
                    logger.Msg($"ContourResults(...)");
                    logger.Msg($" - drawId : {drawId}");
                    //
                    //  to draw:
                    //
                    SNodes     nodes  = source.nodes;
                    SElemFaces eFaces = nodes.elemFacesIn;
                    //
                    //  legend:
                    // 
                    (double min, double avg, double max) = nodes.result.minAvgMax;
                    SAnnoLegend.SContourSettings contourSettings = new SAnnoLegend.SContourSettings("Contour Result", drawId, 9, min, max, nodes.result.unit);
                    SColors colors = DrawLegend(drawId, contourSettings, showLegend);
                    //
                    //  nodes in bands:
                    //
                    Dictionary<int, double> nodalValues = new Dictionary<int, double>();
                    foreach (SNode n in nodes) nodalValues[n.id] = n.resultValue;  
                    //
                    //  scale:
                    //
                    if (deformationScale != 0.0) ToDo(nameof(deformationScale));
                    //
                    //  draw:
                    //  
                    using (logger.StartStopLog($"creating moving field ... "))
                    {    
                        string errMsg = "It could by caused by changed mesh topology before post-processing. ";
                        SAnnoContourShells anno = new SAnnoContourShells(em); 
                        anno.DrawContourShells(eFaces, GetNodalValue:           (id) => nodalValues.ContainsKey(id) ? nodalValues[id] : throw new Exception($"GetNodalValue(...): Node id '{id}' cannot be found in dictionary 'nodalValues'. {errMsg} "),  
                                                       GetMorphCoords:          null,  
                                                       colorBands:              colors, 
                                                       showContours:            showContours,
                                                       sphereSize:              0.000125,
                                                       showWireframe:           showWireframe,
                                                       wireLineWeight:          wireLineWeight,
                                                       showAllBorderWires:      showAllBorderWires,
                                                       wireColor:               wireColor,         
                                                       translucencyContour:     translucencyContour,
                                                       translucencyWireframe:   translucencyWireframe,
                                                       useParallel:             useParallel,
                                                       exportVTKDataToFile:     exportVTKDataToFile); 
                        //
                        //  save objects:
                        //
                        annoObjects.Add(drawId, anno, "contour-shells");
                    }
                    //
                    //  return:
                    //
                    return source; 
                }
                catch (Exception err) { Throw(err, nameof(ContourResults)); }  // catch (Exception err) { throw new Exception($"ContourResults(...): {err.Message}", err); }
            }
            return null;
        }
        
    }
}
 
