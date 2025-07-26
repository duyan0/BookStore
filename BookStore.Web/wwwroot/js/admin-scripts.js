// Scripts for admin panel

// Initialize TinyMCE Rich Text Editor
function initializeTinyMCE() {
    // Wait for TinyMCE to be available
    if (typeof tinymce === 'undefined') {
        console.log('TinyMCE not loaded yet, retrying...');
        setTimeout(initializeTinyMCE, 500);
        return;
    }

    // Remove any existing TinyMCE instances
    tinymce.remove('textarea.rich-text-editor');

    tinymce.init({
        selector: 'textarea.rich-text-editor',
        height: 400,
        menubar: false,
        plugins: [
            'advlist', 'autolink', 'lists', 'link', 'charmap', 'preview',
            'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
            'insertdatetime', 'table', 'help', 'wordcount'
        ],
        toolbar: 'undo redo | blocks | ' +
            'bold italic underline forecolor backcolor | alignleft aligncenter ' +
            'alignright alignjustify | bullist numlist outdent indent | ' +
            'link table | code preview fullscreen | removeformat help',
        content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; font-size: 14px; line-height: 1.6; margin: 1rem; }',
        branding: false,
        promotion: false,
        skin: 'oxide',
        content_css: 'default',
        block_formats: 'Paragraph=p; Heading 1=h1; Heading 2=h2; Heading 3=h3; Heading 4=h4; Heading 5=h5; Heading 6=h6; Preformatted=pre',
        setup: function (editor) {
            editor.on('change', function () {
                editor.save();
            });
            editor.on('init', function () {
                console.log('TinyMCE initialized successfully for:', editor.id);
            });
        },
        init_instance_callback: function (editor) {
            console.log('TinyMCE instance ready:', editor.id);
        }
    }).then(function(editors) {
        console.log('TinyMCE editors initialized:', editors.length);
    }).catch(function(error) {
        console.error('TinyMCE initialization failed:', error);
    });
}

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

    // Initialize TinyMCE with delay to ensure DOM is ready
    setTimeout(function() {
        initializeTinyMCE();
    }, 100);

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