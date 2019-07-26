using System;
using System.Collections.Generic;

namespace CRLCP.Models
{
    public partial class ImageText
    {
        public long AutoId { get; set; }
        public int UserId { get; set; }
        public long DataId { get; set; }
        public int DomainId { get; set; }
        public string OutputData { get; set; }
        public int OutputLangId { get; set; }
        public int DatasetId { get; set; }
        public DateTime AddedOn { get; set; }
        public int? TotalValidationUsersCount { get; set; }
        public int? VoteCount { get; set; }
        public int? IsValid { get; set; }
        public int? IsAddedInDataset { get; set; }
    }
}
