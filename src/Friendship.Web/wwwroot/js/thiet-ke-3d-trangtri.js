// thiet-ke-3d-trangtri.js - Quản lý Topping và Nến 3D cho nongnongBakery
// Tích hợp với Three.js WebGL và Fabric.js 2D Canvas
// Tham khảo nến và ngọn lửa 3D: VIDAKHOSHPEY22/3D-Birthday-Cake (Giấy phép MIT)

(function () {
    'use strict';

    if (typeof THREE === 'undefined') {
        console.warn('Three.js chưa sẵn sàng cho mô-đun trang trí 3D.');
        return;
    }

    // Bộ nhớ đệm danh mục thành phần
    let catalogsCache = null;
    let offscreenCanvas = null;
    let offscreenCtx = null;
    let offscreenCanvasTang2 = null;
    let offscreenCtxTang2 = null;
    let isSyncing = false; // Tránh đệ quy vô hạn khi gọi renderAll()

    // Màu hex là sRGB; renderer xuất sRGB + tone mapping nên phải đổi sang tuyến tính,
    // nếu không topping bị bạc màu (giống datMauKem trong thiet-ke-3d.js)
    function mau(hex) {
        return new THREE.Color(hex).convertSRGBToLinear();
    }

    /**
     * Lấy danh mục thành phần từ DOM (#du-lieu-danh-muc)
     */
    function layDanhSachThanhPhan() {
        if (catalogsCache) return catalogsCache;
        const el = document.getElementById('du-lieu-danh-muc');
        if (el && el.textContent) {
            try {
                const data = JSON.parse(el.textContent);
                catalogsCache = data.thanhPhans || [];
            } catch (e) {
                console.error('Lỗi phân tích danh mục thành phần 3D:', e);
                catalogsCache = [];
            }
        } else {
            catalogsCache = [];
        }
        return catalogsCache;
    }

    /**
     * Xác định loại topping từ đối tượng Fabric hoặc ID thành phần
     * Trả về: 'dau', 'cherry', 'macaron', 'socola'
     */
    function xacDinhLoaiTopping(t) {
        if (!t) return 'dau';
        if (typeof t === 'string') return t;
        if (t.loaiTopping) return t.loaiTopping;
        if (t.toppingType) return t.toppingType;

        const tpId = t.thanhPhanId || t;
        const danhSach = layDanhSachThanhPhan();
        const tp = danhSach.find(item => item.id === tpId);
        if (tp) {
            const ten = (tp.ten || '').toLowerCase();
            const anh = (tp.anh || '').toLowerCase();
            if (ten.includes('cherry') || anh.includes('cherry')) return 'cherry';
            if (ten.includes('macaron') || anh.includes('macaron')) return 'macaron';
            if (ten.includes('socola') || anh.includes('socola')) return 'socola';
        }

        const objTen = (t.thanhPhanTen || '').toLowerCase();
        if (objTen.includes('cherry')) return 'cherry';
        if (objTen.includes('macaron')) return 'macaron';
        if (objTen.includes('socola')) return 'socola';

        return 'dau';
    }

    /**
     * Tạo khối 3D Quả dâu tây:
     * Hình giọt (LatheGeometry), màu đỏ, có đài lá xanh ở cuống
     */
    function taoKhoi3DDauTay() {
        const group = new THREE.Group();

        // 1. Thân quả dâu hình giọt (LatheGeometry xoay quanh trục Y)
        const scale = 1.95;
        const pts = [
            new THREE.Vector2(0.003 * scale, 0.0),
            new THREE.Vector2(0.026 * scale, 0.02 * scale),
            new THREE.Vector2(0.065 * scale, 0.06 * scale),
            new THREE.Vector2(0.095 * scale, 0.11 * scale),
            new THREE.Vector2(0.098 * scale, 0.14 * scale),
            new THREE.Vector2(0.082 * scale, 0.18 * scale),
            new THREE.Vector2(0.052 * scale, 0.215 * scale),
            new THREE.Vector2(0.022 * scale, 0.232 * scale),
            new THREE.Vector2(0.0, 0.238 * scale)
        ];
        const bodyGeo = new THREE.LatheGeometry(pts, 24);
        const bodyMat = new THREE.MeshStandardMaterial({
            color: mau(0xD32F2F),
            roughness: 0.35,
            metalness: 0.04
        });
        const bodyMesh = new THREE.Mesh(bodyGeo, bodyMat);
        group.add(bodyMesh);

        // 2. Đài lá dâu màu xanh phía trên
        const leafMat = new THREE.MeshStandardMaterial({
            color: mau(0x2E7D32),
            roughness: 0.52,
            metalness: 0.02,
            side: THREE.DoubleSide
        });
        const leafCount = 5;
        const leafRadius = 0.075 * scale;
        const leafY = 0.23 * scale;

        for (let i = 0; i < leafCount; i++) {
            const angle = (i / leafCount) * Math.PI * 2;
            const leafGeo = new THREE.ConeGeometry(0.026 * scale, leafRadius, 3);
            leafGeo.rotateX(Math.PI / 2);
            const leafMesh = new THREE.Mesh(leafGeo, leafMat);
            leafMesh.position.set(
                Math.sin(angle) * (leafRadius * 0.38),
                leafY,
                Math.cos(angle) * (leafRadius * 0.38)
            );
            leafMesh.rotation.y = angle;
            leafMesh.rotation.x = 0.3; // lá ôm vát nhẹ xuống quả
            group.add(leafMesh);
        }

        // 3. Cuống dâu nhỏ ở đỉnh
        const stemGeo = new THREE.CylinderGeometry(0.007 * scale, 0.008 * scale, 0.035 * scale, 8);
        const stemMat = new THREE.MeshStandardMaterial({
            color: mau(0x1B5E20),
            roughness: 0.6
        });
        const stemMesh = new THREE.Mesh(stemGeo, stemMat);
        stemMesh.position.set(0, leafY + 0.016 * scale, 0);
        group.add(stemMesh);

        return group;
    }

    /**
     * Tạo khối 3D Quả cherry:
     * Quả cầu đỏ bóng, có cuống mảnh uốn cong
     */
    function taoKhoi3DCherry() {
        const group = new THREE.Group();

        // 1. Quả cherry tròn bóng
        const radius = 0.16;
        const cherryGeo = new THREE.SphereGeometry(radius, 24, 20);
        cherryGeo.scale(1.0, 0.94, 1.0); // dẹt nhẹ hai đầu tạo độ tự nhiên
        const cherryMat = new THREE.MeshStandardMaterial({
            color: mau(0x990022),
            roughness: 0.15,
            metalness: 0.08
        });
        const cherryMesh = new THREE.Mesh(cherryGeo, cherryMat);
        cherryMesh.position.set(0, radius * 0.92, 0);
        group.add(cherryMesh);

        // 2. Cuống cherry uốn cong mềm mại
        const curve = new THREE.CatmullRomCurve3([
            new THREE.Vector3(0, radius * 1.8, 0),
            new THREE.Vector3(0.03, radius * 2.6, 0.02),
            new THREE.Vector3(0.08, radius * 3.4, 0.04)
        ]);
        const stemGeo = new THREE.TubeGeometry(curve, 10, 0.01, 6, false);
        const stemMat = new THREE.MeshStandardMaterial({
            color: mau(0x4E342E),
            roughness: 0.65,
            metalness: 0.0
        });
        const stemMesh = new THREE.Mesh(stemGeo, stemMat);
        group.add(stemMesh);

        return group;
    }

    /**
     * Tạo khối 3D Bánh Macaron:
     * 2 đĩa dẹt kẹp nhân kem ở giữa
     */
    function taoKhoi3DMacaron() {
        const group = new THREE.Group();
        const r = 0.17;
        const shellH = 0.055;
        const creamH = 0.038;

        // Vỏ macaron pastel hồng ngọt ngào
        const shellMat = new THREE.MeshStandardMaterial({
            color: mau(0xF48FB1),
            roughness: 0.42,
            metalness: 0.02
        });

        // Nhân kem vani béo ngậy
        const creamMat = new THREE.MeshStandardMaterial({
            color: mau(0xFFF8E1),
            roughness: 0.35,
            metalness: 0.02
        });

        // Đĩa dưới
        const bottomGeo = new THREE.CylinderGeometry(r * 0.96, r, shellH, 24);
        const bottomMesh = new THREE.Mesh(bottomGeo, shellMat);
        bottomMesh.position.set(0, shellH / 2, 0);
        group.add(bottomMesh);

        // Lớp nhân kem
        const creamGeo = new THREE.CylinderGeometry(r * 0.92, r * 0.92, creamH, 24);
        const creamMesh = new THREE.Mesh(creamGeo, creamMat);
        creamMesh.position.set(0, shellH + creamH / 2, 0);
        group.add(creamMesh);

        // Đĩa trên
        const topGeo = new THREE.CylinderGeometry(r, r * 0.96, shellH, 24);
        const topMesh = new THREE.Mesh(topGeo, shellMat);
        topMesh.position.set(0, shellH + creamH + shellH / 2, 0);
        group.add(topMesh);

        return group;
    }

    /**
     * Tạo khối 3D Miếng socola:
     * Khối hộp nâu thanh lịch
     */
    function taoKhoi3DSocola() {
        const group = new THREE.Group();
        const width = 0.32;
        const height = 0.065;
        const depth = 0.24;

        const boxGeo = new THREE.BoxGeometry(width, height, depth);
        const boxMat = new THREE.MeshStandardMaterial({
            color: mau(0x3E2723),
            roughness: 0.28,
            metalness: 0.04
        });
        const boxMesh = new THREE.Mesh(boxGeo, boxMat);
        boxMesh.position.set(0, height / 2, 0);
        // Nghiêng nhẹ tạo vẻ nghệ thuật tự nhiên trên bánh
        boxMesh.rotation.x = 0.12;
        boxMesh.rotation.z = -0.08;
        group.add(boxMesh);

        return group;
    }

    /**
     * Sinh khối 3D cho một đối tượng topping tùy theo loại
     */
    function taoKhoi3DTheoLoai(loai) {
        switch (loai) {
            case 'cherry':
                return taoKhoi3DCherry();
            case 'macaron':
                return taoKhoi3DMacaron();
            case 'socola':
                return taoKhoi3DSocola();
            case 'dau':
            default:
                return taoKhoi3DDauTay();
        }
    }

    /**
     * Kiểm tra vị trí (x, z) có nằm trong góc lát cắt bánh không
     */
    function trongKheCat(x, z, isCutawayActive, cutTheta) {
        if (!isCutawayActive) return false;
        let theta = Math.atan2(x, z);
        if (theta < 0) theta += Math.PI * 2;
        return theta > cutTheta;
    }

    /**
     * Xóa sạch các con của nhóm và giải phóng tài nguyên hình học
     */
    function xoaNhom(group) {
        if (!group) return;
        while (group.children.length > 0) {
            const child = group.children[0];
            group.remove(child);
            if (child.geometry) child.geometry.dispose();
            if (child.children && child.children.length > 0) {
                xoaNhom(child);
            }
        }
    }

    /**
     * Cập nhật texture mặt trên từ Canvas 2D mà ĐÃ ẨN các đối tượng topping
     * Tránh hiện 2 lần (hình phẳng 2D bên dưới khối 3D)
     */
    function capNhatTextureKhongTopping(fabricCanvas, canvasTexture, canvasTextureTang2, tierCount, currentDiameterCm, kemColorHex) {
        if (!fabricCanvas || !canvasTexture) return;

        // Chuẩn bị canvas đệm offscreen cho tầng 1
        if (!offscreenCanvas) {
            offscreenCanvas = document.createElement('canvas');
            offscreenCanvas.width = 400;
            offscreenCanvas.height = 400;
            offscreenCtx = offscreenCanvas.getContext('2d');
        }
        // Chuẩn bị canvas đệm offscreen cho tầng 2 nếu có
        if (tierCount === 2 && !offscreenCanvasTang2) {
            offscreenCanvasTang2 = document.createElement('canvas');
            offscreenCanvasTang2.width = 400;
            offscreenCanvasTang2.height = 400;
            offscreenCtxTang2 = offscreenCanvasTang2.getContext('2d');
        }

        // 1. Tìm các đối tượng trên canvas 2D và lưu lại trạng thái hiển thị của topping
        const objects = fabricCanvas.getObjects ? fabricCanvas.getObjects() : [];
        const toppings = objects.filter(o => o.customType === 'topping');
        const visibleStates = toppings.map(t => t.visible !== false);

        // 2. Tạm thời ẩn các topping
        toppings.forEach(t => t.visible = false);

        const srcEl = fabricCanvas.lowerCanvasEl || fabricCanvas.getElement?.() || document.getElementById('cake-canvas');
        const k = srcEl ? (srcEl.width / 400) : 1;

        if (tierCount === 2) {
            const tier2Obj = objects.find(o => o.customType === 'tier2_circle');
            const r2 = tier2Obj && tier2Obj.radius ? tier2Obj.radius : (currentDiameterCm >= 24 ? 115 : 105);

            // Kiểm tra chữ có thuộc vùng tầng trên không (khoảng cách <= r2)
            const textObj = objects.find(o => o.customType === 'chu');
            const tier2VienObj = objects.find(o => o.customType === 'tier2_vien');
            let textInTang2 = false;
            if (textObj) {
                const tx = typeof textObj.left === 'number' ? textObj.left : 200;
                const ty = typeof textObj.top === 'number' ? textObj.top : 200;
                textInTang2 = (Math.hypot(tx - 200, ty - 200) <= r2);
            }

            // A. Render mặt tầng dưới (Tầng 1):
            // Ẩn chữ (nếu thuộc tầng 2) và viền tầng 2 để mặt tầng dưới chỉ hiện vành ngoài
            if (textObj && textInTang2) textObj.visible = false;
            if (tier2VienObj) tier2VienObj.visible = false;

            if (typeof fabricCanvas.renderAll === 'function') {
                fabricCanvas.renderAll();
            }

            if (srcEl && offscreenCtx) {
                offscreenCtx.clearRect(0, 0, 400, 400);
                offscreenCtx.drawImage(srcEl, 28 * k, 28 * k, 344 * k, 344 * k, 0, 0, 400, 400);

                // Che phần diện tích tâm tầng dưới (nơi tầng 2 đặt lên) bằng màu kem
                const rCover = r2 * (400 / 344);
                offscreenCtx.save();
                offscreenCtx.beginPath();
                offscreenCtx.arc(200, 200, rCover, 0, Math.PI * 2);
                offscreenCtx.fillStyle = kemColorHex || '#FFF8DC';
                offscreenCtx.fill();
                offscreenCtx.restore();
            }

            // B. Render mặt tầng trên (Tầng 2):
            if (textObj) textObj.visible = true;
            if (tier2VienObj) tier2VienObj.visible = true;

            if (typeof fabricCanvas.renderAll === 'function') {
                fabricCanvas.renderAll();
            }

            if (srcEl && offscreenCtxTang2) {
                offscreenCtxTang2.clearRect(0, 0, 400, 400);
                const sx = (200 - r2) * k;
                const sy = (200 - r2) * k;
                const sDim = 2 * r2 * k;
                offscreenCtxTang2.drawImage(srcEl, sx, sy, sDim, sDim, 0, 0, 400, 400);
            }

            if (canvasTextureTang2) {
                if (canvasTextureTang2.image !== offscreenCanvasTang2) {
                    canvasTextureTang2.image = offscreenCanvasTang2;
                }
                canvasTextureTang2.needsUpdate = true;
            }
        } else {
            // 1 tầng tiêu chuẩn
            if (typeof fabricCanvas.renderAll === 'function') {
                fabricCanvas.renderAll();
            }
            if (srcEl && offscreenCtx) {
                offscreenCtx.clearRect(0, 0, 400, 400);
                offscreenCtx.drawImage(srcEl, 28 * k, 28 * k, 344 * k, 344 * k, 0, 0, 400, 400);
            }
        }

        // 3. Khôi phục lại trạng thái hiển thị của các topping và chữ trên canvas 2D
        toppings.forEach((t, i) => t.visible = visibleStates[i]);
        const textObjRe = objects.find(o => o.customType === 'chu');
        if (textObjRe) textObjRe.visible = true;
        const tier2VienRe = objects.find(o => o.customType === 'tier2_vien');
        if (tier2VienRe) tier2VienRe.visible = true;

        if (typeof fabricCanvas.renderAll === 'function') {
            fabricCanvas.renderAll();
        }

        // 4. Trỏ Three.js CanvasTexture sang canvas đệm và đánh dấu cần cập nhật
        if (canvasTexture.image !== offscreenCanvas) {
            canvasTexture.image = offscreenCanvas;
        }
        canvasTexture.needsUpdate = true;
    }

    /**
     * Đồng bộ toàn bộ topping từ Fabric 2D sang khối 3D trong Three.js
     */
    function dongBoTopping3D(options) {
        if (isSyncing) return;
        isSyncing = true;

        try {
            const {
                fabricCanvas,
                canvasTexture,
                canvasTextureTang2,
                toppingGroup,
                currentRadius = 1.8,
                currentHeight = 1.35,
                tierCount = 1,
                tier1Height = 1.15,
                tier2Height = 0.95,
                tier2Radius = 1.35,
                currentDiameterCm = 20,
                kemColorHex = '#FFF8DC',
                isCutawayActive = false,
                cutTheta = Math.PI * 1.65
            } = options || {};

            if (!toppingGroup) return;

            // 1. Cập nhật texture không chứa topping (cho cả tầng 1 và tầng 2 nếu có)
            if (fabricCanvas && canvasTexture) {
                capNhatTextureKhongTopping(fabricCanvas, canvasTexture, canvasTextureTang2, tierCount, currentDiameterCm, kemColorHex);
            }

            // 2. Dọn sạch nhóm 3D cũ
            xoaNhom(toppingGroup);

            if (!fabricCanvas || typeof fabricCanvas.getObjects !== 'function') return;

            const objects = fabricCanvas.getObjects();
            const toppings = objects.filter(o => o.customType === 'topping');

            // Bán kính chuẩn trên canvas 2D: 172
            const BAN_KINH_CHUAN_2D = 172;
            const TAM_X_2D = 200;
            const TAM_Y_2D = 200;
            const yTopSurface = currentHeight / 2;

            const tier2Obj = objects.find(o => o.customType === 'tier2_circle');
            const r2 = tier2Obj && tier2Obj.radius ? tier2Obj.radius : (currentDiameterCm >= 24 ? 115 : 105);

            toppings.forEach(t => {
                const posX = typeof t.left === 'number' ? t.left : TAM_X_2D;
                const posY = typeof t.top === 'number' ? t.top : TAM_Y_2D;

                const dCenter = Math.hypot(posX - TAM_X_2D, posY - TAM_Y_2D);
                const isTang2 = (tierCount === 2) && ((t.tang === 2) || (dCenter <= r2));

                let x3d, z3d, yPos;
                if (tierCount === 2 && isTang2) {
                    // Topping tầng 2: đúng vị trí và đúng tỉ lệ theo bán kính tầng 2
                    x3d = ((posX - TAM_X_2D) / r2) * tier2Radius;
                    z3d = ((posY - TAM_Y_2D) / r2) * tier2Radius;
                    yPos = (tier1Height + tier2Height) / 2;
                } else if (tierCount === 2 && dCenter > 0) {
                    // Topping vành tầng dưới: dải 2D (r2..172) ứng với dải 3D (tier2Radius..currentRadius),
                    // nếu quy đổi theo 172 thì topping sát mép tầng trên sẽ lún vào thân tầng trên
                    const t01 = Math.min(1, (dCenter - r2) / (BAN_KINH_CHUAN_2D - r2));
                    const r3d = tier2Radius + t01 * (currentRadius - tier2Radius);
                    x3d = ((posX - TAM_X_2D) / dCenter) * r3d;
                    z3d = ((posY - TAM_Y_2D) / dCenter) * r3d;
                    yPos = (tier1Height - tier2Height) / 2;
                } else {
                    x3d = ((posX - TAM_X_2D) / BAN_KINH_CHUAN_2D) * currentRadius;
                    z3d = ((posY - TAM_Y_2D) / BAN_KINH_CHUAN_2D) * currentRadius;
                    yPos = (tierCount === 2) ? (tier1Height - tier2Height) / 2 : yTopSurface;
                }

                // Nếu bánh đang ở chế độ xem lát cắt và topping nằm trong khe bị cắt thì bỏ qua
                if (trongKheCat(x3d, z3d, isCutawayActive, cutTheta)) return;

                const loai = xacDinhLoaiTopping(t);
                const meshGroup = taoKhoi3DTheoLoai(loai);

                meshGroup.position.set(x3d, yPos, z3d);

                // Góc xoay quanh trục Y: góc trên Fabric là độ, xoay ngược chiều kim đồng hồ
                const angleDeg = t.angle || 0;
                meshGroup.rotation.y = -angleDeg * (Math.PI / 180);

                // Áp dụng tỉ lệ nếu có chỉnh kích thước trên 2D
                const scaleX = t.scaleX || 1;
                const scaleY = t.scaleY || 1;
                let baseScale = (scaleX + scaleY) / 2;
                if (tierCount === 2 && isTang2 && currentRadius > 0) {
                    // Cùng tỉ lệ 3D/px như mặt tầng trên (r2 px ↔ tier2Radius) so với tầng dưới (172 px ↔ currentRadius)
                    baseScale *= (tier2Radius / r2) / (currentRadius / BAN_KINH_CHUAN_2D);
                }
                if (baseScale && Math.abs(baseScale - 1) > 0.05) {
                    meshGroup.scale.set(baseScale, baseScale, baseScale);
                }

                // Gắn tham chiếu ngược để hỗ trợ kéo thả trực tiếp nếu cần
                meshGroup.userData = {
                    fabricObj: t,
                    thanhPhanId: t.thanhPhanId,
                    loai: loai
                };

                toppingGroup.add(meshGroup);
            });
        } finally {
            isSyncing = false;
        }
    }

    /**
     * Dựng hệ thống nến sinh nhật và ngọn lửa phát sáng
     * Xếp nến trên vòng ngoài sát viền bánh và tự động né vị trí topping
     * Tham khảo: VIDAKHOSHPEY22/3D-Birthday-Cake (Giấy phép MIT)
     */
    function taoNenSinhNhat(candleGroup, n, currentRadius, currentHeight, isCutawayActive, cutTheta, toppingGroup, tierOpts) {
        if (!candleGroup) return;
        xoaNhom(candleGroup);

        const count = parseInt(n, 10);
        if (isNaN(count) || count <= 0) return;

        let yTop = currentHeight / 2;
        let effectiveRadius = currentRadius;
        if (tierOpts && tierOpts.tierCount === 2) {
            yTop = (tierOpts.tier1Height + tierOpts.tier2Height) / 2;
            effectiveRadius = tierOpts.tier2Radius || (currentRadius * 0.75);
        }
        const candleHeight = 0.46;
        const candleRadius = 0.042;

        // Các màu nến vui tươi pastel xen kẽ
        const CANDLE_COLORS = [0xFFF176, 0xFF80AB, 0x80D8FF, 0xB388FF, 0x69F0AE];

        // Lấy danh sách tọa độ các topping hiện có để né va chạm
        const toppingPositions = [];
        if (toppingGroup && toppingGroup.children) {
            toppingGroup.children.forEach(c => {
                toppingPositions.push({ x: c.position.x, z: c.position.z });
            });
        }

        function viTriTrungTopping(x, z, minDist = 0.32) {
            return toppingPositions.some(tp => {
                const dx = tp.x - x;
                const dz = tp.z - z;
                return (dx * dx + dz * dz) < (minDist * minDist);
            });
        }

        // Profile giọt lửa (LatheGeometry)
        const flamePts = [
            new THREE.Vector2(0.001, 0.0),
            new THREE.Vector2(0.030, 0.035),
            new THREE.Vector2(0.026, 0.075),
            new THREE.Vector2(0.012, 0.115),
            new THREE.Vector2(0.0, 0.14)
        ];
        const flameGeo = new THREE.LatheGeometry(flamePts, 16);

        function taoMotCayNen(x, z, colorHex) {
            const singleCandle = new THREE.Group();

            // 1. Thân nến
            const candleMat = new THREE.MeshStandardMaterial({
                color: mau(colorHex || 0xFFF9C4),
                roughness: 0.45,
                metalness: 0.05
            });
            const bodyGeo = new THREE.CylinderGeometry(candleRadius, candleRadius, candleHeight, 16);
            const body = new THREE.Mesh(bodyGeo, candleMat);
            body.position.y = candleHeight / 2;
            singleCandle.add(body);

            // 2. Bấc nến
            const wickMat = new THREE.MeshStandardMaterial({
                color: mau(0x263238),
                roughness: 0.8
            });
            const wickGeo = new THREE.CylinderGeometry(0.005, 0.005, 0.05, 8);
            const wick = new THREE.Mesh(wickGeo, wickMat);
            wick.position.y = candleHeight + 0.025;
            singleCandle.add(wick);

            // 3. Ngọn lửa ngoài vàng cam rực rỡ
            const flameMat = new THREE.MeshBasicMaterial({
                color: mau(0xFFA000),
                transparent: true,
                opacity: 0.95
            });
            const flame = new THREE.Mesh(flameGeo, flameMat);
            flame.position.y = candleHeight + 0.04;
            singleCandle.add(flame);

            // 4. Lõi lửa trong vàng tươi
            const innerFlameMat = new THREE.MeshBasicMaterial({
                color: mau(0xFFEB3B)
            });
            const innerGeo = new THREE.ConeGeometry(0.015, 0.07, 8);
            const innerFlame = new THREE.Mesh(innerGeo, innerFlameMat);
            innerFlame.position.y = candleHeight + 0.065;
            singleCandle.add(innerFlame);

            // 5. Điểm sáng lung linh (PointLight dịu ấm)
            const candleLight = new THREE.PointLight(0xFFA000, 0.5, 2.5);
            candleLight.position.y = candleHeight + 0.10;
            singleCandle.add(candleLight);

            singleCandle.position.set(x, yTop, z);
            return singleCandle;
        }

        // Bán kính vòng nến sát viền ngoài mặt bánh (0.74 * effectiveRadius)
        const outerCircleRadius = effectiveRadius * 0.74;

        if (count === 1) {
            // Nếu ở tâm không có topping và không bị cắt thì đặt ở tâm, nếu có topping thì đưa ra vòng ngoài
            if (!viTriTrungTopping(0, 0, 0.35) && !trongKheCat(0, 0, isCutawayActive, cutTheta)) {
                candleGroup.add(taoMotCayNen(0, 0, CANDLE_COLORS[0]));
            } else {
                let daDat = false;
                for (let k = 0; k < 16; k++) {
                    const ang = (k / 16) * Math.PI * 2;
                    const x = Math.sin(ang) * outerCircleRadius;
                    const z = Math.cos(ang) * outerCircleRadius;
                    if (!trongKheCat(x, z, isCutawayActive, cutTheta) && !viTriTrungTopping(x, z, 0.28)) {
                        candleGroup.add(taoMotCayNen(x, z, CANDLE_COLORS[0]));
                        daDat = true;
                        break;
                    }
                }
                if (!daDat && !trongKheCat(0, outerCircleRadius, isCutawayActive, cutTheta)) {
                    candleGroup.add(taoMotCayNen(0, outerCircleRadius, CANDLE_COLORS[0]));
                }
            }
        } else {
            // Xếp đều quanh vòng ngoài sát viền, tự động dịch góc né topping
            for (let i = 0; i < count; i++) {
                const baseAngle = (i / count) * Math.PI * 2;
                let bestAngle = baseAngle;
                const candidateOffsets = [0, 0.12, -0.12, 0.24, -0.24, 0.36, -0.36];
                for (const offset of candidateOffsets) {
                    const ang = baseAngle + offset;
                    const testX = Math.sin(ang) * outerCircleRadius;
                    const testZ = Math.cos(ang) * outerCircleRadius;
                    if (trongKheCat(testX, testZ, isCutawayActive, cutTheta)) continue;
                    if (!viTriTrungTopping(testX, testZ, 0.28)) {
                        bestAngle = ang;
                        break;
                    }
                }

                const x = Math.sin(bestAngle) * outerCircleRadius;
                const z = Math.cos(bestAngle) * outerCircleRadius;
                if (trongKheCat(x, z, isCutawayActive, cutTheta)) continue;
                const cColor = CANDLE_COLORS[i % CANDLE_COLORS.length];
                candleGroup.add(taoMotCayNen(x, z, cColor));
            }
        }
    }

    // Xuất API mô-đun trang trí 3D
    window.Cake3DTrangTri = {
        sync: dongBoTopping3D,
        setCandles: taoNenSinhNhat,
        capNhatTextureKhongTopping: capNhatTextureKhongTopping,
        taoKhoi3DTheoLoai: taoKhoi3DTheoLoai,
        xacDinhLoaiTopping: xacDinhLoaiTopping
    };

    // Nếu Cake3D đã khởi tạo trước khi file này nạp xong, kích hoạt đồng bộ ngay
    if (window.Cake3D && typeof window.Cake3D.syncFrom2D === 'function') {
        window.Cake3D.syncFrom2D();
    }

})();
