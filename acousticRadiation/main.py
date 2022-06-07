import os
import myUtils
import units  # External library to help with unit conversion
import cPickle as pickle

"""
working with SI units (em.Units("m", "rad", "kg")) -> all returned values are in SI units
"""

extName = "acousticRadiation"
global msg
global wrn
global err

msg = ExtAPI.Log.WriteMessage
wrn = ExtAPI.Log.WriteWarning
err = ExtAPI.Log.WriteError

ro = 1.2041 # air density [kg/m**3]
c = 343.25 # speed of sound [m/s]
WRef = 1e-12

extDir = os.path.dirname(ExtAPI.ExtensionManager.CurrentExtension.InstallDir) # directory one level up
# absPathEM = r"E:\Mojmir\projekty\ACT projekty\acousticRadiation_03\SVSEntityManager\C#\SVSEntityManagerF472\bin\Debug"
pathEM = os.path.join(extDir, r"SVSEntityManager\C#\SVSEntityManagerF472\bin\Debug")
msg("pathEM: " + pathEM)


def EM(path = pathEM):     # change with current folder where SVSEntityManagerF472.dll is
    """
        return instance of SVS FEM Entity Manager
    """
    import clr, os, sys
    dll  = r"SVSEntityManagerF472.dll"
    clr.AddReferenceToFileAndPath(os.path.join(path, dll))
    import SVSEntityManagerF472
    sys.path += [path]
    return SVSEntityManagerF472.SEntityManager(ExtAPI)

def EM_whole(path = pathEM):     # change with current folder where SVSEntityManagerF472.dll is
    """
        return instance of SVS FEM Entity Manager
    """
    import clr, os, sys
    dll  = r"SVSEntityManagerF472.dll"
    clr.AddReferenceToFileAndPath(os.path.join(path, dll))
    import SVSEntityManagerF472
    sys.path += [path]
    return SVSEntityManagerF472

def createEmAndEmw():
    if "em" not in globals():
        global em, emw
        em = EM()
        em.Units("m", "rad", "kg")
        emw = EM_whole()

@callback
def onInit(*args):
    msg("onInit")

@callback
def OnReady(*args):
    msg("OnReady")

@callback
def Resume(*args):
    msg("Resume")
    createEmAndEmw()

@callback
def onLoad(*args):
    msg("onLoad")
    # global em, emw
    # em = EM()
    # em.Units("m", "rad", "kg")
    # emw = EM_whole()

@callback
def OnTerminate(context):
    msg("OnTerminate")
    # msg("co to je: " + context)
    ExtAPI.Graphics.Scene.Clear()

    ExtAPI.Graphics.ViewOptions.ShowLegend = True
    ExtAPI.Graphics.Scene.Visible = False  # umi ukazat nebo schovat vykreslene
    try:
        ExtAPI.Graphics.ViewOptions.ModelDisplay = prevModelDisplay
        ExtAPI.Graphics.ViewOptions.ResultPreference.DeformationScaleMultiplier = prevDeformationScaleMultiplier
    except:
        msg("prevModelDisplay and prevDeformationScaleMultiplier are not defined yet")
    prevVisibleBodies.visible = True
    (em.bodies - prevVisibleBodies).visible = False

    for extObj in DataModel.GetUserObjects(extName):
        try:
            extObj.Properties["Results/dictResults"].Value = None
            msg("dictResults deleted")
        except Exception as e:
            pass

@callback
def onValidate(*object):
    msg("onValidate")
    msg("object: " + str(object))
    # msg("arg2: " + str(arg2))
    return

@callback
def isValidFreq(object, prop):
    analysisId = object.Analysis.Id
    treeAnalysis = [i for i in Model.Analyses if i.Id == analysisId][0]

    if prop.Value == 0:
        prop.Value = treeAnalysis.AnalysisSettings.RangeMaximum.Value
        return True
    if treeAnalysis.AnalysisSettings.RangeMinimum.Value > prop.Value or prop.Value > treeAnalysis.AnalysisSettings.RangeMaximum.Value:
        return False
    else: return True

def GetDataDict(object):
    """
    gets serializable data
    :param object:
    :return:
    """
    controller      = object.Controller
    analysis        = controller.analysis
    scopeGeomEnts   = controller.scopeGeomEnts
    freq            = controller.freq
    msg("freq: " + str(freq))
    bodies          = controller.bodies
    elemFaces       = controller.elemFaces

    msg("1")

    if      object.Name == "ERPPostObj":
        dataDict        = elemFaces.GetDictERP(freq, analysis)
        specDataDict    = elemFaces.GetDictSpecERP(freq, analysis)
    elif    object.Name == "ERPLevelPostObj":
        dataDict        = elemFaces.GetDictERPLevel(freq, analysis)
        specDataDict    = elemFaces.GetDictSpecERPLevel(freq, analysis)
    elif    object.Name == "NormalVelPostObj":
        dataDict        = elemFaces.GetDictNormalV(freq, analysis)
        specDataDict    = dataDict

    msg("2")

    object.Properties["Results/dataDict"].Value     = dataDict
    object.Properties["Results/specDataDict"].Value = specDataDict

    msg("3")

    return dataDict, specDataDict

