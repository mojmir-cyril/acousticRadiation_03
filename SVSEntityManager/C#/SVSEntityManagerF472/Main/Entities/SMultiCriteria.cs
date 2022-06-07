#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVSEntityManagerF472 
{
    public static class SMultiCriteria
    { 
        public static double CalculateScore(SEntity ent, IEnumerable<SCriterion> criteria)
        {
            double score = 0.0;
            //
            //  criteria:
            //
            foreach (SCriterion criterion in criteria)
            {
                try
                {
                    //
                    //  eval:
                    //
                    double val = double.NaN;
                    if (criterion is SForBody) val = ((SForBody)criterion).bodyFunc((SBody)ent);
                    if (criterion is SForFace) val = ((SForFace)criterion).faceFunc((SFace)ent);
                    if (criterion is SForEdge) val = ((SForEdge)criterion).edgeFunc((SEdge)ent);
                    if (criterion is SForVert) val = ((SForVert)criterion).vertFunc((SVert)ent);
                    //
                    //  linear interpolation:
                    //
                    double dif = Math.Abs(val - criterion.targetValue);
                    double rat = dif >= criterion.zeroDifference ? 1.0 : dif / criterion.zeroDifference;
                    double rel = 1.0 - rat;
                    //
                    //  score:
                    //
                    score += rel * criterion.topScore;
                }
                catch { }
            }
            //
            //  return:
            //
            return score;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Base:
        //
        // -------------------------------------------------------------------------------------------
        public abstract class SCriterion
        { 
            public double  targetValue      { get; private set; }
            public double  zeroDifference   { get; private set; }
            public double  topScore         { get; private set; }

            public SCriterion( double targetValue, double zeroDifference, double topScore)
            {  
                this.targetValue    = targetValue;
                this.zeroDifference = zeroDifference;
                this.topScore       = topScore; 
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Body:
        //
        // -------------------------------------------------------------------------------------------
        public class SForBody : SCriterion
        {
            public Func<SBody, double> bodyFunc { get; }

            public SForBody(Func<SBody, double> bodyFunc, double targetValue, double zeroDifference, double topScore) : base(targetValue, zeroDifference, topScore)
            {
                this.bodyFunc = bodyFunc; 
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Face:
        //
        // -------------------------------------------------------------------------------------------
        public class SForFace : SCriterion
        {
            public Func<SFace, double> faceFunc { get; }

            public SForFace(Func<SFace, double> edgeFunc, double targetValue, double zeroDifference, double topScore) : base(targetValue, zeroDifference, topScore)
            {
                this.faceFunc = faceFunc; 
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Edge:
        //
        // -------------------------------------------------------------------------------------------
        public class SForEdge : SCriterion
        {
            public Func<SEdge, double> edgeFunc { get; }

            public SForEdge(Func<SEdge, double> edgeFunc, double targetValue, double zeroDifference, double topScore) : base(targetValue, zeroDifference, topScore)
            {
                this.edgeFunc = edgeFunc; 
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Vert:
        //
        // -------------------------------------------------------------------------------------------
        public class SForVert : SCriterion
        {
            public Func<SVert, double> vertFunc { get; }

            public SForVert(Func<SVert, double> vertFunc, double targetValue, double zeroDifference, double topScore) : base(targetValue, zeroDifference, topScore)
            {
                this.vertFunc = vertFunc; 
            } 
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Body:
        //
        // -------------------------------------------------------------------------------------------
    }
}
