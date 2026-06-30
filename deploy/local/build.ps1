$projectRoot    = Resolve-Path "$PSScriptRoot\..\.."
$orderApiSrc    = "$projectRoot\src\Order.Api"
$paymentApiSrc  = "$projectRoot\src\Payment.Api"
$orderPublish   = "$projectRoot\publish\order-api"
$paymentPublish = "$projectRoot\publish\payment-api"

Write-Host "Publishing Order.Api..."
dotnet publish "$orderApiSrc\Order.Api.csproj" -c Release -r linux-x64 --no-self-contained -o "$orderPublish"
if (-not $?) { Write-Host "dotnet publish failed" -ForegroundColor Red; exit 1 }

Write-Host "Building order-api-local:latest..."
podman build -t order-api-local:latest -f "$PSScriptRoot\Dockerfile.lambda" "$orderPublish"
if (-not $?) { Write-Host "podman build failed" -ForegroundColor Red; exit 1 }

Write-Host "Publishing Payment.Api..."
dotnet publish "$paymentApiSrc\Payment.Api.csproj" -c Release -r linux-x64 --no-self-contained -o "$paymentPublish"
if (-not $?) { Write-Host "dotnet publish failed" -ForegroundColor Red; exit 1 }

Write-Host "Building payment-api-local:latest..."
podman build -t payment-api-local:latest -f "$PSScriptRoot\Dockerfile.lambda" "$paymentPublish"
if (-not $?) { Write-Host "podman build failed" -ForegroundColor Red; exit 1 }

Write-Host "Done - order-api-local:latest and payment-api-local:latest are ready." -ForegroundColor Green
