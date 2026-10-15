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
