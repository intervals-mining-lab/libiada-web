interface ApplicationServerKeyResponse {
    applicationServerKey: string;
}

interface PushSubscriptionRequest {
    endpoint: string;
    p256dh: string;
    auth: string;
}

declare const alertify: {
    success(message: string): void;
};

let hasSubscription: boolean = false;
const WEB_API_URL: string = "api/TaskManagerApi/";
const SUBSCRIBE_URL : string= `${WEB_API_URL}Subscribe`;
const UNSUBSCRIBE_URL: string = `${WEB_API_URL}Unsubscribe`;
const APPLICATION_SERVER_KEY_URL: string = `${WEB_API_URL}GetApplicationServerKey`;
const PUSH_UNSUBSCRIBE_BUTTON: JQuery<HTMLElement> = $("#push-unsubscribe-button");
const PUSH_SUBSCRIBE_BUTTON: JQuery<HTMLElement> = $("#push-subscribe-button");

const urlB64ToUint8Array = (base64String: string): Uint8Array<ArrayBuffer> => {
    const padding: string = "=".repeat((4 - (base64String.length % 4)) % 4);
    const base64: string = (base64String + padding)
        .replace(/\-/g, "+")
        .replace(/_/g, "/");
    const rawData: string = atob(base64);
    const outputArray: Uint8Array<ArrayBuffer> = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; i++) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
};

const arrayBufferToBase64 = (buffer: ArrayBuffer | null): string => {
    if (!buffer) throw new Error("Push subscription key is not available.");

    let binary: string = "";
    let bytes: Uint8Array<ArrayBuffer> = new Uint8Array(buffer);
    let len: number = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }

    return window.btoa(binary);
};

const subscribeDevice = async (): Promise<Response | undefined> => {
    const swRegistration: ServiceWorkerRegistration = await navigator.serviceWorker.ready;
    const responseKey: Response = await fetch(APPLICATION_SERVER_KEY_URL);
    const body: ApplicationServerKeyResponse = await responseKey.json() as ApplicationServerKeyResponse;
    const applicationServerKey: string = body.applicationServerKey;

    const options: PushSubscriptionOptionsInit = {
        userVisibleOnly: true,
        applicationServerKey: urlB64ToUint8Array(applicationServerKey)
    };

    const subscription = await swRegistration.pushManager.subscribe(options);
    try {
        const request: PushSubscriptionRequest = {
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
    } catch {
        hasSubscription = false;
        return undefined;
    }
};

const unsubscribeDevice = async (): Promise<Response | undefined> => {
    const swRegistration: ServiceWorkerRegistration = await navigator.serviceWorker.ready;
    const subscription: PushSubscription | null = await swRegistration.pushManager.getSubscription();
    if (subscription) {
        try {
            const request: RequestInit = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ endpoint: subscription.endpoint })
            };
            const response: Response = await fetch(UNSUBSCRIBE_URL, request);
            const unsubscribeSuccessful: boolean = await subscription.unsubscribe();
            if (unsubscribeSuccessful) {
                hasSubscription = false;
                PUSH_UNSUBSCRIBE_BUTTON.addClass("invisible");
                //PUSH_SUBSCRIBE_BUTTON.removeClass("invisible");
            }
            return response;
        } catch {
            hasSubscription = true;
            return undefined;
        }
    }

    return undefined;
};

const initPush = async (): Promise<void> => {
    const swRegistration: ServiceWorkerRegistration = await navigator.serviceWorker.ready;
    const subscription: PushSubscription | null = await swRegistration.pushManager.getSubscription();
    hasSubscription = !(subscription === null);

    if (hasSubscription) {
        //PUSH_UNSUBSCRIBE_BUTTON.removeClass("invisible");
        PUSH_SUBSCRIBE_BUTTON.addClass("invisible");
    } else {
        PUSH_UNSUBSCRIBE_BUTTON.addClass("invisible");
        // PUSH_SUBSCRIBE_BUTTON.removeClass("invisible");
    }
}

const checkPushFunctionality = (): void => {
    if (!("serviceWorker" in navigator)) throw new Error("Service Worker is not supported by your browser!");
    if (!("PushManager" in window)) throw new Error("Push API is not supported by your browser!");
}

const main = async (): Promise<void> => {
    checkPushFunctionality();

    try {
        const swRegistration: ServiceWorkerRegistration = await navigator.serviceWorker.register("/sw-push-notification.js");
        await initPush();
    } catch (exception) {
        console.error("Service Worker Error");
        console.error(exception instanceof Error ? exception.message : String(exception));
    }
};
main();
