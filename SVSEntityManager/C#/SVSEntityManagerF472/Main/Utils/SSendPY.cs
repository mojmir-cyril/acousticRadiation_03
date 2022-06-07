#pragma warning disable IDE1006                         // Naming Styles
#pragma warning disable CS1591                          // Missing XML comment for publicly visible type or member



using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections;
 
using SVSExceptionBase;
using SVSLoggerF472;
//
//  Ansys:
// 
using Ansys.ACT.Interfaces.Mechanical; 
using Ansys.ACT.Core;

namespace SVSEntityManagerF472 
{

    internal class SSendPY : SExceptionBase
    {
        internal IMechanicalExtAPI    api        { get; }
        internal SLogger              logger     { get; }
        internal Extension            extension  { get => (Extension)api.ExtensionManager.CurrentExtension; }
        internal SSendPY(IMechanicalExtAPI api, SLogger logger)
        {
            Null(api, nameof(api), nameof(SSendPY)); // if (api == null) throw new Exception($"SSendPY(...): Null error: api == null. ");
            this.api = api;
            this.logger = logger; 
        }
        internal void   Action(string scriptPY) => Execute(scriptPY, SAct.GetExtensionDir(api, $@"temp\script{DateTime.Now}.py".Replace(":", "").Replace(" ", ""))); 
        internal object Func(string scriptPY)   => Execute(scriptPY, SAct.GetExtensionDir(api, $@"temp\script{DateTime.Now}.py".Replace(":", "").Replace(" ", "")));
        internal object Execute(string scriptPY)
        {
            logger.Msg($"<br><i>{scriptPY}</i><br>");
            try { return extension.ExecuteCommand(scriptPY); }
            catch (Exception err) { Throw($"{err.Message}, script: {scriptPY}", nameof(Execute)); } // catch (Exception err) { throw new Exception($"SSendPY.Execute(...): {err.Message}, script: {scriptPY}"); }
            return null;
        }        
        internal object Execute(string scriptPY, string tempFileName)
        {
            logger.Msg($"<br><i>{scriptPY}</i><br>");
            try
            {
                File.WriteAllText(tempFileName, scriptPY);
                return extension.ExecuteFile(tempFileName);
            }
            catch (Exception err) { Throw($"{err.Message}, script: {scriptPY}", nameof(Execute)); } // catch (Exception err) { throw new Exception($"SSendPY.Execute(...): {err.Message}, script: {scriptPY}, tempFileName: {tempFileName}"); }
            return null;
        }
        internal object ExecuteFile(string fileName)
        {
            logger.Msg($"<br><i>{fileName}</i><br>"); 
            try { return extension.ExecuteFile(fileName); }
            catch (Exception err) { Throw($"{err.Message}, fileName: {fileName}", nameof(ExecuteFile)); } // catch (Exception err) { throw new Exception($"SSendPY.ExecuteFile(...): {err.Message}, script: {fileName}"); }
            return null;
        }
        internal void SendJS(string scriptJS)
        {
            logger.Msg($"<br><i>{scriptJS}</i><br>");
            try 
            {
                dynamic x = api.Application.ScriptByName("jscript");
                x.ExecuteCommand(scriptJS); 
            }
            catch (Exception err) { Throw($"{err.Message}, scriptJS: {scriptJS}", nameof(SendJS)); } //  catch (Exception err) { throw new Exception($"SSendPY.SendJS(...): {err.Message}, script: {scriptJS}"); }
        }
    }
}
