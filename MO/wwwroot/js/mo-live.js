// Živé události chatu na každé stránce MO: odznak nepřečtených v docku + SignalR spojení.
// Běží jen když má uživatel chat k dispozici (<body data-mo-chat="1">).
// Ostatní skripty poslouchají:  window.addEventListener('mo:chat', function (ev) { ev.detail = událost ze serveru })
//                               window.addEventListener('mo:chat-state', ...)   - změna stavu spojení
(function () {
    if (!document.body || document.body.getAttribute('data-mo-chat') !== '1' || !window.moApi) return;

    var badge = document.getElementById('moChatBadge');
    var hub = null, timer = null;

    function setBadge(n) {
        if (!badge) return;
        n = parseInt(n, 10) || 0;
        badge.textContent = n > 99 ? '99+' : String(n);
        badge.hidden = n <= 0;
    }

    function refresh() {
        if (timer) return;
        timer = window.setTimeout(function () {
            timer = null;
            moApi.get('/Chat/UnreadCount').then(function (d) { if (d && d.ok) setBadge(d.count); }).catch(function () { });
        }, 400);
    }

    function connected() {
        return !!hub && typeof signalR !== 'undefined' && hub.state === signalR.HubConnectionState.Connected;
    }

    function fireState() {
        window.dispatchEvent(new CustomEvent('mo:chat-state', { detail: { connected: connected() } }));
    }

    function start() {
        if (typeof signalR === 'undefined') return;
        hub = new signalR.HubConnectionBuilder()
            .withUrl(moApi.url('/chathub'), { transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000, 60000])
            .build();
        hub.on('chat', function (e) {
            if (!e || e.type === 'ping') return;
            refresh();
            window.dispatchEvent(new CustomEvent('mo:chat', { detail: e }));
        });
        hub.onreconnecting(fireState);
        hub.onreconnected(function () { fireState(); refresh(); });
        hub.onclose(fireState);
        hub.start().then(fireState).catch(fireState);
    }

    window.moLive = { connected: connected, setBadge: setBadge, refresh: refresh };

    var s = document.createElement('script');
    s.src = moApi.url('/lib/signalr/signalr.min.js');
    s.onload = start;
    document.head.appendChild(s);

    // návrat do aplikace (telefon spojení na pozadí uspává): srovnat počet a případně znovu připojit
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState !== 'visible') return;
        refresh();
        if (hub && typeof signalR !== 'undefined' && hub.state === signalR.HubConnectionState.Disconnected) {
            hub.start().then(fireState).catch(fireState);
        }
    });
})();
