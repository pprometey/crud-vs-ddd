using System.Globalization;
using System.Resources;

namespace Core.Common.Domain;

public static class FeatureMessages
{
    public static string Msg(ResourceManager resources, string code, params object[] args)
    {
        var template = resources.GetString(code, CultureInfo.CurrentUICulture) ?? code;
        return args.Length == 0 ? template : string.Format(template, args);
    }
}
