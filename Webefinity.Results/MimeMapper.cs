using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Results;

public static class MimeMapper
{
    static Dictionary<string, string> mimeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["application/json"] = ".json",
        ["application/xml"] = ".xml",
        ["application/pdf"] = ".pdf",
        ["application/zip"] = ".zip",
        ["application/msword"] = ".doc",
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ".docx",
        ["application/vnd.ms-excel"] = ".xls",
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = ".xlsx",
        ["application/vnd.ms-powerpoint"] = ".ppt",
        ["application/vnd.openxmlformats-officedocument.presentationml.presentation"] = ".pptx",
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/bmp"] = ".bmp",
        ["image/svg+xml"] = ".svg",
        ["image/webp"] = ".webp",
        ["text/plain"] = ".txt",
        ["text/html"] = ".html",
        ["text/css"] = ".css",
        ["text/csv"] = ".csv",
        ["audio/mpeg"] = ".mp3",
        ["audio/wav"] = ".wav",
        ["video/mp4"] = ".mp4",
        ["video/x-msvideo"] = ".avi",
        ["video/x-matroska"] = ".mkv"
    };

    public static string ContentTypeToExtension(string? contentType)
    {
        if (contentType is not null && mimeMap.TryGetValue(contentType, out var extension)) {
            return extension;
        }
        return string.Empty;
    }

    public static string ExtensionToContentType(string extension)
    {
        var extensionValue = $".{extension.TrimStart('.')}";
        var contentType = mimeMap.Where(r => r.Value == extensionValue).Select(r => r.Key).FirstOrDefault();

        return contentType ?? "application/octet-stream";
    }
}
