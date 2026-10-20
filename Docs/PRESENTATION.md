# STEEL PROTOCOL: identitas dan karakter robot

Judul game dipilih pengguna: **STEEL PROTOCOL**. Game tetap FPS taktis offline dengan bot. Pengambilan fitur/Map 001 dari FPS Sample ke ETS tidak termasuk perubahan ini. Pekerjaan GitHub ditunda.

## Tampilan

- Opening berupa layar boot robot, judul baru, dan progres pemuatan MainMenu. Enter, Space, Escape, atau tombol Skip melewati durasi intro; pemuatan scene tetap diselesaikan sebelum menu ditampilkan.
- Video opening/logo merek lama dilepas dari scene pembuka. Splash Unity dinonaktifkan di Player Settings.
- Menu memakai ruang persiapan robot 3D, pencahayaan biru/jingga, platform unit, dan layout UI baru.
- Pilihan tim serta ikon karakter menggunakan potret robot yang dirender dari model game.
- Menu tidak memiliki halaman kredit personal atau daftar kontributor. Logo judul lama diganti judul baru.

## Karakter

Kedua tim memakai model mekanis Corpo yang sudah tersedia di proyek. Model ini memiliki rangka dan animasi untuk senjata, gerak, reload, dan objektif. Corpo memakai lapisan armor jingga; Natif memakai biru. Shader `Game/Robot Armor` mengubah warna panel armor saat rendering sambil mempertahankan detail logam, normal map, dan lampu unit. Tekstur sumber tidak diubah.

`Assets/Game/Prefabs/Character/Robots` berisi empat prefab yang dipakai gameplay: Corpo/Natif, masing-masing FPS dan TPS. Pemain lokal, bot teman, bot musuh, dan karakter menu semuanya memakai visual robot. Definisi animasi, pemetaan tulang, dan collider Natif diselaraskan dengan rangka robot. Identitas serta aturan dua tim tetap sama.

Aset manusia asli yang tidak dipakai tetap menjadi sumber historis. Validasi memeriksa bahwa scene rilis tidak bergantung pada model manusia Natif atau prefab manusia menu lama.

Menu mempertahankan kamera/listener serta bank audio Wwise sumber. `SoundManager` memastikan DefaultBank, MainMenu, dan VoiceLines termuat sebelum memutar event, termasuk bila MainMenu dibuka langsung tanpa opening. Prefab menu memuat `OfflineMatchSettings` sebagai konfigurasi default lokal.

## File dan direktori

| Lokasi | Isi |
| --- | --- |
| `Assets/Game/Scripts/Runtime/UI/Branding/GameIdentity.cs` | Nama, tagline, nama executable, dan pengisian label judul. |
| `Assets/Game/Scripts/Runtime/UI/Loading/OpeningScreenController.cs` | Pemuatan menu, progres nyata, dan Skip. |
| `Assets/Game/Scripts/Runtime/UI/MainMenu/MenuRobotDisplay.cs` | Gerakan idle ringan untuk unit di menu. |
| `Assets/Game/UI/Branding` | Style identitas dan PanelSettings menu/opening tersendiri. |
| `Assets/Game/UI/Loading` | Layout dan stylesheet opening. |
| `Assets/Game/UI/MainMenu` | Menu, konfigurasi bot, dan indikator pemuatan pertandingan. |
| `Assets/Game/UI/TeamChoice` | Kartu faksi dengan potret robot. |
| `Assets/Game/Prefabs/Character/Robots` | Prefab FPS/TPS kedua tim. |
| `Assets/Game/Art/Branding/Materials` | Armor kedua tim dan material ruang persiapan. |
| `Assets/Game/Art/Branding/Meshes` | Mesh cincin platform robot. |
| `Assets/Game/Art/Branding/Portraits` | Potret robot untuk UI/HUD. |
| `Assets/Game/Rendering/Robots/RobotArmor.shader` | Pewarnaan armor dan pencahayaan URP. |
| `Assets/Game/Scripts/Editor/Project/RobotPresentationSetup.cs` | Membentuk aset robot dan scene presentasi dari sumber proyek. |
| `Assets/Game/Scripts/Editor/Project/RobotPresentationValidation.cs` | Memeriksa referensi karakter, judul, shader, dan dependensi scene. |
| `Assets/Game/Scripts/Editor/Project/PresentationSmokeBatch.cs` | Menguji opening, navigasi menu, dan layout dua resolusi. |

## Pemeliharaan

`UIDocumentUtils.InstantiateFullSize` mempertahankan TemplateContainer ketika memasang UXML pada menu lain. StyleSheet berada pada container tersebut; mengambil hanya anak pertamanya akan menghilangkan style Settings. Pemain menu memakai satu material armor, sedangkan outline taktis hanya dipakai karakter gameplay.

Ganti identitas utama melalui `GameIdentity`, kemudian selaraskan teks fallback UXML dan Product Name. Nama folder sumber `Suna` tetap menjadi lokasi proyek Unity; nama produk dan executable adalah STEEL PROTOCOL / `SteelProtocol.exe`.

