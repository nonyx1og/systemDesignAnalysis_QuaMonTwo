// Ô "Tìm chức năng" trên thanh trên: lọc menu bên theo tên (bỏ dấu), Enter mở mục đầu tiên, Ctrl/⌘ K để focus.
(function () {
  var o = document.getElementById('tim-menu');
  if (!o) return;
  var boDau = function (s) { return s.normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase(); };
  var nhom = Array.prototype.slice.call(document.querySelectorAll('.nav-group'));
  var rong = document.querySelector('.nav-empty');
  function loc() {
    var q = boDau(o.value.trim()), con = 0;
    nhom.forEach(function (g) {
      var hien = 0;
      g.querySelectorAll('li').forEach(function (li) {
        var ok = !q || boDau(li.textContent).indexOf(q) >= 0;
        li.hidden = !ok; if (ok) hien++;
      });
      g.hidden = hien === 0; con += hien;
    });
    rong.hidden = con > 0;
  }
  o.addEventListener('input', loc);
  o.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') { var a = document.querySelector('.nav-group:not([hidden]) li:not([hidden]) a'); if (a) location.href = a.href; }
    if (e.key === 'Escape') { o.value = ''; loc(); o.blur(); }
  });
  document.addEventListener('keydown', function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') { e.preventDefault(); o.focus(); o.select(); }
  });
})();
