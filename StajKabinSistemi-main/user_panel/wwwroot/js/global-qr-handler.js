// Bu script, _Layout.cshtml'de en sonda yüklenmelidir.

document.addEventListener('DOMContentLoaded', function () {
    const qrScannerModalElement = document.getElementById('qrScannerModal');
    if (!qrScannerModalElement) {
        // Bu sayfada QR modal'ı yoksa, hiçbir şey yapma.
        // Bu, konsolda gereksiz hataları önler.
        return;
    }

    const qrResultModalElement = document.getElementById('qrResultModal');
    const qrResultTextElement = document.getElementById('qrResultText');
    const qrResultTitleElement = document.getElementById('qrResultModalLabel');

    if (!qrResultModalElement || !qrResultTextElement || !qrResultTitleElement) {
        console.error("QR Sonuç modal elementleri HTML'de eksik.");
        return;
    }

    const qrScannerModal = new bootstrap.Modal(qrScannerModalElement);
    const qrResultModal = new bootstrap.Modal(qrResultModalElement);

    let html5QrCode = null;

    // QR kod okuma başarılı olduğunda bu fonksiyon çalışır
    async function onScanSuccess(decodedText, decodedResult) {
        // Tarama başarılı olduğu anda kamerayı durdur.
        if (html5QrCode && html5QrCode.isScanning) {
            await html5QrCode.stop();
        }
        qrScannerModal.hide();

        // Sonuç modalını "bekleniyor" durumuna getir
        qrResultTitleElement.textContent = 'Doğrulanıyor...';
        qrResultTextElement.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>';
        qrResultModal.show();

        try {
            // Sunucumuzdaki AccessController'a istek gönder!
            const response = await fetch('/api/access/unlock', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify({ qrCodeData: decodedText })
            });

            console.log('Status:', response.status);
            console.log('Content-Type:', response.headers.get('Content-Type'));

            const result = await response.json();

            if (response.ok) { // HTTP 200-299 arası durum kodları
                qrResultTitleElement.textContent = 'Başarılı!';
                qrResultTextElement.textContent = result.message; // "Hoş geldiniz!..."
            } else { // HTTP 4xx, 5xx durum kodları
                qrResultTitleElement.textContent = 'Giriş Başarısız';
                qrResultTextElement.textContent = result.message; // "Yetkiniz yok..."
            }

        } catch (error) {
            console.error('API isteği sırasında hata:', error);
            qrResultTitleElement.textContent = 'Hata';
            qrResultTextElement.textContent = 'Sunucuyla iletişim kurulamadı. Lütfen internet bağlantınızı kontrol edin.';
        }
    }

    // QR kod okuma başarısız olduğunda (bu genellikle her karede olur, önemli değil)
    function onScanFailure(error) {
        // console.warn(`QR code scan failure: ${error}`);
    }

    // Tarayıcı modal'ı gösterildiğinde kamerayı başlat
    qrScannerModalElement.addEventListener('shown.bs.modal', function () {
        // Sadece bir tane Html5Qrcode nesnesi oluştur
        if (!html5QrCode) {
            html5QrCode = new Html5Qrcode("qr-reader");
        }

        // Kamerayı başlat
        html5QrCode.start(
            { facingMode: "environment" }, // Arka kamerayı kullan
            {
                fps: 10,
                qrbox: { width: 250, height: 250 }
            },
            onScanSuccess,
            onScanFailure
        ).catch(err => {
            console.error("Kamera başlatılamadı:", err);
            qrResultTitleElement.textContent = 'Hata';
            qrResultTextElement.textContent = 'Kamera başlatılamadı. Lütfen kamera izinlerini kontrol edin.';
            qrScannerModal.hide();
            qrResultModal.show();
        });
    });

    // Tarayıcı modal'ı gizlendiğinde kamerayı durdur
    qrScannerModalElement.addEventListener('hidden.bs.modal', function () {
        if (html5QrCode && html5QrCode.isScanning) {
            html5QrCode.stop().catch(err => {
                console.warn("Kamera düzgün durdurulamadı. Bu genellikle sorun teşkil etmez.");
            });
        }
    });
});