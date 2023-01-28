export function initgeolocationmap(dotnetobjref) {

    var __obj = {

        geomap: function (dotnetobjref) {

            __context = this;
            state = ""; // granted, prompt, denied
            geolocation: {

                latitude: 0;
                longitude: 0;
                altitude: 0;
                altitudeaccuracy: 0;
                accuracy: 0;
                heading: 0;
                speed: 0;
            };

            coordsobj: function() {

                this.latitude = __context.latitude;
                this.longitude = __context.longitude;
                this.altitude = __context.altitude;
                this.altitudeaccuracy = __context.altitudeaccuracy;
                this.accuracy = __context.accuracy;
                this.heading = __context.heading;
                this.speed = __context.speed;
            }
            requestpermissions = function () {

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

                            console.log("geo location permissions changed")
                            __context.state = result.state;
                            dotnetobjref.invokeMethodAsync("permissionschanged", result.state);
                        }
                    });
                });

                return promise;
            };
            requestcoords = async function () {

                if (__obj.state != "granted") {

                    await __context.requestpermissions();
                }

                var promise = new Promise((resolve) => {

                    const options = {
                        enableHighAccuracy: true,
                        timeout: 5000,
                        maximumAge: 0
                    };

                    function success(pos) {

                        var coords = pos.coords;
                        __context.latitude = coords.latitude;
                        __context.longitude = coords.longitude;
                        __context.altitude = coords.altitude;
                        __context.altitudeaccuracy = coords.altitudeaccuracy;
                        __context.accuracy = coords.accuracy;
                        __context.heading = coords.heading;
                        __context.speed = coords.speed;

                        var obj = new __context.coordsobj();
                        dotnetobjref.invokeMethodAsync("pushcoords", JSON.stringify(obj));
                    }

                    function error(err) {

                        console.log(err.message);
                    }

                    navigator.geolocation.getCurrentPosition(success, error, options);
                });

                return promise;
            };
            rendergooglemapposition = function (latitude, longitude, googlemapcanvasid) {

                var googlemapsmarkertitle = "Context department";
                var latitudelongitude = new google.maps.LatLng(latitude, longitude);

                var googlemapoptions = {
                    zoom: 16,
                    center: latitudelongitude,
                    mapTypeId: google.maps.MapTypeId.ROADMAP
                };

                var googlemapcanvas = document.getElementById(googlemapcanvasid);
                var map = new google.maps.Map(googlemapcanvas, googlemapoptions);

                var marker = new google.maps.Marker({
                    position: latitudelongitude,
                    map: map,
                    title: googlemapsmarkertitle
                });
            };
        }
    }
    return new __obj.geomap(dotnetobjref);
}
