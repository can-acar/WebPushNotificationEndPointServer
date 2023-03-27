const publicKey = "BDH4SxrOae8gKuHGIfjYbmedlB6y3ubO0b/MhmVNHUPS+CDcL2hQrMGxnEsHBe3YAnJ4sMPpztz6Ku9ut6QABIQ=";

Notification.requestPermission().then(permission => {
    if (permission === 'granted') {
        subscribeUserToPush().then(r => console.log(r));
    }
});


// ... The rest of the JavaScript code (e.g., urlBase64ToUint8Array function) ...
async function subscribeUserToPush() {
    const registration = await navigator.serviceWorker.ready;
    //const publicKey = 'YOUR_PUBLIC_VAPID_KEY';

    const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicKey),
    });

    // Send the subscription object to your server
    await fetch('/api/push-subscriptions', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(subscription),
    });
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}