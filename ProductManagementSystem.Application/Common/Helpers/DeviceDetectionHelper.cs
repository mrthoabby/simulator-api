using System.Text.RegularExpressions;

namespace ProductManagementSystem.Application.Common.Helpers;

public static class DeviceDetectionHelper
{
    private static readonly Random _random = new();

    private static readonly string[] _fallbackNames =
    {
        "Unknown Device",
        "Mobile Device",
        "Desktop Computer",
        "Web Browser",
        "Smart Device",
        "Personal Device"
    };

    public static string ExtractDeviceName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return GetRandomFallbackName();
        }

        try
        {
            var normalizedUA = userAgent.ToLowerInvariant();

            if (IsIPhone(normalizedUA))
                return ExtractIPhoneModel(userAgent);

            if (IsAndroid(normalizedUA))
                return ExtractAndroidDevice(userAgent);

            if (IsIPad(normalizedUA))
                return "iPad";

            if (IsMacOS(normalizedUA))
                return ExtractMacDevice(userAgent);

            if (IsWindows(normalizedUA))
                return ExtractWindowsDevice(userAgent);

            if (IsLinux(normalizedUA))
                return "Linux Computer";

            return ExtractBrowserInfo(userAgent);
        }
        catch
        {
            return GetRandomFallbackName();
        }
    }

    private static bool IsIPhone(string userAgent) =>
        userAgent.Contains("iphone");

    private static bool IsAndroid(string userAgent) =>
        userAgent.Contains("android");

    private static bool IsIPad(string userAgent) =>
        userAgent.Contains("ipad");

    private static bool IsMacOS(string userAgent) =>
        userAgent.Contains("macintosh") || userAgent.Contains("mac os x");

    private static bool IsWindows(string userAgent) =>
        userAgent.Contains("windows");

    private static bool IsLinux(string userAgent) =>
        userAgent.Contains("linux") && !userAgent.Contains("android");

    private static string ExtractIPhoneModel(string userAgent)
    {
        // Extraer modelo de iPhone si está disponible
        var match = Regex.Match(userAgent, @"iPhone\s?OS\s?(\d+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"iPhone (iOS {match.Groups[1].Value})";
        }
        return "iPhone";
    }

    private static string ExtractAndroidDevice(string userAgent)
    {
        // Intentar extraer modelo de Android
        var modelMatch = Regex.Match(userAgent, @"Android.*?;\s*([^)]+)\)", RegexOptions.IgnoreCase);
        if (modelMatch.Success)
        {
            var model = modelMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(model) && model.Length < 50)
            {
                return $"Android ({model})";
            }
        }
        return "Android Device";
    }

    private static string ExtractMacDevice(string userAgent)
    {
        if (userAgent.ToLowerInvariant().Contains("macbook"))
            return "MacBook";

        if (userAgent.ToLowerInvariant().Contains("imac"))
            return "iMac";

        return "Mac Computer";
    }

    private static string ExtractWindowsDevice(string userAgent)
    {
        if (userAgent.ToLowerInvariant().Contains("windows nt 10"))
            return "Windows 10/11 Computer";

        if (userAgent.ToLowerInvariant().Contains("windows nt 6"))
            return "Windows Computer";

        return "Windows Computer";
    }

    private static string ExtractBrowserInfo(string userAgent)
    {
        var normalizedUA = userAgent.ToLowerInvariant();

        if (normalizedUA.Contains("chrome"))
            return "Chrome Browser";

        if (normalizedUA.Contains("firefox"))
            return "Firefox Browser";

        if (normalizedUA.Contains("safari") && !normalizedUA.Contains("chrome"))
            return "Safari Browser";

        if (normalizedUA.Contains("edge"))
            return "Edge Browser";

        return GetRandomFallbackName();
    }

    private static string GetRandomFallbackName()
    {
        return _fallbackNames[_random.Next(_fallbackNames.Length)];
    }
}