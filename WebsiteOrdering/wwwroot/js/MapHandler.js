class MapHandler {
    constructor() {
        this.map = null;
        this.locationService = new LocationService();
        //this.searchService = new SearchService();
        this.deliverySearchService = new SearchService();
        this.pickupSearchService = new SearchService();
        this.currentDeliveryMethod = window.initialDeliveryType || 'delivery';
        this.markers = [];
        this.userMarker = null;
        this.routeControl = null;
        this.isSatelliteMode = false;
        this.currentLayer = null;
        this.nearestStoreControl = null;
        this.routeInfoControl = null;
        this.selectedLocation = { lat: null, lng: null, address: '' };
        // Map layers
        this.osmLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        });

        this.satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: '© Esri'
        });
    }

    // Khởi tạo map
    async initialize() {
        try {
            // Khởi tạo map với view mặc định
            const defaultView = this.locationService.getDefaultView();
            this.map = L.map('map').setView([defaultView.lat, defaultView.lng], defaultView.zoom);

            // Thêm layer mặc định
            this.currentLayer = this.osmLayer;
            this.currentLayer.addTo(this.map);

            // Khởi tạo search service
            //this.searchService.initialize(
            //    'searchInput',
            //    'suggestions',
            //    'loadingSpinner',
            //    this.handlePlaceSelected.bind(this)
            //);
            this.deliverySearchService.initialize(
                'searchInput',
                'suggestions',
                'loadingSpinner',
                (lat, lng, address) => this.handlePlaceSelected(lat, lng, address, 'delivery')
            );

            this.pickupSearchService.initialize(
                'storeSearchInput',
                'storeSuggestions',
                'storeLoadingSpinner',
                (lat, lng, address) => this.handlePlaceSelected(lat, lng, address, 'pickup')
            );
            // Thêm locations từ model
            this.addLocationsToMap();

            // Bind events
            this.bindEvents();

            // Thử khởi tạo vị trí người dùng
            await this.initializeUserLocation();

            console.log('✅ Map initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing map:', error);
        }
    }

    // Khởi tạo vị trí người dùng
    async initializeUserLocation() {
        try {
            const userLocation = await this.locationService.initializeUserLocation();
            if (userLocation) {
                this.showUserLocation(userLocation.lat, userLocation.lng, userLocation.address);
                this.updateResetButtonText('Về vị trí của tôi');
            }
        } catch (error) {
            console.warn('⚠️ Could not get user location:', error.message);
            this.updateResetButtonText('Về vị trí ban đầu');
        }
    }

    // Thêm locations từ model vào map
    addLocationsToMap() {
        if (!locations || locations.length === 0) return;

        locations.forEach(location => {
            const marker = L.marker([location.latitude, location.longitude])
                .bindPopup(`
                    <div class="location-popup">
                        <h4>🏪 ${location.tencnhanh || 'Cửa hàng'}</h4>
                        <p><strong>📍 Địa chỉ:</strong> ${location.diachicn || 'Không có địa chỉ'}</p>
                        ${location.phone ? `<p><strong>📞 Điện thoại:</strong> ${location.phone}</p>` : ''}
                        <div class="popup-actions">
                            <button onclick="mapHandler.showRouteToLocation(${location.latitude}, ${location.longitude})" 
                                    class="popup-btn route-btn">🚗 Chỉ đường</button>
                        </div>
                    </div>
                `);

            this.markers.push(marker);
            marker.addTo(this.map);
        });

        // Fit bounds để hiển thị tất cả locations
        if (this.markers.length > 0) {
            const group = new L.featureGroup(this.markers);
            this.map.fitBounds(group.getBounds().pad(0.1));
        }
    }

    // Bind events cho các nút
    bindEvents() {
        // Nút reset view
        document.getElementById('resetView')?.addEventListener('click', () => {
            this.resetView();
        });

        // Nút toggle satellite
        document.getElementById('toggleSatellite')?.addEventListener('click', () => {
            this.toggleSatelliteMode();
        });

        // Nút find location
        document.getElementById('findLocation')?.addEventListener('click', () => {
            this.getUserLocation();
        });

        // Nút save session location
        document.getElementById('btnSaveSessionLocation')?.addEventListener('click', () => {
            this.saveCurrentLocationToSession();
        });

        // Nút clear route
        document.getElementById('btnClearRoute')?.addEventListener('click', () => {
            this.clearRoute();
        });

        // Map click event
        this.map.on('click', (e) => {
            this.handleMapClick(e);
        });
        document.getElementById('btnSelectStore')?.addEventListener('click', () => this.saveSelectedStoreToSession());
        //Nút đến trang giỏ hàng
        document.getElementById("btnCart")?.addEventListener('click', () => this.cartSite());
    }
    async cartSite() {
        window.location.href = "/Products/";
    }
    async saveSelectedStoreToSession() {
        const selected = document.querySelector('.store-item.selected');
        if (!selected) {
            alert("Vui lòng chọn một cửa hàng trước.");
            return;
        }

        const branchId = selected.dataset.storeId;
        const storeLat = parseFloat(selected.dataset.lat);
        const storeLng = parseFloat(selected.dataset.lng);
        const distanceKm = parseFloat(selected.dataset.distanceKm);
        const estimatedMinutes = parseInt(selected.dataset.estimatedMinutes);

        try {
            const res = await fetch('/Location/SaveSelectedStore', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    branchId: branchId,
                    deliveryMethod: "pickup",
                    distanceKm: distanceKm,
                    estimatedMinutes: estimatedMinutes
                })
            });

            const data = await res.json();
            if (data.success) {
                // Vẽ đường đi từ vị trí người dùng đến cửa hàng đã chọn
                await this.showRouteToLocation(storeLat, storeLng);
            } else {
                alert("Không thể lưu cửa hàng. Vui lòng thử lại.");
            }
        } catch (err) {
            console.error("❌ Lỗi khi lưu cửa hàng:", err);
            alert("Lỗi kết nối đến máy chủ.");
        }
    }
    // Xử lý khi chọn place từ search
    async handlePlaceSelected(lat, lng, address, mode = 'delivery') {
        try {
            // Lưu vị trí vào session
            const result = await this.locationService.saveSelectedLocationToSession(lat, lng, address, mode);

            if (result.success) {
                if (mode === 'delivery') {
                    // Hiển thị vị trí trên map
                    this.showUserLocation(lat, lng, address);

                    // Di chuyển map đến vị trí
                    this.map.setView([lat, lng], 15);
                    //Tìm đường đi
                    this.findNearestStore();
                    // Cập nhật text nút reset
                    this.updateResetButtonText('Về vị trí đã chọn');

                    this.showNotification('✅ Đã lưu vị trí thành công!', 'success');
                }
                else {
                    // Hiển thị vị trí trên map
                    this.showUserLocation(lat, lng, address);
                    // Di chuyển map đến vị trí
                    this.map.setView([lat, lng], 15);
                    // Cập nhật text nút reset
                    this.updateResetButtonText('Về vị trí đã chọn');
                    await this.loadSortedStores();
                }
                //Ẩn gợi ý
                //this.searchService.hideSuggestions();
                //Clear search
                //this.searchService.clearSearch();

            }
        } catch (error) {
            console.error('❌ Error handling place selection:', error);
            this.showNotification('❌ Có lỗi xảy ra khi lưu vị trí!', 'error');
        }
    }
    async loadSortedStores() {
        try {
            const res = await fetch('/Location/GetAllBranchesWithDistance');
            if (!res.ok) throw new Error('Lỗi khi gọi API');

            const stores = await res.json();

            const container = document.getElementById("storeList");
            container.innerHTML = ""; // reset

            stores.forEach(store => {
                const item = document.createElement("div");
                item.className = "store-item";
                item.dataset.storeId = store.idchinhanh;
                item.dataset.lat = store.latitude;
                item.dataset.lng = store.longitude;
                item.dataset.distanceKm = store.distanceKm;
                item.dataset.estimatedMinutes = store.estimatedMinutes;
                item.innerHTML = `
                <div class="store-icon">🏪</div>
                <div class="store-info">
                    <h3 class="store-name">${store.tencnhanh}</h3>
                    <p class="store-address">${store.diachicn}</p>
                    <p class="store-distance">📏 ${store.distanceKm} km</p>
                    <p class="store-estimate">🕒 Đi khoảng: ${store.estimatedTime}</p>
                </div>
            `;

                // Xử lý click chọn chi nhánh
                item.addEventListener("click", () => {
                    document.querySelectorAll(".store-item").forEach(i => i.classList.remove("selected"));
                    item.classList.add("selected");
                });

                container.appendChild(item);
            });
        } catch (err) {
            console.error("❌ Không tải được danh sách cửa hàng:", err);
        }
    }
    // Xử lý click trên map
    async handleMapClick(e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;
        try {
            // Reverse geocoding để lấy địa chỉ
            const address = await this.locationService.reverseGeocode(lat, lng);
            this.userLocationView = { lat: lat, lng: lng, address: address };
            console.log(this.userLocationView);
            // Hiển thị popup xác nhận
            const popup = L.popup()
                .setLatLng(e.latlng)
                .setContent(`
                    <div class="location-popup">
                        <h4>📍 Vị trí đã chọn</h4>
                        <p><strong>Địa chỉ:</strong> ${address}</p>
                    </div>
                `)
                .openOn(this.map);
        } catch (error) {
            console.error('❌ Error handling map click:', error);
        }
    }

    // Xác nhận chọn vị trí từ map click
    async confirmLocationSelection(lat, lng, address) {
        try {
            const result = await this.locationService.saveSelectedLocationToSession(lat, lng, address);

            if (result.success) {
                this.showUserLocation(lat, lng, address);
                this.updateResetButtonText('Về vị trí đã chọn');
                this.map.closePopup();
                this.showNotification('✅ Đã lưu vị trí thành công!', 'success');
            }
        } catch (error) {
            console.error('❌ Error confirming location selection:', error);
            this.showNotification('❌ Có lỗi xảy ra khi lưu vị trí!', 'error');
        }
    }

    // Hiển thị vị trí người dùng
    showUserLocation(lat, lng, address = '') {
        // Xóa marker cũ nếu có
        if (this.userMarker) {
            this.map.removeLayer(this.userMarker);
        }

        // Tạo marker mới
        this.userMarker = L.marker([lat, lng], {
            icon: L.divIcon({
                className: 'user-location-marker',
                html: '<div style="background: #4285F4; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3);"></div>',
                iconSize: [20, 20],
                iconAnchor: [10, 10]
            })
        }).bindPopup(`
            <div class="location-popup">
                <h4>📍 Vị trí của bạn</h4>
                ${address ? `<p><strong>Địa chỉ:</strong> ${address}</p>` : ''}
                <div class="popup-actions">
                    <button onclick="mapHandler.findNearestStore()" class="popup-btn nearest-btn">🏪 Tìm cửa hàng gần nhất</button>
                </div>
            </div>
        `);

        this.userMarker.addTo(this.map);
    }

    // Reset view
    resetView() {
        const userLocationView = this.locationService.getUserLocation();

        if (userLocationView) {
            this.map.setView([userLocationView.lat, userLocationView.lng], userLocationView.zoom);
        } else {
            const defaultView = this.locationService.getDefaultView();
            this.map.setView([defaultView.lat, defaultView.lng], defaultView.zoom);
        }
    }

    // Toggle satellite mode
    toggleSatelliteMode() {
        this.isSatelliteMode = !this.isSatelliteMode;

        // Remove current layer
        this.map.removeLayer(this.currentLayer);

        // Add new layer
        if (this.isSatelliteMode) {
            this.currentLayer = this.satelliteLayer;
            document.getElementById('toggleSatellite').innerHTML = '<span class="icon">🗺️</span><span>Chế độ bản đồ</span>';
        } else {
            this.currentLayer = this.osmLayer;
            document.getElementById('toggleSatellite').innerHTML = '<span class="icon">🛰️</span><span>Chế độ vệ tinh</span>';
        }

        this.currentLayer.addTo(this.map);
    }

    // Lấy vị trí người dùng
    async getUserLocation() {
        try {
            this.showNotification('📍 Đang lấy vị trí của bạn...', 'info');

            const userLocation = await this.locationService.requestBrowserLocation();
            this.userLocationView = userLocation;
            if (userLocation) {
                this.showUserLocation(userLocation.lat, userLocation.lng, userLocation.address);
                this.map.setView([userLocation.lat, userLocation.lng], 15);
                this.updateResetButtonText('Về vị trí của tôi');
                this.findNearestStore();
                this.showNotification('✅ Đã lấy vị trí thành công!', 'success');
                this.loadSortedStores();
            }
        } catch (error) {
            console.error('❌ Error getting user location:', error);
            this.showNotification('❌ Không thể lấy vị trí của bạn. Vui lòng cho phép truy cập vị trí!', 'error');
        }
    }

    // Lưu vị trí hiện tại vào session
    async saveCurrentLocationToSession() {
        try {
            const userLocation = this.userLocationView;
            if (!userLocation) {
                this.showNotification('⚠️ Vui lòng chọn vị trí trước!', 'warning');
                return;
            }
            const result = await this.locationService.saveSelectedLocationToSession(
                userLocation.lat,
                userLocation.lng,
                userLocation.address
            );

            if (result.success) {
                this.showUserLocation(userLocation.lat,
                    userLocation.lng,
                    userLocation.address);
                this.findNearestStore();
                this.showNotification('✅ Đã lưu vị trí vào session thành công!', 'success');
            }
        } catch (error) {
            console.error('❌ Error saving location to session:', error);
            this.showNotification('❌ Có lỗi xảy ra khi lưu vị trí!', 'error');
        }
    }

    // Tìm cửa hàng gần nhất
    async findNearestStore() {
        try {
            this.showNotification('🔍 Đang tìm cửa hàng gần nhất...', 'info');

            const nearestStore = await this.locationService.findNearestStore();

            if (nearestStore && nearestStore.success) {
                const store = nearestStore.store;

                // Hiển thị thông tin cửa hàng
                //this.showNearestStoreInfo(store);

                // Hiển thị route đến cửa hàng
                await this.showRouteToLocation(store.latitude, store.longitude);

                this.showNotification('✅ Đã tìm thấy cửa hàng gần nhất!', 'success');
            } else {
                this.showNotification('⚠️ Không tìm thấy cửa hàng nào gần bạn!', 'warning');
            }
        } catch (error) {
            console.error('❌ Error finding nearest store:', error);
            this.showNotification('❌ Có lỗi xảy ra khi tìm cửa hàng!', 'error');
        }
    }

    // Hiển thị thông tin cửa hàng gần nhất
    showNearestStoreInfo(store) {
        // Remove existing control if any
        if (this.nearestStoreControl) {
            this.map.removeControl(this.nearestStoreControl);
        }

        this.nearestStoreControl = L.control({ position: 'topright' });
        this.nearestStoreControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'nearest-store-notification');
            div.innerHTML = `
                <div>
                    <h4>🏪 Cửa hàng gần nhất</h4>
                    <p><strong>${store.tencnhanh || 'Cửa hàng'}</strong></p>
                    <p>📍 ${store.diachicn || 'Không có địa chỉ'}</p>
                    <p>📏 Khoảng cách: ${store.distance ? store.distance.toFixed(2) + ' km' : 'Không xác định'}</p>
                </div>
            `;
            return div;
        };

        this.nearestStoreControl.addTo(this.map);

        // Auto remove after 10 seconds
        setTimeout(() => {
            if (this.nearestStoreControl) {
                this.map.removeControl(this.nearestStoreControl);
                this.nearestStoreControl = null;
            }
        }, 10000);
    }

    // Hiển thị đường đi đến một location
    async showRouteToLocation(endLat, endLng) {
        try {
            const userLocation = this.locationService.getUserLocation();
            console.log(userLocation);
            if (!userLocation) {
                this.showNotification('⚠️ Vui lòng chọn vị trí của bạn trước!', 'warning');
                return;
            }

            this.showNotification('🔍 Đang tính toán đường đi...', 'info');

            // Clear existing route
            this.clearRoute();

            const routeData = await this.locationService.getRoute(
                userLocation.lat, userLocation.lng,
                endLat, endLng
            );

            if (routeData && routeData.success) {
                this.displayRoute(routeData.route);
                this.showNotification('✅ Đã tìm thấy đường đi!', 'success');
            } else {
                this.showNotification('❌ Không thể tìm thấy đường đi!', 'error');
            }
        } catch (error) {
            console.error('❌ Error showing route:', error);
            this.showNotification('❌ Có lỗi xảy ra khi tính đường đi!', 'error');
        }
    }

    displayRoute(routeData) {
        if (!routeData || !routeData.features || !routeData.features[0]?.geometry?.coordinates) return;

        const rawCoords = routeData.features[0].geometry.coordinates;
        const routeCoordinates = rawCoords.map(coord => [coord[1], coord[0]]); // ⚠️ chuyển từ [lng, lat] -> [lat, lng]

        // Remove existing route if any
        if (this.routeControl) {
            this.map.removeLayer(this.routeControl);
        }

        this.routeControl = L.polyline(routeCoordinates, {
            color: '#4285F4',
            weight: 5,
            opacity: 0.8
        }).addTo(this.map);

        this.map.fitBounds(this.routeControl.getBounds().pad(0.1));

        this.showRouteInfo(routeData);
    }

    // Hiển thị thông tin route
    showRouteInfo(routeData) {
        // Remove existing control if any
        if (this.routeInfoControl) {
            this.map.removeControl(this.routeInfoControl);
        }

        const segment = routeData.features[0]?.properties?.segments?.[0];
        const distance = segment ? (segment.distance / 1000).toFixed(2) : 'N/A';
        const duration = segment ? Math.round(segment.duration / 60) : 'N/A';

        this.routeInfoControl = L.control({ position: 'bottomleft' });
        this.routeInfoControl.onAdd = function () {
            const div = L.DomUtil.create('div', 'route-info-control');
            div.innerHTML = `
            <div style="background:white;padding:10px;border-radius:6px;">
                <h4>🚗 Thông tin đường đi</h4>
                <p>📏 Khoảng cách: ${distance} km</p>
                <p>⏱️ Thời gian: ${duration} phút</p>
                <button onclick="mapHandler.clearRoute()">❌ Xóa đường đi</button>
            </div>
        `;
            return div;
        };

        this.routeInfoControl.addTo(this.map);
    }
    // Xóa route
    clearRoute() {
        if (this.routeControl) {
            this.map.removeLayer(this.routeControl);
            this.routeControl = null;
        }

        if (this.routeInfoControl) {
            this.map.removeControl(this.routeInfoControl);
            this.routeInfoControl = null;
        }

        if (this.nearestStoreControl) {
            this.map.removeControl(this.nearestStoreControl);
            this.nearestStoreControl = null;
        }

        this.showNotification('✅ Đã xóa đường đi!', 'success');
    }

    // Update reset button text
    updateResetButtonText(text) {
        const resetTextElement = document.getElementById('resetText');
        if (resetTextElement) {
            resetTextElement.textContent = text;
        }
    }

    // Show notification
    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : type === 'warning' ? '#ffc107' : '#17a2b8'};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            z-index: 10000;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 14px;
            max-width: 300px;
            animation: slideIn 0.3s ease;
        `;
        notification.textContent = message;

        // Add CSS animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(style);

        document.body.appendChild(notification);

        // Auto remove after 3 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 3000);
    }
    //Binding DeliveryMethod
    //bindDeliveryModeToggle() {
    //    const deliveryOptions = document.querySelectorAll('.delivery-option');
    //    const mapMode = document.getElementById('map-mode');
    //    const storeMode = document.getElementById('store-mode');

    //    if (!deliveryOptions.length || !mapMode || !storeMode) {
    //        console.warn('⚠️ Không tìm thấy phần tử giao diện cần thiết');
    //        return;
    //    }

    //    deliveryOptions.forEach(option => {
    //        option.addEventListener('click', function () {
    //            deliveryOptions.forEach(opt => opt.classList.remove('active'));
    //            this.classList.add('active');

    //            const type = this.dataset.type;
    //            if (type === 'delivery') {
    //                mapMode.classList.add('active');
    //                storeMode.classList.remove('active');
    //            } else if (type === 'pickup') {
    //                mapMode.classList.remove('active');
    //                storeMode.classList.add('active');
    //                // Gọi danh sách cửa hàng nếu đã có vị trí
    //                mapHandler.loadSortedStores();
    //            }
    //        });
    //    });
    //}
    handleDeliveryModeChange(type) {
        const mapMode = document.getElementById('map-mode');
        const storeSidebar = document.querySelector('.store-list-sidebar');

        this.currentDeliveryMethod = type;
        storeSidebar.classList.remove('hidden');
        mapMode.classList.add('active');

        // Search block và store list
        if (type === 'delivery') {
            document.getElementById('pickupSearchBlock')?.classList.add('hidden');
            document.getElementById('deliverySearchBlock')?.classList.remove('hidden');
            document.getElementById('storeList')?.classList.add('hidden');
        } else if (type === 'pickup') {
            document.getElementById('pickupSearchBlock')?.classList.remove('hidden');
            document.getElementById('deliverySearchBlock')?.classList.add('hidden');
            document.getElementById('storeList')?.classList.remove('hidden');
            this.loadSortedStores();
        }

        // Nút trong hàng ngang
        document.getElementById('btnSelectStore')?.classList.toggle('hidden', type !== 'pickup');
        document.getElementById('btnClearRoute')?.classList.toggle('hidden', type !== 'delivery');
    }
    setDeliveryMode(type) {
        const deliveryOptions = document.querySelectorAll('.delivery-option');
        deliveryOptions.forEach(opt => {
            if (opt.dataset.type === type) {
                opt.classList.add('active');
            } else {
                opt.classList.remove('active');
            }
        });

        this.handleDeliveryModeChange(type);
    }
    bindDeliveryModeToggle() {
        const deliveryOptions = document.querySelectorAll('.delivery-option');

        deliveryOptions.forEach(option => {
            option.addEventListener('click', () => {
                deliveryOptions.forEach(opt => opt.classList.remove('active'));
                option.classList.add('active');
                const type = option.dataset.type;
                this.handleDeliveryModeChange(type);
            });
        });

        // Gọi trực tiếp không cần click giả
        this.setDeliveryMode(this.currentDeliveryMethod);
    }
}

// Initialize map when DOM is loaded
let mapHandler;
document.addEventListener('DOMContentLoaded', async () => {
    mapHandler = new MapHandler();
    await mapHandler.initialize();
    await mapHandler.loadSortedStores();
    mapHandler.bindDeliveryModeToggle();
});

// Make mapHandler globally accessible for popup buttons
window.mapHandler = mapHandler;