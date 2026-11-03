// thiet-ke.js - Trình thiết kế bánh kem 2D với Fabric.js (FrendshipBakery)
(function () {
    'use strict';
    // Tên thành phần do quản lý nhập: escape trước khi đưa vào innerHTML.
    const esc = s => String(s ?? '').replace(/[&<>"']/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[ch]));

    // 8 màu kem phủ cố định
    const MAU_KEM = [
        { ten: 'Trắng sữa', hex: '#FFFDF5' },
        { ten: 'Kem vani', hex: '#FFF8DC' },
        { ten: 'Hồng phấn', hex: '#FFB6C1' },
        { ten: 'Xanh pastel', hex: '#B0E0E6' },
        { ten: 'Vàng nhạt', hex: '#FFFACD' },
        { ten: 'Socola', hex: '#5D4037' },
        { ten: 'Matcha', hex: '#8FBC8F' },
        { ten: 'Tím lavender', hex: '#E6E6FA' }
    ];

    // 8 màu chữ
    const MAU_CHU = [
        '#8B0000', '#C2185B', '#E91E63', '#1565C0', '#2E7D32', '#5D4037', '#FF6F00', '#212121'
    ];

    // 3 mẫu thiết kế gợi ý cầu kỳ, sang trọng, sẵn sàng đặt bánh ngay
    const MAU_THIET_KE = {
        1: {
            ten: 'Dâu tây & Macaron Hoàng Gia',
            tenCot: 'Cốt vani',
            tenVien: 'Viền kem xoắn',
            mauKem: '#FFFDF5',
            toppings: [
                { ten: 'Dâu tây', x: 200, y: 112 },
                { ten: 'Macaron', x: 160, y: 125 },
                { ten: 'Macaron', x: 240, y: 125 },
                { ten: 'Dâu tây', x: 125, y: 155 },
                { ten: 'Dâu tây', x: 275, y: 155 },
                { ten: 'Macaron', x: 105, y: 195 },
                { ten: 'Dâu tây', x: 295, y: 195 }
            ],
            chu: 'Happy Birthday',
            fontChu: "'Dancing Script', cursive",
            mauChu: '#8B0000'
        },
        2: {
            ten: 'Socola & Cherry Dạ Tiệc',
            tenCot: 'Cốt socola',
            tenVien: 'Viền kem xoắn',
            mauKem: '#5D4037',
            toppings: [
                { ten: 'Socola miếng', x: 200, y: 110 },
                { ten: 'Cherry', x: 168, y: 125 },
                { ten: 'Cherry', x: 232, y: 125 },
                { ten: 'Socola miếng', x: 135, y: 155 },
                { ten: 'Socola miếng', x: 265, y: 155 },
                { ten: 'Dâu tây', x: 110, y: 195 },
                { ten: 'Dâu tây', x: 290, y: 195 },
                { ten: 'Cherry', x: 200, y: 165 }
            ],
            chu: 'Happy Birthday!',
            fontChu: "'Pacifico', cursive",
            mauChu: '#FF6F00'
        },
        3: {
            ten: 'Khung Kỷ Niệm Bách Hoa',
            tenCot: 'Cốt vani',
            tenVien: 'Viền kem xoắn',
            inAn: true,
            mauKem: '#FFFDF5',
            toppings: [
                { ten: 'Macaron', x: 105, y: 115 },
                { ten: 'Cherry', x: 120, y: 88 },
                { ten: 'Macaron', x: 295, y: 115 },
                { ten: 'Cherry', x: 280, y: 88 }
            ],
            chu: 'Family Love',
            fontChu: "'Dancing Script', cursive",
            mauChu: '#1565C0'
        }
    };

    let dataCatalogs = { sanPhams: [], thanhPhans: [], mau: null };
    const rawEl = document.getElementById('du-lieu-danh-muc');
    if (rawEl && rawEl.textContent) {
        try { dataCatalogs = JSON.parse(rawEl.textContent); } catch (e) { console.error('Lỗi nạp danh mục:', e); }
    }

    const { sanPhams = [], thanhPhans = [], mau = null } = dataCatalogs;

    // Trạng thái ứng dụng
    let canvas;
    let selectedSanPhamNenId = null;
    let selectedCotId = null;
    let selectedVienId = null;
    let selectedInAnId = null;
    let currentKemColor = '#FFFDF5';
    let currentFontChu = "'Dancing Script', cursive";
    let currentMauChu = '#8B0000';
    let isDrawingMode = false;
    let uploadedImageFile = null;
    let soLuongNen = 0;
    let soTangState = 1;
    let selectedTangId = null;

    function datSoNen(val, capNhatInput = true) {
        let n = parseInt(val, 10);
        if (isNaN(n) || n < 0) n = 0;
        if (n > 10) n = 10;
        soLuongNen = n;
        const inputNen = document.getElementById('input-so-nen');
        const nhanNen = document.getElementById('nhan-so-nen');
        if (capNhatInput && inputNen) inputNen.value = n;
        if (nhanNen) nhanNen.textContent = n + ' cây';
        if (window.Cake3D && typeof window.Cake3D.setCandles === 'function') {
            window.Cake3D.setCandles(n);
        }
        luuNhapDebounced();
        if (typeof capNhatBangTomTat === 'function' && currentStep === 5) {
            capNhatBangTomTat();
        }
    }

    function layBanKinhTang2() {
        const cm = layDuongKinhBanhNen();
        if (cm && cm >= 24) return 115;
        return 105;
    }

    function layKichThuocTang2() {
        const cm = layDuongKinhBanhNen();
        if (cm && cm >= 24) return 18;
        if (cm && cm >= 20) return 16;
        return null;
    }

    function xacDinhTangChoTopping(left, top) {
        if (soTangState !== 2) return 1;
        const posX = typeof left === 'number' ? left : 200;
        const posY = typeof top === 'number' ? top : 200;
        const d = Math.hypot(posX - 200, posY - 200);
        return d <= layBanKinhTang2() ? 2 : 1;
    }

    function capNhatTangChoToppingObject(obj) {
        if (!obj) return;
        if (obj.customType === 'topping') {
            obj.tang = xacDinhTangChoTopping(obj.left, obj.top);
        } else if (obj._objects && Array.isArray(obj._objects)) {
            obj._objects.forEach(capNhatTangChoToppingObject);
        }
    }

    function onSoTangChange(n) {
        const cm = layDuongKinhBanhNen();
        const tangTp = thanhPhans.find(t => t.nhom === 'TANG' && t.sanPhamNenId === selectedSanPhamNenId);

        if (n === 2 && (!cm || cm < 20 || !tangTp)) {
            n = 1;
        }

        soTangState = n;
        selectedTangId = (soTangState === 2 && tangTp) ? tangTp.id : null;

        const r1 = document.getElementById('radio-tang-1');
        const r2 = document.getElementById('radio-tang-2');
        if (r1) r1.checked = (soTangState === 1);
        if (r2) r2.checked = (soTangState === 2);

        capNhatHienThiSoTangUI();
        drawCakeBase();
        updateCakeVien();

        if (canvas) {
            canvas.getObjects().forEach(o => {
                if (o.customType === 'topping') {
                    o.tang = xacDinhTangChoTopping(o.left, o.top);
                }
            });
            canvas.renderAll();
        }

        if (window.Cake3D && typeof window.Cake3D.setTiers === 'function') {
            window.Cake3D.setTiers(soTangState, layKichThuocTang2());
            window.Cake3D.syncFrom2D();
        }

        capNhatGiaDebounced();
        if (currentStep === 5) capNhatBangTomTat();
        luuNhapDebounced();
        LichSuCanvas.ghiLichSu();
    }

    function capNhatHienThiSoTangUI() {
        const badge = document.getElementById('badge-tang-status');
        const desc = document.getElementById('desc-so-tang');
        const cm = layDuongKinhBanhNen();
        const tangTp = thanhPhans.find(t => t.nhom === 'TANG' && t.sanPhamNenId === selectedSanPhamNenId);

        if (soTangState === 2 && tangTp) {
            if (badge) {
                badge.textContent = `2 tầng (+${(tangTp.gia / 1000).toLocaleString('vi-VN')}k)`;
            }
            if (desc) {
                const k2 = layKichThuocTang2();
                desc.textContent = `Tầng dưới ${cm}cm, tầng trên ${k2}cm (+${tangTp.gia.toLocaleString('vi-VN')} ₫).`;
            }
        } else {
            if (badge) {
                badge.textContent = '1 tầng';
            }
            if (desc) {
                if (cm && cm >= 20 && tangTp) {
                    const k2 = layKichThuocTang2();
                    desc.textContent = `Bánh 1 tầng ${cm}cm. Có thể chọn 2 tầng (+${tangTp.gia.toLocaleString('vi-VN')} ₫).`;
                } else {
                    desc.textContent = 'Bánh 2 tầng chỉ làm được với cỡ 20cm và 24cm.';
                }
            }
        }

        const sumTang = document.getElementById('summary-so-tang');
        if (sumTang) {
            if (soTangState === 2 && tangTp) {
                sumTang.textContent = `2 tầng (${tangTp.ten})`;
            } else {
                sumTang.textContent = '1 tầng';
            }
        }
    }

    // Stepper State
    let currentStep = 1;
    const TOTAL_STEPS = 5;

    const CHIEU_RONG_CHUAN = 400;
    const KICH_THUOC_TOPPING = 32;
    const MAX_TOPPING_CAKE = 12;
    const toppingImageCache = new Map(); // filename -> HTMLImageElement
    let pendingPositions = []; // Các vị trí {x, y} đã được đặt chỗ để tránh chồng lấn

    // Icon nút "Tự xoay": play khi dừng, pause khi đang xoay
    const SVG_ICON_ATTRS = 'class="dat-icon-inline" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"';
    const ICON_PLAY = `<svg ${SVG_ICON_ATTRS}><polygon points="5 3 19 12 5 21 5 3"/></svg>`;
    const ICON_PAUSE = `<svg ${SVG_ICON_ATTRS}><line x1="8" y1="5" x2="8" y2="19"/><line x1="16" y1="5" x2="16" y2="19"/></svg>`;

    function layDuongDanTopping(file) {
        if (!file) return '';
        if (file.includes('?')) return file;
        return `/img/thanh-phan/${file}`;
    }

    const toppingsMap = new Map(); // thanhPhanId -> count
    const pendingToppings = new Map(); // thanhPhanId -> pending load count

    // Toast notification thay cho alert()
    function hienToast(msg, type = 'warning') {
        const container = document.getElementById('dat-toast-container');
        if (!container) {
            alert(msg);
            return;
        }
        const t = document.createElement('div');
        t.className = `dat-toast ${type}`;
        const label = type === 'success' ? 'Thành công' : type === 'danger' ? 'Lỗi' : 'Lưu ý';
        t.innerHTML = `<strong>${label}:</strong><div>${esc(msg)}</div>`;
        container.appendChild(t);
        setTimeout(() => {
            t.style.opacity = '0';
            t.style.transition = 'opacity 0.3s ease';
            setTimeout(() => t.remove(), 300);
        }, 3500);
    }

    function demToppingTongCong(thanhPhanId) {
        const onCanvas = canvas ? canvas.getObjects().filter(o => o.customType === 'topping' && o.thanhPhanId === thanhPhanId).length : 0;
        const pending = pendingToppings.get(thanhPhanId) || 0;
        return onCanvas + pending;
    }

    function demTongToppingTatCa() {
        let count = 0;
        if (canvas) {
            count = canvas.getObjects().filter(o => o.customType === 'topping').length;
        }
        for (const p of pendingToppings.values()) {
            count += p;
        }
        return count;
    }

    function timViTriToppingMoi() {
        // 24 vị trí ứng viên phân bố đều theo 3 vòng tròn đồng tâm quanh tâm bánh (200, 200).
        // Bánh nền có bán kính 155, topping có bán kính 16.
        // Bán kính lớn nhất của vị trí ứng viên là 112px, đảm bảo topping luôn nằm gọn bên trong mặt bánh (cách mép >= 27px).
        const candidateSlots = [];

        // Vòng 1 (r = 48, 6 vị trí, góc cách nhau 60 độ)
        for (let i = 0; i < 6; i++) {
            const rad = (-Math.PI / 2) + (i * Math.PI / 3);
            candidateSlots.push({
                x: Math.round(200 + 48 * Math.cos(rad)),
                y: Math.round(200 + 48 * Math.sin(rad))
            });
        }

        // Vòng 2 (r = 82, 8 vị trí, góc lệch 22.5 độ)
        for (let i = 0; i < 8; i++) {
            const rad = (-Math.PI / 2) + Math.PI / 8 + (i * Math.PI / 4);
            candidateSlots.push({
                x: Math.round(200 + 82 * Math.cos(rad)),
                y: Math.round(200 + 82 * Math.sin(rad))
            });
        }

        // Vòng 3 (r = 112, 10 vị trí, góc lệch 18 độ)
        for (let i = 0; i < 10; i++) {
            const rad = (-Math.PI / 2) + Math.PI / 10 + (i * Math.PI / 5);
            candidateSlots.push({
                x: Math.round(200 + 112 * Math.cos(rad)),
                y: Math.round(200 + 112 * Math.sin(rad))
            });
        }

        const occupied = [];
        if (canvas) {
            canvas.getObjects().forEach(o => {
                if (o.customType === 'topping') {
                    occupied.push({ x: o.left, y: o.top });
                }
            });
        }
        pendingPositions.forEach(p => occupied.push(p));

        if (occupied.length === 0) {
            return candidateSlots[0];
        }

        // Chọn vị trí có khoảng cách tới vị trí gần nhất là lớn nhất (phân bổ đều nhất và không chồng lên nhau)
        let bestSlot = null;
        let maxMinDist = -1;

        for (const slot of candidateSlots) {
            let minDist = Infinity;
            for (const occ of occupied) {
                const dist = Math.hypot(slot.x - occ.x, slot.y - occ.y);
                if (dist < minDist) minDist = dist;
            }
            if (minDist > maxMinDist) {
                maxMinDist = minDist;
                bestSlot = slot;
            }
        }

        // Nếu khoảng cách tối thiểu < 30px (do người dùng kéo tay di chuyển topping), tìm điểm ngẫu nhiên an toàn r <= 110
        if (maxMinDist < 30) {
            for (let tries = 0; tries < 50; tries++) {
                const angle = Math.random() * Math.PI * 2;
                const dist = 30 + Math.random() * 80;
                const randPos = {
                    x: Math.round(200 + dist * Math.cos(angle)),
                    y: Math.round(200 + dist * Math.sin(angle))
                };
                let minDist = Infinity;
                for (const occ of occupied) {
                    const d = Math.hypot(randPos.x - occ.x, randPos.y - occ.y);
                    if (d < minDist) minDist = d;
                }
                if (minDist >= 36) {
                    return randPos;
                }
            }
        }

        return bestSlot || candidateSlots[0];
    }

    function taoDoiTuongTopping(tp, posX, posY, imgEl, style = null) {
        let toppingObj;
        const currentStyle = style || currentStrawberryStyle;
        const isStrawberry = tp.nhom === 'TOPPING' && tp.ten && tp.ten.toLowerCase().includes('dâu');

        if (imgEl && imgEl.complete && (imgEl.naturalWidth > 0 || imgEl.width > 0)) {
            const nw = imgEl.naturalWidth || imgEl.width || 100;
            const nh = imgEl.naturalHeight || imgEl.height || 100;
            const targetSize = isStrawberry ? 42 : KICH_THUOC_TOPPING;
            const randAngle = Math.floor(Math.random() * 26) - 13;

            toppingObj = new fabric.Image(imgEl, {
                left: posX,
                top: posY,
                originX: 'center',
                originY: 'center',
                scaleX: targetSize / nw,
                scaleY: targetSize / nh,
                angle: randAngle,
                cornerSize: 10,
                cornerColor: '#FFFFFF',
                cornerStrokeColor: '#3A1B26',
                cornerStyle: 'circle',
                transparentCorners: false,
                borderColor: '#3A1B26',
                borderDashArray: [4, 4],
                padding: 6,
                hasRotatingPoint: true,
                rotatingPointOffset: 16,
                lockUniScaling: false,
                lockScalingFlip: true,
                customType: 'topping',
                thanhPhanId: tp.id,
                strawberryStyle: currentStyle,
                shadow: new fabric.Shadow({
                    color: 'rgba(45, 15, 20, 0.32)',
                    blur: 8,
                    offsetX: 1.5,
                    offsetY: 4.5
                })
            });
        } else {
            toppingObj = new fabric.Circle({
                left: posX,
                top: posY,
                radius: KICH_THUOC_TOPPING / 2,
                fill: '#E53935',
                stroke: '#FFFFFF',
                strokeWidth: 2,
                originX: 'center',
                originY: 'center',
                cornerSize: 10,
                cornerColor: '#FFFFFF',
                cornerStrokeColor: '#3A1B26',
                cornerStyle: 'circle',
                borderColor: '#3A1B26',
                borderDashArray: [4, 4],
                padding: 6,
                hasRotatingPoint: true,
                shadow: new fabric.Shadow({
                    color: 'rgba(45, 15, 20, 0.32)',
                    blur: 8,
                    offsetX: 1.5,
                    offsetY: 4.5
                }),
                customType: 'topping',
                thanhPhanId: tp.id
            });
        }
        toppingObj.tang = xacDinhTangChoTopping(posX, posY);
        toppingObj.setCoords();
        return toppingObj;
    }

    function capNhatToppingsTuCanvas() {
        toppingsMap.clear();
        if (canvas) {
            canvas.getObjects().forEach(obj => {
                if (obj.customType === 'topping' && obj.thanhPhanId) {
                    const c = toppingsMap.get(obj.thanhPhanId) || 0;
                    toppingsMap.set(obj.thanhPhanId, c + 1);
                }
            });
        }
        for (const [id, count] of pendingToppings.entries()) {
            if (count > 0) {
                const c = toppingsMap.get(id) || 0;
                toppingsMap.set(id, c + count);
            }
        }
        updateToppingBadges();
        capNhatGiaDebounced();
        if (currentStep === 5) capNhatBangTomTat();
    }

    let cakeBaseObj = null;
    let cakeBoardObj = null;
    let cakeBoardRimObj = null;
    let cakeRimObj = null;
    let cakeSwirlsObj = null;
    let cakeVienObj = null;
    let cakeTier2Obj = null;
    let cakeTier2RimObj = null;
    let cakeTier2SwirlsObj = null;
    let cakeTier2VienObj = null;
    let printAreaBorder = null;
    let printAreaClip = null;
    let printImageObj = null;
    let textObj = null;

    // =========================================================================
    // QUẢN LÝ LỊCH SỬ HOÀN TÁC (CTRL+Z) / LÀM LẠI (CTRL+Y) TRÊN CANVAS (< 80 DÒNG)
    // Tham khảo mô hình fabric-history (MIT License, Alim Özdemir)
    // =========================================================================
    const LichSuCanvas = (() => {
        const TOI_DA = 30;
        const undoStack = [];
        const redoStack = [];
        let dangKhoiPhuc = false;
        let timerGhi = null;
        let trangThaiTruoc = null;
        const THUOC_TINH = ['customType', 'thanhPhanId', 'strawberryStyle', 'selectable', 'evented', 'lockScalingX', 'lockScalingY', 'tang'];

        function layTrangThai() {
            return canvas ? JSON.stringify(canvas.toJSON(THUOC_TINH)) : null;
        }

        function capNhatNut() {
            const btnUndo = document.getElementById('btn-tool-undo');
            const btnRedo = document.getElementById('btn-tool-redo');
            if (btnUndo) btnUndo.disabled = undoStack.length === 0;
            if (btnRedo) btnRedo.disabled = redoStack.length === 0;
        }

        function thucHienGhi() {
            timerGhi = null;
            if (dangKhoiPhuc || !canvas) return;
            const hienTai = layTrangThai();
            if (!hienTai) return;
            if (trangThaiTruoc === null) {
                trangThaiTruoc = hienTai;
                return;
            }
            if (hienTai === trangThaiTruoc) return;
            undoStack.push(trangThaiTruoc);
            if (undoStack.length > TOI_DA) undoStack.shift();
            redoStack.length = 0;
            trangThaiTruoc = hienTai;
            capNhatNut();
        }

        function ghiLichSu(delay = 100) {
            if (dangKhoiPhuc || !canvas) return;
            if (timerGhi) clearTimeout(timerGhi);
            if (delay <= 0) thucHienGhi();
            else timerGhi = setTimeout(thucHienGhi, delay);
        }

        function khoiPhuc(jsonStr) {
            if (!canvas) return;
            dangKhoiPhuc = true;
            canvas.loadFromJSON(jsonStr, () => {
                dongBoSauKhiKhoiPhuc();
                trangThaiTruoc = layTrangThai();
                dangKhoiPhuc = false;
                capNhatNut();
            });
        }

        function hoanTac() {
            if (dangKhoiPhuc || !canvas) return;
            if (timerGhi) { clearTimeout(timerGhi); timerGhi = null; thucHienGhi(); }
            if (undoStack.length === 0) return;
            const hienTai = layTrangThai();
            redoStack.push(hienTai);
            const prev = undoStack.pop();
            khoiPhuc(prev);
        }

        function lamLai() {
            if (dangKhoiPhuc || !canvas) return;
            if (timerGhi) { clearTimeout(timerGhi); timerGhi = null; thucHienGhi(); }
            if (redoStack.length === 0) return;
            const hienTai = layTrangThai();
            undoStack.push(hienTai);
            const next = redoStack.pop();
            khoiPhuc(next);
        }

        function init() {
            if (timerGhi) { clearTimeout(timerGhi); timerGhi = null; }
            undoStack.length = 0;
            redoStack.length = 0;
            trangThaiTruoc = layTrangThai();
            capNhatNut();
        }

        return {
            init,
            ghiLichSu,
            hoanTac,
            lamLai,
            capNhatNut,
            isRestoring: () => dangKhoiPhuc,
            getUndoCount: () => undoStack.length,
            getRedoCount: () => redoStack.length
        };
    })();
    window.LichSuCanvas = LichSuCanvas;

    function dongBoSauKhiKhoiPhuc() {
        cakeBoardObj = null;
        cakeBoardRimObj = null;
        cakeBaseObj = null;
        cakeRimObj = null;
        cakeSwirlsObj = null;
        cakeVienObj = null;
        cakeTier2Obj = null;
        cakeTier2RimObj = null;
        cakeTier2SwirlsObj = null;
        cakeTier2VienObj = null;
        printAreaBorder = null;
        printImageObj = null;
        textObj = null;

        canvas.getObjects().forEach(o => {
            if (o.customType === 'base_board') {
                cakeBoardObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'base_board_rim') {
                cakeBoardRimObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'base_circle') {
                cakeBaseObj = o;
                o.set({ selectable: false, evented: false });
                const stop = o.fill && o.fill.colorStops && o.fill.colorStops.find(c => c.offset === 0.65);
                if (stop) currentKemColor = stop.color;
            } else if (o.customType === 'base_rim') {
                cakeRimObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'tier2_circle') {
                cakeTier2Obj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'tier2_rim') {
                cakeTier2RimObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'tier2_swirls') {
                cakeTier2SwirlsObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'base_swirls') {
                cakeSwirlsObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'cake_vien') {
                cakeVienObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'tier2_vien') {
                cakeTier2VienObj = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'in_an_border') {
                printAreaBorder = o;
                o.set({ selectable: false, evented: false });
            } else if (o.customType === 'print_image') {
                printImageObj = o;
                o.clipPath = printAreaClip;
            } else if (o.customType === 'in_an_draw') {
                o.clipPath = printAreaClip;
            } else if (o.customType === 'chu') {
                textObj = o;
                ganSuKienTextObj(textObj);
                if (o.fontFamily) currentFontChu = o.fontFamily;
                if (typeof o.fill === 'string') currentMauChu = o.fill;
                o.set({
                    cornerSize: 8,
                    cornerColor: '#FFFFFF',
                    cornerStrokeColor: '#3A1B26',
                    borderColor: '#3A1B26',
                    shadow: new fabric.Shadow({
                        color: 'rgba(45, 18, 12, 0.32)',
                        blur: 3.5,
                        offsetX: 1,
                        offsetY: 2
                    })
                });
                o.setCoords();
            } else if (o.customType === 'topping') {
                o.set({
                    cornerSize: 10,
                    cornerColor: '#FFFFFF',
                    cornerStrokeColor: '#3A1B26',
                    cornerStyle: 'circle',
                    transparentCorners: false,
                    borderColor: '#3A1B26',
                    borderDashArray: [4, 4],
                    padding: 6,
                    hasRotatingPoint: true,
                    rotatingPointOffset: 16,
                    lockUniScaling: false,
                    lockScalingFlip: true,
                    lockScalingX: true,
                    lockScalingY: true,
                    shadow: new fabric.Shadow({
                        color: 'rgba(45, 15, 20, 0.32)',
                        blur: 8,
                        offsetX: 1.5,
                        offsetY: 4.5
                    })
                });
                o.setCoords();
            }
        });

        sapXepCacLopCanvas();
        coGianCanvasTheoKhung();
        canvas.renderAll();
        capNhatToppingsTuCanvas();

        // Đồng bộ chữ lên form và input canvas
        const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
        const inputChu = document.getElementById('input-chu-canvas');
        const chuHienCo = textObj ? (textObj.text || '') : '';
        if (inputChu && inputChu.value !== chuHienCo) inputChu.value = chuHienCo;
        if (formChu && formChu.value !== chuHienCo) formChu.value = chuHienCo;

        // Đồng bộ font chữ
        const selectFont = document.getElementById('select-font-chu');
        if (selectFont && currentFontChu) selectFont.value = currentFontChu;

        dongBoGiaoDienSwatches();
        if (currentStep === 5) capNhatBangTomTat();

        // Đồng bộ sang 3D nếu đang mở
        const khung3D = document.getElementById('cake-3d-wrapper');
        if (window.Cake3D && khung3D && !khung3D.classList.contains('d-none')) {
            window.Cake3D.syncFrom2D();
        }

        // Tự động lưu bản nháp
        luuNhapDebounced();
    }

    let currentStrawberryStyle = 'nguyen_qua';
    const STRAWBERRY_FILES = {
        'nguyen_qua': 'dau.svg',
        'bo_doi': 'dau-cat-doi.svg',
        'cat_lat': 'dau-lat.svg'
    };

    function adjustColor(hex, percent) {
        if (!hex || typeof hex !== 'string' || hex[0] !== '#') return hex || '#FFFDF5';
        let num = parseInt(hex.replace('#', ''), 16);
        let amt = Math.round(2.55 * percent);
        let R = (num >> 16) + amt;
        let G = (num >> 8 & 0x00FF) + amt;
        let B = (num & 0x0000FF) + amt;
        return '#' + (0x1000000 + (R < 255 ? R < 0 ? 0 : R : 255) * 0x10000 + (G < 255 ? G < 0 ? 0 : G : 255) * 0x100 + (B < 255 ? B < 0 ? 0 : B : 255)).toString(16).slice(1);
    }

    function createCakeGradient(hex) {
        return new fabric.Gradient({
            type: 'radial',
            coords: {
                r1: 12,
                r2: 155,
                x1: 160,
                y1: 155,
                x2: 200,
                y2: 200
            },
            colorStops: [
                { offset: 0, color: adjustColor(hex, 12) },
                { offset: 0.32, color: adjustColor(hex, 6) },
                { offset: 0.65, color: hex },
                { offset: 0.88, color: adjustColor(hex, -6) },
                { offset: 1, color: adjustColor(hex, -14) }
            ]
        });
    }

    let debounceTimer = null;

    // Thứ tự phân lớp (z-index) chuẩn:
    // Khay vàng (5) < Vòng dập gờ khay (7) < Thân kem bánh (10) < Gờ mép kem (12) < Vân xoáy kem (15) < Viền kem sò (20) < Thân kem tầng 2 (22) < Gờ tầng 2 (23) < Vân tầng 2 (23.5) < Viền sò tầng 2 (24) < Vùng in (30) < Ảnh in (35) < Nét vẽ (40) < Chữ (60) < Topping (70)
    function layTrongSoLop(obj) {
        if (!obj) return 0;
        const type = obj.customType;
        if (type === 'base_board' || obj === cakeBoardObj) return 5;
        if (type === 'base_board_rim' || obj === cakeBoardRimObj) return 7;
        if (type === 'base_circle' || obj === cakeBaseObj) return 10;
        if (type === 'base_rim' || obj === cakeRimObj) return 12;
        if (type === 'base_swirls' || obj === cakeSwirlsObj) return 15;
        if (type === 'cake_vien' || obj === cakeVienObj) return 20;
        if (type === 'tier2_circle' || obj === cakeTier2Obj) return 22;
        if (type === 'tier2_rim' || obj === cakeTier2RimObj) return 23;
        if (type === 'tier2_swirls' || obj === cakeTier2SwirlsObj) return 23.5;
        if (type === 'tier2_vien' || obj === cakeTier2VienObj) return 24;
        if (type === 'in_an_border' || obj === printAreaBorder) return 30;
        if (type === 'print_image' || obj === printImageObj) return 35;
        if (type === 'in_an_draw') return 40;
        if (type === 'chu' || obj === textObj) return 60;
        if (type === 'topping') return 70;
        return 45;
    }

    function sapXepCacLopCanvas() {
        if (!canvas || !canvas._objects) return;
        canvas._objects.sort((a, b) => layTrongSoLop(a) - layTrongSoLop(b));
        canvas.renderAll();
    }

    function preloadToppingImages() {
        ['dau.svg', 'dau-cat-doi.svg', 'dau-lat.svg', 'cherry.svg', 'socola.svg', 'macaron.svg'].forEach(file => {
            if (!toppingImageCache.has(file)) {
                const img = new Image();
                img.crossOrigin = 'anonymous';
                img.src = layDuongDanTopping(file);
                toppingImageCache.set(file, img);
            }
        });

        thanhPhans.forEach(tp => {
            if (tp.nhom === 'TOPPING' && tp.anh) {
                if (!toppingImageCache.has(tp.anh)) {
                    const img = new Image();
                    img.crossOrigin = 'anonymous';
                    img.src = layDuongDanTopping(tp.anh);
                    toppingImageCache.set(tp.anh, img);
                }
            }
        });
    }

    function init() {
        preloadToppingImages();
        initCanvas();
        coGianCanvasTheoKhung();
        initSwatches();
        initEvents();
        initStepper();
        init3DControls();

        // Nạp dữ liệu cũ (nếu sửa) hoặc mẫu gợi ý
        const jsonCu = document.getElementById('input-du-lieu-json')?.value;
        if (jsonCu && jsonCu.trim().startsWith('{')) {
            loadExistingDesign(jsonCu);
        } else {
            // Mặc định chọn bánh nền đầu tiên nếu có
            const selectSp = document.getElementById('select-banh-nen');
            if (selectSp && selectSp.options.length > 1) {
                const mauObj = mau && MAU_THIET_KE[mau];
                // Mẫu có viền: chọn cỡ bánh đầu tiên bán viền đó (viền gắn theo cỡ), nếu không mẫu sẽ mất viền
                let idx = 1;
                if (mauObj && mauObj.tenVien) {
                    const tenVien = mauObj.tenVien.toLowerCase();
                    const coVien = [...selectSp.options].findIndex(o => o.value && thanhPhans.some(t =>
                        t.nhom === 'VIEN' && t.ten.toLowerCase().includes(tenVien) &&
                        (!t.sanPhamNenId || t.sanPhamNenId === parseInt(o.value, 10))));
                    if (coVien > 0) idx = coVien;
                }
                selectSp.selectedIndex = idx;
                onBanhNenChange();

                if (mauObj) {
                    napMau(mauObj);
                }
                setTimeout(() => LichSuCanvas.init(), 150);
            }

            // Chế độ tạo mới (không sửa, không ?mau=): mời khách tiếp tục bản nháp nếu có
            if (!new URLSearchParams(location.search).has('mau')) hoiKhoiPhucNhap();
        }

        // Tự động kích hoạt bước báo lỗi từ server nếu có
        const elBuocLoi = document.getElementById('buoc-loi-server');
        const buocLoi = elBuocLoi ? parseInt(elBuocLoi.value, 10) : 1;
        if (buocLoi > 1 && buocLoi <= TOTAL_STEPS) {
            chuyenBuoc(buocLoi, false, true);
        }

        // Chỉ bắt đầu tự lưu sau khi khách thao tác thật (tránh lưu thiết kế mặc định lúc mới mở trang)
        ['pointerdown', 'keydown', 'input', 'change'].forEach(ev =>
            document.addEventListener(ev, () => { daTuongTac = true; }, { capture: true, once: true }));

        // Đồng bộ chữ nếu mở trang sửa hoặc form đã có sẵn chữ
        const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
        const inputChu = document.getElementById('input-chu-canvas');
        const chuKhoiTao = (inputChu?.value || formChu?.value || '').trim();
        if (chuKhoiTao) {
            capNhatChuCanvas(chuKhoiTao);
        }

        window.addEventListener('load', coGianCanvasTheoKhung);
    }

    function initCanvas() {
        const canvasEl = document.getElementById('cake-canvas');
        if (!canvasEl || typeof fabric === 'undefined') return;

        // Tinh chỉnh Bounding Box & Control Handles chuẩn Emil Design Polish (Màu hồng san hô, tròn, nốt viền)
        fabric.Object.prototype.borderColor = '#3A1B26';
        fabric.Object.prototype.borderScaleFactor = 2;
        fabric.Object.prototype.borderDashArray = [4, 4];
        fabric.Object.prototype.cornerColor = '#FFFFFF';
        fabric.Object.prototype.cornerStrokeColor = '#3A1B26';
        fabric.Object.prototype.cornerStyle = 'circle';
        fabric.Object.prototype.cornerSize = 10;
        fabric.Object.prototype.transparentCorners = false;
        fabric.Object.prototype.padding = 6;

        canvas = new fabric.Canvas('cake-canvas', {
            selection: true,
            preserveObjectStacking: true,
            width: CHIEU_RONG_CHUAN,
            height: CHIEU_RONG_CHUAN
        });
        window._cakeCanvas = canvas;

        // Vùng in ảnh clipPath
        printAreaClip = new fabric.Rect({
            left: 200,
            top: 200,
            width: 150,
            height: 150,
            originX: 'center',
            originY: 'center',
            rx: 6,
            ry: 6,
            absolutePositioned: true
        });

        drawCakeBase();
        coGianCanvasTheoKhung();
        LichSuCanvas.init();

        const khung3D = document.getElementById('cake-3d-wrapper');
        canvas.on('after:render', function () {
            // Chỉ đồng bộ khi đang xem 3D: mỗi lần đồng bộ dựng lại topping 3D và vẽ lại canvas 2 lần
            if (window.Cake3D && khung3D && !khung3D.classList.contains('d-none')) {
                window.Cake3D.syncFrom2D();
            }
            luuNhapDebounced(); // mọi thay đổi trên canvas (thêm/xóa/sửa/màu/chữ) đều render lại
        });

        // Phím tắt bàn phím: Hoàn tác (Ctrl+Z), Làm lại (Ctrl+Y / Ctrl+Shift+Z), Xóa (Delete/Backspace)
        window.addEventListener('keydown', function (e) {
            const tag = (e.target.tagName || '').toUpperCase();
            if (tag === 'INPUT' || tag === 'TEXTAREA' || e.target.isContentEditable) return;

            if (e.ctrlKey || e.metaKey) {
                if (e.key === 'z' || e.key === 'Z') {
                    e.preventDefault();
                    if (e.shiftKey) {
                        LichSuCanvas.lamLai();
                    } else {
                        LichSuCanvas.hoanTac();
                    }
                } else if (e.key === 'y' || e.key === 'Y') {
                    e.preventDefault();
                    LichSuCanvas.lamLai();
                }
            } else if (e.key === 'Delete' || e.key === 'Backspace') {
                xoaPhanTuChon();
            }
        });

        // Lắng nghe sự kiện đổi kích thước cửa sổ hoặc xoay thiết bị để co giãn canvas vừa vặn
        let resizeTimer = null;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(coGianCanvasTheoKhung, 100);
        });
        window.addEventListener('orientationchange', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(coGianCanvasTheoKhung, 150);
        });

        canvas.on('object:moving', function (e) {
            capNhatTangChoToppingObject(e.target);
        });

        canvas.on('object:removed', function (e) {
            const obj = e.target;
            if (obj && obj.customType === 'topping' && obj.thanhPhanId) {
                capNhatToppingsTuCanvas();
            }
            LichSuCanvas.ghiLichSu();
        });

        canvas.on('object:added', function (e) {
            capNhatTangChoToppingObject(e.target);
            sapXepCacLopCanvas();
            LichSuCanvas.ghiLichSu();
        });

        canvas.on('object:modified', function (e) {
            capNhatTangChoToppingObject(e.target);
            sapXepCacLopCanvas();
            LichSuCanvas.ghiLichSu();
        });

    }

    function coGianCanvasTheoKhung() {
        if (!canvas) return;
        const wrapper = document.getElementById('canvas-wrapper');
        const container = wrapper ? wrapper.parentElement : null;
        let availableWidth = container ? container.clientWidth : 0;
        if (container) {
            const cs = window.getComputedStyle(container);
            availableWidth -= (parseFloat(cs.paddingLeft) || 0) + (parseFloat(cs.paddingRight) || 0);
        }
        if (!availableWidth || availableWidth <= 0) {
            availableWidth = Math.min(window.innerWidth - 32, CHIEU_RONG_CHUAN);
        }
        let wrapperExtra = 20;
        if (wrapper) {
            const ws = window.getComputedStyle(wrapper);
            wrapperExtra = (parseFloat(ws.paddingLeft) || 0) + (parseFloat(ws.paddingRight) || 0)
                         + (parseFloat(ws.borderLeftWidth) || 0) + (parseFloat(ws.borderRightWidth) || 0);
        }
        const maxCanvasWidth = Math.max(200, Math.floor(availableWidth - wrapperExtra));
        const targetWidth = Math.min(CHIEU_RONG_CHUAN, maxCanvasWidth);
        const zoom = targetWidth / CHIEU_RONG_CHUAN;

        canvas.setDimensions({ width: targetWidth, height: targetWidth });
        canvas.setZoom(zoom);
        canvas.renderAll();
    }

    function taoVanKemBo(cx, cy, radius) {
        const scale = radius / 155;
        // Đường xoáy lượn sóng tự nhiên từ rìa bánh vào tâm (Archimedean spatula sweep)
        const pathData = 'M 88 200 C 88 138, 138 88, 200 88 C 262 88, 312 138, 312 200 C 312 250, 275 292, 225 304 C 180 314, 132 290, 118 248 C 106 210, 126 168, 164 154 C 196 142, 234 158, 242 190 C 250 218, 228 244, 200 244 C 178 244, 165 228, 168 208 C 171 192, 186 182, 200 184 C 208 185, 214 192, 212 198';

        const shadowPath = new fabric.Path(pathData, {
            fill: 'transparent',
            stroke: 'rgba(45, 20, 10, 0.07)',
            strokeWidth: 8 * scale,
            strokeLineCap: 'round',
            strokeLineJoin: 'round',
            originX: 'center',
            originY: 'center',
            left: cx + 1.2 * scale,
            top: cy + 1.6 * scale,
            scaleX: scale,
            scaleY: scale,
            selectable: false,
            evented: false
        });

        const highlightPath = new fabric.Path(pathData, {
            fill: 'transparent',
            stroke: 'rgba(255, 255, 255, 0.30)',
            strokeWidth: 5.5 * scale,
            strokeLineCap: 'round',
            strokeLineJoin: 'round',
            originX: 'center',
            originY: 'center',
            left: cx - 0.8 * scale,
            top: cy - 0.8 * scale,
            scaleX: scale,
            scaleY: scale,
            selectable: false,
            evented: false
        });

        return new fabric.Group([shadowPath, highlightPath], {
            left: cx,
            top: cy,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false
        });
    }

    function drawCakeBase() {
        if (!canvas) return;

        // 1. Khay đế bánh mạ kim sang trọng (Luxury Gold Foil Cake Board)
        if (cakeBoardObj) {
            canvas.remove(cakeBoardObj);
            cakeBoardObj = null;
        }
        cakeBoardObj = new fabric.Circle({
            left: 200,
            top: 200,
            radius: 172,
            fill: new fabric.Gradient({
                type: 'linear',
                coords: { x1: 28, y1: 28, x2: 372, y2: 372 },
                colorStops: [
                    { offset: 0, color: '#E8C779' },
                    { offset: 0.22, color: '#FFF8E1' },
                    { offset: 0.45, color: '#C89B3C' },
                    { offset: 0.68, color: '#FDF1C9' },
                    { offset: 0.88, color: '#A67722' },
                    { offset: 1, color: '#7D5412' }
                ]
            }),
            stroke: '#D4A843',
            strokeWidth: 1.5,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false,
            shadow: new fabric.Shadow({
                color: 'rgba(40, 15, 10, 0.20)',
                blur: 18,
                offsetX: 0,
                offsetY: 7
            }),
            customType: 'base_board'
        });
        canvas.add(cakeBoardObj);

        // 1b. Vòng gờ dập nổi đế lót bánh (Embossed Foil Ring)
        if (cakeBoardRimObj) {
            canvas.remove(cakeBoardRimObj);
            cakeBoardRimObj = null;
        }
        cakeBoardRimObj = new fabric.Circle({
            left: 200,
            top: 200,
            radius: 164,
            fill: 'transparent',
            stroke: new fabric.Gradient({
                type: 'linear',
                coords: { x1: 40, y1: 40, x2: 360, y2: 360 },
                colorStops: [
                    { offset: 0, color: 'rgba(255, 255, 255, 0.55)' },
                    { offset: 0.5, color: 'rgba(180, 135, 46, 0.40)' },
                    { offset: 1, color: 'rgba(100, 60, 15, 0.50)' }
                ]
            }),
            strokeWidth: 1.2,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false,
            customType: 'base_board_rim'
        });
        canvas.add(cakeBoardRimObj);

        // 2. Thân kem bánh satin (Frosting Base)
        if (cakeBaseObj) {
            canvas.remove(cakeBaseObj);
            cakeBaseObj = null;
        }
        cakeBaseObj = new fabric.Circle({
            left: 200,
            top: 200,
            radius: 155,
            fill: createCakeGradient(currentKemColor),
            stroke: adjustColor(currentKemColor, -12),
            strokeWidth: 1.2,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false,
            hoverCursor: 'default',
            shadow: new fabric.Shadow({
                color: 'rgba(55, 22, 14, 0.24)',
                blur: 14,
                offsetX: 2,
                offsetY: 6
            }),
            customType: 'base_circle'
        });
        canvas.add(cakeBaseObj);

        // 2a. Gờ bo tròn mép kem (Frosting Crown / Bevel Ring)
        if (cakeRimObj) {
            canvas.remove(cakeRimObj);
            cakeRimObj = null;
        }
        cakeRimObj = new fabric.Circle({
            left: 200,
            top: 200,
            radius: 154,
            fill: 'transparent',
            stroke: new fabric.Gradient({
                type: 'linear',
                coords: { x1: 60, y1: 60, x2: 340, y2: 340 },
                colorStops: [
                    { offset: 0, color: 'rgba(255, 255, 255, 0.50)' },
                    { offset: 0.45, color: 'rgba(255, 255, 255, 0.10)' },
                    { offset: 0.75, color: 'rgba(0, 0, 0, 0.03)' },
                    { offset: 1, color: 'rgba(0, 0, 0, 0.12)' }
                ]
            }),
            strokeWidth: 2,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false,
            customType: 'base_rim'
        });
        canvas.add(cakeRimObj);

        // 2b. Thân kem tầng 2 (nếu chọn bánh 2 tầng)
        if (cakeTier2Obj) {
            canvas.remove(cakeTier2Obj);
            cakeTier2Obj = null;
        }
        if (cakeTier2RimObj) {
            canvas.remove(cakeTier2RimObj);
            cakeTier2RimObj = null;
        }
        if (cakeTier2SwirlsObj) {
            canvas.remove(cakeTier2SwirlsObj);
            cakeTier2SwirlsObj = null;
        }
        if (soTangState === 2) {
            const r2 = layBanKinhTang2();
            cakeTier2Obj = new fabric.Circle({
                left: 200,
                top: 200,
                radius: r2,
                fill: createCakeGradient(currentKemColor),
                stroke: adjustColor(currentKemColor, -12),
                strokeWidth: 1.2,
                originX: 'center',
                originY: 'center',
                selectable: false,
                evented: false,
                hoverCursor: 'default',
                shadow: new fabric.Shadow({
                    color: 'rgba(50, 20, 10, 0.26)',
                    blur: 16,
                    offsetX: 2,
                    offsetY: 6
                }),
                customType: 'tier2_circle'
            });
            canvas.add(cakeTier2Obj);

            cakeTier2RimObj = new fabric.Circle({
                left: 200,
                top: 200,
                radius: r2 - 1,
                fill: 'transparent',
                stroke: new fabric.Gradient({
                    type: 'linear',
                    coords: { x1: 200 - r2, y1: 200 - r2, x2: 200 + r2, y2: 200 + r2 },
                    colorStops: [
                        { offset: 0, color: 'rgba(255, 255, 255, 0.50)' },
                        { offset: 0.45, color: 'rgba(255, 255, 255, 0.10)' },
                        { offset: 0.75, color: 'rgba(0, 0, 0, 0.03)' },
                        { offset: 1, color: 'rgba(0, 0, 0, 0.12)' }
                    ]
                }),
                strokeWidth: 1.8,
                originX: 'center',
                originY: 'center',
                selectable: false,
                evented: false,
                customType: 'tier2_rim'
            });
            canvas.add(cakeTier2RimObj);

            cakeTier2SwirlsObj = taoVanKemBo(200, 200, r2);
            cakeTier2SwirlsObj.customType = 'tier2_swirls';
            canvas.add(cakeTier2SwirlsObj);
        }

        // 3. Vân kem bơ chà láng thủ công (Artisan Spatula Swirls)
        if (cakeSwirlsObj) {
            canvas.remove(cakeSwirlsObj);
            cakeSwirlsObj = null;
        }
        cakeSwirlsObj = taoVanKemBo(200, 200, 155);
        cakeSwirlsObj.customType = 'base_swirls';
        canvas.add(cakeSwirlsObj);

        sapXepCacLopCanvas();
    }

    function taoNhomRosette(x, y, rotDeg, scale, creamColor, shadeColor) {
        // Cánh sò kem bơ tạo hình từ đui sao Pháp (French Star Piping Tip)
        const shellBase = new fabric.Path('M 0 -11 C 7 -11, 10 -5, 8 3 C 6 8, 3 12, 0 14 C -3 12, -6 8, -8 3 C -10 -5, -7 -11, 0 -11 Z', {
            fill: creamColor,
            stroke: shadeColor,
            strokeWidth: 0.7,
            originX: 'center',
            originY: 'center',
            left: 0,
            top: 0
        });

        // Rãnh khía đui sao trung tâm và hai cánh bên (Star fluting ridges)
        const spineRidge = new fabric.Path('M 0 -9 C 0 -2, 0 4, 0 11', {
            fill: 'transparent',
            stroke: shadeColor,
            strokeWidth: 1.2,
            strokeLineCap: 'round',
            originX: 'center',
            originY: 'center',
            left: 0,
            top: 0,
            opacity: 0.55
        });

        const leftRidge = new fabric.Path('M -4 -7 C -3.5 -1, -2 4, 0 10', {
            fill: 'transparent',
            stroke: shadeColor,
            strokeWidth: 0.9,
            strokeLineCap: 'round',
            originX: 'center',
            originY: 'center',
            left: -1.5,
            top: 0.5,
            opacity: 0.45
        });

        const rightRidge = new fabric.Path('M 4 -7 C 3.5 -1, 2 4, 0 10', {
            fill: 'transparent',
            stroke: shadeColor,
            strokeWidth: 0.9,
            strokeLineCap: 'round',
            originX: 'center',
            originY: 'center',
            left: 1.5,
            top: 0.5,
            opacity: 0.45
        });

        // Vệt bắt sáng bóng mượt trên sống lưng sò kem (Specular highlight)
        const highlight = new fabric.Path('M 1.5 -8 C 2.2 -3, 1.8 3, 0 9', {
            fill: 'transparent',
            stroke: '#FFFFFF',
            strokeWidth: 1.3,
            strokeLineCap: 'round',
            originX: 'center',
            originY: 'center',
            left: 0.8,
            top: -0.2,
            opacity: 0.85
        });

        return new fabric.Group([shellBase, leftRidge, rightRidge, spineRidge, highlight], {
            left: x,
            top: y,
            angle: rotDeg,
            scaleX: scale,
            scaleY: scale,
            originX: 'center',
            originY: 'center',
            selectable: false,
            evented: false
        });
    }

    function updateCakeVien() {
        if (!canvas) return;
        if (cakeVienObj) {
            canvas.remove(cakeVienObj);
            cakeVienObj = null;
        }
        if (cakeTier2VienObj) {
            canvas.remove(cakeTier2VienObj);
            cakeTier2VienObj = null;
        }

        if (selectedVienId) {
            const rosettes = [];
            const count = 36;
            const radius = 151;
            const creamColor = currentKemColor.toLowerCase() === '#fffdf5' ? '#FFFFFF' : adjustColor(currentKemColor, 15);
            const shadeColor = adjustColor(creamColor, -18);

            for (let i = 0; i < count; i++) {
                const angleRad = (i / count) * Math.PI * 2;
                const x = 200 + radius * Math.cos(angleRad);
                const y = 200 + radius * Math.sin(angleRad);
                const rotDeg = (angleRad * 180 / Math.PI) + 90;
                rosettes.push(taoNhomRosette(x, y, rotDeg, 1.0, creamColor, shadeColor));
            }

            cakeVienObj = new fabric.Group(rosettes, {
                selectable: false,
                evented: false,
                shadow: new fabric.Shadow({
                    color: 'rgba(50, 18, 25, 0.22)',
                    blur: 5,
                    offsetX: 1,
                    offsetY: 2.5
                }),
                customType: 'cake_vien'
            });
            canvas.add(cakeVienObj);

            // Viền tầng 2 nếu có
            if (soTangState === 2) {
                const r2 = layBanKinhTang2();
                const radius2 = r2 - 4;
                const count2 = Math.round(36 * (r2 / 155));
                const rosettes2 = [];

                for (let i = 0; i < count2; i++) {
                    const angleRad = (i / count2) * Math.PI * 2;
                    const x = 200 + radius2 * Math.cos(angleRad);
                    const y = 200 + radius2 * Math.sin(angleRad);
                    const rotDeg = (angleRad * 180 / Math.PI) + 90;
                    rosettes2.push(taoNhomRosette(x, y, rotDeg, 0.85, creamColor, shadeColor));
                }

                cakeTier2VienObj = new fabric.Group(rosettes2, {
                    selectable: false,
                    evented: false,
                    shadow: new fabric.Shadow({
                        color: 'rgba(50, 18, 25, 0.22)',
                        blur: 4,
                        offsetX: 1,
                        offsetY: 2
                    }),
                    customType: 'tier2_vien'
                });
                canvas.add(cakeTier2VienObj);
            }

            sapXepCacLopCanvas();
        }
        canvas.renderAll();
    }

    function initSwatches() {
        const kemContainer = document.getElementById('swatch-mau-kem');
        const labelKem = document.getElementById('label-ten-mau-kem');
        if (kemContainer) {
            kemContainer.innerHTML = '';
            MAU_KEM.forEach((m, idx) => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = `dat-swatch-btn ${idx === 0 ? 'active' : ''}`;
                btn.style.backgroundColor = m.hex;
                btn.dataset.hex = m.hex;
                btn.title = m.ten;
                btn.setAttribute('aria-label', `Chọn màu kem ${m.ten}`);
                btn.onclick = () => {
                    currentKemColor = m.hex;
                    if (labelKem) labelKem.textContent = m.ten;
                    drawCakeBase();
                    updateCakeVien();
                    capNhatLayerExplorer();
                    if (window.Cake3D && typeof window.Cake3D.setCakeColor === 'function') {
                        window.Cake3D.setCakeColor(currentKemColor);
                    }
                    canvas.renderAll();
                    kemContainer.querySelectorAll('.dat-swatch-btn').forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    if (currentStep === 5) capNhatBangTomTat();
                    luuNhapDebounced();
                };
                kemContainer.appendChild(btn);
            });
        }

        const chuContainer = document.getElementById('swatch-mau-chu');
        if (chuContainer) {
            chuContainer.innerHTML = '';
            MAU_CHU.forEach((hex, idx) => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = `dat-swatch-btn ${idx === 0 ? 'active' : ''}`;
                btn.style.backgroundColor = hex;
                btn.dataset.hex = hex;
                btn.setAttribute('aria-label', `Màu chữ ${hex}`);
                btn.onclick = () => {
                    currentMauChu = hex;
                    if (textObj) {
                        textObj.set('fill', currentMauChu);
                        canvas.renderAll();
                    }
                    chuContainer.querySelectorAll('.dat-swatch-btn').forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    luuNhapDebounced();
                    LichSuCanvas.ghiLichSu(0);
                };
                chuContainer.appendChild(btn);
            });
        }
    }

    /**
     * Đồng bộ lại trạng thái active của các ô swatch màu kem, font chữ và màu chữ
     */
    function dongBoGiaoDienSwatches() {
        // 1. Tô lại ô màu kem đang chọn
        const kemContainer = document.getElementById('swatch-mau-kem');
        const labelKem = document.getElementById('label-ten-mau-kem');
        if (kemContainer && currentKemColor) {
            const hexLower = currentKemColor.toLowerCase();
            const foundKem = MAU_KEM.find(m => m.hex.toLowerCase() === hexLower);
            kemContainer.querySelectorAll('.dat-swatch-btn').forEach(btn => {
                const btnHex = (btn.dataset.hex || '').toLowerCase();
                const isMatch = (btnHex && btnHex === hexLower) ||
                                (foundKem && btn.title === foundKem.ten) ||
                                (btn.style.backgroundColor && (btn.style.backgroundColor === hexLower || btn.style.backgroundColor === currentKemColor));
                btn.classList.toggle('active', !!isMatch);
            });
            if (labelKem && foundKem) {
                labelKem.textContent = foundKem.ten;
            }
        }

        // 2. Chọn lại font chữ trong dropdown
        const selectFont = document.getElementById('select-font-chu');
        if (selectFont && currentFontChu) {
            selectFont.value = currentFontChu;
        }

        // 3. Tô lại ô màu chữ đang chọn
        const chuContainer = document.getElementById('swatch-mau-chu');
        if (chuContainer && currentMauChu) {
            const hexLower = currentMauChu.toLowerCase();
            const dummy = document.createElement('div');
            dummy.style.color = currentMauChu;
            const targetRgb = dummy.style.color;

            chuContainer.querySelectorAll('.dat-swatch-btn').forEach(btn => {
                const btnHex = (btn.dataset.hex || '').toLowerCase();
                const aria = (btn.getAttribute('aria-label') || '').toLowerCase();
                const isMatch = (btnHex && btnHex === hexLower) ||
                                aria.includes(hexLower) ||
                                btn.style.backgroundColor === targetRgb ||
                                btn.style.backgroundColor === currentMauChu;
                btn.classList.toggle('active', !!isMatch);
            });
        }
    }

    // =========================================================================
    // STEPPER MANAGEMENT
    // =========================================================================
    function chuyenBuoc(step, cuon = true, boQuaKiemTra = false) {
        if (step < 1 || step > TOTAL_STEPS) return;

        // Ràng buộc kiểm tra trước khi chuyển bước
        if (!boQuaKiemTra) {
            if (step > 1 && !selectedSanPhamNenId) {
                hienToast('Vui lòng chọn kích cỡ bánh nền trước khi tiếp tục.', 'warning');
                return;
            }
            if (step > 2 && !selectedCotId) {
                hienToast('Vui lòng chọn một loại cốt bánh (bắt buộc).', 'warning');
                return;
            }
        }

        currentStep = step;

        // 1. Cập nhật Panels
        for (let i = 1; i <= TOTAL_STEPS; i++) {
            const panel = document.getElementById(`panel-step-${i}`);
            if (panel) {
                panel.classList.toggle('active', i === currentStep);
            }
        }

        // 2. Cập nhật Stepper Buttons & Lines
        for (let i = 1; i <= TOTAL_STEPS; i++) {
            const btn = document.getElementById(`step-btn-${i}`);
            if (btn) {
                btn.classList.toggle('active', i === currentStep);
                btn.classList.toggle('completed', i < currentStep);
                if (i === currentStep) {
                    btn.setAttribute('aria-current', 'step');
                } else {
                    btn.removeAttribute('aria-current');
                }
            }
            const line = document.getElementById(`line-${i}`);
            if (line) {
                line.classList.toggle('completed', i < currentStep);
            }
        }

        // 2b. Dòng "Bước n/5" trên mobile
        const dongBuoc = document.getElementById('dat-step-now');
        const nhanBuoc = document.querySelector(`#step-btn-${currentStep} .dat-step-label`);
        if (dongBuoc && nhanBuoc) dongBuoc.textContent = `Bước ${currentStep}/${TOTAL_STEPS}: ${nhanBuoc.textContent}`;

        // 3. Cập nhật Mobile Bottom Bar Buttons
        const btnMobPrev = document.getElementById('btn-mobile-prev');
        const btnMobNext = document.getElementById('btn-mobile-next');
        if (btnMobPrev) {
            btnMobPrev.style.visibility = currentStep > 1 ? 'visible' : 'hidden';
        }
        if (btnMobNext) {
            if (currentStep === TOTAL_STEPS) {
                btnMobNext.textContent = 'Gửi thiết kế';
                btnMobNext.onclick = () => {
                    const btnSubmit = document.getElementById('btn-luu-thiet-ke');
                    if (btnSubmit) btnSubmit.click();
                };
            } else {
                btnMobNext.textContent = 'Tiếp';
                btnMobNext.onclick = () => chuyenBuoc(currentStep + 1);
            }
        }

        // 4. Nếu tới bước 5, làm mới bảng tóm tắt cấu hình
        if (currentStep === 5) {
            capNhatBangTomTat();
        }

        // 5. Cuộn mượt đến đầu panel trên mobile
        // Lần đầu mở trang không cuộn, để khách thấy ảnh bánh xem trước ở đầu trang
        if (cuon && window.innerWidth <= 768) {
            const activePanel = document.getElementById(`panel-step-${currentStep}`);
            if (activePanel) {
                activePanel.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        }
    }

    function initStepper() {
        // Nút trên Stepper Header
        for (let i = 1; i <= TOTAL_STEPS; i++) {
            const btn = document.getElementById(`step-btn-${i}`);
            if (btn) {
                btn.onclick = () => chuyenBuoc(i);
            }
        }

        // Nút Goto step bất kỳ
        document.querySelectorAll('.btn-goto-step').forEach(btn => {
            const target = parseInt(btn.getAttribute('data-step'), 10);
            btn.onclick = () => chuyenBuoc(target);
        });

        // Nút Next / Prev trong từng Panel
        document.querySelectorAll('.btn-next-step').forEach(btn => {
            const target = parseInt(btn.getAttribute('data-next'), 10);
            btn.onclick = () => chuyenBuoc(target);
        });
        document.querySelectorAll('.btn-prev-step').forEach(btn => {
            const target = parseInt(btn.getAttribute('data-prev'), 10);
            btn.onclick = () => chuyenBuoc(target);
        });

        // Mobile Bar Buttons
        const btnMobPrev = document.getElementById('btn-mobile-prev');
        const btnMobNext = document.getElementById('btn-mobile-next');
        if (btnMobPrev) btnMobPrev.onclick = () => chuyenBuoc(currentStep - 1);
        if (btnMobNext) btnMobNext.onclick = () => chuyenBuoc(currentStep + 1);

        chuyenBuoc(1, false);
    }

    // Tải Three.js + OrbitControls + thiet-ke-3d.js một lần duy nhất, khi khách mở tab 3D lần đầu.
    // URL (có fingerprint) lấy từ <template id="tpl-3d-scripts"> do Razor sinh ra.
    let tai3DPromise = null;
    function nap3D() {
        if (tai3DPromise) return tai3DPromise;
        const tpl = document.getElementById('tpl-3d-scripts');
        const urls = tpl ? Array.from(tpl.content.querySelectorAll('script')).map(s => s.getAttribute('src')) : [];
        const btn = document.getElementById('btn-view-3d');
        const container = document.getElementById('cake-3d-container');
        if (btn) btn.setAttribute('aria-busy', 'true');
        if (container) {
            container.classList.add('is-loading');
            container.textContent = 'Đang tải mô hình 3D...';
        }
        tai3DPromise = urls.reduce(function (p, url) {
            return p.then(function () {
                return new Promise(function (ok, loi) {
                    const s = document.createElement('script');
                    s.src = url;
                    s.onload = ok;
                    s.onerror = function () { loi(new Error('Không tải được ' + url)); };
                    document.head.appendChild(s);
                });
            });
        }, Promise.resolve()).then(function () {
            if (!window.Cake3D) throw new Error('Cake3D không khả dụng (WebGL/Three.js)');
            if (container) container.classList.remove('is-loading');
            window.Cake3D.init(container, canvas.lowerCanvasEl || document.getElementById('cake-canvas'));
            window.Cake3D.setFabricCanvas(canvas);
            dongBoTrangThai3D();
        }).catch(function (e) {
            tai3DPromise = null; // cho phép thử lại ở lần bấm sau
            throw e;
        }).finally(function () {
            if (btn) btn.removeAttribute('aria-busy');
        });
        return tai3DPromise;
    }

    // Đẩy toàn bộ trạng thái hiện tại (màu kem, cỡ bánh, viền, cốt) sang mô hình 3D vừa khởi tạo
    function dongBoTrangThai3D() {
        const c3 = window.Cake3D;
        if (!c3) return;
        c3.setCakeColor(currentKemColor);
        const cm = layDuongKinhBanhNen();
        if (cm) c3.setCakeDimensions(cm);
        c3.setRimStyle(selectedVienId !== null);
        const cot = thanhPhans.find(t => t.id === selectedCotId);
        if (cot) c3.setCotFlavor(cot.ten);
        if (typeof c3.setCandles === 'function') {
            c3.setCandles(soLuongNen);
        }
        if (typeof c3.setTiers === 'function') {
            c3.setTiers(soTangState, layKichThuocTang2());
        }
        c3.syncFrom2D();
    }

    // Đường kính (cm) của bánh nền đang chọn: số đầu tiên trong kichThuoc (vd "20cm")
    function layDuongKinhBanhNen() {
        const sp = sanPhams.find(s => s.id === selectedSanPhamNenId);
        const match = sp && sp.kichThuoc ? sp.kichThuoc.match(/\d+/) : null;
        return match ? parseInt(match[0], 10) : null;
    }

    function init3DControls() {
        const btnView2D = document.getElementById('btn-view-2d');
        const btnView3D = document.getElementById('btn-view-3d');
        const canvasWrapper = document.getElementById('canvas-wrapper');
        const cake3DWrapper = document.getElementById('cake-3d-wrapper');
        const tools2D = document.getElementById('tools-2d');
        const tools3D = document.getElementById('tools-3d');
        const hint2D = document.getElementById('hint-2d');
        const hint3D = document.getElementById('hint-3d');

        function switchView(mode) {
            if (mode === '3d') {
                if (btnView2D) {
                    btnView2D.classList.remove('active');
                    btnView2D.setAttribute('aria-selected', 'false');
                }
                if (btnView3D) {
                    btnView3D.classList.add('active');
                    btnView3D.setAttribute('aria-selected', 'true');
                }
                if (canvasWrapper) canvasWrapper.classList.add('d-none');
                if (cake3DWrapper) cake3DWrapper.classList.remove('d-none');
                if (tools2D) tools2D.classList.add('d-none');
                if (tools3D) tools3D.classList.remove('d-none');
                if (hint2D) hint2D.classList.add('d-none');
                if (hint3D) hint3D.classList.remove('d-none');

                if (window.Cake3D) {
                    window.Cake3D.resize();
                    window.Cake3D.syncFrom2D();
                } else {
                    nap3D().then(function () {
                        if (window.Cake3D) window.Cake3D.resize();
                    }).catch(function (e) {
                        console.error('Không nạp được thư viện 3D:', e);
                        hienToast('Không tải được mô hình 3D. Vui lòng kiểm tra kết nối và thử lại.', 'danger');
                        switchView('2d');
                    });
                }
            } else {
                if (btnView3D) {
                    btnView3D.classList.remove('active');
                    btnView3D.setAttribute('aria-selected', 'false');
                }
                if (btnView2D) {
                    btnView2D.classList.add('active');
                    btnView2D.setAttribute('aria-selected', 'true');
                }
                if (cake3DWrapper) cake3DWrapper.classList.add('d-none');
                if (canvasWrapper) canvasWrapper.classList.remove('d-none');
                if (tools3D) tools3D.classList.add('d-none');
                if (tools2D) tools2D.classList.remove('d-none');
                if (hint3D) hint3D.classList.add('d-none');
                if (hint2D) hint2D.classList.remove('d-none');

                coGianCanvasTheoKhung();
            }
        }

        if (btnView2D) btnView2D.addEventListener('click', () => switchView('2d'));
        if (btnView3D) btnView3D.addEventListener('click', () => switchView('3d'));

        // 3D tools listeners
        const btnRotate = document.getElementById('btn-3d-auto-rotate');
        if (btnRotate) {
            btnRotate.addEventListener('click', function () {
                if (window.Cake3D) {
                    const isRotating = window.Cake3D.toggleAutoRotate();
                    btnRotate.classList.toggle('active', isRotating);
                    btnRotate.setAttribute('aria-pressed', isRotating);
                    const icon = document.getElementById('icon-3d-rotate');
                    if (icon) icon.innerHTML = isRotating ? ICON_PAUSE : ICON_PLAY;
                }
            });
        }

        const btnCutaway = document.getElementById('btn-3d-cutaway');
        if (btnCutaway) {
            btnCutaway.addEventListener('click', function () {
                if (window.Cake3D) {
                    const isCut = window.Cake3D.toggleCutaway();
                    btnCutaway.classList.toggle('active', isCut);
                }
            });
        }

        const btnResetCam = document.getElementById('btn-3d-reset-cam');
        if (btnResetCam) {
            btnResetCam.addEventListener('click', function () {
                if (window.Cake3D) window.Cake3D.resetCamera();
            });
        }
    }

    function capNhatBangTomTat() {
        // Bánh nền
        const sp = sanPhams.find(s => s.id === selectedSanPhamNenId);
        const sumNen = document.getElementById('summary-banh-nen');
        if (sumNen) sumNen.textContent = sp ? `${sp.ten} (${sp.kichThuoc})` : 'Chưa chọn';

        // Số tầng bánh
        const sumTang = document.getElementById('summary-so-tang');
        if (sumTang) {
            const tangTp = thanhPhans.find(t => t.nhom === 'TANG' && t.sanPhamNenId === selectedSanPhamNenId);
            if (soTangState === 2 && tangTp) {
                sumTang.textContent = `2 tầng (${tangTp.ten})`;
            } else {
                sumTang.textContent = '1 tầng';
            }
        }

        // Cốt bánh
        const cot = thanhPhans.find(t => t.id === selectedCotId);
        const sumCot = document.getElementById('summary-cot');
        if (sumCot) sumCot.textContent = cot ? cot.ten : 'Chưa chọn';

        // Màu kem
        const kemObj = MAU_KEM.find(m => m.hex.toLowerCase() === currentKemColor.toLowerCase()) || { ten: currentKemColor };
        const sumKem = document.getElementById('summary-mau-kem');
        if (sumKem) sumKem.textContent = kemObj.ten;

        // Viền bánh
        const vien = thanhPhans.find(t => t.id === selectedVienId);
        const sumVien = document.getElementById('summary-vien');
        if (sumVien) sumVien.textContent = vien ? vien.ten : 'Không viền';

        // Toppings
        const sumTop = document.getElementById('summary-toppings');
        if (sumTop) {
            const list = [];
            for (const [id, count] of toppingsMap.entries()) {
                if (count > 0) {
                    const tp = thanhPhans.find(t => t.id === id);
                    if (tp) list.push(`${tp.ten} × ${count}`);
                }
            }
            sumTop.textContent = list.length > 0 ? list.join(', ') : 'Không có phụ kiện';
        }

        // Nến sinh nhật
        const sumNenSinhNhat = document.getElementById('summary-nen');
        if (sumNenSinhNhat) {
            sumNenSinhNhat.textContent = soLuongNen > 0 ? `${soLuongNen} cây (Miễn phí)` : 'Không lấy nến';
        }

        // Chữ
        const inputChu = document.getElementById('input-chu-canvas');
        const chu = (inputChu?.value || '').trim();
        const sumChu = document.getElementById('summary-chu');
        if (sumChu) sumChu.textContent = chu ? `"${chu}"` : 'Không ghi chữ';
        const sumChuRow = document.getElementById('tom-tat-chu-tren-banh');
        if (sumChuRow) sumChuRow.textContent = chu ? `"${chu}"` : 'Không ghi chữ';

        // In ảnh
        const sumIn = document.getElementById('summary-in-an');
        if (sumIn) {
            sumIn.textContent = selectedInAnId ? (uploadedImageFile ? 'Đã tải ảnh lên' : (isDrawingMode ? 'Có hình vẽ tay' : 'Bật khung in ảnh')) : 'Không dùng';
        }

        // Tổng tiền
        const elTong = document.getElementById('hien-thi-tong');
        const sumTong = document.getElementById('summary-gia-tong');
        if (sumTong && elTong) sumTong.textContent = elTong.textContent;
    }

    function initEvents() {
        const selectSp = document.getElementById('select-banh-nen');
        if (selectSp) {
            selectSp.onchange = onBanhNenChange;
        }

        const rTang1 = document.getElementById('radio-tang-1');
        const rTang2 = document.getElementById('radio-tang-2');
        if (rTang1) rTang1.onchange = () => { if (rTang1.checked) onSoTangChange(1); };
        if (rTang2) rTang2.onchange = () => { if (rTang2.checked) onSoTangChange(2); };

        // Đồng bộ click card bánh nền với select
        document.querySelectorAll('.card-banh-nen').forEach(card => {
            card.onclick = function () {
                const id = this.getAttribute('data-id');
                if (selectSp) {
                    selectSp.value = id;
                    onBanhNenChange();
                }
            };
            card.onkeydown = function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.click();
                }
            };
        });

        const btnUndo = document.getElementById('btn-tool-undo');
        if (btnUndo) btnUndo.onclick = () => LichSuCanvas.hoanTac();

        const btnRedo = document.getElementById('btn-tool-redo');
        if (btnRedo) btnRedo.onclick = () => LichSuCanvas.lamLai();

        const btnXoaChon = document.getElementById('btn-xoa-chon');
        if (btnXoaChon) btnXoaChon.onclick = xoaPhanTuChon;

        const btnXoaAll = document.getElementById('btn-xoa-tat-ca');
        if (btnXoaAll) btnXoaAll.onclick = lamMoiCanvas;

        const inputChu = document.getElementById('input-chu-canvas');
        const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');

        const charCounter = document.getElementById('char-counter');
        function updateCharCounter(val) {
            if (charCounter) {
                const len = (val || '').length;
                charCounter.textContent = `${len}/30 ký tự`;
                charCounter.style.color = len >= 30 ? 'var(--dat-danger)' : 'var(--dat-text-muted)';
            }
        }

        if (inputChu) {
            updateCharCounter(inputChu.value);
            inputChu.addEventListener('input', function () {
                const val = this.value;
                updateCharCounter(val);
                if (formChu && formChu.value !== val) formChu.value = val;
                capNhatChuCanvas(val);
                if (currentStep === 5) capNhatBangTomTat();
            });
        }

        // Kích hoạt các nút gợi ý câu chúc nhanh
        document.querySelectorAll('.btn-suggestion-chip').forEach(btn => {
            btn.addEventListener('click', function () {
                const txt = this.getAttribute('data-text') || '';
                if (inputChu) {
                    inputChu.value = txt;
                    inputChu.dispatchEvent(new Event('input', { bubbles: true }));
                }
            });
        });

        if (formChu) {
            formChu.addEventListener('input', function () {
                const val = this.value;
                if (inputChu && inputChu.value !== val) inputChu.value = val;
                capNhatChuCanvas(val);
                if (currentStep === 5) capNhatBangTomTat();
            });
        }

        const selectFont = document.getElementById('select-font-chu');
        if (selectFont) {
            selectFont.onchange = function () {
                currentFontChu = this.value;
                if (textObj) {
                    textObj.set('fontFamily', currentFontChu);
                    canvas.renderAll();
                }
                luuNhapDebounced();
                LichSuCanvas.ghiLichSu(0);
            };
        }

        // Lắng nghe 4 trường thông tin khách hàng ở bước 5 để tự động lưu vào bản nháp
        const inputHoTen = document.querySelector('input[name="Input.HoTen"]');
        const inputSdt = document.querySelector('input[name="Input.SoDienThoai"]');
        const inputNgay = document.querySelector('input[name="Input.NgayGioLay"]');
        const inputLoiNhan = document.querySelector('textarea[name="Input.LoiNhan"]');
        [inputHoTen, inputSdt, inputNgay, inputLoiNhan].forEach(inp => {
            if (inp) {
                inp.addEventListener('input', luuNhapDebounced);
                inp.addEventListener('change', luuNhapDebounced);
            }
        });

        const cbInAn = document.getElementById('checkbox-in-an');
        if (cbInAn) {
            cbInAn.onchange = onInAnToggle;
        }

        const inputTaiAnhIn = document.getElementById('input-tai-anh-in');
        if (inputTaiAnhIn) {
            inputTaiAnhIn.onchange = onTaiAnhInChange;
        }

        const btnGiamNen = document.getElementById('btn-giam-nen');
        const btnTangNen = document.getElementById('btn-tang-nen');
        const inputSoNen = document.getElementById('input-so-nen');

        if (btnGiamNen) {
            btnGiamNen.onclick = () => datSoNen(soLuongNen - 1);
        }
        if (btnTangNen) {
            btnTangNen.onclick = () => datSoNen(soLuongNen + 1);
        }
        if (inputSoNen) {
            inputSoNen.oninput = () => datSoNen(inputSoNen.value, false);
            inputSoNen.onchange = () => datSoNen(inputSoNen.value, true);
        }

        const btnVeTay = document.getElementById('btn-ve-tay');
        if (btnVeTay) {
            btnVeTay.onclick = toggleVeTay;
        }

        const form = document.getElementById('form-thiet-ke');
        if (form) {
            form.onsubmit = onFormSubmit;
        }

        // Nút xếp viền dâu tự động
        const btnArrange1 = document.getElementById('btn-arrange-rim');
        const btnArrange2 = document.getElementById('btn-tool-arrange-rim');
        if (btnArrange1) btnArrange1.onclick = sapXepVienDauTuDong;
        if (btnArrange2) btnArrange2.onclick = sapXepVienDauTuDong;

        // Nút dọn sạch topping
        const btnClearTop = document.getElementById('btn-clear-toppings');
        if (btnClearTop) btnClearTop.onclick = xoaTatCaTopping;

        // Toolbar chọn kiểu dâu tây
        document.querySelectorAll('#group-strawberry-style button').forEach(btn => {
            btn.onclick = function () {
                const style = this.getAttribute('data-style');
                doiKieuDau(style);
            };
        });

        // Nút đổi kiểu dâu nhanh trên header canvas
        const btnToolBerry = document.getElementById('btn-tool-berry-style');
        if (btnToolBerry) {
            btnToolBerry.onclick = function () {
                const styles = ['nguyen_qua', 'bo_doi', 'cat_lat'];
                const nextIdx = (styles.indexOf(currentStrawberryStyle) + 1) % styles.length;
                doiKieuDau(styles[nextIdx]);
            };
        }
    }

    function sapXepVienDauTuDong() {
        if (!canvas) return;
        const dauTp = thanhPhans.find(t => t.nhom === 'TOPPING' && t.ten && t.ten.toLowerCase().includes('dâu'));
        let currentBerries = canvas.getObjects().filter(o => o.customType === 'topping');

        if (currentBerries.length === 0 && dauTp) {
            const countToAdd = Math.min(8, dauTp.toiDa, MAX_TOPPING_CAKE);
            for (let i = 0; i < countToAdd; i++) {
                themToppingVaoCanvas(dauTp, 200, 200, false);
            }
            currentBerries = canvas.getObjects().filter(o => o.customType === 'topping');
        }

        if (currentBerries.length === 0) {
            hienToast('Chưa có topping dâu tây để sắp xếp viền.', 'info');
            return;
        }

        const count = currentBerries.length;
        const ringRadius = 118;
        for (let i = 0; i < count; i++) {
            const angleRad = (i / count) * Math.PI * 2 - Math.PI / 2;
            const x = Math.round(200 + ringRadius * Math.cos(angleRad));
            const y = Math.round(200 + ringRadius * Math.sin(angleRad));
            const angleDeg = Math.round((angleRad * 180 / Math.PI) + 90);

            currentBerries[i].set({
                left: x,
                top: y,
                angle: angleDeg
            });
            currentBerries[i].setCoords();
        }

        canvas.renderAll();
        capNhatToppingsTuCanvas();
        LichSuCanvas.ghiLichSu(50);
        hienToast(`Đã sắp xếp đều ${count} quả dâu tây viền tròn quanh bánh!`, 'success');
        if (currentStep === 5) capNhatBangTomTat();
    }

    function doiKieuDau(newStyle) {
        if (!newStyle || !STRAWBERRY_FILES[newStyle]) return;
        currentStrawberryStyle = newStyle;

        document.querySelectorAll('#group-strawberry-style button').forEach(b => {
            if (b.getAttribute('data-style') === newStyle) b.classList.add('active');
            else b.classList.remove('active');
        });

        const active = canvas ? canvas.getActiveObject() : null;
        if (active && active.customType === 'topping') {
            const fileName = STRAWBERRY_FILES[newStyle];
            let imgEl = toppingImageCache.get(fileName);
            if (imgEl && imgEl.complete) {
                active.setElement(imgEl);
                active.strawberryStyle = newStyle;
                active.setCoords();
                canvas.renderAll();
                LichSuCanvas.ghiLichSu(0);
                hienToast('Đã đổi kiểu dáng quả dâu đang chọn!', 'success');
            }
        } else {
            const label = newStyle === 'nguyen_qua' ? 'Trái nguyên' : newStyle === 'bo_doi' ? 'Bổ đôi' : 'Cắt lát';
            hienToast(`Đã chọn kiểu dâu: ${label}`, 'info');
        }
    }

    function xoaTatCaTopping() {
        if (!canvas) return;
        const toppings = canvas.getObjects().filter(o => o.customType === 'topping');
        if (toppings.length === 0) {
            hienToast('Chưa có topping nào trên bánh để xóa.', 'info');
            return;
        }
        if (!confirm('Bạn có muốn xóa toàn bộ topping trên mặt bánh?')) return;

        canvas.discardActiveObject();
        toppings.forEach(o => canvas.remove(o));
        pendingPositions = [];
        pendingToppings.clear();
        toppingsMap.clear();
        updateToppingBadges();
        canvas.renderAll();
        capNhatToppingsTuCanvas();
        hienToast('Đã dọn sạch topping trên mặt bánh!', 'info');
        if (currentStep === 5) capNhatBangTomTat();
    }

    function capNhatLayerExplorer() {
        let cotTen = 'Vani xốp mềm';
        let cotColor = '#FBE7C6';
        if (selectedCotId) {
            const cot = thanhPhans.find(t => t.id === selectedCotId);
            if (cot) {
                cotTen = cot.ten;
                const lower = cot.ten.toLowerCase();
                if (lower.includes('socola')) cotColor = '#4E342E';
                else if (lower.includes('matcha') || lower.includes('trà xanh')) cotColor = '#689F38';
                else if (lower.includes('bắp') || lower.includes('ngô')) cotColor = '#FBC02D';
                else if (lower.includes('dâu')) cotColor = '#F48FB1';
            }
        }

        const badgeFlavor = document.getElementById('layer-badge-flavor');
        if (badgeFlavor) badgeFlavor.textContent = cotTen;
        const legendSponge = document.getElementById('legend-text-sponge');
        if (legendSponge) legendSponge.textContent = cotTen;
        const dotSponge = document.getElementById('legend-dot-sponge');
        if (dotSponge) dotSponge.style.backgroundColor = cotColor;

        ['cut-sponge-1', 'cut-sponge-2', 'cut-sponge-3'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.setAttribute('fill', cotColor);
        });

        const kemObj = MAU_KEM.find(m => m.hex.toLowerCase() === currentKemColor.toLowerCase());
        const tenKem = kemObj ? kemObj.ten : 'Trắng sữa';
        const legendFrosting = document.getElementById('legend-text-frosting');
        if (legendFrosting) legendFrosting.textContent = tenKem;
        const dotFrosting = document.getElementById('legend-dot-frosting');
        if (dotFrosting) dotFrosting.style.backgroundColor = currentKemColor;

        ['cut-frosting-top', 'cut-frosting-left', 'cut-frosting-right'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.setAttribute('fill', currentKemColor);
        });
    }

    function onBanhNenChange() {
        const selectSp = document.getElementById('select-banh-nen');
        const val = selectSp ? selectSp.value : null;
        selectedSanPhamNenId = val ? parseInt(val, 10) : null;

        // Cập nhật kích thước trên mô hình 3D
        if (window.Cake3D && typeof window.Cake3D.setCakeDimensions === 'function') {
            const cm = layDuongKinhBanhNen();
            if (cm) window.Cake3D.setCakeDimensions(cm);
        }

        // Cập nhật active card trong grid bánh nền
        document.querySelectorAll('.card-banh-nen').forEach(c => {
            const id = parseInt(c.getAttribute('data-id'), 10);
            c.classList.toggle('active', id === selectedSanPhamNenId);
            c.setAttribute('aria-pressed', String(id === selectedSanPhamNenId));
        });

        // Bỏ cốt và viền không hợp kích thước
        if (selectedCotId) {
            const tp = thanhPhans.find(t => t.id === selectedCotId);
            if (tp && tp.sanPhamNenId && tp.sanPhamNenId !== selectedSanPhamNenId) {
                selectedCotId = null;
            }
        }
        if (selectedVienId) {
            const tp = thanhPhans.find(t => t.id === selectedVienId);
            if (tp && tp.sanPhamNenId && tp.sanPhamNenId !== selectedSanPhamNenId) {
                selectedVienId = null;
                updateCakeVien();
            }
        }

        // Cập nhật tùy chọn 2 tầng theo cỡ bánh
        const cmKichThuoc = layDuongKinhBanhNen();
        const radioTang2 = document.getElementById('radio-tang-2');
        const labelTang2 = document.getElementById('label-tang-2');
        const tangTp = thanhPhans.find(t => t.nhom === 'TANG' && t.sanPhamNenId === selectedSanPhamNenId);

        if (cmKichThuoc && cmKichThuoc >= 20 && tangTp) {
            if (radioTang2) radioTang2.disabled = false;
            if (labelTang2) labelTang2.textContent = `2 tầng (+${(tangTp.gia / 1000).toLocaleString('vi-VN')}k)`;
            if (soTangState === 2) {
                selectedTangId = tangTp.id;
            }
        } else {
            if (radioTang2) {
                radioTang2.disabled = true;
                radioTang2.checked = false;
            }
            const radioTang1 = document.getElementById('radio-tang-1');
            if (radioTang1) radioTang1.checked = true;
            if (labelTang2) labelTang2.textContent = '2 tầng (từ cỡ 20cm)';
            if (soTangState === 2) {
                soTangState = 1;
                selectedTangId = null;
            }
        }

        capNhatHienThiSoTangUI();
        drawCakeBase();
        updateCakeVien();

        renderNhomCot();
        renderNhomVien();
        renderNhomTopping();
        capNhatGiaDebounced();
        if (currentStep === 5) capNhatBangTomTat();
    }

    function renderNhomCot() {
        const container = document.getElementById('container-cot');
        if (!container) return;
        container.innerHTML = '';

        if (!selectedSanPhamNenId) {
            container.innerHTML = '<p class="dat-empty">Chọn cỡ bánh ở bước 1 để xem các loại cốt.</p>';
            return;
        }

        const cots = thanhPhans.filter(t => t.nhom === 'COT' && (!t.sanPhamNenId || t.sanPhamNenId === selectedSanPhamNenId));
        if (cots.length === 0) {
            container.innerHTML = '<p class="dat-empty">Không có cốt bánh phù hợp cho cỡ này.</p>';
            return;
        }

        // Tự động chọn cốt đầu tiên nếu chưa chọn
        if (!selectedCotId && cots.length > 0) {
            selectedCotId = cots[0].id;
        }

        cots.forEach(c => {
            const isSelected = selectedCotId === c.id;
            const col = document.createElement('div');
            col.className = 'dat-cell';
            col.innerHTML = `
                <div class="dat-option-card ${isSelected ? 'active' : ''}" tabindex="0" role="button" aria-pressed="${isSelected}" aria-label="Cốt ${esc(c.ten)}">
                    <span class="dat-option-title">${esc(c.ten)}</span>
                    <span class="dat-option-price">+${c.gia.toLocaleString('vi-VN')} ₫</span>
                </div>
            `;
            const cardEl = col.firstElementChild;
            cardEl.onclick = () => {
                selectedCotId = c.id;
                renderNhomCot();
                capNhatLayerExplorer();
                capNhatGiaDebounced();
                if (window.Cake3D && typeof window.Cake3D.setCotFlavor === 'function') {
                    window.Cake3D.setCotFlavor(c.ten);
                }
                if (currentStep === 5) capNhatBangTomTat();
            };
            cardEl.onkeydown = (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    cardEl.click();
                }
            };
            container.appendChild(col);
        });
    }

    function renderNhomVien() {
        const container = document.getElementById('container-vien');
        if (!container) return;
        container.innerHTML = '';

        const viens = thanhPhans.filter(t => t.nhom === 'VIEN' && (!t.sanPhamNenId || t.sanPhamNenId === selectedSanPhamNenId));

        // Nút Không chọn viền
        const colKhong = document.createElement('div');
        colKhong.className = 'dat-cell';
        const isNone = selectedVienId === null;
        colKhong.innerHTML = `
            <div class="dat-option-card ${isNone ? 'active' : ''}" tabindex="0" role="button" aria-pressed="${isNone}" aria-label="Không chọn viền">
                <span class="dat-option-title">Không viền</span>
                <span class="dat-option-price">0 ₫</span>
            </div>
        `;
        const cardKhong = colKhong.firstElementChild;
        cardKhong.onclick = () => {
            selectedVienId = null;
            updateCakeVien();
            renderNhomVien();
            capNhatGiaDebounced();
            if (window.Cake3D && typeof window.Cake3D.setRimStyle === 'function') {
                window.Cake3D.setRimStyle(false);
            }
            if (currentStep === 5) capNhatBangTomTat();
        };
        cardKhong.onkeydown = (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                cardKhong.click();
            }
        };
        container.appendChild(colKhong);

        viens.forEach(v => {
            const isSelected = selectedVienId === v.id;
            const col = document.createElement('div');
            col.className = 'dat-cell';
            col.innerHTML = `
                <div class="dat-option-card ${isSelected ? 'active' : ''}" tabindex="0" role="button" aria-pressed="${isSelected}" aria-label="Viền ${esc(v.ten)}">
                    <span class="dat-option-title">${esc(v.ten)}</span>
                    <span class="dat-option-price">+${v.gia.toLocaleString('vi-VN')} ₫</span>
                </div>
            `;
            const cardEl = col.firstElementChild;
            cardEl.onclick = () => {
                selectedVienId = v.id;
                updateCakeVien();
                renderNhomVien();
                capNhatGiaDebounced();
                if (window.Cake3D && typeof window.Cake3D.setRimStyle === 'function') {
                    window.Cake3D.setRimStyle(true);
                }
                if (currentStep === 5) capNhatBangTomTat();
            };
            cardEl.onkeydown = (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    cardEl.click();
                }
            };
            container.appendChild(col);
        });
    }

    function renderNhomTopping() {
        const container = document.getElementById('container-topping');
        if (!container) return;
        container.innerHTML = '';

        const toppings = thanhPhans.filter(t => t.nhom === 'TOPPING');
        if (toppings.length === 0) {
            container.innerHTML = '<p class="dat-empty">Chưa có topping nào.</p>';
            return;
        }

        toppings.forEach(tp => {
            const count = toppingsMap.get(tp.id) || 0;
            const col = document.createElement('div');
            col.className = 'dat-cell';
            const imgPath = tp.anh ? `/img/thanh-phan/${tp.anh}` : '';
            col.innerHTML = `
                <div class="dat-topping-item topping-card" data-id="${tp.id}">
                    ${imgPath ? `<img src="${imgPath}" alt="" class="dat-topping-thumb" />` : '<span class="dat-topping-thumb"></span>'}
                    <div class="dat-topping-info">
                        <div class="dat-topping-name" title="${esc(tp.ten)}">${esc(tp.ten)}</div>
                        <div class="dat-topping-price">+${tp.gia.toLocaleString('vi-VN')} ₫</div>
                    </div>
                    <span class="dat-topping-counter badge-count${count > 0 ? ' has' : ''}">${count}/${tp.toiDa}</span>
                    <button type="button" class="dat-btn dat-btn-secondary dat-btn-add-topping btn-add-topping" aria-label="Thêm ${esc(tp.ten)}">Thêm</button>
                </div>
            `;
            const btnAdd = col.querySelector('.btn-add-topping');
            btnAdd.onclick = () => {
                themToppingVaoCanvas(tp);
            };
            container.appendChild(col);
        });
    }

    function updateToppingBadges() {
        document.querySelectorAll('.topping-card').forEach(card => {
            const id = parseInt(card.getAttribute('data-id'), 10);
            const tp = thanhPhans.find(t => t.id === id);
            if (tp) {
                const count = toppingsMap.get(id) || 0;
                const badge = card.querySelector('.badge-count');
                if (badge) {
                    badge.textContent = `${count}/${tp.toiDa}`;
                    badge.classList.toggle('has', count > 0);
                }
            }
        });
    }

    function themToppingVaoCanvas(tp, left = null, top = null, selectAfterAdd = true) {
        if (!canvas) return;
        const currentCount = demToppingTongCong(tp.id);
        if (currentCount >= tp.toiDa) {
            hienToast(`Đã đạt số lượng tối đa (${tp.toiDa}) cho thành phần "${tp.ten}".`, 'warning');
            return;
        }
        if (demTongToppingTatCa() >= MAX_TOPPING_CAKE) {
            hienToast(`Đã đạt số lượng tối đa (${MAX_TOPPING_CAKE}) phụ kiện trang trí trên bánh.`, 'warning');
            return;
        }

        let posX = left;
        let posY = top;
        let isAutoPos = false;
        if (posX == null || posY == null) {
            const viTri = timViTriToppingMoi();
            posX = viTri.x;
            posY = viTri.y;
            isAutoPos = true;
            pendingPositions.push({ x: posX, y: posY });
        }

        function xoaPendingPos() {
            if (isAutoPos) {
                const idx = pendingPositions.findIndex(p => p.x === posX && p.y === posY);
                if (idx >= 0) pendingPositions.splice(idx, 1);
            }
        }

        const isStrawberry = tp.nhom === 'TOPPING' && tp.ten && tp.ten.toLowerCase().includes('dâu');
        const imgKey = isStrawberry
            ? STRAWBERRY_FILES[currentStrawberryStyle] || tp.anh || 'dau.svg'
            : tp.anh;
        let imgEl = imgKey ? toppingImageCache.get(imgKey) : null;
        if (imgKey && !imgEl) {
            imgEl = new Image();
            imgEl.crossOrigin = 'anonymous';
            imgEl.src = layDuongDanTopping(imgKey);
            toppingImageCache.set(imgKey, imgEl);
        }

        function themVoiHieuUngNay(targetObj) {
            const targetScaleX = targetObj.scaleX;
            const targetScaleY = targetObj.scaleY;
            targetObj.set({
                scaleX: targetScaleX * 1.35,
                scaleY: targetScaleY * 1.35,
                opacity: 0.8
            });
            canvas.add(targetObj);
            sapXepCacLopCanvas();
            if (selectAfterAdd) canvas.setActiveObject(targetObj);
            targetObj.animate({
                scaleX: targetScaleX,
                scaleY: targetScaleY,
                opacity: 1
            }, {
                duration: 220,
                onChange: function () {
                    canvas.renderAll();
                },
                easing: fabric.util.ease.easeOutBack
            });
            capNhatToppingsTuCanvas();
        }

        if (imgEl && imgEl.complete && (imgEl.naturalWidth > 0 || imgEl.width > 0)) {
            // Đã tải xong -> thêm ngay lập tức đồng bộ với hiệu ứng nảy
            xoaPendingPos();
            const obj = taoDoiTuongTopping(tp, posX, posY, imgEl);
            themVoiHieuUngNay(obj);
        } else if (imgKey && imgEl) {
            // Ảnh đang tải -> đánh dấu pending và thêm khi tải xong
            pendingToppings.set(tp.id, (pendingToppings.get(tp.id) || 0) + 1);
            capNhatToppingsTuCanvas();

            const onDone = function () {
                imgEl.removeEventListener('load', onDone);
                imgEl.removeEventListener('error', onDone);
                xoaPendingPos();
                pendingToppings.set(tp.id, Math.max(0, (pendingToppings.get(tp.id) || 1) - 1));
                const obj = taoDoiTuongTopping(tp, posX, posY, (imgEl.naturalWidth > 0 || imgEl.width > 0) ? imgEl : null);
                themVoiHieuUngNay(obj);
            };

            imgEl.addEventListener('load', onDone);
            imgEl.addEventListener('error', onDone);
        } else {
            xoaPendingPos();
            const obj = taoDoiTuongTopping(tp, posX, posY, null);
            themVoiHieuUngNay(obj);
        }
    }

    function themToppingDangHinh(tp, posX, posY, selectAfterAdd = true) {
        const group = taoDoiTuongTopping(tp, posX, posY, null);
        canvas.add(group);
        sapXepCacLopCanvas();
        if (selectAfterAdd) canvas.setActiveObject(group);
        canvas.renderAll();
        capNhatToppingsTuCanvas();
    }

    function ganSuKienTextObj(tObj) {
        if (!tObj) return;
        tObj.on('changed', function () {
            const txt = tObj.text || '';
            const inputChu = document.getElementById('input-chu-canvas');
            const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
            if (inputChu && inputChu.value !== txt) inputChu.value = txt;
            if (formChu && formChu.value !== txt) formChu.value = txt;
            if (currentStep === 5) capNhatBangTomTat();
        });
    }

    function capNhatChuCanvas(rawVal) {
        const val = (rawVal || '').trim();
        const inputChu = document.getElementById('input-chu-canvas');
        const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
        if (inputChu && inputChu.value !== (rawVal || '')) inputChu.value = rawVal || '';
        if (formChu && formChu.value !== (rawVal || '')) formChu.value = rawVal || '';

        if (!canvas) return;

        if (!val) {
            if (textObj) {
                canvas.remove(textObj);
                textObj = null;
                canvas.renderAll();
            }
            if (currentStep === 5) capNhatBangTomTat();
            return;
        }

        if (textObj) {
            textObj.set({
                text: val,
                fontFamily: currentFontChu,
                fill: currentMauChu,
                shadow: new fabric.Shadow({
                    color: 'rgba(45, 18, 12, 0.32)',
                    blur: 3.5,
                    offsetX: 1,
                    offsetY: 2
                })
            });
        } else {
            textObj = new fabric.IText(val, {
                left: 200,
                top: 280,
                originX: 'center',
                originY: 'center',
                fontSize: 22,
                fontFamily: currentFontChu,
                fill: currentMauChu,
                cornerSize: 8,
                cornerColor: '#FFFFFF',
                cornerStrokeColor: '#3A1B26',
                cornerStyle: 'circle',
                transparentCorners: false,
                borderColor: '#3A1B26',
                borderDashArray: [4, 4],
                padding: 6,
                shadow: new fabric.Shadow({
                    color: 'rgba(45, 18, 12, 0.32)',
                    blur: 3.5,
                    offsetX: 1,
                    offsetY: 2
                }),
                customType: 'chu'
            });
            ganSuKienTextObj(textObj);
            canvas.add(textObj);
        }
        sapXepCacLopCanvas();
        canvas.renderAll();
        if (currentStep === 5) capNhatBangTomTat();
        LichSuCanvas.ghiLichSu(250);
    }

    function onInAnToggle() {
        const cb = document.getElementById('checkbox-in-an');
        const box = document.getElementById('khu-vuc-in-an');
        const inAnTp = thanhPhans.find(t => t.nhom === 'IN_AN');

        if (cb && cb.checked && inAnTp) {
            selectedInAnId = inAnTp.id;
            if (box) box.classList.remove('d-none');
            hienThiVungIn(true);
        } else {
            selectedInAnId = null;
            if (box) box.classList.add('d-none');
            hienThiVungIn(false);
            if (isDrawingMode) toggleVeTay();
        }
        capNhatGiaDebounced();
        if (currentStep === 5) capNhatBangTomTat();
    }

    function hienThiVungIn(hien) {
        if (!canvas) return;
        if (hien) {
            if (!printAreaBorder) {
                printAreaBorder = new fabric.Rect({
                    left: 200,
                    top: 200,
                    width: 150,
                    height: 150,
                    originX: 'center',
                    originY: 'center',
                    rx: 6,
                    ry: 6,
                    fill: 'rgba(255, 255, 248, 0.65)',
                    stroke: '#9E8A78',
                    strokeDashArray: [6, 4],
                    strokeWidth: 1.5,
                    shadow: new fabric.Shadow({
                        color: 'rgba(40, 20, 10, 0.16)',
                        blur: 6,
                        offsetX: 0,
                        offsetY: 2
                    }),
                    selectable: false,
                    evented: false,
                    customType: 'in_an_border'
                });
                canvas.add(printAreaBorder);
                sapXepCacLopCanvas();
            }
        } else {
            if (printAreaBorder) {
                canvas.remove(printAreaBorder);
                printAreaBorder = null;
            }
            if (printImageObj) {
                canvas.remove(printImageObj);
                printImageObj = null;
            }
            uploadedImageFile = null;
        }
        canvas.renderAll();
    }

    function onTaiAnhInChange(e) {
        const file = e.target.files && e.target.files[0];
        if (!file || !canvas) return;

        uploadedImageFile = file;
        const reader = new FileReader();
        reader.onload = function (ev) {
            fabric.Image.fromURL(ev.target.result, function (img) {
                if (printImageObj) canvas.remove(printImageObj);

                const maxDim = 150;
                const scale = Math.max(maxDim / img.width, maxDim / img.height);
                img.set({
                    left: 200,
                    top: 200,
                    originX: 'center',
                    originY: 'center',
                    scaleX: scale,
                    scaleY: scale,
                    clipPath: printAreaClip,
                    cornerSize: 6,
                    customType: 'print_image'
                });
                printImageObj = img;
                canvas.add(img);
                sapXepCacLopCanvas();
                canvas.setActiveObject(img);
                canvas.renderAll();
                hienToast('Đã nạp ảnh vào vùng in thành công!', 'success');
                if (currentStep === 5) capNhatBangTomTat();
            });
        };
        reader.readAsDataURL(file);
    }

    function toggleVeTay() {
        if (!canvas) return;
        isDrawingMode = !isDrawingMode;
        canvas.isDrawingMode = isDrawingMode;

        const btn = document.getElementById('btn-ve-tay');
        if (btn) {
            btn.textContent = isDrawingMode ? 'Bút vẽ tay: bật' : 'Bút vẽ tay: tắt';
            btn.setAttribute('aria-pressed', String(isDrawingMode));
            btn.className = `dat-btn ${isDrawingMode ? 'dat-btn-primary' : 'dat-btn-secondary'}`;
        }

        if (isDrawingMode) {
            canvas.freeDrawingBrush.width = 4;
            canvas.freeDrawingBrush.color = '#333333';
            canvas.freeDrawingBrush.shadow = new fabric.Shadow({
                color: 'rgba(50, 20, 15, 0.25)',
                blur: 2.5,
                offsetX: 0.8,
                offsetY: 1.5
            });
            canvas.on('path:created', function (e) {
                e.path.clipPath = printAreaClip;
                e.path.customType = 'in_an_draw';
                e.path.strokeLineCap = 'round';
                e.path.strokeLineJoin = 'round';
                sapXepCacLopCanvas();
                if (currentStep === 5) capNhatBangTomTat();
            });
        }
    }

    function xoaPhanTuChon() {
        if (!canvas) return;
        const active = canvas.getActiveObjects();
        if (active && active.length > 0) {
            canvas.discardActiveObject();
            active.forEach(obj => {
                if (obj.customType === 'print_image') {
                    printImageObj = null;
                    uploadedImageFile = null;
                }
                if (obj.customType === 'chu') {
                    textObj = null;
                    const inputChu = document.getElementById('input-chu-canvas');
                    if (inputChu) inputChu.value = '';
                    const hiddenChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
                    if (hiddenChu) hiddenChu.value = '';
                }
                canvas.remove(obj);
            });
            canvas.renderAll();
            capNhatToppingsTuCanvas();
            hienToast('Đã xóa phần tử được chọn', 'info');
        }
    }

    function lamMoiCanvas() {
        if (!confirm('Bạn có chắc muốn vẽ lại từ đầu? Mọi topping và hình vẽ sẽ bị xóa.')) return;
        if (!canvas) return;

        pendingPositions = [];
        pendingToppings.clear();
        toppingsMap.clear();
        updateToppingBadges();

        canvas.clear();
        drawCakeBase();
        updateCakeVien();
        if (selectedInAnId) hienThiVungIn(true);

        textObj = null;
        printImageObj = null;
        uploadedImageFile = null;

        const inputChu = document.getElementById('input-chu-canvas');
        if (inputChu) inputChu.value = '';
        const hiddenChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
        if (hiddenChu) hiddenChu.value = '';

        capNhatToppingsTuCanvas();
        hienToast('Đã làm mới bảng vẽ', 'info');
    }

    function napMau(mauObj) {
        if (!mauObj) return;

        // 1. Cốt
        if (mauObj.tenCot) {
            const cot = thanhPhans.find(t => t.nhom === 'COT' && t.ten.toLowerCase().includes(mauObj.tenCot.toLowerCase()) && (!t.sanPhamNenId || t.sanPhamNenId === selectedSanPhamNenId));
            if (cot) selectedCotId = cot.id;
        }

        // 2. Viền
        if (mauObj.tenVien) {
            const vien = thanhPhans.find(t => t.nhom === 'VIEN' && t.ten.toLowerCase().includes(mauObj.tenVien.toLowerCase()) && (!t.sanPhamNenId || t.sanPhamNenId === selectedSanPhamNenId));
            if (vien) selectedVienId = vien.id;
        } else {
            selectedVienId = null;
        }

        // 3. Màu kem
        if (mauObj.mauKem) {
            currentKemColor = mauObj.mauKem;
            if (cakeBaseObj) cakeBaseObj.set('fill', createCakeGradient(currentKemColor));
            const labelKem = document.getElementById('label-ten-mau-kem');
            const kemObj = MAU_KEM.find(m => m.hex.toLowerCase() === currentKemColor.toLowerCase());
            if (labelKem && kemObj) labelKem.textContent = kemObj.ten;
        }

        // 4. In ấn
        if (mauObj.inAn) {
            const cb = document.getElementById('checkbox-in-an');
            if (cb) {
                cb.checked = true;
                onInAnToggle();
            }
        }

        renderNhomCot();
        renderNhomVien();
        updateCakeVien();

        // 5. Toppings: dọn sạch topping cũ trên canvas nếu có trước khi nạp mẫu
        if (canvas) {
            canvas.discardActiveObject();
            canvas.getObjects().slice().forEach(o => {
                if (o.customType === 'topping') canvas.remove(o);
            });
        }
        pendingPositions = [];
        pendingToppings.clear();
        toppingsMap.clear();
        if (mauObj.toppings && Array.isArray(mauObj.toppings)) {
            let totalAdded = 0;
            mauObj.toppings.forEach(topReq => {
                const tp = thanhPhans.find(t => t.nhom === 'TOPPING' && t.ten.toLowerCase().includes(topReq.ten.toLowerCase()));
                if (tp) {
                    if (typeof topReq.x === 'number' && typeof topReq.y === 'number') {
                        if (totalAdded < MAX_TOPPING_CAKE) {
                            themToppingVaoCanvas(tp, topReq.x, topReq.y, false);
                            totalAdded++;
                        }
                    } else {
                        const count = topReq.soLuong || 1;
                        for (let i = 0; i < count && i < tp.toiDa && totalAdded < MAX_TOPPING_CAKE; i++) {
                            themToppingVaoCanvas(tp, null, null, false);
                            totalAdded++;
                        }
                    }
                }
            });
        }
        capNhatToppingsTuCanvas();

        // 6. Chữ
        if (mauObj.chu) {
            if (mauObj.fontChu) currentFontChu = mauObj.fontChu;
            if (mauObj.mauChu) currentMauChu = mauObj.mauChu;
            capNhatChuCanvas(mauObj.chu);
        }

        dongBoGiaoDienSwatches();
        sapXepCacLopCanvas();
        coGianCanvasTheoKhung();
        capNhatGiaDebounced();
        if (currentStep === 5) capNhatBangTomTat();
        if (window.Cake3D) {
            window.Cake3D.setCakeColor(currentKemColor);
            window.Cake3D.setRimStyle(selectedVienId !== null);
            if (mauObj.tenCot) window.Cake3D.setCotFlavor(mauObj.tenCot);
            window.Cake3D.syncFrom2D();
        }
    }

    function loadExistingDesign(jsonString) {
        try {
            const obj = JSON.parse(jsonString);
            if (!obj) return;

            if (obj.mauKem) currentKemColor = obj.mauKem;
            if (obj.fontChu) currentFontChu = obj.fontChu;
            if (obj.mauChu) currentMauChu = obj.mauChu;

            if (obj.sanPhamNenId) {
                const selectSp = document.getElementById('select-banh-nen');
                if (selectSp) selectSp.value = obj.sanPhamNenId;
                onBanhNenChange();
            }

            if (obj.soTang === 2 || (obj.thanhPhan && obj.thanhPhan.some(item => thanhPhans.some(t => t.id === item.id && t.nhom === 'TANG')))) {
                onSoTangChange(2);
            } else {
                onSoTangChange(1);
            }

            if (obj.thanhPhan && Array.isArray(obj.thanhPhan)) {
                obj.thanhPhan.forEach(item => {
                    const tp = thanhPhans.find(t => t.id === item.id);
                    if (!tp) return;
                    if (tp.nhom === 'COT') selectedCotId = tp.id;
                    if (tp.nhom === 'VIEN') selectedVienId = tp.id;
                    if (tp.nhom === 'IN_AN') {
                        selectedInAnId = tp.id;
                        const cb = document.getElementById('checkbox-in-an');
                        if (cb) { cb.checked = true; onInAnToggle(); }
                    }
                    if (tp.nhom === 'TOPPING') {
                        toppingsMap.set(tp.id, item.soLuong);
                    }
                });
                renderNhomCot();
                renderNhomVien();
                updateCakeVien();
                updateToppingBadges();
            }

            if (obj.canvas && canvas) {
                canvas.loadFromJSON(obj.canvas, () => {
                    // Định vị lại các đối tượng
                    canvas.getObjects().forEach(o => {
                        if (o.customType === 'base_circle') cakeBaseObj = o;
                        if (o.customType === 'tier2_circle') cakeTier2Obj = o;
                        if (o.customType === 'cake_vien') cakeVienObj = o;
                        if (o.customType === 'tier2_vien') cakeTier2VienObj = o;
                        if (o.customType === 'in_an_border') printAreaBorder = o;
                        if (o.customType === 'print_image') {
                            printImageObj = o;
                            o.clipPath = printAreaClip;
                        }
                        if (o.customType === 'in_an_draw') {
                            o.clipPath = printAreaClip;
                        }
                        if (o.customType === 'chu') {
                            textObj = o;
                            ganSuKienTextObj(textObj);
                            // Giữ font/màu chữ đã lưu (nếu không capNhatChuCanvas bên dưới sẽ ghi đè bằng giá trị mặc định)
                            if (o.fontFamily) currentFontChu = o.fontFamily;
                            if (typeof o.fill === 'string') currentMauChu = o.fill;
                        }
                        if (o.customType === 'base_circle') {
                            // Màu kem nằm ở điểm dừng 0.65 của gradient (xem createCakeGradient)
                            const stop = o.fill && o.fill.colorStops && o.fill.colorStops.find(c => c.offset === 0.65);
                            if (stop) currentKemColor = stop.color;
                        }
                        if (o.customType === 'topping') {
                            o.set({
                                lockScalingX: true,
                                lockScalingY: true
                            });
                            o.scaleToWidth(KICH_THUOC_TOPPING);
                            o.scaleToHeight(KICH_THUOC_TOPPING);
                            o.setCoords();
                        }
                    });
                    sapXepCacLopCanvas();
                    coGianCanvasTheoKhung();
                    canvas.renderAll();
                    capNhatToppingsTuCanvas();

                    // Đồng bộ chữ hiện có từ form (hoặc textObj)
                    const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
                    const inputChu = document.getElementById('input-chu-canvas');
                    const chuHienCo = (formChu?.value || textObj?.text || inputChu?.value || '').trim();
                    if (chuHienCo) {
                        capNhatChuCanvas(chuHienCo);
                    }
                    dongBoGiaoDienSwatches();
                    if (currentStep === 5) capNhatBangTomTat();
                    LichSuCanvas.init();
                });
            } else if (obj.thanhPhan) {
                // Nếu không có canvas JSON nhưng có danh sách thành phần, rải topping lên canvas
                obj.thanhPhan.forEach(item => {
                    const tp = thanhPhans.find(t => t.id === item.id);
                    if (tp && tp.nhom === 'TOPPING') {
                        for (let i = 0; i < item.soLuong && i < tp.toiDa; i++) {
                            themToppingVaoCanvas(tp, null, null, false);
                        }
                    }
                });
                capNhatToppingsTuCanvas();
            }

            if (typeof obj.soNen === 'number' || typeof obj.soLuongNen === 'number') {
                const n = typeof obj.soNen === 'number' ? obj.soNen : obj.soLuongNen;
                datSoNen(n);
            }

            dongBoGiaoDienSwatches();

            if (obj.thongTinKhach) {
                const inputHoTen = document.querySelector('input[name="Input.HoTen"]');
                const inputSdt = document.querySelector('input[name="Input.SoDienThoai"]');
                const inputNgay = document.querySelector('input[name="Input.NgayGioLay"]');
                const inputLoiNhan = document.querySelector('textarea[name="Input.LoiNhan"]');

                if (inputHoTen && obj.thongTinKhach.hoTen) inputHoTen.value = obj.thongTinKhach.hoTen;
                if (inputSdt && obj.thongTinKhach.soDienThoai) inputSdt.value = obj.thongTinKhach.soDienThoai;
                if (inputNgay && obj.thongTinKhach.ngayGioLay) inputNgay.value = obj.thongTinKhach.ngayGioLay;
                if (inputLoiNhan && obj.thongTinKhach.loiNhan) inputLoiNhan.value = obj.thongTinKhach.loiNhan;
            }

            capNhatGiaDebounced();
            if (window.Cake3D) {
                window.Cake3D.setCakeColor(currentKemColor);
                window.Cake3D.setRimStyle(selectedVienId !== null);
                if (selectedCotId) {
                    const cot = thanhPhans.find(t => t.id === selectedCotId);
                    if (cot) window.Cake3D.setCotFlavor(cot.ten);
                }
                window.Cake3D.syncFrom2D();
            }
        } catch (e) {
            console.error('Không nạp được thiết kế cũ:', e);
        }
    }

    function capNhatGiaDebounced() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(thucHienTinhGia, 400);
        luuNhapDebounced(); // đổi cỡ/cốt/viền/in ấn/topping đều đi qua đây
    }

    function thucHienTinhGia() {
        if (!selectedSanPhamNenId) {
            hienThiGia(0, 0, 0);
            return;
        }

        const thanhPhanList = [];
        if (selectedCotId) thanhPhanList.push({ id: selectedCotId, soLuong: 1 });
        if (selectedVienId) thanhPhanList.push({ id: selectedVienId, soLuong: 1 });
        if (selectedInAnId) thanhPhanList.push({ id: selectedInAnId, soLuong: 1 });
        if (soTangState === 2 && selectedTangId) thanhPhanList.push({ id: selectedTangId, soLuong: 1 });

        for (const [id, count] of toppingsMap.entries()) {
            if (count > 0) thanhPhanList.push({ id, soLuong: count });
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        const statusEl = document.getElementById('hien-thi-trang-thai-gia');
        if (statusEl) statusEl.textContent = 'Đang tính giá...';

        fetch('/api/dat/gia', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': token
            },
            body: JSON.stringify({
                sanPhamNenId: selectedSanPhamNenId,
                thanhPhan: thanhPhanList
            })
        })
        .then(res => {
            if (!res.ok) throw new Error('Chưa đủ thành phần hoặc lỗi');
            return res.json();
        })
        .then(data => {
            hienThiGia(data.giaNen, data.giaThanhPhan, data.giaTamTinh);
            if (statusEl) statusEl.textContent = 'Đã cập nhật giá mới nhất.';
        })
        .catch(() => {
            // Nếu thiếu cốt hoặc đang chọn dở thì tạm tính cục bộ
            const sp = sanPhams.find(s => s.id === selectedSanPhamNenId);
            let giaNen = sp ? sp.gia : 0;
            let giaTp = 0;
            thanhPhanList.forEach(t => {
                const item = thanhPhans.find(x => x.id === t.id);
                if (item) giaTp += item.gia * t.soLuong;
            });
            hienThiGia(giaNen, giaTp, giaNen + giaTp);
            if (statusEl) statusEl.textContent = selectedCotId ? 'Tạm tính' : 'Cần chọn cốt bánh';
        });
    }

    function hienThiGia(giaNen, giaTp, tong) {
        const elNen = document.getElementById('hien-thi-gia-nen');
        const elTp = document.getElementById('hien-thi-gia-tp');
        const elTong = document.getElementById('hien-thi-tong');
        const mobTong = document.getElementById('mobile-gia-tong');
        const sumTong = document.getElementById('summary-gia-tong');

        const formattedTong = Number(tong).toLocaleString('vi-VN') + ' ₫';

        if (elNen) elNen.textContent = Number(giaNen).toLocaleString('vi-VN') + ' ₫';
        if (elTp) elTp.textContent = Number(giaTp).toLocaleString('vi-VN') + ' ₫';
        if (elTong) elTong.textContent = formattedTong;
        if (mobTong) mobTong.textContent = formattedTong;
        if (sumTong) sumTong.textContent = formattedTong;
    }

    function onFormSubmit(e) {
        if (!selectedSanPhamNenId) {
            hienToast('Vui lòng chọn bánh nền trước khi lưu thiết kế.', 'warning');
            chuyenBuoc(1);
            e.preventDefault();
            return false;
        }

        if (!selectedCotId) {
            hienToast('Vui lòng chọn cốt bánh (bắt buộc).', 'warning');
            chuyenBuoc(2);
            e.preventDefault();
            return false;
        }

        if (!canvas) return;

        // Đảm bảo chữ trên form và chữ trên canvas đồng bộ hoàn toàn trước khi xuất ảnh
        const inputChu = document.getElementById('input-chu-canvas');
        const formChu = document.querySelector('input[name="Input.ChuTrenBanh"]');
        const chuCuoi = (inputChu?.value || formChu?.value || '').trim();
        if (formChu) formChu.value = chuCuoi;
        if (inputChu) inputChu.value = chuCuoi;
        if (chuCuoi) {
            if (textObj) {
                textObj.set({ text: chuCuoi, fontFamily: currentFontChu, fill: currentMauChu });
            } else {
                capNhatChuCanvas(chuCuoi);
            }
        } else if (textObj) {
            canvas.remove(textObj);
            textObj = null;
        }

        // Bỏ chọn trước khi xuất ảnh
        canvas.discardActiveObject().renderAll();

        const curZoom = canvas.getZoom();
        const curW = canvas.getWidth();
        const curH = canvas.getHeight();

        let dataUrl;
        try {
            // Tạm thời reset zoom và kích thước về chuẩn 400x400 để ảnh xuất ra và dữ liệu JSON luôn đạt chuẩn 400x400
            canvas.setZoom(1);
            canvas.setDimensions({ width: CHIEU_RONG_CHUAN, height: CHIEU_RONG_CHUAN });
            canvas.renderAll();

            // 1. Đóng gói DuLieuJson
            const duLieuObj = layDuLieuThietKe();

            const hiddenJson = document.getElementById('input-du-lieu-json');
            if (hiddenJson) hiddenJson.value = JSON.stringify(duLieuObj);

            // 2. Xuất canvas thành ảnh PNG Blob và gán vào input file AnhThietKe
            dataUrl = canvas.toDataURL({ format: 'png', quality: 1 });
        } finally {
            // Khôi phục lại kích thước hiển thị theo khung
            canvas.setDimensions({ width: curW, height: curH });
            canvas.setZoom(curZoom);
            canvas.renderAll();
        }

        const blob = dataUrlToBlob(dataUrl);

        const dtThietKe = new DataTransfer();
        dtThietKe.items.add(new File([blob], 'thietke.png', { type: 'image/png' }));
        const inputAnhThietKe = document.getElementById('input-anh-thiet-ke');
        if (inputAnhThietKe) inputAnhThietKe.files = dtThietKe.files;

        // 3. Nếu có ảnh in ăn được tải lên, gán vào input file AnhInAn
        if (uploadedImageFile) {
            const dtIn = new DataTransfer();
            dtIn.items.add(uploadedImageFile);
            const inputAnhIn = document.getElementById('input-anh-in');
            if (inputAnhIn) inputAnhIn.files = dtIn.files;
        }

        // Gửi thành công thì bản nháp không còn cần nữa
        daGuiForm = true;
        xoaNhap();
        return true;
    }

    // =========================================================================
    // TỰ LƯU BẢN NHÁP (localStorage) - dùng đúng định dạng DuLieuJson như khi gửi form
    // =========================================================================
    const KHOA_NHAP = 'dat-banh-nhap';
    let nhapTimer = null;
    let daTuongTac = false;
    let daGuiForm = false;

    function layDuLieuThietKe() {
        const thanhPhanList = [];
        if (selectedCotId) thanhPhanList.push({ id: selectedCotId, soLuong: 1 });
        if (selectedVienId) thanhPhanList.push({ id: selectedVienId, soLuong: 1 });
        if (selectedInAnId) thanhPhanList.push({ id: selectedInAnId, soLuong: 1 });
        if (soTangState === 2 && selectedTangId) thanhPhanList.push({ id: selectedTangId, soLuong: 1 });
        for (const [id, count] of toppingsMap.entries()) {
            if (count > 0) thanhPhanList.push({ id, soLuong: count });
        }

        const inputHoTen = document.querySelector('input[name="Input.HoTen"]');
        const inputSdt = document.querySelector('input[name="Input.SoDienThoai"]');
        const inputNgay = document.querySelector('input[name="Input.NgayGioLay"]');
        const inputLoiNhan = document.querySelector('textarea[name="Input.LoiNhan"]');

        const toppingsChiTiet = [];
        if (canvas) {
            canvas.getObjects().forEach(o => {
                if (o.customType === 'topping') {
                    toppingsChiTiet.push({
                        thanhPhanId: o.thanhPhanId,
                        left: Math.round(o.left),
                        top: Math.round(o.top),
                        angle: Math.round(o.angle || 0),
                        tang: o.tang || 1
                    });
                }
            });
        }

        return {
            sanPhamNenId: selectedSanPhamNenId,
            soTang: soTangState,
            thanhPhan: thanhPhanList,
            soNen: soLuongNen,
            mauKem: currentKemColor,
            fontChu: currentFontChu,
            mauChu: currentMauChu,
            thongTinKhach: {
                hoTen: inputHoTen?.value || '',
                soDienThoai: inputSdt?.value || '',
                ngayGioLay: inputNgay?.value || '',
                loiNhan: inputLoiNhan?.value || ''
            },
            toppings: toppingsChiTiet,
            canvas: canvas.toJSON(['customType', 'thanhPhanId', 'strawberryStyle', 'selectable', 'evented', 'lockScalingX', 'lockScalingY', 'tang'])
        };
    }

    function luuNhapDebounced() {
        if (!daTuongTac || daGuiForm || !canvas || !selectedSanPhamNenId) return;
        clearTimeout(nhapTimer);
        nhapTimer = setTimeout(() => {
            try {
                const d = layDuLieuThietKe();
                // Đánh dấu nếu có ảnh in để thông báo cho khách khi khôi phục
                const coAnhIn = selectedInAnId !== null || uploadedImageFile !== null || (d.canvas && Array.isArray(d.canvas.objects) && d.canvas.objects.some(o => o.customType === 'print_image'));
                d.coAnhIn = coAnhIn;
                // Ảnh in ăn được là dataURL rất to, không lưu vào nháp (khách phải tải lại ảnh)
                if (d.canvas && Array.isArray(d.canvas.objects)) {
                    d.canvas.objects = d.canvas.objects.filter(o => o.customType !== 'print_image');
                }
                localStorage.setItem(KHOA_NHAP, JSON.stringify(d));
            } catch (e) { /* private mode hoặc đầy bộ nhớ: bỏ qua, nháp chỉ là tiện ích */ }
        }, 800);
    }

    function xoaNhap() {
        clearTimeout(nhapTimer);
        try { localStorage.removeItem(KHOA_NHAP); } catch (e) { /* bỏ qua */ }
    }

    function hoiKhoiPhucNhap() {
        let raw = null;
        try { raw = localStorage.getItem(KHOA_NHAP); } catch (e) { return; }
        if (!raw) return;
        let parsed = null;
        try {
            parsed = JSON.parse(raw);
            if (!parsed.sanPhamNenId) throw new Error('nháp thiếu bánh nền');
        } catch (e) {
            xoaNhap(); // nháp hỏng
            return;
        }
        const container = document.getElementById('dat-toast-container');
        if (!container) return;

        const coAnhIn = !!parsed.coAnhIn;
        const t = document.createElement('div');
        t.className = 'dat-toast dat-toast-nhap';
        t.innerHTML = '<div>Bạn có bản thiết kế đang làm dở' +
            (coAnhIn ? ' <span class="dat-toast-note">(cần chọn lại ảnh in)</span>' : '') +
            '</div>' +
            '<div class="dat-toast-actions">' +
            '<button type="button" data-act="tiep">Tiếp tục</button>' +
            '<button type="button" data-act="bo">Bỏ</button>' +
            '</div>';
        t.addEventListener('click', ev => {
            const act = ev.target.dataset && ev.target.dataset.act;
            if (!act) return;
            if (act === 'tiep') {
                loadExistingDesign(raw);
                if (coAnhIn) {
                    hienToast('Đã khôi phục thiết kế. Lưu ý: Bạn cần chọn lại tệp ảnh in ăn được nếu muốn in ảnh.', 'warning');
                }
            } else {
                xoaNhap();
            }
            t.remove();
        });
        container.appendChild(t);
    }

    function dataUrlToBlob(dataUrl) {
        const parts = dataUrl.split(';base64,');
        const contentType = parts[0].split(':')[1];
        const raw = window.atob(parts[1]);
        const uInt8Array = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; ++i) {
            uInt8Array[i] = raw.charCodeAt(i);
        }
        return new Blob([uInt8Array], { type: contentType });
    }
    // Gọi init ở cuối IIFE: mọi biến let/const phía trên (vd daTuongTac) đã được khởi tạo,
    // tránh lỗi "Cannot access ... before initialization" khi script nạp sau DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
