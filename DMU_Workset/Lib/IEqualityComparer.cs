using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMU_Workset.Lib
{
    public class IEqualityComparer : IEqualityComparer<Element>
    {
        public bool Equals(Element x, Element y)
        {
            if (x == null || y == null)
                return false;

            Category firstCategory = x.Category;
            Category secondCategory = y.Category;

            if (firstCategory == null || secondCategory == null)
            {
                return false;
            }
            return x.Category.Name.Equals(y.Category.Name);
        }

        public int GetHashCode(Element obj)
        {
            return 0;
        }
    }
}
