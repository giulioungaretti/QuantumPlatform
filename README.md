# Quantum Platform

This is an exercise in building an end to end solution for experimental physics work.

## Goals

The idea of the solution is to have a GUI that allows to add samples and relevant characterizations such as

- Experimental values e.g. temperature, pressure
- Attachments e.g. pictures from microscope, datafiles

The GUI allows to create and view a sample, and all the samples for a user.

## Structure

The solution has many pieces, the source lives under the `./src` folder.

### GUI

The GUI, aka client, is a web application that can be packaged as an electron app as well.
It is written in F# and compiled to js using fable.
The architecture of the app follows the MVU pattern pioneered by elm and implemented with fable.elmish.

#### TODO:

- The app is not configured to be smart about the API location
- The app bundle size is not optimzied

### Server

The server is an HTTP server that translates REST operations to orleans operations using the orleans client interface.
The app uses Giraffe as the functional abstraction on top of asp.NET // kestrel.

kool aid 🍸: server client and orleans grains share the same data models, allowing for pretty strong type safety.
Under the covers this is achieved by using F# records with CLIMutable attributes and auto generated JSON encoders/decoders for GUI/Server interactions.
Server and client also share endpoints as types, albeit aliases, to minimize the typo-wreak-havoc risk.

##### TODO:

- the server has an hardcoded CORS policy for localhost it should be configurable so that a real world scenario would work (e.g. web app on a cdn, or electron app on some device)

### Orleans.silo

The orleans silo powering the middle tier and storage is build using the new-ish net core generic host and f# it is a pretty standard thing.

## Notes on the orleans implementation architecture

The core of this solution is a sample, which is modeled as a journaled grain to allow for transparent auditing of what happened to it during its life time.
Sample are tracked in a sample container, so that an application can get a list of them.

### more features

- state transitions can be monitored (f.ex. receive notification when a sample gets a new characterization, react and do some work on a dataset that was just added from somewhere else )
- aggregates can be easily computed.

## missing pieces

### Containers

While dockerfiles and docker compose files are there, the build is currently b0rken (ref: a8f9efe69c49e2c7f411bb5ad2dae4e2693fce9f).
The architecture of this solution is easy for the server part, it is just a stateless container that can be configured by standard means and does not need any special network configuration.
How to deal with orleans clusters and containers is not entirely clear to me.
An orleans cluster is a tad more complicated than the usual "service" I have experience with.

More notes on this part to be committed later.

### _production_ deploy

Getting the artifacts complied and ready to go is implemented, but it's far from prod ready (see TODOs above).
The actual deploy part is missing all togheter.

### testing

I would never skip testing, but alas it may not bring much value to this solution.

### auth and permission

This demo leaves out auth and permissions because of time.

Auth would be implemented with an external service implementing oauth//open id.

The orleans-client exposes a login endpoint, and checks that the endpoints that need auth receive a valid token.

The orleans backend would not know about auth.

Permission is a tad more complicated and could be implemented both as an actor
in orleans, or as an external service that owns the permission data and scheme.
If time allows, more plans shall appear here.
