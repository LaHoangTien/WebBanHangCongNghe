using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace WebBanHang
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project

    public static class SessionExtentions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default :
            JsonSerializer.Deserialize<T>(value);
        }
    }
}
