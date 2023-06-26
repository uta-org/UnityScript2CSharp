using System.Text;
using System.Text.RegularExpressions;

namespace UnityScript2CSharpRegex
{
    public static class UnityScript2CSharp
    {
        // TODO: local variables, unassigned local vars...

        public static string TranslateCode(string unityScript, string? fileName = null)
        {
            // Perform translation of fields and functions
            var code = Translate(unityScript);

            code =
                "using System;\n" +
                "using UnityEngine;\n" +
                "using System.Collections;\n" +
                "using System.Collections.Generic;\n" +
                "using Random = UnityEngine.Random;\n" +
                "\n" +
                $"public class {fileName.GetString("{ReplaceMe}")} : MonoBehaviour\n{{\n\t{code.Replace("\n", "\n\t")}\n}}";

            return code;
        }

        private static string Translate(string input)
        {
            var multiline = RegexOptions.Singleline | RegexOptions.Multiline;

            string ReplaceVariableTypesInsideFunctions(Match match)
            {
                // Get function block and signature
                var signature = match.Groups[1].Value;
                var funcBlock = match.Groups[2].Value;

                Console.WriteLine("[Block]\n" +
                                  $"signature: {signature}\n" +
                                  "-----------------------\n" +
                                  $"funcBlock\n{funcBlock}");

                // Pattern to find vars
                var varsPattern = @"var\s+(\w+)\s*:\s*([\w.]+\[*\]*)(.*?)(?=;)";

                var blockUpdated = Regex.Replace(funcBlock, varsPattern, (m) =>
                {
                    var name = m.Groups[1].Value;
                    var type = m.Groups[2].Value;
                    var rest = m.Groups[3].Value;

                    Console.WriteLine("[ScopedVariables]\n" +
                                      $"name: {name}\n" +
                                      $"type: {type}\n" +
                                      $"rest: {rest}");

                    var shouldUseNew = ShouldUseNew(type, rest);

                    // Construct the variable declaration in C#
                    var csharpDeclaration = new StringBuilder();

                    if (shouldUseNew && rest.Contains("="))
                        rest = rest.Replace("=", "");

                    csharpDeclaration.Append(type)
                        .Append(" ")
                        .Append(name)
                        .Append(shouldUseNew ? " = new " : "")
                        .Append(rest);

                    var s = csharpDeclaration.ToString();
                    Console.WriteLine(s);
                    return s;
                }, multiline);

                return $@"{signature}{{
    {blockUpdated}
}}";
            }

            // Not working, but at least we could test them...
            //var regexFunctionBlock = @"(function\s+\w+\(.*\)\s*){(.*?)^}";
            //var regexFunctionBlock = @"(function\s+\w+\s*\(.*?\)\s*){(.*?^})";
            //var regexFunctionBlock = @"(function\s+\w+\s*\(.*?\)\s*){(.*?^})";

            //var regexFunctionBlock = @"(function\s+\w+\s*\(.*?\)\s*){([^}]*)}";
            var regexFunctionBlock = @"(function\s+\w+\s*\(.*?\)\s*:*\s*\w*\[*\]*\s*){([^}]*)}";

            var output = input;

            var mmm = Regex.Matches(input, regexFunctionBlock, RegexOptions.Multiline);
            if (mmm.Count > 0)
            {
                Console.WriteLine($"Matches count: {mmm.Count}");
                Console.WriteLine($"Matches: {string.Join("\n-------------------\n", mmm.Select(m => m.Value))}");
                output = Regex.Replace(output, regexFunctionBlock, ReplaceVariableTypesInsideFunctions, multiline);
            }
            else
                Console.WriteLine("Not matching!");

            // Replacement function to convert variable declaration to C#
            string ReplaceVariable(Match match)
            {
                var visibility = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var type = match.Groups[3].Value;
                var rest = match.Groups[4].Value;

                Console.WriteLine("[Variable]\n" +
                                  $"visibility: {visibility}\n" +
                                  $"name: {name}\n" +
                                  $"type: {type}\n" +
                                  $"rest: {rest}");

                // Determine if the field is public
                var isPublic = visibility.Contains("public") || visibility == "var";

                // Determine if the field is private
                var isPrivate = visibility.Contains("private");

                // Determine the decimal type suffix
                var typeSuffix = "";

                // Check if "new" should be used for initialization
                var shouldUseNew = ShouldUseNew(type, rest);

                // Construct the variable declaration in C#
                var csharpDeclaration = new StringBuilder();
                if (isPublic) csharpDeclaration.Append("public ");
                else if (isPrivate) csharpDeclaration.Append("private ");

                if (shouldUseNew && rest.Contains("="))
                    rest = rest.Replace("=", "");

                csharpDeclaration.Append(type)
                    .Append(" ")
                    .Append(name)
                    .Append(shouldUseNew ? " = new " : "")
                    .Append(typeSuffix)
                    .Append(rest);

                return csharpDeclaration.ToString();
            }

            // Apply the replacement function to the input using the regular expression
            output = Regex.Replace(output,
                @"(var|public var|private var|protected var)\s+(\w+)\s*:\s*([\w.]+\[*\]*)(.*?)(?=;)", ReplaceVariable);

            // ------------------------------------
            // REPLACE TYPES THAT WERE NOT REPLACED
            // ------------------------------------

