// Dòng nhập động. <tbody data-rows="Input.Bom"> chứa các <tr data-row>; trường có name dạng Input.Bom[0].Ten.
(function () {
  function danhSoLai(tbody) {
    var p = tbody.dataset.rows.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    var re = new RegExp('^' + p + '\\[\\d+\\]');
    tbody.querySelectorAll('tr[data-row]').forEach(function (tr, i) {
      tr.querySelectorAll('[name]').forEach(function (el) {
        el.name = el.name.replace(re, tbody.dataset.rows + '[' + i + ']');
      });
    });
  }
  document.addEventListener('click', function (e) {
    var them = e.target.closest('[data-add-row]');
    if (them) {
      var tb = document.querySelector(them.dataset.addRow);
      var cuoi = tb.querySelector('tr[data-row]:last-child');
      var moi = cuoi.cloneNode(true);
      moi.querySelectorAll('input').forEach(function (i) { i.value = ''; });
      moi.querySelectorAll('select').forEach(function (s) { s.selectedIndex = 0; });
      tb.appendChild(moi);
      danhSoLai(tb);
    }
    var xoa = e.target.closest('[data-del-row]');
    if (xoa) {
      var tbody = xoa.closest('tbody');
      if (tbody.querySelectorAll('tr[data-row]').length > 1) { xoa.closest('tr').remove(); danhSoLai(tbody); }
    }
  });
})();
