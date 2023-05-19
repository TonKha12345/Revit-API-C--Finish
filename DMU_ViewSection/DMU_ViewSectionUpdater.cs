using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace DMU_ViewSection
{
    public class DMU_ViewSectionUpdater: IUpdater
    {
        private UpdaterId m_updaterId = null;
        private ElementId m_windowId = null;
        private ElementId m_sectionId = null;       // The real ViewSection that contains the origin and ViewDirection
        private Element m_sectionElement = null;    // The view section Element to move and rotate

        public DMU_ViewSectionUpdater(AddInId addinId)
        {
            m_updaterId = new UpdaterId(addinId, new Guid("066400B2-BB97-44CF-9853-C8FB4091B31E"));
        }

        internal void Register(Document doc)
        {
            if(!UpdaterRegistry.IsUpdaterRegistered(m_updaterId))
            {
                UpdaterRegistry.RegisterUpdater(this, doc);
            }
        }

        internal void AddTriggerForUpdate(Document doc, List<ElementId> idsToWatch, ElementId sectionId, Element sectionElement)
        {
            if (idsToWatch.Count == 0) return;
            m_windowId = idsToWatch.FirstOrDefault();
            m_sectionId = sectionId;
            m_sectionElement = sectionElement;
            UpdaterRegistry.AddTrigger(m_updaterId, doc, idsToWatch, Element.GetChangeTypeGeometry());
        }

        internal void RejustSectionView(Document doc, Element elem, ViewSection section)
        {
            //Get position and rotation of FamilyInstance
            XYZ position = XYZ.Zero;
            XYZ fOrientation = XYZ.Zero;
            if(elem is FamilyInstance)
            {
                FamilyInstance familyInstance = elem as FamilyInstance;
                if(familyInstance.Location != null && familyInstance.Location is LocationPoint)
                {
                    LocationPoint locationPoint = familyInstance.Location as LocationPoint;
                    position = locationPoint.Point;
                }
                fOrientation = familyInstance.FacingOrientation;
            }

            //Get position and Direction of view section
            XYZ sOrigin = section.Origin;
            XYZ sDirection = section.ViewDirection;

            XYZ fRectOrientation = fOrientation.CrossProduct(XYZ.BasisZ);

            //Rotate the section Element
            double angle = fOrientation.AngleTo(sDirection);

            //Need to adjust the rotation angle based on the direction of rotation
            XYZ cross = fRectOrientation.CrossProduct(sDirection).Normalize();

            double sign = 1.0;
            if (!cross.IsAlmostEqualTo(XYZ.BasisZ))
            {
                sign = -1.0;
            }

            double rotateAngle = 0.0;
            if(Math.Abs(angle) > 0 && Math.Abs(angle) <= Math.PI / 2)
            {
                if(angle < 0)
                {
                    rotateAngle = Math.PI / 2.0 + angle;
                }
                else
                {
                    rotateAngle = Math.PI / 2.0 - angle;
                }
            }
            else if(Math.Abs(angle) > Math.PI / 2)
            {
                if(angle < 0)
                {
                    rotateAngle = angle + Math.PI / 2.0;
                }
                else
                {
                    rotateAngle = angle - Math.PI / 2.0;
                }
            }
            rotateAngle *= sign; 

            if(Math.Abs(rotateAngle) > 0)
            {
                Line axis = Line.CreateBound(sOrigin, sOrigin + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, m_sectionElement.Id, axis, rotateAngle);
            }

            //Regenerate the document
            doc.Regenerate();

            //Move the section Element
            double dotF = position.DotProduct(fRectOrientation);
            double dotS = sOrigin.DotProduct(fRectOrientation);
            double moveDot = dotF - dotS;
            XYZ sNewDirection = section.ViewDirection;
            double correction = fRectOrientation.DotProduct(sNewDirection);
            XYZ translationVec = sNewDirection * correction * moveDot;

            if (!translationVec.IsZeroLength())
            {
                ElementTransformUtils.MoveElement(doc,m_sectionElement.Id, translationVec);
            }

        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                //through modified elements to find which we want the section to follow
                foreach(ElementId id in data.GetModifiedElementIds())
                {
                    if(id == m_windowId)
                    {
                        FamilyInstance window = doc.GetElement(id) as FamilyInstance;
                        ViewSection section = doc.GetElement(m_sectionId) as ViewSection;
                        RejustSectionView(doc, window, section);
                    }
                }
            }
            catch(Exception e) 
            {
                MessageBox.Show(e.ToString(), "Error");
            }
            return;
        }

        public string GetAdditionalInformation()
        {
            return "Contact via email trantonkha1999@gmail.com";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Section Update";
        }

        
    }
}
