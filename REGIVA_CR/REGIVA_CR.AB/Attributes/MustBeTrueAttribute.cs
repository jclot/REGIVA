using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.Attributes
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool b && b;
        }
    }
}
