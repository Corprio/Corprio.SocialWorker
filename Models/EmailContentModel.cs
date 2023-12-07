using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Data model for saving globalized text for email contents
    /// </summary>
    public class EmailContentModel
    {
        public Global.Globalization.GlobalizedText Subject { get; set; }
        public Global.Globalization.GlobalizedText Body { get; set; }
    }
}
