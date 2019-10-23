using System;
using System.Collections.Generic;
using System.Text;

namespace GivenSpecs.Interfaces
{
    public interface IParameterConverter<T> where T : new()
    {
        T ConvertParameter(string input);
    }
}
