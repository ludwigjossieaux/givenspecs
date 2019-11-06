using System;
using System.Collections.Generic;
using System.Text;

namespace GivenSpecs.Application.Exceptions
{
    public class StepNotFoundException: Exception
    {
        private readonly string _keyword;
        private readonly string _text;

        public override string Message
        {
            get 
            { 
                return $"no step for {_keyword} -> {_text}";
            }
        }

        public StepNotFoundException(string keyword, string text): base()
        {
            this._keyword = keyword;
            this._text = text;
        }
    }
}
