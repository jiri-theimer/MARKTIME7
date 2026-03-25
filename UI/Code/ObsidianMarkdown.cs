using Markdig;
using System.Net;
using System.Text.RegularExpressions;

namespace UI.Code
{


public static class ObsidianMarkdown
{
    public static string ToHtml(string mdFilePath, string vaultPath, string webRootPath,string dirName)
    {
        var markdown = File.ReadAllText(mdFilePath);
        markdown = ReplaceObsidianImages(markdown, vaultPath, webRootPath, dirName);
        return Markdown.ToHtml(markdown);
    }

    private static string ReplaceObsidianImages(string markdown, string vaultPath, string webRootPath,string dirName)
    {
        return Regex.Replace(markdown, @"!\[\[([^\]]+)\]\]", match =>
        {
            var raw = match.Groups[1].Value.Trim();
            var imagePath = raw.Split('|')[0].Trim();

            if (!IsImage(imagePath))
                return match.Value;

            var fileName = Path.GetFileName(imagePath);
            CopyImage(imagePath, vaultPath, webRootPath);

            return $"<br><img class=\"zoomable\" src=\"/_help/{dirName}/images/{WebUtility.HtmlEncode(fileName)}\" alt=\"{WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(fileName))}\"><br>";
        });
    }

    private static void CopyImage(string relativeImagePath, string vaultPath, string webRootPath)
    {
        var source = FindFile(relativeImagePath, vaultPath);
        if (source is null)
            return;

        var imagesDir = Path.Combine(webRootPath, "images");
        Directory.CreateDirectory(imagesDir);

        var target = Path.Combine(imagesDir, Path.GetFileName(source));
        File.Copy(source, target, true);
    }

    private static string? FindFile(string relativePath, string vaultPath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);

        var direct = Path.Combine(vaultPath, normalized);
        if (File.Exists(direct))
            return direct;

        var fileName = Path.GetFileName(relativePath);
        return Directory.EnumerateFiles(vaultPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
    }

    private static bool IsImage(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg";
    }
}


}