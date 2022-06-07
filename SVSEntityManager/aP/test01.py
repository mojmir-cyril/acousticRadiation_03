


s = AnyParameter.SSelector(ExtAPI, Tree.FirstActiveObject)
s.bodies.Sel()
s.surfs.Sel()
s.surfs.faces.Sel()
s.surfs.faces.spheres.Sel()
s.solids.faces.spheres.Sel()
s.solids.faces.spheres.bodies.Sel()
s.faces.spheres.bodies.Sel()
s.edges.circles.Sel()
s.edges.circles.faces.Sel()
s.surfs.faces.Add()

(s.faces.spheres + s.faces.planes).Sel()