using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRLCP.Models
{
    public class ValidationImageTextModel
    {
        public long DestAutoId { get; set; }

        public string DestinationData { get; set; }

        public long SourceDataId { get; set; }
        public string SourceData { get; set; }

        public int DatasetID { get; set; }
    }
}
