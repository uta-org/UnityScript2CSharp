function translateCode() {
  // Get the input text from the textarea
  var unityScript = document.getElementById("unityScript").value;

  // Perform translation of fields and functions
  var fields = translateFields(unityScript);

  document.getElementById("csharp").value = fields;
}

function translateFields(input) {
  // Replacement function to convert variable declaration to C#
  function replaceVariable(match, visibility, name, type, rest) {
    console.log(
      "[Variables] ",
      match + "\n",
      "visibility: ",
      visibility + "\n",
      "name: ",
      name + "\n",
      "type: ",
      type + "\n",
      "rest: ",
      rest + "\n"
    );

    // Determine if the field is public
    var isPublic = visibility.includes("public") || visibility === "var";
    // Determine if the field is private
    var isPrivate = visibility.includes("private");

    // Determine the decimal type suffix
    var typeSuffix = "";

    // Check if "new" should be used for initialization
    var shouldUseNew =
      !["int", "float", "string", "boolean"].includes(type) &&
      rest.includes("=") &&
      !/[A-Za-z]+\.[A-Za-z]+/.test(rest);

    // Construct the variable declaration in C#
    var csharpDeclaration = "";
    if (isPublic) {
      csharpDeclaration += "public ";
    } else if (isPrivate) {
      csharpDeclaration += "private ";
    }

    if (shouldUseNew && rest.includes("=")) rest = rest.replace("=", "");
    csharpDeclaration +=
      type +
      " " +
      name +
      (shouldUseNew ? " = new " : "") +
      typeSuffix +
      rest;

    return csharpDeclaration;
  }

  // Apply the replacement function to the input using the regular expression
  var output = input.replace(/(var|public var|private var|protected var)\s+(\w+)\s*:\s*([\w.]+)(.*?)(?=;)/g, replaceVariable);

  // -----------------------------------------------
  // REPLACE TYPES THAT WERE NOT REPLACED
  // -----------------------------------------------

  // Replacement function to convert variable declaration to C#
  function replaceType(match, visibility, name, value, rest) {
    // Determine if the field is public
    var isPublic = visibility.includes("public") || visibility === "var";
    // Determine if the field is private
    var isPrivate = visibility.includes("private");

    console.log(
      "[Types] ",
      match + "\n",
      "visibility: ",
      visibility + "\n",
      "name: ",
      name + "\n",
      "value: ",
      value + "\n",
      "rest: ",
      rest + "\n"
    );

    // Determine the variable type based on its value
    var type = "";
    if (/^(-|)\d+$/.test(value)) {
      type = "int";
    } else if (/^\d*\.\d+$/.test(value)) {
      type = "float";
    } else if (value === "true" || value === "false") {
      type = "bool";
    } else if (/^["'].*["']$/.test(value)) {
      type = "string";
    } else {
      type = "unknown"; // Unknown type
    }

    // Construct the variable declaration in C#
    var csharpDeclaration = "";
    if (isPublic) {
      csharpDeclaration += "public ";
    } else if (isPrivate) {
      csharpDeclaration += "private ";
    }
    csharpDeclaration += type + " " + name + " = " + value + rest;

    return csharpDeclaration;
  }

  output = output.replace(/(var|public var|private var|protected var)\s+(\w+)\s*=\s*([^;]+)(.*?)(?=;)/g, replaceType);

  // Add 'f' suffix to decimal numbers
  output = output.replace(/(\d+|)\.(\d+)/g, "$1.$2f");

  // Replace enum declaration to ensure it is public if it doesn't have an accessibility modifier
  output = output.replace(/enum (\w+)/g, "public enum $1");

  output = output.replace(/new\s{2,}(\w+)/g, "new $1");

  // ---------
  // FUNCTIONS
  // ---------

  // debugFunction(output);

  output = output.replace(/function\s+(\w+)\s*\(\s*((?:\w+\s*:\s*\w+\s*(?:,|\s)*)*)\)\s*:*\s*(\w+)?/g,
    (match, funcName, paramGroup, returnType) => {
      console.log('[Functions]\n',
        'match', match + '\n',
        'funcName', funcName + '\n',
        'paramGroup', paramGroup + '\n',
        'returnType', returnType);

      const replacedParamGroup = paramGroup
        .replace(/(\w+)\s*:\s*(\w+)/g, "$2 $1")
        .replace(/:\s*/g, "")
        .replace(/,/g, ", ");

      return `${(returnType || 'void')} ${funcName}(${replacedParamGroup})`;
    });

  // Generic types
  output = output.replace(/(\w+)\s*\.\s*<\s*(\w+)\s*>\s*\(\)/g, '$1<$2>()');

  output = output.replace(/function/g, 'void');
  output = output.replace(/^#pragma\b.*[\r\n]+/gm, '');
  output = output.replace(/boolean/g, "bool");

  return output;
}
