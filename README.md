# STEEL PROTOCOL - FPS Offline

Game FPS taktis untuk Windows: satu pemain lokal bersama bot, dua tim, ekonomi senjata, dan objektif memasang atau menjinakkan bom. Proyek memakai **Unity 6000.0.37f1**, Universal Render Pipeline, Entities/DOTS, dan audio Wwise.

Game memakai identitas **STEEL PROTOCOL**, opening/loading baru, menu ruang persiapan robot, dan karakter robot untuk pemain maupun kedua tim bot. Dokumentasi mencakup penataan 15 Oktober dan perubahan presentasi 19 Oktober, dan pembersihan atribusi personal 20 Oktober 2026. Detail aset, alur opening, dan pemeliharaan ada di [identitas dan karakter robot](Docs/PRESENTATION.md).

## Mulai dari sini

| Keperluan | Panduan |
| --- | --- |
| Membuka proyek, bermain, kontrol | [Panduan penggunaan](Docs/GETTING_STARTED.md) |
| Penjelasan folder dan penempatan file | [Struktur proyek](Docs/PROJECT_STRUCTURE.md) |
| Daftar seluruh file sumber dan aset | [Indeks file](Docs/FILE_INDEX.md) |
| Daftar script, tipe C#, tanggung jawab modul | [Referensi kode](Docs/CODE_REFERENCE.md) |
| Identitas, opening, menu, dan karakter robot | [Presentasi game](Docs/PRESENTATION.md) |
| Alur game, bot, UI, dan audio | [Arsitektur](Docs/ARCHITECTURE.md) |
| Pengaturan pertandingan bot | [Mode offline](Docs/OFFLINE_MODE.md) |
| Membuat build Windows dan pengujian | [Build dan pengujian](Docs/BUILD_AND_TEST.md) |
| Pemindahan, pembersihan, hasil pemeriksaan | [Catatan penataan](Docs/MAINTENANCE.md) |
| Lisensi dan pemberitahuan aset/pustaka | [Pemberitahuan pihak ketiga](Docs/THIRD_PARTY_NOTICES.md) |
| Clone lengkap, Git LFS, dan push perubahan | [Repositori GitHub](Docs/GITHUB.md) |

## Membuka dan memainkan proyek

1. Di Unity Hub, tambahkan **folder Suna ini**, lalu buka dengan **Unity 6000.0.37f1**. Root proyek harus berisi `Assets`, `Packages`, dan `ProjectSettings`.
2. Tunggu impor aset dan kompilasi selesai.
3. Buka `Assets/Game/Scenes/MainMenu.unity`, kemudian tekan **Play**.
4. Pilih **Play Offline**, atur **Custom Bot Match**, lalu pilih **Start Match**.

Untuk opening lengkap, mulai dari `Assets/Game/Scenes/SplashScreen.unity`.
Untuk executable, buat build dari sumber saat ini dan jalankan `Builds/Offline/SteelProtocol.exe` bersama semua file pendampingnya. **Saat penataan dimulai, build terbaru sudah tidak ada di disk**; executable cadangan lama disimpan di folder `_ProjectBackups` di luar proyek. Lokasinya tercatat di [catatan penataan](Docs/MAINTENANCE.md).

## Susunan utama

