/* ===== VERSATTO (C#) - inicialização Materialize ===== */
document.addEventListener('DOMContentLoaded', function () {
    M.AutoInit();
    M.FormSelect.init(document.querySelectorAll('select'));
    M.Sidenav.init(document.querySelectorAll('.sidenav'));
    M.Tooltip.init(document.querySelectorAll('.tooltipped'));
    M.Dropdown.init(document.querySelectorAll('.dropdown-trigger'), { coverTrigger: false, constrainWidth: false });
    M.updateTextFields(); // campos preenchidos pelo servidor (filtros, edição) com o rótulo no lugar certo
});
