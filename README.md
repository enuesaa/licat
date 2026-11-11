# licat
toy app.   
browse files and copy their contents.

[![ci](https://github.com/enuesaa/licat/actions/workflows/ci.yml/badge.svg)](https://github.com/enuesaa/licat/actions/workflows/ci.yml)

## Usage
```bash
$ licat --help
Description:
  licat --- browse files and copy their contents

Usage:
  licat [options]

Options:
  -r, --resume  Resume previously copied file selection
  -h, --help    Show help and usage information
  --version     Show version information
```

## Development
```bash
# Dev
dotnet run

# Update dependencies
dotnet list package --outdated
dotnet add package <package-name>
dotnet build

# Upgrade dotnet
brew upgrade --cask dotnet-sdk
vim licat.csproj # change the value of <TargetFramework>net11.0</TargetFramework>
```