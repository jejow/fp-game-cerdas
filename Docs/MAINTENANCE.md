# Penataan proyek - 15 Oktober 2026

Proyek Suna ditata agar sumber game, dokumentasi, data audio, alat Editor, dan file
lokal pengembangan mudah ditemukan. File dipindahkan beserta `.meta` saat Editor
tertutup. Namespace serta nama tipe C# dipertahankan untuk kompatibilitas serialisasi.

## Perpindahan utama

| Lokasi lama | Lokasi baru |
| --- | --- |
| `Assets/_Projet` | `Assets/Game` |
| `_Projet/Arts` | `Game/Art` |
| `_Projet/_Scripts` | `Game/Scripts/Runtime` |
| `_Scripts/Editor` | `Game/Scripts/Editor/Offline` |
| `_Scripts/Eumerations` | `Game/Scripts/Runtime/Enums` |
| `_Scripts/Authorings` | `Game/Scripts/Runtime/Authoring` |
| `_Scripts/MonoBehaviours` | `Game/Scripts/Runtime/Behaviours` |
| `_Scripts/NetCode/Offline` | `Game/Scripts/Runtime/Offline` |
| `_Scripts/NetCode` | `Game/Scripts/Runtime/Simulation` |
| `_Scripts/NetCode/Temp` | `Game/Scripts/Runtime/Session`, Loading, UI/Loading, dan Simulation/Common sesuai fungsi |
| `ScriptablesObject` | `Game/Data`; definisi C# dipindah ke `Game/Scripts/Runtime/Data` |
| Layout/style di `_Scripts/UI` | `Game/UI`; controller tetap pada Runtime/UI |
| `_Scripts/Shaders` | `Game/Rendering/GameplayShaders` |
| `Prefabs/Scenes` | `Game/Scenes` |
| `Netcode/MultiplayerTest.unity` | `Game/Scenes/Gameplay.unity` |
| `MultiplayerTest/EntityScene.unity` | `Game/Scenes/SubScenes/GameplayEntities.unity` |
| `Scenes/SandBox`, `Arts/SceneIntegration`, `6767.unity` | `Assets/Development` |
| Script root, VFX link, wrapper input | Modul yang sesuai di `Game/Scripts/Runtime` |
| `Assets/Editor`, `Assets/Tools/Noise` | `Game/Scripts/Editor/NoiseTools`, `Runtime/Noise` |
| Placeholder, Robot1F, Travis, volumetric clouds | `Assets/ThirdParty` dengan metadata/assembly asal |
| `CSLike 0.0_WwiseProject` | `Audio/WwiseProject` |
| `images` | `Docs/Images` |
| Arsip source integrasi Wwise di root | `Tools/ThirdPartySources` |

Scene build, string pemuatan gameplay, URL UXML/USS, lokasi wrapper input, dan
path Wwise diperbarui mengikuti struktur baru. Bank audio biner dan model tidak
diganti. Nama scene Gameplay sekarang mencerminkan penggunaan offline.

## Pembersihan dan cadangan

Cadangan ada pada folder saudara proyek:
`../_ProjectBackups/Suna-20261015`.

Sebelum penataan disimpan snapshot file teks sumber, inventaris file/GUID, status
Git, dan perubahan lokal yang sudah ada. File lama, log historis, output kompilasi
lokal `.validation`, skema UI otomatis, serta folder kosong dipisahkan ke cadangan.

Satu file `Systems/StuffSystemClient.cs` yang seluruh isinya komentar diarsipkan.
Implementasi aktif pada `Systems/Stuff/StuffSystemClient.cs` tetap tersedia.
Konfigurasi Multiplayer Widgets/UserChoices yang tidak memiliki referensi aset
dipisahkan. Script diagnostik aktif dipertahankan dalam modul Diagnostics.

Cache dan file proyek IDE dapat dibuat kembali Unity. Pemindahan ke cadangan
bersifat dapat dipulihkan dan **tidak berarti ruang disk dibebaskan**.
Rincian hasil pembersihan terakhir dicatat di `Validation/cleanup-summary.json`.

Pembersihan akhir memindahkan 27 target cache/IDE/build lama ke
`../_ProjectBackups/Suna-20261015/GeneratedAfterValidation`. Root proyek kini berisi
sumber, konfigurasi, dokumentasi, dan alat proyek. Histori `.git` dipertahankan.
Unity akan mengimpor ulang aset saat dibuka berikutnya.

