self.addEventListener("push", (event) => {
    let notificationData = event.data.json();
    let title = notificationData.title || "";
    let body = notificationData.body || "";
    let icon = notificationData.icon || "";
    let tag = notificationData.tag || "/";

    event.waitUntil(self.registration.showNotification(title,
        {
            body: body,
            icon: icon,
            tag: tag
        })
    );
});

self.addEventListener("notificationclick", event => {
    target = event.notification.tag;
    event.notification.close();

    event.waitUntil(clients.matchAll({ type: "window" })
        .then(clientList => {
            for (let i = 0; i < clientList.length; i++) {
                let client = clientList[i];
                if (client.url == target && "focus" in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(target);
            }
        }));
});
