using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Corprio.SocialWorker.Controllers
{
    [Authorize]
    public class UserPreferenceController : AspNetCore.Site.Controllers.UserPreferenceControllerBase
	{
        public UserPreferenceController() : base()
        {
        }
    }
}
