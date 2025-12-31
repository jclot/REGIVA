using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.Exceptions
{
    public class DuplicateInfoException : Exception
    {
        public string FieldName { get; }
        public DuplicateInfoException(string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
        }
    }
}