Menu **Tools > Project > Apply robot presentation** membangun ulang scene opening/menu dan aset presentasi. Jalankan hanya bila memang ingin merekonstruksi tampilan tersebut, karena perubahan manual pada kedua scene dapat tergantikan. File yang pertama kali diganti dicadangkan di `../_ProjectBackups/Suna-20261019-Rebranding/Before`.

Pemeriksaan struktur dan presentasi: `Tools/Project/Invoke-Unity.ps1 -Action Validate`. Uji opening/menu: `-Action Presentation`. Uji pertandingan offline: `-Action Smoke`. Hasil presentasi berada di `Docs/Validation/presentation-validation.json` dan `presentation-smoke.json`; hasil pertandingan berada di `Logs/OfflineVerification`.

Build baru dibuat melalui `-Action Build`, dengan keluaran `Builds/Offline/SteelProtocol.exe` beserta data pendamping. Perubahan sumber belum menghasilkan executable baru sampai proses build dijalankan.

## Hasil pengujian 19 Oktober 2026

- 4.406 identitas aset dan empat scene diperiksa tanpa script yang hilang.
- 27 pemeriksaan identitas, karakter robot, panel opening, material, dan dependensi rilis lulus.
- Pemeriksaan UI mencakup opening/Skip, menu, pengaturan, ketiadaan halaman kontributor, dan layout 1600 x 900 serta 1280 x 720.
- 88 pemeriksaan gameplay lulus: dua tim, tangan FPS/tubuh robot, tembak/reload/damage, plant/defuse, kematian/respawn, Easy/Normal/Hard, konfigurasi 1 vs 1 sampai 5 vs 5, minimap, dan driver lokal IPC. Tidak ada error runtime pada uji terakhir.

Uji awal mencatat `Wwise: Voice Starvation` saat beban Editor tinggi. Antrean output audio ditambah dari 4 menjadi 6 refill; sample rate dan samples per frame tetap. Buffer yang lebih besar memberi cadangan pemrosesan dengan tambahan latensi audio, sesuai [penjelasan Audiokinetic tentang buffer dan latensi](https://www.audiokinetic.com/fr/public-library/2024.1.5_8803/?id=soundengine_events.html&source=SDK). Runner Smoke memakai dua job worker untuk membatasi beban pekerjaan Editor. Error audio tetap dihitung sebagai kegagalan uji; laporan PASS terakhir tidak menyembunyikannya.

Bukti terbaru: [hasil gameplay](Validation/robot-gameplay-smoke.txt), [laporan UI](Validation/presentation-smoke.json), [validasi aset](Validation/presentation-validation.json), [pilihan robot](Validation/robot-team-choice-20261019.png), dan [gameplay FPS](Validation/robot-gameplay-20261019.png). Pengujian dilakukan pada Unity Editor; executable baru belum dibangun.

## Pembersihan atribusi personal, 20 Oktober 2026

Halaman dan tombol Credits dihapus bersama layout/style serta referensi prefab/controller. Daftar profil, foto kontributor, dan tangkapan layar halaman lama dikeluarkan dari proyek. Dokumentasi menggunakan pemberitahuan vendor terpisah di [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

Audit mencakup 533 FBX, lebih dari 1,35 juta properti string model biner, header 929 gambar, dan 211 WAV. Path akun pembuat dibersihkan pada 69 FBX (170 field metadata). Ukuran file dan seluruh byte di luar metadata tetap sama. Empat WAV dibersihkan dari tag pembuat RIFF/ID3; hash format/sampel audio dan ukuran file tidak berubah. Bank audio tetap memakai data suara yang sama.

Uji Play Mode setelah penghapusan halaman menghasilkan 20 pemeriksaan UI PASS tanpa error runtime. Pemeriksaan presentasi mencakup 30 aturan, termasuk ketiadaan halaman kontributor pada menu dan dependensi rilis. Laporan metadata berada di [model](Validation/model-metadata-cleanup.json), [audio](Validation/audio-metadata-cleanup.json), dan [pemeriksaan bank audio](Validation/generated-audio-attribution-audit.json).

Satu callback Editor Wwise yang masih mengakses referensi event setelah aset dilepas diperbaiki dengan memeriksa referensi dan melepas callback yang sudah tidak valid. Lisensi dan pemberitahuan hak cipta vendor dipertahankan. Salinan pemulihan berada di `../_ProjectBackups/Suna-20261020-CreditCleanup`, di luar proyek dan di luar sumber Git.

Hasil akhir: 4.404 identitas aset, empat scene, nol Missing Script, 30 pemeriksaan presentasi, dan 20 pemeriksaan UI lulus. Audit nama pada sumber portabel tidak menemukan nama kontributor di luar sumber vendor/lisensi yang dilindungi. Sebanyak 8.918 tautan dokumentasi dan sembilan layout UXML diperiksa. Rincian lengkap: [laporan pembersihan](Validation/attribution-cleanup.json).
