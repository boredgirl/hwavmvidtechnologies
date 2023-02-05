export function initdevices(dotnetobjref) {

    var __obj = {

        devicesmap: function (dotnetobjref) {

            var __context = this;

            this.device = function (id, name) {

                this.id = id;
                this.name = name;
            };

            this.items = {

                audios: [],
                microphones: [],
                webcams: [],
            };

            this.getitems = async function () {

                await __context.requestpermissions();
                await __context.setitems();

                var promise = new Promise(function (resolve) {

                    dotnetobjref.invokeMethodAsync("AddAudios", JSON.stringify(__context.items.audios));
                    dotnetobjref.invokeMethodAsync("AddMicrophones", JSON.stringify(__context.items.microphones));
                    dotnetobjref.invokeMethodAsync("AddWebcams", JSON.stringify(__context.items.webcams));

                    resolve();
                });

                return promise;
            };
            this.requestpermissions = async function () {

                navigator.mediaDevices
                    .getUserMedia({ video: true, audio: true })
                    .then((stream) => {
                        window.localStream = stream;
                        window.localAudio.srcObject = stream;
                        window.localAudio.autoplay = true;
                    })
                    .catch((err) => {
                        console.error(`you got an error: ${err}`);
                    });
            };
            this.setitems = async function () {

                var promise = new Promise(function (resolve) {

                    window.navigator.mediaDevices.enumerateDevices()
                        .then(function (mediadeviceinfos) {

                            for (var i = 0; i < mediadeviceinfos.length; i++) {

                                var temp = i;
                                var deviceInfo = mediadeviceinfos[temp];
                                var option = {};
                                option.value = deviceInfo.deviceId;
                                if (deviceInfo.kind == 'audiooutput') {
                                    option.text = deviceInfo.label || 'Speaker ' + (temp + 1);

                                    var audiodevice = new __context.device(option.value, option.text);
                                    __context.items.audios.push(audiodevice);
                                }
                                else if (deviceInfo.kind == 'audioinput') {
                                    option.text = deviceInfo.label || "Microphone " + (temp + 1);

                                    var microphonedevice = new __context.device(option.value, option.text);
                                    __context.items.microphones.push(microphonedevice);
                                }
                                else if (deviceInfo.kind == 'videoinput') {
                                    option.text = deviceInfo.label || "Camera " + (temp + 1);

                                    var webcamdevice = new __context.device(option.value, option.text);
                                    __context.items.webcams.push(webcamdevice);
                                }
                                else {
                                    console.log("Found another device: ", deviceInfo);
                                }
                            }

                            __context.items.audios.push(
                                new __context.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

                            __context.items.microphones.push(
                                new __context.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

                            __context.items.webcams.push(
                                new __context.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

                            resolve();
                        })
                        .catch(function (err) {

                            console.warn(err.message);
                        });
                });

                return promise;
            };

        }
    }

    return new __obj.devicesmap(dotnetobjref);
}
