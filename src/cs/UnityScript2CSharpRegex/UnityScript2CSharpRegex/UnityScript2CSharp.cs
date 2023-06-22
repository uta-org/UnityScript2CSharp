using System.Text;
using System.Text.RegularExpressions;

namespace UnityScript2CSharpRegex
{
    public static class UnityScript2CSharp
    {
        public static string TranslateCode(string unityScript, string? fileName = null)
        {
            // Perform translation of fields and functions
            var code = Translate(unityScript);

            code =
                $"public class {fileName.GetString("{ReplaceMe}")} : MonoBehaviour\n{{\n\t{code.Replace("\n", "\n\t")}\n}}";

            return code;
        }

        private static string Translate(string input)
        {
            // Replacement function to convert variable declaration to C#
            string ReplaceVariable(Match match)
            {
                var visibility = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var type = match.Groups[3].Value;
                var rest = match.Groups[4].Value;

                // Determine if the field is public
                var isPublic = visibility.Contains("public") || visibility == "var";
                // Determine if the field is private
                var isPrivate = visibility.Contains("private");

                // Determine the decimal type suffix
                var typeSuffix = "";

                // Check if "new" should be used for initialization
                var shouldUseNew = !new[] { "int", "float", "string", "bool" }.Contains(type) &&
                                   rest.Contains("=") &&
                                   !Regex.IsMatch(rest, @"[A-Za-z]+\.[A-Za-z]+");

                // Construct the variable declaration in C#
                var csharpDeclaration = new StringBuilder();
                if (isPublic)
                {
                    csharpDeclaration.Append("public ");
                }
                else if (isPrivate)
                {
                    csharpDeclaration.Append("private ");
                }

                if (shouldUseNew && rest.Contains("="))
                {
                    rest = rest.Replace("=", "");
                }

                csharpDeclaration.Append(type)
                    .Append(" ")
                    .Append(name)
                    .Append(shouldUseNew ? " = new " : "")
                    .Append(typeSuffix)
                    .Append(rest);

                return csharpDeclaration.ToString();
            }

            // Apply the replacement function to the input using the regular expression
            var output = Regex.Replace(input,
                @"(var|public var|private var|protected var)\s+(\w+)\s*:\s*([\w.]+)(.*?)(?=;)", ReplaceVariable);

            // -----------------------------------------------
            // REPLACE TYPES THAT WERE NOT REPLACED
            // -----------------------------------------------

            // Replacement function to convert variable declaration to C#
            string ReplaceType(Match match)
            {
                var visibility = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var value = match.Groups[3].Value;
                var rest = match.Groups[4].Value;

                // Determine if the field is public
                var isPublic = visibility.Contains("public") || visibility == "var";
                // Determine if the field is private
                var isPrivate = visibility.Contains("private");

                // Determine the variable type based on its value
                var type = "";
                if (Regex.IsMatch(value, @"^(-|)\d+$"))
                {
                    type = "int";
                }
                else if (Regex.IsMatch(value, @"^\d*\.\d+$"))
                {
                    type = "float";
                }
                else if (value == "true" || value == "false")
                {
                    type = "bool";
                }
                else if (Regex.IsMatch(value, @"^[""].*[""]$"))
                {
                    type = "string";
                }
                else
                {
                    type = "unknown"; // Unknown type
                }

                // Construct the variable declaration in C#
                var csharpDeclaration = new StringBuilder();
                if (isPublic)
                {
                    csharpDeclaration.Append("public ");
                }
                else if (isPrivate)
                {
                    csharpDeclaration.Append("private ");
                }

                csharpDeclaration.Append(type)
                    .Append(" ")
                    .Append(name)
                    .Append(" = ")
                    .Append(value)
                    .Append(rest);

                return csharpDeclaration.ToString();
            }

            output = Regex.Replace(output,
                @"(var|public var|private var|protected var)\s+(\w+)\s*=\s*([^;]+)(.*?)(?=;)", ReplaceType);

            // Add 'f' suffix to decimal numbers
            output = Regex.Replace(output, @"(\d+|)\.(\d+)", "$1.$2f");

            // Replace enum declaration to ensure it is public if it doesn't have an accessibility modifier
            output = Regex.Replace(output, @"enum (\w+)", "public enum $1");
            output = Regex.Replace(output, @"new\s{2,}(\w+)", "new $1");

            // ---------
            // FUNCTIONS
            // ---------

            // debugFunction(output);

            output = Regex.Replace(output, @"function\s+(\w+)\s*\(\s*((?:\w+\s*:\s*\w+\s*(?:,|\s)*)*)\)\s*:*\s*(\w+)?",
                match =>
                {
                    var funcName = match.Groups[1].Value;
                    var paramGroup = match.Groups[2].Value;
                    var returnType = match.Groups[3].Value;

                    Console.WriteLine("[Functions]\n" +
                                      "match: " + match.Value + "\n" +
                                      "funcName: " + funcName + "\n" +
                                      "paramGroup: " + paramGroup + "\n" +
                                      "returnType: " + returnType);

                    var replacedParamGroup = Regex.Replace(paramGroup, @"(\w+)\s*:\s*(\w+)", "$2 $1")
                        .Replace(":", "")
                        .Replace(",", ", ");

                    return $"{returnType.GetString("void")} {funcName}({replacedParamGroup})";
                });

            // Generic types
            output = Regex.Replace(output, @"(\w+)\s*\.\s*<\s*(\w+)\s*>\s*\(\)", "$1<$2>()");

            output = output.Replace("function", "void");
            output = Regex.Replace(output, @"^#pragma\b.*[\r\n]+", "");
            output = output.Replace("boolean", "bool");

            return output;
        }

        private static string GetString(this string str, string rep)
        {
            return string.IsNullOrEmpty(str) ? rep : str;
        }
    }
}
