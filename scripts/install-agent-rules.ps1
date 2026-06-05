[CmdletBinding()]
param(
    [string]$TargetPath = (Get-Location).Path,

    [ValidateSet("all", "generic", "codex", "copilot", "cursor", "claude", "docs", "prompts")]
    [string[]]$Profile = @("all"),

    [switch]$Overwrite,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$KitRoot = Split-Path -Parent $ScriptDir

if (-not (Test-Path -LiteralPath $TargetPath)) {
    if ($DryRun) {
        Write-Host "WOULD create target directory: $TargetPath"
        $TargetRoot = [System.IO.Path]::GetFullPath($TargetPath)
    }
    else {
        New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
        $TargetRoot = (Resolve-Path -LiteralPath $TargetPath).Path
    }
}
else {
    $TargetRoot = (Resolve-Path -LiteralPath $TargetPath).Path
}

$commonFiles = @(
    "AGENTS.md",
    "docs/ai/APPROVAL_GATES.md",
    "docs/ai/OPERATING_MODEL.md",
    "docs/ai/PACKAGING_PROTOCOL.md",
    "docs/ai/PROJECT_DISCOVERY.md",
    "docs/ai/PROJECT_PROFILE.md",
    "docs/ai/RULE_IMPROVEMENT_PROTOCOL.md",
    "docs/ai/SECURITY_CHECKLIST.md",
    "docs/ai/SOURCE_REFRESH_PROTOCOL.md",
    "docs/ai/VERIFICATION_PROTOCOL.md",
    "templates/DECISION_LOG.md",
    "templates/PLAN.md",
    "templates/SPEC.md",
    "README.md",
    "SOURCES.md",
    "scripts/install-agent-rules.ps1"
)

$promptFiles = @(
    "prompts/autonomous-cycle.prompt.md",
    "prompts/improve-existing-project.prompt.md",
    "prompts/new-project.prompt.md",
    "prompts/release-readiness.prompt.md",
    "prompts/rule-author.prompt.md",
    "prompts/security-review.prompt.md"
)

$baseFiles = $commonFiles + $promptFiles

$profileFiles = @{
    "generic" = $baseFiles
    "codex"   = $baseFiles + @("tooling/codex-config.example.toml")
    "copilot" = $baseFiles + @(".github/copilot-instructions.md")
    "cursor"  = $baseFiles + @(".cursor/rules/vibe-agent-core.mdc")
    "claude"  = $baseFiles + @("CLAUDE.md")
    "docs"    = $commonFiles
    "prompts" = $promptFiles
}

function Add-Unique {
    param(
        [string[]]$Items,
        [string[]]$More
    )

    foreach ($item in $More) {
        if ($Items -notcontains $item) {
            $Items += $item
        }
    }

    return $Items
}

function Get-ExistingFullPath {
    param([string]$Path)

    if (Test-Path -LiteralPath $Path) {
        return (Resolve-Path -LiteralPath $Path).Path
    }

    return [System.IO.Path]::GetFullPath($Path)
}

$filesToInstall = @()

if ($Profile -contains "all") {
    $filesToInstall = Add-Unique $filesToInstall $commonFiles
    $filesToInstall = Add-Unique $filesToInstall $promptFiles
    $filesToInstall = Add-Unique $filesToInstall @(
        "CLAUDE.md",
        ".github/copilot-instructions.md",
        ".cursor/rules/vibe-agent-core.mdc",
        "tooling/codex-config.example.toml"
    )
}
else {
    foreach ($profileName in $Profile) {
        $filesToInstall = Add-Unique $filesToInstall $profileFiles[$profileName]
    }
}

foreach ($relativePath in $filesToInstall) {
    $source = Join-Path $KitRoot $relativePath
    $destination = Join-Path $TargetRoot $relativePath
    $destinationDir = Split-Path -Parent $destination

    if (-not (Test-Path -LiteralPath $source)) {
        throw "Missing kit file: $source"
    }

    $sourceFullPath = Get-ExistingFullPath $source
    $destinationFullPath = Get-ExistingFullPath $destination

    if ($sourceFullPath -eq $destinationFullPath) {
        Write-Host "SKIP self: $relativePath"
        continue
    }

    if ((Test-Path -LiteralPath $destination) -and -not $Overwrite) {
        Write-Host "SKIP existing: $relativePath"
        continue
    }

    if ($DryRun) {
        if (Test-Path -LiteralPath $destination) {
            Write-Host "WOULD overwrite: $relativePath"
        }
        else {
            Write-Host "WOULD create: $relativePath"
        }
        continue
    }

    if (-not (Test-Path -LiteralPath $destinationDir)) {
        New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
    }

    if ($Overwrite) {
        Copy-Item -LiteralPath $source -Destination $destination -Force
    }
    else {
        Copy-Item -LiteralPath $source -Destination $destination
    }

    Write-Host "INSTALLED: $relativePath"
}

Write-Host "Done. Target: $TargetRoot"
