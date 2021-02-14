# OmgImgFlow Image Server API

API for the [omgimgflow app](https://github.com/lhargil/omgimgflow).

## Getting started

### Technical Requirements

1. [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
2. [Docker](https://www.docker.com/products/docker-desktop) _(optional, if a local MySQL installation is available)_

### Directory changes

Create directory `omgimgflow_cache` and `omgimgflow_photos` in your `HOME` directory. The cache directory contains the cached images generated from the image manipulations. The photos directory contains the uploaded photos from the user.

### Set connection string

```bash
$ dotnet user-secrets set "ConnectionStrings:OmgImagesServerDb" "{connecionstring}"
```

### Run the app

Instantiate mysql db

```bash
$ docker-compose -f mysql.yaml up -d
```

Run the API and access at https://localhost:5001

```
$ dotnet run
```

_This project is powered by ASP.NET Core 5, EntityFramework Core 5._
