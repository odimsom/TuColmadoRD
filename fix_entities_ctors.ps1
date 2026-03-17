# Add a private parameterless constructor if missing
$entitiesPath = "C:\Users\Francisco C. Dev\source\repos\TuColmadoRD\TuColmadoRD.Core.Domain\Entities"
$files = Get-ChildItem -Path $entitiesPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $className = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $hasEmptyCtor = $false
    
    foreach ($line in $content) {
        if ($line -match "private\s+$className\s*\(\s*\)") {
            $hasEmptyCtor = $true
            break
        }
        if ($line -match "protected\s+$className\s*\(\s*\)") {
            $hasEmptyCtor = $true
            break
        }
        if ($line -match "public\s+$className\s*\(\s*\)") {
            $hasEmptyCtor = $true
            break
        }
    }
    
    if (-not $hasEmptyCtor -and -not $file.Name.StartsWith("I")) {
        # Find where to insert it. Easiest is to put it right after the public properties before the first method or constructor.
        # Let's just put it before the last closing brace of the class.
        $insertIdx = -1
        for ($i = $content.Length - 1; $i -ge 0; $i--) {
            if ($content[$i].Trim() -eq "}") {
                # This is namespace closing brace maybe
                # Let's find the second to last '}'
                # Actually, simplest is just to append it before the second to last `}`
            }
        }
        
        # Better robust approach: insert private ClassName() { } after the class opening `{`
        for ($i = 0; $i -lt $content.Length; $i++) {
            if ($content[$i] -match "public\s+(sealed\s+)?class\s+$className") {
                # next line is `{` or later
                for ($j = $i; $j -lt $content.Length; $j++) {
                    if ($content[$j].Contains("{")) {
                        $newContent = $content[0..$j] + "    private $className() { }" + $content[($j+1)..($content.Length-1)]
                        Set-Content -Path $file.FullName -Value $newContent
                        break
                    }
                }
                break
            }
        }
    }
}
