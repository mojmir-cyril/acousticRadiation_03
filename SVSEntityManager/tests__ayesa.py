####################################################################################
#
#   EM:
#
#################################################################################### 
#
# EM =  ... from ACT: TestEM 
# em =  ... from ACT: TestEM 
#
#  time:
#
import datetime
Now = datetime.datetime.now
#
em.Msg("---------------------------------------------------------")
em.Msg("   test.py:                                              ")
em.Msg("---------------------------------------------------------")
em.Msg("for: ")
em.Msg(r"  E:\C#Tools\SVSEntityManager\models\ayesa.wbpj")
em.Msg("")
#
#  model : "e:\AnyParamater\ayesa.wbpj"
#
def Compare(id, i1, i2):  # id = CHECK-001-001
    if i1 != i2: em.Err(str(id) + " : failed ({} != {})".format(i1, i2))
    else:        em.Msg(str(id) + " : <b><font color=green>ok</font></b> ({} == {})".format(i1, i2))
#
def SelAndCount(o): 
    now = Now()
    o.Sel()
    dt = Now() - now
    if dt.seconds > 3: em.Msg(dt)
    return em.current.count
#
#  group 001:
#
Compare("CHECK-001-001", SelAndCount(em.bodies)     ,    266 )
Compare("CHECK-001-002", SelAndCount(em.surfs)      ,     31 )
Compare("CHECK-001-003", SelAndCount(em.lines)      ,    164 )
Compare("CHECK-001-004", SelAndCount(em.solids)     ,     71 )
Compare("CHECK-001-005", SelAndCount(em.faces)      ,   2346 )
Compare("CHECK-001-006", SelAndCount(em.edges)      ,   6615 )
Compare("CHECK-001-007", SelAndCount(em.verts)      ,   4579 )
Compare("CHECK-001-008", SelAndCount(em.elems)      , 526302 )
Compare("CHECK-001-009", SelAndCount(em.nodes)      , 983412 ) # 983414
Compare("CHECK-001-010", SelAndCount(em.elemFaces)  ,      0 ) 


em = EM()
print em.elemFaces.count

