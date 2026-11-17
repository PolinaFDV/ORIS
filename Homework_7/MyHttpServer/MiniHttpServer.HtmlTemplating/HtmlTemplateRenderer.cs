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
        var foreachMatch = Regex.Match(template, @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+(\w+)\s*\)");
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
        
        int elseIndex = template.IndexOf("$else", endOfStartTag);
        int endifIndex = template.IndexOf("$endif", endOfStartTag);

        if (endifIndex == -1) throw new Exception("Синтаксическая ошибка: отсутствует $endif");

        string condition = match.Groups[1].Value.Trim();
        
        bool isTrue = EvaluateCondition(condition, dataModel, localScope);

        string trueBlock, falseBlock, replacement;

        if (elseIndex != -1 && elseIndex < endifIndex)
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
        
        int endforIndex = template.IndexOf("$endfor", endOfStartTag);
        if (endforIndex == -1) throw new Exception("Синтаксическая ошибка: отсутствует $endfor");

        string itemName = match.Groups[1].Value; // "user"
        string collectionName = match.Groups[2].Value; // "Users"
        string block = template.Substring(endOfStartTag, endforIndex - endOfStartTag);

        object? collection = GetValueByPath(dataModel, collectionName) 
                             ?? (localScope != null ? GetValueByPath(localScope, collectionName) : null);
                             
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
}