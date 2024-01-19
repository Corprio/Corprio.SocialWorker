using Microsoft.AspNetCore.Mvc;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Header of an inter-Corprio-projects API request
    /// </summary>
    public class CorprioRequestHeader
    {
        [FromHeader(Name = "X-Corprio-Hash")]
        public string Hash { get; set; }
    }
}
