using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMU_Workset.Lib;
using AlphaBIM;
using System.IO;
using ParameterUtils = AlphaBIM.ParameterUtils;

namespace DMU_Workset.AutoVPCmd
{
    public class AutoVPViewModel
    {
        internal UIDocument UiDoc;
        internal Document Doc;
        internal AutoVPViewModel(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Doc = UiDoc.Document;

            Initialize();
            LoadLastestSetting();
        }
        #region Binding
        public List<string> AllParameters { get; set; } = new List<string>();
        public string SelectedParameterAuto { get; set; }
        public string ValueParametersAuto { get; set; }
        public bool IsCheckedAuto { get; set; }
        #endregion

        private void Initialize()
        {
            #region Get all Parameter
            List<Element> allElements = new FilteredElementCollector(Doc, Doc.ActiveView.Id).WhereElementIsNotElementType().Where(e => e.Category != null).ToList();
            allElements = allElements.Distinct(new IEqualityComparer()).ToList();

            foreach (Element element in allElements)
            {
                AllParameters.AddRange(ParameterUtils.GetAllParameters(element,false));
            }
            AllParameters = AllParameters.Distinct().ToList();
            AllParameters.Sort();

            #endregion
        }
        
        private void LoadLastestSetting()
        {
            SettingRepository<AutoVPSetting> settingRepository = new SettingRepository<AutoVPSetting>();
            string pathSetting = Path.Combine("C:\\Users\\Admin\\Desktop\\Hoc Revit API\\DMU_Workset\\Source", "SettingAutoValueParameter");
            AutoVPSetting lastestSetting = settingRepository.GetSetting(pathSetting);
            SelectedParameterAuto = lastestSetting.SelectedParameterAuto;
            ValueParametersAuto = lastestSetting.ValueParametersAuto;
            IsCheckedAuto = lastestSetting.IsCheckedAuto;
        }

        internal void SaveSetting()
        {
            SettingRepository<AutoVPSetting> settingRepository = new SettingRepository<AutoVPSetting>();
            AutoVPSetting settingToSave = new AutoVPSetting();
            settingToSave.SelectedParameterAuto = SelectedParameterAuto;
            settingToSave.ValueParametersAuto = ValueParametersAuto;
            settingToSave.IsCheckedAuto = IsCheckedAuto;

            string pathSetting = Path.Combine("C:\\Users\\Admin\\Desktop\\Hoc Revit API\\DMU_Workset\\Source", "SettingAutoValueParameter");
            settingRepository.SaveSetting(settingToSave, pathSetting);
        }
    }
}
