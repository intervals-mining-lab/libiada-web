let hasSubscription = false;
const SUBSCRIBE_URL = "PushNotification/Subscribe";
const UNSUBSCRIBE_URL = "PushNotification/Unsubscribe";
const APPLICATION_SERVER_KEY = "BHc-fO9M6yG_pZKFrZehXoNZNr6M6QGe48v6_nyoREJFij1kLIY_TVUzYnFqmFph2TPWjpF4fnCQmbJXRWdyFxU";
var pushButton = $(".btn-push-notification");

const urlB64ToUint8Array = base64String => {
    const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, "+")
        .replace(/_/g, "/");
    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
};

const arrayBufferToBase64 = (buffer) => {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}


const subscribeDevice = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    var options = {
        userVisibleOnly: true,
        applicationServerKey: urlB64ToUint8Array(APPLICATION_SERVER_KEY)
    };
    
    const subscription = await swRegistration.pushManager.subscribe(options);
    try {
        const response = await fetch(SUBSCRIBE_URL, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body:
                JSON.stringify({
                    'endpoint': subscription.endpoint,
                    'p256dh': arrayBufferToBase64(subscription.getKey("p256dh")),
                    'auth': arrayBufferToBase64(subscription.getKey("auth"))
                })
        });
        hasSubscription = true;
        updatePushButton();
        return response;
    }
    catch {
        hasSubscription = false;
    }
};

const unsubscribeDevice = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    const subscription = await swRegistration.pushManager.getSubscription();
    if (subscription) {
        try {
            const response = await fetch(UNSUBSCRIBE_URL, {
                method: "DELETE",
                headers: {
                    'Content-Type': 'application/json'
                },
                body:
                    JSON.stringify({
                        'endpoint': subscription.endpoint
                    })

            });
            const result = await subscription.unsubscribe();
            hasSubscription = false;
            updatePushButton();
            return response;
        }
        catch {
            hasSubscription = true;
        }
    }
};

const initPush = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
   
    const subscription = await swRegistration.pushManager.getSubscription();
    hasSubscription = !(subscription === null);

    pushButton.removeClass('hidden');
    updatePushButton();

    pushButton.on('click', function () {
        if (hasSubscription) {
            unsubscribeDevice();

        } else {
            subscribeDevice();
        }
    }); 
}

const updatePushButton = () => {
    console.log('promised');
    if (hasSubscription) {
        pushButton.text("Disable Push");
    } else {
        pushButton.text("Subscribe on Push");
    }
}

const check = () => {
    if (!('serviceWorker' in navigator)) {
        throw new Error('Service Worker is not supported by your browser!');
    }
    if (!('PushManager' in window)) {
        throw new Error('Push API is not supported by your browser!');
    }
}
    
const main = async () => {
    check();

    try {
        const swRegistration = await navigator.serviceWorker.register("sw-push-notification.js");
        await initPush();
    }
    catch {
        console.log('Service Worker Error');
    }
};
main();