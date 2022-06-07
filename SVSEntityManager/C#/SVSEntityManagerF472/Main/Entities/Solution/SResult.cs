#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 

using System;
using System.Collections.Generic;
using System.Linq; 

using SVSConvertF472;
using SVSExceptionBase; 
using System.IO;
using System.Globalization; 

using Ansys.Core.Units;
using Ansys.ACT.Automation.Mechanical; 
using Ansys.ACT.Automation.Mechanical.Results;
using Ansys.Mechanical.DataModel.Results;

namespace SVSEntityManagerF472
{
    public class SResult : SLoggerBase
    { 
        private Dictionary<int, SNode>          nodeRess            { get; set; }  // nodeId : solutionNode 
        //                                                          
        //  public:                                                 
        //                                                          
        public DataModelObject                  iResult             { get; private set; }  // Result, UserDefinedResult
        public string                           name                { get => iResult.Name; } 
        public bool                             isResult            { get => iResult is Result; }
        public bool                             isUserDefinedResult { get => iResult is UserDefinedResult; }
        public double                           time                { get => iResult is Result            r1 ? r1.Time.ConvertUnit(em.timeUnit).Value : 
                                                                             iResult is UserDefinedResult r2 ? r2.Time.ConvertUnit(em.timeUnit).Value : 
                                                                             throw new Exception($"time: Unsupported result type '{iResult.GetType()}', Result or UserDefinedResult object types are supported only. ");
                                                                      set => throw new Exception("TO-DO"); }
        public SNode                            this[int nodeId]    { get => nodeRess[nodeId]; }
        public List<int>                        nodeIds             { get => nodeRess.Keys.ToList(); }
        public List<double>                     values              { get => nodeRess.Values.Select(v => v.value).ToList(); }
        public int                              nodeCount           { get => nodeRess.Keys.Count(); }
        public SNodes                           source              { get; set; }
        public (double, double, double)         minAvgMax           { get => (values.Min(), values.Average(), values.Max()); }
        public string                           unit                { get; set; } = "N/A";
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        public SResult(SEntityManager em) : base(em, nameof(SResult)) 
        {  
            nodeRess = new Dictionary<int, SNode>();
        }
        // -------------------------------------------------------------------------------------------
        //
        //      __SetSource:
        //
        // -------------------------------------------------------------------------------------------  
        internal SResult __SetSource(SNodes nodes)
        {
            source = nodes;
            return this;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Assign:
        //
        // ------------------------------------------------------------------------------------------- 
        //
        //  em = EM()
        //  em.Node(2427).nodes.result.Assign("NLEPEQ - SHELL") 
        //  n=em.Node(2427).nodes[0]
        //  print n.resultValue
        //
        // 
        private DataModelObject F(SEntityManager em, int resultId)
        {
            DataModelObject r1 = SSolution.SUtils.FindResult(em, resultId);
            DataModelObject r2 = SSolution.SUtils.FindUserResult(em, resultId);
            return r1 ?? r2;
        }
        private DataModelObject F(SEntityManager em, string resultName)
        {
            DataModelObject r1 = SSolution.SUtils.FindResult(em, resultName);
            DataModelObject r2 = SSolution.SUtils.FindUserResult(em, resultName);
            return r1 ?? r2;
        }
        public SResult Assign(int resultId)                    => Assign(F(em, resultId), source);
        public SResult Assign(string resultName)               => Assign(F(em, resultName), source);
        public SResult Assign(string resultName, SNodes nodes) => Assign(F(em, resultName), nodes);
        public SResult Assign(Result result)                   => Assign(result, source);
        public SResult Assign(DataModelObject result, SNodes nodes, bool clearOld = true)
        {
            using (logger.StartStopLog(nameof(Assign)))
            { 
                try
                { 
                    Null(em,            nameof(em),     nameof(Assign));   // if (nodes == null)      throw new Exception($"Null error: nodes == null. ");
                    Null(result,        nameof(result), nameof(Assign));   // if (result == null)     throw new Exception($"Null error: result == null. ");
                    NullAndCount(nodes, nameof(nodes),  nameof(Assign));   // if (nodes.Count() <= 0) throw new Exception($"Count 0 error: nodes.Count() <= 0. ");
                    //
                    //  internal result:
                    //
                    iResult = result;
                    unit    = TryGetUnit(result);
                    //
                    //  clear old:
                    //
                    if (clearOld) nodeRess.Clear();
                    //
                    //  nodes:
                    //
                    List<SNode> resNodes = nodes.Select(node => AddNode(node)).ToList();
                    SData resData = null;
                    if      (result is Result r1)            resData = SRead.Read(em, r1, resNodes.Select(n => n.nodeId));
                    else if (result is UserDefinedResult r2) resData = SRead.Read(em, r2, resNodes.Select(n => n.nodeId));
                    else throw new Exception($"Unsupported result type '{result.GetType()}', Result or UserDefinedResult object types are supported only. "); 
                    //
                    //  unit:
                    //
                    unit = TryGetUnit(iResult); 
                    //
                    //  set:
                    //
                    resNodes.ForEach(n => n.Set(resData[n.nodeId], unit)); 
                    //
                    //  current:
                    //
                    return this; //  em.solutions.current = this;
                }
                catch (Exception err) { Throw(err,  nameof(Assign)); }  // catch (Exception err) { throw new Exception($"SResult.Assign(...): {err.Message}", err); }
            }
            return null;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      unit:
        //
        // -------------------------------------------------------------------------------------------    
        public static string TryGetUnit(DataModelObject iResult)
        {
            //
            //  o = Tree.FirstActiveObject
            //  EntityManager.SResult.TryGetUnit(o)
            //
            //
            try 
            { 
                dynamic io = iResult.InternalObject;    // unitString
                string unit = io.PlotDataUnitString;    // isUserDefinedResult ? io.TableUnitString : io.TableUnitString
                if (unit == "") return "N/A";
                return unit;
            }  
            catch 
            { 
                return "N/A"; 
            } 
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      AddNode & Stats:
        //
        // -------------------------------------------------------------------------------------------        
        private SNode AddNode(SVSEntityManagerF472.SNode node)
        {
            Null(em, nameof(em), nameof(AddNode));  // if (node == null) throw new Exception($"SResult.__Assign(...): Null error: node == null. ");
            int id = node.id;
            SNode resNode = new SNode(this, id);
            nodeRess[id] = resNode;
            return resNode;
        }
        public string Stats(Action<string> Log = null)
        {
            string m = ""; 
            m += $"SResult.Stats(...):            <br>";
            m += $" - min : {values.Min()}        <br>";
            m += $" - max : {values.Max()}        <br>";
            m += $" - avg : {values.Average()}    <br>"; 
            logger.Msg(m);
            return m;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      load & save:
        //
        // ------------------------------------------------------------------------------------------- 
        public SResult SaveToFile(string fileName, char decimalSeparator = '.', char cellSeparator = ';')
        {
            try
            {
                List<int>    ns   = nodeRess.Keys.ToList();
                List<double> vs   = nodeRess.Values.Select(v => v.value).ToList();
                SData  data = new SData(em, ns, vs, unit);
                SRead.SaveToFile(data, fileName, decimalSeparator, cellSeparator); 
                return this;
            }
            catch (Exception err) { Throw(err,  nameof(SaveToFile)); }  // catch (Exception err) { throw new Exception($"SaveToFile(...): {err.Message}", err); }
            return null;
        }
        public SResult LoadFromFile(string fileName, char decimalSeparator = '.', char cellSeparator = ';')
        {
            try
            {
                SData data = SRead.LoadFromFile(em, fileName, decimalSeparator, cellSeparator);  
                nodeRess = data.ToResultNodeDict();
                unit     = data.unit;
                return this;
            }
            catch (Exception err) { Throw(err,  nameof(LoadFromFile)); }  // catch (Exception err) { throw new Exception($"LoadFromFile(...): {err.Message}", err); }
            return null;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      ToDictionary:
        //
        // ------------------------------------------------------------------------------------------- 
        public Dictionary<int, double> ToDictionary()  // dict[nodeId] = resultValue
            => nodeIds.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);   
        // -------------------------------------------------------------------------------------------
        //
        //      ToDictionary:
        //
        // ------------------------------------------------------------------------------------------- 
        public SResult ConvertTo(string targetUnit)
        {
            try
            {
                nodeRess.Values.AsParallel().ForAll(n => n.ConvertTo(targetUnit));
                return this; 
            } 
            catch (Exception err) { Throw(err,  nameof(ConvertTo)); }  // catch (Exception err) { throw new Exception($"ConvertTo(...): targetUnit = '{targetUnit}', {err.Message}", err); }
            return null; 
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Data:
        //
        // ------------------------------------------------------------------------------------------- 
        public class SData : SLoggerBase
        {
            private  Dictionary<int, double>   data                { get; set; }
            private List<int>                  nodes               { get => data.Keys.ToList();   }
            private List<double>               values              { get => data.Values.ToList(); }
            public double                      this[int nodeId]    { get => GetValue(nodeId); }
            public string                      unit                { get; set; } = "N/A";
            public SData(SEntityManager em) : base(em, nameof(SData))  // empty
            {
                data = new Dictionary<int, double>();
            }
            public SData(SEntityManager em, Dictionary<int, double> valueDict, string unit) : base(em, nameof(SData))
            {
                this.unit = unit;
                data = new Dictionary<int, double>(valueDict);
            }
            public SData(SEntityManager em, List<int> nodes, List<double> values, string unit) : base(em, nameof(SData))
            {
                this.unit = unit;
                data = new Dictionary<int, double>();
                for (int i = 0; i < nodes.Count; i++) data[nodes[i]] = values[i]; 
            }
            public SData(SEntityManager em, string fromCSV, char decimalSeparator = '.', char cellSeparator = ';') : base(em, nameof(SData))
            { 
                try
                { 
                    logger.Msg($"SResult.SData(...) ... fromCSV ... ");
                    data = new Dictionary<int, double>();
                    //
                    //  csv:
                    //
                    if (cellSeparator != ' ') fromCSV = fromCSV.Replace(" ", "");
                    string[] lines = fromCSV.Replace("\r", "\n").Split('\n');
                    unit = lines[0].Replace("[", "").Replace("]", "");
                    int ignored = 0;
                    foreach (string line in lines)
                    {
                        try
                        {
                            if (line == "") continue;
                            if (line == lines[0]) continue;
                            string[] cells = line.Split(cellSeparator);
                            if (cells.Count() <= 1) throw new Exception();
                            int node = SConvert.ToInt32(cells[0]);
                            double D(string v) => SConvert.ToDouble(v.Replace(',', decimalSeparator).Replace('.', decimalSeparator));
                            double val = D(cells[1]); 
                            data[node] = val; 
                        }
                        catch  { ignored++; }
                    }
                    logger.Msg($"{data.Count()} nodes has been loaded from CSV string. ");
                    if (ignored >= 1) logger.Wrn($"SResult.SData(...): {ignored} ignored during loading from CSV string. "); 

                }
                catch (Exception err) { Throw(err, nameof(SData)); }  // catch (Exception err) { throw new Exception($"Load(...): {err.Message}", err); } 
            } 
            public double GetValue(int nodeId)
            {
                if (data.Keys.Count() <= 0)      throw new Exception($"SResult.SData.GetValue(...): Result data is empty. ");
                if (!data.Keys.Contains(nodeId)) throw new Exception($"SResult.SData.GetValue(...): Result data has no value for node '{nodeId}'. ");
                return data[nodeId];
            }
            public void AddValue(int nodeId, double resultValue)
            {
                if (data.Keys.Contains(nodeId))
                { 
                    if (resultValue != data[nodeId])
                        throw new Exception($"SResult.SData.AddValue(...): Result data already has value for node '{nodeId}' " +
                                            $"and value is different ({resultValue} != {data[nodeId]}), threfore, " +
                                            $"node cannot be added. ");
                    return;
                }
                data.Add(nodeId, resultValue);
            }
            public string ToCSV(char decimalSeparator = '.', char cellSeparator = ';')
            {
                CultureInfo  c   = new CultureInfo("en-US");
                string       s   = c.NumberFormat.NumberDecimalSeparator; 
                string       csv = "";
                for (int i = 0; i < data.Keys.Count; i++)
                {
                    var k = nodes[i].ToString();
                    var v = values[i].ToString().Replace(s, decimalSeparator.ToString());
                    csv += $"{k}{cellSeparator}{v}\n";
                }
                return $"[{unit}]\n" + csv;
            } 
            public Dictionary<int, SNode> ToResultNodeDict()
            {
                Dictionary<int, SNode> d = new Dictionary<int, SNode>();
                for (int i = 0; i < nodes.Count; i++) d[nodes[i]] = new SNode(nodes[i], values[i], unit);
                return d;
            }
            public void ConvertTo(string targetUnit)
            {
                try
                {
                    double scale = unit == "N/A" ? 1.0 : new Quantity(1.0, unit).ConvertUnit(targetUnit).Value;
                    if (scale != 1.0)
                    {
                        Dictionary<int, double> x = data.Keys.ToDictionary(k => k, k => data[k] * scale);
                        data = x;
                    } 
                }
                catch (Exception err) { Throw(err, nameof(ConvertTo)); }  // catch (Exception err) { throw new Exception($"ConvertTo(...): unit = '{unit}', targetUnit = '{targetUnit}', {err.Message}", err); }
            }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Node:
        //
        // ------------------------------------------------------------------------------------------- 
        public class SNode
        { 
            public int              nodeId              { get; }
            public SResult          result              { get; } 
            public string           resultName          { get => result?.name ?? "N/A"; }
            public double           time                { get => result?.time ?? double.NaN; }
            public double           value               { get; private set; } 
            public string           unit                { get; private set; } 
            public bool             isOk                { get => __isOk(); }
            public SNode(SResult result, int nodeId)
            {
                if (result == null) throw new Exception($"SResultNode(...): Null error: result == null. ");
                this.nodeId = nodeId;
                this.result = result;
                this.value  = double.NaN;
            } 
            public SNode(int nodeId, double value, string unit)
            {
                this.nodeId = nodeId; 
                this.value  = value;
                this.unit   = unit;
            } 
            internal void Set(double value, string unit)
            {
                this.value = value;
                this.unit  = unit;
            }
            private bool __isOk( )
            {
                if (value is double.NaN) return false; 
                return true;
            } 
            public void ConvertTo(string targetUnit)
            {
                double scale = unit == "N/A" ? 1.0 : new Quantity(1.0, unit).ConvertUnit(targetUnit).Value;  
                if (scale != 1.0)
                {
                    value = value * scale;
                    unit  = targetUnit;
                }
            }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Node:
        //
        // ------------------------------------------------------------------------------------------- 
        public static class SRead
        {
            //
            //  result = Tree.FirstActiveObject
            //  data   = EntityManager.SResult.SRead.Read(result, [74001, 74000, 73999, 72738])
            //  data   = EntityManager.SResult.SRead.Read(result, 18)
            //  data   = EntityManager.SResult.SRead.Read(result)       # all
            //  unit   = EntityManager.SResult.SRead.Unit(result)       # ---> 'Length'
            //
            //  min(data.Values)
            //  max(data.Values)
            //
            public static string Unit(Result result)                                                    => ((dynamic)result).GetQuantityName(((dynamic)result).InternalObject);  // using Ansys.Common.Interop.AnsCoreObjects;
            public static string Unit(UserDefinedResult result)                                         => result.GetQuantityName();
            public static SData Read(SEntityManager em, Result result, int bodyRefId)                                => new SData(em, result.GetNodalValues(bodyRefId), TryGetUnit(result));
            public static SData Read(SEntityManager em, UserDefinedResult result, int bodyRefId)                     => new SData(em, result.GetNodalValues(bodyRefId), TryGetUnit(result));
            public static SData Read(SEntityManager em, Result result, IEnumerable<int> nodeIds = null)              => Read(em, result.PlotData, nodeIds, TryGetUnit(result));
            public static SData Read(SEntityManager em, UserDefinedResult result, IEnumerable<int> nodeIds = null)   => Read(em, result.PlotData, nodeIds, TryGetUnit(result));
            public static SData Read(SEntityManager em, ResultDataTable plotDataResult, IEnumerable<int> nodeIds = null, string unit = "N/A")
            {
                List<int>    nodes  = plotDataResult["Node"].Cast<int>().ToList();
                List<double> values = plotDataResult["Values"].Cast<double>().ToList();
                SData        data   = new SData(em);
                //
                //  unit:
                //
                data.unit = unit;
                //
                //  fill:
                //
                if (nodeIds != null)
                {
                    for (int i = 0; i < nodes.Count(); i++)
                        if (nodeIds.Contains(nodes[i]))
                            data.AddValue(nodes[i], values[i]);
                }
                else
                {
                    for (int i = 0; i < nodes.Count(); i++)
                        data.AddValue(nodes[i], values[i]);
                }
                //
                //  return:
                //
                return data;
            }
            public static void SaveToFile(SData resultData, string fileName, char decimalSeparator = '.', char cellSeparator = ';')
            {  
                string data = resultData.ToCSV(decimalSeparator, cellSeparator);
                File.WriteAllText(fileName, data);
            }
            public static SData LoadFromFile(SEntityManager em, string fileName, char decimalSeparator = '.', char cellSeparator = ';')
            {
                string fromCSV = File.ReadAllText(fileName);
                return new SData(em, fromCSV, decimalSeparator, cellSeparator);
            }

        }
    }
}
