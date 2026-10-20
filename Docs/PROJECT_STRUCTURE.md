# Struktur proyek

Root `Suna` adalah proyek Unity yang dapat dibuka langsung melalui Unity Hub.
`Assets`, `Packages`, dan `ProjectSettings` harus tetap bersama. Dokumentasi berikut
menjelaskan folder utama; [DIRECTORY_INDEX.md](DIRECTORY_INDEX.md) mencatat setiap
subdirektori dan [FILE_INDEX.md](FILE_INDEX.md) mencatat setiap file sumber/aset.

## Root proyek

| Direktori/file | Isi dan kegunaan |
| --- | --- |
| `Assets` | Semua aset yang dikenali Unity beserta metadata GUID-nya. |
| `Audio/WwiseProject` | Proyek authoring Wwise, audio asli, work unit, dan bank hasil generasi. |
| `Builds` | Hasil build player yang dibuat dari sumber ini; baca README di dalamnya. |
| `Docs` | Panduan, arsitektur, referensi kode, indeks file, pemberitahuan vendor, dan bukti validasi. |
| `Packages/manifest.json` | Dependensi langsung serta package embedded. |
| `Packages/packages-lock.json` | Versi dan dependensi hasil resolusi Unity Package Manager. |
| `ProjectSettings` | Build scenes, kualitas, input, rendering, dan pengaturan Unity. |
| `Tools/Project` | Generator dokumentasi dan perintah batch Unity. |
| `Tools/ThirdPartySources` | Arsip source integrasi Wwise dari sumber asli proyek. |
| `.github` | Panduan kode repositori. Konfigurasi CI warisan diarsipkan di luar proyek. |
| `.vscode`, `.vsconfig` | Konfigurasi editor kode yang dipertahankan. |
| `.gitattributes`, `.gitignore` | Aturan Git dan pengecualian hasil otomatis. |
| `README.md` | Pintu masuk dokumentasi dan langkah menjalankan game. |

`Library`, `Temp`, `Logs`, `UserSettings`, `.csproj`, dan `.sln` merupakan hasil
lokal Unity/IDE. Unity dapat membuatnya kembali setelah proyek dibuka. Histori
repositori `.git` dipertahankan dan tidak merupakan aset yang ikut build.

## Aset milik game

| Direktori | Isi |
| --- | --- |
| `Assets/Game/Art` | Model, tekstur, material, animasi, HUD artwork, cinematic, opening, dan VFX. |
| `Assets/Game/Data` | Instance ScriptableObject untuk karakter, senjata, mod, UI, dan pengaturan. |
| `Assets/Game/Input` | `DefaultInputSystem.inputactions`, sumber binding input. |
| `Assets/Game/Physics` | Material dan data fisika. |
| `Assets/Game/Prefabs` | Prefab karakter, perlengkapan, lingkungan, UI, dan penghubung simulasi. |
| `Assets/Game/Rendering/GameplayShaders` | Shader, material, dan render target efek gameplay yang sebelumnya tercampur dengan script. |
| `Assets/Game/Scenes` | Scene pembuka, menu, gameplay, lighting, dan entity subscene. |
| `Assets/Game/Scripts/Runtime` | Logika yang dipakai permainan. |
| `Assets/Game/Scripts/Editor` | Alat Editor untuk audio, build, validasi, dan noise. |
| `Assets/Game/UI` | Layout UXML dan stylesheet USS, termasuk stylesheet elemen bersama. |

`Scenes/Gameplay` menyimpan lightmap, lighting data, dan volume dari scene gameplay.
Isi itu diperlukan oleh peta dan tidak dianggap sampah pengembangan.
`Scenes/SubScenes/GameplayEntities.unity` menyimpan authoring entity pertandingan.

## Kode

Runtime dipisahkan ke `Authoring`, `Components`, `Data`, `Enums`, `Interfaces`,
`Behaviours`, `Effects`, `Input`, `Loading`, `Session`, `Offline`, `Simulation`,
`Systems`, `UI`, `Utilities`, `Diagnostics`, dan `Noise`.

