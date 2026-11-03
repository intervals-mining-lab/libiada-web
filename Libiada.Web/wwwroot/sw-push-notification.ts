/// <reference lib="webworker" />

interface PushNotificationData {
    title?: string;
    body?: string;
    icon?: string;
    tag?: string;
}

interface WorkerGlobalScopeEventMap {
    push: PushEvent;
    notificationclick: NotificationEvent;
}

interface WorkerGlobalScope {
    readonly registration: ServiceWorkerRegistration;
    readonly clients: Clients;
}

self.addEventListener("push", (event: PushEvent): void => {
    if (!event.data) {
        throw new Error("Push event does not contain notification data.");
    }

    const notificationData: PushNotificationData = event.data.json();
    const title: string = notificationData.title ?? "";
    const body: string = notificationData.body ?? "";
    const icon: string = notificationData.icon ?? "";
    const tag: string = notificationData.tag ?? "/";
    const options: NotificationOptions = {
        body: body,
        icon: icon,
        tag: tag
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener("notificationclick", (event: NotificationEvent): void => {
    const target: string = event.notification.tag;
    event.notification.close();

    event.waitUntil(self.clients.matchAll({ type: "window" })
        .then((clientList: readonly WindowClient[]) => {
            for (let i = 0; i < clientList.length; i++) {
                let client = clientList[i];
                if (client.url == target && "focus" in client) {
                    return client.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow(target);
            }

            return undefined;
        }));
});
