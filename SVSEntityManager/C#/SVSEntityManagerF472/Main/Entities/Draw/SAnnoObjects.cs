#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member

using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
// using System.Threading;

using SVSExceptionBase;
using SVSLoggerF472;

//
//  Ansys:
//
// using Ansys.Mechanical.DataModel.Converters;
// using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
// using Ansys.Mechanical.DataModel.Enums;
// using Ansys.ACT.Interfaces.Mesh;
// using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
// using Ansys.ACT.Mechanical.Tools;
// using Ansys.ACT.Common.Graphics;


namespace SVSEntityManagerF472
{
    public class SAnnoObjects : SLoggerBase
    {

        public List<SAnnoObject>                      this[string drawId]     { get => Get(drawId); set => objectsDict[drawId] = value; }  
        public List<string>                           drawIds                 { get => objectsDict.Keys.ToList(); }  
        public List<SAnnoObject>                      objects                 { get => objectsDict.Values.SelectMany(o => o).ToList(); }  
        public Dictionary<string, List<SAnnoObject>>  objectsDict             { get; set; }
        public List<IGraphicsEntity>                  graphicsEntities        { get => objects.SelectMany(o => o.graphicsEntities).ToList(); }  
        public SAnnoObjects(SEntityManager em) : base(em, nameof(SAnnoObjects))
        { 
            objectsDict = new Dictionary<string, List<SAnnoObject>>();
        } 
        private List<SAnnoObject> Get(string drawId)
        {
            if (!objectsDict.ContainsKey(drawId)) objectsDict[drawId] = new List<SAnnoObject>(); 
            return objectsDict[drawId];
        }
        public void Add(string drawId, List<IGraphicsEntity> ents)
        {
            SAnnoObject anno = new SAnnoObject(em);
            anno.graphicsEntities.AddRange(ents);
            this[drawId].Add(anno);
        }
        public void Add(string drawId, SAnnoObject anno, string msgLabel)
        { 
            logger.Msg($"SAnnoObjects.Add(...):");
            logger.Msg($" - msgLabel : {msgLabel}");
            logger.Msg($" - drawId   : {drawId}");
            logger.Msg($" - entities : {anno.graphicsEntities.Count}");
            this[drawId].Add(anno);
        }
        public void Add(string drawId, List<SAnnoObject> annos)
        { 
            this[drawId].AddRange(annos);
        }
    }
}

