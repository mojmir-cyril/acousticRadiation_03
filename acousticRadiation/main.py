import os
import myUtils
import units  # External library to help with unit conversion

msg = ExtAPI.Log.WriteMessage

def EM(path = r"E:\Mojmir\projekty\ACT projekty\acousticRadiation_03\SVSEntityManager\C#\SVSEntityManagerF472\bin\Debug"):     # change with current folder where SVSEntityManagerF472.dll is
    """
        return instance of SVS FEM Entity Manager
    """
    import clr, os, sys
    dll  = r"SVSEntityManagerF472.dll"
    clr.AddReferenceToFileAndPath(os.path.join(path, dll))
    import SVSEntityManagerF472
    sys.path += [path]
    return SVSEntityManagerF472.SEntityManager(ExtAPI)

def onInit(*args):
    msg("onInit")

def onLoad(*args):
    msg("onLoad")
    global em
    em = EM()

def CreateAcousticRadiationObj(analysis):
    msg("CreateAcousticRadiationObj")
    analysis.CreateResultObject("acousticRadiation", "acousticRadiation")
    # analysis.Name
    # model = ExtAPI.DataModel.Project.Model
    # analysis = model.Analysis[0]
    # print(analysis.Name)


def SelectElFaces(element_ids, element_face_indices):
    ExtAPI.SelectionManager.ClearSelection()
    mySel = ExtAPI.SelectionManager.CreateSelectionInfo(SelectionTypeEnum.MeshElementFaces)
    mySel.Ids = element_ids
    mySel.ElementFaceIndices = element_face_indices
    ExtAPI.SelectionManager.NewSelection(mySel)

def onShow(object):
    msg("onShow")
    global OBJ
    OBJ = object
    sel = OBJ.Properties["Geometry"].Value
    emSel = em.SelEnts(sel)
    elFaces = emSel.elemFaces
    analysis = Model.Analyses[0]
    res = analysis.GetResultsData()
    Freqs = res.ListTimeFreq

    U = res.GetResult("U")
    UVals = U.GetNodeValues(elFaces.nodes.info.Ids)
    VVals =


def EvalResult(result, stepInfo, collector):
    global RES
    RES = result

    msg("EvalRes")
    analysis = result.Analysis
    Ids = collector.Ids
    # Indices = collector.Indices
    # properties = result.Properties

    values = [Id * 1.0 for Id in Ids]
    # msg("values: " + str(values))
    msg("Ids: " + str(collector.Ids))
    # collector.SetAllValues(values, Ids)

    geom = result.Properties["Geometry"]
    sel = geom.Value
    # sel.

    #
    # # mySel = SelectElFaces(Ids, element_face_indices)
    #
    # meshData = analysis.MeshData
    # msg("meshData: "+ str(meshData.GetType()) + str(dir(meshData)))
    # # elemFaces = meshData.GetElementFaces()
    # # msg("elemFaces: "+ str(elemFaces.GetType()) + str(dir(elemFaces)) + str(elemFaces))
    #
    # # mesh = ExtAPI.DataModel.MeshDataByName("Global")
    # # element = mesh.ElementById(element_id)

    msg("pocetIds: " + str(len(Ids)))
    # msg("pocetIndices: " + str(len(Indices)))
    #
    # # msg("analysis: "+ str(analysis.GetType()) + str(dir(analysis)))
    msg("result: " + str(dir(result)))
    msg("resultLocation: " + str(dir(result.ResultLocation)) + str(result.ResultLocation))
    msg("resultLocationElem: " + str(dir(result.ResultLocation.Element)) + str(result.ResultLocation.Element))

    # resLocVals = result.ResultLocation.GetValues()
    # msg("resLocVals: " + str(dir(resLocVals)) + str(resLocVals))

    allProps = result.AllProperties
    msg("resultAllProps: " + str(dir(allProps)) + str(allProps))


    # msg("stepInfo: " + str(dir(stepInfo)))
    msg("collector: " + str(dir(collector)))
    pass

    # # ExtAPI.Log.WriteMessage("result: " + str(help(result)) + ", stepInfo: " + str(help(stepInfo)) + ", collector: " + str(help(collector)))
    # step = stepInfo.Set
    # nodeIds = collector.Ids
    # analysis = result.Analysis
    # reader = analysis.GetResultsData()
    # reader.CurrentResultSet = step
    # freqs = reader.ListTimeFreq
    # deformation = reader.GetResult("U")
    # fromUnit = deformation.GetComponentInfo("X").Unit  # Unit of Length when solution was solved
    # toUnits = "m"  # Always generate the result to display is SI unit
    #
    #
    # unitConvFact = units.ConvertUnit(1, fromUnit, toUnits, "Length")
    #
    # model = ExtAPI.DataModel.Project.Model
    # a = model.Analyses[0]
    # mesh = a.MeshData
    # nodeIds = mesh.NodeIds
    # ExtAPI.Log.WriteMessage(str(nodeIds))
    #
    # omega = 1
    #
    # for id in nodeIds:
    #     node = mesh.NodeById(id)
    # x, y, z = node.X, node.Y, node.Z
    # sum = (y ** 2 + z ** 2) ** (1 / 2)
    # y_norm, z_norm = y / sum, z / sum
    # a_r = omega * sum ** 2
    #
    # fx = 0.0
    # fy = a_r * y_norm
    # fz = a_r * z_norm
    # # Set the vector for each node to display
    # collector.SetValues(id, [2, 2, 2])
    #
    # # ExtAPI.Log.WriteMessage(str(reader.CurrentResultSet))
    #
    # # ExtAPI.Log.WriteMessage(str(result))
    #
    # return




