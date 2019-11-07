using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D.Net.EmailInterfaces
{
    public interface IEMailAttachment
    {
        String Text { get; set; }
        Byte[] Body { get; set; }
    }
}
