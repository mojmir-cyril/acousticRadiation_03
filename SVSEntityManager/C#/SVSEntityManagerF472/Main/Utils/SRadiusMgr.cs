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
// using Ansys.Mechanical.DataModel.Converters;
// using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
// using Ansys.Mechanical.DataModel.Enums;
// using Ansys.ACT.Interfaces.Mesh;
// using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
// using Ansys.ACT.Mechanical.Tools;
// using Ansys.ACT.Common.Graphics;

using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Geometry;
using System.ComponentModel;
using Ansys.Core.Commands.DiagnosticCommands;
using Ansys.Mechanical.DataModel.Enums;

using Ansys.ACT.Mechanical;
using Ansys.ACT.Core;

namespace SVSEntityManagerF472 
{

    public class SRadiusMgr
    {
        public SEntityManager            em           { get; } 
        public Dictionary<int, double>   radiuses     { get; private set; }
        public SRadiusMgr(SEntityManager em)
        {
            if (em == null) throw new Exception($"SRadiusMgr(...): Null error: api == null. ");
            this.em = em; 
            em.logger.Msg("SRadiusMgr(...)");
            //
            //  all:
            //
            GetAllRadiuses();
        }  
        public void GetAllRadiuses()
        { 
            radiuses = new Dictionary<int, double>();
            //
            //  selection manager:
            //
            dynamic ds        = ((dynamic)em.api.DataModel).InternalObject["ds"];
            int transactionId = ds.GenerateNewTransactionId();
            dynamic sm        = ds.SelectionManager;
            //
            //  transaction:
            //
            ds.UserTransactionStarted(transactionId);
            //
            //  old:
            //
            SEntitiesBase old = em.current;
            //
            //  cyls:
            //
            SFaces cyls = em.faces;//.cyls;
            cyls.Sel();
            for (int i = 1; i < cyls.count + 1; i++)
            {
                int id       = cyls[i - 1].id;
                radiuses[id] = sm.RadiusofSelectedCylinder(1, 1, i);
            }
            if (old != null) old.Sel();



            // throw new Exception($"GetAllRadiuses(...): TO-DO: ... "); 
            // ds.UserTransactionEnded(transactionId);
            //
            //  geometry manager:
            //
            // ds = ExtAPI.DataModel.InternalObject["ds"];
            // transactionId = ds.GenerateNewTransactionId()
            // ds.UserTransactionStarted(transactionId)
            // cur = em.current
            // em.cyls.Sel()
            // print sm.RadiusofSelectedCylinder(1, 1, 1)
            // cur.Sel()
            // ds.UserTransactionEnded(transactionId)
        }
    }
}