Executable cadangan lama masih lengkap di
`../_ProjectBackups/Suna-20261015/GeneratedAfterValidation/Builds/Legacy/OfflineBeforeFullOffline-20261008/Suna.exe`.
Folder pendampingnya tetap diperlukan saat menjalankan executable tersebut.

## Pemeriksaan

Hasil pemeriksaan terbaru tersedia di:

- [Struktur dan GUID aset](Validation/structure-validation.json).
- [Ringkasan penataan](Validation/maintenance-summary.json).
- [Inventaris perpindahan](Validation/path-migration.json).
- [Hasil tes offline terakhir](Validation/smoke-result.txt).
- [Pemeriksaan sumber sebelum/sesudah](Validation/source-preservation.json).
- [Kecocokan bank audio](Validation/audio-banks.json).

Pengujian terakhir **PASS: 82 pemeriksaan** dalam empat pertandingan offline,
tanpa error runtime yang ditangkap suite. Kompilasi C# selesai tanpa error. Tes
mencakup kedua tim, gerak/input, damage, tembakan, reload, plant/defuse,
kematian/respawn, preset, Easy/Normal/Hard, tim/bot, minimap, IPC, dan cleanup.
Screenshot pada `Validation/gameplay-20261015.png` menangkap fase awal pertandingan;
penilaian audio dengan pendengaran dan kualitas kontrol fisik tidak termasuk tes batch.

Log Editor masih mencatat warning tick batching saat pemuatan/render melambat dan
satu warning preview scene saat shutdown. Jumlahnya disimpan dalam ringkasan
validasi. Hasil PASS menyatakan pemeriksaan yang dijalankan berhasil, bukan bahwa
seluruh warning Editor/package sudah dihilangkan.

Pemeriksaan struktur membandingkan 4.398 identitas aset, urutan scene build, referensi
script serialisasi empat scene utama, pemisahan file kode, serta path proyek/metadata
audio. Pada pemeriksaan awal, empat scene juga dibuka sebagai preview dan tidak
memiliki Missing Script. Pemeriksa berikutnya membaca referensi serialisasi agar
tidak memuat lightmap/subscene ke memori sebelum tes pertandingan.

Pengujian pertama saat impor peta melambat melewati pemeriksaan menu, gerak bot,
damage, tembakan, dan reload, lalu kehabisan waktu ketika menunggu perpindahan fase
setelah plant. Snapshot menunjukkan bom sudah dipasang. Alat tes batch diberi
anggaran waktu tambahan; aturan plant empat detik dalam kode permainan tetap sama.
Riwayat upaya tersebut disimpan pada cadangan, dan hasil terakhir dicatat terpisah.

Satu URL gambar pada template `Game/UI/WeaponsIcon/WeaponAsset.uxml` menunjuk aset
yang sudah tidak ada sebelum penataan (`Arme_non_selectionne.png`, GUID
`f708df506774cc14f94f8c074c2e4250`). Referensi ini dicatat sebagai kondisi sumber
sebelumnya; penataan tidak mengganti desain/template tersebut.

## Lingkup hasil

Penataan ini memperbarui struktur sumber dan dokumentasi. Perubahan branding,
background, opening, karakter robot, dan voice line belum diterapkan.
Executable terbaru sudah tidak ada ketika pekerjaan dimulai; build cadangan lama
tetap diberi label Legacy di cadangan luar proyek. Build dari sumber sekarang
mengikuti panduan build.

## Perubahan STEEL PROTOCOL - 19 Oktober 2026

Identitas, opening, background/menu, Settings, pilihan tim, dan visual karakter diperbarui. Kedua tim, pemain lokal, tangan FPS, dan bot memakai robot. Struktur sumber dipertahankan; kode baru mengikuti folder Runtime/UI, Editor/Project, UI, Art/Branding, dan Rendering/Robots. Indeks file/direktori/kode diperbarui. Fitur/Map 001 FPS Sample tidak dipindahkan dan pekerjaan GitHub ditunda.

Sumber sebelum perubahan ada di `../_ProjectBackups/Suna-20261019-Rebranding/Before`. Aset manusia dan opening asal tetap disimpan sebagai referensi, tanpa dipakai sebagai karakter/opening rilis. Cache Library yang sebelumnya diarsipkan dipakai kembali untuk validasi; cache tetap lokal dan dikecualikan dari Git.

Validasi terbaru serta rincian hasil ada di [PRESENTATION.md](PRESENTATION.md). Sebagian proses batch pada mesin ini sempat tertahan di `Cleanup mono` setelah seluruh hasil ditulis dan aset disimpan; proses tersebut dihentikan pada fase shutdown. Uji gameplay terakhir menulis PASS dan Editor keluar normal. Bukti gagal sementara disimpan di cadangan di luar proyek.

