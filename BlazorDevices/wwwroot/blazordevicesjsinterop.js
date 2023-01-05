export function initblazordevices(dotnetobjref) {

    var __obj = {

        blazordevicesmap: function (dotnetobjref) {

            var __selfblazordevicesmap = this;

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

                await __selfblazordevicesmap.requestpermissions();
                await __selfblazordevicesmap.setitems();

                var promise = new Promise(function (resolve) {

                    dotnetobjref.invokeMethodAsync("AddAudios", JSON.stringify(__selfblazordevicesmap.items.audios));
                    dotnetobjref.invokeMethodAsync("AddMicrophones", JSON.stringify(__selfblazordevicesmap.items.microphones));
                    dotnetobjref.invokeMethodAsync("AddWebcams", JSON.stringify(__selfblazordevicesmap.items.webcams));

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

                                    var audiodevice = new __selfblazordevicesmap.device(option.value, option.text);
                                    __selfblazordevicesmap.items.audios.push(audiodevice);
                                }
                                else if (deviceInfo.kind == 'audioinput') {
                                    option.text = deviceInfo.label || "Microphone " + (temp + 1);

                                    var microphonedevice = new __selfblazordevicesmap.device(option.value, option.text);
                                    __selfblazordevicesmap.items.microphones.push(microphonedevice);
                                }
                                else if (deviceInfo.kind == 'videoinput') {
                                    option.text = deviceInfo.label || "Camera " + (temp + 1);

                                    var webcamdevice = new __selfblazordevicesmap.device(option.value, option.text);
                                    __selfblazordevicesmap.items.webcams.push(webcamdevice);
                                }
                                else {
                                    console.log("Found another device: ", deviceInfo);
                                }
                            }

                            __selfblazordevicesmap.items.audios.push(
                                new __selfblazordevicesmap.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

                            __selfblazordevicesmap.items.microphones.push(
                                new __selfblazordevicesmap.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

                            __selfblazordevicesmap.items.webcams.push(
                                new __selfblazordevicesmap.device("0000000000000000000000000000000000000000000000000000000000000000", "None"));

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

    return new __obj.blazordevicesmap(dotnetobjref);
}
