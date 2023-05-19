using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMU_Workset.AutoVPApp
{
    public class AutoVPApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            AddInId activeAddinId = application.ActiveAddInId;
            AutoVPUpdater updater = new AutoVPUpdater(activeAddinId);
            UpdaterRegistry.RegisterUpdater(updater);

            ElementClassFilter filter = new ElementClassFilter(typeof(Duct));
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            AddInId activeAddinId = application.ActiveAddInId;
            AutoVPUpdater updater = new AutoVPUpdater(activeAddinId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            return Result.Succeeded;
        }
    }
}
