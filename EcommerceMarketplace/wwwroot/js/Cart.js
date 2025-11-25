/**
 * JavaScript para a página do carrinho de compras
 * Gerencia atualização de quantidades e cálculos
 */

/**
 * Atualiza a quantidade de um item no carrinho
 * @param {number} cartItemId - ID do item no carrinho
 * @param {number} newQuantity - Nova quantidade desejada
 */
function updateQuantity(cartItemId, newQuantity) {
    // Validação básica
    if (newQuantity < 1) {
        showNotification('Quantidade mínima é 1', 'warning');
        return;
    }
    
    // Obter o input de quantidade para verificar o máximo
    const quantityInput = document.getElementById(`quantity-${cartItemId}`);
    const maxStock = parseInt(quantityInput.getAttribute('max'));
    
    if (newQuantity > maxStock) {
        showNotification(`Quantidade máxima disponível: ${maxStock}`, 'warning');
        return;
    }
    
    // Mostrar feedback visual de loading
    const subtotalElement = document.getElementById(`subtotal-${cartItemId}`);
    const originalContent = subtotalElement.innerHTML;
    subtotalElement.innerHTML = '<span class="spinner-border spinner-border-sm text-primary"></span>';
    
    // Desabilitar botões temporariamente
    const buttons = document.querySelectorAll(`button[onclick*="${cartItemId}"]`);
    buttons.forEach(btn => btn.disabled = true);
    
    // Obter o token antiforgery
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    // Fazer requisição AJAX
    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `cartItemId=${cartItemId}&quantity=${newQuantity}&__RequestVerificationToken=${token}`
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Erro na requisição');
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            // Sucesso - recarregar a página para atualizar todos os valores
            // (Em produção, poderíamos atualizar dinamicamente sem reload)
            showNotification('Quantidade atualizada!', 'success');
            
            // Pequeno delay para o usuário ver a mensagem
            setTimeout(() => {
                location.reload();
            }, 500);
        } else {
            // Erro retornado pela API
            subtotalElement.innerHTML = originalContent;
            buttons.forEach(btn => btn.disabled = false);
            showNotification(data.message || 'Erro ao atualizar quantidade', 'danger');
        }
    })
    .catch(error => {
        console.error('Erro:', error);
        subtotalElement.innerHTML = originalContent;
        buttons.forEach(btn => btn.disabled = false);
        showNotification('Erro ao atualizar quantidade. Tente novamente.', 'danger');
    });
}

/**
 * Exibe uma notificação temporária ao usuário
 * @param {string} message - Mensagem a ser exibida
 * @param {string} type - Tipo de alerta: 'success', 'danger', 'warning', 'info'
 */
function showNotification(message, type = 'info') {
    // Criar elemento de alerta
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alert.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alert.setAttribute('role', 'alert');
    
    // Ícones para cada tipo de alerta
    const icons = {
        success: 'bi-check-circle',
        danger: 'bi-exclamation-triangle',
        warning: 'bi-exclamation-triangle',
        info: 'bi-info-circle'
    };
    
    alert.innerHTML = `
        <i class="bi ${icons[type]} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    // Adicionar ao body
    document.body.appendChild(alert);
    
    // Remover automaticamente após 3 segundos
    setTimeout(() => {
        alert.classList.remove('show');
        setTimeout(() => {
            alert.remove();
        }, 150);
    }, 3000);
}

/**
 * Confirmação antes de remover item
 */
document.addEventListener('DOMContentLoaded', function() {
    // Adicionar confirmação aos botões de remover
    const removeForms = document.querySelectorAll('form[action*="RemoveItem"]');
    
    removeForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const productName = this.closest('.card').querySelector('.card-title a').textContent.trim();
            
            if (!confirm(`Deseja realmente remover "${productName}" do carrinho?`)) {
                e.preventDefault();
                return false;
            }
        });
    });
    
    // Adicionar confirmação ao limpar carrinho
    const clearForm = document.querySelector('form[action*="Clear"]');
    
    if (clearForm) {
        clearForm.addEventListener('submit', function(e) {
            if (!confirm('Deseja realmente limpar todo o carrinho?')) {
                e.preventDefault();
                return false;
            }
        });
    }
});

/**
 * Auto-dismiss para alertas do TempData após alguns segundos
 */
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert:not(.position-fixed)');
    
    alerts.forEach(alert => {
        // Auto-dismiss após 5 segundos
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});