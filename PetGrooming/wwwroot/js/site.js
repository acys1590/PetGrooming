$(document).ready(function () {
    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    //// Confirm delete actions
    //$('a[href*="Delete"]').click(function (e) {
    //    if (!confirm('Are you sure you want to delete this item?')) {
    //        e.preventDefault();
    //    }
    //});

    // Form validation enhancements
    $('form').submit(function () {
        $(this).find('button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    });

    // Phone number formatting
    $('input[type="tel"], input[name*="Phone"]').on('input', function () {
        var value = this.value.replace(/\D/g, '');
        var formattedValue = value.replace(/(\d{3})(\d{3})(\d{4})/, '$1-$2-$3');
        if (formattedValue.length <= 12) {
            this.value = formattedValue;
        }
    });

    // Species-specific breed suggestions
    $('#Pet_Species').change(function () {
        var species = $(this).val();
        var breedInput = $('#Pet_Breed');

        breedInput.val('');

        switch (species) {
            case 'Dog':
                breedInput.attr('placeholder', 'e.g., Golden Retriever, Labrador, German Shepherd');
                break;
            case 'Cat':
                breedInput.attr('placeholder', 'e.g., Persian, Siamese, Maine Coon');
                break;
            case 'Bird':
                breedInput.attr('placeholder', 'e.g., Parrot, Canary, Cockatiel');
                break;
            case 'Rabbit':
                breedInput.attr('placeholder', 'e.g., Holland Lop, Netherland Dwarf');
                break;
            default:
                breedInput.attr('placeholder', 'Enter breed');
        }
    });

    // ✅ Approval checkbox toggle with confirm on unapprove
    $('.approval-checkbox').change(function () {
        var checkbox = $(this);
        var petId = checkbox.data('id');
        var approved = checkbox.is(':checked');

        // If user is UNCHECKING (disapproving), confirm first
        if (!approved) {
            if (!confirm("⚠️ Are you sure you want to unapprove this appointment?")) {
                // ❌ Cancel -> revert checkbox back to checked
                checkbox.prop('checked', true);
                return;
            }
        }

        // ⏳ disable checkbox while waiting
        checkbox.prop('disabled', true);

        $.ajax({
            url: '/Pets/ToggleApproval',
            type: 'POST',
            data: { id: petId, approved: approved },
            success: function (response) {
                showNotification(response.message, "success");
            },
            error: function () {
                showNotification("❌ Something went wrong. Try again.", "danger");
                checkbox.prop('checked', !approved); // revert if failed
            },
            complete: function () {
                // ✅ re-enable after request finishes
                checkbox.prop('disabled', false);
            }
        });
    });


});

// Utility functions
function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function showNotification(message, type = 'info') {
    var alertClass = 'alert-' + type;
    var iconClass = type === 'success' ? 'fa-check-circle' :
        type === 'danger' ? 'fa-exclamation-circle' :
            'fa-info-circle';

    var alert = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="fas ${iconClass}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('.container > main').prepend(alert);

    setTimeout(function () {
        $('.alert').first().fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}
