using Microsoft.AspNetCore.Mvc;

namespace Corprio.SocialWorker.Models
{
    public class CorprioRequestHeader
    {
        [FromHeader(Name = "X-Corprio-Hash")]
        public string Hash { get; set; }
    }
}
