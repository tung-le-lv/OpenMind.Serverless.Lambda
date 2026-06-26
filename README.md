## Local Development

### One-time setup

Install the required tools:

```powershell
# AWS SAM CLI
winget install Amazon.SAM-CLI

# docker-compose (needed by podman compose)
winget install Docker.DockerCompose
```

Add SAM CLI to your PowerShell profile so it's always in PATH:

```powershell
# Append to $PROFILE (run once)
Add-Content $PROFILE "`n`$env:PATH += `";C:\Program Files\Amazon\AWSSAMCLI\bin`""
```

Verify your Podman machine is configured. The machine name and SSH port are visible via:

```powershell
podman machine list
podman machine inspect <machine-name>  # note the SSH Port value
```

---

### Starting a dev session

Run these steps in order each time you open a fresh terminal.

#### Step 1 — Start DynamoDB Local

```powershell
podman compose up -d
```

This starts DynamoDB Local on port 8000 and creates the `Orders-local` table with the `CustomerIdIndex` GSI.

#### Step 2 — Build

```powershell
sam build --template-file deploy/aws/template.yaml --base-dir .

podman build -t order-api-local:latest -f deploy/local/Dockerfile.lambda .aws-sam/build/CreateOrderFunction
```

#### Step 3 — Open SSH tunnel (keep this terminal open)

SAM uses the Docker SDK which requires a TCP socket. This tunnel bridges Windows TCP → Podman's Unix socket inside WSL.

```powershell
# Get your machine's SSH port (usually 58765, but verify with: podman machine inspect <name>)
$key = "$env:USERPROFILE\.local\share\containers\podman\machine\machine"
ssh -N -L 2375:/run/podman/podman.sock -p 58765 -i $key -o StrictHostKeyChecking=no root@127.0.0.1
```

#### Step 4 — Start SAM (new terminal)

```powershell
$env:DOCKER_HOST = "tcp://127.0.0.1:2375"
sam local start-api `
  --template-file deploy/local/template-local.yaml `
  --docker-network openmind-local `
  --skip-pull-image
```

Or 
```powershell
.\deploy\local\start.ps1
```
for better logs formatter.  

The API is available at `http://localhost:3000`.

> **First request per endpoint is slow (~30 s)** while SAM builds the Lambda runtime wrapper image. Every request after that is fast.

---

### After changing code

```powershell
sam build --template-file deploy/aws/template.yaml --base-dir .
podman build -t order-api-local:latest -f deploy/local/Dockerfile.lambda .aws-sam/build/CreateOrderFunction
```

or  
```powershell
.\deploy\local\build.ps1
```

Then **Ctrl+C** SAM and re-run the `sam local start-api` command from Step 4.

### Inspect DynamoDB

Use [NoSQL Workbench](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.html) to browse data locally.

Add a connection: **Operation Builder → Add Connection → DynamoDB Local → hostname `localhost`, port `8000`**.

![NoSQL Workbench](docs/aws-workbench.jpg)