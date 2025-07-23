// Scripts for admin panel

// Toggle the side navigation
document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    
    if (sidebarToggle) {
        // Toggle the side navigation
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

    // Initialize DataTables
    const datatablesSimple = document.getElementById('datatablesSimple');
    if (datatablesSimple) {
        new simpleDatatables.DataTable(datatablesSimple);
    }

    // Initialize image preview for file inputs
    const fileInputs = document.querySelectorAll('.custom-file-input');
    fileInputs.forEach(input => {
        input.addEventListener('change', function() {
            const preview = this.parentElement.nextElementSibling;
            if (preview && preview.classList.contains('image-preview-container')) {
                const file = this.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function(e) {
                        const img = preview.querySelector('img') || document.createElement('img');
                        img.src = e.target.result;
                        img.classList.add('image-preview', 'mt-2');
                        if (!preview.querySelector('img')) {
                            preview.appendChild(img);
                        }
                    }
                    reader.readAsDataURL(file);
                }
            }
        });
    });

    // Enable tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Enable popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Confirm delete
    const deleteButtons = document.querySelectorAll('.btn-delete');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Bạn có chắc chắn muốn xóa mục này?')) {
                e.preventDefault();
            }
        });
    });
}); 