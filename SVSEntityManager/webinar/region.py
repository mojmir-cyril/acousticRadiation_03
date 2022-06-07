geos = [747,746,667,749,748]
#############################################################
#
#  ACT:
#
############################################################# 
mesh = DataModel.MeshDataByName("Global") 
regs = [mesh.MeshRegionById(id) for id in geos]  
nodes = []
for r in regs: nodes += r.Nodes 
sm = ExtAPI.SelectionManager
inf = sm.CreateSelectionInfo(SelectionTypeEnum.MeshNodes)  
inf.Ids = [n.Id for n in nodes]  
sm.NewSelection(inf)
#############################################################
#
#  Entity Manager:
#
############################################################# 
em.Entities(geos).nodes.Sel()