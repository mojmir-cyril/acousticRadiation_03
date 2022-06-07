#############################################################
#
#  multi-line:
#
############################################################# 
em.CS()
bs1 = em.bodies
bs2 = bs1.IfName("body")
es1 = bs2.elems
ns1 = es1.nodes
ns2 = ns1.If(lambda n: n.x < 5 and n.y > 10)
es2 = ns2.elems
es2.Sel() 
#############################################################
#
#  single-line:
#
############################################################# 
em.CS().bodies.IfName("body").elems.nodes.If(lambda n: n.x < 5 and n.y > 10).elems.Sel()
 