# def EvalNodalForcesFromResFile(result, stepInfo, collector)


# step = stepInfo.Set
# nodeIds = collector.Ids
# analysis = result.Analysis
# reader = analysis.GetResultsData()
# reader.CurrentResultSet = step
# freqs = reader.ListTimeFreq
# deformation = reader.GetResult("F")
# fromUnit = deformation.GetComponentInfo("X").Unit  # Unit of Length when solution was solved
# toUnits = "m"  # Always generate the result to display is SI unit

# ExtAPI.Log.WriteMessage("ahoj")

# unitConvFact = units.ConvertUnit(1, fromUnit, toUnits, "Length")

# model = ExtAPI.DataModel.Project.Model
# a = model.Analyses[0]
# mesh = a.MeshData
# nodeIds = mesh.NodeIds
# omega = 1


# # Reuse the APDL commands for applying the load
# SourceDir = ExtAPI.ExtensionManager.CurrentExtension.InstallDir
# macFile = "APDL_script_for_sila_na_uzly.inp"
#
#
#
# # Read the macro and copy the commands to ds.dat
# fs = open(os.path.join(SourceDir, macFile), "r")
# allLines = fs.readlines()
# fs.close()
# stream.Write("".join(allLines))

# Alternatively, you can call the input file directly .... by /input command
# inpComm = """/inp,'%s' \n"""%os.path.join(SourceDir, macFile)
# stream.Write(inpComm)

"""
def write_sila_na_uzly(load, stream):
    ExtAPI.Log.WriteMessage("Write sila_na_uzly...")
    stream.Write("/com,\n")
    stream.Write("/com,*********** sila_na_uzly " + load.Caption + " ***********\n")
    stream.Write("/com,\n")

    # Collect the user inputs
    propGeo = load.Properties["Geometry"]
    geoType = propGeo.Properties['DefineBy'].Value
    if geoType == 'Named Selection':
        refName = propGeo.Value.Name
    else:
        refIds = propGeo.Value.Ids

    omega = load.Properties["Magnitude"].Value
    sila = omega

    # Convert the user inputs to APDL commands

    stream.Write("sila=" + sila.ToString() + "\n")

    stream.Write("\n/prep7 !podlaha \n")

    mesh = load.Analysis.MeshData
    geo = load.Analysis.GeoData

    if geoType != 'Named Selection':
        # Create the element component if user has selected Geometry
        myUtils.createElementComponent(refIds, "sila_na_uzly" + load.Id.ToString(), mesh, stream)
        stream.Write("CMSEL, S, sila_na_uzly" + load.Id.ToString() + "\n")
    else:
        stream.Write("CMSEL,S," + refName + "\n")

    stream.Write("/solu !objevi se to tu? \n ")

    model = ExtAPI.DataModel.Project.Model
    a = model.Analyses[0]
    mesh = a.MeshData
    nodeIds = mesh.NodeIds

    for id in nodeIds:
        node = mesh.NodeById(id)
        # x, y, z = {}, {}, {}
        # x.update = {id: node.X}
        # y.update = {id: node.Y}
        # z.update = {id: node.Z}
        # rotace okolo x
        x, y, z = node.X, node.Y, node.Z
        sum = (y ** 2 + z ** 2) ** (1 / 2)
        y_norm, z_norm = y / sum, z / sum
        a_r = omega * sum ** 2
        string_FY = "F, %d, FY, %f  \n" % (id, a_r * y_norm)  # FY
        string_FZ = "F, %d, FZ, %f  \n" % (id, a_r * z_norm)  # FZ
        stream.Write(string_FY)  # FY
        stream.Write(string_FZ)  # FZ
"""


# def NodeValues(load, nodeIds):
#     values = []
#     mesh = load.Analysis.MeshData
#     for id, i in nodeIds, enumerate(nodeIds):
#         node = mesh.NodeById(id)
#         # x, y, z = [], [], []
#         # x.append(node.X)
#         # y.append(node.Y)
#         # z.append(node.Z)
#         # nebo pomoci dictionary?
#
#         x, y, z = {}, {}, {}
#         x.update = {id: node.X}
#         y.update = {id: node.Y}
#         z.update = {id: node.Z}
#     return x, y, z


# prislusnemu id v nodeIds priradit pomoci vypoctu vzdalenosti od pocatku (osy) a uhlove frekvence a vektoru smerujiciho od osy (treba x) jednotlive slozky sily (zrychleni) - pote priradit v APDL skriptu - jak?


