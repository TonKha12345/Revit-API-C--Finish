using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Bai3_WPF.Lib
{
    public class EqualityCompareCategory : IEqualityComparer<Element>
    {
        //Loai bo 1 trong 2 element co category name giong nhau
        public bool Equals(Element x, Element y)
        {
            return x.Category.Name.Equals(y.Category.Name);
        }

        public int GetHashCode(Element obj)
        {
            return 0;
        }
    }
}

