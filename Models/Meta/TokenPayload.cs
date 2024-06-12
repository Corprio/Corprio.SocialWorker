using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models.Meta
{
    public class LongLivedUserAccessTokenPayload : MetaPayloadError
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public double ExpiresIn { get; set; }
    }

    public class AppAccessTokenPayload
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    /// <summary>
    /// https://developers.facebook.com/docs/facebook-login/guides/%20access-tokens/debugging
    /// </summary>
    public class TokenDebugPayload
    {
        [JsonProperty("data")]
        public DebugDetails Data { get; set; }
    }

    public class DebugDetails
    {
        [JsonProperty("application")]
        public string Application { get; set; }

        [JsonProperty("app_id")]
        public string ApplicationId { get; set; }

        [JsonProperty("data_access_expires_at")]
        public double DataAccessExpiresAt { get; set; }

        [JsonProperty("error")]
        public DebugError Error { get; set; }

        [JsonProperty("expires_at")]
        public double ExpiresAt { get; set; }

        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }

        [JsonProperty("profile_id")]
        public string ProfileId { get; set; }

        [JsonProperty("scopes")]
        public List<string> Scope { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }

    public class DebugError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("subcode")]
        public string Subcode { get; set; }
    }

    public class MeAccountsPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<FbPage> Data { get; set; }

        [JsonProperty("paging")]
        public Pagination Paging { get; set; }
    }
}
