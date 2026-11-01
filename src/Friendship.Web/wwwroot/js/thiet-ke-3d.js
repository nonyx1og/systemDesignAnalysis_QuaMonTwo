// thiet-ke-3d.js - Trình diễn Mô hình Bánh Kem 3D Thời Gian Thực (Three.js WebGL)
// Tích hợp cho FrendshipBakery (FrendshipBakery 3D)
// Tham khảo mô hình bánh nhiều tầng và xếp khối Three.js:
// 1. ascodeasice/three-js-birthday-cake (Giấy phép MIT) - Cấu trúc tham số xếp tầng Cylinder theo trục Y.
// 2. naborajs/birthday-bloom (Giấy phép MIT) - Kỹ thuật dựng khối nhiều tầng và quản lý ánh sáng WebGL.

(function () {
    'use strict';

    if (typeof THREE === 'undefined') {
        console.warn('Three.js chưa được nạp, chế độ 3D tạm thời tắt.');
        return;
    }

    let scene, camera, renderer, controls;
    let cakeGroup, cakeMesh, cakeBoardMesh, rimGroup, candleGroup, insideCutGroup, shadowMesh, toppingGroup;
    let cakeMeshTang2 = null;
    let tierCountState = 1;
    let tier2Radius = 1.35;
    let tier1Height = 1.15;
    let tier2Height = 0.95;
    let currentDiameterCm = 20;
    let fabricCanvasInstance = null;
    let candleCountState = 0;
    let sideMaterial, topMaterial, bottomMaterial, boardMaterial, rimMaterial;
    let topMaterialTang2 = null;
    let canvasTexture = null;
    let canvasTextureTang2 = null;
    let animationFrameId = null;
    let isInitialized = false;
    let isAutoRotating = false;
    let isCutawayActive = false;
    let hasRimState = false;
    const CUT_THETA = Math.PI * 1.65; // phần thân còn lại khi xem lát cắt
    let currentRadius = 1.8;
    let currentHeight = 1.35;
    let currentKemColorHex = '#FFFDF5';
    let currentCotFlavor = 'vani';
    let fabricCanvasEl = null;

    // Màu đại diện cho từng loại cốt bánh bên trong
    const COT_COLORS = {
        'vani': '#F2D49B',       // Vàng bông lan vani (đậm hơn lớp kem trắng để thấy rõ tầng)
        'socola': '#4E342E',     // Nâu đậm socola sponge
        'matcha': '#689F38',     // Xanh lá trà xanh matcha
        'bap': '#FBC02D',        // Vàng bắp ngọt
        'dau': '#F48FB1'         // Hồng dâu tây
    };

    /**
     * Khởi tạo Three.js Scene và Camera
     */
    function init(containerEl, sourceCanvas) {
        if (!containerEl) return;
        fabricCanvasEl = sourceCanvas || document.getElementById('cake-canvas');

        const width = containerEl.clientWidth || 400;
        const height = containerEl.clientHeight || 400;

        // 1. Scene
        scene = new THREE.Scene();

        // 2. Camera
        camera = new THREE.PerspectiveCamera(42, width / height, 0.1, 100);
        datCameraChuan();

        // 3. Renderer
        renderer = new THREE.WebGLRenderer({
            antialias: true,
            alpha: true,
            preserveDrawingBuffer: true
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        if (renderer.outputEncoding !== undefined) {
            renderer.outputEncoding = THREE.sRGBEncoding;
        }
        // Tone mapping ACES nén vùng sáng, kem không bị cháy trắng dưới ánh sáng môi trường
        renderer.toneMapping = THREE.ACESFilmicToneMapping;
        renderer.toneMappingExposure = 1.15;

        containerEl.innerHTML = '';
        containerEl.appendChild(renderer.domElement);

        // 4. OrbitControls
        if (typeof THREE.OrbitControls !== 'undefined') {
            controls = new THREE.OrbitControls(camera, renderer.domElement);
            controls.enableDamping = true;
            controls.dampingFactor = 0.08;
            controls.minDistance = 2.0;
            controls.maxDistance = 11;
            controls.maxPolarAngle = Math.PI / 2 + 0.05; // Không xoay xuống dưới đế
            controls.target.set(0, 0.15, 0);
            controls.autoRotate = false;
            controls.autoRotateSpeed = 2.0;
        }

        // 5. Hệ thống đèn chiếu Studio ấm áp
        setupLighting();

        // 6. Nhóm đối tượng Bánh kem
        cakeGroup = new THREE.Group();
        scene.add(cakeGroup);

        rimGroup = new THREE.Group();
        cakeGroup.add(rimGroup);

        candleGroup = new THREE.Group();
        cakeGroup.add(candleGroup);

        toppingGroup = new THREE.Group();
        cakeGroup.add(toppingGroup);

        insideCutGroup = new THREE.Group();
        cakeGroup.add(insideCutGroup);

        // 7. Xây dựng mô hình ban đầu
        buildMaterials();
        buildCakeBoard();
        buildCakeMesh();

        isInitialized = true;
        animate();

        // Đón nhận sự kiện thay đổi kích thước container
        window.addEventListener('resize', onWindowResize);
    }

    /**
     * Thiết lập ánh sáng Studio ấm áp ngọt ngào
     */
    function setupLighting() {
        // Ánh sáng môi trường dạng phòng studio (IBL): kem có phản chiếu mềm thay vì phẳng/cháy trắng.
        // RoomEnvironment nạp lazy cùng three; thiếu thì chỉ còn các đèn bên dưới.
        if (THREE.RoomEnvironment && THREE.PMREMGenerator) {
            const pmrem = new THREE.PMREMGenerator(renderer);
            scene.environment = pmrem.fromScene(new THREE.RoomEnvironment(), 0.04).texture;
            pmrem.dispose();
        }

        // Các đèn dưới đây chỉ bổ trợ hướng sáng (đã giảm cường độ vì môi trường đã chiếu sáng nền)
        const ambientLight = new THREE.AmbientLight(0xFFF8EE, 0.2);
        scene.add(ambientLight);

        // Đèn chính góc 45° tạo khối
        const keyLight = new THREE.DirectionalLight(0xFFFFFF, 0.6);
        keyLight.position.set(4, 7, 5);
        scene.add(keyLight);

        // Đèn phụ dịu ấm lấp đầy bóng râm
        const fillLight = new THREE.DirectionalLight(0xFFEADB, 0.25);
        fillLight.position.set(-4, 3, -3);
        scene.add(fillLight);

        // Đèn viền phản quang từ phía sau (Backlight / Rim light) tạo ánh viền óng ả
        const rimLight = new THREE.DirectionalLight(0xFFD4A3, 0.4);
        rimLight.position.set(0, 4, -6);
        scene.add(rimLight);
    }

    // Màu kem hex là sRGB; renderer xuất sRGB nên phải đổi sang không gian tuyến tính (three r128 không tự đổi)
    function datMauKem(material, hex) {
        material.color.set(hex).convertSRGBToLinear();
    }

    /**
     * Chuẩn bị vật liệu (Materials)
     */
    function buildMaterials() {
        // Texture động từ Fabric Canvas 2D
        if (fabricCanvasEl) {
            canvasTexture = new THREE.CanvasTexture(fabricCanvasEl);
            canvasTexture.encoding = THREE.sRGBEncoding; // ảnh 2D là sRGB, tránh bị nhạt khi xuất ra renderer sRGB
            canvasTexture.generateMipmaps = true;
            canvasTexture.minFilter = THREE.LinearMipmapLinearFilter;
            canvasTexture.magFilter = THREE.LinearFilter;
        }

        // Vật liệu kem thành bánh (Side)
        sideMaterial = new THREE.MeshPhysicalMaterial({
            roughness: 0.55,
            metalness: 0,
            envMapIntensity: 0.6,
            clearcoat: 0.08,
            clearcoatRoughness: 0.5,
            sheen: new THREE.Color(0x2A2420) // ánh mịn như kem bơ
        });
        datMauKem(sideMaterial, currentKemColorHex);

        if (fabricCanvasEl) {
            const offTang2 = document.createElement('canvas');
            offTang2.width = 400;
            offTang2.height = 400;
            canvasTextureTang2 = new THREE.CanvasTexture(offTang2);
            canvasTextureTang2.encoding = THREE.sRGBEncoding;
            canvasTextureTang2.generateMipmaps = true;
            canvasTextureTang2.minFilter = THREE.LinearMipmapLinearFilter;
            canvasTextureTang2.magFilter = THREE.LinearFilter;
        }

        // Vật liệu mặt trên có in hình 2D (Tầng 1)
        topMaterial = new THREE.MeshStandardMaterial({
            map: canvasTexture,
            envMapIntensity: 0.6,
            roughness: 0.45,
            metalness: 0.03
        });

        // Vật liệu mặt trên tầng 2 có in hình 2D (Tầng 2)
        topMaterialTang2 = new THREE.MeshStandardMaterial({
            map: canvasTextureTang2 || canvasTexture,
            envMapIntensity: 0.6,
            roughness: 0.45,
            metalness: 0.03
        });

        // Mặt đáy bánh
        bottomMaterial = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0xD8C4B6),
            roughness: 0.8
        });

        // Đế bánh vàng kim (Gold Cake Board)
        boardMaterial = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0xD4AF37),
            metalness: 0.82,
            roughness: 0.22
        });

        // Viền kem bóp nổi
        rimMaterial = new THREE.MeshPhysicalMaterial({
            roughness: 0.45,
            metalness: 0,
            envMapIntensity: 0.6,
            clearcoat: 0.1,
            clearcoatRoughness: 0.4,
            sheen: new THREE.Color(0x2A2420)
        });
        datMauKem(rimMaterial, currentKemColorHex);
    }

    /**
     * Dựng đế bánh vàng kim sang trọng (Gold Cake Board)
     */
    function buildCakeBoard() {
        if (cakeBoardMesh) {
            cakeGroup.remove(cakeBoardMesh);
            cakeBoardMesh.geometry.dispose();
        }

        const boardRadius = currentRadius * 1.28;
        const boardHeight = 0.08;
        const boardGeo = new THREE.CylinderGeometry(boardRadius, boardRadius, boardHeight, 64);
        cakeBoardMesh = new THREE.Mesh(boardGeo, boardMaterial);
        const bottomY = (tierCountState === 2) ? (-(tier1Height + tier2Height) / 2) : (-currentHeight / 2);
        cakeBoardMesh.position.y = bottomY - boardHeight / 2;
        cakeGroup.add(cakeBoardMesh);

        // Bóng mờ dưới đế (contact shadow giả bằng gradient xuyên tâm, không cần shadowMap)
        if (!shadowMesh) {
            const c = document.createElement('canvas');
            c.width = c.height = 128;
            const g = c.getContext('2d');
            const grad = g.createRadialGradient(64, 64, 0, 64, 64, 64);
            grad.addColorStop(0.45, 'rgba(60,40,30,0.38)');
            grad.addColorStop(1, 'rgba(60,40,30,0)');
            g.fillStyle = grad;
            g.fillRect(0, 0, 128, 128);
            shadowMesh = new THREE.Mesh(
                new THREE.PlaneGeometry(1, 1),
                new THREE.MeshBasicMaterial({ map: new THREE.CanvasTexture(c), transparent: true, depthWrite: false })
            );
            shadowMesh.rotation.x = -Math.PI / 2;
            cakeGroup.add(shadowMesh);
        }
        shadowMesh.scale.setScalar(boardRadius * 2.6);
        shadowMesh.position.y = cakeBoardMesh.position.y - boardHeight / 2 - 0.005;
    }

    function adjustCylinderTopUV(geo, baseRadius) {
        if (!geo || !geo.attributes || !geo.attributes.uv || !geo.attributes.position) return;
        const uvAttr = geo.attributes.uv;
        const posAttr = geo.attributes.position;
        const factor = 155 / (400 * baseRadius);
        for (let i = 0; i < posAttr.count; i++) {
            const y = posAttr.getY(i);
            if (y > 0.0001) {
                const x = posAttr.getX(i);
                const z = posAttr.getZ(i);
                const u = 0.5 + x * factor;
                const v = 0.5 - z * factor;
                uvAttr.setXY(i, u, v);
            }
        }
        uvAttr.needsUpdate = true;
    }

    function adjustCylinderTopUVTang2(geo, r2) {
        if (!geo || !geo.attributes || !geo.attributes.uv || !geo.attributes.position) return;
        const uvAttr = geo.attributes.uv;
        const posAttr = geo.attributes.position;
        for (let i = 0; i < posAttr.count; i++) {
            const y = posAttr.getY(i);
            if (y > 0.0001) {
                const x = posAttr.getX(i);
                const z = posAttr.getZ(i);
                const u = 0.5 + x / (2 * r2);
                const v = 0.5 - z / (2 * r2);
                uvAttr.setXY(i, u, v);
            }
        }
        uvAttr.needsUpdate = true;
    }

    /**
     * Dựng thân bánh hình trụ 3D (Hỗ trợ bánh 1 tầng hoặc bánh 2 tầng)
     */
    function buildCakeMesh() {
        if (cakeMesh) {
            cakeGroup.remove(cakeMesh);
            cakeMesh.geometry.dispose();
            cakeMesh = null;
        }
        if (cakeMeshTang2) {
            cakeGroup.remove(cakeMeshTang2);
            cakeMeshTang2.geometry.dispose();
            cakeMeshTang2 = null;
        }

        let thetaLength = Math.PI * 2;
        if (isCutawayActive) {
            thetaLength = CUT_THETA; // Cắt một góc ~63 độ để lộ cốt bên trong
        }

        if (tierCountState === 2) {
            // Tầng 1 (Dưới)
            const cakeGeo1 = new THREE.CylinderGeometry(
                currentRadius,
                currentRadius,
                tier1Height,
                64,
                1,
                false,
                0,
                thetaLength
            );
            adjustCylinderTopUV(cakeGeo1, currentRadius);
            const materials1 = [sideMaterial, topMaterial, bottomMaterial];
            cakeMesh = new THREE.Mesh(cakeGeo1, materials1);
            cakeMesh.position.y = -tier2Height / 2;
            cakeGroup.add(cakeMesh);

            // Tầng 2 (Trên, xếp đồng tâm theo trục Y)
            const cakeGeo2 = new THREE.CylinderGeometry(
                tier2Radius,
                tier2Radius,
                tier2Height,
                64,
                1,
                false,
                0,
                thetaLength
            );
            adjustCylinderTopUVTang2(cakeGeo2, tier2Radius);
            const materials2 = [sideMaterial, topMaterialTang2 || topMaterial, bottomMaterial];
            cakeMeshTang2 = new THREE.Mesh(cakeGeo2, materials2);
            cakeMeshTang2.position.y = tier1Height / 2;
            cakeGroup.add(cakeMeshTang2);
        } else {
            // 1 tầng tiêu chuẩn
            const cakeGeo = new THREE.CylinderGeometry(
                currentRadius,
                currentRadius,
                currentHeight,
                64,
                1,
                false,
                0,
                thetaLength
            );
            adjustCylinderTopUV(cakeGeo, currentRadius);
            const materials = [sideMaterial, topMaterial, bottomMaterial];
            cakeMesh = new THREE.Mesh(cakeGeo, materials);
            cakeMesh.position.y = 0;
            cakeGroup.add(cakeMesh);
        }

        buildCakeBoard();
        updateInsideCutaway();
    }

    /**
     * Hiệu ứng mô phỏng mặt cắt cốt bánh chân thực 3 tầng kèm mứt & kem tươi
     */
    function updateInsideCutaway() {
        insideCutGroup.clear();
        if (!isCutawayActive) return;

        const cutColorHex = COT_COLORS[currentCotFlavor] || COT_COLORS['vani'];
        // DoubleSide: mặt phẳng cắt phải thấy được từ cả hai phía của khe cắt
        const spongeMat = new THREE.MeshStandardMaterial({
            color: new THREE.Color(cutColorHex),
            roughness: 0.88,
            metalness: 0.0,
            side: THREE.DoubleSide
        });
        const creamLayerMat = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0xFFFBF2),
            roughness: 0.38,
            metalness: 0.04,
            side: THREE.DoubleSide
        });
        const jamMat = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0xE62E4A),
            roughness: 0.25,
            metalness: 0.08,
            side: THREE.DoubleSide
        });

        function taoLatCatTungTang(r, h, yCenter) {
            const face = new THREE.Group();
            const spongeH = h * 0.26;
            const creamH = h * 0.08;
            const jamH = h * 0.025;

            // 3 tầng sponge bông lan
            const spongeGeo = new THREE.PlaneGeometry(r, spongeH);
            const s1 = new THREE.Mesh(spongeGeo, spongeMat);
            s1.position.set(r / 2, yCenter + h * 0.32, 0);
            face.add(s1);

            const s2 = new THREE.Mesh(spongeGeo, spongeMat);
            s2.position.set(r / 2, yCenter, 0);
            face.add(s2);

            const s3 = new THREE.Mesh(spongeGeo, spongeMat);
            s3.position.set(r / 2, yCenter - h * 0.32, 0);
            face.add(s3);

            // 2 lớp kem tươi kẹp giữa
            const creamGeo = new THREE.PlaneGeometry(r, creamH);
            const c1 = new THREE.Mesh(creamGeo, creamLayerMat);
            c1.position.set(r / 2, yCenter + h * 0.16, 0.001);
            face.add(c1);

            const c2 = new THREE.Mesh(creamGeo, creamLayerMat);
            c2.position.set(r / 2, yCenter - h * 0.16, 0.001);
            face.add(c2);

            // Dải mứt dâu óng kẹp giữa
            const jamGeo = new THREE.PlaneGeometry(r * 0.85, jamH);
            const j1 = new THREE.Mesh(jamGeo, jamMat);
            j1.position.set(r * 0.5, yCenter + h * 0.16, 0.002);
            face.add(j1);

            return face;
        }

        function buildCutFace(rotAngle) {
            const faceGroup = new THREE.Group();
            if (tierCountState === 2) {
                const y1Center = -tier2Height / 2;
                const y2Center = tier1Height / 2;
                faceGroup.add(taoLatCatTungTang(currentRadius, tier1Height, y1Center));
                faceGroup.add(taoLatCatTungTang(tier2Radius, tier2Height, y2Center));
            } else {
                faceGroup.add(taoLatCatTungTang(currentRadius, currentHeight, 0));
            }
            faceGroup.rotation.y = rotAngle;
            return faceGroup;
        }

        // Mặt cắt 1 (tại góc 0)
        const f1 = buildCutFace(-Math.PI / 2);
        insideCutGroup.add(f1);

        // Mặt cắt 2 (tại góc kết thúc thetaLength). Cylinder đặt đỉnh tại (sinθ, cosθ),
        // nên trục +x của mặt phẳng phải xoay một góc θ − π/2 quanh trục y.
        const f2 = buildCutFace(CUT_THETA - Math.PI / 2);
        insideCutGroup.add(f2);
    }

    /**
     * Dựng viền kem bóp nổi 3D khía múi sao (3D Star-piped Fluted Rosettes)
     */
    function buildPipedRim(hasRim) {
        hasRimState = !!hasRim;
        rimGroup.clear();
        if (!hasRim) return;

        // Bỏ các múi kem nằm trong khe lát cắt (θ theo quy ước Cylinder: x = sinθ, z = cosθ)
        function trongKheCat(x, z) {
            if (!isCutawayActive) return false;
            let theta = Math.atan2(x, z);
            if (theta < 0) theta += Math.PI * 2;
            return theta > CUT_THETA;
        }

        const rosetteRadius = 0.088;
        const starGeo = new THREE.CylinderGeometry(rosetteRadius * 0.25, rosetteRadius, rosetteRadius * 1.35, 6);
        starGeo.rotateX(Math.PI / 2);

        function themVongRosette(r, y, count, scale = 1.0, tilt = 0.25) {
            for (let i = 0; i < count; i++) {
                const angle = (i / count) * Math.PI * 2;
                const x = Math.cos(angle) * r;
                const z = Math.sin(angle) * r;
                if (trongKheCat(x, z)) continue;

                const rosette = new THREE.Mesh(starGeo, rimMaterial);
                if (scale !== 1.0) rosette.scale.set(scale, scale, scale);
                rosette.position.set(x, y, z);
                rosette.rotation.y = -angle;
                if (tilt) rosette.rotation.x = tilt;
                rimGroup.add(rosette);
            }
        }

        if (tierCountState === 2) {
            const bottomY = -(tier1Height + tier2Height) / 2;
            const t1TopY = bottomY + tier1Height;
            const t2TopY = bottomY + tier1Height + tier2Height;

            // 1. Viền mép trên Tầng 1
            const countT1 = Math.round(currentRadius * 18);
            themVongRosette(currentRadius - rosetteRadius * 0.5, t1TopY + rosetteRadius * 0.65, countT1, 0.95, 0.25);

            // 2. Viền chân Tầng 1
            const countBot = Math.round(currentRadius * 20);
            themVongRosette(currentRadius + rosetteRadius * 0.15, bottomY + rosetteRadius * 0.45, countBot, 0.75, 0);

            // 3. Viền mép trên Tầng 2
            const countT2 = Math.round(tier2Radius * 18);
            themVongRosette(tier2Radius - rosetteRadius * 0.5, t2TopY + rosetteRadius * 0.65, countT2, 0.85, 0.25);

            // 4. Viền chân tiếp giáp Tầng 2 và Tầng 1
            themVongRosette(tier2Radius + rosetteRadius * 0.12, t1TopY + rosetteRadius * 0.40, countT2, 0.70, 0);
        } else {
            const rosetteCount = Math.round(currentRadius * 18);
            const rimY = currentHeight / 2 + rosetteRadius * 0.65;
            themVongRosette(currentRadius - rosetteRadius * 0.5, rimY, rosetteCount, 1.0, 0.25);

            const bottomRimCount = Math.round(currentRadius * 20);
            const bottomY = -currentHeight / 2 + rosetteRadius * 0.45;
            themVongRosette(currentRadius + rosetteRadius * 0.15, bottomY, bottomRimCount, 0.75, 0);
        }
    }

    /**
     * Đồng bộ tức thời từ Fabric.js 2D sang mô hình 3D
     */
    function syncFrom2D() {
        if (!isInitialized) return;

        // Đồng bộ topping 3D và cập nhật texture đã loại bỏ topping
        const fCanvas = fabricCanvasInstance;
        if (window.Cake3DTrangTri && typeof window.Cake3DTrangTri.sync === 'function' && fCanvas) {
            window.Cake3DTrangTri.sync({
                fabricCanvas: fCanvas,
                canvasTexture: canvasTexture,
                canvasTextureTang2: canvasTextureTang2,
                toppingGroup: toppingGroup,
                currentRadius: currentRadius,
                currentHeight: currentHeight,
                tierCount: tierCountState,
                tier1Height: tier1Height,
                tier2Height: tier2Height,
                tier2Radius: tier2Radius,
                currentDiameterCm: currentDiameterCm,
                kemColorHex: currentKemColorHex,
                isCutawayActive: isCutawayActive,
                cutTheta: CUT_THETA
            });
        } else if (canvasTexture) {
            canvasTexture.needsUpdate = true;
        }

        // Đồng bộ nến nếu đang có nến
        if (window.Cake3DTrangTri && typeof window.Cake3DTrangTri.setCandles === 'function' && candleCountState > 0) {
            window.Cake3DTrangTri.setCandles(
                candleGroup,
                candleCountState,
                currentRadius,
                currentHeight,
                isCutawayActive,
                CUT_THETA,
                toppingGroup,
                {
                    tierCount: tierCountState,
                    tier1Height: tier1Height,
                    tier2Height: tier2Height,
                    tier2Radius: tier2Radius
                }
            );
        }

        // Cập nhật màu kem thành bánh
        if (sideMaterial) {
            datMauKem(sideMaterial, currentKemColorHex);
        }
        if (rimMaterial) {
            datMauKem(rimMaterial, currentKemColorHex);
        }
    }

    /**
     * Đổi màu kem nền
     */
    function setCakeColor(hexColor) {
        currentKemColorHex = hexColor || '#FFFDF5';
        if (sideMaterial) {
            datMauKem(sideMaterial, currentKemColorHex);
        }
        if (rimMaterial) {
            datMauKem(rimMaterial, currentKemColorHex);
        }
        if (canvasTexture) {
            canvasTexture.needsUpdate = true;
        }
    }

    /**
     * Đổi kích thước bánh nền (Đường kính cm)
     */
    function setCakeDimensions(diameterCm) {
        currentDiameterCm = diameterCm || 20;
        let r = 1.8;
        if (diameterCm <= 16) r = 1.5;
        else if (diameterCm <= 18) r = 1.7;
        else if (diameterCm <= 20) r = 1.9;
        else if (diameterCm <= 22) r = 2.1;
        else if (diameterCm >= 24) r = 2.3;

        // Lùi/tiến camera theo tỉ lệ tiLeCamera(), giữ nguyên góc khách đang xoay
        const kCu = tiLeCamera();
        currentRadius = r;
        if (tierCountState === 2) {
            const k2 = (currentDiameterCm >= 24) ? 18 : 16;
            tier2Radius = currentRadius * (k2 / currentDiameterCm);
        }
        if (camera && controls) {
            camera.position.sub(controls.target).multiplyScalar(tiLeCamera() / kCu).add(controls.target);
            controls.update();
        }
        buildCakeBoard();
        buildCakeMesh();
        buildPipedRim(hasRimState); // viền phải theo bán kính mới
        setCandles(candleCountState); // nến xếp theo bán kính mới
        syncFrom2D();
    }

    /**
     * Bật/tắt kiểu viền bánh 3D
     */
    function setRimStyle(hasRim) {
        buildPipedRim(hasRim);
    }

    /**
     * Đổi hương vị cốt bánh (dùng khi xem mặt cắt)
     */
    function setCotFlavor(flavorName) {
        const lower = String(flavorName || '').toLowerCase();
        if (lower.includes('socola')) currentCotFlavor = 'socola';
        else if (lower.includes('matcha') || lower.includes('trà xanh')) currentCotFlavor = 'matcha';
        else if (lower.includes('bắp') || lower.includes('ngô')) currentCotFlavor = 'bap';
        else if (lower.includes('dâu')) currentCotFlavor = 'dau';
        else currentCotFlavor = 'vani';

        if (isCutawayActive) {
            updateInsideCutaway();
        }
    }

    /**
     * Bật/tắt chế độ xem lát cắt bánh
     */
    function toggleCutaway() {
        isCutawayActive = !isCutawayActive;
        buildCakeMesh();
        buildPipedRim(hasRimState);
        setCandles(candleCountState);
        syncFrom2D();
        return isCutawayActive;
    }

    /**
     * Bật/tắt tự động xoay 360°
     */
    function toggleAutoRotate() {
        if (!controls) return false;
        isAutoRotating = !isAutoRotating;
        controls.autoRotate = isAutoRotating;
        return isAutoRotating;
    }

    // Tính tỉ lệ camera theo cả bán kính và tổng chiều cao để bánh 2 tầng không bị cắt mép khung
    function tiLeCamera() {
        const tongChieuCao = (tierCountState === 2) ? (tier1Height + tier2Height) : currentHeight;
        const tiLeBanKinh = currentRadius / 1.5;
        const tiLeChieuCao = tongChieuCao / 1.35;
        return Math.max(1, tiLeBanKinh, tiLeChieuCao);
    }

    function datCameraChuan() {
        const k = tiLeCamera();
        camera.position.set(0, 3.2 * k, 5.6 * k);
    }

    /**
     * Đặt lại góc nhìn chuẩn 45°
     */
    function resetCamera() {
        if (!camera || !controls) return;
        datCameraChuan();
        controls.target.set(0, 0.15, 0);
        controls.update();
    }

    /**
     * Chuyển góc nhìn thẳng từ trên xuống (Top-down)
     */
    function topView() {
        if (!camera || !controls) return;
        camera.position.set(0, 5.0 * tiLeCamera(), 0.001);
        controls.target.set(0, 0, 0);
        controls.update();
    }

    /**
     * Điều chỉnh lại kích thước renderer khi container co giãn
     */
    function resize() {
        if (!renderer || !camera) return;
        const container = renderer.domElement.parentElement;
        if (!container) return;

        const w = container.clientWidth || 400;
        const h = container.clientHeight || 400;

        camera.aspect = w / h;
        camera.updateProjectionMatrix();
        renderer.setSize(w, h);
        if (canvasTexture) canvasTexture.needsUpdate = true;
    }

    function onWindowResize() {
        resize();
    }

    /**
     * Vòng lặp Render 60 FPS
     */
    function animate() {
        animationFrameId = requestAnimationFrame(animate);

        // Đang ở chế độ 2D (khung 3D bị ẩn) thì không render, tránh tốn pin/CPU trên điện thoại
        if (!renderer || renderer.domElement.offsetParent === null) return;

        if (controls) {
            controls.update();
        }

        if (renderer && scene && camera) {
            renderer.render(scene, camera);
        }
    }

    /**
     * Đặt số lượng nến sinh nhật trên bánh 3D
     */
    function setCandles(n) {
        candleCountState = parseInt(n, 10) || 0;
        if (window.Cake3DTrangTri && typeof window.Cake3DTrangTri.setCandles === 'function') {
            window.Cake3DTrangTri.setCandles(
                candleGroup,
                candleCountState,
                currentRadius,
                currentHeight,
                isCutawayActive,
                CUT_THETA,
                toppingGroup,
                {
                    tierCount: tierCountState,
                    tier1Height: tier1Height,
                    tier2Height: tier2Height,
                    tier2Radius: tier2Radius
                }
            );
        }
    }

    /**
     * Chuyển đổi bánh 1 tầng hoặc 2 tầng trong không gian 3D
     */
    function setTiers(n, kichThuocTang2) {
        const soTang = (n === 2) ? 2 : 1;
        const cuK = tiLeCamera();
        tierCountState = soTang;

        if (soTang === 2) {
            let ratio = 0.75;
            if (kichThuocTang2 && currentDiameterCm) {
                ratio = kichThuocTang2 / currentDiameterCm;
            }
            tier2Radius = currentRadius * ratio;
        }

        buildCakeMesh();
        buildPipedRim(hasRimState);

        // Lùi camera nếu chiều cao nâng lên để không bị cắt mép khung
        if (camera && controls) {
            const moiK = tiLeCamera();
            if (cuK > 0 && Math.abs(moiK - cuK) > 0.001) {
                camera.position.sub(controls.target).multiplyScalar(moiK / cuK).add(controls.target);
                controls.update();
            }
        }

        setCandles(candleCountState);
        syncFrom2D();
    }

    /**
     * Dựng texture mặt trên từ ảnh preview 2D đã lưu (thay vì Fabric canvas trực tiếp)
     */
    function loadTextureFromImage(imageOrUrl, onLoad) {
        function xuLyAnh(img) {
            const offscreen = document.createElement('canvas');
            offscreen.width = 400;
            offscreen.height = 400;
            const ctx = offscreen.getContext('2d');
            const w = img.naturalWidth || img.width || 400;
            const h = img.naturalHeight || img.height || 400;
            const kx = w / 400;
            const ky = h / 400;
            // Chỉ lấy hình vuông ôm mặt bánh (tương tự như Cake3DTrangTri)
            ctx.drawImage(img, 28 * kx, 28 * ky, 344 * kx, 344 * ky, 0, 0, 400, 400);

            if (tierCountState === 2) {
                const r2Px = (currentDiameterCm >= 24 ? 115 : 105);
                const rCover = r2Px * (400 / 344);
                ctx.save();
                ctx.beginPath();
                ctx.arc(200, 200, rCover, 0, Math.PI * 2);
                ctx.fillStyle = currentKemColorHex || '#FFF8DC';
                ctx.fill();
                ctx.restore();

                const offTang2 = document.createElement('canvas');
                offTang2.width = 400;
                offTang2.height = 400;
                const ctx2 = offTang2.getContext('2d');
                ctx2.drawImage(img, (200 - r2Px) * kx, (200 - r2Px) * ky, 2 * r2Px * kx, 2 * r2Px * ky, 0, 0, 400, 400);
                if (canvasTextureTang2) canvasTextureTang2.dispose();
                canvasTextureTang2 = new THREE.CanvasTexture(offTang2);
                canvasTextureTang2.encoding = THREE.sRGBEncoding;
                canvasTextureTang2.generateMipmaps = true;
                canvasTextureTang2.minFilter = THREE.LinearMipmapLinearFilter;
                canvasTextureTang2.magFilter = THREE.LinearFilter;
                canvasTextureTang2.needsUpdate = true;
                if (topMaterialTang2) {
                    topMaterialTang2.map = canvasTextureTang2;
                    topMaterialTang2.needsUpdate = true;
                }
            }

            if (canvasTexture) {
                canvasTexture.dispose();
            }
            canvasTexture = new THREE.CanvasTexture(offscreen);
            canvasTexture.encoding = THREE.sRGBEncoding;
            canvasTexture.generateMipmaps = true;
            canvasTexture.minFilter = THREE.LinearMipmapLinearFilter;
            canvasTexture.magFilter = THREE.LinearFilter;
            canvasTexture.needsUpdate = true;

            if (topMaterial) {
                topMaterial.map = canvasTexture;
                topMaterial.needsUpdate = true;
            }
            if (typeof onLoad === 'function') onLoad(canvasTexture);
        }

        if (typeof imageOrUrl === 'string') {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = function () { xuLyAnh(img); };
            img.onerror = function (err) {
                console.error('Không nạp được ảnh texture bánh:', err);
            };
            img.src = imageOrUrl;
        } else if (imageOrUrl instanceof HTMLImageElement) {
            if (imageOrUrl.complete && (imageOrUrl.naturalWidth || imageOrUrl.width)) {
                xuLyAnh(imageOrUrl);
            } else {
                imageOrUrl.addEventListener('load', function () { xuLyAnh(imageOrUrl); });
            }
        }
    }

    /**
     * Chụp ảnh snapshot của bánh 3D từ WebGL canvas hiện tại
     */
    function getSnapshot() {
        if (!renderer || !scene || !camera) return null;
        renderer.render(scene, camera);
        return renderer.domElement.toDataURL('image/png');
    }

    // Xuất API công khai ra window.Cake3D
    window.Cake3D = {
        init: init,
        syncFrom2D: syncFrom2D,
        setCakeColor: setCakeColor,
        setCakeDimensions: setCakeDimensions,
        setRimStyle: setRimStyle,
        setCotFlavor: setCotFlavor,
        toggleCutaway: toggleCutaway,
        toggleAutoRotate: toggleAutoRotate,
        resetCamera: resetCamera,
        topView: topView,
        resize: resize,
        // Các hàm mở rộng cho nến và topping 3D
        setCandles: setCandles,
        setTiers: setTiers,
        setFabricCanvas: function (fc) { fabricCanvasInstance = fc; syncFrom2D(); },
        getToppingGroup: function () { return toppingGroup; },
        getCandleGroup: function () { return candleGroup; },
        getCurrentRadius: function () { return currentRadius; },
        getCurrentHeight: function () { return currentHeight; },
        loadTextureFromImage: loadTextureFromImage,
        getSnapshot: getSnapshot
    };

})();