#
#  group 002:
#
Compare("CHECK-002-001", em.surfs.faces.count        , 226  )
Compare("CHECK-002-002", em.lines.edges.count        , 853  )
Compare("CHECK-002-003", em.solids.faces.count       , 2120 )
Compare("CHECK-002-004", em.solids.edges.count       , 5082 )
Compare("CHECK-002-005", em.solids.faces.edges.count , 5082 ) 
#
#  group 003:
# 
Compare("CHECK-003-001", em.edges.circles.count       , 2038)
Compare("CHECK-003-002", em.edges.circles.faces.count , 1552)
Compare("CHECK-003-003", em.faces.count               , 2346)
Compare("CHECK-003-004", em.edges.count               , 6615)
Compare("CHECK-003-005", em.verts.count               , 4579)
Compare("CHECK-003-006", em.lines.edges.count         ,  853)
Compare("CHECK-003-007", em.solids.edges.count        , 5082)
Compare("CHECK-003-008", em.surfs.edges.count         ,  680)
#
#  group 004:
# 
Compare("CHECK-004-001", (em.lines.edges + em.solids.edges + em.surfs.edges).count   , 6615 )
Compare("CHECK-004-002", (em.edges - em.solids.edges).count                          , 1533 )
Compare("CHECK-004-003", (em.edges.circles * em.solids.edges).count                  , 1691 ) 
Compare("CHECK-004-004", (em.faces.planes + em.faces.cyls).count                     , 2290 ) 
Compare("CHECK-004-005", (em.faces.planes + em.faces.cyls * em.solids.faces).count   , 2111 ) 
Compare("CHECK-004-006", ((em.faces.planes + em.faces.cyls) * em.solids.faces).count , 2064 ) 
#
#  group 005:
#
Compare("CHECK-005-001", em.AccurDigits(length = 0, angle = 1, mass = 1).bodies[0].x  , 551845.00 )
Compare("CHECK-005-002", em.AccurDigits(length = 2, angle = 1, mass = 1).bodies[0].x  , 551844.91 )
Compare("CHECK-005-003", em.AccurDigits(length = -2, angle = 1, mass = 1).bodies[0].x , 551800.00 )
#
#  group 006:
#
Compare("CHECK-006-001", SelAndCount(em.AccurDigits(length = -2, angle = 1, mass = 1).bodies[0].Extend(lambda c, x: c.volume == x.volume)),     6   )
Compare("CHECK-006-002", max(em.AccurDigits(length = -4, angle = 1, mass = -1).current.masses),                                             41870.0 )
#
#  group 007:
#
em.AccurDigits(length = 0, angle = 0, mass = 0)
Compare("CHECK-007-001", em.parts.count                        ,   13 )
Compare("CHECK-007-002", em.parts.If(lambda e: e.x > 0).count  ,    0 )
Compare("CHECK-007-003", em.bodies.If(lambda e: e.x > 0).count ,  249 )   # 241
Compare("CHECK-007-004", em.faces.If(lambda e: e.x > 0).count  , 2345 )
Compare("CHECK-007-005", em.edges.If(lambda e: e.x > 0).count  , 6606 )
Compare("CHECK-007-006", em.verts.If(lambda e: e.x > 0).count  , 4579 )
#
#  group 008:
#
em.CS("Level - 20 m")
Compare("CHECK-008-001", em.parts.count                        ,   13 )
Compare("CHECK-008-002", em.parts.If(lambda e: e.y > 0).count  ,    0 )
Compare("CHECK-008-003", em.bodies.If(lambda e: e.y > 0).count ,   70 )
Compare("CHECK-008-004", em.faces.If(lambda e: e.y > 0).count  , 2113 )
Compare("CHECK-008-005", em.edges.If(lambda e: e.y > 0).count  , 5903 )
Compare("CHECK-008-006", em.verts.If(lambda e: e.y > 0).count  , 4089 )
em.CS()        
#
#  group 009:
#
Compare("CHECK-009-001", em.parts.IfName("pile").count  ,    3 )
Compare("CHECK-009-002", em.bodies.IfName("pile").count ,  101 )
Compare("CHECK-009-003", em.faces.IfName("pile").count  ,  695 )
Compare("CHECK-009-004", em.edges.IfName("pile").count  , 1847 )
Compare("CHECK-009-005", em.verts.IfName("pile").count  , 1252 )
#
#  group 010:
#
em.bodies.Unsuppress()
em.bodies.IfName("counter").Suppress()
em.bodies.IfName("pile").Show().HideOthers()
Compare("CHECK-010-001", em.bodies.actives.count      , 256)
Compare("CHECK-010-002", em.bodies.suppresseds.count  ,  10)
Compare("CHECK-010-003", em.bodies.showns.count       , 101)
Compare("CHECK-010-004", em.bodies.hiddens.count      , 155)
Compare("CHECK-010-005", em.actives.count             , 256)
Compare("CHECK-010-006", em.suppresseds.count         ,  10)
Compare("CHECK-010-007", em.showns.count              , 101)
Compare("CHECK-010-008", em.hiddens.count             , 155)
#
#  group 011:
# 
a = em.Entities([18143]).faces          # 11
b = em.Entities([18246, 18237, 18243])  # 3
c = em.Entities([18144, 18246, 18243])  # 3
a.Sel()
b.Remove()
Compare("CHECK-011-001", em.current.count, 8)
a.Sel()
b.Filter()
Compare("CHECK-011-002", em.current.count, 3)
a.Sel()
c.Filter()
Compare("CHECK-011-003", em.current.count, 3)
b.Sel()
c.Filter()
Compare("CHECK-011-004", em.current.count, 2)
b.Sel()
c.Add()
Compare("CHECK-011-005", em.current.count, 4)
b.Sel()
b.Remove()
 
   
 ####################################################################################
 ####################################################################################
 ####################################################################################

 

em.solids.If(lambda f: f.volume < 0.0001).Sel()

em.solids.Max(lambda e: e.volume, count = 5).Sel()
em.solids.Min(lambda e: e.volume, count = 5).Sel()

em.solids.Min(lambda e:  e.x, count = 5).Sel()
em.solids.Min(lambda e: -e.x, count = 5).Sel()

tot = em.solids.Sum(lambda f: f.volume) 
avg = tot / em.solids.count

em.solids.Min(lambda e:  e.x, count = 5).Get(lambda e: e.name)
em.solids.Min(lambda e:  e.x, count = 5).Get(lambda e: e.volume)


this = Tree.FirstActiveObject
this.Location = em.solids.Min(lambda e:  e.x, count = 5).location


em = EntityManager.SEntityManager(ExtAPI) 
em.faces.Units("mm", "deg").If(lambda e: -33    < e.x < -32).Sel()
em.faces.Units("m",  "deg").If(lambda e: -0.033 < e.x < -0.032).Sel()

em.solids.CS("Local 1").If(lambda f: f.x < 0.0001).Sel()
em.solids.CS(55).If(lambda f: f.x < 0.0001).Sel()
em.solids.CS(55).Units("mm", "deg").If(lambda f: f.angle < 45).Sel()
em.solids.Units("mm", "deg").CS("Local 2").If(lambda f: -0.1 < f.x < 0.1).Sel()

