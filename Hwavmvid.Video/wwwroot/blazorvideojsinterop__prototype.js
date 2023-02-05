export function livestreamitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    this.dotnetobjref = dotnetobjref;
    this.id1 = id1;
    this.id2 = id2;
    this.type = type;
    this.sourceType = sourceType;
    this.framerate = framerate;
    this.videoBitsPerSecond = videoBitsPerSecond;
    this.audioBitsPerSecond = audioBitsPerSecond;
    this.videoSegmentsLength = videoSegmentsLength;
    this.audioDefaultDeviceId = audioDefaultDeviceId;
    this.microphoneDefaultDeviceId = microphoneDefaultDeviceId;
    this.webcamDefaultDeviceId = webcamDefaultDeviceId;

    this.locallivestreamwebcamselementidprefix = '#local-livestream-element-id-';
    this.remotelivestreamelementidprefix = '#remote-livestream-element-id-';
    this.microsourcelocalid = '#local-livestream-micro-source-';
    this.audiosourcelocalid = '#local-livestream-audio-source-';
    this.videosourcelocalid = '#local-livestream-video-source-';
    this.videosourceurllocalidprefix = '#local-livestream-video-source-url-';

    this.videomimetypeobject = {

        mimetypelocallivestream: "video/webm;codecs=opus,vp8",
        mimetyperemotelivestream: "video/webm;codecs=opus,vp8",
    };
    this.base64toblob = function (base64str) {

        //var bytestring = atob(base64str.split('base64,')[1]);
        var bytestring = atob(base64str);
        var arraybuffer = new ArrayBuffer(bytestring.length);
        var bytes = new Uint8Array(arraybuffer);
        for (var i = 0; i < bytestring.length; i++) {
            bytes[i] = bytestring.charCodeAt(i);
        }
        var blob = new Blob([arraybuffer], { type: this.videomimetypeobject.mimetyperemotelivestream });
        return blob;
    };
    this.throwerror = function (message) {

        this.dotnetobjref.invokeMethodAsync('OnError', this.id1, this.id2, message);
    };

    this.livestreams = [];
    this.getlivestream = function (itemid1, itemid2) {

        return this.livestreams.find(item => item.id1 === itemid1 && item.id2 == itemid2);
    };
    this.addlivestream = function (obj) {

        this.removelivestream(obj.id1, obj.id2);
        var item = this.getlivestream(obj.id1, obj.id2);
        if (item === undefined) {

            this.livestreams.push(obj);
        }
    };
    this.removelivestream = function (id1, id2) {

        //this.livestreams = this.livestreams.filter(item => item.id !== roomId);
        var livestream = this.getlivestream(id1, id2);
        if (livestream !== undefined) {

            this.livestreams.splice(this.livestreams.indexOf(livestream), 1);
        }
    };

}

