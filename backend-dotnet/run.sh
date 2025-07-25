#!/bin/bash
echo "Starting Blog Platform API (ASP.NET Core)..."
echo ""
echo "Restoring packages..."
dotnet restore
echo ""
echo "Building solution..."
dotnet build
echo ""
echo "Starting server on https://localhost:5001"
echo "API Documentation: https://localhost:5001/swagger"
echo ""
cd BlogPlatform.API
dotnet run