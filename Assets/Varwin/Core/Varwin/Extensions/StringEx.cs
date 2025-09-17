using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

public static class StringEx
{
    public static string SubstringAfter(this string str, string value)
    {
        int index = str.IndexOf(value);
        if (index < 0)
        {
            return str;
        }
        return str.Substring(index, str.Length - index);
    }
    
    public static string SubstringAfterLast(this string str, string value)
    {
        int index = str.LastIndexOf(value);
        if (index < 0)
        {
            return str;
        }
        return str.Substring(index, str.Length - index);
    }

    public static string ClearString(this string str, char[] charsToClean)
    {
        foreach (var symbol in charsToClean)
        {
            str = str.Replace(symbol.ToString(), "");
        }

        return str;
    }

    public static string SubstringBefore(this string str, string value)
    {
        return str.Substring(0, str.IndexOf(value));
    }
    
    public static string SubstringBeforeLast(this string str, string value)
    {
        return str.Substring(0, str.LastIndexOf(value));
    }
    
    public static string ToEscaped(this string input)
    {
#if NET_STANDARD_2_0
        return input.Replace("\"", "\\\"");
#else
        var sourceCodeString = input.GenerateSourceCodeString();
        string escapedStringWithoutWrappingQuotes = sourceCodeString.Substring(1, sourceCodeString.Length - 2);

        return escapedStringWithoutWrappingQuotes;
#endif
    }

    public static string GenerateSourceCodeString(this string input)
    {
#if NET_STANDARD_2_0
        return input.Replace("\"", "\\\"");
#else
        using (var writer = new StringWriter())
        {
            using (var provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                return writer.ToString();
            }
        }
#endif
    }
    
    /// <summary>
    /// Replace a string char at index with another char
    /// </summary>
    /// <param name="text">string to be replaced</param>
    /// <param name="index">position of the char to be replaced</param>
    /// <param name="c">replacement char</param>
    public static string ReplaceAtIndex(this string text, int index, char c)
    {
        return new StringBuilder(text)
        {
            [index] = c
        }.ToString();
    }
}