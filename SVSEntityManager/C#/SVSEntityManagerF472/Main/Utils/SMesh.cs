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
using Ansys.ACT.Interfaces.Common;
using Ansys.ACT.Interfaces.Mechanical;
using Ansys.ACT.Automation.Mechanical;
using Ansys.ACT.Interfaces.Mesh; 
using Ansys.ACT.Interfaces.Geometry;

namespace SVSEntityManagerF472
{
    public class SMesh : SExceptionBase
    {
        public IMechanicalExtAPI api        { get; }
        public Action<object>    Msg        { get; }
        public Tree              tree       { get => api.DataModel.Tree; }
        public Model             model      { get => api.DataModel.Project.Model; }
        public string            meshName   { get; set; } = "Global";
        internal SMesh(IMechanicalExtAPI api, SLogger logger, string meshName = "Global")
        { 
            Null(api, nameof(api), nameof(SMesh)); // if (api == null) throw new Exception($"SMesh(...): Null error: api == null. ");
            this.api      = api;
            this.Msg      = logger.Msg;
            this.meshName = meshName; 
        }
        public IMeshRegion Region(int refId)
        {
            IMeshData mesh = api.DataModel.MeshDataByName(meshName);
            return mesh.MeshRegionById(refId);
        }
        public IMeshRegion Region(IBaseGeoEntity geom) => Region(geom);
        public IMeshRegion Region(IGeoEntity geom)
        {
            IMeshData mesh = api.DataModel.MeshDataByName(meshName);
            return mesh.MeshRegionById(geom.Id);
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Elems:
        //
        // -------------------------------------------------------------------------------------------
        public IElement GetElem(int elemId)
        {
            IMeshData mesh = api.DataModel.MeshDataByName(meshName);
            return mesh.ElementById(elemId);
        }
        public List<IElement> GetElems(NamedSelection ns)
        {
            return GetElems(ns.Location.Ids);
        }
        public List<IElement> GetElems(IEnumerable<IGeoEntity> geoms)
        {
            return GetElems(from e in geoms select e.Id);
        }
        public List<IElement> GetElems(IEnumerable<int> geomIds)
        {
            List<IElement> ret = new List<IElement>();
            foreach (int id in geomIds) ret.AddRange(Region(id).Elements.ToList());
            ret = ret.Distinct(new SEqualityComparerIElement()).ToList();                 // delete duplicates
            return ret;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      Nodes:
        //
        // -------------------------------------------------------------------------------------------
        public INode GetNode(int nodeId)
        {
            IMeshData mesh = api.DataModel.MeshDataByName(meshName);
            return mesh.NodeById(nodeId);
        }
        public List<INode> GetNodes(NamedSelection ns)
        {
            if (ns.Location.SelectionType == SelectionTypeEnum.GeometryEntities) return GetNodes(ns.Location.Ids);
            else if (ns.Location.SelectionType == SelectionTypeEnum.MeshNodes)
            {
                IMeshData mesh = api.DataModel.MeshDataByName(meshName); 
                return ns.Location.Ids.Select(id => mesh.NodeById(id)).ToList();
            }
            else throw new Exception($"GetNodes(...): TO-DO: ns.Location.SelectionType == {ns.Location.SelectionType}. ");
        }
        public List<INode> GetNodes(IGeoEntity geom)
        {
            return GetNodes(new List<IGeoEntity>() { geom });
        }
        public List<INode> GetNodes(IEnumerable<IGeoEntity> geoms)
        {
            return GetNodes(from e in geoms select e.Id);
        }
        public List<INode> GetNodes(IEnumerable<int> geomIds)
        {
            List<INode> ret = new List<INode>();
            foreach (int id in geomIds) ret.AddRange(Region(id).Nodes.ToList());
            ret = ret.Distinct(new SEqualityComparerINode()).ToList();                 // delete duplicates
            return ret;
        }
        // -------------------------------------------------------------------------------------------
        //
        //      FaceElems:
        //
        // -------------------------------------------------------------------------------------------
        // public List<IElement> GetFaceElems(NamedSelection ns)
        // {
        //     Ansys.ACT.Mechanical.MechanicalSelectionInfo i = (Ansys.ACT.Mechanical.MechanicalSelectionInfo)ns.Location;
        //     return GetFaceElems(i.Ids, i.ElementFaceIndices);
        // }
        public List<IElement> GetFaceElems(IEnumerable<IGeoEntity> geoms)
        {
            return GetFaceElems(geoms.Select(g => g.Id));
        }
        public List<IElement> GetFaceElems(IEnumerable<int> geomIds)
        {
            List<IElement> ret = new List<IElement>();
            foreach (int id in geomIds) ret.AddRange(Region(id).Elements.ToList());
            ret = ret.Distinct(new SEqualityComparerIElement()).ToList();                 // delete duplicates
            return ret;
        }

        public int Pokus(int iElementId, uint ulAppliedFilter, int iRefId, int iElementType)
        { 
            Ansys.ACT.Common.Mesh.MeshWrapper mesh = (Ansys.ACT.Common.Mesh.MeshWrapper)api.DataModel.MeshDataByName(meshName); 
            return ((dynamic)mesh).AssemblyMesh.GetElementFaceOnFaceRef(iElementId, ulAppliedFilter, iRefId, iElementType); // int GetElementFaceOnFaceRef(int iElementId, uint ulAppliedFilter, int iRefId, int iElementType);
        }
    }
}
