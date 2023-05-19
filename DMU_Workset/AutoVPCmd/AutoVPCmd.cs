using AlphaBIM;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMU_Workset.AutoVPCmd
{
    [Transaction(TransactionMode.Manual)]
    public class AutoVPCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //string dllFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            //AssemblyLoader.LoadAllRibbonAssemblies(dllFolder);
            AutoVPViewModel viewModel = new AutoVPViewModel(uidoc);
            AutoVPWindow window = new AutoVPWindow(viewModel);
            window.ShowDialog();
            return Result.Cancelled;
        }
    }
}
