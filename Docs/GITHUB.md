# Repositori GitHub

Repositori proyek: **jejow/fp-game-cerdas**. Branch utama adalah `main`. Nama game
di dalam Unity tetap Suna; nama repositori merupakan identitas final project.

## Mengambil sumber lengkap

Instal Git dan Git LFS, kemudian jalankan:

```powershell
git lfs install
git clone https://github.com/jejow/fp-game-cerdas.git
cd fp-game-cerdas
git lfs pull
git lfs fsck
```

Repositori privat memerlukan login GitHub yang memiliki akses. Setelah aset LFS
selesai diunduh, buka folder hasil clone dengan Unity **6000.0.37f1**. Jalankan
scene `Assets/Game/Scenes/MainMenu.unity` untuk mencoba permainan.

LFS menyimpan tekstur, model, audio, video, plugin, dan arsip biner. File kode,
scene/prefab YAML, material, `.meta`, serta dokumentasi tetap berada di Git biasa.
Mengunduh ZIP dari GitHub tidak selalu menggantikan langkah pengambilan aset LFS;
gunakan clone dan `git lfs pull` untuk memperoleh file asli.

## Isi yang dipublikasikan

Sumber meliputi `Assets`, `Audio`, `Packages`, `ProjectSettings`, `Docs`, `Tools`,
README, serta konfigurasi repositori. `Builds/README.md` menyimpan petunjuk build.
Cache Unity, file IDE otomatis, hasil build biner, dan `_ProjectBackups` di luar
proyek tidak termasuk sumber repositori.

Bank Windows untuk StreamingAssets dibuat kembali dari
`Audio/WwiseProject/GeneratedSoundBanks/Windows` oleh `OfflineAudioSetup`. Tunggu
inisialisasi Editor selesai, atau pilih **Tools > Offline > Sync existing audio banks**
sebelum pemeriksaan/build pertama. GUID cache dan data hasil otomatis tidak dijadikan
syarat identitas aset ketika memeriksa hasil clone.

Riwayat repositori asal disimpan lokal dalam cadangan. Repositori baru dimulai dari
snapshot sumber yang sudah ditata. Kredit dan dokumen asal tetap tersedia di Docs.

## Menyimpan perubahan berikutnya

```powershell
git status
git add -A
git commit -m "Describe the project change"
git push
```

Git LFS mengunggah aset yang berubah melalui hook push. Pertahankan `.gitattributes`
dan `.gitignore`. Jangan memasukkan token GitHub atau file kredensial ke proyek.
Alur GitHub Actions warisan masih membutuhkan secrets lisensi Unity; panduan build
lokal ada di [BUILD_AND_TEST.md](BUILD_AND_TEST.md).

Referensi mekanisme penyimpanan aset:
[GitHub Docs tentang Git LFS](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-git-large-file-storage).
