export function initbingmapmap(dotnetobjref, componentid, elementid) {

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

            this.map = undefined;
            this.mapmarker = undefined;
            this.maprequestcounter = 0;

            this.renderbingmapposition = function (latitude, longitude) {

                try {
                    __context.maprequestcounter++;

                    var iselement = document.getElementById(elementid);
                    if (iselement != null && __context.map == undefined) {

                        __context.map = new Microsoft.Maps.Map("#" + elementid, {
                            //credentials: 'bing map key',
                            center: new Microsoft.Maps.Location(latitude, longitude),
                            zoom: 14,
                            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
                        });

                        __context.mapmarker = new Microsoft.Maps.Pushpin(__context.map.getCenter(), {
                            title: 'detected device',
                            subTitle: 'location granted though',
                            text: '17' + '[' + __context.maprequestcounter + ']',
                        });

                        __context.map.entities.push(__context.mapmarker);
                    }

                    if (iselement != null && __context.map != undefined) {

                        __context.map.setOptions({

                            center: new Microsoft.Maps.Location(latitude, longitude),
                        });

                        __context.mapmarker.setOptions({

                            text: '17' + '[' + __context.maprequestcounter + ']',
                        });
                    }
                }
                catch (err) { console.log(err) };
            };
        }
    }

    return new __obj.geomap(dotnetobjref, componentid, elementid);
}
