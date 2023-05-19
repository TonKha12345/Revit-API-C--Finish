using AlphaBIM;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace CreatePipe
{
    /// <summary>
    /// Interaction logic for CreatePipeWindow.xaml
    /// </summary>
    public partial class CreatePipeWindow
    {
        #region Field
        private CreatePipeViewModel _viewModel;
        private TransactionGroup tranG;
        #endregion

        public CreatePipeWindow(CreatePipeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            tranG = new TransactionGroup(_viewModel.Doc, "trans");
        }

        private void btn_Ok(object sender, RoutedEventArgs e)
        {
            //Get Pipe segment and Size
            
            RoutingPreferenceRule rpr = _viewModel.SelectedPipeTypeMain.RoutingPreferenceManager.GetRule(RoutingPreferenceRuleGroupType.Segments, 0);
            Segment segmentPipe = _viewModel.Doc.GetElement(rpr.MEPPartId) as Segment;  
            List<MEPSize> sizes = segmentPipe.GetSizes().ToList();

            RoutingPreferenceRule rpr1 = _viewModel.SelectedPipeTypeMain.RoutingPreferenceManager.GetRule(RoutingPreferenceRuleGroupType.Elbows, 1);
            FamilyInstance elbows = _viewModel.Doc.GetElement(rpr1.MEPPartId) as FamilyInstance;

            List<Pipe> pipeBranch = new List<Pipe>();
            List<Connector> connectorsBranch = new List<Connector>();

            List<Pipe> pipeMain = new List<Pipe>();
            List<Connector> connectorsMain = new List<Connector>();

            ProgressWindow.Maximum = sizes.Count;
            double value = 0;

            tranG.Start();

            //Create Pipe from Layer
            foreach (MEPSize size in sizes)
            {
                if (tranG.HasStarted())
                {
                    //Display Progressbar
                    value += 1;
                    _viewModel.Percent = value / ProgressWindow.Maximum * 100;
                    ProgressWindow.Dispatcher?.Invoke(() => ProgressWindow.Value = value, DispatcherPriority.Background);

                    //Create Pipe
                    if (size.NominalDiameter < AlphaBIMUnitUtils.MmToFeet(_viewModel.MiddleDiameter))
                    {
                        string pipeName = string.Concat(_viewModel.NameStartWith, AlphaBIMUnitUtils.FeetToMm(size.NominalDiameter).ToString());
                        List<Line> allLinesinCad = _viewModel.GetLineHaveName(_viewModel.SelectedCadLink, pipeName);
                        foreach (Line line in allLinesinCad)
                        {
                            using (Transaction t = new Transaction(_viewModel.Doc, "t"))
                            {
                                t.Start();
                                Pipe p = Pipe.Create(_viewModel.Doc, _viewModel.SelectedSystemTypeBrand.Id, _viewModel.SelectedPipeTypeBrand.Id, _viewModel.SelectedLevel.Id, line.GetEndPoint(0), line.GetEndPoint(1));
                                p.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(AlphaBIMUnitUtils.MmToFeet(_viewModel.BrandPipeOffset));
                                p.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(size.NominalDiameter);
                                ConnectorSet conSet = p.ConnectorManager.Connectors;
                                foreach(Connector connector in conSet)
                                {
                                    connectorsBranch.Add(connector);
                                }
                                pipeBranch.Add(p);
                                t.Commit();
                            }
                        }
                    }
                    else
                    {
                        string pipeName = string.Concat(_viewModel.NameStartWith, AlphaBIMUnitUtils.FeetToMm(size.NominalDiameter).ToString());
                        List<Line> allLinesinCad = _viewModel.GetLineHaveName(_viewModel.SelectedCadLink, pipeName);
                        foreach (Line line in allLinesinCad)
                        {
                            using (Transaction t4 = new Transaction(_viewModel.Doc, "t"))
                            {
                                t4.Start();
                                Pipe p = Pipe.Create(_viewModel.Doc, _viewModel.SelectedSystemTypeMain.Id, _viewModel.SelectedPipeTypeMain.Id, _viewModel.SelectedLevel.Id, line.GetEndPoint(0), line.GetEndPoint(1));
                                p.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(AlphaBIMUnitUtils.MmToFeet(_viewModel.MainPipeOffset));
                                p.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(size.NominalDiameter);
                                ConnectorSet conSet = p.ConnectorManager.Connectors;
                                foreach (Connector connector in conSet)
                                {
                                    connectorsMain.Add(connector);
                                }
                                pipeMain.Add(p);
                                t4.Commit();
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            _viewModel.CreateUnionFitting(connectorsBranch);
            _viewModel.CreateUnionFitting(connectorsMain);
            ////Connect Branch Pipe and Main Pipe
            //foreach (Pipe pipe in pipeBranch)
            //{
            //    if (tranG.HasStarted())
            //    {
            //        _viewModel.ConnectSystem(pipe);
            //    }
            //}

            //Create Tee and Elbows Fitting
            List<Pipe> allPipeToCreateTee = new FilteredElementCollector(_viewModel.Doc).OfClass(typeof(Pipe)).Cast<Pipe>().ToList();
            _viewModel.CreateTeeAndElbowFitting(allPipeToCreateTee);

            if (tranG.HasStarted())
            {
                tranG.Assimilate();
                DialogResult = true;
            }
        }

        private void btn_Cancle(object sender, RoutedEventArgs e)
        {
            tranG.RollBack();
            DialogResult = false;
        }
    }
}
