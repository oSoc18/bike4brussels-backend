# rideaway-backend(v 2.0.2)

Backend for rideaway: a bicycle navigation application. Provides a routing endpoint where you can provide two coordinates to get the route between these points. Different routing profiles can be used to get different routing behavior and there is support for route instructions. Also provides an endpoint to request bicycle parkings in a certain radius around a location.

For all requests over the brussels cycle network timestamps, starting location and ending location are saved in the csv file corresponding with the date, which is available from `hostname/requests/data`.

There is also a script included in `src/scripts` to extract a geojson file of the routes in the Brussels network. This is adapted from https://github.com/oSoc17/rideaway-data. This file can then be accessed via `hostname/routes/network.geojson`.

The backend uses the open source routing library [Itinero](https://github.com/itinero/routing) by [Ben Abelshausen](https://github.com/xivk).

## Installation

The backend requires a routerdb file called `belgium.routerdb` to be in `src/mapdata`. There is a bash shell script in `src/scripts` that downloads the latest `osm.pbf` file of belgium from [geofabrik](https://www.geofabrik.de/) and processes it with [IDP](https://github.com/itinero/idp) to create the routerdb file.

To build the application, make sure you have the .NET Core library installed: https://www.microsoft.com/net/core.

Run `build.sh` or `build.bat` to build the application and `run.sh` or `run.bat` to run the application. The Api will start on http://localhost:5000.

## Api

### Get a route

Launch a `GET` request to `hostname/route`

#### Parameters

- `loc1` and `loc2`: The starting and ending coordinates of the route (example: `loc1=50.86071,4.35614`)
- `profile`: choose a profile to do routing, possible values are:
	- `networks`: use the bicycle networks as much as possible
	- `balanced`: tries to select calmer streets
	- `shortest`: the shortest route, but maybe not the most pleasant one
	- `brussels`: use the brussels bicycle network as much as possible
- `instructions`: Boolean to specify if you want the API to return route instructions or not (instructions are only supported on the `brussels` profile)
- `lang`: specify the language of the instructions (supported: `en` and `nl`)

### Get bicycle parkings in a certain radius

Launch a `GET` request to `hostname/parking`

#### Parameters

- `loc`: location around which the parking need to be found (example: `loc=50.86071,4.35614`)
- `radius`: radius in meters
