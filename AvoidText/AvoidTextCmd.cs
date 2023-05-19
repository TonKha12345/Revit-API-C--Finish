using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AvoidText
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class AvoidTextCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (TransactionGroup tranG = new TransactionGroup(doc, "Avoid obstruction"))
            {
                tranG.Start();
                AvoidTextViewModel viewModel = new AvoidTextViewModel(uidoc);
                AvoidTextWindow window = new AvoidTextWindow(viewModel);
                bool? showDialog = window.ShowDialog();
                if (showDialog == null || showDialog == false)
                {
                    tranG.RollBack();
                    return Result.Failed;
                }
                tranG.Assimilate();
                return Result.Succeeded;
            }
        }
    }
}

