"""ACT Units Module

This module provides helper functions to convert and manipulate units.

"""

import System
import clr

clr.AddReference("Ans.Core")

import Ansys.Core.Units


def ConvertUnit(value, fromUnit, toUnit, quantityName=None):
    """Return the given value converted to the given units.

    Keyword arguments:
    value -- the value to convert
    fromUnit -- the unit of the given value
    toUnit -- the target unit
    quantityName -- the quantity name (default None)

    """
    return Ansys.Core.Units.UnitsManager.ConvertUnit(value, fromUnit, toUnit, quantityName)


def ConvertUserAngleUnitToDegrees(api, value):
    """Return the value converted from the current angle unit to degrees.

    Keyword arguments:
    api -- Extension API (ExtAPI)
    value -- the angle value to convert

    """
    user = api.DataModel.CurrentUnitFromQuantityName("Angle")
    return ConvertUnit(value, user, "degree", "Angle")


def ConvertToSolverConsistentUnit(api, value, quantityName, analysis):
    """Return the value converted to solver consistent unit of the given quantity.
    The value is in user unit of the given quantity.

    Keyword arguments:
    api -- Extension API (ExtAPI)
    value -- the value to convert, in user unit.
    quantityName -- name of the quantity
    analysis -- current analysis object

    """
    userUnit = api.DataModel.CurrentUnitFromQuantityName(quantityName)
    solverUnit = analysis.CurrentConsistentUnitFromQuantityName(quantityName)
    return ConvertUnit(value, userUnit, solverUnit, quantityName)


def ConvertToUserUnit(api, value, fromUnit, quantityName):
    """Return the value converted to user unit of the given quantity.

    Keyword arguments:
    api -- Extension API (ExtAPI)
    value -- the value to convert
    fromUnit -- the unit of the given value
    quantityName -- name of the quantity

    """
    userUnit = api.DataModel.CurrentUnitFromQuantityName(quantityName)
    return ConvertUnit(value, fromUnit, userUnit, quantityName)


def ConvertUnitToSolverConsistentUnit(api, value, fromUnit, quantityName, analysis):
    """Return the value converted to solver consistent unit of the given quantity.

    Keyword arguments:
    api -- Extension API (ExtAPI)
    value -- the value to convert
    fromUnit -- the unit of the given value
    quantityName -- name of the quantity
    analysis -- current analysis object
    """
    solverUnit = analysis.CurrentConsistentUnitFromQuantityName(quantityName)
    return ConvertUnit(value, fromUnit, solverUnit, quantityName)


def GetMeshToUserConversionFactor(api):
    """Return the length factor to convert mesh values to user values.

    Keyword arguments:
    api -- Extension API (ExtAPI)
    """
    meshUnit = api.DataModel.MeshDataByName("Global").Unit
    userUnit = api.DataModel.CurrentUnitFromQuantityName("Length")
    return ConvertUnit(1, meshUnit, userUnit)


def MeshToUserConversionFactor(api):
    """Obsolete method, use 'GetMeshToUserConversionFactor' instead."""
    api.Log.WriteWarning(
        "Obsolete method 'MeshToUserConversionFactor' in 'units' module: use 'GetMeshToUserConversionFactor' instead.")
    return GetMeshToUserConversionFactor(api)


def GetCurrentCompactUnitString(quantityName):
    """Return a compact unit string for the supplied field.
    for example, on an acceleration object acc (in MKS & NMM, respectively):
    -  acc.Magnitude.Output.Unit ==> 'm sec^-1 sec^-1' , 'm sec^-1 sec^-1'
    -  GetCurrentCompactUnitString(ExtAPI, acc.Magnitude.Output.QuantityName) ==> 'm/s**2' , 'mm/s**2'

    Keyword arguments:
    api -- Extension API (ExtAPI)
    quantityName -- "Acceleration"
    """

    import Ansys.ACT.Mechanical
    return Ansys.ACT.Mechanical.UnitsHelper.GetCurrentCompactUnitString(quantityName)

