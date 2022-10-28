# OgnGateway.NET
![.NET Build status](https://github.com/neon-JS/ogngatewaynet/workflows/.NET/badge.svg)

A simple gateway for OGN-data which feeds websockets.
This gateway listens to the APRS-servers of the OpenGliderNetwork and parses & passes the received messages to all
connected clients. It is configurable so that certain events (see below) will trigger an immediately message
while the rest is buffered and sent in intervals. Those intervals and rules can be changed, just like the area
that should be filtered.

### Events
- A new aircraft appears in visible range
- An aircraft starts or lands

## Structure
This solution is split into two projects:

- _OgnGateway_, which handles the connection to the APRS-servers, aircraft data and conversion to business models.
- _WebsocketGateway_, which uses the _OgnGateway_ and provides the received data to websockets and HTTP endpoints.

It also contains a very simple _frontend_, which is written with Vue.js and currently in german.
This _frontend_ connects to the _WebsocketGateway_ and lists the incoming data / creates live notifications.

## How to use
- Clone / download this repository.
- Install all necessary dependencies (_Reactive_, _Akka_) via _NuGet_.
- Fill out the _appsettings.json.default_ and save it as _appsettings.json_.
- Extend the solution with your code.

## How to host
- Clone / download this repository.
- Install all necessary dependencies (_Reactive_, _Akka_) via _NuGet_.
- Fill out the _appsettings.json.default_ and save it as _appsettings.json_.
- Either build the _WebsocketGateway_ project or just run it.
- Connect to the websocket with your frontend, application etc.

## Websocket
The server sends the clients `FlightDataDto`s regularly or on events. 
It's accessible at _/websocket_.

## HTTP-API
The following endpoints can be accessed without authentication:

- _GET /api/current_  
Returns: `FlightDataDto[]`

- _GET /api/config_  
Returns `Config` (see _appsettings.json.default_ for further information): 


## DTOs
### FlightDataDto
```json
{
   "speed": 123.4, /* km/h */
   "altitude": 3456.7, /* m */
   "verticalSpeed": 2.3, /* m/s */
   "turnRate": 2.1, /* turns/min */
   "course": 123.0, /* degrees */
   "position": {
      "latitude": 123.4,
      "longitude": 111.1
   },
   "dateTime": "2012-04-23T18:25:43.511Z",
   "aircraft": {
      "id": "ABCD12",
      "callSign": "A1", /* or null */
      "registration": "D-XYZA", /* or null */
      "type": "Airbus A380" /* or null */
   },
   "flying": false /* as defined in appsettings.json */
}
```

### Config
```json
{
   "maxAgeSeconds": 20,
   "eventsOnly": false,
   "intervalSeconds": 2, /* or null */
   "filterPosition": {
         "latitude": 12.345,
         "longitude": 123.45
   },
   "filterRadius": 15
}       
```

## License
This code is licensed under the MIT-License (see _LICENSE.md_).
Before using this, make sure to not violate against OGN rules!

- [OGN data usage](https://www.glidernet.org/ogn-data-usage/)
- [ODbL summary](https://opendatacommons.org/licenses/odbl/summary/)
