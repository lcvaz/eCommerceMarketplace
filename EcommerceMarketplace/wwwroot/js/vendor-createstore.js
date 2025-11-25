// ===== VENDOR CREATE STORE SCRIPT =====
// Script específico para o formulário de criação de loja

(function() {
    'use strict';

    // Inicialização quando o DOM estiver pronto
    document.addEventListener('DOMContentLoaded', function() {

        // ===== MÁSCARA PARA CEP =====
        // Formata automaticamente o CEP enquanto o usuário digita
        // Transforma "01310100" em "01310-100"

        const zipcodeInput = document.getElementById('zipcode-input');
        if (zipcodeInput) {
            zipcodeInput.addEventListener('input', function(e) {
                let value = e.target.value.replace(/\D/g, ''); // Remove tudo que não é dígito

                if (value.length > 5) {
                    // Adiciona o hífen após o 5º dígito
                    value = value.slice(0, 5) + '-' + value.slice(5, 8);
                }

                e.target.value = value;
            });
        }

        // ===== AUTO-FOCO NO PRIMEIRO CAMPO COM ERRO =====
        // Se houver erros de validação, coloca o foco no primeiro campo com erro

        const firstError = document.querySelector('.field-validation-error');
        if (firstError) {
            const input = firstError.parentElement.querySelector('input, textarea');
            if (input) {
                input.focus();
            }
        }

        // ===== CONFIRMAÇÃO ANTES DE CANCELAR SE HOUVER DADOS PREENCHIDOS =====
        // Previne perda acidental de dados

        let formChanged = false;
        const form = document.querySelector('form');
        const cancelButton = document.querySelector('a[href*="Dashboard"]');

        if (form && cancelButton) {
            const inputs = form.querySelectorAll('input, textarea');

            inputs.forEach(input => {
                input.addEventListener('change', () => {
                    formChanged = true;
                });
            });

            cancelButton.addEventListener('click', function(e) {
                if (formChanged && !confirm('Deseja realmente cancelar? Os dados preenchidos serão perdidos.')) {
                    e.preventDefault();
                }
            });
        }
    });

})();
