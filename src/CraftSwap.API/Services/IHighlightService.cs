namespace CraftSwap.Services;

/// <summary>
/// 关键词高亮服务接口
/// 属于结果组装层，负责对搜索结果进行关键词高亮处理
/// </summary>
public interface IHighlightService
{
    /// <summary>
    /// 对文本进行关键词高亮
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="preTag">高亮前缀标签</param>
    /// <param name="postTag">高亮后缀标签</param>
    /// <returns>高亮后的文本，如果没有匹配则返回null</returns>
    string? Highlight(string? text, string? keyword, string preTag = "<em>", string postTag = "</em>");

    /// <summary>
    /// 对标签列表进行关键词高亮
    /// </summary>
    /// <param name="tags">原始标签列表</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="preTag">高亮前缀标签</param>
    /// <param name="postTag">高亮后缀标签</param>
    /// <returns>高亮后的标签列表，如果没有匹配则返回null</returns>
    List<string>? HighlightTags(List<string>? tags, string? keyword, string preTag = "<em>", string postTag = "</em>");
}
