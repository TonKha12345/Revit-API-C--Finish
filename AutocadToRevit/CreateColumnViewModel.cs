using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using AutocadToRevit.Lib;
using AlphaBIM;
using Autodesk.Revit.UI.Selection;
using Lib;

namespace AutocadToRevit
{
    public class CreateColumnViewModel: ViewModelBase
    {
        public UIDocument UiDoc;
        public Document Doc;
        public ImportInstance SelectedCadLink;

        //Khoi tao Binding
        #region Khai báo Binding Properties 
        public List<string> AllLayers { get; set; }
        public string SelectedLayer { get; set; }
        public List<Family> AllFamiliesColumn { get; set; }
        public Family SelectedFamilyColumn { get; set; }
        public List<Level> AllLevel { get; set; }
        public Level BaseLevel { get; set; }
        public Level TopLevel { get; set; }
        public double BaseOffset { get; set; }
        public double TopOffset { get; set; }
        private double _percent;
        public double Percent
        {
            get { return _percent; }
            set 
            {
                _percent = value;
                OnPropertyChanged();
            }
        
        }
        #endregion Khai báo biến & properties

        public CreateColumnViewModel(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
            Doc = uiDoc.Document;

            Reference r = UiDoc.Selection.PickObject(ObjectType.Element);
            SelectedCadLink = Doc.GetElement(r) as ImportInstance;
            AllLayers = new List<string>();
            AllLayers = CadUtils.GetAllLayer(SelectedCadLink);
            AllLayers = AllLayers.Distinct().ToList();
            AllLayers.Sort();
            SelectedLayer = AllLayers[0];

            AllLevel = new List<Level>();
            AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            AllLevel = AllLevel.OrderBy(f=>f.Elevation).ToList();
            
            BaseLevel = AllLevel[0];
            TopLevel = AllLevel[1];

            AllFamiliesColumn = new List<Family>();
            AllFamiliesColumn = new FilteredElementCollector(Doc).OfClass(typeof(Family)).Cast<Family>().Where(e=>e.FamilyCategory.Name== "Structural Columns" || e.FamilyCategory.Name=="Column").ToList();
            
        }
    }
}
