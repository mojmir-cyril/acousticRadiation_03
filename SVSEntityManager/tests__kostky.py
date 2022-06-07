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
nowWhole = Now()
#
em = EM()
#
Msg = em.logger.Msg
Err = em.logger.Err
#
Msg("")
Msg("============================================================")
Msg("   tests__kostky.py:                                        ")
Msg("============================================================")
Msg("  for: ")
Msg(r"   e:\C#Tools\SVSEntityManager\models\kostky_2021R2.wbpj")
Msg("")
Msg("  now : {}".format(Now()))
Msg("")
#
#  functions:
#
def Compare(id, i1, i2):  # id = CHECK-001-001
    if i1 != i2: Err(str(id) + " : failed ({} != {})".format(i1, i2))
    else:        Msg(str(id) + " : <b><font color=green>ok</font></b> ({} == {})".format(i1, i2))
#
def SelAndCount(o): 
    now = Now()
    o.Sel()
    dt = Now() - now
    if dt.seconds > 3: Msg(dt)
    return em.current.count
#
#  group 001:
#
Compare("CHECK-001-001", SelAndCount(em.bodies)            ,     33 )
Compare("CHECK-001-002", SelAndCount(em.surfs)             ,     14 )
Compare("CHECK-001-003", SelAndCount(em.lines)             ,      8 )
Compare("CHECK-001-004", SelAndCount(em.solids)            ,     11 )
Compare("CHECK-001-005", SelAndCount(em.faces)             ,    130 )
Compare("CHECK-001-006", SelAndCount(em.edges)             ,    315 )
Compare("CHECK-001-007", SelAndCount(em.verts)             ,    222 )
Compare("CHECK-001-008", SelAndCount(em.elems)             ,  55148 )  # 55148
Compare("CHECK-001-009", SelAndCount(em.nodes)             ,  95929 )   
Compare("CHECK-001-010", SelAndCount(em.elemFaces)         ,  12362 )  # 12362 = 9872 + 2490
#
#  group 002:
#
Compare("CHECK-002-001", em.surfs.faces.count                       ,     42 )
Compare("CHECK-002-002", em.surfs.edges.count                       ,    128 )
Compare("CHECK-002-003", em.lines.edges.count                       ,     12 )
Compare("CHECK-002-004", em.solids.faces.count                      ,     88 )
Compare("CHECK-002-005", em.solids.edges.count                      ,    175 )
Compare("CHECK-002-006", em.solids.faces.edges.count                ,    175 ) 
Compare("CHECK-002-007", em.surfs.elemFaces.count                   ,   2490 )   
Compare("CHECK-002-008", em.solids.elemFaces.count                  ,   9872 )  
Compare("CHECK-002-009", em.solids.nodes.elemFacesIn.count          , 219410 )  # nebo: 219409 (nikdo nevi:-)
Compare("CHECK-002-010", em.solids.faces.nodes.elemFacesIn.count    ,   9874 )  # spravne asi 9872 dle WorkSheet (asi problem v nejakym rohu)
Compare("CHECK-002-011", em.solids.faces.nodes.elemFaces.exts.count ,   9874 )  # spravne asi 9872 dle WorkSheet (asi problem v nejakym rohu)
Compare("CHECK-002-012", em.nodes.bodies.nodes.bodies.count         ,     33 )  
Compare("CHECK-002-013", em.elems.bodies.elems.bodies.count         ,     33 )  
Compare("CHECK-002-014", em.elemFaces.bodies.elemFaces.bodies.count ,     25 )  # 14 + 11
#
#  group 003:
# 
Compare("CHECK-003-001", em.edges.circles.count            ,     48 )
Compare("CHECK-003-002", em.edges.circles.faces.count      ,     51 )
Compare("CHECK-003-003", em.faces.count                    ,    130 )
Compare("CHECK-003-004", em.edges.count                    ,    315 )
Compare("CHECK-003-005", em.verts.count                    ,    222 )
Compare("CHECK-003-006", em.lines.edges.count              ,     12 )
Compare("CHECK-003-007", em.solids.edges.count             ,    175 )
Compare("CHECK-003-008", em.surfs.edges.count              ,    128 )
Compare("CHECK-003-009", em.verts.faces.count              ,    130 ) 
Compare("CHECK-003-010", em.edges.faces.count              ,    130 ) 
Compare("CHECK-003-011", em.bodies.faces.count             ,    130 ) 
Compare("CHECK-003-012", em.nodes.faces.count              ,    130 ) 
Compare("CHECK-003-013", em.elems.faces.count              ,    130 ) 
Compare("CHECK-003-014", em.elemFaces.faces.count          ,    130 ) 
#
#  group 004:
# 
Compare("CHECK-004-001", (em.lines.edges + em.solids.edges + em.surfs.edges).count   ,  315 )
Compare("CHECK-004-002", (em.edges - em.solids.edges).count                          ,  140 )
Compare("CHECK-004-003", (em.edges.circles * em.solids.edges).count                  ,   36 ) 
Compare("CHECK-004-004", (em.faces.planes + em.faces.cyls).count                     ,  122 ) 
Compare("CHECK-004-005", (em.faces.planes + em.faces.cyls * em.solids.faces).count   ,  116 ) # 97 + 19
Compare("CHECK-004-006", ((em.faces.planes + em.faces.cyls) * em.solids.faces).count ,   82 ) # 19 + 63
Compare("CHECK-004-007", (em.solids + em.surfs).faces.count                          ,  130 )  
#
#  group 005:
#
em.lengthUnit = "mm"
Compare("CHECK-005-001", em.AccurDigits(length = 0, angle = 1, mass = 1).bodies[0].x  , 5.0  ) # 551845.00
Compare("CHECK-005-002", em.AccurDigits(length = 2, angle = 1, mass = 1).bodies[0].x  , 5.07 ) # 551844.91
Compare("CHECK-005-003", em.AccurDigits(length = -2, angle = 1, mass = 1).bodies[0].x ,   0  ) # 551800.00
#
#  group 006:
#
Compare("CHECK-006-001", SelAndCount(em.AccurDigits(length = 0, angle = 1, mass = 1).bodies[0].Extend(lambda c, x: c.volume == x.volume)) ,  3      )
Compare("CHECK-006-002", max(em.AccurDigits(length = 10, angle = 10, mass = 3).bodies.masses)                                             ,  0.008  )
Compare("CHECK-006-003", max(em.AccurDigits(length = 10, angle = 10, mass = 4).bodies.masses)                                             ,  0.0079 )
em.AccurDigits()
#
#  group 007:
#
em.CS().lengthUnit = "mm"
Compare("CHECK-007-001", em.parts.count                        ,    4 )
Compare("CHECK-007-002", em.parts.If(lambda e: e.x > 0).count  ,    3 )
Compare("CHECK-007-003", em.bodies.If(lambda e: e.x > 0).count ,   22 )   
Compare("CHECK-007-004", em.faces.If(lambda e: e.x > 0).count  ,  116 )
Compare("CHECK-007-005", em.edges.If(lambda e: e.x > 0).count  ,  270 )
Compare("CHECK-007-006", em.verts.If(lambda e: e.x > 0).count  ,  180 )
#
#  group 008:
#
em.CS("local")
Compare("CHECK-008-001", em.parts.count                        ,    4 )
Compare("CHECK-008-002", em.parts.If(lambda e: e.x > 0).count  ,    2 )
Compare("CHECK-008-003", em.bodies.If(lambda e: e.x > 0).count ,   30 )
Compare("CHECK-008-004", em.faces.If(lambda e: e.x > 0).count  ,   99 )
Compare("CHECK-008-005", em.edges.If(lambda e: e.x > 0).count  ,  252 )
Compare("CHECK-008-006", em.verts.If(lambda e: e.x > 0).count  ,  183 )
em.CS()        
#
#  group 009:
#
Compare("CHECK-009-001", em.parts.IfName("pile").count  ,     0 )
Compare("CHECK-009-002", em.bodies.IfName("pile").count ,     0 )
Compare("CHECK-009-003", em.faces.IfName("pile").count  ,     0 )
Compare("CHECK-009-004", em.edges.IfName("pile").count  ,     0 )
Compare("CHECK-009-005", em.verts.IfName("pile").count  ,     0 )
#
#  group 010:
#
em.bodies.Unsuppress()
em.bodies.IfName("tet-tet").Suppress()
em.bodies.IfName("hex-hex").Show().HideOthers()
Compare("CHECK-010-001", em.bodies.actives.count      ,   31)
Compare("CHECK-010-002", em.bodies.suppresseds.count  ,    2)
Compare("CHECK-010-003", em.bodies.showns.count       ,    2)
Compare("CHECK-010-004", em.bodies.hiddens.count      ,   29)
Compare("CHECK-010-005", em.actives.count             ,   31)
Compare("CHECK-010-006", em.suppresseds.count         ,    2)
Compare("CHECK-010-007", em.showns.count              ,    2)
Compare("CHECK-010-008", em.hiddens.count             ,   29)
em.bodies.Unsuppress().Show()
#
#  group 011:
# 
a = em.faces                            # 11
b = em.solids.faces                     # 3
c = em.surfs.faces                      # 3
a.Sel(); b.Remove(); Compare("CHECK-011-001", em.current.count,  42)
a.Sel(); b.Filter(); Compare("CHECK-011-002", em.current.count,  88)
a.Sel(); c.Filter(); Compare("CHECK-011-003", em.current.count,  42)
b.Sel(); c.Add();    Compare("CHECK-011-004", em.current.count, 130) 
#
#  group 012:
# 
s = EM().bodies.Stats(lambda x: x.volume) 
def R(x): return round(x * 100.) / 100.
def X(x): return round(x / 100.) * 100.
Compare("CHECK-012-001", R(s.sum  )  ,  R(  10226.551     ))
Compare("CHECK-012-002", X(s.ssum )  ,  X(6729619.878     ))
Compare("CHECK-012-003", R(s.max  )  ,  R(   1000         ))
Compare("CHECK-012-004", R(s.avg  )  ,  R(    309.8954848 ))
Compare("CHECK-012-005", R(s.min  )  ,  R(     10         ))
Compare("CHECK-012-006", R(s.srss )  ,  R(   2594.15109   ))
Compare("CHECK-012-007", R(s.count)  ,  R(     33         ))
Compare("CHECK-012-008", R(s.mean )  ,  R(    309.8954848 ))
Compare("CHECK-012-009", R(s.stdv )  ,  R(    328.4701867 ))
Compare("CHECK-012-010", R(s.rms  )  ,  R(    451.5837409 )) 
Compare("CHECK-012-011", R(s.mode )  ,  R(    100.        )) 
#
#  write:
# 
em.logger.Show()
#
#
#
Msg("============================================================")
Msg("  nowWhole = Now() : " + str(Now() - nowWhole))
Msg("============================================================")
