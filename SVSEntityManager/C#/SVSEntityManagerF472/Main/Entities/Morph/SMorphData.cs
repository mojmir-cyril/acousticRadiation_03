#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO;
using System.Collections;

using Ansys.ACT.Interfaces.Mesh;
using SVSLoggerF472;
using SVSExceptionBase;
using SVSConvertF472;

namespace SVSEntityManagerF472
{
    public class SMorphData : SLoggerBase
    {  
        public string                    id                 { get; }
        public SMorph                    morph              { get; } 
        public List<SMorphNode>          data               { get; private set; }
        public SNodes                    nodes              { get; } 
        public IMeshData                 mesh               { get => morph.mesh; }
        public int                       count              { get => data.Count(); }
        public SMorphNode                this[int nodeId]   { get => data.Find(n => n.nodeId == nodeId); }
        public string                    unit               { get; private set; } = "N/A";
        public SMorphData(SMorph morph, SNodes nodes, string dataId) : base(morph.em, nameof(SMorphData))
        {
            this.morph = morph;
            id         = dataId;
            this.nodes = nodes;
        }
        public SMorphData Save(string fileName = "N/A", char decimalSeparator = '.', char cellSeparator = ';')
        {
            using (logger.StartStop($"SMorphData.Save"))
            {
                unit = mesh.Unit;
                data = nodes.Select(n => new SMorphNode(n)).ToList();
                if (fileName != "N/A") File.WriteAllText(fileName, GetCSV(decimalSeparator, cellSeparator)); 
                return this;
            }
        }
        public SMorphData Load(string fileName = "N/A", char decimalSeparator = '.', char cellSeparator = ';')
        {
            using (logger.StartStop($"SMorphData.Load"))
            {
                try
                {
                    (string u, List<SSimplePoint> points) = LoadFromFile(fileName, decimalSeparator, cellSeparator, em.logger);
                    List<int>                     ids1    = nodes.ids.ToList();
                    List<int>                     ids2    = points.Select(p => p.id).ToList(); 
                    List<int>                     ids3    = ids2.Union(ids1).Distinct().ToList();
                    Dictionary<int, int>          dIds    = ids1.ToDictionary(i => i, i => i); 
                    //
                    //
                    //
                    if (u != mesh.Unit) throw new Exception($"Load(...): Different units. u = '{u}',  mesh.Unit = '{ mesh.Unit}'. ");
                    //
                    //  log:
                    //
                    logger.Msg($"SMorphData.Load(...):");
                    logger.Msg($" - ids1 : {ids1.Count}");
                    logger.Msg($" - ids2 : {ids2.Count}");
                    logger.Msg($" - ids3 : {ids3.Count}");
                    //
                    //  check:
                    //
                    if (ids1.Count > ids3.Count) logger.Wrn($"Source nodes {ids1.Count} is more than correctly loaded nodes {ids3.Count} from file '{fileName}', id = {id}. ");
                    if (ids2.Count > ids3.Count) logger.Wrn($"Nodes {ids2.Count} in file is more than correctly loaded nodes {ids3.Count} from file '{fileName}', id = {id}. ");
                    //
                    //  data:
                    //
                    List<SMorphNode> newData = points.AsParallel() // .AsOrdered()
                                                     .WithDegreeOfParallelism(8)
                                                     .Where(p => dIds.ContainsKey(p.id))  // ids1.Contains(p.id)
                                                     .Select(p => new SMorphNode(em, p.id, p.x, p.y, p.z))
                                                     .ToList();
                    data = newData;
                    return this; 
                }
                catch (Exception err) { Throw(err, nameof(Load)); }  // catch (Exception err) { throw new Exception($"Load(...): {err.Message}", err); }
            }
            return null;
        } 
        public static (string, List<SSimplePoint>) LoadFromFile(string fileName = "N/A", char decimalSeparator = '.', char cellSeparator = ';', SLogger logger = null)
        {
            try
            { 
                string file = File.ReadAllText(fileName);
                if (cellSeparator != ' ') file = file.Replace(" ", "");
                string[] lines = file.Replace("\r", "\n").Split('\n');
                string unit = lines[0].Replace("[", "").Replace("]", "");
                int ignored = 0;
                List<SSimplePoint> data = new List<SSimplePoint>();
                foreach (string line in lines)
                {
                    try
                    {
                        if (line == lines[0]) continue;
                        if (line == "") throw new Exception();
                        string[] cells = line.Split(cellSeparator);
                        if (cells.Count() <= 3) throw new Exception();
                        int node = SConvert.ToInt32(cells[0]);
                        double D(string v) => SConvert.ToDouble(v.Replace(',', decimalSeparator).Replace('.', decimalSeparator));
                        double x = D(cells[1]);
                        double y = D(cells[2]);
                        double z = D(cells[3]);
                        SSimplePoint n = new SSimplePoint(node, x, y, z);
                        data.Add(n);
                    }
                    catch  { ignored++; }
                }
                if (logger != null)
                {
                    logger.Msg($"{data.Count()} nodes has been loaded from file '{fileName}'. ");
                    if (ignored >= 1) logger.Wrn($"SMorphData.LoadFromFile(...): {ignored} ignored during load file '{fileName}'. ");
                }
                return (unit, data); 
            }
            catch (Exception err) { Throw(err, nameof(LoadFromFile), nameof(LoadFromFile)); }  // catch (Exception err) { throw new Exception($"LoadFromFile(...): {err.Message}", err); } 
            return ("N/A", null);
        }
        public SMorphData Restore(SNodes nodes)
        {
            using (logger.StartStop($"SMorphData.Restore"))
            {
                IList<int> ids = nodes.ids;
                List<SMorphNode> toRestore = data.Where(n => ids.Contains(n.nodeId)).Where(n => n.dist > 0.0).ToList();
                //
                //  log:
                //
                logger.Msg($"Restore(...):");
                logger.Msg($" - count             : {count}");
                logger.Msg($" - nodes.count       : {nodes.count}");
                logger.Msg($" - toRestore.Count() : {toRestore.Count()}");
                logger.Msg($" - max dist          : {toRestore.AsParallel().WithDegreeOfParallelism(8).Max(n => n.dist)}");
                //
                //  move:
                // 
                SMorphUtils.MoveNodesTo(mesh, toRestore, useMXYZ: false); // toRestore.ForEach(n => SMorphUtils.MoveNodeTo(mesh, n.bodyId, n.nodeId, n.xyz)); 
                //
                //  return nodes:
                //
                return this;
            }
        }
        public void Draw(Func<int, int> ColorById = null,
                         Func<int, double[]> Point1XYZs = null,   // use node xyz if null
                         int pointSize1 = 0, 
                         int pointSize2 = 5, 
                         int lineWeight = 1, 
                         bool addDistText = false, bool addNodeId = false)
        {
            SDrawUtils.DrawMovingPoints(data.Cast<ISMorphNode>(), morph.em.api, 
                                        ColorById,  
                                        Point1XYZs,
                                        pointSize1, pointSize2, 
                                        lineWeight, 
                                        addDistText, addNodeId);
        }
        public string GetCSV(char decimalSeparator = '.', char cellSeparator = ';') 
            => $"[{unit}]\n" + string.Join("\n", data.Select(n => n.GetCSVLine(decimalSeparator, cellSeparator)));
    }
}
