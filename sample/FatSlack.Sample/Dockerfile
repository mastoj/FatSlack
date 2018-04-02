FROM gcr.io/google-appengine/aspnetcore:2.0
WORKDIR /app
COPY ./bin/Release/netcoreapp2.0/publish/ .
ENTRYPOINT [ "dotnet", "FatSlack.Sample.dll" ]