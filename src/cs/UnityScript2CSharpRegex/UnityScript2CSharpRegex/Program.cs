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
function Comparison(x: RaycastHit, y: RaycastHit): int {
	return Mathf.Sign(x.distance - y.distance);
}

function SprayDirection() {
	var vx = (1 - 2 * Random.value) * actualSpread;
	var vy = (1 - 2 * Random.value) * actualSpread;
	var vz = 1.0;
	return weaponCam.transform.TransformDirection(Vector3(vx, vy, vz));
}

function SprayDirection(dir: Vector3) {
	var vx = (1 - 2 * Random.value) * actualSpread;
	var vy = (1 - 2 * Random.value) * actualSpread;
	var vz = (1 - 2 * Random.value) * actualSpread;
	return dir + Vector3(vx, vy, vz);
}

function Reload() {
	if (ammoLeft >= ammoPerClip || clips <= 0 || !gunActive || Avoidance.collided) {
		return;
	}
	reloadCancel = false;
	idleTime = 0;
	aim1.canSprint = PlayerWeapons.PW.reloadWhileSprinting;
	if (progressiveReload) {
		ProgReload();
		return;
	}

	if (reloading)
		return;

	//aim1.canSwitchWeaponAim = false;
	if (aim1.canAim) {
		aim1.canAim = false;
		aim = true;
	}
	if (gunType == gunTypes.spray) {
		if (GetComponent.< AudioSource > ()) {
			if (GetComponent.< AudioSource > ().clip == loopSound && GetComponent.< AudioSource > ().isPlaying) {
				GetComponent.< AudioSource > ().Stop();
			}
		}
	}
	reloading = true;
	if (secondaryWeapon != null) {
		secondaryWeapon.reloading = true;
	} else if (!isPrimaryWeapon) {
		primaryWeapon.reloading = true;
	}
	var tempEmpty: boolean;
	yield WaitForSeconds(waitforReload);
	if (reloadCancel) {
		return;
	}

	if (isPrimaryWeapon) {
		BroadcastMessage(""ReloadAnimEarly"", SendMessageOptions.DontRequireReceiver);
		if (ammoLeft >= ammoPerShot) {
			tempEmpty = false;
			BroadcastMessage(""ReloadAnim"", reloadTime, SendMessageOptions.DontRequireReceiver);
		} else {
			tempEmpty = true;
			BroadcastMessage(""ReloadEmpty"", emptyReloadTime, SendMessageOptions.DontRequireReceiver);
		}
	} else {
		BroadcastMessage(""SecondaryReloadAnimEarly"", SendMessageOptions.DontRequireReceiver);
		if (ammoLeft >= ammoPerShot) {
			tempEmpty = false;
			BroadcastMessage(""SecondaryReloadAnim"", reloadTime, SendMessageOptions.DontRequireReceiver);
		} else {
			tempEmpty = true;
			BroadcastMessage(""SecondaryReloadEmpty"", emptyReloadTime, SendMessageOptions.DontRequireReceiver);
		}
	}

	// Wait for reload time first - then add more bullets!
	if (ammoLeft > ammoPerShot) {
		yield WaitForSeconds(reloadTime);
	} else {
		yield WaitForSeconds(emptyReloadTime);
	}
	if (reloadCancel) {
		return;
	}
	reloading = false;
	if (secondaryWeapon != null) {
		secondaryWeapon.reloading = false;
	} else if (!isPrimaryWeapon) {
		primaryWeapon.reloading = false;
	}
	// We have a clip left reload
	if (ammoType == ammoTypes.byClip) {
		if (clips > 0) {
			if (!infiniteAmmo)
				clips--;
			ammoLeft = ammoPerClip;
		}
	} else if (ammoType == ammoTypes.byBullet) {
		if (clips > 0) {
			if (clips > ammoPerClip) {
				if (!infiniteAmmo)
					clips -= ammoPerClip - ammoLeft;

				ammoLeft = ammoPerClip;
			} else {
				var ammoVal: float = Mathf.Clamp(ammoPerClip, clips, ammoLeft + clips);
				if (!infiniteAmmo)
					clips -= (ammoVal - ammoLeft);

				ammoLeft = ammoVal;
			}
		}
	}
	if (!tempEmpty && addOneBullet) {
		if (ammoType == ammoTypes.byBullet && clips > 0) {
			ammoLeft += 1;
			clips -= 1;
		}
	}
	if (aim)
		aim1.canAim = true;
	aim1.canSprint = true;
	//aim1.canSwitchWeaponAim = true;
	SendMessage(""UpdateAmmo"", ammoLeft, SendMessageOptions.DontRequireReceiver);
	SendMessage(""UpdateClips"", clips, SendMessageOptions.DontRequireReceiver);
	ApplyToSharedAmmo();
	PlayerWeapons.autoFire = autoFire;
}
";

    Console.WriteLine(UnityScript2CSharp.TranslateCode(input));
}

TranslateExample();
//TranslateProject();