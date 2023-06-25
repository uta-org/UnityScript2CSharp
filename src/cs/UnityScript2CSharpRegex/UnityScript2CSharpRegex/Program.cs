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
function test() {
    var xQuaternion : Quaternion;
}

    function Update () {
        if(freeze || !PlayerWeapons.playerActive) return;
        
        if(retSensitivity > 0)
            retSensitivity*=-1;
    
        if(useLookMotion && PlayerWeapons.canLook) {
            var xQuaternion : Quaternion;
            var yQuaternion : Quaternion;

            rotationZ = ClampAngle (rotationZ, -actualZRange,  actualZRange);
            if(Mathf.Abs(Input.GetAxis(""Mouse X"")) <.05){
                if(sensitivityX > 0){
                } else {
                }
            }
                
            xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            var zQuaternion : Quaternion = Quaternion.AngleAxis (rotationZ, Vector3.forward);
            yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
                
            transform.localRotation = Quaternion.Lerp(transform.localRotation ,originalRotation * xQuaternion * yQuaternion *zQuaternion, Time.deltaTime*10);
        }
        
        if(useWalkMotion){
            //Velocity-based changes
            var relVelocity : Vector3 = transform.InverseTransformDirection(PlayerWeapons.CM.movement.velocity);            
            var zVal : float;
            var xVal : float;
            var xVal2 : float;
            
            lastOffset = posOffset;
            
            var s : float = Vector3(PlayerWeapons.CM.movement.velocity.x, 0, PlayerWeapons.CM.movement.velocity.z).magnitude/14;
    
            
            if(!AimMode.staticAiming){                
                var xPos : float = Mathf.Clamp(relVelocity.x*xPosMoveSensitivity, -xPosMoveRange*s, xPosMoveRange*s);
                posOffset.x = Mathf.Lerp(posOffset.x, xPos, Time.deltaTime*xPosAdjustSpeed);// + startPos.x;
                
                var zPos : float = Mathf.Clamp(relVelocity.z*zPosMoveSensitivity, -zPosMoveRange*s, zPosMoveRange*s);
                posOffset.z = Mathf.Lerp(posOffset.z, zPos, Time.deltaTime*zPosAdjustSpeed);// + startPos.z;
                
            } else {
                posOffset.x = Mathf.Lerp(posOffset.x, 0, Time.deltaTime*xPosAdjustSpeed*3);// + startPos.x;
                posOffset.z = Mathf.Lerp(posOffset.z, 0, Time.deltaTime*zPosAdjustSpeed*3);// + startPos.z;
                            
            }
            
            //Apply Jostle
            lastJostle = curJostle;
            curJostle = Vector3.Lerp(curJostle, jostleAmt, Time.deltaTime*10);
            jostleAmt = Vector3.Lerp(jostleAmt, Vector3.zero, Time.deltaTime*3);
                        
            lastTarget = curTarget;
            curTarget = Vector3.Lerp(curTarget, posOffset, Time.deltaTime*8);
            
            transform.localPosition += curTarget - lastTarget;
            transform.localPosition += curJostle - lastJostle;
        }
    }";

    // TODO: public Quaternion, maybe because of the identation, but isn't detected
    Console.WriteLine(UnityScript2CSharp.TranslateCode(input));
}

//TranslateExample();
TranslateProject();