Di dalam `Systems`, file dikelompokkan berdasarkan `Character`, `Combat`, `Round`,
`Economy`, `Presentation`, `Simulation`, `Harvester`, `Stuff`, `UI`, dan
`GameResources`. Kelas serta namespace lama dipertahankan untuk menjaga kompatibilitas
serialisasi. Penjelasan setiap file C# beserta tipe yang dideklarasikan tersedia di
[CODE_REFERENCE.md](CODE_REFERENCE.md).

`Editor/Offline` menyediakan sinkronisasi bank, tes aturan, tes pertandingan, runner
batch, dan build Windows. `Editor/Project` memeriksa struktur dan GUID. `Editor/NoiseTools`
mempertahankan assembly alat noise. Runtime/Noise mempunyai assembly sendiri.

## Folder khusus dan pihak ketiga

| Direktori | Alasan dipertahankan |
| --- | --- |
| `Assets/Development` | Sandbox dan scene integrasi yang dipisahkan dari scene produk; tidak masuk daftar build utama. |
| `Assets/ThirdParty` | Placeholder, Robot1F, efek Travis, dan volumetric clouds bersama metadata/assembly asal. |
| `Assets/Wwise` | Integrasi, plugin native, editor tools, dan referensi audio. Struktur vendor dipertahankan. |
| `Assets/WwiseSettings.xml` | Lokasi authoring project dan konfigurasi integrasi audio. |
| `Assets/StreamingAssets` | File yang perlu tersedia lokal pada runtime, termasuk bank audio. |
| `Assets/Resources` | Aset yang diakses lewat Resources dan konfigurasi runtime package. |
| `Assets/AddressableAssetsData` | Group/alamat aset serta konfigurasi builder Addressables. |
| `Assets/Settings` | URP, renderer, volume, dan pengaturan rendering. |
| `Assets/TextMesh Pro` | Font, shader, resource, dan material TMP. |
| `Assets/UI Toolkit` | Tema UI Toolkit. |
| `Assets/Samples` | Sample package yang dipertahankan beserta dependensinya. |
| `Assets/Gizmos` | Ikon alat/gizmo Editor. |
| `Assets/SceneDependencyCache` | Cache entity scene yang dibuat Unity; pengecualian Git tetap berlaku. |

## Format file

| Ekstensi | Peran |
| --- | --- |
| `.cs` | Kode C#. |
| `.meta` | GUID dan pengaturan impor; selalu ikut aset pasangannya. |
| `.unity`, `.prefab` | Scene dan prefab. |
| `.asset` | Data serialisasi Unity, termasuk konfigurasi dan ScriptableObject. |
| `.asmdef`, `.asmref` | Definisi/referensi assembly. |
| `.uxml`, `.uss` | Struktur dan style UI Toolkit. |
| `.inputactions` | Map dan binding Input System. |
| `.shader`, `.hlsl`, `.shadergraph`, `.vfx` | Rendering dan efek visual. |
| `.fbx`, `.anim`, `.controller` | Model, animasi, dan Animator. |
| `.wproj`, `.wwu`, `.bnk`, `.wem` | Authoring dan data audio Wwise. |

## Menambah atau memindahkan file

Gunakan modul yang sesuai dengan tanggung jawabnya. Aset konfigurasi masuk `Data`,
definisi C#-nya masuk `Scripts/Runtime/Data`, layout UI masuk `UI`, controller masuk
`Scripts/Runtime/UI`, dan alat Editor masuk `Scripts/Editor`.

Pindahkan melalui AssetDatabase Unity, atau tutup Editor lalu pindahkan aset bersama
`.meta`. Perbarui string scene/path, UXML/USS URL, dan alat build yang memakai path
langsung. GUID dan alamat Addressables tidak perlu dibuat baru. Setelah itu jalankan
validasi dan perbarui indeks: `python Tools/Project/update_docs.py`.
