using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai5_CreateElement.Lib
{
    public static class GetAllFamilySymbol
    {
        public static FamilySymbol GetAllFamilySymbolOfFraming(Family family, double b, double h)
        {
            List<FamilySymbol> allfamilysymbols = family.GetAllFamilySymbols();
            foreach (FamilySymbol familysymbol in allfamilysymbols)
            {
                Parameter bParameter = familysymbol.LookupParameter("b");
                Parameter hParameter = familysymbol.LookupParameter("h");
                double bvalue = bParameter.AsDouble();
                double hvalue = hParameter.AsDouble();

                if(bvalue == b || hvalue == h)
                {
                    return familysymbol;
                }
            }
            
            double sectionX = Math.Round(AlphaBIMUnitUtils.FeetToMm(b));
            double sectionY = Math.Round(AlphaBIMUnitUtils.FeetToMm(h));
            string name = string.Concat(sectionX, " x ", sectionY);

            FamilySymbol result = null;
            using(Transaction trans = new Transaction(family.Document, "Create hb"))
            {
                trans.Start();
                ElementType s1 = allfamilysymbols[0].Duplicate(name);
                s1.LookupParameter("b").Set(b);
                s1.LookupParameter("h").Set(h);

                result = s1 as FamilySymbol;
                trans.Commit();
            }
            return result;


        }
        public static List<FamilySymbol> GetAllFamilySymbols(this Family family)
        {
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();    
            foreach (ElementId id in family.GetFamilySymbolIds())
            {
                FamilySymbol familysymbol = family.Document.GetElement(id) as FamilySymbol;
                familySymbols.Add(familysymbol);
            }
            return familySymbols;
        }
    }
}
