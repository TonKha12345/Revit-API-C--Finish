using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public static class GetTextNoteIntersecWithElement
    {
        public static List<TextNote> GetTextNoteIntersecWithElements(this Element e, Document doc)
        {
            //Lay ve BoundingBox cua Element e
            BoundingBoxXYZ box  = e.get_BoundingBox(doc.ActiveView);
            XYZ MinPoint = new XYZ(box.Min.X - AlphaBIMUnitUtils.MmToFeet(50), box.Min.Y - AlphaBIMUnitUtils.MmToFeet(50),0);
            XYZ MaxPoint = new XYZ(box.Max.X + AlphaBIMUnitUtils.MmToFeet(50), box.Max.Y + AlphaBIMUnitUtils.MmToFeet(50),0);
            Outline outlineElement = new Outline(MinPoint, MaxPoint);

            //Lay ve BoundingBox cua cac TextNote giao voi Element e
            List<TextNote> AllTextNote = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(TextNote)).Cast<TextNote>().ToList();
            List<TextNote> listTextNote = new List<TextNote>();
            foreach (TextNote textNote in AllTextNote)
            {
                BoundingBoxXYZ box1 = textNote.get_BoundingBox(doc.ActiveView);
                XYZ MinPointText = new XYZ(box1.Min.X - AlphaBIMUnitUtils.MmToFeet(50), box1.Min.Y - AlphaBIMUnitUtils.MmToFeet(50),0);
                XYZ MaxPointText = new XYZ(box1.Max.X + AlphaBIMUnitUtils.MmToFeet(50), box1.Max.Y + AlphaBIMUnitUtils.MmToFeet(50), 0);
                Outline outlineTextNote = new Outline(MinPointText, MaxPointText);

                bool b = outlineElement.Intersects(outlineTextNote, 0.001);
                if (b) { listTextNote.Add(textNote); }
            }
            return listTextNote;
        }
    }
}
