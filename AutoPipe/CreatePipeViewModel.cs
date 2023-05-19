using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using AlphaBIM;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using AvoidText;


namespace CreatePipe
{
    public class CreatePipeViewModel: ViewModelBase
    {
        #region Field
        public UIDocument UiDoc;
        public Document Doc;
        private Detector m_detector;
        internal ImportInstance SelectedCadLink;
        private double minPipeLength = 0.1;
        #endregion

        #region Binding
        //Binding Main Pipe
        public List<PipeType> AllPipeType { get; set; } = new List<PipeType>();
        public PipeType SelectedPipeTypeMain { get; set; }
        public List<PipingSystemType> AllSystemType { get; set; } = new List<PipingSystemType>();
        public PipingSystemType SelectedSystemTypeMain { get; set; }
        public double MainPipeOffset { get; set; }

        //Binding Brand Pipe
        public PipeType SelectedPipeTypeBrand { get; set; }
        public PipingSystemType SelectedSystemTypeBrand { get; set; }
        public double BrandPipeOffset { get; set; }

        //Binding Setting
        public string NameStartWith { get; set; }
        public double MiddleDiameter { get; set; }
        public List<Level> AllLevel { get; set; } = new List<Level>();
        public Level SelectedLevel { get; set; }

        //Binding Setting
        public List<PipeSegment> AllLayers { get; set; } = new List<PipeSegment>();
        public PipeSegment SelectedLayer { get; set; }

        //Binding ProfressWindow
        private double _percent;
        public double Percent
        {
            get { return _percent; }
            set
            {
                _percent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public CreatePipeViewModel(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Doc = uiDoc.Document;
            m_detector = new Detector(Doc);

            //Get AllPipeType
            AllPipeType = new FilteredElementCollector(Doc).OfClass(typeof(PipeType)).Cast<PipeType>().ToList();
            SelectedPipeTypeMain = AllPipeType.FirstOrDefault();
            SelectedPipeTypeBrand= AllPipeType.FirstOrDefault();

            //Get Piping SystemType
            AllSystemType = new FilteredElementCollector(Doc).OfClass(typeof(PipingSystemType)).Cast<PipingSystemType>().ToList();
            SelectedSystemTypeMain  = AllSystemType.FirstOrDefault();
            SelectedSystemTypeBrand = AllSystemType.FirstOrDefault();

            //Get AllLevel
            AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            SelectedLevel = AllLevel.OrderBy(x => x.Elevation).FirstOrDefault();

            //Get ImportInstance
            Reference r = uiDoc.Selection.PickObject(ObjectType.Element, new ImportInstanceSelectionFilter());
            SelectedCadLink = Doc.GetElement(r) as ImportInstance;

            //Get Pipe Segment
            AllLayers = new FilteredElementCollector(Doc).OfClass(typeof(PipeSegment)).Cast<PipeSegment>().ToList();
            SelectedLayer = AllLayers.FirstOrDefault();

        }

        /// <summary>
        /// Get line and PoluLine of ImportInstance ( link Autocad )
        /// </summary>
        /// <param name="cadInstance"></param>
        /// <returns> list GeometryObject </returns>
        public List<GeometryObject> GetLinesAndPolyLine(ImportInstance cadInstance)
        {
            List<GeometryObject> allLines = new List<GeometryObject>();

            // Defaults to medium detail, no references and no view.
            Options option = new Options();
            option.IncludeNonVisibleObjects = true;
            option.ComputeReferences = true;
            GeometryElement geoElement = cadInstance.get_Geometry(option);

            foreach (GeometryObject geoObject in geoElement)
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInstance = geoObject as GeometryInstance;
                    GeometryElement geoElement2 = geoInstance.GetInstanceGeometry();
                    foreach (GeometryObject geoObject2 in geoElement2)
                    {
                        if (geoObject2 is Line || geoObject2 is PolyLine)
                        {
                            allLines.Add(geoObject2);
                        }
                    }
                }
            }
            return allLines;
        }

