/**
 * JavaScript para a página de detalhes do produto
 * Controla o seletor de quantidade
 */

/**
 * Incrementa a quantidade do produto
 * @param {number} maxStock - Estoque máximo disponível
 */
function incrementQuantity(maxStock) {
    const input = document.getElementById('quantity');
    if (!input) return;
    
    const currentValue = parseInt(input.value);
    
    if (isNaN(currentValue)) {
        input.value = 1;
        return;
    }
    
    if (currentValue < maxStock) {
        input.value = currentValue + 1;
    } else {
        // Feedback visual quando atingir o máximo
        input.classList.add('border-warning');
        setTimeout(() => {
            input.classList.remove('border-warning');
        }, 500);
    }
}

/**
 * Decrementa a quantidade do produto
 */
function decrementQuantity() {
    const input = document.getElementById('quantity');
    if (!input) return;
    
    const currentValue = parseInt(input.value);
    
    if (isNaN(currentValue)) {
        input.value = 1;
        return;
    }
    
    if (currentValue > 1) {
        input.value = currentValue - 1;
    } else {
        // Feedback visual quando atingir o mínimo
        input.classList.add('border-warning');
        setTimeout(() => {
            input.classList.remove('border-warning');
        }, 500);
    }
}

/**
 * Validação ao enviar o formulário
 */
document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form[action*="AddToCart"]');
    
    if (form) {
        form.addEventListener('submit', function(e) {
            const quantityInput = document.getElementById('quantity');
            const quantity = parseInt(quantityInput.value);
            const maxStock = parseInt(quantityInput.getAttribute('max'));
            
            if (isNaN(quantity) || quantity < 1) {
                e.preventDefault();
                alert('Por favor, insira uma quantidade válida');
                quantityInput.value = 1;
                return false;
            }
            
            if (quantity > maxStock) {
                e.preventDefault();
                alert(`Quantidade máxima disponível: ${maxStock}`);
                quantityInput.value = maxStock;
                return false;
            }
        });
    }
});