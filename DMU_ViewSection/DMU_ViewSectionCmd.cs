using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace DMU_ViewSection
{
    [Transaction(TransactionMode.Manual)]
    public class DMU_ViewSectionCmd : IExternalCommand
    {
        Document m_document;
        UIDocument m_documentUI;

        //application's private data
        private DMU_ViewSectionUpdater m_viewSectionUpdater = null;
        private AddInId m_thisAppId;
        private List<ElementId> idsToWatch = new List<ElementId>();
        private ElementId m_oldSectionId = ElementId.InvalidElementId;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_documentUI = commandData.Application.ActiveUIDocument;
            m_document = m_documentUI.Document;
            m_thisAppId = commandData.Application.ActiveAddInId;

            try
            {
                //Creating and registering the updater for the document
                if (m_viewSectionUpdater == null)
                {
                    using (Transaction trans = new Transaction(m_document, " Register"))
                    {
                        trans.Start();
                        m_viewSectionUpdater = new DMU_ViewSectionUpdater(m_thisAppId);
                        m_viewSectionUpdater.Register(m_document);
                        trans.Commit();
                    }
                }

                MessageBox.Show("Select a section view, then select a window", "Information");

                Element sectionElement = null; //Section Element pick object
                Element window = null;
                Element section = null; //Section Element in view
                try
                {
                    //Get section Element
                    Reference refSection = m_documentUI.Selection.PickObject(ObjectType.Element, "Select section View");
                    if (refSection != null)
                    {
                        Element sectionElem = m_document.GetElement(refSection);
                        if(sectionElem != null)
                        {
                            sectionElement = sectionElem;
                        }
                    }

                    //Get window
                    Reference refWindow = m_documentUI.Selection.PickObject(ObjectType.Element, "Select window");
                    if(refWindow != null)
                    {
                        Element win = m_document.GetElement(refWindow);
                        if(win != null)
                        {
                            window = win;
                        }
                    }
                }
                catch(OperationCanceledException)
                {
                    MessageBox.Show("The selection to be fauld", "Information");
                    return Result.Cancelled;
                }

                if(window == null || sectionElement == null)
                {
                    MessageBox.Show("Window is null", "Error");
                    return Result.Cancelled;
                }

                //Find the real ViewSection for the selected section element
                List<Element> sectionViews = new FilteredElementCollector(m_document).OfCategory(BuiltInCategory.OST_Views).Where(e => e.Name.Equals(sectionElement.Name)).ToList();    
                if(sectionViews.Count == 0)
                {
                    MessageBox.Show("Does not view Element in Views", "Error");
                    return Result.Failed;
                }
                section = sectionViews.FirstOrDefault();

                //The section view associcated to the window, and add trigger for it
                if(!idsToWatch.Contains(window.Id) || m_oldSectionId != section.Id)
                {
                    idsToWatch.Clear();
                    idsToWatch.Add(window.Id);
                    m_oldSectionId = section.Id;
                    UpdaterRegistry.RemoveAllTriggers(m_viewSectionUpdater.GetUpdaterId());
                    m_viewSectionUpdater.AddTriggerForUpdate(m_document, idsToWatch, section.Id, sectionElement);
                    MessageBox.Show("The ViewSection id: " + section.Id.ToString() + "has been associate to the window id: " + window.Id.ToString() + "\n You can try to move or rotate window to see update ", "Information");
                }
                else
                {
                    MessageBox.Show("The model has been already associated to the ViewSection", "Information");
                }

                m_document.DocumentClosing += UnregisterSectionUpdaterOnClose;
                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void UnregisterSectionUpdaterOnClose(object source, DocumentClosingEventArgs args)
        {
            idsToWatch.Clear();
            m_oldSectionId = ElementId.InvalidElementId;
            if(m_viewSectionUpdater != null)
            {
                UpdaterRegistry.UnregisterUpdater(m_viewSectionUpdater.GetUpdaterId());
                m_viewSectionUpdater = null;
            }
        }
    }
}
