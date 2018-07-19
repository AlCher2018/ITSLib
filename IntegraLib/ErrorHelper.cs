using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public class ErrorHelper
    {
        public static string GetShortErrMessage(Exception ex)
        {
            string retVal = ex.Message;
            if (ex.InnerException != null) retVal += " Inner exception: " + ex.InnerException.Message;
            return retVal;
        }

    }  // end class
}
