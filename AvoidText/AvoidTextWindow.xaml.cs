using AlphaBIM;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;

namespace AvoidText
{
    /// <summary>
    /// Interaction logic for AvoidTextWindow.xaml
    /// </summary>
    public partial class AvoidTextWindow
    {
        private AvoidTextViewModel _viewModel;
        private TransactionGroup tranG;
        
        public AvoidTextWindow(AvoidTextViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            tranG = new TransactionGroup(_viewModel.Doc, "Process");
        }

        private void btn_Ok(object sender, RoutedEventArgs e)
        {
            
            List<Pipe> allPipeToRun = new List<Pipe>();
            if (_viewModel.IsCurrentView)
            {
                FilteredElementCollector collector = new FilteredElementCollector(_viewModel.Doc, _viewModel.Doc.ActiveView.Id);

                ElementCategoryFilter pipes = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);

                ParameterValueProvider valueProvider = new ParameterValueProvider(_viewModel.SelectedParameter.Id);
                FilterStringEquals evaluator = new FilterStringEquals();
                string ruleString = _viewModel.Value;
                FilterStringRule rule = new FilterStringRule(valueProvider, evaluator, ruleString);

                ElementParameterFilter pFilter = new ElementParameterFilter(rule);

                //Logical And Filter
                LogicalAndFilter andFilter = new LogicalAndFilter(pipes, pFilter);

                allPipeToRun = collector.WherePasses(andFilter).WhereElementIsNotElementType().Cast<Pipe>().ToList();
                
            }
            else if (_viewModel.IsSelection)
            {
                FilteredElementCollector collector = new FilteredElementCollector(_viewModel.Doc, _viewModel.UiDoc.Selection.GetElementIds());

                ElementCategoryFilter pipes = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);

                ParameterValueProvider valueProvider = new ParameterValueProvider(_viewModel.SelectedParameter.Id);
                FilterStringEquals evaluator = new FilterStringEquals();
                string ruleString = _viewModel.Value;
                FilterStringRule rule = new FilterStringRule(valueProvider, evaluator, ruleString);

                ElementParameterFilter pFilter = new ElementParameterFilter(rule);

                //Logical And Filter
                LogicalAndFilter andFilter = new LogicalAndFilter(pipes, pFilter);

                allPipeToRun = collector.WherePasses(andFilter).WhereElementIsNotElementType().Cast<Pipe>().ToList();

            }
            else if (_viewModel.IsEntireProject)
            {
                FilteredElementCollector collector = new FilteredElementCollector(_viewModel.Doc);

                ElementCategoryFilter pipes = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);

                ParameterValueProvider valueProvider = new ParameterValueProvider(_viewModel.SelectedParameter.Id);
                FilterStringEquals evaluator = new FilterStringEquals();
                string ruleString = _viewModel.Value;
                FilterStringRule rule = new FilterStringRule(valueProvider, evaluator, ruleString);

                ElementParameterFilter pFilter = new ElementParameterFilter(rule);

                //Logical And Filter
                LogicalAndFilter andFilter = new LogicalAndFilter(pipes, pFilter);

                allPipeToRun = collector.WherePasses(andFilter).WhereElementIsNotElementType().Cast<Pipe>().ToList();
            }

            if (allPipeToRun.Count == 0) DialogResult = false;
            tranG.Start();
            foreach(Pipe pipe in allPipeToRun)
            {
                if (tranG.HasStarted())
                {
                    _viewModel.Resolve(pipe);
                }
                else
                {
                    break;
                }
            }
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
