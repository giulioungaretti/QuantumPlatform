# api

First pass contains all the things needed for the api:

- an orlean client that responds to HTTP calls
- an orlean "server"

## plan

- f# http framework that maps routes to operatoins on grain(s)
- set of f# data types that can be used both for the http server, http client, and orleans
- f# defined grain interfaces and impementations
- f# silo
- docker to build all the pieces
- .net core


## missing pieces 

### auth and permission
This demo leaves out auth and permissions because of time.

Auth would be implemented with an external service implemeting oauth//open id.

The orleans-client exposes a login endpoint, and checks that the endpoints that need auth recieve a vaild token.

The orlean backend would not know about auth.

Permission is a tad more complicated and could be implemented both as an actor
in orleans, or as an external service that owns the permission data and scheme.
If time allows, more plans shall appear here.