em.NS("group 1").Sel()
em.NS(105).Sel()
# em.NS(nsObj).Sel()  

em = EntityManager.SEntityManager(ExtAPI)
now = datetime.datetime.now()
em.faces.CS("Coordinate System").Units("mm", "deg").If(lambda e: e.x > 0).Sel()
print datetime.datetime.now() - now






# ----------------------------------------- #
#   NEW 1.4.2021:
# ----------------------------------------- #

em = EntityManager.SEntityManager(ExtAPI)

print em.current[0].Ip1
print em.current[0].Ip2
print em.current[0].Ip3

print em.current.bodies.names


em.current += em.faces
em.current  = em.faces - em.current

# ----------------------------------------- #
#   NEW 3.4.2021:
# ----------------------------------------- #

em = EntityManager.SEntityManager(ExtAPI)
print em.bodies.Count(lambda b: b.x < 10)
print em.bodies.Avg(lambda b: b.x)
print em.faces.Avg(lambda b: b.x)
print em.edges.Avg(lambda b: b.x)
print em.verts.Avg(lambda b: b.x)

print em.bodies.IfName("comp")
print em.faces.IfName("comp")
print em.edges.IfName("comp")
print em.verts.IfName("comp")

#
#  Intersect:
#
(em.current * em.bodies.IfName("bolt")).Sel()

# 
#   Help():								    
# 
em.Help()



# 
#   Multi-criterion Analysis --> score:								    
# 
em = EntityManager.SEntityManager(ExtAPI)
bs = em.bodies
V  = lambda b: b.volume
X  = lambda b: b.x
Y  = lambda b: b.y
Z  = lambda b: b.z
dv = bs.Avg(V)
dx = bs.Avg(lambda b: abs(b.x) + abs(b.y) + abs(b.z))
c1 = em.NewBodyCriterion(bodyFunc = V, targetValue = bs.Stats(V).max, zeroDifference = dv, topScore = 100)
c2 = em.NewBodyCriterion(bodyFunc = X, targetValue = bs.Stats(X).max, zeroDifference = dx, topScore =  50)
c3 = em.NewBodyCriterion(bodyFunc = Y, targetValue = bs.Stats(Y).avg, zeroDifference = dx, topScore =  50)
c4 = em.NewBodyCriterion(bodyFunc = Z, targetValue = bs.Stats(Z).min, zeroDifference = dx, topScore =  50)
bs.Max(lambda b: b.Score([c1, c2, c3, c4]), 5).Sel()

print bs.Get(lambda b: b.Score([c1, c2, c3, c4]))

for b in bs: 
    s = b.Score([c1, c2, c3, c4])
    b.SetRGB(s, 255 - s, 255 - s)
    b.transparency = 1.0
    print b.name + " =====> " + str(s)
    
# 
#   set props:								    
# 
em = EntityManager.SEntityManager(ExtAPI)
em.bodies.transparency = 0.2
em.bodies.name         = "bolt"
em.bodies.materialName = "steel"
em.bodies.color        = 0x888888

#
#  face orientation (normal):
#
em = EntityManager.SEntityManager(ExtAPI)
f = em.faces[0]
print f.avgNormal    # ---> [vx, vy, vz]
print f.avgNormal.x 
print f.avgNormal.y 
print f.avgNormal.z 

#
#  check orientation:
#
f1     = em.faces[0]
f2     = em.faces[52]
scalar = f1.avgNormal * f2.avgNormal
print f1.avgNormal
print f2.avgNormal
print scalar

em.faces.If(lambda f: f.avgNormal * em.Normal(0, 0, 1) > 0.90).Sel()

em.faces.cyls.CS(em.tree.first).If(lambda f: f.avgNormal * em.Normal(1,0,0) > 0.98 and f.radius == 20).Sel()


# ----------------------------------------- #
#   NEW 6.4.2021:
# ----------------------------------------- #

em.surfs.Activate()
em.surfs.faces.cyls.bodies.Activate()
em.surfs.suppresseds.Activate()
em.surfs.showns.Activate()
em.surfs.hiddens.Activate()
em.surfs.hiddens.Activate()
em.surfs.hiddens.Sel()
em.surfs.actives.Sel()

#
#  shape index 1D, 2D, 3D:
#
em = EntityManager.SEntityManager(ExtAPI)
b = em.current[0]
print "Ip1 : " + str(b.Ip1)
print "Ip2 : " + str(b.Ip2)
print "Ip3 : " + str(b.Ip3)
print "1D index : " + str(b.shapeIndex1D)
print "2D index : " + str(b.shapeIndex2D)
print "3D index : " + str(b.shapeIndex3D)

# ----------------------------------------- #
#   NEW 9.4.2021:
# ----------------------------------------- #

