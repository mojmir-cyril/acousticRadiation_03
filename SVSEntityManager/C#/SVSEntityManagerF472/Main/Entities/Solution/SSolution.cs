#pragma warning disable IDE1006                         // Naming Styles
// #pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member
 
using System;
using System.Linq; 
using System.Collections.Generic;

using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Post;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.Core.Units;
using Ansys.Mechanical.DataModel.Enums;
using Ansys.ACT.Automation.Mechanical.Results;

using SVSExceptionBase;


namespace SVSEntityManagerF472
{
    /// <summary>
    /// gets SSolution object which keeps solution data from assigned analysis/solution (direct access to RST file),
    /// only one solution can bee assigned to Entity Manager,
    /// ACT result reader is used:
    /// IResultReader reader = analysis.GetResultsData();
    /// </summary>
    /// <example>
    /// <code>
    /// em = EM()  
    /// s = em.solution.Assign("Static Structural") # by analysis name in the Mechanical tree 
    /// </code>
    /// </example>
    public class SSolution : SLoggerBase
    {
        private Dictionary<int, SNode>      __nodeSols          { get; }  // nodeId : solutionNode
        private STimes                              __times             { get; set; } 
        /// <summary>
        /// Mechanical Analysis (ACT) object wich is assined for getting of a solution
        /// </summary>
        public Analysis                             analysis            { get; private set; }
        /// <summary>
        /// Mechanical Analysis (ACT) object name
        /// </summary>
        public string                               analName            { get => analysis.Name; }
        /// <summary>
        /// gets STimes object which contains time points from an Analysis
        /// which is assgined by SSolution object
        /// </summary>
        /// <exmple>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1)
        /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
        /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
        /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
        /// </code>
        /// </exmple>
        public STimes                               times               { get => __times != null ? __times : throw new Exception("SSolution.times: Solution is not loaded. "); 
                                                                          set => __times = value; }
        /// <summary>
        /// gets count of time points in assgined Analysis
        /// </summary>
        public int                                  timesCount          { get => times.count; }
        /// <summary>
        /// gets SSolution.SNode object by node id
        /// </summary>
        public SNode                        this[int nodeId]    { get => __nodeSols[nodeId]; } 
        /// <summary>
        /// gets all node ids for which is the solution defined
        /// </summary>
        public List<int>                            nodeIds             { get => __nodeSols.Keys.ToList(); }
        /// <summary>
        /// gets count of nodes
        /// </summary>
        public int                                  nodeCount           { get => __nodeSols.Keys.Count(); }
        /// <summary>
        /// gets all nodes
        /// </summary>
        public SNodes                               source              { get; set; }
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// constructor
        /// </summary>
        public SSolution(SEntityManager em) : base(em, nameof(SSolution))
        { 
            __nodeSols    = new Dictionary<int, SNode>();
        } 
        private IResultReader __GetReader(bool useUIThread = true)
        {
            logger.Msg("SSolution.__GetReader(...)");
            //
            //  get:
            //
            IResultReader reader = useUIThread ? (IResultReader)api.Application.InvokeUIThread(analysis.GetResultsData) : analysis.GetResultsData();
            //
            //  check:
            //
            if (reader == null)
            {
                ObjectState s = analysis.Solution.ObjectState;
                if (s != ObjectState.Solved) 
                    throw new Exception($"__GetReader(...): Result reader cannot be obtained (although Solution has Solved state). ");
                throw new Exception($"__GetReader(...): Result reader cannot be obtained. It could be due to that Solution has {s} state. Try to resolve the Analysis). ");
            } 
            //
            //  return:
            //
            return reader;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      __SetSource:
        //
        // -------------------------------------------------------------------------------------------  
        internal SSolution __SetSource(SNodes nodes)
        {
            source = nodes;
            return this;
        } 
        // -------------------------------------------------------------------------------------------
        //
        //      Assign:
        //
        // ------------------------------------------------------------------------------------------- 
        /// <summary>
        /// assigns solution/analysis into SSolution object,
        /// it is necessary before reading solution (dispalcements)
        /// </summary>
        /// <example>
        /// <code>
        /// em = EM() 
        /// s = em.solution.Assign(1) # by analysis index in the Mechanical tree 
        /// </code>
        /// </example>
        public SSolution Assign(int analIndex, string unit = "N/A") => Assign(SUtils.FindAnalysis(em, analIndex), source, unit);
        /// <summary>
        /// assigns solution/analysis into SSolution object,
        /// it is necessary before reading solution (dispalcements)
        /// </summary>
        /// <example>
        /// <code>
        /// em = EM()  
        /// s = em.solution.Assign("Static Structural") # by analysis name in the Mechanical tree 
        /// </code>
        /// </example>
        public SSolution Assign(string analName, string unit = "N/A") => Assign(SUtils.FindAnalysis(em, analName), source, unit);
        /// <summary>
        /// assigns solution/analysis into SSolution object,
        /// it is necessary before reading solution (dispalcements)
        /// </summary>
        /// <example>
        /// <code>
        /// em = EM()  
        /// s = em.solution.Assign(Tree.FirstActiveOject) # by analysis object in the Mechanical tree 
        /// </code>
        /// </example>
        public SSolution Assign(Analysis analysis, string unit = "N/A") => Assign(analysis, source, unit);
        /// <summary>
        /// assigns solution/analysis into SSolution object,
        /// it is necessary before reading solution (dispalcements)
        /// </summary>
        /// <example>
        /// <code>
        /// em = EM()  
        /// s = em.solution.Assign(Tree.FirstActiveOject, nodes)  # by analysis object in the Mechanical tree, used only for given nodes
        /// </code>
        /// </example>
        public SSolution Assign(Analysis analysis, SNodes nodes, string unit = "N/A", bool clearOld = true)
        {
            IResultReader reader = null;
            try
            {
                logger.Msg("SSolution.Assign(...)");  
                //
                //  checks:
                //
                Null(em,            nameof(em));       // if (em == null)         throw new Exception($"Null error: em == null. ");
                Null(analysis,      nameof(analysis)); // if (analysis == null)   throw new Exception($"Null error: analysis == null. ");
                NullAndCount(nodes, nameof(nodes));    // if (nodes == null)      throw new Exception($"Null error: nodes == null. "); if (nodes.Count() <= 0) throw new Exception($"Count 0 error: nodes.Count() <= 0. ");
                //
                //  internal analysis:
                //
                this.analysis = analysis;
                //
                //  clear old:
                //
                if (clearOld) __nodeSols.Clear();
                //
                //  Reader:
                // 
                reader = __GetReader();
                //
                //  check:
                //
                Null(reader, nameof(analysis)); // if (reader == null) throw new Exception($"Null error: reader == null. "); 
                //
                //  read:
                // 
                times        = new STimes(reader.ListTimeFreq); 
                IResult uRes = reader.GetResult("U");  
                double scale = new Quantity(1, uRes.GetComponentInfo("X").Unit).ConvertUnit(unit != "N/A" ? unit : em.lengthUnit).Value; 
                //
                //  log:
                //
                logger.Msg($" - times : {times}");
                logger.Msg($" - scale : {scale}"); 
                //
                //  check:
                //
                Null(uRes, nameof(uRes)); // if (uRes == null) throw new Exception($"Null error: uRes == null. ");
                //
                //  over steps:
                //
                for (int i = 0; i < timesCount; i++)
                {
                    SNode r = null;
                    reader.CurrentResultSet = i + 1; 
                    //
                    //  over nodes:
                    //
                    for (int l = 0; l < nodes.Count(); l++)
                    {
                        SVSEntityManagerF472.SNode node = nodes[l];
                        Null(node, nameof(node)); // if (node == null) throw new Exception($"Null error: node == null. ");
                        //
                        //  log:
                        //
                        if (i <= 5 && l <= 5) logger.Msg($" - time : {i} ... node : {node.id}");
                        if (i <= 5 && l == 6) logger.Msg($" - ... ");
                        //
                        //  assign & reuse:
                        //
                        r = i == 0 ? __AddNode(node): __nodeSols[node.id];
                        //
                        //  vals:
                        //
                        double[] us = uRes.GetNodeValues(node.id).Select((x) => x * scale).ToArray();
                        //
                        //  assign:
                        //
                        r.__Set(i, us);
                    }
                }
                //
                //  current:
                //
                return this; //  em.solutions.current = this;
            }
            catch (Exception err) { Throw(err, nameof(Assign)); }  // catch (Exception err) { throw new Exception($"SSolution.Assign(...): {err.Message}", err); }

            finally { reader?.Dispose(); }
            return null;
        }
        private SNode __AddNode(SVSEntityManagerF472.SNode node)
        { 
            Null(node, nameof(node), nameof(__AddNode)); // if (node == null) throw new Exception($"SSolution.__Assign(...): Null error: node == null. "); 
            int id = node.id; 
            SNode solNode = new SNode(this, id);
            __nodeSols.Add(id, solNode); 
            // node.solutionNode = solNode; 
            solNode.__Dim(times); 
            return solNode;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Node:
        //
        // -------------------------------------------------------------------------------------------

        /// <summary>
        /// SSolution.SNode object which keeps solution data for the node from assigned analysis/solution 
        /// </summary>
        public class SNode
        {
            /// <summary>
            /// gets node id (same id as in ACT)
            /// </summary>
            public int              nodeId              { get; }
            /// <summary>
            /// gets SSolution object which keeps solution data from assigned analysis/solution (direct access to RST file),
            /// only one solution can bee assigned to Entity Manager,
            /// ACT result reader is used:
            /// IResultReader reader = analysis.GetResultsData();
            /// </summary>
            public SSolution        solution            { get; }
            /// <summary>
            /// gets Mechanical Analysis (ACT) object wich is assined for getting of a solution (SSolution)
            /// </summary>
            public Analysis         analysis            { get => solution.analysis; }
            /// <summary>
            /// gets Mechanical Analysis (ACT) object name
            /// </summary>
            public string           analName            { get => analysis.Name; }
            /// <summary>
            /// gets STimes object which contains time points from an Analysis
            /// which is assgined by SSolution object
            /// </summary>
            /// <exmple>
            /// <code>
            /// em = EM() 
            /// s = em.solution.Assign(1)
            /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
            /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
            /// </code>
            /// </exmple>
            public STimes           times               { get => solution.times; }
            /// <summary>
            /// gets count of time points
            /// </summary>
            public int              count               { get => times.count; }
            /// <summary>
            /// gets SDisps object which contains displacement points from an Analysis
            /// which is assgined by SSolution object
            /// </summary>
            /// <exmple>
            /// <code>
            /// em = EM() 
            /// s = em.solution.Assign(1)
            /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
            /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
            /// </code>
            /// </exmple>
            public SDisps           uxs                 { get; private set; }
            /// <summary>
            /// gets SDisps object which contains displacement points from an Analysis
            /// which is assgined by SSolution object
            /// </summary>
            /// <exmple>
            /// <code>
            /// em = EM() 
            /// s = em.solution.Assign(1)
            /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
            /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
            /// </code>
            /// </exmple>
            public SDisps           uys                 { get; private set; }
            /// <summary>
            /// gets SDisps object which contains displacement points from an Analysis
            /// which is assgined by SSolution object
            /// </summary>
            /// <exmple>
            /// <code>
            /// em = EM() 
            /// s = em.solution.Assign(1)
            /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
            /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
            /// </code>
            /// </exmple>
            public SDisps           uzs                 { get; private set; }
            /// <summary>
            /// gets SDisps object which contains displacement points from an Analysis
            /// which is assgined by SSolution object
            /// </summary>
            /// <exmple>
            /// <code>
            /// em = EM() 
            /// s = em.solution.Assign(1)
            /// print em.current[0].times  # gets vector of times from assigned solution (in timeUnit)
            /// print em.current[0].uxs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uys    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].uzs    # gets vector of displacements over time for the first node (in lengthUnit)
            /// print em.current[0].usums  # gets vector of displacements over time for the first node (in lengthUnit)
            /// </code>
            /// </exmple>
            public SDisps           usums               { get; private set; }
            /// <summary>
            /// gets True if solution is available
            /// </summary> 
            public bool             isOk                { get => __isOk(); }
            /// <summary>
            /// gets SSolution.SNode object which keeps solution data for the node from assigned analysis/solution 
            /// </summary>
            public SNode(SSolution solution, int nodeId)
            {
                if (solution == null) throw new Exception($"SSolution.SNode(...): Null error: solution == null. ");
                this.nodeId    = nodeId;
                this.solution = solution;
            }
            internal void __Dim(STimes times)
            {
                if (times == null)      throw new Exception($"SSolution.SNode.__Dim(...): Null error: times == null. ");
                if (this.times == null) throw new Exception($"SSolution.SNode.__Dim(...): Null error: this.times == null. ");
                int c = times.count;
                if (times.values.ToList().Intersect(this.times.values).Count() != c) throw new Exception("SSolution.SNode.__Dim(...): Wrong size of times. ");
                //
                //  new & dim:
                //
                if (uxs == null)    uxs   = uxs   == null ? new SDisps(times, "UX")   : uxs.Dim(c);
                if (uys == null)    uys   = uys   == null ? new SDisps(times, "UY")   : uys.Dim(c);
                if (uzs == null)    uzs   = uzs   == null ? new SDisps(times, "UZ")   : uzs.Dim(c);
                if (usums == null)  usums = usums == null ? new SDisps(times, "USUM") : usums.Dim(c);
            }
            internal void __Set(int set, double[] uxyz)
            {
                if (uxyz.Count() != 3) throw new Exception("SResultNode.Set(...): Wrong dimension of uxyz parameter. ");
                if (set <= -1 || set >= times.count) throw new Exception($"SResultNode.Set(...): Parameter set {set} is out of range (0, {times.count - 1}). ");
                uxs[set] = uxyz[0];
                uys[set] = uxyz[1];
                uzs[set] = uxyz[2];
                Func<double, double> P2 = v => Math.Pow(v, 2);
                usums[set] = Math.Sqrt(P2(uxyz[0]) + P2(uxyz[1])+ P2(uxyz[2]));
            }
            private bool __isOk( )
            {
                if (nodeId      == 0)     return false;
                if (analysis    == null)  return false;
                if (times       == null)  return false;
                if (uxs         == null)  return false;
                if (uys         == null)  return false;
                if (uzs         == null)  return false;
                if (usums       == null)  return false;
                if (times.count == 0)     return false;
                if (uxs.count   != count) return false;
                if (uys.count   != count) return false;
                if (uzs.count   != count) return false;
                if (usums.count != count) return false;
                return true;
            }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Node:
        //
        // -------------------------------------------------------------------------------------------
        public static class SUtils
        {
            public static Result FindResult(SEntityManager em, int resultId)                       => em.tree.results.Where(x => x.ObjectId == resultId).FirstOrDefault();
            public static Result FindResult(SEntityManager em, string resultName)                  => em.tree.results.Where(x => x.Name == resultName).FirstOrDefault();
            public static UserDefinedResult FindUserResult(SEntityManager em, int resultId)        => em.tree.userResults.Where(x => x.ObjectId == resultId).FirstOrDefault();
            public static UserDefinedResult FindUserResult(SEntityManager em, string resultName)   => em.tree.userResults.Where(x => x.Name == resultName).FirstOrDefault();
            public static Analysis FindAnalysis(SEntityManager em, int analIndex)                  => em.tree.anals[analIndex];
            public static Analysis FindAnalysis(SEntityManager em, string analName)                => em.tree.anals.Where(x => x.Parent.Name == analName).FirstOrDefault();
        }
    }
}
