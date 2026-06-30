$projectRoot    = Resolve-Path "$PSScriptRoot\..\.."
$orderApiSrc    = "$projectRoot\src\Order.Api"
$paymentApiSrc  = "$projectRoot\src\Payment.Api"
$orderPublish   = "$projectRoot\publish\order-api"
$paymentPublish = "$projectRoot\publish\payment-api"
$orderZip       = "$projectRoot\publish\order-api.zip"
$paymentZip     = "$projectRoot\publish\payment-api.zip"

# Clean publish dirs to avoid stale trimmed assemblies causing TypeLoadException
if (Test-Path $orderPublish)   { Remove-Item -Recurse -Force $orderPublish }
if (Test-Path $paymentPublish) { Remove-Item -Recurse -Force $paymentPublish }

Write-Host "Publishing Order.Api (self-contained, linux-x64)..."
dotnet publish "$orderApiSrc\Order.Api.csproj" -c Release -r linux-x64 --self-contained true -p:PublishTrimmed=false -o "$orderPublish"
if (-not $?) { Write-Host "dotnet publish failed" -ForegroundColor Red; exit 1 }

Copy-Item "$orderPublish\Order.Api" "$orderPublish\bootstrap" -Force

# Compress-Archive does not preserve Unix execute permissions; use podman/zip inside Linux to set chmod +x on bootstrap
Write-Host "Packaging order-api.zip (via Linux container to preserve execute bit)..."
if (Test-Path $orderZip) { Remove-Item $orderZip }
podman run --rm `
    -v "${orderPublish}:/src" `
    -v "$projectRoot\publish:/out" `
    alpine sh -c "apk add --quiet zip && cd /src && chmod +x bootstrap && zip -r /out/order-api.zip ."
if (-not $?) { Write-Host "zip failed" -ForegroundColor Red; exit 1 }

Write-Host "Publishing Payment.Api (self-contained, linux-x64)..."
dotnet publish "$paymentApiSrc\Payment.Api.csproj" -c Release -r linux-x64 --self-contained true -p:PublishTrimmed=false -o "$paymentPublish"
if (-not $?) { Write-Host "dotnet publish failed" -ForegroundColor Red; exit 1 }

Copy-Item "$paymentPublish\Payment.Api" "$paymentPublish\bootstrap" -Force

Write-Host "Packaging payment-api.zip (via Linux container to preserve execute bit)..."
if (Test-Path $paymentZip) { Remove-Item $paymentZip }
podman run --rm `
    -v "${paymentPublish}:/src" `
    -v "$projectRoot\publish:/out" `
    alpine sh -c "apk add --quiet zip && cd /src && chmod +x bootstrap && zip -r /out/payment-api.zip ."
if (-not $?) { Write-Host "zip failed" -ForegroundColor Red; exit 1 }

Write-Host "Done - order-api.zip ($([math]::Round((Get-Item $orderZip).Length/1MB,1)) MB) and payment-api.zip ($([math]::Round((Get-Item $paymentZip).Length/1MB,1)) MB) are ready." -ForegroundColor Green
