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
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical; 
using Ansys.Mechanical.DataModel.Enums;
using Ansys.ACT.Interfaces.Analysis; 
using Ansys.ACT.Automation.Mechanical.Connections;
using Ansys.ACT.Automation.Mechanical.Results; 

using SVSExceptionBase;
using SVSConvertF472;
using SVSLoggerF472;

namespace SVSEntityManagerF472
{
    internal class STree : SLoggerBase
    { 
        internal Ansys.ACT.Mechanical.MechanicalDataModel     dataModel       { get => (Ansys.ACT.Mechanical.MechanicalDataModel)api.DataModel; } // 2021R2 // public IMechanicalDataModel     dataModel       { get => api.DataModel; } 
        internal Tree                                         tree            { get => dataModel.Tree; } 
        internal Model                                        model           { get => dataModel.Project.Model; } 
        internal DataModelObject                              first           { get => (DataModelObject)tree.FirstActiveObject; } 
        internal List<DataModelObject>                        all             { get => tree.AllObjects.Select((x) => (DataModelObject)x).ToList(); } 
        internal List<DataModelObject>                        actives         { get => tree.ActiveObjects.Select((x) => (DataModelObject)x).ToList(); } 
        internal List<ContactRegion>                          contacts        { get => dataModel.GetObjectsByType(DataModelObjectCategory.ContactRegion).Select((x) => (ContactRegion)x).ToList(); } 
        internal List<NamedSelection>                         nss             { get => dataModel.GetObjectsByType(DataModelObjectCategory.NamedSelection).Select((x) => (NamedSelection)x).ToList(); } 
        internal List<Comment>                                comments        { get => dataModel.GetObjectsByType(DataModelObjectCategory.Comment).Select((x) => (Comment)x).ToList(); } 
        internal List<Analysis>                               anals           { get => dataModel.GetObjectsByType(DataModelObjectCategory.Analysis).Select((x) => (Analysis)x).ToList(); } 
        internal List<Solution>                               solutions       { get => dataModel.GetObjectsByType(DataModelObjectCategory.Solution).Select((x) => (Solution)x).ToList(); }
        internal List<Result>                                 results         { get => dataModel.GetObjectsByType(DataModelObjectCategory.Result).Select((x) => (Result)x).ToList(); } 
        internal List<UserDefinedResult>                      userResults     { get => dataModel.GetObjectsByType(DataModelObjectCategory.UserDefinedResult).Select((x) => (UserDefinedResult)x).ToList(); } 
        internal List<DataModelObject>                        allResults      { get => results.Cast<DataModelObject>().Union(userResults).ToList(); } 
        internal List<CoordinateSystem>                       css             { get => dataModel.GetObjectsByType(DataModelObjectCategory.CoordinateSystem).Select((x) => (CoordinateSystem)x).ToList(); } 
        internal CoordinateSystem                             globalCS        { get => css.Where((x) => x.IsGlobal).ToList()[0]; } 
        // -------------------------------------------------------------------------------------------
        //
        //      ctor:
        //
        // ------------------------------------------------------------------------------------------- 
        internal STree(SEntityManager em) : base(em, nameof(STree)) { }
        // -------------------------------------------------------------------------------------------
        //
        //      Methods:
        //
        // -------------------------------------------------------------------------------------------
        internal DataModelObject Obj(int id)
        {
            try { return (DataModelObject)dataModel.GetObjectById(id); }
            catch (Exception err) { Throw(err, nameof(Obj)); } // catch (Exception err) { throw new Exception($"STree.Obj(...): Object with id '{id}' cannot be found, exception: {err.Message}. "); } 
            // try { return (DataModelObject)tree.Find(func: (x) => x.ObjectId == id).ToArray()[0]; }
            // catch (Exception e) { throw new Exception($"STree.Obj(...): Object with id '{id}' cannot be found, exception: {e}. "); }
            return null;
        }
        internal NamedSelection GetNS(string name)
        {
            if (name == null) { throw new Exception($"GetNS(...): name == null. ");   }
            if (name == "") { throw new Exception($"GetNS(...): name == ''.   ");  }
            if (model.NamedSelections == null) { throw new Exception($"GetNS(...): model.NamedSelections == null");  }
            try { foreach (NamedSelection ns in nss) if (ns.Name == name) return ns; }
            catch (Exception err) { Throw(err, nameof(GetNS)); } // catch (Exception err) { throw new Exception($"GetNS(...): {err.Message}"); }
            return null;
        }
        internal IAnalysis GetAnal(string name, string type = "")
        {
            try { foreach (IAnalysis anal in anals) if (anal.Name == name) if (type == "" || type == anal.AnalysisType) return anal; }
            catch (Exception err) { Throw(err, nameof(GetAnal)); } // catch (Exception err) { throw new Exception($"GetAnal(...): {err.Message}"); }
            return null;
        }
        internal void AddComment(string name = "New Comment", string text = "No text.")
        {
            try
            {
                dynamic x = api.DataModel.Tree.FirstActiveObject.InternalObject;
                dynamic c = x.AddComment();
                c.Name = name;
                c.Text = text;
                // return (Comment)first; 
            }
            catch (Exception err) { Throw(err, nameof(AddComment)); }
        } 
        internal void ExportToTextFile(SSendPY sendPY, string fileName)
        {
            string cmd = $"DS.Script.doControlledExportToTextFile(1, '{fileName}');";
            sendPY.SendJS(cmd);
        }
        internal SPoint GetOrigin(CoordinateSystem csObject, SUnitsUtils units, bool doLog = false)
        {
            try
            {
                if (csObject == null) throw new Exception("STree.GetOrigin(...): csObject == null. ");
                if (csObject.IsGlobal) return new SPoint(0, 0, 0, 0, "m");
                List<string> sxyz = csObject.TransformedConfiguration.ToString().Replace("[  ", "").Replace("[ ", "").Replace("  ]", "").Replace(" ]", "").Replace("  ", " ").Replace("  ", " ").Replace(" ", ";").Split(';').ToList();
                double x = new SLength(SConvert.ToDouble(sxyz[0]), units, SCurrentUnit.Current).ToInternal();
                double y = new SLength(SConvert.ToDouble(sxyz[1]), units, SCurrentUnit.Current).ToInternal();
                double z = new SLength(SConvert.ToDouble(sxyz[2]), units, SCurrentUnit.Current).ToInternal();
                if (doLog)
                {
                    logger.Msg("GetOrigin(...): ");
                    logger.Msg($" - csObject.TransformedConfiguration : {csObject.TransformedConfiguration}");
                    logger.Msg($" - x : {x}");
                    logger.Msg($" - y : {y}");
                    logger.Msg($" - z : {z}");
                }
                return new SPoint(0, x, y, z, SUnitsUtils.internalLengthUnit);
            }
            catch (Exception e)
            {
                string t = "N/A", n = "N/A";
                try { t = csObject.TransformedConfiguration.ToString(); } catch { }
                try { n = csObject.Name.ToString(); } catch { }
                Throw("STree.GetOrigin(): " + e.ToString() + " csObject.TransformedConfiguration : " + t + ", csOObject.Name : " + n, nameof(AddComment)); // throw new Exception("STree.GetOrigin(): " + e.ToString() + " csObject.TransformedConfiguration : " + t + ", csOObject.Name : " + n, e);
                return null;
            }
        }
        // -------------------------------------------------------------------------------------------
        //
        //      save & resume:
        //
        // -------------------------------------------------------------------------------------------
        private List<DataModelObject> __temps { get; set; }
        internal void Save()   => __temps = actives.ToList();
        internal void Resume() => tree.Activate(__temps);
    }
}
