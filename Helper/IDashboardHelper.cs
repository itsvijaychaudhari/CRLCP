using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRLCP.Helper
{
    public interface IDashboardHelper
    {
         string DatasetName { get; set; }
         int SrcDatasetCount { get; set; }
         int DestDatasetCount { get; set; }
         int ValDatasetCount { get; set; }
    }

    public class DashboardHelper : IDashboardHelper
    {
        public string DatasetName { get ; set ; }
        public int SrcDatasetCount { get ; set ; }
        public int DestDatasetCount { get ; set ; }
        public int ValDatasetCount { get ; set ; }
    }
}
