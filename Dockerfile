# Alpine image does not include bash which the sdk depends on so use bionic to build instead
FROM microsoft/dotnet:2.1.402-sdk-bionic
WORKDIR /src
COPY . ./
RUN dotnet publish Farss.Server/Farss.Server.fsproj -o /output

FROM microsoft/dotnet:2.1.6-aspnetcore-runtime-alpine3.7 
EXPOSE 80/tcp
WORKDIR /app
COPY --from=0 /output ./
ENTRYPOINT ["dotnet", "Farss.Server.dll"]