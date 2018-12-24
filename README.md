# rideaway-backend (v 2.0.2)

Backend for bike4brussels: routeplanner application. Provides a routing endpoint where you can provide two coordinates to get the route between these points. Different routing profiles can be used to get different routing behavior and there is support for route instructions. ~~Also provides an endpoint to request bicycle parkings in a certain radius around a location.~~  => Was not used (it's still there, but no guarantees)

Timestamps, starting location and ending location are saved for each request in the csv file corresponding with the date ~~, which is available from `hostname/requests/data`.~~

~~There is also a script included in `src/scripts` to extract a geojson file of the routes in the Brussels network. This is adapted from https://github.com/oSoc17/rideaway-data. This file can then be accessed via `hostname/routes/network.geojson`.~~ => Was not used (it's still there, but no guarantees)

The backend uses the open source routing library [Itinero](https://github.com/itinero/routing) by [Ben Abelshausen](https://github.com/xivk).

## Installation

The backend requires a routerdb file called `belgium.routerdb` to be in `src/mapdata`. There is a bash shell script in `src/scripts` that downloads the latest `osm.pbf` file of belgium from [geofabrik](https://www.geofabrik.de/) and processes it with [IDP](https://github.com/itinero/idp) to create the routerdb file.

To build the application, make sure you have the .NET Core library installed: https://www.microsoft.com/net/core.

Run `build.sh` or `build.bat` to build the application and `run.sh` or `run.bat` to run the application. The Api will start on http://localhost:5000.

## Deployment

### Preparing directories
```bash
> sudo mkdir /var/www
> sudo mkdir /var/www/bike4brussels-backend
```

### Downloading backend
```bash
> cd /var/www/bike4brussels-backend
> sudo git clone https://github.com/oSoc18/bike4brussels-backend
```

### Downloading mapdata
The following command will download the mapdata for Belgium and process this into a routing database, using the profiles included. This takes a while.
```bash
> cd /var/www/bike4brussels-backend/publish
> sudo ./fetch_osm_data.sh
```

### Build and publish the dotnet application
The included script builds, publishes and puts all the files in the correct folder.
```bash
> cd /var/www/bike4brussels-backend/bike4brussels-backend
> sudo ./deploy_project_SERVER_ONLY.sh
```

### Setting up a deamon to run the backend
```bash
> sudo useradd -s /sbin/nologin dotnetuser
> sudo chown -R dotnetuser:dotnetuser /var/www/bike4brussels-backend/publish
> sudo nano /etc/systemd/system/b4b-backend.service
```
Paste
```
[Unit]
Description=Backend service for bike4brussels.osm.be
DefaultDependencies=no
Wants=network.target # network is required
After=network.target

[Service]
ExecStart=/var/www/bike4brussels-backend/publish/rideaway-backend
WorkingDirectory=/var/www/bike4brussels-backend/publish
Restart=always
RestartSec=10 # Restart service after 10 seconds if dotnet service crashes
SyslogIdentifier=bike4brussels-backend
User=dotnetuser
Group=dotnetuser
PrivateTmp=true
Environment=ASPNETCORE_ENVIRONMENT=Production # specify environment variable for environment

[Install]
WantedBy = multi-user.target
```
save and startup the service.

```bash
> sudo systemctl start b4b-backend.service
```

and don't forget to "enable" the service, so it starts automatically at startup :

```bash
> sudo systemctl enable example-service.service
```

### Proxy server
To be able to handle requests at a given subpath of your website url, requests will have to be proxied. For this Nginx can be used. A possible nginx configuration file can look like this:
```
server {
    listen	 80;
    server_name  bike4brussels.osm.be;

    # default redirect to frontend
    location / {
        root /path/to/frontend/files;
        index index.html;
    }

    # redirect requests to /api/ to the backend (listening at port 5000)
    location /api/ {
        proxy_pass http://localhost:5000/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-Host $http_host;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-For $remote_addr;
        proxy_cache_bypass $http_upgrade;
        add_header X-Cache-Status $upstream_cache_status;
    }

    # redirect server error pages to the static page /50x.html
    #
    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
        root   /usr/share/nginx/html;
    }
}
```

## Api

### Get a route

Launch a `GET` request to `hostname/route`

#### Parameters

- `loc1` and `loc2`: The starting and ending coordinates of the route (example: `loc1=50.86071,4.35614`)
- `profile`: choose a profile to do routing, possible values are:

| profile name   | Explanation |
|---------------:|-------------|
| ` `            | This profile minimizes the time to destination. (fast) |
| `balanced`     | This profile avoids the biggest streets and prefers cycleways. |
| `relaxed`      | This profile avoids big roads, highly prefers cycleways, avoids uncomfortable surfaces such as cobblestones, and avoids streets with parallel parked cars. |
| `brussels`     | This profile heavily prefers the Brussels cycle network. |
| `networks`     | This profile uses the bicycle networks as much as possible (general). |

- `instructions`: Boolean to specify if you want the API to return route instructions or not (instructions are currently in highly Alpha stage)
- `lang`: specify the language of the instructions (supported: `en`, `fr` and `nl`)

