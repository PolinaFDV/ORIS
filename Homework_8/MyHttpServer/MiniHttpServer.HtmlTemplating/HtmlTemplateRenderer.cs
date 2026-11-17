using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Net;

namespace MiniHttpServer.HtmlTemplating;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        if (string.IsNullOrEmpty(htmlTemplate)) return htmlTemplate;
        
        string result = ProcessBlocks(htmlTemplate, dataModel, null);
        return ReplaceVariables(result, dataModel);
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        string template = File.ReadAllText(filePath);
        return RenderFromString(template, dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        string result = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, result);
        return result;
    }

    private string ProcessBlocks(string template, object dataModel, object? localScope)
    {
        var foreachMatch = Regex.Match(template, @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+([\w\.]+)\s*\)");
        var ifMatch = Regex.Match(template, @"\$if\s*\(\s*([^)]+)\s*\)");

        if (foreachMatch.Success && (!ifMatch.Success || foreachMatch.Index < ifMatch.Index))
        {
            return ProcessForeach(template, foreachMatch, dataModel, localScope);
        }
        else if (ifMatch.Success)
        {
            return ProcessIf(template, ifMatch, dataModel, localScope);
        }

        return template;
    }

    private string ProcessIf(string template, Match match, object dataModel, object? localScope)
    {
        int startIndex = match.Index;
        int endOfStartTag = match.Index + match.Length;
        
        int endifIndex = FindMatchingTag(template, endOfStartTag, "$if", "$endif");
        
        if (endifIndex == -1) throw new Exception("Синтаксическая ошибка: отсутствует $endif");

        int elseIndex = template.IndexOf("$else", endOfStartTag);
        
        if (elseIndex != -1 && elseIndex > endifIndex)
        {
            elseIndex = -1;
        }

        string condition = match.Groups[1].Value.Trim();
        bool isTrue = EvaluateCondition(condition, dataModel, localScope);

        string trueBlock, falseBlock, replacement;

        if (elseIndex != -1)
        {
            trueBlock = template.Substring(endOfStartTag, elseIndex - endOfStartTag);
            falseBlock = template.Substring(elseIndex + 5, endifIndex - (elseIndex + 5));
            replacement = isTrue ? trueBlock : falseBlock;
        }
        else
        {
            trueBlock = template.Substring(endOfStartTag, endifIndex - endOfStartTag);
            replacement = isTrue ? trueBlock : "";
        }

        replacement = ProcessBlocks(replacement, dataModel, localScope);

        int endBlock = endifIndex + 6;
        string result = template.Substring(0, startIndex) + replacement + template.Substring(endBlock);

        return ProcessBlocks(result, dataModel, localScope);
    }

    private string ProcessForeach(string template, Match match, object dataModel, object? localScope)
    {
        int startIndex = match.Index;
        int endOfStartTag = match.Index + match.Length;
        
        int endforIndex = FindMatchingTag(template, endOfStartTag, "$foreach", "$endfor");
        
        if (endforIndex == -1) throw new Exception("Синтаксическая ошибка: отсутствует $endfor");

        string itemName = match.Groups[1].Value;
        string collectionName = match.Groups[2].Value; 
        string block = template.Substring(endOfStartTag, endforIndex - endOfStartTag);

        object? collection = null;
        
        if (localScope != null)
        {
            string pathToSearch = collectionName;
            if (pathToSearch.Contains('.'))
            {
                pathToSearch = pathToSearch.Substring(pathToSearch.IndexOf('.') + 1);
            }
            collection = GetValueByPath(localScope, pathToSearch);
        }

        if (collection == null)
        {
            collection = GetValueByPath(dataModel, collectionName);
        }
                             
        string result = "";

        if (collection is IEnumerable enumerable && collection is not string)
        {
            foreach (var item in enumerable)
            {
                if (item == null) continue;
                
                string currentBlock = block;
                
                currentBlock = Regex.Replace(currentBlock, @"\$\{" + itemName + @"\.(\w+)\}", m => {
                    string prop = m.Groups[1].Value;
                    object? val = GetPropertyValue(item, prop);
                    return EncodeHtml(val?.ToString() ?? "");
                });

                currentBlock = currentBlock.Replace("${" + itemName + "}", EncodeHtml(item.ToString() ?? ""));

                result += ProcessBlocks(currentBlock, dataModel, item);
            }
        }

        int endBlock = endforIndex + 7;
        string processed = template.Substring(0, startIndex) + result + template.Substring(endBlock);

        return ProcessBlocks(processed, dataModel, localScope);
    }

    private string ReplaceVariables(string template, object dataModel)
    {
        return Regex.Replace(template, @"\$\{([^}]+)\}", match =>
        {
            string expr = match.Groups[1].Value.Trim();
            object? value = GetValueByPath(dataModel, expr);
            return EncodeHtml(value?.ToString() ?? "");
        });
    }

    private object? GetValueByPath(object dataModel, string path)
    {
        string[] parts = path.Split('.');
        object? current = dataModel;
        foreach (var part in parts)
        {
            if (current == null) return null;
            current = GetPropertyValue(current, part);
        }
        return current;
    }

    private object? GetPropertyValue(object obj, string propertyName)
    {
        PropertyInfo? prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return prop?.GetValue(obj);
    }

    private bool EvaluateCondition(string condition, object dataModel, object? localScope)
    {
        object? value = null;
        
        if (localScope != null && condition.Contains('.'))
        {
            string[] parts = condition.Split('.');
            string propPath = string.Join('.', parts.Skip(1)); 
            value = GetValueByPath(localScope, propPath);
        }
        else
        {
            value = GetValueByPath(dataModel, condition);
        }

        if (value is bool b) return b;
        
        string strValue = value?.ToString()?.ToLower() ?? "";
        return strValue != "false" && strValue != "";
    }

    private string EncodeHtml(string text) => WebUtility.HtmlEncode(text);

    private int FindMatchingTag(string template, int startIndex, string openTag, string closeTag)
    {
        int depth = 1;
        int currentIndex = startIndex;

        while (currentIndex < template.Length)
        {
            int nextOpen = template.IndexOf(openTag, currentIndex);
            int nextClose = template.IndexOf(closeTag, currentIndex);

            if (nextClose == -1) return -1;

            if (nextOpen != -1 && nextOpen < nextClose)
            {
                depth++;
                currentIndex = nextOpen + openTag.Length;
            }
            else
            {
                depth--;
                if (depth == 0) return nextClose;
                currentIndex = nextClose + closeTag.Length;
            }
        }

        return -1;
    }
}