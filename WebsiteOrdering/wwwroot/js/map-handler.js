document.addEventListener("DOMContentLoaded", function () {
    (async () => {
        const map = L.map('map', {
            scrollWheelZoom: true,
            zoomControl: true,
            attributionControl: true,
            fadeAnimation: true,
            zoomAnimation: true,
            markerZoomAnimation: true
        }).setView([10.770967, 106.667207], 12);

        const osmLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors',
            maxZoom: 19
        });

        const satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: 'Tiles © Esri',
            maxZoom: 19
        });

        osmLayer.addTo(map);
        let nearestStoreNotification = null;
        let userLocation = null;
        let currentLayer = 'osm';
        const defaultView = { lat: 10.770967, lng: 106.667207, zoom: 14 };
        let userLocationView = null;

        // Icons
        const googleStyleIcon = L.divIcon({
            html: `<svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="#EA4335">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5S10.62 6.5 12 6.5s2.5 1.12 2.5 2.5S13.38 11.5 12 11.5z"/>
            </svg>`,
            iconSize: [36, 36],
            iconAnchor: [18, 36],
            popupAnchor: [0, -36],
            className: ''
        });

        const userLocationIcon = L.divIcon({
            html: `<svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="#34A853">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5S10.62 6.5 12 6.5s2.5 1.12 2.5 2.5S13.38 11.5 12 11.5z"/>
            </svg>`,
            iconSize: [36, 36],
            iconAnchor: [18, 36],
            popupAnchor: [0, -36],
            className: ''
        });

        const markers = [];
        let userMarker = null;
        let currentRoute = null;
        let routeControl = null;

        // Load existing locations
        if (typeof locations !== 'undefined') {
            locations.forEach(loc => {
                const marker = L.marker([loc.latitude, loc.longitude], { icon: googleStyleIcon })
                    .addTo(map)
                    .bindPopup(`
                        <div style="text-align: center; min-width: 200px;">
                            <h4 style="margin: 0 0 10px 0; color: #333;">${loc.name}</h4>
                            <p style="margin: 0; color: #666;">📍 ${loc.address}</p>
                            <div style="margin-top: 12px; border-top: 1px solid #eee; padding-top: 12px;">
                                <button onclick="getDirections(${loc.latitude}, ${loc.longitude}, '${loc.name.replace(/'/g, "\\\'")}')" 
                                        style="background: #4285F4; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; font-size: 12px; margin: 2px;">
                                    🧭 Chỉ đường
                                </button>
                                <br>
                                <small style="color: #888;">Nhấp để xem chi tiết</small>
                            </div>
                        </div>
                    `);
                markers.push(marker);
            });
        }

        // Initialize user location
        async function initializeUserLocation() {
            try {
                // First, check if user location exists in session
                const sessionRes = await fetch('/Location/GetSessionLocation');

                if (sessionRes.ok) {
                    // User location exists in session
                    const userLoc = await sessionRes.json();
                    userLocation = {
                        lat: userLoc.latitude,
                        lng: userLoc.longitude
                    };
                    addUserLocationMarker(userLoc.latitude, userLoc.longitude, userLoc.address);

                    // Center map on user location
                    map.setView([userLoc.latitude, userLoc.longitude], 12);

                } else {
                    await requestUserLocation();
                }

                // find nearest store
                await findAndShowNearestStore();

            } catch (error) {
                console.error("❌ Error initializing user location:", error);
                // Fallback to default location
                console.log("📍 Using default location (Ho Chi Minh City)");
            }
        }

        // Request user location from browser
        async function requestUserLocation() {
            return new Promise((resolve, reject) => {
                if (!navigator.geolocation) {
                    reject(new Error("Geolocation not supported"));
                    return;
                }

                // Show loading indicator
                const loadingPopup = L.popup()
                    .setLatLng([10.770967, 106.667207])
                    .setContent('<div style="text-align: center;"><p>🔍 Đang xác định vị trí...</p></div>')
                    .openOn(map);

                navigator.geolocation.getCurrentPosition(
                    async (position) => {
                        const lat = position.coords.latitude;
                        const lng = position.coords.longitude;
                        console.log(`Kinh độ: ${lat}, Vĩ độ: ${lng}`);
                        userLocation = {
                            lat: position.coords.latitude,
                            lng: position.coords.longitude
                        };
                        try {
                            // Save to session with reverse geocoding
                            const saveRes = await fetch('/SetSessionLocation',
                            {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({ latitude: lat, longitude: lng })
                            });

                            if (saveRes.ok) {
                                const result = await saveRes.json();
                                addUserLocationMarker(result.latitude, result.longitude, result.address);
                                await findAndShowNearestStore();
                                // Center map on user location
                                map.setView([lat, lng], 12);

                                console.log("✅ User location detected and saved");
                                resolve(result);
                            } else {
                                throw new Error("Failed to save location");
                            }
                        } catch (error) {
                            console.error("❌ Error saving user location:", error);
                            reject(error);
                        } finally {
                            map.closePopup(loadingPopup);
                        }
                    },
                    (error) => {
                        map.closePopup(loadingPopup);
                        console.warn("⚠️ Location access denied or failed:", error.message);

                        // Show user-friendly message
                        L.popup()
                            .setLatLng([10.770967, 106.667207])
                            .setContent(`
                                <div style="text-align: center; color: #666;">
                                    <p>📍 Không thể xác định vị trí</p>
                                    <small>Nhấp vào bản đồ để chọn vị trí của bạn</small>
                                </div>
                            `)
                            .openOn(map);

                        reject(error);
                    },
                    {
                        enableHighAccuracy: true,
                        timeout: 10000,
                        maximumAge: 300000 // 5 minutes
                    }
                );
            });
        }
        //Get Route
        async function getRouteFromBackend(userLocation, store) {
            try {
                // Show loading indicator
                const loadingPopup = L.popup()
                    .setLatLng([userLocation.lat, userLocation.lng])
                    .setContent('<div style="text-align: center;"><p>Đang tìm đường...</p></div>')
                    .openOn(map);

                const response = await fetch('/Location/GetRoute', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        startLat: userLocation.lat,
                        startLng: userLocation.lng,
                        endLat: store.latitude,
                        endLng: store.longitude
                    })
                });

                map.closePopup(loadingPopup);

                if (response.ok) {
                    const routeData = await response.json();
                    console.log('routeData:', routeData); // Debug log
                    console.log('routeData.features:', routeData.route.features);
                    console.log('Full routeData structure:', JSON.stringify(routeData, null, 2));

                    // Display the route on map
                    displayRoute(routeData, store);

                    return routeData;
                }
                throw new Error('Backend routing failed');
            } catch (error) {
                console.error('❌ Route error:', error);
                // Show error message
                L.popup()
                    .setLatLng([userLocation.lat, userLocation.lng])
                    .setContent('<div style="text-align: center; color: red;"><p>❌ Không thể tìm đường</p></div>')
                    .openOn(map);
                throw error;
            }
        }
        // Replace your displayRoute function with this corrected version
        function displayRoute(routeData, store) {
            // Remove existing route if any
            if (currentRoute) {
                map.removeLayer(currentRoute);
            }
            if (routeControl) {
                map.removeControl(routeControl);
            }
            console.log(routeData);
            // Your backend returns GeoJSON format, so we need to handle it properly
            if (!routeData || !routeData.route.features || routeData.route.features.length === 0) {
                console.warn('⚠️ No route features found');
                return;
            }

            // Get the first feature (the route)
            const routeFeature = routeData.route.features[0];
            if (!routeFeature || !routeFeature.geometry || !routeFeature.geometry.coordinates) {
                console.warn('⚠️ No route coordinates found in feature');
                return;
            }

            // Extract coordinates from GeoJSON (format: [lng, lat])
            const coordinates = routeFeature.geometry.coordinates;

            // Convert to Leaflet format [lat, lng]
            const routeCoordinates = coordinates.map(coord => [coord[1], coord[0]]);

            // Create and add the route polyline
            currentRoute = L.polyline(routeCoordinates, {
                color: '#4285F4',
                weight: 5,
                opacity: 0.8,
                smoothFactor: 1
            }).addTo(map);

            // Fit map to show the entire route using the bbox from response
            if (routeData.bbox) {
                const bbox = routeData.bbox; // [minLng, minLat, maxLng, maxLat]
                const bounds = L.latLngBounds(
                    [bbox[1], bbox[0]], // southwest [lat, lng]
                    [bbox[3], bbox[2]]  // northeast [lat, lng]
                );
                map.fitBounds(bounds.pad(0.1));
            }

            const segment = routeFeature.properties.segments[0];
            const distance = (segment.distance / 1000).toFixed(2) + ' km';
            const duration = Math.round(segment.duration / 60) + ' phút'; 

            // Add route information control
            routeControl = L.control({ position: 'topleft' });
            routeControl.onAdd = function (map) {
                const div = L.DomUtil.create('div', 'route-info-control');
                div.innerHTML = `
            <div style="background: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); max-width: 300px;">
                <h4 style="margin: 0 0 10px 0; color: #333;">🗺️ Thông tin đường đi</h4>
                <p style="margin: 5px 0; color: #666;"><strong>Đến:</strong> ${store.name}</p>
                <p style="margin: 5px 0; color: #666;"><strong>Khoảng cách:</strong> ${distance}</p>
                <p style="margin: 5px 0; color: #666;"><strong>Thời gian:</strong> ${duration}</p>
                <div style="margin-top: 10px;">
                    <button onclick="clearRoute()" style="background: #EA4335; color: white; border: none; padding: 5px 10px; border-radius: 4px; cursor: pointer; font-size: 12px;">
                        ❌ Xóa đường đi
                    </button>
                </div>
            </div>
        `;
                return div;
            };
            routeControl.addTo(map);

            console.log('✅ Route displayed successfully');
            console.log('📊 Route info:', { distance, duration, coordinates: routeCoordinates.length + ' points' });
        }
        // Add user location marker
        function addUserLocationMarker(lat, lng, address) {
            // Remove existing user marker
            if (userMarker) {
                map.removeLayer(userMarker);
            }

            // Store user location for reset functionality
            userLocationView = { lat: lat, lng: lng, zoom: 15 };

            userMarker = L.marker([lat, lng], { icon: userLocationIcon })
                .addTo(map)
                .bindPopup(`
                    <div style="text-align: center; min-width: 200px;">
                        <h4 style="margin: 0 0 10px 0; color: #34A853;">📍 Vị trí của bạn</h4>
                        <p style="margin: 0; color: #666; font-size: 12px;">${address}</p>
                    </div>
                `)
                .openPopup();

            // Update reset button text to indicate user location is available
            updateResetButtonText();
        }

        // Update reset button text based on user location availability
        function updateResetButtonText() {
            const resetButton = document.getElementById('resetView');
            const resetText = document.getElementById('resetText');

            if (userLocationView && resetText) {
                resetText.textContent = 'Về vị trí của tôi';
                resetButton.title = 'Quay về vị trí hiện tại của bạn';
            } else if (resetText) {
                resetText.textContent = 'Về vị trí ban đầu';
                resetButton.title = 'Quay về vị trí mặc định (TP.HCM)';
            }
        }

        // Find and show nearest store
        async function findAndShowNearestStore() {
            try {
                // Clear previous notification
                clearNearestStoreNotification();

                const nearestRes = await fetch('/Location/GetNearestStore');

                if (nearestRes.ok) {
                    const nearestData = await nearestRes.json();

                    if (nearestData.success) {
                        const store = nearestData.store;
                        const distance = nearestData.distance.toFixed(2);

                        // Show notification
                        nearestStoreNotification = L.control({ position: 'topright' });
                        nearestStoreNotification.onAdd = function (map) {
                            const div = L.DomUtil.create('div', 'nearest-store-notification');
                            div.innerHTML = `
                                <div style="background: white; padding: 10px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.2);">
                                    <strong>🏪 Cửa hàng gần nhất:</strong><br>
                                    ${store.name} (${distance} km)
                                </div>
                            `;
                            return div;
                        };
                        nearestStoreNotification.addTo(map);

                        // Highlight nearest store marker
                        const nearestMarker = markers.find(m =>
                            m.getLatLng().lat === store.latitude &&
                            m.getLatLng().lng === store.longitude
                        );

                        if (nearestMarker) {
                            nearestMarker.bindPopup(`
                                <div style="text-align: center; min-width: 200px;">
                                    <h4 style="margin: 0 0 10px 0; color: #EA4335;">🏪 ${store.name}</h4>
                                    <p style="margin: 0; color: #666;">📍 ${store.address}</p>
                                    <div style="margin-top: 10px; padding-top: 10px; border-top: 1px solid #eee;">
                                        <strong style="color: #34A853;">Gần nhất: ${distance} km</strong>
                                    </div>
                                </div>
                            `).openPopup();
                        }
                        if (userLocation) {
                            try {
                                await getRouteFromBackend(userLocation, store);
                            } catch (error) {
                                console.warn("⚠️ Could not get route:", error.message);
                            }
                        } else {
                            console.warn("⚠️ User location chưa được thiết lập");
                        }
                    }
                }
            } catch (error) {
                console.error("❌ Error finding nearest store:", error);
            }
        }
        document.getElementById('findLocation').addEventListener('click', requestUserLocation);
        // Control buttons
        document.getElementById('resetView').addEventListener('click', function () {
            // Smart reset: prioritize user location, fallback to default
            if (userLocationView) {
                // Reset to user's location
                map.setView([userLocationView.lat, userLocationView.lng], userLocationView.zoom);

                // Open user marker popup to indicate reset location
                if (userMarker) {
                    userMarker.openPopup();
                }
            } else {
                // No user location, reset to default view
                map.setView([defaultView.lat, defaultView.lng], defaultView.zoom);
                console.log("Reset to default view (Ho Chi Minh City)");
            }
        });

        document.getElementById('toggleSatellite').addEventListener('click', function () {
            if (currentLayer === 'osm') {
                map.removeLayer(osmLayer);
                satelliteLayer.addTo(map);
                this.innerHTML = '<i class="icon">🗺️</i>Chế độ bản đồ';
                currentLayer = 'satellite';
            } else {
                map.removeLayer(satelliteLayer);
                osmLayer.addTo(map);
                this.innerHTML = '<i class="icon">🛰️</i>Chế độ vệ tinh';
                currentLayer = 'osm';
            }
        });

        // Fit bounds if multiple markers exist
        if (markers.length > 1) {
            const group = L.featureGroup(markers);
            map.fitBounds(group.getBounds().pad(0.1));
        }

        // Map click handler for adding new locations
        const selectedLocation = { lat: null, lng: null, address: '' };

        map.on('click', async function (e) {
            const lat = e.latlng.lat.toFixed(6);
            const lng = e.latlng.lng.toFixed(6);
            selectedLocation.lat = lat;
            selectedLocation.lng = lng;

            try {
                const res = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`);
                const data = await res.json();
                selectedLocation.address = data.display_name || "Không tìm thấy địa chỉ";

                const popupContent = `
                    <div style="text-align: center; min-width: 250px;">
                        <h4 style="margin: 0 0 10px 0; color: #333;">📍 Vị trí đã chọn</h4>
                        <p style="margin: 5px 0; color: #666;"><b>Tọa độ:</b> ${lat}, ${lng}</p>
                        <p style="margin: 5px 0; color: #666; font-size: 12px;"><b>Địa chỉ:</b> ${selectedLocation.address}</p>
                        <div style="margin-top: 15px; padding-top: 10px; border-top: 1px solid #eee;">
                            <button onclick="showSaveDialog('${lat}', '${lng}', '${selectedLocation.address.replace(/'/g, "\\\'")}')" 
                                    style="background: #4CAF50; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; font-size: 14px;">
                                💾 Lưu vị trí
                            </button>
                        </div>
                    </div>
                `;

                L.popup()
                    .setLatLng([lat, lng])
                    .setContent(popupContent)
                    .openOn(map);

            } catch (err) {
                console.error("❌ Reverse geocoding error:", err);
            }
        });

        // Save location function
        async function saveLocation(locationData) {
            try {
                const response = await fetch('/Location/SaveLocation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify(locationData)
                });

                const result = await response.json();

                if (result.success) {
                    //alert('✅ ' + result.message);
                    addNewMarkerToMap(result.location);
                } else {
                    alert('❌ ' + result.message);
                }
            } catch (error) {
                console.error('❌ Save error:', error);
                alert('❌ Có lỗi xảy ra khi lưu vị trí!');
            }
        }

        function addNewMarkerToMap(location) {
            marker = L.marker([location.latitude, location.longitude], { icon: googleStyleIcon })
                .addTo(map)
                .bindPopup(`
                    <div style="text-align: center; min-width: 200px;">
                        <h4 style="margin: 0 0 10px 0; color: #333;">${location.name}</h4>
                        <p style="margin: 0; color: #666;">📍 ${location.address}</p>
                        <div style="margin-top: 12px; border-top: 1px solid #eee; padding-top: 12px;">
                            <small style="color: #888;">ID: ${location.id}</small>
                        </div>
                    </div>
                `);
            markers.push(marker);
        }

        function showSaveDialog(lat, lng, address) {
            const locationName = prompt('Nhập tên cho vị trí này:', 'Vị trí mới');

            if (locationName && locationName.trim()) {
                const locationData = {
                    name: locationName.trim(),
                    address: address,
                    latitude: parseFloat(lat),
                    longitude: parseFloat(lng)
                };

                saveLocation(locationData);
            }
        }

        // Manual save user location button
        document.getElementById('btnSaveSessionLocation')?.addEventListener('click', async function () {
            if (!selectedLocation.lat || !selectedLocation.lng) {
                alert("⚠️ Vui lòng chọn vị trí trên bản đồ trước.");
                return;
            }

            try {
                const res = await fetch('/Location/SaveUserSessionLocation', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        latitude: selectedLocation.lat,
                        longitude: selectedLocation.lng,
                        address: selectedLocation.address
                    })
                });

                const result = await res.json();
                if (result.success) {
                    userLocation = {
                        lat: parseFloat(selectedLocation.lat),
                        lng: parseFloat(selectedLocation.lng)
                    };
                    addUserLocationMarker(selectedLocation.lat, selectedLocation.lng, selectedLocation.address);
                    await findAndShowNearestStore();
                //    alert('✅ ' + result.message);
                } else {
                    alert('❌ ' + result.message);
                }
            } catch (error) {
                console.error('❌ Session save error:', error);
                alert('❌ Lỗi khi gửi yêu cầu lưu vị trí.');
            }
        });

        // Make functions global
        window.showSaveDialog = showSaveDialog;

        // INITIALIZE USER LOCATION ON PAGE LOAD
        await initializeUserLocation();

        // Initialize reset button text
        updateResetButtonText();
        function clearRoute() {
            if (currentRoute) {
                map.removeLayer(currentRoute);
                currentRoute = null;
            }
            if (routeControl) {
                map.removeControl(routeControl);
                routeControl = null;
            }
            console.log('Route cleared');
        }
        function clearNearestStoreNotification() {
            if (nearestStoreNotification) {
                map.removeControl(nearestStoreNotification);
                nearestStoreNotification = null;
            }
        }
        window.clearRoute = clearRoute;
        //Search vị trí
        let debounceTimer;
        let suggestions = [];
        const input = document.getElementById('searchInput');
        const suggestionList = document.getElementById('suggestions');
        const loadingSpinner = document.getElementById('loadingSpinner');
        // Hàm gọi API Nominatim với error handling
        async function searchAddress(query) {
            try {
                loadingSpinner.style.display = 'block';

                // Ưu tiên tìm kiếm ở Việt Nam
                const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&addressdetails=1&limit=8&countrycodes=vn&accept-language=vi`;

                const response = await fetch(url);
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }

                const results = await response.json();
                return results;
            } catch (error) {
                console.error('Error searching address:', error);
                return [];
            } finally {
                loadingSpinner.style.display = 'none';
            }
        }
        // Hàm hiển thị suggestions
        function displaySuggestions(results) {
            suggestions = results;
            selectedIndex = -1;

            if (results.length === 0) {
                suggestionList.innerHTML = '<div class="no-results">Không tìm thấy kết quả phù hợp</div>';
                suggestionList.classList.add('show');
                return;
            }

            const html = results.map((place, index) => {
                const addressParts = place.display_name.split(',');
                const title = addressParts[0] || 'Không rõ';
                const address = addressParts.slice(1).join(',').trim() || 'Không rõ địa chỉ';

                return `
                    <div class="suggestion-item" data-index="${index}">
                        <div class="suggestion-icon">📍</div>
                        <div class="suggestion-content">
                            <div class="suggestion-title">${title}</div>
                            <div class="suggestion-address">${address}</div>
                        </div>
                    </div>
                `;
            }).join('');

            suggestionList.innerHTML = html;
            suggestionList.classList.add('show');

            // Thêm event listeners cho các suggestion items
            suggestionList.querySelectorAll('.suggestion-item').forEach((item, index) => {
                item.addEventListener('click', () => selectPlace(index));
                item.addEventListener('mouseenter', () => highlightSuggestion(index));
            });
        }
        // Ẩn suggestions khi click ra ngoài
        document.addEventListener('click', (e) => {
            if (!input.contains(e.target) && !suggestionList.contains(e.target)) {
                suggestionList.classList.remove('show');
                selectedIndex = -1;
            }
        });
        input.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            const query = e.target.value.trim();

            if (query.length < 2) {
                suggestionList.classList.remove('show');
                selectedLocation.classList.remove('show');
                return;
            }

            debounceTimer = setTimeout(async () => {
                const results = await searchAddress(query);
                displaySuggestions(results);
            }, 300);
        });
        // Focus vào input khi trang load
        window.addEventListener('load', () => {
            input.focus();
        });
    })();
});