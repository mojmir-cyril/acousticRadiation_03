#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

//
//  Ansys:
//
using Ansys.Mechanical.DataModel.Converters;
using Ansys.Core.Units;
// using Ansys.Common.Interop.WBControls;
// using Ansys.Common.Interop.AnsCoreObjects;
using Ansys.Mechanical.DataModel.Enums;
using Ansys.ACT.Interfaces.Mesh;
using Ansys.ACT.Automation.Mechanical.BoundaryConditions;
using Ansys.ACT.Mechanical.Tools;
using Ansys.ACT.Common.Graphics;
using Ansys.ACT.Interfaces.UserObject;
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Graphics;
using Ansys.ACT.Interfaces.Graphics.Entities;
using Ansys.ACT.Interfaces.Geometry;
using System.ComponentModel;
using Ansys.Core.Commands.DiagnosticCommands;
using Ansys.Mechanical;
using Ansys.Mechanical.DataModel;
using Ansys.ACT.Interfaces.Analysis;
using Ansys.Utilities;
using SVSExceptionBase;

namespace SVSEntityManagerF472
{
    public static class SAct
    {
        // public static void logger.Msg(IExtAPI api, object msg) => api.Log.WriteMessage($"<pre style='background-color:#AACCFF; margin:0px;'>C#: {msg}</pre>");
        // public static void Err(IExtAPI api, object msg) => api.Log.WriteMessage($"<pre style='background-color:#AACCFF; margin:0px;'>C#: <font color=red>{msg}</font></pre>");
        public static string GetExtensionDir(IExtAPI api, string file = "") // IMechanicalExtAPI
        {
            //
            //  SAct.GetExtensionDir()
            //  SAct.GetExtensionDir("file.py")
            //
            string path = api.ExtensionManager.CurrentExtension.InstallDir.Replace("/", "\\");
            if (file != "") return System.IO.Path.Combine(path, file);
            else return path;
        }
        public static string GetImageDir(IExtAPI api, string file = "", string imageFolder = "images") // IMechanicalExtAPI
        {
            //
            //  SAct.GetExtensionDir()
            //  SAct.GetExtensionDir("file.py")
            //
            string path = api.ExtensionManager.CurrentExtension.InstallDir.Replace("/", "\\");
            if (file != "") return System.IO.Path.Combine(path, imageFolder, file);
            else return System.IO.Path.Combine(path, imageFolder);
        }
        public static string GetWorkingDir(IAnalysis anal, string file = "")
        {
            //
            //  SAct.GetWorkingDir()
            //  SAct.GetWorkingDir("file.py")
            //
            string path = anal.WorkingDir.Replace("/", "\\");
            if (file != "") return System.IO.Path.Combine(path, file);
            else return path;
        }
        public static string GetUserDir(IAnalysis anal, string file = "")
        {
            //
            //  SAct.GetUserDir()
            //  SAct.GetUserDir("file.py")
            //
            string path = anal.WorkingDir.Replace("/", "\\");
            List<string> l = path.Split('\\').ToList();
            l = l.GetRange(0, l.Count() - 4);
            l.Add("user_files");
            path = string.Join("\\", l);
            if (file != "") return System.IO.Path.Combine(path, file);
            else return path;
        }
        public static int AnsysVersion()
        {
            //
            // vyzaduje DLL:
            //   - c:\Program Files\ANSYS Inc\v202\scdm\Addins\Ansys 20.2\Ans.Utilities.dll
            //   - c:\Program Files\ANSYS Inc\v202\Addins\GRANTA MI\Ans.Utilities.dll
            //   - c:\Program Files\ANSYS Inc\v202\Addins\ACT\bin\Win64\Ans.Utilities.dll   .... pouzito
            //   - c:\Program Files\ANSYS Inc\v202\Addins\AIM\bin\Win64\Ans.Utilities.dll
            //   - c:\Program Files\ANSYS Inc\v202\Framework\bin\Win64\Ans.Utilities.dll
            //   - c:\Program Files\ANSYS Inc\v202\RSM\bin\FrameworkDependencies\Ans.Utilities.dll
            //
            string s = new ReleaseInformation().ExternalVersionString;
            if (s == "2019 R1") return 193;
            else if (s == "2019 R2") return 194;
            else if (s == "2019 R3") return 195;
            else if (s == "2019 R2") return 194;
            else if (s == "2019 R3") return 195;
            else if (s == "2020 R1") return 201;
            else if (s == "2020 R2") return 202;
            else if (s == "2020 R3") return 203;
            else if (s == "2020 R4") return 204;
            else if (s == "2021 R1") return 211;
            else if (s == "2021 R2") return 212;
            else if (s == "2021 R3") return 213;
            else if (s == "2021 R4") return 214;
            return -1;
        }
        // public static int ToInt32(object o)
        // {
        //     return Convert.ToInt32(ToDouble(o));
        // }
        // public static double ToDouble(object o)
        // {
        //     return ToDouble(o.ToString());
        // }
        // public static double ToDouble(string s)
        // {
        //     try
        //     {
        //         CultureInfo c = new CultureInfo("en-US");
        //         s = s.Replace(",", c.NumberFormat.NumberDecimalSeparator);
        //         s = s.Replace(".", c.NumberFormat.NumberDecimalSeparator);
        //         return Convert.ToDouble(s, c);
        //     }
        //     catch (Exception err) {  SExceptionBase.Throw(err, nameof(SAct), nameof(ToDouble)); }  // catch (Exception err) { throw new Exception($"SAct.ToDouble(...): {err.Message}", err); }
        //     return double.NaN;
        // }
    }
}