```text
Suna/
├── Assets/
│   ├── Game/
│   │   ├── Art/              Model, tekstur, animasi, opening, VFX
│   │   ├── Data/             Instance ScriptableObject
│   │   ├── Input/            Konfigurasi Input System
│   │   ├── Physics/          Data fisika
│   │   ├── Prefabs/          Prefab gameplay dan presentasi
│   │   ├── Rendering/        Shader/material efek gameplay
│   │   ├── Scenes/           SplashScreen, MainMenu, Gameplay, SubScenes
│   │   ├── Scripts/
│   │   │   ├── Runtime/      Kode game, ECS, bot, UI, data, input
│   │   │   └── Editor/       Build, validasi, audio setup, alat noise
│   │   └── UI/               Layout UXML dan stylesheet USS
│   ├── Development/          Scene percobaan di luar build utama
│   ├── ThirdParty/           Aset/vendor yang dipisahkan
│   ├── Wwise/               Integrasi dan referensi audio Wwise
│   └── ...                  Folder khusus Unity dijelaskan di Docs
├── Audio/WwiseProject/       Authoring, audio sumber, bank Wwise
├── Builds/                   Hasil build dan cadangan lama
├── Docs/                     Dokumentasi, indeks, bukti validasi
├── Packages/                 Dependensi dan package embedded
├── ProjectSettings/          Konfigurasi Unity bersama
├── Tools/                    Perintah maintenance dan source SDK
└── README.md                 Pintu masuk dokumentasi
```

Indeks file mencakup aset dan `.meta`, script, konfigurasi, package embedded, audio sumber, dokumentasi, dan alat proyek. Cache Unity, histori Git, serta biner hasil build dijelaskan sebagai folder otomatis; file di dalamnya tidak disalin ke indeks sumber karena berubah ketika impor/build.

## Fitur

- Karakter robot untuk pemain, bot kedua tim, tangan FPS, serta menu/UI.
- Opening dan menu STEEL PROTOCOL dengan progres pemuatan nyata.
- Pertandingan offline: 0–4 bot teman, 1–5 bot lawan.
- Preset 1 vs 1 dan 5 vs 5; Easy, Normal, Hard.
- Corpo memasang bom; Natif mempertahankan lokasi dan menjinakkan bom.
- Gerak FPS, senjata, reload, damage, kematian, ronde, toko, dan ekonomi.
- HUD, minimap, pengaturan, pause, pilihan tim, loading, dan audio lokal.

Entities, NetCode, dan Transport tetap menjadi mesin simulasi lokal. Nama `Client`, `Server`, dan namespace `GameNetwork` adalah bagian implementasi; driver pertandingan offline memakai IPC dalam satu proses. Detail ada di [arsitektur](Docs/ARCHITECTURE.md).

## Aturan pemeliharaan

- Pindahkan aset melalui Unity, atau pindahkan **aset beserta `.meta`** ketika Editor tertutup. GUID menjaga referensi scene, prefab, dan Addressables.
- Kode game berada di `Scripts/Runtime`; kode Editor di `Scripts/Editor`; layout/style UI di `Assets/Game/UI`.
- `DefaultInputSystem.cs` dibangkitkan dari `Assets/Game/Input/DefaultInputSystem.inputactions`; jangan edit wrapper itu secara manual.
- Pertahankan batas assembly noise, volumetric clouds, Wwise, dan package.
- `Library`, `Temp`, `Logs`, `UserSettings`, `.csproj`, dan `.sln` dibuat ulang Unity/IDE dan dikecualikan dari Git. Pembukaan pertama setelah pembersihan memerlukan impor ulang aset.
- Jalankan validasi struktur dan smoke test setelah mengubah scene, referensi, atau sistem pertandingan.

Lisensi dan pemberitahuan vendor dipertahankan di sumber masing-masing; lihat [pemberitahuan pihak ketiga](Docs/THIRD_PARTY_NOTICES.md). Halaman kredit personal, daftar profil, dan foto kontributor tidak disertakan dalam proyek game.

## Tampilan game

![Menu utama STEEL PROTOCOL](Docs/Validation/main-menu-20261020.png)

Opening, konfigurasi bot, dan Settings juga memiliki tangkapan layar pengujian di `Docs/Validation`.

Sebelum commit/push, jalankan pemeriksaan staging pada [panduan repositori](Docs/GITHUB.md#pemeriksaan-sebelum-commit-dan-push). Laporan persiapan sumber berada di [repository-preflight.json](Docs/Validation/repository-preflight.json).
