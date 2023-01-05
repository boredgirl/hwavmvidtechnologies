export function initmotorsportmap(dotnetobjref, elementId, type) {

    var __obj = {

        draggablelistmap: function (dotnetobjref, elementId) {

            this.addevents = function () {

                document.getElementById(elementId).addEventListener('dragstart', function (event) {
                    console.log("on drag");
                    event.dataTransfer.effectAllowed = "move";

                    var id = event.target.id;
                    var arr = id.split('-');
                    var obj_id = arr[arr.length - 1];

                    var exceptDropzone = '.motorsportdropzone-' + obj_id;
                    var dropzones = document.querySelectorAll('.motorsportdropzone:not(' + exceptDropzone + ')');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "block";
                    });

                    event.dataTransfer.setData("dropzonefieldelementid", obj_id);
                });
                document.getElementById(elementId).addEventListener('dragend', function (event) {

                    var dropzones = document.getElementsByClassName('motorsportdropzone');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "none";
                        item.classList.remove('active-motorsportdropzone');
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
                    console.log("on drag enter");
                    if (event.target.classList !== undefined) {

                        event.target.classList.add('active-motorsportdropzone');
                    }
                });
                document.getElementById(elementId).addEventListener('dragleave', function (event) {

                    if (event.target.classList !== undefined) {

                        event.target.classList.remove('active-motorsportdropzone');
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
                    var droppedfieldelementid = arr[arr.length - 1];

                    var mapitemtypeid = event.dataTransfer.getData('dropzonefieldelementid');
                    console.log(mapitemtypeid, droppedfieldelementid)
                    dotnetobjref.invokeMethodAsync('ItemDropped', mapitemtypeid, droppedfieldelementid);
                });
            };
            this.removeevents = function () {

                document.getElementById(elementId).removeEventListener("dragenter", (item, e) => { });
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