def recontructResults(object, specDataDict):
    """
    make results plotable -> from dictionary {(elemId, elemFaceId) : result} to dictionary {SElemFace : result}
    :param object:
    :return:
    """
    efsList         = []
    controller      = object.Controller
    # dictResults = System.Collections.Generic.Dictionary[System.Tuple[System.Int32, System.Int32], System.Double]()
    dictResults     = System.Collections.Generic.Dictionary[emw.SElemFace, System.Double]()
    for keyVal in specDataDict:
        ef = emw.SElemFace(em, keyVal.Key.Item1, keyVal.Key.Item2)
        dictResults[ef] = keyVal.Value
    return dictResults

def plotData(object):#elemFaces, dataDict, specDataDict, analysis, freq):
    controller              = object.Controller
    analysis                = controller.analysis
    scopeGeomEnts           = controller.scopeGeomEnts
    freq                    = controller.freq
    msg("freq: " + str(freq))
    bodies                  = controller.bodies
    elemFaces               = controller.elemFaces
    numberOfColors          = controller.numberOfColors
    decimalPlacePrecision   = controller.decimalPlacePrecision
    dictResults             = controller.dictResults
    dataDict                = controller.dataDict

    bodies.visible                  = True
    (em.bodies - bodies).visible    = False

    ExtAPI.Graphics.ViewOptions.ResultPreference.DeformationScaleMultiplier = 0.
    ExtAPI.Graphics.ViewOptions.ShowLegend                                  = False
    ExtAPI.Graphics.ViewOptions.ModelDisplay                                = ModelDisplay.Wireframe
    ExtAPI.Graphics.ViewOptions.ShowMesh                                    = True

    # elemFaces.DrawElemFacesResults(specDataDict, analysis, freq=freq, type="Result Type", unit="Unit")

    # if dictResults

    if      object.Name == "ERPPostObj":
        elemFaces.DrawElemFacesResults(dictResults, analysis, freq=freq, numberOfColors=numberOfColors, type="Specific Equivalent Radiated Power", unit="W/m^2")
        object.Properties["Results/OverallERP"].Value       = round(sum(dataDict.Values), decimalPlacePrecision)
        object.Properties["Results/OverallERPlevel"].Value  = round(10 * log10(sum(dataDict.Values) / WRef), decimalPlacePrecision)
    elif    object.Name == "ERPLevelPostObj":
        elemFaces.DrawElemFacesResults(dictResults, analysis, freq=freq, numberOfColors=numberOfColors, type="Specific Equivalent Radiated Power Level", unit="dB")
        object.Properties["Results/OverallERP"].Value       = round(sum(dataDict.Values), decimalPlacePrecision)
        object.Properties["Results/OverallERPlevel"].Value  = round(10 * log10(sum(dataDict.Values) / WRef), decimalPlacePrecision)
    elif    object.Name == "NormalVelPostObj":
        elemFaces.DrawElemFacesResults(dictResults, analysis, freq=freq, numberOfColors=numberOfColors, type="Specific Equivalent Radiated Power", unit="m/s")
        object.Properties["Results/MinVelocity"].Value      = round(min(dataDict.Values), decimalPlacePrecision)
        object.Properties["Results/MaxVelocity"].Value      = round(max(dataDict.Values), decimalPlacePrecision)
    ExtAPI.Graphics.Scene.Visible = True  # umi ukazat nebo schovat vykreslene

@callback
def CreateERPObj(analysis):
    msg("CreateERPObj")
    with ExtAPI.DataModel.Tree.Suspend(): #Transaction():
        analysis.CreatePostObject("ERPPostObj", "acousticRadiation")

@callback
def CreateERPLevelObj(analysis):
    msg("CreateERPLevelObj")
    with ExtAPI.DataModel.Tree.Suspend():
        analysis.CreatePostObject("ERPLevelPostObj", "acousticRadiation")

@callback
def CreateNormalVelObj(analysis):
    msg("CreateERPObj")
    with ExtAPI.DataModel.Tree.Suspend():
        analysis.CreatePostObject("NormalVelPostObj", "acousticRadiation")


