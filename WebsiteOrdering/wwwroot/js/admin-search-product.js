// admin-search.js
let searchTimeout;
const searchInput = document.getElementById('searchInput');
const searchSuggestions = document.getElementById('searchSuggestions');

if (searchInput && searchSuggestions) {
    searchInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        const searchTerm = this.value;

        if (searchTerm.length < 2) {
            searchSuggestions.style.display = 'none';
            return;
        }

        searchTimeout = setTimeout(() => {
            fetchSuggestions(searchTerm);
        }, 300);
    });

    function fetchSuggestions(term) {
        // API endpoint cho Admin area
        fetch(`/Admin/ProductsManagement/searchProducts?term=${encodeURIComponent(term)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                showSuggestions(data);
            })
            .catch(error => {
                console.error('Error:', error);
                searchSuggestions.style.display = 'none';
            });
    }

    function showSuggestions(products) {
        searchSuggestions.innerHTML = '';

        if (products.length === 0) {
            searchSuggestions.style.display = 'none';
            return;
        }

        products.forEach(product => {
            const li = document.createElement('li');
            li.innerHTML = `<a class="dropdown-item" href="#" onclick="selectSuggestion('${product.name.replace(/'/g, "\\'")}')">${product.name}</a>`;
            searchSuggestions.appendChild(li);
        });

        searchSuggestions.style.display = 'block';
    }

    // Hàm chọn suggestion
    window.selectSuggestion = function (name) {
        searchInput.value = name;
        searchSuggestions.style.display = 'none';
        // Tự động submit form
        searchInput.closest('form').submit();
    }

    // Hàm xóa tìm kiếm
    window.clearSearch = function () {
        searchInput.value = '';
        searchSuggestions.style.display = 'none';
        // Redirect về trang admin
        window.location.href = '/Admin/ProductsManagement';
    }

    // Ẩn suggestions khi click ra ngoài
    document.addEventListener('click', function (event) {
        if (!searchInput.contains(event.target) && !searchSuggestions.contains(event.target)) {
            searchSuggestions.style.display = 'none';
        }
    });

    // Xử lý phím Enter và Arrow keys
    searchInput.addEventListener('keydown', function (event) {
        const suggestions = searchSuggestions.querySelectorAll('.dropdown-item');
        let activeIndex = -1;

        // Tìm item đang active
        suggestions.forEach((item, index) => {
            if (item.classList.contains('active')) {
                activeIndex = index;
            }
        });

        switch (event.key) {
            case 'Enter':
                event.preventDefault();
                if (activeIndex >= 0) {
                    suggestions[activeIndex].click();
                } else {
                    searchSuggestions.style.display = 'none';
                    this.closest('form').submit();
                }
                break;

            case 'ArrowDown':
                event.preventDefault();
                if (suggestions.length > 0) {
                    // Xóa active hiện tại
                    suggestions.forEach(item => item.classList.remove('active'));
                    // Thêm active cho item tiếp theo
                    activeIndex = (activeIndex + 1) % suggestions.length;
                    suggestions[activeIndex].classList.add('active');
                }
                break;

            case 'ArrowUp':
                event.preventDefault();
                if (suggestions.length > 0) {
                    // Xóa active hiện tại
                    suggestions.forEach(item => item.classList.remove('active'));
                    // Thêm active cho item trước đó
                    activeIndex = activeIndex <= 0 ? suggestions.length - 1 : activeIndex - 1;
                    suggestions[activeIndex].classList.add('active');
                }
                break;

            case 'Escape':
                searchSuggestions.style.display = 'none';
                break;
        }
    });
}