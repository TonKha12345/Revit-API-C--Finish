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
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using AutocadToRevit.Lib;
using Lib;
using System.Windows.Threading;
using AlphaBIM;
using Line = Autodesk.Revit.DB.Line;

namespace AutocadToRevit.Lib
{
    /// <summary>
    /// Interaction logic for CreateColumn.xaml
    /// </summary>
    public partial class CreateColumn
    {
        private CreateColumnViewModel _viewModel;
        TransactionGroup tranG;
        public CreateColumn(CreateColumnViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            tranG = new TransactionGroup(_viewModel.Doc,"create");
        }

        private void btn_Ok(object sender, RoutedEventArgs e)
        {
            // Setting Progressbar
            //Lay ve maximum element cho thanh progressbar
            List<PlanarFace> hachToCreateColumn = CadUtils.GetHatchHaveName(_viewModel.SelectedCadLink, _viewModel.SelectedLayer);

            List<ColumnData> allColmnData = new List<ColumnData>();
            foreach (PlanarFace hatch in hachToCreateColumn)
            {
                ColumnData columnData = new ColumnData(hatch);
                allColmnData.Add(columnData);
            }
            ProgressWindow.Maximum = allColmnData.Count;
            
            tranG.Start();
            List<ElementId> newColumns = new List<ElementId>();
            double value = 0;
            foreach(ColumnData column in allColmnData)
            {
                if(tranG.HasStarted())
                {
                    value = value + 1;
                    _viewModel.Percent = value / ProgressWindow.Maximum * 100;
                    ProgressWindow.Dispatcher?.Invoke(() => ProgressWindow.Value = value,DispatcherPriority.Background);

                    FamilySymbol familysymbols = AlphaBIM.FamilyUtils.GetFamilySymbolColumn(_viewModel.SelectedFamilyColumn, column.CanhNgan, column.CanhDai, "b", "h");
                    if (familysymbols == null) continue;
                    using ( Transaction trans = new Transaction(_viewModel.Doc, "start create"))
                    {
                        trans.Start();

                        DeleteWarningSuper warningSuper = new DeleteWarningSuper();
                        FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                        failOpt.SetFailuresPreprocessor(warningSuper);
                        trans.SetFailureHandlingOptions(failOpt);

                        FamilyInstance instance = _viewModel.Doc.Create.NewFamilyInstance(column.TamCot, familysymbols, _viewModel.BaseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                        instance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM)
                            .Set(_viewModel.BaseLevel.Id);
                        instance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM)
                            .Set(_viewModel.TopLevel.Id);

                        instance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM)
                            .Set(AlphaBIMUnitUtils.MmToFeet(_viewModel.BaseOffset));

                        instance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)
                            .Set(AlphaBIMUnitUtils.MmToFeet(_viewModel.TopOffset));

                        Line axis = Line.CreateUnbound(column.TamCot, XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(_viewModel.Doc,
                            instance.Id, axis,
                            column.GocXoay);

                        newColumns.Add(instance.Id);
                        trans.Commit();
                    }
                }
                else
                {
                    break;
                }

            }
            if (tranG.HasStarted())
            {
                DialogResult = true;
                tranG.Assimilate();
                _viewModel.UiDoc.Selection.SetElementIds(newColumns);

            }
        }

        private void btn_Cancle(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            if (tranG.HasStarted())
            {
                tranG.RollBack();
            }
        }
    }
}
