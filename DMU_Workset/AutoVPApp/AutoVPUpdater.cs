using AlphaBIM;
using Autodesk.Revit.DB;
using DMU_Workset.AutoVPCmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMU_Workset.AutoVPApp
{
    internal class AutoVPUpdater : IUpdater
    {
        private AddInId _activeAddInId;
        internal AutoVPUpdater(AddInId activeAddInId)
        {
            _activeAddInId = activeAddInId;
        }

        public void Execute(UpdaterData data)
        {
            SettingRepository<AutoVPSetting> settingRepository = new SettingRepository<AutoVPSetting>();
            string pathSetting = Path.Combine("C:\\Users\\Admin\\Desktop\\Hoc Revit API\\DMU_Workset\\Source", "SettingAutoValueParameter");
            AutoVPSetting lastestSetting = settingRepository.GetSetting(pathSetting);
            if (lastestSetting.IsCheckedAuto == false) return;
            List<ElementId> addElementIds = data.GetAddedElementIds().ToList();
            if(addElementIds.Count > 0)
            {
                SetValue(data.GetDocument(), lastestSetting, addElementIds);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Contact via email: trantonkha1999@gmail.com";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        public UpdaterId GetUpdaterId()
        {
            Guid guid = new Guid("3E14490D-2CC1-4C09-A4C5-C92CFA404DF8");
            UpdaterId updaterId = new UpdaterId(_activeAddInId, guid);
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Contact via email: trantonkha1999@gmail.com";
        }

        private void SetValue(Document doc, AutoVPSetting setting, List<ElementId> addedElementIds)
        {
            foreach (ElementId elementId in addedElementIds)
            {
                Element elem = doc.GetElement(elementId);
                Parameter p = elem.LookupParameter(setting.SelectedParameterAuto);
                if(p != null && !p.IsReadOnly)
                {
                    p.SetValue(setting.ValueParametersAuto);
                } 
            }
        }
    }
}
