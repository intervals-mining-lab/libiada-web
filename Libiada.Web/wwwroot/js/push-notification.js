"use strict";
let hasSubscription = false;
const WEB_API_URL = "api/TaskManagerApi/";
const SUBSCRIBE_URL = `${WEB_API_URL}Subscribe`;
const UNSUBSCRIBE_URL = `${WEB_API_URL}Unsubscribe`;
const APPLICATION_SERVER_KEY_URL = `${WEB_API_URL}GetApplicationServerKey`;
const PUSH_UNSUBSCRIBE_BUTTON = $("#push-unsubscribe-button");
const PUSH_SUBSCRIBE_BUTTON = $("#push-subscribe-button");
const urlB64ToUint8Array = (base64String) => {
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
    if (!buffer)
        throw new Error("Push subscription key is not available.");
    let binary = "";
    let bytes = new Uint8Array(buffer);
    let len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
};
const subscribeDevice = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    const responseKey = await fetch(APPLICATION_SERVER_KEY_URL);
    const body = await responseKey.json();
    const applicationServerKey = body.applicationServerKey;
    const options = {
        userVisibleOnly: true,
        applicationServerKey: urlB64ToUint8Array(applicationServerKey)
    };
    const subscription = await swRegistration.pushManager.subscribe(options);
    try {
        const request = {
            endpoint: subscription.endpoint,
            p256dh: arrayBufferToBase64(subscription.getKey("p256dh")),
            auth: arrayBufferToBase64(subscription.getKey("auth"))
        };
        const response = await fetch(SUBSCRIBE_URL, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(request)
        });
        hasSubscription = true;
        //PUSH_UNSUBSCRIBE_BUTTON.removeClass("invisible");
        PUSH_SUBSCRIBE_BUTTON.addClass("invisible");
        alertify.success("You have been subscribed to push notifications");
        return response;
    }
    catch {
        hasSubscription = false;
        return undefined;
    }
};
const unsubscribeDevice = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    const subscription = await swRegistration.pushManager.getSubscription();
    if (subscription) {
        try {
            const request = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ endpoint: subscription.endpoint })
            };
            const response = await fetch(UNSUBSCRIBE_URL, request);
            const unsubscribeSuccessful = await subscription.unsubscribe();
            if (unsubscribeSuccessful) {
                hasSubscription = false;
                PUSH_UNSUBSCRIBE_BUTTON.addClass("invisible");
                //PUSH_SUBSCRIBE_BUTTON.removeClass("invisible");
            }
            return response;
        }
        catch {
            hasSubscription = true;
            return undefined;
        }
    }
    return undefined;
};
const initPush = async () => {
    const swRegistration = await navigator.serviceWorker.ready;
    const subscription = await swRegistration.pushManager.getSubscription();
    hasSubscription = !(subscription === null);
    if (hasSubscription) {
        //PUSH_UNSUBSCRIBE_BUTTON.removeClass("invisible");
        PUSH_SUBSCRIBE_BUTTON.addClass("invisible");
    }
    else {
        PUSH_UNSUBSCRIBE_BUTTON.addClass("invisible");
        // PUSH_SUBSCRIBE_BUTTON.removeClass("invisible");
    }
};
const checkPushFunctionality = () => {
    if (!("serviceWorker" in navigator))
        throw new Error("Service Worker is not supported by your browser!");
    if (!("PushManager" in window))
        throw new Error("Push API is not supported by your browser!");
};
const main = async () => {
    checkPushFunctionality();
    try {
        const swRegistration = await navigator.serviceWorker.register("/sw-push-notification.js");
        await initPush();
    }
    catch (exception) {
        console.error("Service Worker Error");
        console.error(exception instanceof Error ? exception.message : String(exception));
    }
};
main();
//# sourceMappingURL=push-notification.js.map