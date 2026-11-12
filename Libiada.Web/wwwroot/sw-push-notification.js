"use strict";
self.addEventListener("push", (event) => {
    if (!event.data) {
        throw new Error("Push event does not contain notification data.");
    }
    const notificationData = event.data.json();
    const title = notificationData.title ?? "";
    const body = notificationData.body ?? "";
    const icon = notificationData.icon ?? "";
    const tag = notificationData.tag ?? "/";
    const options = {
        body: body,
        icon: icon,
        tag: tag
    };
    event.waitUntil(self.registration.showNotification(title, options));
});
self.addEventListener("notificationclick", (event) => {
    const target = event.notification.tag;
    event.notification.close();
    event.waitUntil(self.clients.matchAll({ type: "window" })
        .then((clientList) => {
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
//# sourceMappingURL=sw-push-notification.js.map