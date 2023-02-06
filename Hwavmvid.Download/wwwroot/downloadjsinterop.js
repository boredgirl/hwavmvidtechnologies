export function initdownload() {

    var __obj = {

        downloadmap: function () {

            var __context = this;

            this.downloadcapturedvideoitem = function (filename, base64str) {

                var bytestring = atob(base64str);
                var arraybuffer = new ArrayBuffer(bytestring.length);
                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }
                var blob = new Blob([arraybuffer], { type: "video/mp4;codecs=h264" });
                var uri = URL.createObjectURL(blob);
                __context.invokedownload(filename, uri);
                URL.revokeObjectURL(uri);
            };

            this.invokedownload = function (filename, uri) {

                var aElement = document.createElement('a');
                aElement.href = uri;

                if (filename) {
                    aElement.download = filename;
                }

                aElement.click();
                aElement.remove();
            };
        }
    }

    return new __obj.downloadmap();
}
