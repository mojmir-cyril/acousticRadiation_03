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
using Ansys.Mechanical.DataModel.Interfaces;
using Ansys.ACT.Mechanical; 
using Ansys.ACT.Automation.Mechanical.Results; 
using Ansys.Mechanical.DataModel.Results; 


namespace SVSEntityManagerF472.Pokusy
{
    internal static class SMeshMove
    {
        internal static void Move(IMeshData mesh, int bodyId, IEnumerable<int> ids, IEnumerable<double> pts, int partId)
        {
            //
            //  mesh   = ExtAPI.DataModel.MeshDataByName("Global")
            //  bodyId = 4
            //  ids    = [283, 732]
            //  pts    = [0.0, 0.0, -0.008, 0.0, 0.0025, -0.008]  
            //  partId = 3
            //  EntityManager.Pokusy.SMeshMove.Move(mesh, bodyId, ids, pts, partId)
            //  ExtAPI.DataModel.Project.Model.Mesh.InternalObject.NotifyMeshControlGroup()
            //  ExtAPI.DataModel.Project.Model.Mesh.Update()
            //  
            //  n = mesh.NodeById(74018) 
            //  print n.X
            //  print n.Y
            //  print n.Z 
            //
            Helpers.MoveNodesByBody(mesh, bodyId, ids, pts);
            Helpers.RedrawPartMesh(partId); 
        }
        // public static void Morph(IExtAPI api, IMeshData mesh, int bodyId, int fixGoemId, int moveGeomId, double dx, double dy, double dz, int partId)
        // {
        //     // 
        //     //  mesh       = ExtAPI.DataModel.MeshDataByName("Global")
        //     //  bodyId     = 18
        //     //  fixGoemId  = 20
        //     //  moveGeomId = 33
        //     //  partId     = 3
        //     //  EntityManager.Pokusy.SMeshMove.Morph(ExtAPI, mesh, bodyId, fixGoemId, moveGeomId, 0.0, 0.0, -0.025, partId)  
        //     //  ExtAPI.DataModel.Project.Model.Mesh.InternalObject.NotifyMeshControlGroup()
        //     //  ExtAPI.DataModel.Project.Model.Mesh.Update()
        //     //
        //     api.Log.WriteMessage($"bodyId     : {bodyId}");
        //     api.Log.WriteMessage($"fixGoemId  : {fixGoemId}");
        //     api.Log.WriteMessage($"moveGeomId : {moveGeomId}");
        // 
        //     IList<int> fixIds = mesh.MeshRegionById(fixGoemId).NodeIds;
        //     IList<int> moveIds = mesh.MeshRegionById(moveGeomId).NodeIds;
        //     Morph(api, mesh, bodyId, fixIds, moveIds, dx, dy, dz, partId);
        // }
        // public static void Morph(IExtAPI api, IMeshData mesh, int bodyId, 
        //                          IEnumerable<int> fixIds, 
        //                          IEnumerable<int> moveIds, 
        //                          double dx, double dy, double dz, 
        //                          int partId)
        // {
        //     // 
        //     //  mesh    = ExtAPI.DataModel.MeshDataByName("Global")
        //     //  bodyId  = 18
        //     //  fixIds  = [431, 438, 501, 550]
        //     //  moveIds = [599, 424]
        //     //  partId  = 3
        //     //  EntityManager.SMeshMove.Morph(ExtAPI, mesh, bodyId, fixIds, moveIds, 0.0, 0.0, -0.005, partId)  
        //     //  ExtAPI.DataModel.Project.Model.Mesh.InternalObject.NotifyMeshControlGroup()
        //     //  ExtAPI.DataModel.Project.Model.Mesh.Update()
        //     //
        //     //
        //     //  checks:
        //     //
        //     if (api == null)     throw new Exception($"Morph(...): Null error: api == null. ");
        //     if (mesh == null)    throw new Exception($"Morph(...): Null error: mesh == null. ");
        //     if (fixIds == null)  throw new Exception($"Morph(...): Null error: fixIds == null. ");
        //     if (moveIds == null) throw new Exception($"Morph(...): Null error: moveIds == null. ");
        //     //
        //     //
        //     //
        //     // List<int> nodeIds = mesh.MeshRegionById(bodyId).NodeIds.ToList();
        //     // List<SMorphDXYZ> dxyzs = nodeIds.Select(id => new SMorphDXYZ(mesh.NodeById(id), isFixOrMove: fixIds.Contains(id) || moveIds.Contains(id))).ToList();
        //     // //
        //     // //  log:
        //     // //
        //     // api.Log.WriteMessage($"bodyId   : {bodyId}");
        //     // api.Log.WriteMessage($"nodeIds  : {string.Join(", ", nodeIds)}");
        //     // api.Log.WriteMessage($"fixIds   : {string.Join(", ", fixIds)}");
        //     // api.Log.WriteMessage($"moveIds  : {string.Join(", ", moveIds)}");
        //     // //
        //     // //  fixed & morphed:
        //     // //
        //     // List<SMorphDXYZ> bcs = dxyzs.Where(d => d.isFixOrMove).ToList();
        //     // List<SMorphDXYZ> mvs = dxyzs.Where(d => !d.isFixOrMove).ToList();
        //     //
        //     //  set move & fix:
        //     //
        //     // bcs.Where(d => moveIds.Contains(d.id)).ToList().ForEach(d => d.Set(dx, dy, dz));
        //     //
        //     //  assign:
        //     //
        //     // foreach (SMorphDXYZ d in dxyzs) d.AssignConnectedDXYZs(dxyzs);
        //     //
        //     //  checks:
        //     //
        //     // int err1 = dxyzs.Where(d => d.connectedNodes.Count() <= 0).Count();
        //     // int err2 = dxyzs.Where(d => d.connectedDXYZs == null).Count();
        //     // if (err1 >= 1) throw new Exception($"Morph(...): err1 = {err1} >= 1, dxyzs.Count() = {dxyzs.Count()}");
        //     // if (err2 >= 1) throw new Exception($"Morph(...): err2 = {err2} >= 1, dxyzs.Count() = {dxyzs.Count()}");
        //     //
        //     //  methods:
        //     //
        //     SMorphMethods m = SMorphMethods.RadialBaseFuncSmoothing;
        //     //
        //     //  NeighborAverage:
        //     //
        //     if (m == SMorphMethods.NeighborAverage)
        //     {
        //         //
        //         //  iterace:
        //         //
        //         for (int i = 0; i < 2500; i++)
        //         {
        //             mvs.ForEach(d => d.EvalAverage());
        //             double maxChange = mvs.Max(d => d.lastChange);
        //             api.Log.WriteMessage($" - iter : {i} : {maxChange} <= 0.00001");
        //             if (maxChange <= 0.00001) break;
        //         } 
        //     }
        //     //
        //     //  RadialBaseFunc:
        //     //
        //     else if (m == SMorphMethods.RadialBaseFunc)
        //     {
        //         mvs.ForEach(d => d.EvalRBF(bcs));
        //     }
        //     //
        //     //  RadialBaseFunc:
        //     //
        //     else if (m == SMorphMethods.RadialBaseFuncSmoothing)
        //     {
        //         mvs.ForEach(d => d.EvalRBF(bcs));
        //         for (int i = 0; i < 5; i++) mvs.ForEach(d => d.EvalAverage());
        //     }
        //     //
        //     //  move:
        //     //
        //     dxyzs.ToList().ForEach(d => Helpers.MoveNodeByBody(mesh, bodyId, d.id, d.xyz));
        //     Helpers.RedrawPartMesh(partId);
        //     // ((IMechanicalExtAPI)api).DataModel.Project.Model.Mesh.InternalObject.NotifyMeshControlGroup();
        //     // ((IMechanicalExtAPI)api).DataModel.Project.Model.Mesh.Update();
        // } 
    }
    // public static class SResult.SRead
    // {
    //     //
        //  result = Tree.FirstActiveObject
        //  data   = EntityManager.SResult.SRead.Read(result, [74001, 74000, 73999, 72738])
        //  data   = EntityManager.SResult.SRead.Read(result, 18)
        //  data   = EntityManager.SResult.SRead.Read(result)       # all
        //  unit   = EntityManager.SResult.SRead.Unit(result)       # ---> 'Length'
        //
        //  min(data.Values)
        //  max(data.Values)
        //
    //     public static string Unit(Result result) => result.GetQuantityName(((dynamic)result).InternalObject);
    //     public static string Unit(UserDefinedResult result) => result.GetQuantityName();
    //     public static Dictionary<int, double> Read(Result result, int bodyRefId) => result.GetNodalValues(bodyRefId);
    //     public static Dictionary<int, double> Read(UserDefinedResult result, int bodyRefId) => result.GetNodalValues(bodyRefId);
    //     public static Dictionary<int, double> Read(Result result, IEnumerable<int> nodeIds = null) => Read(result.PlotData, nodeIds);
    //     public static Dictionary<int, double> Read(UserDefinedResult result, IEnumerable<int> nodeIds = null) => Read(result.PlotData, nodeIds);
    //     public static Dictionary<int, double> Read(ResultDataTable plotDataResult, IEnumerable<int> nodeIds = null)
    //     {   
    //         List<int>               nodes          = plotDataResult["Node"].Cast<int>().ToList();
    //         List<double>            values         = plotDataResult["Values"].Cast<double>().ToList(); 
    //         Dictionary<int, double> data           = new Dictionary<int, double>(); 
    //         //
    //         //  fill:
    //         //
    //         if (nodeIds != null)
    //         {
    //             for (int i = 0; i < nodes.Count(); i++) 
    //                 if (nodeIds.Contains(nodes[i])) 
    //                     data.Add(nodes[i], values[i]);
    //         }
    //         else
    //         {
    //             for (int i = 0; i < nodes.Count(); i++) 
    //                 data.Add(nodes[i], values[i]);
    //         }
    //         //
    //         //  return:
    //         //
    //         return data;
    //     }
    // } 
}
