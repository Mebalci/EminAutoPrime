// site.js

document.addEventListener("DOMContentLoaded", function () {
    const navToggle = document.querySelector(".navbar-toggler");
    navToggle.addEventListener("click", function () {
        document.querySelector(".navbar-collapse").classList.toggle("show");
    });
});

<script>
    $(document).ready(function() {
        // Randevular sayfası fonksiyonları
        function loadRandevular(tamamlanmis = false) {
            const table = tamamlanmis ? '#gecmisRandevularTable tbody' : '#aktifRandevularTable tbody';
            $.get('/Kullanici/GetRandevular', { tamamlanmis: tamamlanmis }, function (data) {
                $(table).empty();
                data.forEach(randevu => {
                    $(table).append(`
                    <tr>
                        <td>${randevu.ServisAlani}</td>
                        <td>${randevu.Arac}</td>
                        <td>${randevu.RandevuTarihi}</td>
                        <td>${randevu.Durum}</td>
                        <td>${randevu.Aciklama || '-'}</td>
                    </tr>
                `);
                });
            });
        }

    // Marka değiştiğinde modelleri güncelle
    $('#markaSelect').change(function() {
        const markaId = $(this).val();
    const modelSelect = $('#modelSelect');
    modelSelect.prop('disabled', true);

    if (markaId) {
        $.get('/Kullanici/GetModellerByMarka', { markaId: markaId }, function (data) {
            modelSelect.empty().append('<option value="">Model Seçin</option>');
            data.forEach(model => {
                modelSelect.append(`<option value="${model.id}">${model.text}</option>`);
            });
            modelSelect.prop('disabled', false);
        });
        }
    });

    // Form submit işlemleri
    $('#randevuKaydet').click(function() {
        const formData = $('#randevuForm').serialize();
    $.post('/Kullanici/RandevuOlustur', formData, function(response) {
            if (response.success) {
        $('#randevuModal').modal('hide');
    loadRandevular();
    alert('Randevu başarıyla oluşturuldu.');
            } else {
        alert(response.message);
            }
        });
    });

    $('#aracKaydet').click(function() {
        const formData = $('#aracForm').serialize();
    $.post('/Kullanici/AracKaydet', formData, function(response) {
            if (response.success) {
        $('#aracModal').modal('hide');
    location.reload(); // Araç listesini yenile
            } else {
        alert(response.message);
            }
        });
    });

    // Sayfa yüklendiğinde aktif randevuları yükle
    loadRandevular();
});
</script>