        /// <summary>
        /// Get Line and PolyLine Have Name Selected Layer
        /// </summary>
        /// <param name="cadInstance"></param>
        /// <param name="layerName"></param>
        /// <returns>List Lines</returns>
        public List<Line> GetLineHaveName(ImportInstance cadInstance,string layerName)
        {
            List<Line> Lines = new List<Line>();
            List<PolyLine> PolyLines = new List<PolyLine>();
            List<GeometryObject> allLine = GetLinesAndPolyLine(cadInstance);

            if (allLine.Count == 0) return Lines;

            foreach (GeometryObject geoOb in allLine)
            {
                if (geoOb is Line)
                {
                    GraphicsStyle graphicsStyle = cadInstance.Document.GetElement(geoOb.GraphicsStyleId) as GraphicsStyle;
                    if (graphicsStyle == null) continue;
                    Category styleCategory = graphicsStyle.GraphicsStyleCategory;
                    if (styleCategory.Name.Equals(layerName))
                    {
                        Lines.Add(geoOb as Line);
                    }
                }
                else
                {
                    PolyLine geoOb2 = geoOb as PolyLine;
                    GraphicsStyle graphicsStyle = cadInstance.Document.GetElement(geoOb.GraphicsStyleId) as GraphicsStyle;
                    if (graphicsStyle == null) continue;
                    Category styleCategory = graphicsStyle.GraphicsStyleCategory;
                    if (styleCategory.Name.Equals(layerName))
                    {
                        PolyLines.Add(geoOb2);
                    }
                }
            }

            foreach(PolyLine polyLine in PolyLines)
            {
                List<XYZ> points = new List<XYZ>();
                points = polyLine.GetCoordinates().ToList();
                for (int i = 0; i < points.Count - 1; i++)
                {
                    using (Transaction t = new Transaction(Doc, "t"))
                    {
                        t.Start();
                        Line line = Line.CreateBound(points[i], points[i + 1]);
                        Lines.Add(line);
                        t.Commit();
                    }
                }
            }
            return Lines;
        }

        /// <summary>
        ///     Are the two given pipes parallel?
        /// </summary>
        public bool IsPipeParallel(Pipe p1, Pipe p2)
        {
            Line c1 = GetCurve(p1) as Line;
            Line c2 = GetCurve(p2) as Line;
            return Math.Sin(c1.Direction.AngleTo(
                c2.Direction)) < 0.01;

        }

        /// <summary>
        /// Are the two given pipes perpendicular?
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool IsPerpendicular( Pipe p1,  Pipe p2)
        {
            Line c1 = GetCurve(p1) as Line;
            Line c2 = GetCurve(p2) as Line;
            return Math.Cos(c1.Direction.AngleTo(
                c2.Direction)) < 0.01;
        }

        /// <summary>
        ///     Return the curve from a Revit database Element
        ///     location curve, if it has one.
        /// </summary>
        public Curve GetCurve(Element e)
        {
            Debug.Assert(null != e.Location,
                "expected an element with a valid Location");

            var lc = e.Location as LocationCurve;

            Debug.Assert(null != lc,
                "expected an element with a valid LocationCurve");

            return lc.Curve;
        }

