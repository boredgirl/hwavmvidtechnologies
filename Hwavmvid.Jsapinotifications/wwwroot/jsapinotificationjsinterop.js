export async function requestpermission() {

    var getaccess = function () {

        var promise = new Promise((resolve) => {

            Notification.requestPermission()
                .then((permission) => {

                    if (Notification.permission === 'denied' || Notification.permission === 'default') {

                        var denied = Notification.permission === 'denied' || Notification.permission === 'default';
                        resolve(!denied);
                    }

                    if (Notification.permission === 'granted') {

                        var granted = Notification.permission === 'granted';
                        resolve(granted);
                    }
                });
        });

        return promise;
    }

    return await getaccess();
}

export function shownotification(id, title, dir, lang, body, tag, icon, data) {

    var options = {

        dir: dir,
        lang: lang,
        body: body,
        tag: tag,
        icon: icon,
    };

    var notification = new Notification(title, options);

    /*
    notification.addEventListener('click',  event => { });
    notification.addEventListener('show',   event => { });
    notification.addEventListener('error',  event => { });
    notification.addEventListener('close',  event => { });
    */
}
