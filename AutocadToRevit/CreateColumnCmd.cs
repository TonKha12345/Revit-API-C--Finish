using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using AutocadToRevit.Lib;
using System.Windows;

namespace AutocadToRevit
{
    [Transaction(TransactionMode.Manual)]
    public class CreateColumnCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using(TransactionGroup tranG = new TransactionGroup(doc, "Create Column"))
            {
                tranG.Start();
                CreateColumnViewModel viewModel = new CreateColumnViewModel(uidoc);
                CreateColumn window = new CreateColumn(viewModel);
                bool? showDialog = window.ShowDialog();
                if (showDialog == null || showDialog == false)
                {
                    tranG.RollBack();
                    return Result.Cancelled;
                }
                tranG.Assimilate();
                return Result.Succeeded;
            }
        }
    }
}
