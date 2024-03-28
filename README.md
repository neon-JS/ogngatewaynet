# ⚠️ Status
This project is being archived because it was superseded by [neon-JS/above_me](https://github.com/neon-JS/above_me).
Docker images are not deleted for compatibility reasons. As this means that neither dependency nor security updates
will be performed, it is strongly advised not to continue using OgnGateway.NET. 
Free to fork it & continue development! 

## OgnGateway.NET

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

## How to host

### Native
- Clone / download this repository.
- Configure _src/WebsocketGateway/appsettings.json_.
- `dotnet run`

### Docker
- Clone / download this repository.
- Configure _docker/docker-compose.yml_ or _src/WebsocketGateway/appsettings.json_.
- `cd docker && docker compose up`

## HTTP-API
The following endpoints can be accessed without authentication:

- _GET /api/current_  
Returns: `FlightDataDto[]`

- _GET /api/config_  
Returns `Config` 

- _GET /websocket_
Conntects to websocket which sends the clients `FlightDataDto`s regularly or on events. 

### DTOs
#### FlightDataDto
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

#### Config
See _src/WebsocketGateway/appsettings.json_ for further information.
```json
{
   "maxAgeSeconds": 20,
   "eventsOnly": false,
   "intervalSeconds": 2, /* or null */
   "filterPosition": {
         "latitude": 12.345,
         "longitude": 123.45
   },
   "filterRadius": 15 /* km */
}       
```

## License
This code is licensed under the MIT-License (see _LICENSE.md_).
Before using this, make sure to not violate against OGN rules!

- [OGN data usage](https://www.glidernet.org/ogn-data-usage/)
- [ODbL summary](https://opendatacommons.org/licenses/odbl/summary/)
