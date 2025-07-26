// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ===== SHOPPING CART FUNCTIONALITY =====

// Add to cart function
function addToCart(bookId, quantity = 1) {
    // Check if user is logged in
    const isLoggedIn = document.querySelector('meta[name="user-logged-in"]')?.getAttribute('content') === 'true';

    if (!isLoggedIn) {
        // Show login prompt
        if (confirm('Bạn cần đăng nhập để thêm sách vào giỏ hàng. Chuyển đến trang đăng nhập?')) {
            window.location.href = '/Account/Login';
        }
        return;
    }

    // Show loading state
    const button = event?.target;
    let originalContent = '';

    if (button) {
        originalContent = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang thêm...';
    }

    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // Prepare form data
    const formData = new FormData();
    formData.append('bookId', bookId);
    formData.append('quantity', quantity);
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    // Make AJAX request to add to cart
    fetch('/Shop/AddToCart', {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        }
        throw new Error('Network response was not ok');
    })
    .then(data => {
        if (data.success) {
            // Show success message
            showNotification(data.message || 'Đã thêm sách vào giỏ hàng!', 'success');

            // Update cart counter
            updateCartCounter();
        } else {
            showNotification(data.message || 'Không thể thêm sách vào giỏ hàng.', 'error');
        }

        // Reset button state
        if (button) {
            button.disabled = false;
            button.innerHTML = originalContent;
        }
    })
    .catch(error => {
        console.error('Error adding to cart:', error);
        showNotification('Không thể thêm sách vào giỏ hàng. Vui lòng thử lại.', 'error');

        // Reset button state
        if (button) {
            button.disabled = false;
            button.innerHTML = originalContent;
        }
    });
}

// Update cart counter
function updateCartCounter() {
    fetch('/Shop/GetCartCount', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        const cartCounters = document.querySelectorAll('.cart-counter');
        cartCounters.forEach(counter => {
            if (data.count > 0) {
                counter.textContent = data.count;
                counter.style.display = 'inline-block';
            } else {
                counter.style.display = 'none';
            }
        });
    })
    .catch(error => {
        console.error('Error updating cart counter:', error);
    });
}

// Show notification
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    // Add to page
    document.body.appendChild(notification);

    // Auto remove after 3 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
}

// Add to cart with custom quantity (for detail page)
function addToCartWithQuantity(bookId) {
    const quantityInput = document.getElementById('quantityInput');
    const quantity = quantityInput ? parseInt(quantityInput.value) : 1;

    // Validate quantity
    if (quantity < 1) {
        showNotification('Số lượng phải lớn hơn 0', 'error');
        return;
    }

    const maxQuantity = quantityInput ? parseInt(quantityInput.getAttribute('max')) : 999;
    if (quantity > maxQuantity) {
        showNotification(`Số lượng không được vượt quá ${maxQuantity}`, 'error');
        return;
    }

    // Call the main addToCart function with custom quantity
    addToCart(bookId, quantity);
}

// Initialize cart counter on page load
document.addEventListener('DOMContentLoaded', function() {
    updateCartCounter();
});
