using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm4Rest_Viewer.Services
{
 public class RestEventArgs : EventArgs
    {
            public string message { get; set; }
            public DateTime TimeStamp { get; set; }
        
  }
}
