﻿// See https://aka.ms/new-console-template for more information
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
function Start() {
	if (!isPrimaryWeapon) {
		gunActive = false;
		var wpns = new Array();
		wpns = this.GetComponents(GunScript);
		for (var p: int = 0; p < wpns.length; p++) {
			var g: GunScript = wpns[p] as GunScript;
			if (g.isPrimaryWeapon) {
				primaryWeapon = g;
			}
		}
	}
}
";

    Console.WriteLine(UnityScript2CSharp.TranslateCode(input));
}

TranslateExample();
//TranslateProject();