## Pembersihan atribusi personal, 20 Oktober 2026

Halaman/tombol Credits, layout/style, field controller, dan referensi prefab dihapus. Dokumentasi lama, foto profil, dan tangkapan layar yang memuat daftar kontributor dipindahkan ke cadangan di luar proyek. File lisensi, header hak cipta vendor, dan pemberitahuan font/pustaka dipertahankan; daftar sumber berada di [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

Nama personal pada komentar kode, nama objek/prefab collider, dan folder sandbox dibersihkan. Aset yang diganti nama tetap memakai GUID dan metadata Unity yang sama. Path mesin pada laporan portabel diganti path relatif. Indeks direktori/file/kode dibangkitkan ulang setelah pemindahan.

Audit metadata memeriksa 533 FBX, 929 gambar, dan 211 WAV. Sebanyak 69 FBX dibersihkan pada 170 field metadata, dengan seluruh byte non-metadata tetap identik. Empat WAV dibersihkan pada tag pembuat RIFF/ID3; format dan sampel suara diverifikasi identik dengan SHA-256. Rincian ada di [model](Validation/model-metadata-cleanup.json) dan [audio](Validation/audio-metadata-cleanup.json).

Uji menu mencakup 20 pemeriksaan Play Mode, tanpa error runtime. Validasi presentasi mencakup 30 pemeriksaan. Selama uji pertama, callback database Wwise mencatat MissingReferenceException setelah event asset dilepas; callback kini memeriksa referensi yang sudah tidak valid. Uji ulang lulus. Sebagian proses Editor batch selesai menguji tetapi tertahan pada penutupan Mono; laporan disimpan sebelum proses batch ditutup.

Perubahan ini tidak menghasilkan executable baru dan tidak mengunggah perubahan ke GitHub. Cadangan sumber, file yang dikeluarkan dari proyek, serta log pemeriksaan lengkap ada di `../_ProjectBackups/Suna-20261020-CreditCleanup`.

Validasi akhir Unity selesai dengan kode keluar 0: 4.404 identitas aset, empat scene, dan nol Missing Script. Nama netral yang dipakai untuk sandbox adalah `RoundTests` dan `UITests`; material prototipe memakai awalan `DebugDefaultMat`. Referensi GUID seluruh aset yang diganti nama dipertahankan. Path sumber historis yang memuat nama personal ikut dianonimkan. Laporan lengkap: [pembersihan atribusi](Validation/attribution-cleanup.json).

## Persiapan commit/push, 20 Oktober 2026

Aturan ignore ditambah untuk arsip lokal, log, dump, file cadangan, environment,
kredensial penandatanganan, serta lisensi Unity lokal. Sebanyak 23 file solusi
dan proyek IDE otomatis dipindahkan ke cadangan di luar repositori. Workflow CI
warisan yang belum disiapkan untuk rilis ini juga diarsipkan.

Git LFS pada Windows kini membaca nama file beraksen melalui pengaturan lokal
`core.quotepath=false`. Format baris pada 14 file diselaraskan dengan versi
sebelumnya tanpa mengubah teks ternormalisasi. Urutan manifest dikembalikan
mengikuti GUID sumber lama; seluruh 4.404 pemetaan identitas tetap sama.

Tiga metadata folder hasil otomatis dikeluarkan dari sumber Git, sesuai
pengecualian manifest. Tiga folder sumber kosong diberi `.gitkeep` agar GUID
yang dipertahankan bisa direkonstruksi pada clone. Checker staging tersedia di
`Tools/Project/check_repository.py`; panduan ada di [GITHUB.md](GITHUB.md).
Cadangan dan log pemeriksaan lokal ada di
`../_ProjectBackups/Suna-20261020-PrePush`.

Pemeriksaan staging juga memperbaiki tiga metadata simbol Mac Wwise yang masih
tercatat sebagai pointer LFS pada index lama. Isi YAML lokal dimasukkan ulang
sebagai file Git biasa sesuai aturan `.meta`; nilai GUID tidak diubah.
Pemeriksaan sumber memverifikasi 8.641 file staging, 4.404 identitas aset, 2.031
pointer LFS, dan 1.870 objek LFS unik. Tidak ada metadata hilang/yatim, tautan
dokumentasi rusak, atau pola kredensial kuat yang terdeteksi. Hash objek LFS
lulus pemeriksaan penuh. Perubahan disiapkan pada staging tanpa commit/push.
