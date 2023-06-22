function translateCode() {
    // Obtiene el texto de entrada desde el textarea
    var unityScript = document.getElementById("unityScript").value;

    // Realiza la traducción de campos y funciones
    var fields = translateFields(unityScript);

    document.getElementById("csharp").value = fields;
  }

  function translateFields(input) {
    // Expresión regular para encontrar la declaración de variables en UnityScript
    var regex =
      /(var|public var|private var|protected var)\s+(\w+)\s*:\s*([\w.]+)(.*?)(?=;)/g;
    // var regex = /(var|public var|private var|protected var)\s+(\w+)\s*=\s*([^;]+)(.*?)(?=;)/g;

    // Función de reemplazo para convertir la declaración de variables a C#
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

      // Determinar si el campo es público
      var isPublic = visibility.includes("public") || visibility === "var";
      // Determinar si el campo es privado
      var isPrivate = visibility.includes("private");

      // Determinar el sufijo de tipo decimal
      var typeSuffix = "";

      // Verificar si se debe usar "new" para inicialización
      var shouldUseNew =
        !["int", "float", "string", "boolean"].includes(type) &&
        rest.includes("=") &&
        !/[A-Za-z]+\.[A-Za-z]+/.test(rest);

      // Construir la declaración de la variable en C#
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

    // Aplicar la función de reemplazo a la entrada usando la expresión regular
    var output = input.replace(regex, replaceVariable);

    output = output.replace(/boolean/g, "bool");

    //////////// REEMPLAZAR TIPOS QUE NO HAYAN SIDO REEMPLAZADOS
    //////////// ----------------------------------------------
    var regexType =
      /(var|public var|private var|protected var)\s+(\w+)\s*=\s*([^;]+)(.*?)(?=;)/g;

    // Función de reemplazo para convertir la declaración de variables a C#
    function replaceType(match, visibility, name, value, rest) {
      // Determinar si el campo es público
      var isPublic = visibility.includes("public") || visibility === "var";
      // Determinar si el campo es privado
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

      // Determinar el tipo de variable basado en su valor
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
        type = "unknown"; // Tipo desconocido
      }

      // Construir la declaración de la variable en C#
      var csharpDeclaration = "";
      if (isPublic) {
        csharpDeclaration += "public ";
      } else if (isPrivate) {
        csharpDeclaration += "private ";
      }
      csharpDeclaration += type + " " + name + " = " + value + rest;

      return csharpDeclaration;
    }

    output = output.replace(regexType, replaceType);

    // Añadir f al final de los números decimales
    output = output.replace(/(\d+|)\.(\d+)/g, "$1.$2f");

    // Reemplazar la declaración de enum para asegurar que sea pública si no tiene decorador de accesibilidad
    output = output.replace(/enum (\w+)/g, "public enum $1");

    output = output.replace(/new\s{2,}(\w+)/g, "new $1");

    // ---------------------------------------------------------
    // FUNCTIONS
    // ---------------------------------------------------------

    // Reemplazar la declaración de función
    output = output.replace(
      /function\s+(\w+)\s*\((.*?)\)\s*:\s*([\w.]+)\s*{/g,
      function (match, functionName, parameters, returnType) {
        console.log('[Function]\n',
        'match: ', match + '\n', 
        'functionName: ', functionName + '\n', 
        'parameters: ', parameters + '\n', 
        'returnType: ', returnType + '\n');

        // Construir la declaración de función en C#
        var csharpDeclaration =
          "public " +
          returnType +
          " " +
          functionName +
          "(" +
          parameters +
          ") {";
        return csharpDeclaration;
      }
    );

    // // Reemplazar los tipos de UnityScript por sus equivalentes en C#
    // output = output.replace(/:\s*([\w.]+)\s*/g, function (match, type) {
    //   // Reemplazar el tipo de UnityScript por su equivalente en C#
    //   // var csharpType = convertUnityScriptTypeToCSharp(type);
    //   return ": " + type + " ";
    // });

    output = output.replace(/function/g, 'void');
    // output = output.replace(/#pragma$/g, '');

    return output;
  }

  function translateFunctionBody(functionBody) {
    // You can implement your own logic here to translate the function body
    // This is just a placeholder implementation that leaves the body unchanged
    return functionBody;
  }