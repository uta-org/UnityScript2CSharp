// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");


// TODO: output path

using System.IO.Compression;
using UnityScript2CSharpRegex;
using static UnityScript2CSharpRegex.Properties.Resources;

//Console.WriteLine(UnityScript2CSharp.TranslateCode(Test));

// var outputPath = @"A:\VIZZUTA\Unity\Tests\dummy-shooter\Assets\ThirdParty\Weapons";
void TranslateProject()
{
    //var outputPath = "A:\\VIZZUTA\\Unity\\Tests\\dummy-shooter\\Assets\\ThirdParty\\Weapons";
    var outputPath = "A:\\VIZZUTA\\Unity\\Tests\\dummy-shooter\\Assets\\ThirdParty\\Weapons";

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
//FPS Constructor - Weapons
//Copyright© Dastardly Banana Productions 2010
//This script is distributed exclusively through ActiveDen and it's use is restricted to the terms of the ActiveDen 
//licensing agreement.
//
// Questions should be addressed to info@dastardlybanana.com
//
class WeaponClassArrayType{
	var weaponClass : String;
	var WeaponInfoArray : WeaponInfo[];
}

static var storeActive : boolean = false;
static var canActivate : boolean = true;
static var singleton : DBStoreController;

 var balance: float; // Store account balance 
 
 //var scrollPosition : Vector2;
@HideInInspector var WeaponInfoArray : WeaponInfo[] ;
@HideInInspector var WeaponInfoByClass :WeaponClassArrayType[];
@HideInInspector var weaponClassNames : String[];
@HideInInspector var weaponClassNamesPopulated : String [];
@HideInInspector var playerW : PlayerWeapons;
@HideInInspector var nullWeapon : GameObject; //there must be one null weapon as a placeholder to put in an empty slot.
@HideInInspector var slotInfo: SlotInfo;
var canExitWhileEmpty : boolean = false;
static var inStore : boolean = false;
 
function Initialize() {
	singleton = this;
	playerW = FindObjectOfType(PlayerWeapons) as PlayerWeapons;
	slotInfo = FindObjectOfType(SlotInfo) as SlotInfo;		
	WeaponInfoArray = FindObjectsOfType(WeaponInfo) as WeaponInfo[];
	for(var w : WeaponInfo in WeaponInfoArray) {
		if(w.weaponClass == weaponClasses.Null) 
			nullWeapon = w.gameObject;
	}
	setupWeaponClassNames();
	setupWeaponInfoByClass();
}


function getNumOwned(slot: int) {
	//will use the slot info later to restrict count
	var n : int = 0;
	for (var i: int = 0; i < WeaponInfoArray.length; i++) {
		if(WeaponInfoArray[i].owned && slotInfo.isWeaponAllowed(slot,WeaponInfoArray[i]))
			n++;
	}
	return n;
}

function getWeaponNamesOwned(slot : int) : String[] {
	var names : String[] = new String[getNumOwned(slot)];
	var n : int = 0;
	for (var i: int = 0; i <  WeaponInfoArray.length; i++) {
		if(WeaponInfoArray[i].owned && slotInfo.isWeaponAllowed(slot,WeaponInfoArray[i])){
			names[n] = WeaponInfoArray[i].gunName;
			n++;
		}
	}
	return names;
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

TranslateExample();
//TranslateProject();