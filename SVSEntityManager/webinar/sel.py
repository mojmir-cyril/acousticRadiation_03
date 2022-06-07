geos = [747,746,667,749,748]
fix  = Tree.FirstActiveObject.AddFixedSupport()
#############################################################
#
#  ACT:
#
############################################################# 
sm = ExtAPI.SelectionManager
inf = sm.CreateSelectionInfo(SelectionTypeEnum.GeometryEntities)
inf.Ids = geos   
fix.Location = inf

