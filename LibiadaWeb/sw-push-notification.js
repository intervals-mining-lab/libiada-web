self.addEventListener('push', (event) => {
    let notificationData = event.data.json();
    let title = notificationData.title || '';
    let body = notificationData.body || '';
    let icon = notificationData.icon || '';
    let tag = notificationData.tag || '/';

    event.waitUntil(
        self.registration.showNotification(title,
        {
            body: body,
            icon: icon,
            tag: tag
        })
    );
});

self.addEventListener('notificationclick', function (event) {
    target = event.notification.tag;
    event.notification.close();

    // This looks to see if the current is already open and
    // focuses if it is
    event.waitUntil(clients.matchAll({
        type: "window"
    }).then(function (clientList) {
        for (var i = 0; i < clientList.length; i++) {
            var client = clientList[i];
            if (client.url == target && 'focus' in client)
                return client.focus();
        }
        if (clients.openWindow)
            return clients.openWindow(target);
    }));
});