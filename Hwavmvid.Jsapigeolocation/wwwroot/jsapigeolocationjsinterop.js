export function initgeolocationmap(dotnetobjref, componentid, elementid) {

    var __obj = {

        geomap: function (dotnetobjref, componentid, googlemapcanvasid) {

            var __context = this;
            this.state = ""; // granted, prompt, denied
            this.geolocation = {

                latitude: 0,
                longitude: 0,
                altitude: 0,
                altitudeaccuracy: 0,
                accuracy: 0,
                heading: 0,
                speed: 0,
            };

            this.coordsobj = function() {

                this.latitude = __context.latitude;
                this.longitude = __context.longitude;
                this.altitude = __context.altitude;
                this.altitudeaccuracy = __context.altitudeaccuracy;
                this.accuracy = __context.accuracy;
                this.heading = __context.heading;
                this.speed = __context.speed;
            };
            this.requestpermissions = function () {

                var promise = new Promise((resolve) => {

                    navigator.permissions.query({ name: 'geolocation' }).then(function (result) {

                        __context.state = result.state;

                        if (result.state == 'granted') {

                            console.log("geo location permissions granted")
                        }
                        else if (result.state == 'prompt') {

                            console.log("geo location permissions prompted");
                        }
                        else if (result.state == 'denied') {

                            console.log("geo location permissions denied");
                        }
                        result.onchange = function () {

                            __context.state = result.state;
                            dotnetobjref.invokeMethodAsync("Permissionschanged", componentid, result.state);
                        }

                        dotnetobjref.invokeMethodAsync("Permissionschanged", componentid, result.state);
                        resolve();
                    });
                });

                return promise;
            };
            this.requestcoords = function () {

                var promise = new Promise((resolve) => {

                    const options = {
                        enableHighAccuracy: true,
                        timeout: 5000,
                        maximumAge: 0
                    };

                    function success(position) {

                        var timestamp = position.timestamp;
                        var coords = position.coords;

                        __context.latitude = coords.latitude;
                        __context.longitude = coords.longitude;
                        __context.altitude = coords.altitude;
                        __context.altitudeaccuracy = coords.altitudeAccuracy;
                        __context.accuracy = coords.accuracy;
                        __context.heading = coords.heading;
                        __context.speed = coords.speed;

                        var obj = new __context.coordsobj();
                        console.log(JSON.stringify(obj));
                        dotnetobjref.invokeMethodAsync("Pushcoords", componentid, JSON.stringify(obj));
                        resolve();
                    }

                    function error(err) {

                        console.log(err.message);
                        resolve();
                    }

                    navigator.geolocation.getCurrentPosition(success, error, options);
                });

                return promise;
            };
            this.renderbingmapposition = function (latitude, longitude) {

                try
                {

                    var map = new Microsoft.Maps.Map("#" + elementid, {
                        //credentials: 'bing map key',
                        center: new Microsoft.Maps.Location(latitude, longitude)
                    });

                    var marker = new Microsoft.Maps.Pushpin(center, {
                        title: 'detected device',
                        subTitle: 'location granted though',
                        text: '17'
                    });

                    map.entities.push(marker);
                }
                catch (err) { console.log(err) };
            };
        }
    }
    return new __obj.geomap(dotnetobjref, componentid, elementid);
}
