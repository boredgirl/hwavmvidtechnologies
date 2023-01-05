export function initchathub () {

    var __obj = {

        chathubmap: function () {

            var __selfchathubmap = this;

            this.showchathubscontainer = function () {

                var chathubscontainer = document.querySelector('.chathubs-container');
                chathubscontainer.style.transition = "opacity 0.24s";
                chathubscontainer.style.opacity = "1";
            };

            this.consolelog = function (msg) {

                console.log(msg);
            };

            this.downloadcapturedvideoitem = function (filename, base64str) {

                var bytestring = atob(base64str);
                var arraybuffer = new ArrayBuffer(bytestring.length);

                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }

                var blob = new Blob([arraybuffer], { type: "video/webm;codecs=h264" });

                var uri = URL.createObjectURL(blob);
                __selfchathubmap.executefiledownload(filename, uri);
                URL.revokeObjectURL(uri);
            };

            this.executefiledownload = function (filename, uri) {

                var anhorelement = document.createElement('a');
                anhorelement.href = uri;

                if (filename) {
                    anhorelement.download = filename;
                }

                anhorelement.click();
                anhorelement.remove();
            };

        },
    }

    return new __obj.chathubmap();
}