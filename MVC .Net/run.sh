#!/usr/bin/env bash
set -euo pipefail

# ir para o diretório do script (raiz do projeto)
cd "$(dirname "$0")"

echo "dotnet restore"
dotnet restore

echo "dotnet build"
dotnet build

echo "dotnet run --project 'MVC .Net.csproj'"
dotnet run --project "MVC .Net.csproj"