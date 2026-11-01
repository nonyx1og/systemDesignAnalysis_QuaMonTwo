// Tra khách khi rời ô SĐT: <input data-sdt> và <span data-khach-info>.
(function () {
  var o = document.querySelector('[data-sdt]');
  var info = document.querySelector('[data-khach-info]');
  if (!o || !info) return;
  o.addEventListener('blur', function () {
    var v = o.value.trim();
    if (!v) { info.textContent = ''; return; }
    fetch('/BanHang/Khach?sdt=' + encodeURIComponent(v))
      .then(function (r) { return r.json(); })
      .then(function (d) {
        info.textContent = d.loi ? d.loi
          : d.ten ? 'Khách: ' + d.ten + ' · ' + d.diem + ' điểm'
          : 'Khách mới: nhập tên khách bên dưới.';
      })
      .catch(function () { info.textContent = ''; });
  });
})();
