export class browserresizemap {

    constructor(dotnetobjref) {

        this.dotnetobjref = dotnetobjref;
    }

    async getInnerHeight () {

        return window.innerHeight;
    };
    async getInnerWidth () {

        return window.innerWidth;
    };
    async registerResizeCallback () {

        var __this = this;
        var resizeTimer;
        window.addEventListener('resize', function (e) {

            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {

                __this.resized();
            }, 200);
        });
    };
    resized () {

        this.dotnetobjref.invokeMethodAsync('OnBrowserResize');
    };
}