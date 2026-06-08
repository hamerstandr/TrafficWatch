const CACHE_NAME = 'trafficwatch-v1';
const urlsToCache = [
  '/',
  '/state/',
  '/state/index.html',
  '/state/css/bulma.css',
  '/state/css/My.css',
  '/state/js/vue.js',
  '/state/js/axios.js',
  '/state/js/app.js',
  '/state/Image/favicon.ico',
  '/state/Image/favicon_128.png',
  '/state/manifest.json'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        if (response) {
          return response;
        }
        return fetch(event.request);
      })
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME) {
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
});
