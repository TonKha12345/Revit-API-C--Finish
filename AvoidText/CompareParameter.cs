using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidText
{
    public class CompareParameter : IEqualityComparer<Parameter>
    {
        public bool Equals(Parameter x, Parameter y)
        {
            return x.Definition.Name.Equals(y.Definition.Name);
        }

        public int GetHashCode(Parameter obj)
        {
            return 0;
        }
    }
}
