function translateCode() {
  // Obtiene el texto de entrada desde el textarea
  var unityScript = document.getElementById("unityScript").value;

  // Realiza la traducción de campos y funciones
  var fields = translateFields(unityScript);

  document.getElementById("csharp").value = fields;
}

function translateFields(input) {
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
  var output = input.replace(/(var|public var|private var|protected var)\s+(\w+)\s*:\s*([\w.]+)(.*?)(?=;)/g, replaceVariable);

  // -----------------------------------------------
  // REEMPLAZAR TIPOS QUE NO HAYAN SIDO REEMPLAZADOS
  // -----------------------------------------------

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

  output = output.replace(/(var|public var|private var|protected var)\s+(\w+)\s*=\s*([^;]+)(.*?)(?=;)/g, replaceType);

  // Añadir f al final de los números decimales
  output = output.replace(/(\d+|)\.(\d+)/g, "$1.$2f");

  // Reemplazar la declaración de enum para asegurar que sea pública si no tiene decorador de accesibilidad
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


  // output = replaceFuncSignature(output);

  // // Reemplazar la declaración de función
  // output = output.replace(
  //   /function\s+(\w+)\s*\(\s*((\w+)\s*:\s*(\w+)\s*(,|)\s*)*\s*\)\s*(:|)\s*(\w+)/g,
  //   function (match, functionName, parameters, returnType) {
  //     console.log('[Function]\n',
  //       'match: ', match + '\n',
  //       'functionName: ', functionName + '\n',
  //       'parameters: ', parameters + '\n',
  //       'returnType: ', returnType + '\n');

  //     // Construir la declaración de función en C#
  //     var csharpDeclaration =
  //       "public " +
  //       (!returnType ? 'void' : returnType) +
  //       " " +
  //       functionName +
  //       "(" +
  //       (!parameters ? '' : parameters) +
  //       ")";
  //     return csharpDeclaration;
  //   }
  // );



  // Generic types
  output = output.replace(/(\w+)\s*\.\s*<\s*(\w+)\s*>\s*\(\)/g, '$1<$2>()');

  output = output.replace(/function/g, 'void');
  output = output.replace(/^#pragma\b.*[\r\n]+/gm, '');
  output = output.replace(/boolean/g, "bool");

  return output;
}

// function debugFunction(input = undefined) {
//   const regex = /function\s+(\w+)\s*\(\s*((?:\w+\s*:\s*\w+\s*(?:,|\s)*)*)\)\s*(?::\s*(\w+))?/;

//   input = input || "function Interact (x: RaycastHit, y: Hit, a: b): Color";

//   const matches = input.match(regex);

//   if (matches) {
//     const [, funcName, paramGroup, returnType] = matches;

//     const paramsRegex = /(\w+)\s*:\s*(\w+)/g;
//     let paramMatches;
//     const params = [];

//     while ((paramMatches = paramsRegex.exec(paramGroup)) !== null) {
//       const [, paramName, paramType] = paramMatches;
//       params.push({ paramName, paramType });
//     }

//     console.log("Function Name:", funcName);
//     console.log("Parameters:", params);
//     console.log("Return Type:", returnType);
//   }
// }

// function replaceFuncSignature(input) {
//   // // const input = "function Interact (x: RaycastHit, y: Hit, a: b): Color";
//   // const input = "function Interact ()";

//   // Utilizamos la expresión regular para capturar los grupos necesarios
//   const regex = /function\s+(\w+)\s*\(\s*((?:\w+\s*:\s*\w+\s*(?:,|\s)*)*)\)\s*(:|)\s*(\w+)?/;
//   const matches = input.match(regex);

//   if (matches) {
//     // Capturamos los grupos individuales
//     const [, funcName, paramGroup, returnType] = matches;

//     // Reemplazamos la cadena utilizando los grupos capturados
//     const output = `${(returnType || 'void')} ${funcName}(${paramGroup
//       .replace(/(\w+)\s*:\s*(\w+)/g, "$2 $1")
//       .replace(/:\s*/g, "")
//       .replace(/,/g, ", ")})`;

//     return output;
//   }

//   return input;
// }

function translateFunctionBody(functionBody) {
  // You can implement your own logic here to translate the function body
  // This is just a placeholder implementation that leaves the body unchanged
  return functionBody;
}