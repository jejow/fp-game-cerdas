# Repositori GitHub

Repositori proyek: **jejow/fp-game-cerdas**. Branch utama adalah `main`. Nama game
di dalam Unity adalah STEEL PROTOCOL; folder sumber proyek tetap bernama Suna.

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
snapshot sumber yang sudah ditata. Pemberitahuan lisensi pihak ketiga tersedia di Docs dan sumber vendor masing-masing.

## Menyimpan perubahan berikutnya

```powershell
git status
git add -A
git commit -m "Describe the project change"
git push
```

Git LFS mengunggah aset yang berubah melalui hook push. Pertahankan `.gitattributes`
dan `.gitignore`. Jangan memasukkan token GitHub atau file kredensial ke proyek.
Workflow CI warisan diarsipkan di luar repositori saat pembersihan. Salah satu
aksinya, `actions/upload-artifact@v3`, sudah dihentikan menurut [pemberitahuan GitHub](https://github.com/actions/upload-artifact#actionsupload-artifact).
Build lokal tersedia di [BUILD_AND_TEST.md](BUILD_AND_TEST.md); sumber ini tidak
menjalankan build otomatis ketika di-push.

Referensi mekanisme penyimpanan aset:
[GitHub Docs tentang Git LFS](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-git-large-file-storage).

## Pemeriksaan sebelum commit dan push

Jalankan dari root proyek setelah meninjau perubahan:

```powershell
python Tools/Project/update_docs.py
git add -A
python Tools/Project/check_repository.py --lfs-fsck
git diff --cached --stat
git commit -m "Prepare STEEL PROTOCOL offline project"
git push origin main
```

Pemeriksaan membaca staging, sehingga file yang ditinjau sama dengan file yang
akan masuk commit. Script memeriksa pasangan metadata/GUID, aset LFS beserta
objek lokalnya, file otomatis atau privat, dan tautan dokumentasi. Hasil PASS
adalah pemeriksaan lokal; login, izin akses, dan layanan remote diperiksa Git
saat push dijalankan. Script tidak melakukan commit atau push.

File `.env`, kredensial penandatanganan, lisensi Unity lokal, log, cache, cadangan,
serta hasil build dikecualikan. File lisensi sumber pihak ketiga tetap disertakan.
Tiga folder sumber yang kosong memakai `.gitkeep` untuk mempertahankan folder
beserta GUID-nya ketika di-clone. Metadata untuk folder hasil otomatis tidak
disertakan; pengecualiannya tercatat pada manifest validasi aset.

Pada Windows, jika tampilan path non-ASCII mengganggu Git LFS, gunakan konfigurasi
lokal repositori berikut:

```powershell
git config --local core.quotepath false
```

Pembersihan 20 Oktober menyimpan file IDE otomatis serta CI lama di
`../_ProjectBackups/Suna-20261020-PrePush`. Cache impor Unity tetap lokal dan
dikecualikan dari Git agar pembukaan proyek berikutnya tetap cepat.