export function locallivestreamwebcamitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    livestreamitem.call(this, dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);

    this.locallivestreamwebcams = function (__selfblazorvideomap, audio, micro, video) {

        var __selflocallivestreamwebcams = this;

        this.nonedeviceselectedvalue = '0000000000000000000000000000000000000000000000000000000000000000';

        this.videoelementid = __selfblazorvideomap.locallivestreamwebcamselementidprefix + id1 + id2;
        this.getvideoelement = function () {
            return document.querySelector(__selflocallivestreamwebcams.videoelementid);
        };

        this.microsourcelocalid = __selfblazorvideomap.microsourcelocalid + id1 + id2;
        this.getmicrosourcelocaldomelement = function () {
            return document.querySelector(__selflocallivestreamwebcams.microsourcelocalid);
        };
        this.audiosourcelocalid = __selfblazorvideomap.audiosourcelocalid + id1 + id2;
        this.getaudiosourcelocaldomelement = function () {
            return document.querySelector(__selflocallivestreamwebcams.audiosourcelocalid);
        };
        this.videosourcelocalid = __selfblazorvideomap.videosourcelocalid + id1 + id2;
        this.getvideosourcelocaldomelement = function () {
            return document.querySelector(__selflocallivestreamwebcams.videosourcelocalid);
        };

        this.audio = audio;
        this.micro = micro;
        this.video = video;

        this.microselect = this.getmicrosourcelocaldomelement();
        this.audioselect = this.getaudiosourcelocaldomelement();
        this.videoselect = this.getvideosourcelocaldomelement();

        this.vElement = this.getvideoelement();
        this.vElement.onloadedmetadata = function (e) {

            __selflocallivestreamwebcams.vElement.play();
        };
        this.vElement.autoplay = true;
        this.vElement.controls = true;
        this.vElement.muted = true;

        this.vElement.addEventListener("pause", function () {

            __selflocallivestreamwebcams.pauselivestreamtask();
        });

        this.currentgotdevices = null;
        this.gotDevices = function (mediadeviceinfos) {

            var microselectchild = __selflocallivestreamwebcams.microselect.firstElementChild;
            while (microselectchild) {
                __selflocallivestreamwebcams.microselect.removeChild(microselectchild);
                microselectchild = __selflocallivestreamwebcams.microselect.firstElementChild;
            }

            var audioselectchild = __selflocallivestreamwebcams.audioselect.firstElementChild;
            while (audioselectchild) {
                __selflocallivestreamwebcams.audioselect.removeChild(audioselectchild);
                audioselectchild = __selflocallivestreamwebcams.audioselect.firstElementChild;
            }

            var videoselectchild = __selflocallivestreamwebcams.videoselect.firstElementChild;
            while (videoselectchild) {
                __selflocallivestreamwebcams.videoselect.removeChild(videoselectchild);
                videoselectchild = __selflocallivestreamwebcams.videoselect.firstElementChild;
            }

            var audioInputs = mediadeviceinfos.filter(item => item.kind === "audioinput");
            var audioOutputs = mediadeviceinfos.filter(item => item.kind === "audiooutput");
            var videoInputs = mediadeviceinfos.filter(item => item.kind === "videoinput");

            audioInputs.forEach(function (device, index, array) {

                const option = document.createElement("option");
                option.value = device.deviceId;

                option.text = device.label || "Microphone " + (__selflocallivestreamwebcams.microselect.length + 1);
                __selflocallivestreamwebcams.microselect.appendChild(option);

                if (index == 0) {
                    option.value = device.deviceId;
                    option.selected = "selected";
                }
            });
            audioOutputs.forEach(function (device, index, array) {

                const option = document.createElement("option");
                option.value = device.deviceId;

                option.text = device.label || 'Speaker ' + (__selflocallivestreamwebcams.audioselect.length + 1);
                __selflocallivestreamwebcams.audioselect.appendChild(option);

                if (index == 0) {
                    option.value = device.deviceId;
                    option.selected = "selected";
                }
            });
            videoInputs.forEach(function (device, index, array) {

                const option = document.createElement("option");
                option.value = device.deviceId;

                option.text = device.label || "Camera " + (__selflocallivestreamwebcams.videoselect.length + 1);
                __selflocallivestreamwebcams.videoselect.appendChild(option);

                if (index == 0) {
                    option.value = device.deviceId;
                    option.selected = "selected";
                }
            });

            var arr = [__selflocallivestreamwebcams.microselect, __selflocallivestreamwebcams.audioselect, __selflocallivestreamwebcams.videoselect];
            arr.forEach(function (element, index, array) {

                var option = document.createElement("option");
                option.text = "None";
                option.value = __selflocallivestreamwebcams.nonedeviceselectedvalue;
                element.appendChild(option);
            });

            __selflocallivestreamwebcams.audioselect.value = audio ?? audioDefaultDeviceId ?? __selflocallivestreamwebcams.audioselect.value;
            __selflocallivestreamwebcams.microselect.value = micro ?? microphoneDefaultDeviceId ?? __selflocallivestreamwebcams.microselect.value;
            __selflocallivestreamwebcams.videoselect.value = video ?? webcamDefaultDeviceId ?? __selflocallivestreamwebcams.videoselect.value;
        };

        this.currentgetstream = null;
        this.getStream = function (audiochanged, microchanged, videochanged) {

            var __selfgetstream = this;

            this.recorder = null;
            this.constrains = {
                audio: {
                    volume: { ideal: 1.0 },
                    echoCancellation: { ideal: true },
                    latency: { ideal: 1.0 },
                    noiseSuppression: { ideal: true },
                    autoGainControl: { ideal: true },
                },
                video: {
                    width: { min: 640, ideal: 640, max: 640 },
                    height: { min: 360, ideal: 360, max: 360 },
                    frameRate: { ideal: framerate },
                    facingMode: { ideal: "environment" },
                }
            };

            var audioDeviceIdConstraintValue = audiochanged ?? audio ?? audioDefaultDeviceId ?? __selflocallivestreamwebcams.audioselect.value;
            var microDeviceIdConstraintValue = microchanged ?? micro ?? microphoneDefaultDeviceId ?? __selflocallivestreamwebcams.microselect.value;
            var videoDeviceIdConstraintValue = videochanged ?? video ?? webcamDefaultDeviceId ?? __selflocallivestreamwebcams.videoselect.value;

            this.constrains.audio['deviceId'] = { exact: audioDeviceIdConstraintValue };
            this.constrains.audio['deviceId'] = { exact: microDeviceIdConstraintValue };
            this.constrains.video['deviceId'] = { exact: videoDeviceIdConstraintValue };

            window.navigator.mediaDevices
                .getUserMedia(this.constrains)
                .then(function (mediastream) {


                    __selflocallivestreamwebcams.vElement.srcObject = mediastream;

                    __selfgetstream.options = { mimeType: __selfblazorvideomap.videomimetypeobject.mimetypelocallivestream, audioBitsPerSecond: audioBitsPerSecond, videoBitsPerSecond: videoBitsPerSecond, ignoreMutedMedia: true };
                    __selfgetstream.recorder = new MediaRecorder(mediastream, __selfgetstream.options);

                    __selfgetstream.recorder.start();
                    __selfgetstream.recorder.ondataavailable = (event) => {

                        if (event.data.size > 0) {

                            __selflocallivestreamwebcams.broadcastvideodata(event.data);
                        }
                    };

                })
                .catch(function (err) {
                    __selfblazorvideomap.throwerror(err.message);
                });
        };

        this.initdevices = async function () {

            var promise = new Promise(function (resolve) {

                window.navigator.mediaDevices.enumerateDevices()
                    .then(function (mediadeviceinfos) {

                        if (__selflocallivestreamwebcams.currentgotdevices !== null) {

                            delete __selflocallivestreamwebcams.currentgotdevices;
                            __selflocallivestreamwebcams.currentgotdevices = null;
                        }

                        __selflocallivestreamwebcams.currentgotdevices = new __selflocallivestreamwebcams.gotDevices(mediadeviceinfos);
                        resolve();
                    })
                    .catch(function (err) {

                        __selfblazorvideomap.throwerror(err.message);
                    });
            });

            return promise;
        };
        this.initstream = function (audiochanged, microchanged, videochanged) {

            if (__selflocallivestreamwebcams.currentgetstream !== null) {

                __selflocallivestreamwebcams.currentgetstream = null;
                delete __selflocallivestreamwebcams.currentgetstream;
            }

            __selflocallivestreamwebcams.currentgetstream = new __selflocallivestreamwebcams.getStream(audiochanged, microchanged, videochanged);
        };

        this.startsequence = function () {

            try {

                if (__selflocallivestreamwebcams.currentgetstream?.recorder?.state === 'inactive' || __selflocallivestreamwebcams.currentgetstream?.recorder?.state === 'paused') {

                    __selflocallivestreamwebcams.currentgetstream.recorder.start();
                }
            }
            catch (err) {

                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.stopsequence = function () {

            try {

                if (__selflocallivestreamwebcams.currentgetstream?.recorder?.state === 'recording' || __selflocallivestreamwebcams.currentgetstream?.recorder?.state === 'paused') {

                    __selflocallivestreamwebcams.currentgetstream.recorder.stop();
                }
            }
            catch (err) {
                __selfblazorvideomap.throwerror(err.message);
            }
        };

        this.handleonchangeevent = async function () {

            dotnetobjref.invokeMethodAsync('OnUpdateDevices', id1, id2, __selflocallivestreamwebcams.audioselect.value, __selflocallivestreamwebcams.microselect.value, __selflocallivestreamwebcams.videoselect.value);
            await __selflocallivestreamwebcams.cancel();
            __selflocallivestreamwebcams.initstream(__selflocallivestreamwebcams.audioselect.value, __selflocallivestreamwebcams.microselect.value, __selflocallivestreamwebcams.videoselect.value);
        };

        this.microselect.removeEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);
        this.microselect.addEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);

        this.audioselect.removeEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);
        this.audioselect.addEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);

        this.videoselect.removeEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);
        this.videoselect.addEventListener("change", __selflocallivestreamwebcams.handleonchangeevent);

        this.broadcastvideodata = function (sequence) {

            var reader = new FileReader();
            reader.onloadend = async function (event) {

                var dataURI = event.target.result;

                var totalBytes = Math.ceil(event.total * 8 / 6);
                var totalKiloBytes = Math.ceil(totalBytes / 1024);
                if (totalKiloBytes >= 10000) {

                    var errmsg = 'data uri too large to broadcast >= 500kb';
                    __selfblazorvideomap.throwerror(errmsg);
                    return;
                }

                __selfblazorvideomap.dotnetobjref.invokeMethodAsync('OnDataAvailable', dataURI, __selfblazorvideomap.id1, __selfblazorvideomap.id2);
            };
            reader.readAsDataURL(sequence);
        };
        this.pauselivestreamtask = function () {

            dotnetobjref.invokeMethodAsync('PauseLivestreamTask', id1, id2);
        };
        this.takesnapshot = function () {
            var canvas = document.createElement("canvas");
            canvas.width = 640;
            canvas.height = 360;
            var context = canvas.getContext("2d");
            context.drawImage(__selflocallivestreamwebcams.vElement, 0, 0, canvas.width, canvas.height);
            var imageURI = canvas.toDataURL();
            return imageURI;
        };
        this.cancel = async function () {

            var promise = new Promise(function (resolve) {

                try {

                    if (__selflocallivestreamwebcams.currentgetstream != null) {

                        __selflocallivestreamwebcams.currentgetstream.recorder?.stream?.getTracks().forEach(track => track.stop());
                        __selflocallivestreamwebcams.currentgetstream.recorder?.stop();

                        delete __selflocallivestreamwebcams.currentgetstream;
                        __selflocallivestreamwebcams.currentgetstream = null;
                    }

                    __selflocallivestreamwebcams.vElement.srcObject = null;
                }
                catch (err) {

                    __selfblazorvideomap.throwerror(err.message);
                }
                finally {
                    resolve();
                }
            });

            return promise;
        };
    };
    this.initlocallivestreamwebcams = function (audio, micro, video) {

        try {

            this.getlivestream(this.id1, this.id2);
            if (livestream === undefined) {

                var livestream = new this.locallivestreamwebcams(this, audio, micro, video);
                var livestreamdicitem = {

                    id1: this.id1,
                    id2: this.id2,
                    item: livestream,
                };

                this.addlivestream(livestreamdicitem);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.initdeviceslocallivestreamwebcams = async function () {

        try {

            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

                await livestream.item.initdevices();
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startbroadcastinglocallivestreamwebcams = async function () {

        try {

            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

                await livestream.item.initstream(livestream.item.audio, livestream.item.micro, livestream.item.video);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startsequencelocallivestreamwebcams = function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

            livestream.item.startsequence();
        }
    };
    this.stopsequencelocallivestreamwebcams = function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

            livestream.item.stopsequence();
        }
    };
    this.takesnapshotlocallivestreamwebcams = function () {

        try {
            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

                var imageURI = livestream.item.takesnapshot();
                imageURI = imageURI.split('base64,')[1];
                return imageURI;
            }
        }
        catch (err) {
            this.throwerror(err.message);
        }
    };
    this.closelocallivestreamwebcams = async function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebcams) {

            await livestream.item.cancel();
            this.removelivestream(this.id1, this.id2);
        }
    };

}
export function locallivestreamwebsourceitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    livestreamitem.call(this, dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);

    this.locallivestreamwebsource = function (__selfblazorvideomap) {

        var __selflocallivestreamwebsource = this;

        this.videoelementid = __selfblazorvideomap.locallivestreamwebcamselementidprefix + id1 + id2;
        this.getvideoelement = function () {
            return document.querySelector(__selflocallivestreamwebsource.videoelementid);
        };

        this.videosourceurllocalid = __selfblazorvideomap.videosourceurllocalidprefix + id1 + id2;
        this.getvideosourceurllocaldomelement = function () {
            return document.querySelector(__selflocallivestreamwebsource.videosourceurllocalid);
        };

        this.vElement = this.getvideoelement();
        this.vElement.onloadedmetadata = function (e) {

            __selflocallivestreamwebsource.vElement.play();
        };
        this.vElement.autoplay = true;
        this.vElement.controls = true;
        this.vElement.muted = true;

        this.broadcastvideodata = function (sequence) {

            var reader = new FileReader();
            reader.onloadend = async function (event) {

                var dataURI = event.target.result;

                var totalBytes = Math.ceil(event.total * 8 / 6);
                var totalKiloBytes = Math.ceil(totalBytes / 1024);
                if (totalKiloBytes >= 10000) {

                    var errmsg = 'data uri too large to broadcast >= 500kb';
                    return;
                }

                __selfblazorvideomap.dotnetobjref.invokeMethodAsync('OnDataAvailable', dataURI, __selfblazorvideomap.id1, __selfblazorvideomap.id2);
            };
            reader.readAsDataURL(sequence);
        };
        this.webresourcestreaminstance = null;
        this.webresourcestream = function () {

            var __selfwebsourcestream = this;

            this.recorder = null;
            this.constrains = {
                audio: {
                    volume: { ideal: 1.0 },
                    echoCancellation: { ideal: true },
                    latency: { ideal: 1.0 },
                    noiseSuppression: { ideal: true },
                    autoGainControl: { ideal: true },
                },
                video: {
                    width: { ideal: 640 },
                    height: { ideal: 360 },
                    frameRate: { ideal: framerate },
                    facingMode: { ideal: "environment" },
                    displaySurface: { ideal: 'application' },
                }
            };

            this.constrains['deviceId'] = __selflocallivestreamwebsource.videoelementid;

            var urlInputElement = __selflocallivestreamwebsource.getvideosourceurllocaldomelement();
            var url = urlInputElement.value;

            this.mediasource = new MediaSource();
            this.sourcebuffer = undefined;

            __selfwebsourcestream.mediasource.addEventListener('sourceopen', function (event) {

                if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(__selfblazorvideomap.videomimetypeobject.mimetypelocallivestream))) {

                    console.warn('Unsupported MIME type or codec: ', __selfblazorvideomap.videomimetypeobject.mimetypelocallivestream);
                }

                fetch(url, { method: 'GET', mode: 'cors', referrerPolicy: 'no-referrer' })

                    .then(response => {
                        const reader = response.body.getReader();

                        return new ReadableStream({
                            start(controller) {
                                return pump();
                                function pump() {
                                    return reader.read().then(({ done, value }) => {

                                        if (done) {
                                            controller.close();
                                            return;
                                        }

                                        controller.enqueue(value);
                                        return pump();
                                    });
                                }
                            }
                        })
                    })
                    .then(stream => new Response(stream))
                    .then(response => {

                        /*
                        var bytearray = response.arrayBuffer();
                        var arraybuffer = new ArrayBuffer(bytes.length);
                        var int8array = new Uint8Array(arraybuffer);
                        for (var i = 0; i < bytearray.length; i++) {
                            int8array[i] = bytearray.charCodeAt(i);
                        }
                        var blob = new Blob([arraybuffer]);

                        __selfwebsourcestream.sourcebuffer = __selfwebsourcestream.mediasource.addSourceBuffer(__selfblazorvideomap.videomimetypeobject.mimetypelocallivestream);
                        __selfwebsourcestream.sourcebuffer.mode = 'segments';
                        __selflocallivestreamwebsource.appendbuffer(blob);
                        return blob;
                        */

                        return response.blob();
                    })
                    .then(blob => {

                        return URL.createObjectURL(blob);
                    })
                    .then(url => {

                        var vElementCanvas = document.createElement("canvas");
                        vElementCanvas.width = 640;
                        vElementCanvas.height = 360;

                        var vElementCanvasContext = vElementCanvas.getContext("2d");

                        __selflocallivestreamwebsource.vElement.addEventListener('loadeddata', function (e) {

                            var updateCanvas = function () {

                                vElementCanvasContext.drawImage(__selflocallivestreamwebsource.vElement, 0, 0);
                                requestAnimationFrame(updateCanvas);
                            };

                            updateCanvas();
                        });

                        __selflocallivestreamwebsource.vElement.addEventListener('canplay', function (e) {


                            __selflocallivestreamwebsource.vElement.play();
                            var mediastream = vElementCanvas.captureStream();

                            __selfwebsourcestream.options = { mimeType: __selfblazorvideomap.videomimetypeobject.mimetypelocallivestream, audioBitsPerSecond: audioBitsPerSecond, videoBitsPerSecond: videoBitsPerSecond, ignoreMutedMedia: true };
                            __selfwebsourcestream.recorder = new MediaRecorder(mediastream, __selfwebsourcestream.options);

                            __selfwebsourcestream.recorder.start();
                            __selfwebsourcestream.recorder.ondataavailable = (event) => {

                                if (event.data.size > 0) {

                                    __selflocallivestreamwebsource.broadcastvideodata(event.data);
                                }
                            };
                        });

                        __selflocallivestreamwebsource.vElement.src = url;
                    })
                    .catch(err => __selfblazorvideomap.throwerror(err.message));
            });

            __selflocallivestreamwebsource.vElement.src = URL.createObjectURL(__selfwebsourcestream.mediasource);

        };
        this.startsequence = function () {

            try {

                if (__selflocallivestreamwebsource.webresourcestreaminstance?.recorder?.state === 'inactive' || __selflocallivestreamwebsource.webresourcestreaminstance?.recorder?.state === 'paused') {

                    __selflocallivestreamwebsource.webresourcestreaminstance.recorder.start();
                }
            }
            catch (err) {

                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.stopsequence = function () {

            try {

                if (__selflocallivestreamwebsource.webresourcestreaminstance?.recorder?.state === 'recording' || __selflocallivestreamwebsource.webresourcestreaminstance?.recorder?.state === 'paused') {

                    __selflocallivestreamwebsource.webresourcestreaminstance.recorder.stop();
                }
            }
            catch (err) {
                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.takesnapshot = function () {
            var canvas = document.createElement("canvas");
            canvas.width = 640;
            canvas.height = 360;
            var context = canvas.getContext("2d");
            context.drawImage(__selflocallivestreamwebsource.vElement, 0, 0, canvas.width, canvas.height);
            var imageURI = canvas.toDataURL();
            return imageURI;
        };
        this.cancel = async function () {

            var promise = new Promise(function (resolve) {

                try {

                    if (__selflocallivestreamwebsource.webresourcestreaminstance != null) {

                        __selflocallivestreamwebsource.webresourcestreaminstance.recorder?.stream?.getTracks().forEach(track => track.stop());
                        __selflocallivestreamwebsource.webresourcestreaminstance.recorder?.stop();

                        delete __selflocallivestreamwebsource.webresourcestreaminstance;
                        __selflocallivestreamwebsource.webresourcestreaminstance = null;
                    }

                    __selflocallivestreamwebsource.vElement.srcObject = null;
                }
                catch (err) {

                    __selfblazorvideomap.throwerror(err.message);
                }
                finally {
                    resolve();
                }
            });

            return promise;
        };
    };
    this.initlocallivestreamwebsource = function () {

        try {

            this.getlivestream(this.id1, this.id2);
            if (livestream === undefined) {

                var livestream = new this.locallivestreamwebsource(this);
                var livestreamdicitem = {

                    id1: this.id1,
                    id2: this.id2,
                    item: livestream,
                };

                this.addlivestream(livestreamdicitem);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startvideolocallivestreamwebsource = function () {

        try {

            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebsource) {

                livestream.item.webresourcestreaminstance = new livestream.item.webresourcestream();
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startsequencelocallivestreamwebsource = function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebsource) {

            livestream.item.startsequence();
        }
    };
    this.stopsequencelocallivestreamwebsource = function () {

        var livestream = __selfblazorvideomap.getlivestream(id1, id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebsource) {

            livestream.item.stopsequence();
        }
    };
    this.takesnapshotlocallivestreamwebsource = function () {

        try {
            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebsource) {

                var imageURI = livestream.item.takesnapshot();
                imageURI = imageURI.split('base64,')[1];
                return imageURI;
            }
        }
        catch (err) {
            this.throwerror(err.message);
        }
    };
    this.closelocallivestreamwebsource = async function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebsource) {

            try {

                await livestream.item.cancel();
                this.removelivestream(this.id1, this.id2);
            }
            catch (err) {
                this.throwerror(err.message);
            }
        }
    };

}
export function locallivestreamwebscreenitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    livestreamitem.call(this, dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);

    this.locallivestreamwebscreen = function (__selfblazorvideomap) {

        var __selflocallivestreamwebscreen = this;

        this.videoelementid = __selfblazorvideomap.locallivestreamwebcamselementidprefix + id1 + id2;
        this.getvideoelement = function () {
            return document.querySelector(__selflocallivestreamwebscreen.videoelementid);
        };

        this.vElement = this.getvideoelement();
        this.vElement.onloadedmetadata = function (e) {

            __selflocallivestreamwebscreen.vElement.play();
        };
        this.vElement.autoplay = true;
        this.vElement.controls = true;
        this.vElement.muted = true;

        this.broadcastvideodata = function (sequence) {

            var reader = new FileReader();
            reader.onloadend = async function (event) {

                var dataURI = event.target.result;

                var totalBytes = Math.ceil(event.total * 8 / 6);
                var totalKiloBytes = Math.ceil(totalBytes / 1024);
                if (totalKiloBytes >= 10000) {

                    var errmsg = 'data uri too large to broadcast >= 500kb';
                    __selfblazorvideomap.throwerror(errmsg);
                    return;
                }

                __selfblazorvideomap.dotnetobjref.invokeMethodAsync('OnDataAvailable', dataURI, __selfblazorvideomap.id1, __selfblazorvideomap.id2);
            };
            reader.readAsDataURL(sequence);
        };
        this.webscreenstreaminstance = null;
        this.webscreenstream = function () {

            var __selfwebscreenstream = this;

            this.recorder = null;
            this.constrains = {
                audio: {
                    volume: { ideal: 1.0 },
                    echoCancellation: { ideal: true },
                    latency: { ideal: 1.0 },
                    noiseSuppression: { ideal: true },
                    autoGainControl: { ideal: true },
                    suppressLocalAudioPlayback: false,
                },
                video: {
                    width: { ideal: 640 },
                    height: { ideal: 360 },
                    frameRate: { ideal: framerate },
                    facingMode: { ideal: "environment" },
                    displaySurface: { ideal: 'application' },
                },
                surfaceSwitching: "include",
                selfBrowserSurface: "include",
                systemAudio: "include",
            };

            this.constrains['deviceId'] = __selflocallivestreamwebscreen.videoelementid;

            window.navigator.mediaDevices.getDisplayMedia(this.constrains)
                .then(function (mediastream) {

                    __selflocallivestreamwebscreen.vElement.srcObject = mediastream;

                    __selfwebscreenstream.options = { mimeType: __selfblazorvideomap.videomimetypeobject.mimetypelocallivestream, audioBitsPerSecond: audioBitsPerSecond, videoBitsPerSecond: videoBitsPerSecond, ignoreMutedMedia: true };
                    __selfwebscreenstream.recorder = new MediaRecorder(mediastream, __selfwebscreenstream.options);

                    __selfwebscreenstream.recorder.start();
                    __selfwebscreenstream.recorder.ondataavailable = (event) => {

                        if (event.data.size > 0) {

                            __selflocallivestreamwebscreen.broadcastvideodata(event.data);
                        }
                    };
                })
                .catch(err => {

                    __selfblazorvideomap.throwerror(err.message);
                });
        };
        this.startsequence = function () {

            try {

                if (__selflocallivestreamwebscreen.webscreenstreaminstance?.recorder?.state === 'inactive' || __selflocallivestreamwebscreen.webscreenstreaminstance?.recorder?.state === 'paused') {

                    __selflocallivestreamwebscreen.webscreenstreaminstance.recorder.start();
                }
            }
            catch (err) {

                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.stopsequence = function () {

            try {

                if (__selflocallivestreamwebscreen.webscreenstreaminstance?.recorder?.state === 'recording' || __selflocallivestreamwebscreen.webscreenstreaminstance?.recorder?.state === 'paused') {

                    __selflocallivestreamwebscreen.webscreenstreaminstance.recorder.stop();
                }
            }
            catch (err) {
                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.takesnapshot = function () {
            var canvas = document.createElement("canvas");
            canvas.width = 640;
            canvas.height = 360;
            var context = canvas.getContext("2d");
            context.drawImage(__selflocallivestreamwebscreen.vElement, 0, 0, canvas.width, canvas.height);
            var imageURI = canvas.toDataURL();
            return imageURI;
        };
        this.cancel = async function () {

            var promise = new Promise(function (resolve) {

                try {

                    if (__selflocallivestreamwebscreen.webscreenstreaminstance != null) {

                        __selflocallivestreamwebscreen.webscreenstreaminstance.recorder?.stream?.getTracks().forEach(track => track.stop());
                        __selflocallivestreamwebscreen.webscreenstreaminstance.recorder?.stop();

                        delete __selflocallivestreamwebscreen.webscreenstreaminstance;
                        __selflocallivestreamwebscreen.webscreenstreaminstance = null;
                    }

                    __selflocallivestreamwebscreen.vElement.srcObject = null;
                }
                catch (err) {

                    __selfblazorvideomap.throwerror(err.message);
                }
                finally {
                    resolve();
                }
            });

            return promise;
        };
    };
    this.initlocallivestreamwebscreen = function () {

        try {

            this.getlivestream(this.id1, this.id2);
            if (livestream === undefined) {

                var livestream = new this.locallivestreamwebscreen(this);
                var livestreamdicitem = {

                    id1: this.id1,
                    id2: this.id2,
                    item: livestream,
                };

                this.addlivestream(livestreamdicitem);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startvideolocallivestreamwebscreen = function () {

        try {

            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebscreen) {

                livestream.item.webscreenstreaminstance = new livestream.item.webscreenstream();
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.startsequencelocallivestreamwebscreen = function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebscreen) {

            livestream.item.startsequence();
        }
    };
    this.stopsequencelocallivestreamwebscreen = function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebscreen) {

            livestream.item.stopsequence();
        }
    };
    this.takesnapshotlocallivestreamwebscreen = function () {

        try {
            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebscreen) {

                var imageURI = livestream.item.takesnapshot();
                imageURI = imageURI.split('base64,')[1];
                return imageURI;
            }
        }
        catch (err) {
            this.throwerror(err.message);
        }
    };
    this.closelocallivestreamwebscreen = async function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.locallivestreamwebscreen) {

            try {

                await livestream.item.cancel();
                this.removelivestream(this.id1, this.id2);
            }
            catch (err) {
                this.throwerror(err.message);
            }
        }
    };

}
export function remotelivestreamitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    livestreamitem.call(this, dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);

    this.remotelivestream = function (__selfblazorvideomap) {

        var __selfremotelivestream = this;

        this.videoelementid = __selfblazorvideomap.remotelivestreamelementidprefix + id1 + id2;
        this.getvideoelement = function () {
            return document.querySelector(__selfremotelivestream.videoelementid);
        };

        this.remotemediasequences = [];
        this.mediasource = new MediaSource();
        this.sourcebuffer = undefined;

        this.mediasource.addEventListener('sourceopen', function (event) {

            if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(__selfblazorvideomap.videomimetypeobject.mimetyperemotelivestream))) {

                console.error('Unsupported MIME type or codec: ', __selfblazorvideomap.videomimetypeobject.mimetyperemotelivestream);
            }

            __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediasource.addSourceBuffer(__selfblazorvideomap.videomimetypeobject.mimetyperemotelivestream);
            __selfremotelivestream.sourcebuffer.mode = 'sequence';

            __selfremotelivestream.sourcebuffer.addEventListener('updatestart', function (e) { });
            __selfremotelivestream.sourcebuffer.addEventListener('update', function (e) { });
            __selfremotelivestream.sourcebuffer.addEventListener('updateend', function (e) {

                if (e.currentTarget.buffered.length == 1) {

                    var timestampOffset = __selfremotelivestream.sourcebuffer.timestampOffset;
                    var end = e.currentTarget.buffered.end(0);
                }
            });
        });
        this.mediasource.addEventListener('sourceended', function (event) { });
        this.mediasource.addEventListener('sourceclose', function (event) { });

        this.video = this.getvideoelement();
        this.video.controls = false;
        this.video.autoplay = true;
        this.video.preload = 'auto';
        this.video.muted = true;

        this.video.addEventListener('mouseover', e => { this.video.setAttribute("controls", "controls"); });
        this.video.addEventListener('mouseout', e => { this.video.removeAttribute("controls"); });

        try {
            this.video.srcObject = this.mediasource;
        } catch (err) {
            this.video.src = URL.createObjectURL(this.mediasource);
        }

        this.appendbuffer = function (base64str) {

            try {

                var blob = __selfblazorvideomap.base64toblob(base64str);

                var reader = new FileReader();
                reader.onloadend = function (event) {

                    var timeDiff = __selfremotelivestream.video.currentTime - __selfremotelivestream.sourcebuffer.timestampOffset;
                    if (timeDiff > __selfblazorvideomap.videoSegmentsLength) {
                        __selfremotelivestream.currentTime = __selfremotelivestream.currentTime + __selfblazorvideomap.videoSegmentsLength;
                    }

                    if (!__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediasource.readyState === 'open') {

                        __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(reader.result));
                    }
                }
                reader.readAsArrayBuffer(blob);
            }
            catch (err) {
                __selfblazorvideomap.throwerror(err.message);
            }
        };
        this.takesnapshot = function () {
            var canvas = document.createElement("canvas");
            canvas.width = 640;
            canvas.height = 360;
            var context = canvas.getContext("2d");
            context.drawImage(__selfremotelivestream.video, 0, 0, canvas.width, canvas.height);
            var imageURI = canvas.toDataURL();
            return imageURI;
        };
        this.cancel = async function () {

            var promise = new Promise(function (resolve) {

                try {

                    __selfremotelivestream.video.src = null;
                    resolve();
                }
                catch (err) {

                    __selfblazorvideomap.throwerror(err.message);
                }
            });

            return promise;
        };
    };
    this.initremotelivestream = function () {

        try {

            this.getlivestream(this.id1, this.id2);
            if (livestream === undefined) {

                var livestream = new this.remotelivestream();
                var livestreamdicitem = {

                    id1: this.id1,
                    id2: this.id2,
                    item: livestream,
                };

                this.addlivestream(livestreamdicitem);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.appendbufferremotelivestream = function (base64str) {

        try {

            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.remotelivestream) {

                livestream.item.appendbuffer(base64str);
            }
        }
        catch (err) {

            this.throwerror(err.message);
        }
    };
    this.takesnapshotremotelivestream = function () {

        try {
            var livestream = this.getlivestream(this.id1, this.id2);
            if (livestream !== undefined && livestream.item instanceof this.remotelivestream) {

                var imageURI = livestream.item.takesnapshot();
                imageURI = imageURI.split('base64,')[1];
                return imageURI;
            }
        }
        catch (err) {
            this.throwerror(err.message);
        }
    };
    this.closeremotelivestream = async function () {

        var livestream = this.getlivestream(this.id1, this.id2);
        if (livestream !== undefined && livestream.item instanceof this.remotelivestream) {

            await livestream.item.cancel();
            this.removelivestream(id1, id2);
        }
    };

}

export function initblazorvideo(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId) {

    if (type === "locallivestream") {

        if (sourceType === "webcams")
        {
            locallivestreamwebcamitem.prototype = new livestreamitem();
            locallivestreamwebcamitem.prototype.constructor = livestreamitem;

            return new locallivestreamwebcamitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);
        }
        if (sourceType === "websource")
        {
            locallivestreamwebsourceitem.prototype = new livestreamitem();
            locallivestreamwebsourceitem.prototype.constructor = livestreamitem;

            return new locallivestreamwebsourceitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);
        }
        if (sourceType === "webscreen")
        {
            locallivestreamwebscreenitem.prototype = new livestreamitem();
            locallivestreamwebscreenitem.prototype.constructor = livestreamitem;

            return new locallivestreamwebscreenitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);
        }
    }
    if (type === "remotelivestream") {

        remotelivestreamitem.prototype = new livestreamitem();
        remotelivestreamitem.prototype.constructor = livestreamitem;

        return new remotelivestreamitem(dotnetobjref, id1, id2, type, sourceType, framerate, videoBitsPerSecond, audioBitsPerSecond, videoSegmentsLength, audioDefaultDeviceId, microphoneDefaultDeviceId, webcamDefaultDeviceId);
    }
}
