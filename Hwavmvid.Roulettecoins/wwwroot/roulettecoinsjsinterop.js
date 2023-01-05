export function initroulettecoins(dotnetobjref, elementId, type) {

    var __obj = {

        draggablelistmap: function (dotnetobjref, elementId) {

            this.addevents = function () {

                document.getElementById(elementId).addEventListener('dragstart', function (event) {

                    event.dataTransfer.effectAllowed = "move";

                    var id = event.target.id;
                    var arr = id.split('-');
                    var coinid = arr[arr.length - 1];

                    var exceptDropzone = '.roulettedropzone-' + coinid;
                    var dropzones = document.querySelectorAll('.roulettedropzone:not(' + exceptDropzone + ')');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "block";
                    });

                    event.dataTransfer.setData("coinid", coinid);
                });
                document.getElementById(elementId).addEventListener('dragend', function (event) {

                    var dropzones = document.getElementsByClassName('roulettedropzone');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "none";
                        item.classList.remove('active-roulettedropzone');
                    });
                });
            };
            this.removeevents = function () {

                document.getElementById(elementId).removeEventListener("dragstart", (item, e) => { });
                document.getElementById(elementId).removeEventListener("dragend", (item, e) => { });
            };
        },
        droppablelistmap: function (dotnetobjref, elementId) {

            this.addevents = function () {

                document.getElementById(elementId).addEventListener('dragenter', function (event) {

                    event.target.classList.add('active-roulettedropzone');
                });
                document.getElementById(elementId).addEventListener('dragleave', function (event) {

                    if (event.target.classList !== undefined) {

                        event.target.classList.remove('active-roulettedropzone');
                    }
                });
                document.getElementById(elementId).addEventListener('dragover', function (event) {

                    event.preventDefault();
                    event.dataTransfer.dropEffect = 'move';
                });
                document.getElementById(elementId).addEventListener('drop', function (event) {

                    event.preventDefault();

                    var id = event.target.id;
                    var arr = id.split('-');
                    var droppedfieldid = arr[arr.length - 1];

                    var coinid = event.dataTransfer.getData('coinid');
                    console.log(coinid, droppedfieldid)
                    dotnetobjref.invokeMethodAsync('ItemDropped', coinid, droppedfieldid);
                });
            };
            this.removeevents = function () {

                document.getElementById(elementId).removeEventListener("dragenter", (item, e) => {});
                document.getElementById(elementId).removeEventListener("dragleave", (item, e) => { });
                document.getElementById(elementId).removeEventListener("dragover", (item, e) => { });
                document.getElementById(elementId).removeEventListener("drop", (item, e) => { });
            };            
        },
    };

    if (type === "draggable")
        return new __obj.draggablelistmap(dotnetobjref, elementId);

    if (type === "droppable")
        return new __obj.droppablelistmap(dotnetobjref, elementId);

}