            // Replacement function to convert variable declaration to C#
            string ReplaceType(Match match)
            {
                var visibility = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var value = match.Groups[3].Value;
                var rest = match.Groups[4].Value;

                var mm = match.Value;

                Console.WriteLine("[Types]\n" +
                                  $"visibility: {visibility}\n" +
                                  $"name: {name}\n" +
                                  $"value: {value}\n" +
                                  $"rest: {rest}");

                // Determine if the field is public
                var isPublic = visibility.Contains("public") || (visibility == "var" && mm.Contains(":"));
                // Determine if the field is private
                var isPrivate = visibility.Contains("private");

                // Determine the variable type based on its value
                string type;
                if (Regex.IsMatch(value, @"^(-|)\d+$"))
                    type = "int";
                else if (Regex.IsMatch(value, @"^\d*\.\d+$"))
                    type = "float";
                else if (value is "true" or "false")
                    type = "bool";
                else if (Regex.IsMatch(value, @"^[""].*[""]$"))
                    type = "string";
                else
                    type = "var"; // Unknown type

                // Construct the variable declaration in C#
                var csharpDeclaration = new StringBuilder();
                if (isPublic) csharpDeclaration.Append("public ");
                else if (isPrivate) csharpDeclaration.Append("private ");

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

            // Remove 'ff'
            output = Regex.Replace(output, @"(\d+|)\.(\d+)ff", "$1.$2f", RegexOptions.IgnoreCase);

            // Replace enum declaration to ensure it is public if it doesn't have an accessibility modifier
            output = Regex.Replace(output, @"enum (\w+)", "public enum $1");

            // Solve unneeded spaces between new declarations
            output = Regex.Replace(output, @"new\s{2,}(\w+)", "new $1");

            // Solve for(public int...
            output = Regex.Replace(output, @"for\s*\(\s*public (\w+)", "for($1");


            // ---------
            // FUNCTIONS
            // ---------

            // debugFunction(output);

            output = Regex.Replace(output, @"function\s+(\w+)\s*\(\s*((?:\w+\s*:\s*\w+\[*\]*\s*(?:,|\s)*)*)\)\s*:*\s*(\w+\[*\]*)?",
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

                    var replacedParamGroup = Regex.Replace(paramGroup, @"(\w+)\s*:\s*(\w+\[*\]*)", "$2 $1")
                        .Replace(":", "")
                        .Replace(",", ", ");

                    return $"{returnType.GetString("void")} {funcName}({replacedParamGroup})";
                });

            // Generic types
            output = Regex.Replace(output, @"(\w+)\s*\.\s*<\s*(\w+)\s*>\s*\(\)", "$1<$2>()");

            output = output.Replace("function", "void");
            output = output.Replace("boolean", "bool");

            // Remove #pragma...
            output = Regex.Replace(output, @"^#pragma\b.*[\r\n]+", "");

            // Replace GetComponent(type) to GetComponent<type>()
            output = Regex.Replace(output, @"GetComponent(\w+)*\((\w+)\)", "GetComponent$1<$2>()");

            output = Regex.Replace(output, @"AddComponent(\w+)*\((\w+)\)", "AddComponent$1<$2>()");

            // Replace attributes // TODO: fails if several with , ? (check)
            output = Regex.Replace(output, @"(?<!\/\/.*)@(?!(?:.*"")|(?:.*@.*\b))([\w\.]+)", "[$1]");

            // Replace new Array -> new List<object> // TODO: this should be improved
            output = output.Replace("new Array()", "new List<object>()");

            // Remove Array(...)
            output = Regex.Replace(output, @"Array\((.+?)\)\.ToBuiltin\(.+?\)", "$1");

            // Create a new array reference (?)
            output = Regex.Replace(output, @"\.ToBuiltin\(.+?\)", ".ToArray()");

            // Reorder static public into public static (ie)
            output = Regex.Replace(output, @"static (\w+)", "$1 static");

            // for into foreach
            output = Regex.Replace(output, @"for\s*\((.+?) in", "foreach($1 in");

            output = output.Replace(" String ", " string ");

            // Write keyword 'new', between (, +, -, * or / for Vectors, Color, Rect and Quaternion types.
            output = Regex.Replace(output, @"(\(|\*|\+|\-|\/|=)\s*(Vector.|Color|Rect|Quaternion)\s*\(", "$1 new $2(");

            output = output.Replace("( new", "(new");

            #region "Solve IEnumerator"
            var regexEnumeratorBlock = @"(void\s+\w+\s*\(.*?\)\s*){(.*?^})"; // TODO: this should be improved, because ^} is not true in case of identation

            var matches = Regex.Matches(output, regexEnumeratorBlock, multiline);
            if (matches.Count > 0)
            {
                matches.ToList().ForEach(match =>
                {
                    var signature = match.Groups[1].Value;
                    var block = match.Groups[2].Value;

                    if (!block.Contains("yield")) 
                        return;

                    // If contains yield in the function block when it means that we are inside an IEnumerator
                    var newBlock = block;
                    var newSignature = signature.Replace("void", "IEnumerator");

                    newBlock = newBlock.Replace("yield ", "yield return new ");
                    newBlock = newBlock.Replace("return;", "yield break;");

                    output = output.Replace(signature, newSignature);
                    output = output.Replace(block, newBlock);
                });
            }
            #endregion

            // Remove dupe new new
            output = output.Replace("new new ", "new ");

            return output;
        }

        private static bool ShouldUseNew(string type, string rest)
        {
            return !new[] { "int", "float", "string", "bool", "String", "boolean" }.Contains(type) &&
                   rest.Contains("=") &&
                   !Regex.IsMatch(rest, @"[A-Za-z]+\.[A-Za-z]+") &&
                   !Regex.IsMatch(rest, @"\[.+?\]") &&
                   !Regex.IsMatch(rest, @"\.");
        }

        private static string GetString(this string str, string rep)
        {
            return string.IsNullOrEmpty(str) ? rep : str;
        }
    }
}