# def plotResults(object):
#     analysis = object.Analysis
#
#     scopeGeomEnts = em.Entities(object.Properties["Settings/Geometry"].Value)
#     freq = float(object.Properties["Settings/Frequency"].Value)
#     msg("freq: " + str(freq))
#     bodies = scopeGeomEnts.bodies
#     elemFaces = scopeGeomEnts.elemFaces.Update()
#
#     """
#     C# element face ERP
#     """
#
#
#     # elemFaces.DrawElemFacesSpecERP(stepInfo, analysis)
#     numberOfColors = int(object.Properties["Settings/numberOfColors"].Value)
#     decimalPlacePrecision = 4
#
#     # if object.Name == "ERPPostObj":
#     #     elemFaces.DrawElemFacesSpecERP(freq, analysis, numberOfColors)
#     #     object.Properties["Results/OverallERP"].Value = round(elemFaces.ERP(freq, analysis),
#     #                                                                                   decimalPlacePrecision)
#     #     object.Properties["Results/OverallERPlevel"].Value = round(
#     #         elemFaces.ERPLevel(freq, analysis), decimalPlacePrecision)
#     # elif object.Name == "ERPLevelPostObj":
#     #     elemFaces.DrawElemFacesSpecERPLevel(freq, analysis, numberOfColors)
#     #     object.Properties["Results/OverallERP"].Value = round(elemFaces.ERP(freq, analysis),
#     #                                                                                   decimalPlacePrecision)
#     #     object.Properties["Results/OverallERPlevel"].Value = round(
#     #         elemFaces.ERPLevel(freq, analysis), decimalPlacePrecision)
#     # elif object.Name == "NormalVelPostObj":
#     #     elemFaces.DrawElemFacesNormalV(freq, analysis, numberOfColors)
#     #     object.Properties["Results/MinVelocity"].Value = round(
#     #         elemFaces.Min(lambda elemFace: elemFace.GetElemFaceNormalV(freq, analysis, analysis.GetResultsData())).GetElemFaceNormalV(freq,
#     #                                                                                                        analysis, analysis.GetResultsData()),
#     #         decimalPlacePrecision)
#     #     object.Properties["Results/MaxVelocity"].Value = round(
#     #         elemFaces.Max(lambda elemFace: elemFace.GetElemFaceNormalV(freq, analysis, analysis.GetResultsData())).GetElemFaceNormalV(freq,
#     #                                                                                                        analysis, analysis.GetResultsData()),
#     #         decimalPlacePrecision)
#
#     ExtAPI.Graphics.Scene.Visible = True  # umi ukazat nebo schovat vykreslene
#
#     # else:
#     #     msg.("Result must be evaluated first")

    """
    element face ERP results
    """
    # nodeAreas = {}
    # nodeAreasNotDivided = {}
    # nodeNums = {}
    #
    # elemFacesAreas = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesNormals = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesUNormal = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesVNormal = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesSpecERP = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesERP = {}  # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # URes = res.GetResult("U")
    # # UVals = URes.GetNodeValues(elemFaces.nodes.info.Ids)
    #
    # ERPElemFaceSum = 0
    # for elemFace in elemFaces:
    #     # elemFacesAreas[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFace.elemFaceArea
    #     # elemFacesNormals[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFace.normal
    #     elemFaceArea = elemFace.elemFaceArea
    #     elemFaceNormal = elemFace.normal
    #     UElemFaceNormalSum = 0
    #     for elemFaceNode in elemFace.nodes:
    #         UVal = URes.GetNodeValues(elemFaceNode.id)
    #
    #         Ux = URes.GetNodeValues(elemFaceNode.id)[0]
    #         Uy = URes.GetNodeValues(elemFaceNode.id)[1]
    #         Uz = URes.GetNodeValues(elemFaceNode.id)[2]
    #         UNormal = elemFaceNormal.x * Ux + elemFaceNormal.y * Uy + elemFaceNormal.z * Uz
    #         UElemFaceNormalSum += UNormal
    #     UElemFaceNormal = UElemFaceNormalSum / elemFace.nodes.count
    #     VElemFaceNormal = UElemFaceNormal * omega
    #     specERPElemFace = 1.0 / 2.0 * ro * c * VElemFaceNormal ** 2
    #     ERPElemFace = 1.0 / 2.0 * ro * c * VElemFaceNormal ** 2 * elemFaceArea
    #
    #     ERPElemFaceSum += ERPElemFace
    #
    #     elemFacesAreas[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFaceArea
    #     elemFacesNormals[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFaceNormal
    #     elemFacesUNormal[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = UElemFaceNormal
    #     elemFacesVNormal[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = VElemFaceNormal
    #     elemFacesSpecERP[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = specERPElemFace
    #     elemFacesERP[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = ERPElemFace
    #
    # # msg("elemFacesAreas: " + str(elemFacesAreas))
    # # msg("elemFacesNormals: " + str(elemFacesNormals))
    # # msg("elemFacesUNormal: " + str(elemFacesUNormal))
    # # msg("elemFacesVNormal: " + str(elemFacesVNormal))
    # # msg("elemFacesSpecERP: " + str(elemFacesSpecERP))
    # # msg("elemFacesERP: " + str(elemFacesERP))
    #
    # msg("ERPElemFaceSum [W]: " + str(ERPElemFaceSum))
    # try:
    #     ERPLevElemFaceSum = 10.0 * log10(ERPElemFaceSum / WRef)
    #     msg("ERPLevElemFaceSum [dB]: " + str(ERPLevElemFaceSum))
    # except Exception as e:
    #     msg(str(e) + ": Probably problem with zero argument of logarithm, which is not defined (or can be alternatively interpreted as negative infinity)")
    # msg("freq [Hz]: " + str(freq))


    """
    nodal ERP results
    """
    # for n in nodes:
    #     outNeighborElemFaces = n.elemFaces * elemFaces # vnejsi sousedni elemFaces k danemu uzlu, vyfiltrovane pomoci elemFaces
    #     # outNeighborElemFaces.Sel()
    #     # msg("elemFaces length: " + str(len(outNeighborElemFaces)))
    #     nodeArea = 0
    #     nodeNum = 0
    #     nodeAreaNotDivided = 0
    #     for outNeighborElemFace in outNeighborElemFaces:
    #         # msg("outNeighborElemFace: " + str(outNeighborElemFace))
    #         nodeArea += outNeighborElemFace.elemFaceArea / len(outNeighborElemFace.nodes.corners)
    #         nodeNum += len(outNeighborElemFace.nodes.corners)
    #         nodeAreaNotDivided += outNeighborElemFace.elemFaceArea
    #         # nodeNormal = outNeighborElemFace.
    #         # msg("done ")
    #
    #     nodeAreas[n.id] = nodeArea
    #     nodeAreasNotDivided[n.id] = nodeAreaNotDivided
    #     nodeNums[n.id] = nodeNum
    # sumArea = 0
    # sumAreasNotDivided = 0
    # sumNodeNums = 0

    # for valNodeAreas, valNodeAreasNotDivided, valNodeNums in zip(nodeAreas.values(), nodeAreasNotDivided.values(), nodeNums.values()):
    #     sumArea += valNodeAreas
    #     sumAreasNotDivided += valNodeAreasNotDivided
    #     sumNodeNums += valNodeNums
    # msg("nodeAreas: "+ str(nodeAreas))
    # msg("nodeAreasNotDivided: "+ str(nodeAreasNotDivided))
    # msg("nodeNums: "+ str(nodeNums))
    #
    # msg("sumArea: "+ str(sumArea))
    # msg("sumAreasNotDivided: "+ str(sumAreasNotDivided))
    # msg("sumNodeNums: "+ str(sumNodeNums))

    # U = res.GetResult("U")
    # UVals = U.GetNodeValues(elemFaces.nodes.info.Ids)

