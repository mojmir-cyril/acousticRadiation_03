#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using System.Collections.Generic; 

using Ansys.ACT.Interfaces.Mechanical; 
using Ansys.ACT.Interfaces.Graphics.Entities;
using SVSLoggerF472;

namespace SVSEntityManagerF472
{
    public class SAnnoObject : SAnnoUtils
    { 
        public List<IGraphicsEntity>    graphicsEntities    { get; set; }
        // public SColors                  colors              { get; set; } 
        // --------------------------------------------------------------------------------------------------------
        // 
        //    Methods:
        //
        // --------------------------------------------------------------------------------------------------------
        public SAnnoObject(SEntityManager em) : base(em) 
        { 
            graphicsEntities = new List<IGraphicsEntity>(); 
        } 
        public void Delete()
        {
            if (graphicsEntities.Count() >= 1)
            { 
                logger.Msg("SAnnoObject.Delete(...)");
                logger.Msg($" - GetType()                : <b>{GetType()}                </b>"); 
                logger.Msg($" - graphicsEntities.Count() : <b>{graphicsEntities.Count()} </b>");
                if (graphicsEntities.Count() > 0)                                                    
                    foreach (IGraphicsEntity p in graphicsEntities) 
                        p.Delete();
                            // if (p.Visible) p.Delete();
                            // p.Visible = false;  //   !!!  takze se chova a zapomene se seznam   !!! 
                            // // p.Delete();      //   !!!  Delete je brutalne pomalej            !!! 
                graphicsEntities.Clear(); 
            }
        }
        public void ShowHide(bool toShow)
        {
            // logger.Msg($"{GetType()}.ShowHide(...): {graphicsEntities.Count()} ----> {toShow} ");
            if (graphicsEntities.Count() > 0) foreach (IGraphicsEntity p in graphicsEntities) p.Visible = toShow;
        }
        public bool Visible
        {
            get => graphicsEntities.Count() > 0 ? graphicsEntities[0].Visible : false;   //graphicsPoint3Ds
            set => ShowHide(value); 
        }
        public void Gray(double trans = 0.5)
        {
            logger.Msg($"Gray(...) : {GetType()}");
            if (graphicsEntities.Count() > 0) foreach (IGraphicsEntity p in graphicsEntities) if(p.Visible)
            {
                if (p.Color != 0x999999) { p.Color = 0x999999; p.Translucency = trans; }
                else break; // jakmile narazi na jednu sedou tak konci, predpoklad ze i ostatni jsou sedy !!!
            }
        } 
        public void AddRange(List<IGraphicsEntity> entities) 
        {
            if (graphicsEntities == null) graphicsEntities = new List<IGraphicsEntity>();
            graphicsEntities.AddRange(entities);
        }
    }
}