#
#  polar selection:
#
em.faces.If(lambda f: 19 < f.r < 21).Sel() 

em.current[0].a # angle coordinate [deg|rad]
em.current[0].r # radial coordinate [m|mm]
em.current[0].c # circumference coordinate [m|mm]

em.faces.If(lambda f: f.polarNormal * em.Normal(1, 0, 0) > 0.90).Sel()
em.CS("cs2").faces.If(lambda f: f.polarNormal * em.Normal(0, 1, 0) > 0.98).Sel()
em.CS().faces.If(lambda f: f.polarNormal * em.Normal(0, 1, 0) > 0.98).Sel()
	
em.faces.If(lambda f: 19.8 < f.r < 19.9).Sel()
em.CS("cs2").faces.If(lambda f: 19.8 < f.r < 19.9).Sel()
em.CS("cs2").faces.If(lambda f: 0 < f.a < 90).Sel()

em.CS("cs2")
n = em.current[0].polarNormal
em.faces.If(lambda f: f.polarNormal * n > 0.98).Sel()

em.bodies.color = 0xFF0000

em.CS("cs2")
n = em.current[0].polarNormal
em.bodies.IfName("gear2").faces.If(lambda f: f.polarNormal * n > 0.98).Sel()


em.bodies.IfName("ukb").color = 0xFF0000

# ----------------------------------------- #
#   NEW 23.4.2021:
# ----------------------------------------- #

#
#  invert:
#
em.solids.invert.faces.Sel()
em.shells.invert.faces.cyls.Sel()

#
#  stats:
#
v = em.bodies.Stats(lambda e: e.volume)
print "v.min   : " + str(v.min)
print "v.max   : " + str(v.max)
print "v.avg   : " + str(v.avg)
print "v.srss  : " + str(v.srss)
print "v.sum   : " + str(v.sum)
print "v.count : " + str(v.count)
print "v.mean  : " + str(v.mean)    # mean = avg
print "v.stdv  : " + str(v.stdv)    # standard deviation

#
#  stats:
#
x = em.bodies.Stats(lambda e: e.x)
print "x.min   : " + str(x.min)
print "x.max   : " + str(x.max)
print "x.avg   : " + str(x.avg)
print "x.srss  : " + str(x.srss)
print "x.sum   : " + str(x.sum)
print "x.count : " + str(x.count)
print "x.mean  : " + str(x.mean)    # mean = avg
print "x.stdv  : " + str(x.stdv)    # standard deviation 

#
#  parts:
#
print em.parts.count
print em.bodies.count
print em.faces.count
print em.edges.count
print em.verts.count

#
#  entity + entity ----> entities
#
b1 = em.bodies[0]
b2 = em.bodies[1]
bs = b1 + b2
bs.Sel() 

#
#  operators (bodies + body ...):
#
em.bodies - em.bodies[0]


#
#  extend by values:
# 
em.Clear()
for c in em.current: em.bodies.If(lambda x: x.volume == c.volume).Add()
#  or:
em.current.Extend(lambda c, x: c.volume == x.volume).Sel()
em.current.Extend(lambda c, x: c.volume == x.volume).showns.Sel()

#
#  shared (filter):
#
em = EntityManager.SEntityManager(ExtAPI)
em.faces.shareds.Sel()
em.edges.shareds.Sel()
em.verts.shareds.Sel()
em.lines.verts.shareds.Sel()  # shared only on line bodies


# ----------------------------------------- #
#   NEW 28.4.2021:
# ----------------------------------------- #

#
#  nodes:
#

em = EntityManager.SEntityManager(ExtAPI)
em.bodies.nodes.Sel()
em.nodes.Sel()

(em.nodes - em.bodies.IfName("vane").nodes).Sel()
(em.nodes * em.bodies.IfName("vane").nodes).Sel()

em.bodies.IfName("vane").faces.cyls.Sel() 
em.bodies.IfName("vane").faces.cyls.nodes.Sel()

a = em.bodies.IfName("vane").faces.cyls.nodes
b = em.faces.planes.nodes
(a + b).Sel()

# ----------------------------------------- #
#   TO-DO:								    #
# ----------------------------------------- #

em = EntityManager.SEntityManager(ExtAPI)
print em.faces[0].radius


print em.faces[0].axis             # cylinder axis vector ... uvidime, zda to nebude moc slozity

print em.bodies[0].principalAxis1  # uvidime, zda to nebude moc slozity
print em.bodies[0].principalAxis2  # uvidime, zda to nebude moc slozity
print em.bodies[0].principalAxis3  # uvidime, zda to nebude moc slozity




em.faces.cyls.convex.Sel()     # srouby
em.faces.cyls.concave.Sel()    # otvory










 
