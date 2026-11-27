param(
    [string]$SeedFile = (Join-Path $PSScriptRoot "..\seeds\001_reference_data.sql")
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SeedFile)) {
    throw "Seed file not found: $SeedFile"
}

$sql = Get-Content -Path $SeedFile -Raw

$expectedRoles = @("Citizen", "Volunteer", "Staff", "Coordinator", "Admin")
$expectedCategories = @("FOOD", "WATER", "MEDICAL", "SHELTER", "EVACUATION", "OTHER")
$expectedDistricts = @(
    "Colombo", "Gampaha", "Kalutara",
    "Kandy", "Matale", "Nuwara Eliya",
    "Galle", "Matara", "Hambantota",
    "Jaffna", "Kilinochchi", "Mannar", "Vavuniya", "Mullaitivu",
    "Batticaloa", "Ampara", "Trincomalee",
    "Kurunegala", "Puttalam",
    "Anuradhapura", "Polonnaruwa",
    "Badulla", "Monaragala",
    "Ratnapura", "Kegalle"
)

foreach ($role in $expectedRoles) {
    if ($sql -notmatch "N'$([regex]::Escape($role))'") {
        throw "Missing role in reference data: $role"
    }
}

foreach ($cat in $expectedCategories) {
    if ($sql -notmatch "'$([regex]::Escape($cat))'") {
        throw "Missing need category in reference data: $cat"
    }
}

foreach ($district in $expectedDistricts) {
    if ($sql -notmatch "N'$([regex]::Escape($district))'") {
        throw "Missing district in reference data: $district"
    }
}

$districtCodeMatches = [regex]::Matches($sql, "'LK-\d{2}'") | ForEach-Object { $_.Value }
$uniqueCodes = $districtCodeMatches | Select-Object -Unique
if ($uniqueCodes.Count -ne 25) {
    throw "Expected 25 unique district codes, found $($uniqueCodes.Count)."
}

Write-Output "OK: Verified $($expectedRoles.Count) roles, $($expectedCategories.Count) categories, and $($uniqueCodes.Count) districts."