""" 
puvodni pythonovske reseni
"""

    # global OBJ
    # OBJ = object
    # sel = OBJ.Properties["Geometry"].Value
    # emSel = em.SelEnts(sel)
    #
    # global elemFaces
    # elemFaces = emSel.elemFaces
    #
    # nodes = elemFaces.nodes.corners
    # analysis = OBJ.Analysis
    # nodes.Sel()
    #
    # try:
    #     res = analysis.GetResultsData()
    # except:
    #     msg("Results could not be loaded.")
    #     pass
    #
    # Freqs = res.ListTimeFreq
    # freq = res.CurrentTimeFreq
    #
    # # for el in elemFaces:
    # #     area = el.elemFaceArea
    # #     msg("area: " + str(area))
    #
    #
    #
    # """
    # element face ERP results
    # """
    # nodeAreas = {}
    # nodeAreasNotDivided = {}
    # nodeNums = {}
    #
    #
    # elemFacesAreas    = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId) element id, elemface id
    # elemFacesNormals  = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesUNormal  = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesVNormal  = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesSpecERP  = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)
    # elemFacesERP      = {} # key = str(elemFace.id) + "," + str(elemFace.elemFaceId)

    # elemFacesERP      = {} # key =  str(elemFace.id) + "," + str(elemFace.elemFaceId)

    # URes = res.GetResult("U")
    # # UVals = URes.GetNodeValues(elemFaces.nodes.info.Ids)
    #
    #
    # for elemFace in elemFaces:
    #     # elemFacesAreas[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFace.elemFaceArea
    #     # elemFacesNormals[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFace.normal
    #     elemFaceArea = elemFace.elemFaceArea
    #     elemFaceNormal = elemFace.normal
    #     UElemFaceNormalSum = 0
    #     for elemFaceNode in elemFace.nodes:
    #         UVal = URes.GetNodeValues(elemFaceNode.id)
    #
    #         Ux = URes.GetNodeValues(elemFaceNode.id)[0]
    #         Uy = URes.GetNodeValues(elemFaceNode.id)[1]
    #         Uz = URes.GetNodeValues(elemFaceNode.id)[2]
    #         UNormal = elemFaceNormal.x * Ux + elemFaceNormal.y * Uy + elemFaceNormal.z * Uz
    #         UElemFaceNormalSum += UNormal
    #     UElemFaceNormal = UElemFaceNormalSum / elemFace.nodes.count
    #     VElemFaceNormal = UElemFaceNormal * freq
    #     specERPElemFace = 1.0/2.0 * ro * c * VElemFaceNormal**2
    #     ERPElemFace = 1.0/2.0 * ro * c * VElemFaceNormal**2 * elemFaceArea
    #
    #     elemFacesAreas[str(elemFace.id)   + "," + str(elemFace.elemFaceId)] = elemFaceArea
    #     elemFacesNormals[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = elemFaceNormal
    #     elemFacesUNormal[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = UElemFaceNormal
    #     elemFacesVNormal[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = VElemFaceNormal
    #     elemFacesSpecERP[str(elemFace.id) + "," + str(elemFace.elemFaceId)] = specERPElemFace
    #     elemFacesERP[str(elemFace.id)     + "," + str(elemFace.elemFaceId)] = ERPElemFace
    #
    # msg("elemFacesAreas: "+ str(elemFacesAreas))
    # msg("elemFacesNormals: "+ str(elemFacesNormals))
    # msg("elemFacesUNormal: "+ str(elemFacesUNormal))
    # msg("elemFacesVNormal: "+ str(elemFacesVNormal))
    # msg("elemFacesSpecERP: "+ str(elemFacesSpecERP))
    # msg("elemFacesERP: "+ str(elemFacesERP))
    #
    # """
    # nodal ERP results
    # """
    # # for n in nodes:
    # #     outNeighborElemFaces = n.elemFaces * elemFaces # vnejsi sousedni elemFaces k danemu uzlu, vyfiltrovane pomoci elemFaces
    # #     # outNeighborElemFaces.Sel()
    # #     # msg("elemFaces length: " + str(len(outNeighborElemFaces)))
    # #     nodeArea = 0
    # #     nodeNum = 0
    # #     nodeAreaNotDivided = 0
    # #     for outNeighborElemFace in outNeighborElemFaces:
    # #         # msg("outNeighborElemFace: " + str(outNeighborElemFace))
    # #         nodeArea += outNeighborElemFace.elemFaceArea / len(outNeighborElemFace.nodes.corners)
    # #         nodeNum += len(outNeighborElemFace.nodes.corners)
    # #         nodeAreaNotDivided += outNeighborElemFace.elemFaceArea
    # #         # nodeNormal = outNeighborElemFace.
    # #         # msg("done ")
    # #
    # #     nodeAreas[n.id] = nodeArea
    # #     nodeAreasNotDivided[n.id] = nodeAreaNotDivided
    # #     nodeNums[n.id] = nodeNum
    # # sumArea = 0
    # # sumAreasNotDivided = 0
    # # sumNodeNums = 0
    #
    # # for valNodeAreas, valNodeAreasNotDivided, valNodeNums in zip(nodeAreas.values(), nodeAreasNotDivided.values(), nodeNums.values()):
    # #     sumArea += valNodeAreas
    # #     sumAreasNotDivided += valNodeAreasNotDivided
    # #     sumNodeNums += valNodeNums
    # # msg("nodeAreas: "+ str(nodeAreas))
    # # msg("nodeAreasNotDivided: "+ str(nodeAreasNotDivided))
    # # msg("nodeNums: "+ str(nodeNums))
    # #
    # # msg("sumArea: "+ str(sumArea))
    # # msg("sumAreasNotDivided: "+ str(sumAreasNotDivided))
    # # msg("sumNodeNums: "+ str(sumNodeNums))
    #
    #
    #
    # # U = res.GetResult("U")
    # # UVals = U.GetNodeValues(elemFaces.nodes.info.Ids)