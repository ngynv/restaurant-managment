class SearchService {
    constructor() {
        this.debounceTimer = null;
        this.suggestions = [];
        this.selectedIndex = -1;
        this.onPlaceSelected = null; // Callback khi chọn địa điểm
    }

    // Khởi tạo search functionality
    initialize(inputId, suggestionsId, loadingSpinnerId, onPlaceSelectedCallback) {
        this.input = document.getElementById(inputId);
        this.suggestionList = document.getElementById(suggestionsId);
        this.loadingSpinner = document.getElementById(loadingSpinnerId);
        this.onPlaceSelected = onPlaceSelectedCallback;

        this.bindEvents();
    }

    // Bind events
    bindEvents() {
        if (!this.input) return;

        // Input event with debounce
        this.input.addEventListener('input', (e) => {
            clearTimeout(this.debounceTimer);
            const query = e.target.value.trim();

            if (query.length < 2) {
                this.hideSuggestions();
                return;
            }

            this.debounceTimer = setTimeout(async () => {
                const results = await this.searchAddress(query);
                this.displaySuggestions(results);
            }, 300);
        });

        // Keyboard navigation
        this.input.addEventListener('keydown', (e) => {
            if (!this.suggestionList.classList.contains('show')) return;

            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    this.selectedIndex = Math.min(this.selectedIndex + 1, this.suggestions.length - 1);
                    this.highlightSuggestion(this.selectedIndex);
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    this.selectedIndex = Math.max(this.selectedIndex - 1, -1);
                    this.highlightSuggestion(this.selectedIndex);
                    break;
                case 'Enter':
                    e.preventDefault();
                    if (this.selectedIndex >= 0 && this.selectedIndex < this.suggestions.length) {
                        this.selectPlace(this.selectedIndex);
                    }
                    break;
                case 'Escape':
                    this.hideSuggestions();
                    break;
            }
        });

        // Click outside to hide suggestions
        document.addEventListener('click', (e) => {
            if (!this.input.contains(e.target) && !this.suggestionList.contains(e.target)) {
                this.hideSuggestions();
            }
        });

        // Focus to input when page loads
        window.addEventListener('load', () => {
            this.input.focus();
        });
    }

    // Tìm kiếm địa chỉ qua Nominatim API
    async searchAddress(query) {
        try {
            if (this.loadingSpinner) {
                this.loadingSpinner.style.display = 'block';
            }

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
            if (this.loadingSpinner) {
                this.loadingSpinner.style.display = 'none';
            }
        }
    }

    // Hiển thị suggestions
    displaySuggestions(results) {
        if (!this.suggestionList) return;

        this.suggestions = results;
        this.selectedIndex = -1;

        if (results.length === 0) {
            this.suggestionList.innerHTML = '<div class="no-results">Không tìm thấy kết quả phù hợp</div>';
            this.suggestionList.classList.add('show');
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

        this.suggestionList.innerHTML = html;
        this.suggestionList.classList.add('show');

        // Bind click events cho suggestions
        this.suggestionList.querySelectorAll('.suggestion-item').forEach((item, index) => {
            item.addEventListener('click', () => this.selectPlace(index));
            item.addEventListener('mouseenter', () => this.highlightSuggestion(index));
        });
    }

    // Highlight suggestion
    highlightSuggestion(index) {
        const items = this.suggestionList.querySelectorAll('.suggestion-item');
        items.forEach((item, i) => {
            item.classList.toggle('highlighted', i === index);
        });
        this.selectedIndex = index;
    }

    // Chọn địa điểm
    async selectPlace(index) {
        if (index < 0 || index >= this.suggestions.length) return;

        const place = this.suggestions[index];
        const lat = parseFloat(place.lat);
        const lng = parseFloat(place.lon);
        const address = place.display_name;

        // Cập nhật input
        this.input.value = address;
        this.hideSuggestions();

        // Callback để xử lý place được chọn
        if (this.onPlaceSelected) {
            try {
                await this.onPlaceSelected(lat, lng, address);
            } catch (error) {
                console.error('Error handling place selection:', error);
                alert('❌ Có lỗi xảy ra khi lưu vị trí!');
            }
        }
    }

    // Ẩn suggestions
    hideSuggestions() {
        if (this.suggestionList) {
            console.log('Ẩn suggestions');
            this.suggestionList.classList.remove('show');
        } else {
            console.warn('suggestionList không tồn tại!');
        }
        this.selectedIndex = -1;
    }

    // Clear search
    clearSearch() {
        if (this.input) {
            this.input.value = '';
        }
        this.hideSuggestions();
    }
}