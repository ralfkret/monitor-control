Add-Type -TypeDefinition (get-content "$PSScriptRoot\MonitorControl.cs" -Raw) 

function mc {
  [CmdletBinding()]
  param ()

  DynamicParam {
    $config = Get-Content -Path "$PSScriptRoot\config.json" | ConvertFrom-Json -AsHashtable
    
    $paramDictionary = New-Object System.Management.Automation.RuntimeDefinedParameterDictionary
    $attributes = New-Object System.Collections.ObjectModel.Collection[System.Attribute]

    $attribute1 = New-Object System.Management.Automation.ParameterAttribute
    $attributes.Add($attribute1)

    $attribute2 = New-Object System.Management.Automation.ValidateSetAttribute($config.keys)
    $attributes.Add($attribute2)

    $parameterName = "MonitorInput"
    $dynamicParam = New-Object System.Management.Automation.RuntimeDefinedParameter(
      $parameterName, [string], $attributes
    )

    $paramDictionary.Add($parameterName, $dynamicParam)

    return $paramDictionary
  }

  begin{
    $monitorInput = $PSBoundParameters[$parameterName]
    $value = if (-not $monitorInput) { $null }  else { $config[$monitorInput] }
  }

  process {
    [MonitorControl]::Main($value)
  }
}