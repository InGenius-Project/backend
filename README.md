# Project Title

## Table of Contents

- [About](#about)
- [Getting Started](#getting_started)
- [Usage](#usage)
- [Contributing](../CONTRIBUTING.md)

## About <a name = "about"></a>

This is a web API project primarily built with C# EF Core.
It serves as the backend for the internship matching platform "InG".

## Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See [usage](#usage) for notes on how to deploy the project on a live system.

### Prerequisites

First, you need to install dotnet 8.0 SDK and dotnet Runtime.  
Check [Dotnet Offical Website](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

- Sqlite (for dev)
- SqlServer (for production)

### Secret And Database Connection

- Create file `IngBackendApi.Application/appsettings.Secrets.json` which is needed for running the application

  ```python
  # appsettings.Secrets.json example
  {
    "Secrets": {
      # Generate a JWT secret key and paste here to handling jwt token authorization
      "JwtSecretKey": "<Your Secret Jwt Key>",
      # This credential is for email server sending user signup verification mail (GMAIL)
      "EmailUsername": "<Your Email Server Username>",
      "EmailPassword": "<Your Email Server Password>"
    },
    # ConnectionString in appsettings.Development.json is for dev purpose which should be Sqlite connection string
    # This is for production database connection which should be SqlServer connection string
    "ConnectionStrings": {
      "Default": "<Prodction DataBase Connection String>"
    },
    # You can disable AI function by removing AIService and relative using case in controllers
    "AI": {
      "Api": "<IngAi API>",
      "KeywordExtractionApi": "<IngAi KeywordExtractionApi API>",
      "GenerateAreaApi": "<IngAi Generate AreaApi API>"
    }
  }
  ```

### Installing

Clone the repository and restore the project packages.

- Restore package with dotnet

```
git clone https://github.com/InGenius-Project/backend.git
cd backend
dotnet restore
```

- Initialize the database
  - Add migration
    ```
    dotnet ef --project .\IngBackendApi.Application\ migrations add "init-db"
    ```
  - Update database
    ```
    dotnet ef --project .\IngBackendApi.Application\ database update
    ```

## Usage <a name = "usage"></a>

- run
  ```
  dotnet run --project .\IngBackendApi.Application\
  ```
- run test
  ```
  dotnet test
  ```
- Btw, if you want to watch changes
  ```
  dotnet watch --project .\IngBackendApi.Application\
  ```
