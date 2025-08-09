class LocationService {
    constructor() {
        this.userLocation = null;
        this.userLocationView = null;
        this.defaultView = { lat: 10.770967, lng: 106.667207, zoom: 14 };
    }

    // Khởi tạo vị trí người dùng
    async initializeUserLocation() {
        try {
            // Kiểm tra session trước
            const sessionRes = await fetch('/Location/GetSessionLocation');

            if (sessionRes.ok) {
                const userLoc = await sessionRes.json();
                this.setUserLocation(userLoc.lat, userLoc.lng);
                //this.userLocation = {
                //    lat: userLoc.latitude,
                //    lng: userLoc.longitude
                //};
                this.userLocationView = {
                    lat: userLoc.lat,
                    lng: userLoc.lng
                };
                return userLoc;
            } else {
                // Yêu cầu vị trí từ trình duyệt
                return await this.requestBrowserLocation();
            }
        } catch (error) {
            console.error("❌ Error initializing user location:", error);
            throw error;
        }
    }

    // Yêu cầu vị trí từ trình duyệt
    async requestBrowserLocation() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error("Geolocation not supported"));
                return;
            }

            navigator.geolocation.getCurrentPosition(
                async (position) => {
                    const lat = position.coords.latitude;
                    const lng = position.coords.longitude;

                    this.setUserLocation(lat, lng);

                    try {
                        const result = await this.saveUserLocationToSession(lat, lng);
                        resolve(result);
                    } catch (error) {
                        reject(error);
                    }
                },
                (error) => {
                    console.warn("⚠️ Location access denied or failed:", error.message);
                    reject(error);
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 300000
                }
            );
        });
    }

    // Lưu vị trí người dùng vào session
    async saveUserLocationToSession(lat, lng) {
        try {
            const response = await fetch('/SetSessionLocation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    latitude: lat,
                    longitude: lng,
                })
            });

            if (response.ok) {
                const result = await response.json();
                this.setUserLocation(result.lat, result.lng);
                //this.userLocation = { lat: result.lat, lng: result.lng };
                this.userLocationView = { lat: result.lat, lng: result.lng, zoom: 15 };
                return result;
            } else {
                throw new Error("Failed to save location to session");
            }
        } catch (error) {
            console.error("❌ Error saving user location to session:", error);
            throw error;
        }
    }

    // Lưu vị trí đã chọn từ map click hoặc search
    async saveSelectedLocationToSession(lat, lng, address, deliveryMethod) {
        try {
            const response = await fetch('/Location/SaveUserSessionLocation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    latitude: parseFloat(lat),
                    longitude: parseFloat(lng),
                    address: address,
                    deliveryMethod: deliveryMethod
                })
            });

            const result = await response.json();
            if (result.success) {
                this.setUserLocation(lat, lng);
                this.userLocationView = { lat: lat, lng: result.lng, zoom: 15 };
                return result;
            } else {
                throw new Error(result.message);
            }
        } catch (error) {
            console.error('❌ Session save error:', error);
            throw error;
        }
    }

    // Tìm cửa hàng gần nhất
    async findNearestStore() {
        try {
            const response = await fetch('/Location/GetNearestStore');
            if (response.ok) {
                const result = await response.json();
                return result;
            }
            throw new Error('Failed to find nearest store');
        } catch (error) {
            console.error("❌ Error finding nearest store:", error);
            throw error;
        }
    }

    // Lấy route từ backend
    async getRoute(startLat, startLng, endLat, endLng) {
        try {
            const response = await fetch('/Location/GetRoute', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    startLat: startLat,
                    startLng: startLng,
                    endLat: endLat,
                    endLng: endLng
                })
            });

            if (response.ok) {
                const routeData = await response.json();
                return routeData;
            }
            throw new Error('Backend routing failed');
        } catch (error) {
            console.error('❌ Route error:', error);
            throw error;
        }
    }

    // Lưu địa điểm mới
    async saveLocation(locationData) {
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
            return result;
        } catch (error) {
            console.error('❌ Save location error:', error);
            throw error;
        }
    }

    // Reverse geocoding
    async reverseGeocode(lat, lng) {
        try {
            const response = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`);
            const data = await response.json();
            return data.display_name || "Không tìm thấy địa chỉ";
        } catch (error) {
            console.error("❌ Reverse geocoding error:", error);
            return "Không tìm thấy địa chỉ";
        }
    }

    // Getters
    getUserLocation() {
        return this.userLocation;
    }
    setUserLocation(lat, lng) {
        this.userLocation = { lat, lng };
    }
    getUserLocationView() {
        return this.userLocationView;
    }

    getDefaultView() {
        return this.defaultView;
    }
}