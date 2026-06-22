using System.Text.RegularExpressions;

namespace CraftSwap.Services;

/// <summary>
/// 关键词高亮服务实现
/// 属于结果组装层，负责对搜索结果进行关键词高亮处理
/// </summary>
public class HighlightService : IHighlightService
{
    /// <summary>
    /// 对文本进行关键词高亮
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="preTag">高亮前缀标签</param>
    /// <param name="postTag">高亮后缀标签</param>
    /// <returns>高亮后的文本，如果没有匹配则返回null</returns>
    public string? Highlight(string? text, string? keyword, string preTag = "<em>", string postTag = "</em>")
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        var escapedKeyword = Regex.Escape(keyword.Trim());
        var pattern = $"({escapedKeyword})";
        var replacement = $"{preTag}$1{postTag}";

        var highlighted = Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);

        return highlighted == text ? null : highlighted;
    }

    /// <summary>
    /// 对标签列表进行关键词高亮
    /// </summary>
    /// <param name="tags">原始标签列表</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="preTag">高亮前缀标签</param>
    /// <param name="postTag">高亮后缀标签</param>
    /// <returns>高亮后的标签列表，如果没有匹配则返回null</returns>
    public List<string>? HighlightTags(List<string>? tags, string? keyword, string preTag = "<em>", string postTag = "</em>")
    {
        if (tags == null || tags.Count == 0 || string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        var escapedKeyword = Regex.Escape(keyword.Trim());
        var pattern = $"({escapedKeyword})";
        var replacement = $"{preTag}$1{postTag}";

        var highlightedTags = new List<string>();
        bool hasMatch = false;

        foreach (var tag in tags)
        {
            var highlightedTag = Regex.Replace(tag, pattern, replacement, RegexOptions.IgnoreCase);
            highlightedTags.Add(highlightedTag);
            if (highlightedTag != tag)
            {
                hasMatch = true;
            }
        }

        return hasMatch ? highlightedTags : null;
    }
}
