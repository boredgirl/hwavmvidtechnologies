export function initblazorpager() {

    var __obj = {

        blazorpagermap: function () {

            this.scrollToElement = function (elementId) {

                try {
                    var item = document.getElementById(elementId);
                    if (item !== undefined) {

                        window.scrollTo(0, item.offsetTop);
                    }
                }
                catch (err) {
                    console.warn(err);
                }
            };
        }
    }

    return new __obj.blazorpagermap();
}