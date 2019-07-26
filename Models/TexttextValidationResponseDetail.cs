using System;
using System.Collections.Generic;

namespace CRLCP.Models
{
    public partial class TexttextValidationResponseDetail
    {
        public long AutoId { get; set; }
        public int UserId { get; set; }
        public long RefSourceAutoid { get; set; }
        public long RefAutoid { get; set; }
        public int IsMatch { get; set; }
        public int? ValidationFlag { get; set; }
    }
}
