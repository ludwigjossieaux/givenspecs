using System;
using System.Collections.Generic;
using System.Text;

namespace GivenSpecs.Application.Exceptions
{
    public class TableRowInvalidHeadersException: Exception
    {
        public override string Message => "TableRow should be initialized with valid headers"; 
    }
}
