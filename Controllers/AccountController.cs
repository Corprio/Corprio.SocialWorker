using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Corprio.SocialWorker.Controllers
{
    [Authorize]
    public class AccountController : AspNetCore.Site.Controllers.AccountControllerBase
    {
        public AccountController(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
