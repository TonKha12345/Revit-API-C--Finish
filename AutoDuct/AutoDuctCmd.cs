using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GXYZ = Autodesk.Revit.DB.XYZ;
using AlphaBIM;
using Autodesk.Revit.UI.Selection;
using System.Xml;
using Autodesk.Revit.DB.Plumbing;

namespace AutoDuct
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDuctCmd : IExternalCommand
    {
        private UIDocument uidoc;
        private static Document doc;
        
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            List<Duct> ducts = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Duct)).Cast<Duct>().ToList();
            using (Transaction trans = new Transaction(doc,"Iexternal"))
            {
                trans.Start();
                foreach (Duct duct in ducts)
                {
                    SplitDuct(duct);
                }
                trans.Commit();
            }
            return Result.Succeeded;
        }




        /// <summary>
        /// information of a connector
        /// </summary>
        public class ConnectorInfo
        {
            /// <summary>
            /// The owner's element ID
            /// </summary>
            ElementId m_ownerId;

            /// <summary>
            /// The origin of the connector
            /// </summary>
            GXYZ m_origin;

            /// <summary>
            /// The Connector object
            /// </summary>
            Connector m_connector;

            /// <summary>
            /// The connector this object represents
            /// </summary>
            public Connector Connector
            {
                get { return m_connector; }
                set { m_connector = value; }
            }

            /// <summary>
            /// The owner ID of the connector
            /// </summary>
            public ElementId OwnerId
            {
                get { return m_ownerId; }
                set { m_ownerId = value; }
            }

            /// <summary>
            /// The origin of the connector
            /// </summary>
            public GXYZ Origin
            {
                get { return m_origin; }
                set { m_origin = value; }
            }

            /// <summary>
            /// The constructor that finds the connector with the owner ID and origin
            /// </summary>
            /// <param name="ownerId">the ownerID of the connector</param>
            /// <param name="origin">the origin of the connector</param>
            public ConnectorInfo(ElementId ownerId, GXYZ origin)
            {
                m_ownerId = ownerId;
                m_origin = origin;
                m_connector = ConnectorInfo.GetConnector(m_ownerId, origin);
            }

            /// <summary>
            /// The constructor that finds the connector with the owner ID and the values of the origin
            /// </summary>
            /// <param name="ownerId">the ownerID of the connector</param>
            /// <param name="x">the X value of the connector</param>
            /// <param name="y">the Y value of the connector</param>
            /// <param name="z">the Z value of the connector</param>
            public ConnectorInfo(ElementId ownerId, double x, double y, double z)
                : this(ownerId, new GXYZ(x, y, z))
            {
            }

            /// <summary>
            /// Get the connector of the owner at the specific origin
            /// </summary>
            /// <param name="ownerId">the owner ID of the connector</param>
            /// <param name="connectorOrigin">the origin of the connector</param>
            /// <returns>if found, return the connector, or else return null</returns>
            public static Connector GetConnector(ElementId ownerId, GXYZ connectorOrigin)
            {
                ConnectorSet connectors = GetConnectors(ownerId);
                foreach (Connector conn in connectors)
                {
                    if (conn.ConnectorType == ConnectorType.Logical)
                        continue;
                    if (conn.Origin.IsAlmostEqualTo(connectorOrigin))
                        return conn;
                }
                return null;
            }

            /// <summary>
            /// Get all the connectors of an element with a specific ID
            /// </summary>
            /// <param name="ownerId">the owner ID of the connector</param>
            /// <returns>the connector set which includes all the connectors found</returns>
            public static ConnectorSet GetConnectors(ElementId ownerId)
            {
                Element element = doc.GetElement(ownerId);
                return GetConnectors(element);
            }

            /// <summary>
            /// Get all the connectors of a specific element
            /// </summary>
            /// <param name="element">the owner of the connector</param>
            /// <returns>if found, return all the connectors found, or else return null</returns>
            public static ConnectorSet GetConnectors(Autodesk.Revit.DB.Element element)
            {
                if (element == null) return null;
                FamilyInstance fi = element as FamilyInstance;
                if (fi != null && fi.MEPModel != null)
                {
                    return fi.MEPModel.ConnectorManager.Connectors;
                }
                MEPSystem system = element as MEPSystem;
                if (system != null)
                {
                    return system.ConnectorManager.Connectors;
                }

                MEPCurve duct = element as MEPCurve;
                if (duct != null)
                {
                    return duct.ConnectorManager.Connectors;
                }
                return null;
            }

            /// <summary>
            /// Find the two connectors of the specific ConnectorManager at the two locations
            /// </summary>
            /// <param name="connMgr">The ConnectorManager of the connectors to be found</param>
            /// <param name="ptn1">the location of the first connector</param>
            /// <param name="ptn2">the location of the second connector</param>
            /// <returns>The two connectors found</returns>
            public static Connector[] FindConnectors(ConnectorManager connMgr, GXYZ pnt1, GXYZ pnt2)
            {
                Connector[] result = new Connector[2];
                ConnectorSet conns = connMgr.Connectors;
                foreach (Connector conn in conns)
                {
                    if (conn.Origin.IsAlmostEqualTo(pnt1))
                    {
                        result[0] = conn;
                    }
                    else if (conn.Origin.IsAlmostEqualTo(pnt2))
                    {
                        result[1] = conn;
                    }
                }
                return result;
            }

        };

        /// <summary>
        /// Split Duct void
        /// </summary>
        /// <param name="duct"></param>
        public void SplitDuct(Duct duct)
        {
            //Get level
            ElementId levelId = duct.LevelId;

            //Length 1 duct
            double length1Duct = AlphaBIMUnitUtils.MmToFeet(2000);

            //Get centerLine od duct
            Line ductLine = (duct.Location as LocationCurve).Curve as Line;

            //Get Length Duct
            double len = duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();

            if (len < length1Duct)
            {
                return;
            }

            //Get vector direction
            GXYZ startPoint = ductLine.GetEndPoint(0);
            GXYZ endPoint = ductLine.GetEndPoint(1);
            GXYZ dir = endPoint.Subtract(startPoint);

            // Get split point
            GXYZ dir1 = dir * (length1Duct / len);
            GXYZ newPoint = startPoint + dir1;

            //Find 2 connectors which pipe's two end connector connected to
            Connector startConn = FindConnectedTo(duct, startPoint);
            Connector endConn = FindConnectedTo(duct, endPoint);

            //Create new Duct
            Duct startDuct = null;
            if (startConn != null)
            {
                startDuct = Duct.Create(doc, duct.DuctType.Id, levelId, startConn, newPoint);
            }
            else
            {
                startDuct = Duct.Create(doc, duct.MEPSystem.GetTypeId(), duct.DuctType.Id, levelId, startPoint, newPoint);
            }
            CopyParameters(duct, startDuct);

            Duct endDuct = null;
            if (endConn != null)
            {
                if(newPoint.DistanceTo(endConn.Origin) > 0.5)
                {
                    endDuct = Duct.Create(doc, duct.MEPSystem.GetTypeId(), duct.DuctType.Id, levelId, newPoint, endConn.Origin);
                }
            }
            else
            {
                if(newPoint.DistanceTo(endPoint) > 0.5)
                {
                    endDuct = Duct.Create(doc, duct.MEPSystem.GetTypeId(), duct.DuctType.Id, levelId, newPoint, endPoint);
                }
            }
            if(endDuct != null) CopyParameters(duct, endDuct);

            //Create Union
            if(startDuct != null && endDuct != null)
            {
                Connector conn1 = ConnectorInfo.GetConnector(startDuct.Id, newPoint);
                Connector conn2 = ConnectorInfo.GetConnector(endDuct.Id, newPoint);
                conn1.ConnectTo(conn2);
                doc.Create.NewUnionFitting(conn1, conn2);
            }

            //Delete duct after resolve
            doc.Delete(duct.Id);
            
            if(endDuct != null)
            {
                double endDuctLength = endDuct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                if (endDuctLength > length1Duct)
                {
                    SplitDuct(endDuct);
                }
            }
        }

        /// <summary>
        /// Find connectors that connected to
        /// </summary>
        /// <param name="duct"></param>
        /// <param name="conXYZ"></param>
        /// <returns></returns>
        private Connector FindConnectedTo(Duct duct, Autodesk.Revit.DB.XYZ conXYZ)
        {
            Connector connItself = FindConnector(duct, conXYZ);
            ConnectorSet connSet = connItself.AllRefs;
            foreach (Connector conn in connSet)
            {
                if (conn.Owner.Id != duct.Id &&
                    conn.ConnectorType == ConnectorType.End)
                {
                    return conn;
                }
            }
            return null;
        }

        /// <summary>
        /// Find out a connector from pipe with a specified point.
        /// </summary>
        /// <param name="pipe">Pipe to find the connector</param>
        /// <param name="conXYZ">Specified point</param>
        /// <returns>Connector whose origin is conXYZ</returns>
        private Connector FindConnector(Duct duct, Autodesk.Revit.DB.XYZ conXYZ)
        {
            ConnectorSet conns = duct.ConnectorManager.Connectors;
            foreach (Connector conn in conns)
            {
                if (conn.Origin.IsAlmostEqualTo(conXYZ))
                {
                    return conn;
                }
            }
            return null;
        }

        /// <summary>
        /// Copy parameters from source pipe to target pipe.
        /// </summary>
        /// <param name="source">Coping source</param>
        /// <param name="target">Coping target</param>
        private void CopyParameters(Duct source, Duct target)
        {
            double Width = source.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
            target.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(Width);

            double Height = source.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
            target.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(Height);
        }
    }
}
