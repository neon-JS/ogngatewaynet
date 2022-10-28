const { createApp } = Vue;

const API_HOST = `${window.location.origin}/api`;
const WEBSOCKET_HOST = `ws://${window.location.host}/websocket`;

createApp({
    data() {
        return {
            data: [],
            hasNotificationPermission: window.Notification && Notification.permission === 'granted',
            showFrame: false,
            iframeSrc: null,
            configuration: {
                maxAgeSeconds: Infinity,
                eventsOnly: false,
                intervalSeconds: 0,
                filterPosition: {
                    latitude: 0,
                    longitude: 0
                },
                filterRadius: 0
            }
        }
    },
    methods: {
        convertCoordinateToReadable(coordinate, isLatitude) {
            let degrees = Math.floor(coordinate);
            const minutes = Math.floor(coordinate * 100 % 100 * 0.6);
            const seconds = Math.floor(coordinate * 10000 % 100 * 0.6);

            let direction;
            if (isLatitude) {
                if (degrees > 0) {
                    direction = 'N';
                } else {
                    direction = 'S';
                    degrees *= -1;
                }
                degrees = degrees.toString().padStart(2, '0');
            } else {
                if (degrees > 0) {
                    direction = 'E';
                } else {
                    direction = 'W';
                    degrees *= -1;
                }
                degrees = degrees.toString().padStart(3, '0');
            }

            return `${degrees}° ${minutes.toString().padStart(2, '0')}' ${seconds.toString().padStart(2, '0')}" ${direction}`;
        },

        getIframeSrc() {
            if (!this.iframeSrc) {
                this.iframeSrc = `https://live.glidernet.org/#c=${this.configuration.filterPosition.latitude},${this.configuration.filterPosition.longitude}&z=15&s=1`;
            }
            return this.iframeSrc;
        },

        requestNotificationPermissions() {
            Notification.requestPermission(permission => 
                this.$nextTick(() => this.hasNotificationPermission = permission === 'granted')
            );
        },

        sendNotification(title, message, icon) {
            if (this.hasNotificationPermission) {
                return new Notification(title, { body: message, icon })
            }
        },

        updateFrameCoordinates(latitude, longitude) {
            this.$nextTick(() => this.iframeSrc = 'about:blank');
            
            setTimeout(() => this.$nextTick(() => {
                this.iframeSrc = `https://live.glidernet.org/#c=${latitude},${longitude}&z=15&s=1`;
                if (!this.showFrame) {
                    this.showFrame = true;
                }
            }), 10);
        },

        handleMessage(entry, isUpdate = true) {
            this.data = this.data || [];
            let outdatedIndex = this.data
                .findIndex(outdatedEntry => outdatedEntry.aircraft.id === entry.aircraft.id);

            if (outdatedIndex === -1) {
                if (isUpdate) {
                    const text = entry.aircraft.registration
                        ? `Die [${entry.aircraft.registration}] ist neu aufgetaucht.`
                        : 'Ein Flugzeug mit unbekannter Kennung ist neu aufgetaucht.';
                    this.convertCoordinateToReadablesendNotification(`Gliderradar: Neues Flugzeug`, text);
                }

                this.data.push(entry);
            } else {
                if (isUpdate) {
                    if (!this.data[outdatedIndex].flying && entry.flying) {
                        const text = entry.aircraft.registration
                            ? `Die [${entry.aircraft.registration}] ist gestartet.`
                            : 'Ein Flugzeug mit unbekannter Kennung ist gestartet.';
                        this.sendNotification(`Gliderradar: Start`, text);

                    } else if (this.data[outdatedIndex].flying && !entry.flying) {
                        const text = entry.aircraft.registration
                            ? `Die [${entry.aircraft.registration}] ist gelandet.`
                            : 'Ein Flugzeug mit unbekannter Kennung ist gelandet.';
                        this.sendNotification(`Gliderradar: Landung`, text);
                    }
                }

                this.data[outdatedIndex] = entry;

                if (isUpdate) {
                    this.sortEntries();
                }
            }
        },

        sortEntries() {
            this.data = this.data
                .sort((entry1, entry2) => {
                    if (entry1.aircraft.registration === '') return 1;
                    if (entry2.aircraft.registration === '') return -1;

                    return (entry1.aircraft.id > entry2.aircraft.id) ? 1 : -1;
                })
                .sort((entry1, entry2) => {
                    if (entry1.flying && !entry2.flying) return -1;
                    if (!entry1.flying && entry2.flying) return 1;
                    return 0;
                })
                .filter((entry) => 
                    (((new Date()).valueOf() - new Date(entry.dateTime).valueOf()) / 1000) 
                    <= this.configuration.maxAgeSeconds
                );
        },

        async connect() {
            const websocket = new WebSocket(WEBSOCKET_HOST);
            websocket.onmessage = message => this.handleMessage(JSON.parse(message.data));

            fetch(`${API_HOST}/current`)
                .then(b => b.json())
                .then(data => {
                    data.forEach(e => this.handleMessage(e, false));
                    this.sortEntries();
                });

            fetch(`${API_HOST}/config`)
                .then(b => b.json())
                .then(config => this.configuration = config);
        },
    },
    mounted() {
        this.connect();
    }
}).mount('#list');