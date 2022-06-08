# -*- coding: utf-8 -*-
wrn("ObjectController")
import units
wrn("units imported")
class ObjectController():

    quantityDict = {"ERPPostObj"        :"Power",
                "ERPLevelPostObj"       :"Sound Pressure Level",
                "NormalVelPostObj"      :"Velocity",
                "ERPRangePostObj"       :"Power",
                "ERPLevelRangePostObj"  :"Sound Pressure Level",
                "NormalVelRangePostObj" :"Velocity"}

    specQuantityDict = {"ERPPostObj"    :"Heat Flux",
                "ERPLevelPostObj"       :"Sound Pressure Level",
                "NormalVelPostObj"      :"Velocity",
                "ERPRangePostObj"       :"Heat Flux",
                "ERPLevelRangePostObj"  :"Sound Pressure Level",
                "NormalVelRangePostObj" :"Velocity"}

    mksUnitDict = {"ERPPostObj"          :"W",
                "ERPLevelPostObj"       :"dB",
                "NormalVelPostObj"      :"m/s",
                "ERPRangePostObj"       :"W",
                "ERPLevelRangePostObj"  :"dB",
                "NormalVelRangePostObj" :"m/s"}

    mksSpecUnitDict = {"ERPPostObj"     :"W/m^2",
                "ERPLevelPostObj"       :"dB",
                "NormalVelPostObj"      :"m/s",
                "ERPRangePostObj"       :"W/m^2",
                "ERPLevelRangePostObj"  :"dB",
                "NormalVelRangePostObj" :"m/s"}


    @callback
    def __init__(self, ExtAPI, object):
        msg("onInitCtrl")

        createEmAndEmw()
        msg("1")

        global prevObj
        prevObj = object
        # updateProperties(ExtAPI, object)
        self.name                   = "ObjectController" + object.Name
        self.ExtAPI                 = ExtAPI
        self.decimalPlacePrecision  = 4
        self.object                 = object
        self.updated                = False
        msg("2")
        self.quantityName           = self.quantityDict[self.object.Name]
        self.currentUnit            = units.GetCurrentCompactUnitString(self.quantityName).replace("²", "^2")
        self.specQuantityName       = self.specQuantityDict[self.object.Name]
        wrn("before specUnit")
        self.currentSpecUnit        = units.GetCurrentCompactUnitString(self.specQuantityName).replace("²", "^2")
        wrn("after specUnit")
        msg("3")

        self.mksUnit                = self.mksUnitDict[self.object.Name].replace("²", "^2")
        self.mksSpecUnit            = self.mksSpecUnitDict[self.object.Name].replace("²", "^2")
        self.unitWhenShowed         = self.mksUnit
        msg("4")

        if not "isDefPropertiesSet" in dir(self):
            self.isDefPropertiesSet = False

        # if object.Properties["Results/specDataDict"].Value != None:
        #     msg("ifspecDataDict")
        #     self.updateProperties()

    @callback
    def onready(self, *object):
        msg("onready")

    @callback
    def updateProperties(self):
        msg("updateProperties")
        self.name = "ObjectController" + self.object.Name
        # self.ExtAPI = ExtAPI
        self.analysis               = self.object.Analysis
        msg("before em scope: " + str(self.object.Properties["Settings/Geometry"].Value))

        self.scopeGeomEnts          = em.Entities(self.object.Properties["Settings/Geometry"].Value)
        msg("after em scope")

        self.freq                   = float(self.object.Properties["Settings/Frequency"].Value)
        self.bodies                 = self.scopeGeomEnts.bodies
        self.elemFaces              = self.scopeGeomEnts.elemFaces.Update()
        self.numberOfColors         = int(self.object.Properties["Settings/numberOfColors"].Value)
        self.decimalPlacePrecision  = 4
        self.quantityName           = self.quantityDict[self.object.Name]
        self.currentUnit            = units.GetCurrentCompactUnitString(self.quantityName).replace("²", "^2")
        self.specQuantityName       = self.specQuantityDict[self.object.Name]
        wrn("before specUnit")
        self.currentSpecUnit        = units.GetCurrentCompactUnitString(self.specQuantityName).replace("²", "^2")
        wrn("after specUnit")

        msg("before if")
        msg("bool 1:" + str(self.object.State))
        msg("bool 2:" + str(self.object.Properties["Results/specDataDict"].Value != None))
        if self.object.Properties["Results/specDataDict"].Value != None: # set(["dataDict", "specDataDict", "dictResults"]).issubset([i.UniqueName for i in self.object.AllProperties])):
            try:
                ##################################
                self.dataDict       = self.object.Properties["Results/dataDict"].Value
                self.specDataDict   = self.object.Properties["Results/specDataDict"].Value
                self.dictResults    = recontructResults(self.object, self.specDataDict)

                self.dataDict       = convertDictUnits(self.dataDict, self.mksUnit, self.currentUnit)
                wrn(self.mksSpecUnit)
                wrn(self.currentSpecUnit)
                self.specDataDict   = convertDictUnits(self.specDataDict, self.mksSpecUnit, self.currentSpecUnit)

                self.dictResults    = convertDictUnits(self.dictResults, self.mksSpecUnit, self.currentSpecUnit)

                ##################################
                msg("data revived from treeObj properties")
            except Exception as e:
                msg("Error when updating properties (class ObjectController, method updateProperties)" + str(e))
            # tady vyplnit dane property z ulozenych dat v property objektu ve strome

    @callback
    def isvalid(self, object, *args):
        msg("isvalid")
        try:
            if object.Properties["Results/specDataDict"].Value != None and not self.updated:
                msg("ifspecDataDict")
                self.updateProperties()
                self.updated = True

        except Exception as e:
            err(str(e))
        return True

    @callback
    def onadd(self, object):
        """
        presets default settings of tree object properties
        :param object: 
        :return: 
        """
        msg("onAdd")

        s = ExtAPI.SelectionManager.CreateSelectionInfo(0)
        solidBody = Ansys.ACT.Interfaces.Geometry.GeoBodyTypeEnum.GeoBodySolid
        sheetBody = Ansys.ACT.Interfaces.Geometry.GeoBodyTypeEnum.GeoBodySheet
        if em.current == None:
            allTreeBodies = Model.Geometry.GetChildren(DataModelObjectCategory.Body, True)
            flexibleGeoBodies = [body.GetGeoBody() for body in allTreeBodies if
                                 [property for property in body.Properties if property.Name == "StiffnessBehavior"][
                                     0].StringValue == "Flexible"]
            s.Ids = [body.Id for body in flexibleGeoBodies if
                     body.BodyType == solidBody or body.BodyType == sheetBody]
        else:
            # s.Ids = em.current.ids
            s.Ids = [body.id for body in em.current if body.bodyType == solidBody or body.bodyType == sheetBody]
        object.Properties["Settings/Geometry"].Value = s

        analysisId = object.Analysis.Id
        treeAnalysis = [i for i in Model.Analyses if i.Id == analysisId][0]
        object.Properties["Settings/Frequency"].Value = treeAnalysis.AnalysisSettings.RangeMaximum.Value

        Tree.Refresh()
        self.isDefPropertiesSet      = True
        self.updateProperties()

    @callback
    def onshow(self, object):
        """

        :param object:
        :return:
        """
        msg("onShow")
        msg("ObjCaption: " + object.Caption)

        self.updateProperties()

        global prevObj
        global prevModelDisplay
        global prevVisibleBodies
        global prevDeformationScaleMultiplier
        prevObj = object
        prevModelDisplay = ExtAPI.Graphics.ViewOptions.ModelDisplay
        prevVisibleBodies = em.Entities([body.id for body in em.bodies if body.visible == True]).bodies
        prevDeformationScaleMultiplier = Graphics.ViewOptions.ResultPreference.DeformationScaleMultiplier

        length = Tree.ActiveObjects.Count
        msg("a0")
        try:
            if self.dictResults != None and object.ObjectId == Tree.ActiveObjects[length-1].ObjectId:
                # plotResults(object)
                msg("a1")
                # if self.unitWhenShowed != self.currentUnit:
                msg("a2")


                self.unitWhenShowed = units.GetCurrentCompactUnitString(self.quantityName)
                msg("a3")

                try:
                    plotData(object)
                except Exception as e:
                    msg(str(e))

                scopeGeomEnts = em.Entities(object.Properties["Settings/Geometry"].Value)
                freq = float(object.Properties["Settings/Frequency"].Value)
                msg("freq: " + str(freq))
                bodies = scopeGeomEnts.bodies

                # em.bodies.visible = False
                bodies.visible = True
                (em.bodies - bodies).visible = False

                ExtAPI.Graphics.ViewOptions.ResultPreference.DeformationScaleMultiplier = 0.
                ExtAPI.Graphics.ViewOptions.ShowLegend = False
                ExtAPI.Graphics.ViewOptions.ModelDisplay = ModelDisplay.Wireframe
                ExtAPI.Graphics.ViewOptions.ShowMesh = True
        except Exception as e:
            err(str(e))

    @callback
    def ongenerate(self, object, func):
        msg("onGenerate")
        func(0, "Start Generating Data")

        self.updateProperties()

        # workDirPath = analysis.WorkingDir
        # filename = "acoustRad_evalResultDict.pickle" # object.Name + "_id_" + str(object.Id)
        # fullPath = os.path.join(workDirPath, filename)
        # msg(fullPath)
        def UIThread(object):
            try:
                self.analysis.GetResultsData()
                # if Tree.
                state = True
            except:
                msg("Results could not be loaded.")
                state = False
            dataDict, specDataDict          = GetDataDict(object)
            dictResults                     = recontructResults(object, specDataDict)
            return state, dataDict, specDataDict, dictResults
        self.state, self.dataDict, self.specDataDict, self.dictResults = ExtAPI.Application.InvokeUIThread(UIThread, object)

        object.Properties["Results/dataDict"].Value     = self.dataDict     # saved without conversion -> in mks
        object.Properties["Results/specDataDict"].Value = self.specDataDict # saved without conversion -> in mks
        # object.Properties["Results/dictResults"].Value  = self.dictResults  # saved without conversion -> in mks

        func(100, "Done Generating Data")
        return ExtAPI.Application.InvokeUIThread(UIThread, object)

    @callback
    def onaftergenerate(self, object):
        msg("onAfterGenerate")
        length = Tree.ActiveObjects.Count
        if object.ObjectId == Tree.ActiveObjects[length-1].ObjectId:
            self.onshow(object)

    @callback
    def onhide(self, object):
        msg("onHide")
        ExtAPI.Graphics.ViewOptions.ShowLegend = True
        ExtAPI.Graphics.Scene.Visible = False  # umi ukazat nebo schovat vykreslene
        try:
            ExtAPI.Graphics.ViewOptions.ModelDisplay = prevModelDisplay
            ExtAPI.Graphics.ViewOptions.ResultPreference.DeformationScaleMultiplier = prevDeformationScaleMultiplier
        except:
            msg("prevModelDisplay and prevDeformationScaleMultiplier are not defined yet")
        prevVisibleBodies.visible = True
        (em.bodies - prevVisibleBodies).visible = False

