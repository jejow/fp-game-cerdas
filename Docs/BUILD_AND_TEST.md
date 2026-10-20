# Build dan pengujian

## Membuat build Windows

1. Buka proyek dengan Unity **6000.0.37f1**, tunggu kompilasi, lalu simpan scene.
2. Pilih **Tools > Offline > Sync existing audio banks** bila bank Windows belum ada.
3. Pilih **Tools > Offline > Build Windows validation player**.
4. Periksa `Logs/OfflineVerification/build-details.txt` dan `build-result.txt`.
5. Jalankan `Builds/Offline/SteelProtocol.exe` bersama semua isi foldernya.

Builder memakai `BuildOptions.None` untuk Windows 64-bit. Daftar scene aktif adalah
SplashScreen, MainMenu, Gameplay dalam urutan tersebut. Build hanya diterima bila
hasilnya `Succeeded` dan tidak ada error yang ditangkap builder. Warning dijelaskan
di laporan; jangan menganggap status Succeeded cukup bila error masih tercatat.

Pada awal penataan, executable terbaru di `Builds/Offline` sudah tidak ditemukan.
Build cadangan versi lama dipertahankan dan tidak dianggap sebagai rilis kode terbaru.
Penataan folder tidak otomatis menghasilkan executable baru.

## Pemeriksaan dari Editor

| Menu | Yang diperiksa |
| --- | --- |
| `Tools > Project > Validate project structure` | GUID aset yang dipindah, urutan scene, Missing Script pada scene utama, pemisahan kode, dan path audio. |
| `Tools > Offline > Validate objective rules` | Aturan plant/defuse, pembatalan, jarak, fase ronde, dan cleanup. |
| `Tools > Offline > Run offline match smoke test` | Menu/preset, preferensi, tim, bot, input, tembakan, reload, damage, respawn, objektif, minimap, IPC, dan cleanup. |

Hasil struktur berada di `Docs/Validation/structure-validation.json`. Hasil tes
pertandingan berada di `Logs/OfflineVerification/smoke-result.txt`. Laporan yang
dipertahankan setelah maintenance disalin ke `Docs/Validation`.

## Menjalankan batch

Tutup proyek di Unity sebelum memulai proses batch. Jalankan dari root Suna:

```powershell
.\Tools\Project\Invoke-Unity.ps1 -Action Validate
.\Tools\Project\Invoke-Unity.ps1 -Action Smoke
.\Tools\Project\Invoke-Unity.ps1 -Action Build
```

Jika lokasi instalasi Editor berbeda, tambahkan `-UnityPath "C:\lokasi\Unity.exe"`.
Jalankan satu proses setiap kali agar impor peta dan shader tidak bersaing untuk RAM.
Script menyimpan log ke `Logs/ProjectMaintenance` dan mengembalikan kegagalan bila
Unity keluar dengan kode selain nol. Smoke test memasang perangkat input virtual
dan mengaktifkan Wwise untuk batch, tanpa mengubah binding/perilaku permainan biasa.

Pemuatan scene pertama dapat lama karena impor/baking entity scene. Batas pemuatan
Editor pada alur pertandingan adalah 600 detik. Runner menunggu suite menulis hasil
sebelum menutup Editor. Log lengkap membantu membedakan masalah import, compile,
audio, atau gameplay.

## Batas bukti

Pemeriksaan otomatis meliputi perilaku yang dinyatakan dalam hasil tes. Pendengaran
audio, rasa kontrol mouse/keyboard fisik, kualitas tampilan, dan performa pada PC lain
tetap memerlukan pemeriksaan langsung. Penataan ini menguji sumber pada Editor;
hasil historis build/standalone di `Validation/History` mempertahankan tanggal aslinya.

Perbarui indeks setelah mengubah struktur dengan `python Tools/Project/update_docs.py`.

## Pemeriksaan presentasi STEEL PROTOCOL

Jalankan `Tools/Project/Invoke-Unity.ps1 -Action Presentation` untuk menguji opening, Skip, menu, settings, ketiadaan halaman kontributor, dan layout 1600 x 900 serta 1280 x 720. Laporan serta tangkapan layar berada di `Docs/Validation`. `-Action Validate` memeriksa struktur dan referensi robot; `-Action Smoke` juga memeriksa visual robot pada pertandingan kedua tim. Lihat [PRESENTATION.md](PRESENTATION.md).

Runner Smoke membatasi Unity pada dua job worker. Antrean buffer Wwise kini enam refill; log error tetap aktif dan uji menuntut nol error runtime. Laporan terbaru: [88 pemeriksaan gameplay](Validation/robot-gameplay-smoke.txt).
