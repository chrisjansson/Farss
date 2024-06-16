FROM mcr.microsoft.com/dotnet/aspnet:8.0.3
EXPOSE 5000/tcp
WORKDIR /app
COPY publish/ ./
ENTRYPOINT ["dotnet", "Farss.Server.dll"]
