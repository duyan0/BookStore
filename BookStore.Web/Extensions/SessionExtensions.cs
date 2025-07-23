namespace BookStore.Web.Extensions
{
    public static class SessionExtensions
    {
        public static bool IsLoggedIn(this ISession session)
        {
            var token = session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Simple check for JWT token format and expiration
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                var payload = parts[1];
                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                if (json != null && json.ContainsKey("exp"))
                {
                    var expElement = (System.Text.Json.JsonElement)json["exp"];
                    var exp = expElement.GetInt64();
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    return expDateTime > DateTime.UtcNow;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string GetCurrentUsername(this ISession session)
        {
            return session.GetString("Username") ?? "";
        }

        public static string GetCurrentUserFullName(this ISession session)
        {
            return session.GetString("FullName") ?? "";
        }

        public static bool IsCurrentUserAdmin(this ISession session)
        {
            return session.GetString("IsAdmin") == "true";
        }

        public static int GetCurrentUserId(this ISession session)
        {
            return session.GetInt32("UserId") ?? 0;
        }
    }
}