        /// <summary>
        /// Create Union Fitting
        /// </summary>
        /// <param name="connectors"></param>
        public void CreateUnionFitting(List<Connector> connectors)
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                if (!connectors[i].IsConnected)
                {
                    XYZ conn = connectors[i].Origin;
                    for (int j = i + 1; j < connectors.Count - 1; j++)
                    {
                        if ((connectors[j].Origin.DistanceTo(conn)) < 0.01 && !(connectors[j].IsConnected))
                        {
                            using (Transaction t3 = new Transaction(Doc, "t"))
                            {
                                t3.Start();
                                connectors[i].ConnectTo(connectors[j]);
                                Pipe p1 = connectors[i].Owner as Pipe;
                                Pipe p2 = connectors[j].Owner as Pipe;
                                if (IsPipeParallel(p1, p2))
                                {
                                    Doc.Create.NewUnionFitting(connectors[i], connectors[j]);
                                    connectors.RemoveAt(j);
                                }
                                t3.Commit();
                            }
                        }          
                    }
                }
            }
        }

        /// <summary>
        /// Create Tee and Elbows Fitting
        /// </summary>
        /// <param name="pipes"></param>
        public void CreateTeeAndElbowFitting(List<Pipe> pipes)
        {
            List<Pipe> pipeDelete = new List<Pipe>();
            for (int i = 0; i < pipes.Count; i++)
            {
                // Get the centerline of pipe.
                Line pipeLine = (pipes[i].Location as LocationCurve).Curve as Line;

                // Calculate the intersection references with pipe's centerline.
                List<ReferenceWithContext> obstructionRefArr = m_detector.Obstructions(pipeLine);

                // Filter out the references, just allow Pipe.
                Filter(pipes[i], obstructionRefArr);

                if (obstructionRefArr.Count == 0)
                {
                    // There are no intersection found.
                    continue;
                }

                //Get pipe intersection first
                Reference ref1 = obstructionRefArr.FirstOrDefault().GetReference();
                Pipe pipe2 = Doc.GetElement(ref1) as Pipe;
                Line pipeLine2 = (pipe2.Location as LocationCurve).Curve as Line;

                XYZ closestPoint = ref1.GlobalPoint;
                using (Transaction t = new Transaction(Doc, "t"))
                {
                    t.Start();
                    if (Math.Sin(pipeLine.Direction.AngleTo(XYZ.BasisX)) < 0.01 && IsPerpendicular(pipes[i], pipe2) == true)
                    {
                        // Find two connectors which pipe's two ends connector connected to. 
                        Connector startConn = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(0));
                        Connector endConn = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(1));

                        Pipe startPipe = null;
                        if (null != startConn)
                        {
                            XYZ point1 = startConn.Origin;
                            XYZ point2 = new XYZ(startConn.Origin.X, closestPoint.Y, startConn.Origin.Z);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's start connector and pipe's start section.
                                startPipe = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, startConn, point2);
                            }
                        }
                        else
                        {
                            XYZ point1 = new XYZ(pipeLine2.GetEndPoint(0).X, closestPoint.Y, pipeLine2.GetEndPoint(0).Z);
                            XYZ point2 = pipeLine2.GetEndPoint(0);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's start point and pipe's start section.
                                startPipe = Pipe.Create(Doc, pipe2.MEPSystem.GetTypeId(), pipe2.PipeType.Id, SelectedLevel.Id, point1, point2);
                            }
                        }

                        // Copy parameters from pipe to startPipe. 
                        if(startPipe != null) CopyParameters(pipe2, startPipe);

                        Pipe endPipe = null;
                        if (null != endConn)
                        {
                            XYZ point1 = endConn.Origin;
                            XYZ point2 = new XYZ(endConn.Origin.X, closestPoint.Y, endConn.Origin.Z);
                            if (point1.DistanceTo(point2) > minPipeLength)
                            {
                                // Create a pipe between pipe's end connector and pipe's end section.
                                endPipe = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, endConn, point2);
                            }
                        }
                        else
                        {
                            XYZ point1 = new XYZ(pipeLine2.GetEndPoint(1).X, closestPoint.Y, pipeLine2.GetEndPoint(1).Z);
                            XYZ point2 = pipeLine2.GetEndPoint(1);
                            if (point1.DistanceTo(point2) > minPipeLength)
                            {
                                // Create a pipe between pipe's end point and pipe's end section.
                                endPipe = Pipe.Create(Doc, pipe2.MEPSystem.GetTypeId(), pipe2.PipeType.Id, SelectedLevel.Id, point1, point2);
                            }     
                        }

                        // Copy parameters from pipe to endPipe.
                        if (endPipe != null) CopyParameters(pipe2, endPipe);

                        //Create Fitting
                        if(startPipe != null && endPipe == null)
                        {
                            XYZ xyzConn1 = GetConnectorClosestTo(startPipe, closestPoint).Origin;
                            XYZ xyzConn2 = GetConnectorClosestTo(pipes[i], closestPoint).Origin;
                            if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                            {
                                GetConnectorClosestTo(startPipe, closestPoint).ConnectTo(GetConnectorClosestTo(pipes[i], closestPoint));
                                Doc.Create.NewElbowFitting(GetConnectorClosestTo(startPipe, closestPoint), GetConnectorClosestTo(pipes[i], closestPoint));
                                pipeDelete.Add(pipe2);
                            }
                        }
                        else if (startPipe == null && endPipe != null)
                        {
                            XYZ xyzConn1 = GetConnectorClosestTo(endPipe, closestPoint).Origin;
                            XYZ xyzConn2 = GetConnectorClosestTo(pipes[i], closestPoint).Origin;
                            if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                            {
                                GetConnectorClosestTo(endPipe, closestPoint).ConnectTo(GetConnectorClosestTo(pipes[i], closestPoint));
                                Doc.Create.NewElbowFitting(GetConnectorClosestTo(endPipe, closestPoint), GetConnectorClosestTo(pipes[i], closestPoint));
                                pipeDelete.Add(pipe2);
                            }
                        }
                        else if(startPipe != null && endPipe != null)
                        {
                            Connector conn1 = GetConnectorClosestTo(pipes[i], closestPoint);
                            Connector conn2 = GetConnectorClosestTo(startPipe, closestPoint);
                            Connector conn3 = GetConnectorClosestTo(endPipe, closestPoint);

                            conn2.ConnectTo(conn3);
                            conn1.ConnectTo(conn3);
                            conn1.ConnectTo(conn2);

                            Doc.Create.NewTeeFitting(conn2, conn3, conn1);
                            pipeDelete.Add(pipe2);
                        }
                    }

                    else if (Math.Sin(pipeLine.Direction.AngleTo(XYZ.BasisY)) < 0.01 && IsPerpendicular(pipes[i], pipe2) == true)
                    {
                        // Find two connectors which pipe's two ends connector connected to. 
                        Connector startConn = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(0));
                        Connector endConn = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(1));

                        Pipe startPipe = null;
                        if (null != startConn)
                        {
                            XYZ point1 = startConn.Origin;
                            XYZ point2 = new XYZ(closestPoint.X, startConn.Origin.Y, startConn.Origin.Z);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's start connector and pipe's start section.
                                startPipe = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, startConn, point2);
                            }
                        }
                        else
                        {
                            XYZ point1 = new XYZ(closestPoint.X, pipeLine2.GetEndPoint(0).Y, pipeLine2.GetEndPoint(0).Z);
                            XYZ point2 = pipeLine2.GetEndPoint(0);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's start point and pipe's start section.
                                startPipe = Pipe.Create(Doc, pipe2.MEPSystem.GetTypeId(), pipe2.PipeType.Id, SelectedLevel.Id, point1, point2);
                            }  
                        }

                        // Copy parameters from pipe to startPipe. 
                        if(startPipe != null) CopyParameters(pipe2, startPipe); 

                        Pipe endPipe = null;
                        if (null != endConn)
                        {
                            XYZ point1 = endConn.Origin;
                            XYZ point2 = new XYZ(closestPoint.X, endConn.Origin.Y, endConn.Origin.Z);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's end connector and pipe's end section.
                                endPipe = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, endConn, point2);
                            }
                        }
                        else
                        {
                            XYZ point1 = new XYZ(closestPoint.X, pipeLine2.GetEndPoint(1).Y, pipeLine2.GetEndPoint(1).Z);
                            XYZ point2 = pipeLine2.GetEndPoint(1);
                            if (point1.DistanceTo(point2) > pipe2.Diameter)
                            {
                                // Create a pipe between pipe's end point and pipe's end section.
                                endPipe = Pipe.Create(Doc, pipe2.MEPSystem.GetTypeId(), pipe2.PipeType.Id, SelectedLevel.Id, point1, point2);
                            }
                        }

                        // Copy parameters from pipe to endPipe.
                        if (endPipe != null)  CopyParameters(pipe2, endPipe);

                        //Create Fitting
                        if (startPipe != null && endPipe == null)
                        {
                            XYZ xyzConn1 = GetConnectorClosestTo(startPipe, closestPoint).Origin;
                            XYZ xyzConn2 = GetConnectorClosestTo(pipes[i], closestPoint).Origin;
                            if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                            {
                                GetConnectorClosestTo(startPipe, closestPoint).ConnectTo(GetConnectorClosestTo(pipes[i], closestPoint));
                                Doc.Create.NewElbowFitting(GetConnectorClosestTo(startPipe, closestPoint), GetConnectorClosestTo(pipes[i], closestPoint));
                                pipeDelete.Add(pipe2);
                            }
                        }
                        else if (startPipe == null && endPipe != null)
                        {
                            XYZ xyzConn1 = GetConnectorClosestTo(endPipe, closestPoint).Origin;
                            XYZ xyzConn2 = GetConnectorClosestTo(pipes[i], closestPoint).Origin;
                            if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                            {
                                GetConnectorClosestTo(endPipe, closestPoint).ConnectTo(GetConnectorClosestTo(pipes[i], closestPoint));
                                Doc.Create.NewElbowFitting(GetConnectorClosestTo(endPipe, closestPoint), GetConnectorClosestTo(pipes[i], closestPoint));
                                pipeDelete.Add(pipe2);
                            }
                        }
                        else if (startPipe != null && endPipe != null)
                        {
                            Connector conn1 = GetConnectorClosestTo(pipes[i], closestPoint);
                            Connector conn2 = GetConnectorClosestTo(startPipe, closestPoint);
                            Connector conn3 = GetConnectorClosestTo(endPipe, closestPoint);

                            conn2.ConnectTo(conn3);
                            conn1.ConnectTo(conn3);
                            conn1.ConnectTo(conn2);

                            Doc.Create.NewTeeFitting(conn2, conn3, conn1);
                            pipeDelete.Add(pipe2);
                        }
                    }
                    t.Commit();
                }
            }
            foreach (Pipe pipe in pipeDelete)
            {
                using(Transaction T1 = new Transaction(Doc, "Delete"))
                {
                    T1.Start();
                    Doc.Delete(pipe.Id);
                    T1.Commit();
                }
            }
        }
        /// <summary>
        /// Connect Branch Pipe with Pain Pipe
        /// </summary>
        /// <param name="pipe"></param>
        public void ConnectSystem(Pipe pipe)
        {
            using (Transaction TT = new Transaction(Doc, "connect System"))
            {
                TT.Start();
                FailureHandlingOptions option = TT.GetFailureHandlingOptions();
                option.SetFailuresPreprocessor(new DeleteWarningSuper());
                TT.SetFailureHandlingOptions(option);

                // Length between Branch and Main Pipe
                double Length = AlphaBIMUnitUtils.MmToFeet(BrandPipeOffset) - AlphaBIMUnitUtils.MmToFeet(MainPipeOffset);

                Line pipeLine = (pipe.Location as LocationCurve).Curve as Line;

                XYZ point1 = new XYZ(pipeLine.GetEndPoint(0).X, pipeLine.GetEndPoint(0).Y, pipeLine.GetEndPoint(0).Z - Length);
                XYZ point2 = new XYZ(pipeLine.GetEndPoint(1).X, pipeLine.GetEndPoint(1).Y, pipeLine.GetEndPoint(1).Z - Length);
                Line line = Line.CreateBound(point1, point2);

                List<ReferenceWithContext> refs = m_detector.Obstructions(line);
                Filter(pipe, refs);
                if (refs.Count == 0)
                {
                    return;
                }
                Reference cur = refs.FirstOrDefault().GetReference();
                Pipe pipe2 = Doc.GetElement(cur) as Pipe;
                Line pipeLine2 = (pipe2.Location as LocationCurve).Curve as Line;

                IList<ClosestPointsPairBetweenTwoCurves> iter = new List<ClosestPointsPairBetweenTwoCurves>();
                pipeLine.ComputeClosestPoints(pipeLine2, true, false, false, out iter);
                XYZ closestPoint1 = iter.FirstOrDefault().XYZPointOnFirstCurve;
                XYZ closestPoint2 = iter.FirstOrDefault().XYZPointOnSecondCurve;
                Pipe pipeConnect = Pipe.Create(Doc, SelectedSystemTypeBrand.Id, pipe.PipeType.Id, SelectedLevel.Id, closestPoint1, closestPoint2);
                CopyParameters(pipe, pipeConnect);

                //Resolve pipeBranch
                //Find Connectors 
                Connector startConn = FindConnectedTo(pipe, pipeLine.GetEndPoint(0));
                Connector endConn = FindConnectedTo(pipe, pipeLine.GetEndPoint(1));

                Pipe startPipe = null;
                if (null != startConn)
                {
                    XYZ p1 = startConn.Origin;
                    XYZ p2 = closestPoint1;
                    if (p1.DistanceTo(p2) > pipe.Diameter)
                    {
                        startPipe = Pipe.Create(Doc, pipe.PipeType.Id, SelectedLevel.Id, startConn, p2);
                    }
                }
                else
                {
                    XYZ p1 = closestPoint1;
                    XYZ p2 = pipeLine.GetEndPoint(0);
                    if (p1.DistanceTo(p2) > pipe.Diameter)
                    {
                        startPipe = Pipe.Create(Doc, SelectedSystemTypeBrand.Id, pipe.PipeType.Id, SelectedLevel.Id, p1, p2);
                    }
                }

                // Copy parameters from pipe to startPipe. 
                if (startPipe != null) CopyParameters(pipe, startPipe);

                Pipe endPipe = null;
                if (null != endConn)
                {
                    XYZ p1 = endConn.Origin;
                    XYZ p2 = closestPoint1;
                    if (p1.DistanceTo(p2) > pipe.Diameter)
                    {
                        endPipe = Pipe.Create(Doc, pipe.PipeType.Id, SelectedLevel.Id, endConn, p2);
                    }
                }
                else
                {
                    XYZ p1 = closestPoint1;
                    XYZ p2 = pipeLine.GetEndPoint(1);
                    if (p1.DistanceTo(p2) > pipe.Diameter)
                    {
                        endPipe = Pipe.Create(Doc, SelectedSystemTypeBrand.Id, pipe.PipeType.Id, SelectedLevel.Id, p1, p2);
                    }
                }

                // Copy parameters from pipe to endPipe.
                if (endPipe != null) CopyParameters(pipe, endPipe);

                //Create Elbow if not create Tee

                if (startPipe != null && endPipe == null)
                {
                    XYZ xyzConn1 = GetConnectorClosestTo(pipeConnect, closestPoint1).Origin;
                    XYZ xyzConn2 = GetConnectorClosestTo(startPipe, closestPoint1).Origin;
                    if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                    {
                        GetConnectorClosestTo(startPipe, closestPoint1).ConnectTo(GetConnectorClosestTo(pipeConnect, closestPoint1));
                        Doc.Create.NewElbowFitting(GetConnectorClosestTo(startPipe, closestPoint1), GetConnectorClosestTo(pipeConnect, closestPoint1));
                        Doc.Delete(pipe.Id);
                    }
                }
                else if(endPipe != null && startPipe == null)
                {
                    XYZ xyzConn1 = GetConnectorClosestTo(endPipe, closestPoint1).Origin;
                    XYZ xyzConn2 = GetConnectorClosestTo(pipeConnect, closestPoint1).Origin;
                    if (xyzConn1.DistanceTo(xyzConn2) < 0.01)
                    {
                        GetConnectorClosestTo(endPipe, closestPoint1).ConnectTo(GetConnectorClosestTo(pipeConnect, closestPoint1));
                        Doc.Create.NewElbowFitting(GetConnectorClosestTo(endPipe, closestPoint1), GetConnectorClosestTo(pipeConnect, closestPoint1));
                        Doc.Delete(pipe.Id);
                    }
                }
                else if (startPipe != null && endPipe != null)
                {
                    Connector conn1 = GetConnectorClosestTo(startPipe, closestPoint1);
                    Connector conn2 = GetConnectorClosestTo(endPipe, closestPoint1);
                    Connector conn3 = GetConnectorClosestTo(pipeConnect, closestPoint1);

                    conn1.ConnectTo(conn2);
                    conn2.ConnectTo(conn3);
                    conn1.ConnectTo(conn3);

                    Doc.Create.NewTeeFitting(conn1, conn2, conn3);
                    Doc.Delete(pipe.Id);
                }

                //Resolve pipeMain
                //Find Connectors 
                Connector startConn2 = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(0));
                Connector endConn2 = FindConnectedTo(pipe2, pipeLine2.GetEndPoint(1));

                Pipe startPipe2 = null;
                if (null != startConn2)
                {
                    XYZ p3 = startConn2.Origin;
                    XYZ p4 = closestPoint2;
                    if (p3.DistanceTo(p4) > pipe2.Diameter)
                    {
                        startPipe2 = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, startConn2, p4);
                    }
                }
                else
                {
                    XYZ p3 = closestPoint2;
                    XYZ p4 = pipeLine2.GetEndPoint(0);
                    if (p3.DistanceTo(p4) > pipe2.Diameter)
                    {
                        startPipe2 = Pipe.Create(Doc, SelectedSystemTypeMain.Id, pipe2.PipeType.Id, SelectedLevel.Id, p3, p4);
                    }
                }

                // Copy parameters from pipe to startPipe. 
                if (startPipe2 != null) CopyParameters(pipe2, startPipe2);

                Pipe endPipe2 = null;
                if (null != endConn2)
                {
                    XYZ p3 = endConn2.Origin;
                    XYZ p4 = closestPoint2;
                    if (p3.DistanceTo(p4) > pipe2.Diameter)
                    {
                        endPipe2 = Pipe.Create(Doc, pipe2.PipeType.Id, SelectedLevel.Id, endConn2, p4);
                    }
                }
                else
                {
                    XYZ p3 = closestPoint2;
                    XYZ p4 = pipeLine2.GetEndPoint(1);
                    if (p3.DistanceTo(p4) > pipe2.Diameter)
                    {
                        endPipe2 = Pipe.Create(Doc, SelectedSystemTypeMain.Id, pipe2.PipeType.Id, SelectedLevel.Id, p3, p4);
                    }
                }

                // Copy parameters from pipe to endPipe.
                if (endPipe2 != null) CopyParameters(pipe2, endPipe2);

                if (startPipe2 != null && endPipe2 == null)
                {
                    XYZ xyzConn3 = GetConnectorClosestTo(startPipe2, closestPoint2).Origin;
                    XYZ xyzConn4 = GetConnectorClosestTo(pipeConnect, closestPoint2).Origin;
                    if (xyzConn3.DistanceTo(xyzConn4) < 0.01)
                    {
                        GetConnectorClosestTo(startPipe2, closestPoint2).ConnectTo(GetConnectorClosestTo(pipeConnect, closestPoint2));
                        Doc.Create.NewElbowFitting(GetConnectorClosestTo(startPipe2, closestPoint2), GetConnectorClosestTo(pipeConnect, closestPoint2));
                        Doc.Delete(pipe2.Id);
                    }
                }
                else if(endPipe2 != null && startPipe2 == null)
                {
                    XYZ xyzConn3 = GetConnectorClosestTo(endPipe2, closestPoint2).Origin;
                    XYZ xyzConn4 = GetConnectorClosestTo(pipeConnect, closestPoint2).Origin;
                    if (xyzConn3.DistanceTo(xyzConn4) < 0.01)
                    {
                        GetConnectorClosestTo(endPipe2, closestPoint2).ConnectTo(GetConnectorClosestTo(pipeConnect, closestPoint2));
                        Doc.Create.NewElbowFitting(GetConnectorClosestTo(endPipe2, closestPoint2), GetConnectorClosestTo(pipeConnect, closestPoint2));
                        Doc.Delete(pipe2.Id);
                    }
                }
                else if (startPipe2 != null && endPipe2 != null)
                {
                    Connector conn4 = GetConnectorClosestTo(startPipe2, closestPoint2);
                    Connector conn5 = GetConnectorClosestTo(endPipe2, closestPoint2);
                    Connector conn6 = GetConnectorClosestTo(pipeConnect, closestPoint2);

                    conn4.ConnectTo(conn5);
                    conn4.ConnectTo(conn6);
                    conn5.ConnectTo(conn6);

                    Doc.Create.NewTeeFitting(conn4, conn5, conn6);
                    Doc.Delete(pipe2.Id);
                }
                TT.Commit();
            }
        }

        /// <summary>
        /// Filter reference of Pipe from obstructions
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="refs"></param>
        private void Filter(Pipe pipe, List<ReferenceWithContext> refs)
        {
            for (int i = refs.Count - 1; i >= 0; i--)
            {
                Reference cur = refs[i].GetReference();
                Element curElem = Doc.GetElement(cur);
                if (curElem.Id == pipe.Id || !(curElem is Pipe))
                {
                    refs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///     Return the connector on the element
        ///     closest to the given point.
        /// </summary>
        public static Connector GetConnectorClosestTo(Pipe e,XYZ p)
        {
            var cm = GetConnectorManager(e);

            return null == cm
                ? null
                : GetConnectorClosestTo(cm.Connectors, p);
        }

        /// <summary>
        ///     Return the connector set element
        ///     closest to the given point.
        /// </summary>
        private static Connector GetConnectorClosestTo(ConnectorSet connectors,XYZ p)
        {
            Connector targetConnector = null;
            var minDist = double.MaxValue;

            foreach (Connector c in connectors)
            {
                var d = c.Origin.DistanceTo(p);

                if (d < minDist)
                {
                    targetConnector = c;
                    minDist = d;
                }
            }

            return targetConnector;
        }

        /// <summary>
        ///     Return the given element's connector manager,
        ///     using either the family instance MEPModel or
        ///     directly from the MEPCurve connector manager
        ///     for ducts and pipes.
        /// </summary>
        private static ConnectorManager GetConnectorManager(Pipe e)
        {
            return e.ConnectorManager;
        }

        /// <summary>
        /// Find connectors that connected to
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="conXYZ"></param>
        /// <returns></returns>
        private Connector FindConnectedTo(Pipe pipe, Autodesk.Revit.DB.XYZ conXYZ)
        {
            Connector connItself = FindConnector(pipe, conXYZ);
            ConnectorSet connSet = connItself.AllRefs;
            foreach (Connector conn in connSet)
            {
                if (conn.Owner.Id != pipe.Id &&
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
        private Connector FindConnector(Pipe pipe, Autodesk.Revit.DB.XYZ conXYZ)
        {
            ConnectorSet conns = pipe.ConnectorManager.Connectors;
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
        private void CopyParameters(Pipe source, Pipe target)
        {
            double diameter = source.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
            target.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diameter);
        }
    }
}
