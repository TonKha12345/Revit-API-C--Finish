using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace CreatePipe
{
    [Transaction(TransactionMode.Manual)]
    public class CreatePipeCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (TransactionGroup tranGR = new TransactionGroup(doc, "AutoPipeCmd"))
            {
                tranGR.Start();
                CreatePipeViewModel viewModel = new CreatePipeViewModel(uidoc);
                CreatePipeWindow window = new CreatePipeWindow(viewModel);
                bool? showDialog = window.ShowDialog();
                if (showDialog == null || showDialog == false)
                {
                    tranGR.RollBack();
                    return Result.Failed;
                }
                tranGR.Assimilate();
                return Result.Succeeded;
            }
        }
    }
}
