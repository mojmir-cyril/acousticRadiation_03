#pragma warning disable IDE1006                         // Naming Styles

using System.Collections.Generic; 

namespace SVSEntityManagerF472
{
    /// <summary>
    /// interface of basic properties
    /// </summary>
    public interface ISEntities<TEnt> where TEnt : SEntity
    {
        /// <summary>
        /// The SEntityManager object created by SVS FEM s.o.r. for fast/easy work with geometrical entitites.
        /// The main instance (em) genarally keeps all necessary settings for selecting.
        /// </summary>
        SEntityManager          em               { get; }
        // -------------------------------------------------------------------------------------------
        //
        //      entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// gets list of internal (ACT) objects 
        /// </summary>
        List<TEnt>              entities         { get; }
        /// <summary>
        /// gets list of object Ids
        /// </summary>
        List<int>               ids              { get; }
        /// <summary>
        /// gets list of element face ids (used only for element faces)
        /// </summary>
        List<int>               elemFaceIds      { get; } // elem-face (ElementFaceIndices)
        /// <summary>
        /// gets geometry type of the entity/entities
        /// </summary>
        SType                   type             { get; }
        /// <summary>
        /// gets true if collection is empty (count == 0)
        /// </summary>
        bool                    isEmpty          { get; }
        /// <summary>
        /// gets true if nodal type of the entities  
        /// </summary>
        bool                    isNode           { get; }
        /// <summary>
        /// gets true if elemental type of the entities  
        /// </summary>
        bool                    isElem           { get; }
        /// <summary>
        /// gets true if element face type of the entities 
        /// </summary> 
        bool                    isElemFace       { get; }
        /// <summary>
        /// gets true if geometry type of the entities  
        /// </summary>
        bool                    isGeom           { get; }
        // -------------------------------------------------------------------------------------------
        //
        //      individual entities:
        //
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// converts to attached parts
        /// </summary>
        SParts                   parts           { get; }
        /// <summary>
        /// converts to attached bodies
        /// </summary>
        SBodies                  bodies          { get; }
        /// <summary>
        /// converts to attached faces
        /// </summary>
        SFaces                   faces           { get; }
        /// <summary>
        /// converts to attached edges
        /// </summary>
        SEdges                   edges           { get; }
        /// <summary>
        /// converts to attached verts
        /// </summary>
        SVerts                   verts           { get; }
        /// <summary>
        /// converts to attached nodes
        /// </summary>
        SNodes                   nodes           { get; }
        /// <summary>
        /// converts to attached elems
        /// </summary>
        SElems                   elems           { get; }
        /// <summary>
        /// converts to attached elemFaces
        /// </summary>
        SElemFaces               elemFaces       { get; }
        /// <summary>
        /// gets SInfo object which can be use for setting of a Location
        /// SInfo is object inherited from (ACT) objects: MechanicalSelectionInfo and ISelectionInfo
        /// </summary>
        /// <exmple>
        /// <code>
        /// o = Tree.FirstActiveObject
        /// o.Location = em.solids.Min(lambda e:  e.x, count = 5).info
        /// #
        /// #  where:
        /// #     o ... is an object in the Mechanical tree with Location property (e.g. Named Selection, Force, ...)
        /// </code>
        /// </exmple>
        SInfo                    info           { get; }  
    }
}
