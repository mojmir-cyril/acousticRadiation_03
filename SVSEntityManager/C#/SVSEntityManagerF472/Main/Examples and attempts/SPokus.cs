#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace SVSEntityManagerF472.Pokusy
{
    public class SPokus
    {
        public SPokus(IMechanicalExtAPI api)
        {
            dynamic x = api.DataModel.Tree.FirstActiveObject.InternalObject;
            dynamic c = x.AddComment();
            c.Name = "Muj C# comment";

            api.Log.WriteMessage(c.__doc__);
        }
        public SPokus(IMechanicalExtAPI api, dynamic obj) 
        {
            api.Log.WriteMessage(obj.NecoVypis());
            api.Log.WriteMessage(obj.GetType().ToString());

            string x = Ansys.Utilities.ApplicationConfiguration.DefaultConfiguration.AnsysPlatform;

            Ansys.Core.FileManagement.Queries.GetUserFilesDirectoryQuery q = new Ansys.Core.FileManagement.Queries.GetUserFilesDirectoryQuery();
            string user = new Ansys.Core.FileManagement.Queries.GetUserFilesDirectoryQuery().UserFilesDirectoryPath.Get();
            api.Log.WriteMessage(user);

        }
    }
}
 