export function initblazorvideoplayer(dotnetobjref, mapid) {

    var __obj = {

        blazorvideoplayermap: function (dotnetobjref, mapid) {
            
            var __selfblazorvideomap = this;
            this.remotelivestreamelementidprefix = '#remote-livestream-element-id-';

            this.videomimetypeobject = {

                get mimetype() {

                    var userAgent = navigator.userAgent.toLowerCase();

                    if (userAgent.indexOf('chrome') > -1 ||
                        userAgent.indexOf('firefox') > -1 ||
                        userAgent.indexOf('opera') > -1 ||
                        userAgent.indexOf('safari') > -1 ||
                        userAgent.indexOf('msie') > -1 ||
                        userAgent.indexOf('edge') > -1) {

                        return 'video/webm;codecs=opus,vp8';
                    }
                    else {

                        console.warn('using unknown browser'); return 'video/webm;codecs=opus,vp8';
                    }
                }
            };
            this.base64toblob = function (base64str) {

                //var bytestring = atob(base64str.split('base64,')[1]);
                var bytestring = atob(base64str);
                var arraybuffer = new ArrayBuffer(bytestring.length);

                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }

                var blob = new Blob([arraybuffer], { type: __selfblazorvideomap.videomimetypeobject.mimetype });
                return blob;
            };

            this.livestreams = [];
            this.getlivestream = function (mapid) {

                return __selfblazorvideomap.livestreams.find(item => item.id === mapid);
            };
            this.addlivestream = function (obj) {

                var item = __selfblazorvideomap.getlivestream(obj.id);
                if (item === undefined) {

                    __selfblazorvideomap.livestreams.push(obj);
                }
            };
            this.removelivestream = function (mapid) {

                //__selfblazorvideomap.livestreams = __selfblazorvideomap.livestreams.filter(item => item.id !== roomId);
                var livestream = __selfblazorvideomap.getlivestream(mapid);
                if (livestream !== undefined) {

                    __selfblazorvideomap.livestreams.splice(__selfblazorvideomap.livestreams.indexOf(livestream), 1);
                }
            };

            this.remotelivestream = function () {

                var __selfremotelivestream = this;

                this.videoelementid = __selfblazorvideomap.remotelivestreamelementidprefix + mapid;
                this.getvideoelement = function () {
                    return document.querySelector(__selfremotelivestream.videoelementid);
                };

                this.remotemediasequences = [];
                this.mediasource = new MediaSource();
                this.sourcebuffer = undefined;

                this.mediasource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(__selfblazorvideomap.videomimetypeobject.mimetype))) {

                        console.error('Unsupported MIME type or codec: ', __selfblazorvideomap.videomimetypeobject.mimetype);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediasource.addSourceBuffer(__selfblazorvideomap.videomimetypeobject.mimetype);
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
                this.mediasource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediasource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoelement();
                this.video.controls = true;
                this.video.autoplay = false;
                this.video.preload = 'auto';
                this.video.muted = true;

                this.video.addEventListener('mouseover', e => { this.video.setAttribute("controls", "controls"); });
                this.video.addEventListener('mouseout', e => { this.video.removeAttribute("controls"); });

                try {
                    this.video.srcObject = this.mediasource;
                } catch (ex) {
                    this.video.src = URL.createObjectURL(this.mediasource);
                }

                this.getNextSequenceInterval = function () {

                    setInterval(async function () {

                        var bufferedSeconds = __selfremotelivestream.sourcebuffer.timestampOffset - __selfremotelivestream.video.currentTime;
                        if (bufferedSeconds < 2.8) {
                            dotnetobjref.invokeMethodAsync('GetNextSequence', mapid);
                        }
                    }, 400);
                };
                this.getFirstSequence = function () {

                    dotnetobjref.invokeMethodAsync('GetNextSequence', mapid);
                };
                this.appendbuffer = function (base64str) {

                    try {

                        var blob = __selfblazorvideomap.base64toblob(base64str);
                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            var timeDiff = __selfremotelivestream.video.currentTime - __selfremotelivestream.sourcebuffer.timestampOffset;
                            if (!__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediasource.readyState === 'open') {

                                __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(reader.result));
                            }
                        }
                        reader.readAsArrayBuffer(blob);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };
                this.cancel = function () {

                    var promise = new Promise(function (resolve) {

                        try {

                            __selfremotelivestream.video.src = null;
                            resolve();
                        }
                        catch (err) {

                            console.log(err);
                            resolve();
                        }
                    });

                    return promise;
                };
            };
            this.initremotelivestream = function () {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(mapid);
                    if (livestream === undefined) {

                        var livestream = new __selfblazorvideomap.remotelivestream();
                        var livestreamdicitem = {

                            id: mapid,
                            item: livestream,
                        };

                        __selfblazorvideomap.addlivestream(livestreamdicitem);
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.startremotelivestream = function () {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(mapid);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                        livestream.item.getNextSequenceInterval();
                        livestream.item.video.play();
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.getfirstsequenceremotelivestream = function () {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(mapid);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                        livestream.item.getFirstSequence();
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.clearvideobufferremotelivestream = function () {

                var livestream = __selfblazorvideomap.getlivestream(mapid);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                    livestream.item.video.currentTime = livestream.item.video.currentTime + livestream.item.sourcebuffer.timestampOffset;
                }
            };
            this.appendbufferremotelivestream = function (base64str) {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(mapid);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                        livestream.item.appendbuffer(base64str);
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.closeremotelivestream = async function () {

                var livestream = __selfblazorvideomap.getlivestream(mapid);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                    await livestream.item.cancel();
                    __selfblazorvideomap.removelivestream(mapid);
                }
            };

        }
    }

    return new __obj.blazorvideoplayermap(dotnetobjref, mapid);
};
