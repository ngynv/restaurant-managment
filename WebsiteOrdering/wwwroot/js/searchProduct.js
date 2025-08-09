let searchTimeout;
const searchInput = document.getElementById('searchInput');
const searchSuggestions = document.getElementById('searchSuggestions');

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
    // SỬA: Đường dẫn đúng cho API
    fetch(`/Products/searchProducts?term=${encodeURIComponent(term)}`)
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
        li.innerHTML = `<a class="dropdown-item" href="#" onclick="selectSuggestion('${product.name}')">${product.name}</a>`;
        searchSuggestions.appendChild(li);
    });
    
    searchSuggestions.style.display = 'block';
}

function selectSuggestion(name) {
    searchInput.value = name;
    searchSuggestions.style.display = 'none';
    searchInput.closest('form').submit();
}

function clearSearch() {
    searchInput.value = '';
    searchSuggestions.style.display = 'none';
    // SỬA: Redirect về trang đúng
    window.location.href = '/Products';
}

// Ẩn suggestions khi click ra ngoài
document.addEventListener('click', function (event) {
    if (!searchInput.contains(event.target) && !searchSuggestions.contains(event.target)) {
        searchSuggestions.style.display = 'none';
    }
});

// Xử lý phím Enter trong search input
searchInput.addEventListener('keydown', function(event) {
    if (event.key === 'Enter') {
        searchSuggestions.style.display = 'none';
    }
});