export function initroulettecoins(dotnetobjref, elementId, type) {

    var __obj = {

        draggablelistmap: function (dotnetobjref, elementId) {

            this.addevents = async function () {

                var jsitem = document.getElementById(elementId);
                if (jsitem != null) {

                    jsitem.addEventListener('dragstart', function (e) {

                        if (e.target != null) {

                            e.dataTransfer.effectAllowed = "move";

                            var id = e.target.id;
                            var arr = id.split('-');
                            var coinid = arr[arr.length - 1];

                            if (coinid != null) {

                                var exceptDropzone = '.roulettedropzone-' + coinid;
                                var dropzones = document.querySelectorAll('.roulettedropzone:not(' + exceptDropzone + ')');

                                if (dropzones != null) {

                                    Array.prototype.forEach.call(dropzones, function (item) {

                                        item.style.display = "block";
                                    });
                                }

                                e.dataTransfer.setData("coinid", coinid);
                            }
                        }
                    });
                    jsitem.addEventListener('dragend', function (e) {

                        var dropzones = document.getElementsByClassName('roulettedropzone');
                        if (dropzones != null) {

                            Array.prototype.forEach.call(dropzones, function (item) {

                                item.style.display = "none";
                                item.classList.remove('active-roulettedropzone');
                            });
                        }
                    });
                }
            };
            this.removeevents = async function () {

                var jsitem = document.getElementById(elementId);
                if (jsitem != null) {

                    try {
                        jsitem.removeEventListener("dragstart", (item, e) => { });
                        jsitem.removeEventListener("dragend", (item, e) => { });
                    } catch (err) { }
                }
            };
        },
        droppablelistmap: function (dotnetobjref, elementId) {

            this.addevents = async function () {

                var jsitem = document.getElementById(elementId);
                if (jsitem != null) {

                    jsitem.addEventListener('dragenter', function (e) {

                        if (e.target != null)
                            e.target.classList.add('active-roulettedropzone');
                    });
                    jsitem.addEventListener('dragleave', function (e) {

                        if (e.target !== null)
                            e.target.classList.remove('active-roulettedropzone');
                    });
                    jsitem.addEventListener('dragover', function (e) {

                        e.preventDefault();
                        e.dataTransfer.dropEffect = 'move';
                    });
                    jsitem.addEventListener('drop', function (e) {

                        e.preventDefault();
                        if (e.target != null) {

                            var id = e.target.id;
                            var arr = id.split('-');
                            var droppedfieldid = arr[arr.length - 1];

                            var coinid = e.dataTransfer.getData('coinid');
                            dotnetobjref.invokeMethodAsync('ItemDropped', coinid, droppedfieldid);
                        }
                    });
                }
            };
            this.removeevents = async function () {

                var jsitem = document.getElementById(elementId);
                if (jsitem != null) {

                    try {
                        jsitem.removeEventListener("dragenter", (item, e) => { });
                        jsitem.removeEventListener("dragleave", (item, e) => { });
                        jsitem.removeEventListener("dragover", (item, e) => { });
                        jsitem.removeEventListener("drop", (item, e) => { });
                    } catch (err) { }
                }
            };            
        },
    };

    if (type === "draggable")
        return new __obj.draggablelistmap(dotnetobjref, elementId);

    if (type === "droppable")
        return new __obj.droppablelistmap(dotnetobjref, elementId);

}