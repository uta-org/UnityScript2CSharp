// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");


// TODO: output path

using System.IO.Compression;
using UnityScript2CSharpRegex;
using static UnityScript2CSharpRegex.Properties.Resources;

//Console.WriteLine(UnityScript2CSharp.TranslateCode(Test));

void TranslateProject()
{
    var outputPath = "C:\\Users\\arodriguezg\\Unity3D\\Dummy-Shooter\\Assets\\ThirdParty\\Weapons";
    //var outputPath = "A:\\VIZZUTA\\Unity\\Tests\\dummy-shooter\\Assets\\ThirdParty\\Weapons";

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
function getWeaponsOwned(slot : int) {
    var w : WeaponInfo[] = new WeaponInfo[getNumOwned(slot)];
    var n : int = 0;
    for (var i: int = 0; i <  WeaponInfoArray.length; i++) {
        if(WeaponInfoArray[i].owned && slotInfo.isWeaponAllowed(slot,WeaponInfoArray[i])){
            w[n] = WeaponInfoArray[i];
            n++;
        }
    }
    return w;
}

function getWeaponsOwned(slot : int) : WeaponInfo {
    var w : WeaponInfo[] = new WeaponInfo[getNumOwned(slot)];
    var n : int = 0;
    for (var i: int = 0; i <  WeaponInfoArray.length; i++) {
        if(WeaponInfoArray[i].owned && slotInfo.isWeaponAllowed(slot,WeaponInfoArray[i])){
            w[n] = WeaponInfoArray[i];
            n++;
        }
    }
    return w;
}

function getWeaponsOwned(slot : int) : WeaponInfo[] {
    var w : WeaponInfo[] = new WeaponInfo[getNumOwned(slot)];
    var n : int = 0;
    for (var i: int = 0; i <  WeaponInfoArray.length; i++) {
        if(WeaponInfoArray[i].owned && slotInfo.isWeaponAllowed(slot,WeaponInfoArray[i])){
            w[n] = WeaponInfoArray[i];
            n++;
        }
    }
    return w;
}";

    // TODO: public Quaternion, maybe because of the identation, but isn't detected
    Console.WriteLine(UnityScript2CSharp.TranslateCode(input));
}

//TranslateExample();
TranslateProject();