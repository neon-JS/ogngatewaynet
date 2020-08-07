# OgnGateway.NET
A simple gateway for OGN-data which feeds (SignalR-) websockets.
This gateway listens to the APRS-servers of the OpenGliderNetwork and parses & passes the received messages to all
connected clients. It is configurable so that certain events (see below) will trigger an immediately message while the
rest is buffered and sent in intervals. Those intervals and rules can be changed, just like the area that should be
filtered.

### Events
- A new aircraft appears in visible range
- An aircraft starts or lands

[Link to the OGN-wiki](http://wiki.glidernet.org/)

**Info**: This is a rewrite of the _[OgnListener](https://gitlab.com/neon-js/ognlistener/)_ project which did similar
things and was written in node.js. The _OgnListener_ is not being developed anymore!

## Structure
This solution is split into two projects:

- _OgnGateway_, which handles the connection to the APRS-servers, aircraft data and conversion to business models.
- _WebsocketGateway_, which uses the _OgnGateway_ and provides the received data to (SignalR-) websockets.

It also contains a very simple _frontend_, which is (poorly) written with Vue.js and currently in german.
This _frontend_ connects to the _WebsocketGateway_ and lists the incoming data / creates live notifications.

## How to use
- Clone / download this repository.
- Install all necessary dependencies (_Reactive_, _Akka_) via _NuGet_.
- Fill out the _appsettings.json.default_ and save it as _appsettings.json_.
- Extend the solution with your code :D

## How to host
- Clone / download this repository.
- Install all necessary dependencies (_Reactive_, _Akka_) via _NuGet_.
- Fill out the _appsettings.json.default_ and save it as _appsettings.json_.
- Either build the _WebsocketGateway_ project or just run it.
- Connect to the SignalR websocket with your frontend, application etc.

## SignalR
There are two methods which are used to communicate between server and client:

- `NewData(FlightDataDto data): void`, which is sent from **server -> client** and contains the latest aircraft- /
   flight-data.
- `InitialRequest(): FlightDataDto[]`, which is sent from **client -> server**. The result contains all latest aircraft
   data of the last timespan (this timespan is configurable, see `GatewayOptions.MaxAgeSeconds` in
   _appsettings.json.default_).
- `GetConfiguration(): {MaxAgeSeconds, EventsOnly, IntervalSeconds, FilterPosition, FilterRadius}`, which is sent from 
   **client -> server**. The result contains some information about the server-configuration. 

## License
This code is licensed under the MIT-License (see _LICENSE.md_).
Before using this, make sure to not violate against OGN rules!

- [OGN data usage](https://www.glidernet.org/ogn-data-usage/)
- [ODbL summary](https://opendatacommons.org/licenses/odbl/summary/)
