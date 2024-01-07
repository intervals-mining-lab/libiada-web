let hasSubscription = false;
const WEB_API_URL = "api/TaskManagerWebApi/";
const SUBSCRIBE_URL = `${WEB_API_URL}Subscribe`;
const UNSUBSCRIBE_URL = `${WEB_API_URL}Unsubscribe`;
const APPLICATION_SERVER_KEY_URL = `${WEB_API_URL}GetApplicationServerKey`;
const PUSH_UNSUBSCRIBE_BUTTON = $("#push-unsubscribe-button");
const PUSH_SUBSCRIBE_BUTTON = $("#push-subscribe-button");

const urlB64ToUint8Array = base64String => {
    const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, "+")
        .replace(/_/g, "/");
    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; i++) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
};

const arrayBufferToBase64 = (buffer) => {
    let binary = "";
    let bytes = new Uint8Array(buffer);
    let len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}


const subscribeDevice = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    const responseKey = await fetch(APPLICATION_SERVER_KEY_URL);
    const body = JSON.parse(await responseKey.json());
    const applicationServerKey = body.applicationServerKey;
    
    let options = {
        userVisibleOnly: true,
        applicationServerKey: urlB64ToUint8Array(applicationServerKey)
    };
    
    const subscription = await swRegistration.pushManager.subscribe(options);
    try {
        const response = await fetch(SUBSCRIBE_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                "endpoint": subscription.endpoint,
                "p256dh": arrayBufferToBase64(subscription.getKey("p256dh")),
                "auth": arrayBufferToBase64(subscription.getKey("auth"))
            })
        });
        hasSubscription = true;
        PUSH_SUBSCRIBE_BUTTON.removeClass('invisible');
        PUSH_SUBSCRIBE_BUTTON.addClass('invisible');
        alertify.success("You have been subscribed to push notifications");
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
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body:
                    JSON.stringify({ endpoint: subscription.endpoint })

            });
            const result = await subscription.unsubscribe();
            hasSubscription = false;
            PUSH_UNSUBSCRIBE_BUTTON.addClass('invisible');
            PUSH_SUBSCRIBE_BUTTON.removeClass('invisible');
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
    
    if (hasSubscription) {
        PUSH_UNSUBSCRIBE_BUTTON.removeClass('invisible');
        PUSH_SUBSCRIBE_BUTTON.addClass('invisible');
    }
}

const check = () => {
    if (!("serviceWorker" in navigator)) {
        throw new Error("Service Worker is not supported by your browser!");
    }
    if (!("PushManager" in window)) {
        throw new Error("Push API is not supported by your browser!");
    }
}
    
const main = async () => {
    check();

    try {
        const swRegistration = await navigator.serviceWorker.register("js/sw-push-notification.js"); 
        await initPush();
    }
    catch (exception) {
        console.log("Service Worker Error");
        console.log(exception);
    }
};
main();