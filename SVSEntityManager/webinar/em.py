#############################################################
#
#  Entity Manager:
#
############################################################# 
import clr
clr.AddReferenceToFileAndPath(r"e:\C#Tools\SVSEntityManagerF472.dll")
import SVSEntityManagerF472 
em = SVSEntityManagerF472.SEntityManager(ExtAPI)
#############################################################
#
#  Help:
#
#############################################################  
em.Help() 
#############################################################
#
#  Example:
#
############################################################# 
fix.Location = em.Entities(geos).info
 
