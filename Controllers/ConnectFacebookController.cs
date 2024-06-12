using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog;
using Corprio.DataModel.Business;
using Microsoft.Extensions.Configuration;
using Corprio.Core;
using System.Linq;
using Corprio.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Corprio.SocialWorker.Helpers;
using Corprio.SocialWorker.Models.Meta;

namespace Corprio.SocialWorker.Controllers
{
    public class ConnectFacebookController : MetaApiController
    {
        private readonly ApplicationDbContext db;

        public ConnectFacebookController(ApplicationDbContext context, IConfiguration configuration, 
            IHttpClientFactory httpClientFactory) : base(configuration, httpClientFactory)
        {
            db = context;   
        }                
                        
        /// <summary>
        /// Get a long-lived user access token from Meta
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="userAccessToken">Short-lived user access token in exchange for a long-lived token</param>
        /// <returns>Long-lived user access token</returns>
        public async Task<string> GetLongLivedAccessToken(HttpClient httpClient, string userAccessToken)
        {
            var queryParams = new { grant_type = "fb_exchange_token", client_id = AppId, client_secret = AppSecret, fb_exchange_token = userAccessToken };
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/oauth/access_token"),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode) 
            {
                Log.Error($"HTTP request to obtain long-lived access token fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            LongLivedUserAccessTokenPayload payload = JsonConvert.DeserializeObject<LongLivedUserAccessTokenPayload>(responseString);
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error when getting long-lived access token. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.AccessToken;
        }        
        
        /// <summary>
        /// Register/refresh access token(s) and relevant FB pages and Meta user in the database
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>        
        /// <param name="organizationID">Organization ID</param>
        /// <param name="metaId">Meta entity ID associated with the access token</param>
        /// <param name="token">Access token</param>
        /// <param name="reAssignMetaProfile">True if the Facebook account can be reassigned from one organization to another</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> RefreshAccessToken([FromServices] HttpClient httpClient, [FromRoute] Guid organizationID, 
            string metaId, string token, bool reAssignMetaProfile = false)
        {            
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(metaId))
                return StatusCode(400, "Token and Meta entity ID cannot be blank. ");
            
            token = await GetLongLivedAccessToken(httpClient: httpClient, userAccessToken: token);
            if (string.IsNullOrWhiteSpace(token)) return StatusCode(500, Resources.SharedResource.ResourceManager.GetString("ErrMsg_FailedToGetToken"));
                        
            MetaUser metaUser = db.MetaUsers.Include(x => x.Pages).FirstOrDefault(x => x.FacebookUserID == metaId && x.Dormant == false);
            bool newMetaUser = metaUser == null;
            if (newMetaUser)
            {                
                metaUser = new MetaUser()
                {
                    ID = Guid.NewGuid(), 
                    FacebookUserID = metaId,
                    OrganizationID = organizationID,
                };
            }
            else if (metaUser.OrganizationID != organizationID && !organizationID.Equals(Guid.Empty))
            {                                                
                if (!reAssignMetaProfile)
                {                    
                    // note: the client-side must expect 409 = conflict of organization IDs
                    return StatusCode(409, "Conflict of organization ID.");
                }

                // note: instead of deleting the existing Meta profile, we put it to sleep
                metaUser.Dormant = true;
                db.MetaUsers.Update(metaUser);
                await db.SaveChangesAsync();

                // note: even if this organization used to have connection with this Facebook account
                // (i.e., it has a dormant meta user profile), we create a new one for it
                metaUser = new MetaUser()
                {
                    ID = Guid.NewGuid(),
                    FacebookUserID = metaId,
                    OrganizationID = organizationID,
                };
                newMetaUser = true;
            }

            metaUser.Token = token;
            if (newMetaUser)
            {
                db.MetaUsers.Add(metaUser);
            }
            else
            {
                db.MetaUsers.Update(metaUser);
            }
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save Facebook user {metaUser.ID}. {ex.Message}");
                throw;
            }

            // the following query returns all pages on which the Facebook user has a role
            string responseString = await ApiActionHelper.GetQuery(httpClient: httpClient, accessToken: token, endPoint: $"{BaseUrl}/{metaId}/accounts");
            MeAccountsPayload payload = JsonConvert.DeserializeObject<MeAccountsPayload>(responseString);
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error when getting pages owned by {metaId}. {payload?.Error?.CustomErrorMessage()}");
            }
            List<FbPage> fbPages = payload?.Data ?? throw new Exception(string.Format(Resources.SharedResource.ErrMsg_FailedToGetPages, metaId));

            bool newMetaPage;
            foreach (FbPage page in fbPages)
            {
                MetaPage metaPage = metaUser.Pages.FirstOrDefault(x => x.PageId == page.Id);
                newMetaPage = metaPage == null;
                if (newMetaPage) metaPage = new MetaPage() { ID = Guid.NewGuid(), FacebookUserID = metaUser.ID, PageId = page.Id };
                metaPage.Name = StringHelper.StringTruncate(page.Name, 300);
                metaPage.Token = page.AccessToken;

                // the following query returns the IG account associated with the FB page
                // (note: not every FB page is associated with an IG account)
                responseString = await ApiActionHelper.GetQuery(httpClient: httpClient, accessToken: page.AccessToken, endPoint: $"{BaseUrl}/{ApiVersion}/{page.Id}?fields=instagram_business_account");
                FbPagePayload pagePayload = JsonConvert.DeserializeObject<FbPagePayload>(responseString);
                if (pagePayload?.Error != null)
                {
                    Log.Error($"Encountered an error in obtaining IG user ID for {page.Id}. {payload?.Error?.CustomErrorMessage()}");
                }
                metaPage.InstagramID = pagePayload?.InstagramBusinessAccount?.Id;

                if (newMetaPage)
                {
                    db.MetaPages.Add(metaPage);
                }
                else
                {
                    db.MetaPages.Update(metaPage);
                }
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to save page {metaPage.ID}. {ex.Message}");
                    throw;
                }                
            }
            
            // the user may have opted out some pages previously they granted us permissions, which should be deleted
            foreach (MetaPage page in metaUser.Pages)
            {
                if (!fbPages.Any(x => x.Id == page.PageId))
                {
                    db.MetaPages.Remove(page);
                    await db.SaveChangesAsync();
                }
            }

            return StatusCode(200);
        }
    }
}
