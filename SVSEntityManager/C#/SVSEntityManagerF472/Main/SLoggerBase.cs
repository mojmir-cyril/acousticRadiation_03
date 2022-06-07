#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member 


using System;
using SVSLoggerF472;

//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical;
using SVSExceptionBase;
using System.Reflection;
using System.IO;

namespace SVSEntityManagerF472
{
    public abstract class SLoggerBase : SExceptionBase
    {
        /// <summary>
        /// The SEntityManager object created by SVS FEM s.o.r. for fast/easy work with geometrical entitites.
        /// The main instance (em) genarally keeps all necessary settings for selecting.
        /// </summary>
        public SEntityManager                            em                      { get; set; }
        /// <summary>
        /// ANSYS Mechanical ExtAPI
        /// </summary>
        public IMechanicalExtAPI                         api                     { get; }
        /// <summary>
        /// logging events to ANSYS Extensions Log File and defined log file
        /// </summary>
        public SLogger                                   logger                  { get; }  
        public SLoggerBase(SEntityManager em, string logCaption = "N/A")
        {
            //
            //  bezny objekt
            //
            Null(em, nameof(em), nameof(SLoggerBase));  
            this.em = em; 
            api     = em.api;
            logger  = em.logger;
            if (logCaption != "N/A") logger.Msg($"{logCaption}(...)");  
        }
        public SLoggerBase(IMechanicalExtAPI api, string logCaption)
        { 
            Null(api, nameof(api), nameof(SLoggerBase));   
            this.api = api; 
            //
            //  SLogger:
            // 
            string logFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logger = SLogger.GetInstance(this, api, logFolder: logFolder);
            logger.Msg($"{logCaption}(...)");  
        }
    }
}
 
