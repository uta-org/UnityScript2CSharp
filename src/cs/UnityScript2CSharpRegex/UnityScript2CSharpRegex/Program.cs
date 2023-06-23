// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");


// TODO: output path

using System.IO.Compression;
using System.Text.RegularExpressions;
using UnityScript2CSharpRegex;
using static UnityScript2CSharpRegex.Properties.Resources;

//Console.WriteLine(UnityScript2CSharp.TranslateCode(Test));

// var outputPath = @"A:\VIZZUTA\Unity\Tests\dummy-shooter\Assets\ThirdParty\Weapons";
void TranslateProject()
{
    var outputPath = @"C:\Users\arodriguezg\Unity3D\Dummy-Shooter\Assets\ThirdParty\Weapons";

    var zipData = FPS_Constructor_V1;
    using var memoryStream = new MemoryStream(zipData);
    using var archive = new ZipArchive(memoryStream);
    foreach (var entry in archive.Entries)
    {
        // Verificar si la entrada es un archivo (no un directorio)
        if (!entry.FullName.EndsWith("/"))
        {
            // Extraer el nombre del archivo
            var fileName = Path.GetFileNameWithoutExtension(entry.FullName);

            // Leer el contenido del archivo
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            var content = reader.ReadToEnd();

            var diskFile = Path.Combine(outputPath, entry.FullName);

            var bak = diskFile + ".bak";
            if (!File.Exists(bak))
                File.Move(diskFile, bak);

            var cs = diskFile.Replace(".js", ".cs");
            var code = UnityScript2CSharp.TranslateCode(content, fileName);

            File.WriteAllText(cs, code);

            Console.WriteLine($"Done {fileName} at {cs}!");

            //Console.WriteLine($"{entry.FullName}: {File.Exists(diskFile)}");
            //Console.WriteLine($"Contenido de {fileName}:\n{content}");
        }
    }
}

void TranslateExample()
{
    var input = @"
var length1 : float;
var width1 : float;
var scale : boolean = false;

function Update(a: b, c: d){
	var temp : float;
	var temp2 : float;
	
	var hit : RaycastHit;
	var layerMask = 1 << PlayerWeapons.playerLayer;
}
";

    Console.WriteLine(UnityScript2CSharp.TranslateCode(input));

    //// Patrón para encontrar las variables var dentro de la función
    //var patron = @"function\s+\w+\(.*\)\s*{(.*?)^}";

    //// Coincidencia para la función Update
    //var coincidencia = Regex.Match(input, patron, RegexOptions.Singleline | RegexOptions.Multiline);

    //if (coincidencia.Success)
    //{
    //    // Obtener el bloque de la función Update
    //    var bloqueUpdate = coincidencia.Groups[1].Value;

    //    // Patrón para encontrar las variables var
    //    var patronVariables = @"var\s+(\w+)\s*(:\s*(\w+))?(?:\s*=\s*(.*?))?;";
    //    //                         var\s+(\w+)\s*(:\s*(\w+))?\s*=\s*(.*?);


    //    // Coincidencias para las variables var
    //    var coincidenciasVariables = Regex.Matches(bloqueUpdate, patronVariables, RegexOptions.Multiline);

    //    foreach (Match coincidenciaVariable in coincidenciasVariables)
    //    {
    //        var nombreVariable = coincidenciaVariable.Groups[1].Value;
    //        var tipoVariable = coincidenciaVariable.Groups[2].Value;
    //        var valorVariable = coincidenciaVariable.Groups[3].Value;

    //        Console.WriteLine($"Variable: {nombreVariable}, Tipo: {tipoVariable}, Valor: {valorVariable}");
    //    }
    //}
    //else
    //{
    //    Console.WriteLine("No se encontró la función Update");
    //}
}

TranslateExample();
//TranslateProject();