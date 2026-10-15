# Arsitektur

## Alur permainan

`SplashScreen -> MainMenu -> Custom Bot Match -> Gameplay -> MainMenu`.

`VideoPlayerSceneHandler` meneruskan opening ke scene berikutnya. `LoadManager` menangani perpindahan scene. `MainMenuController` membaca pilihan menu, lalu `GameManager` meneruskan permintaan ke `OfflineMatch`.

`OfflineMatchOptions` menyimpan pengaturan lokal melalui PlayerPrefs. `OfflineMatch` membuat world simulasi, memasang driver IPC, memuat Gameplay dan entity subscene, menyiapkan navigasi, lalu membersihkan pertandingan ketika kembali ke menu. `SessionData` menyediakan status/progres bagi `LoadingScreen`.

## Modul runtime

| Modul | Tanggung jawab |
| --- | --- |
| Authoring | Konfigurasi GameObject dan Baker menjadi data ECS. |
| Components | Komponen, tag, buffer, dan data pertandingan ECS. |
| Systems | Pembaruan karakter, combat, ronde, ekonomi, presentasi, UI, dan simulasi. |
| Offline | Sesi lokal, bot, navigasi, opsi, transisi audio, pemeriksaan standalone eksplisit. |
| Simulation | Bootstrap world, peserta client/server lokal, RPC internal, utilitas NetCode. |
| Session | Penghubung menu ke pertandingan dan data progres sesi. |
| Loading | Pemuatan scene dan pengendali video opening. |
| UI | Controller UI Toolkit; UXML/USS berada di Game/UI. |
| Data | Definisi C# ScriptableObject; instance berada di Game/Data. |
| Behaviours | Pengendali kamera, decal, suara, dan GameObject pendukung. |
| Effects | Penghubung efek senjata dan harvester. |
| Input | Wrapper C# Input System yang dihasilkan otomatis. |
| Utilities / Enums / Interfaces | Fungsi bersama, enumerasi, dan kontrak. |
| Diagnostics | Komponen diagnostik lokal yang dipertahankan dari sumber. |
| Noise | Assembly utilitas noise yang dipakai alat Editor. |

Folder mengelompokkan tanggung jawab tanpa mengganti namespace/nama tipe yang dapat tersimpan dalam aset. Nama historis `GameNetwork`, `Harvester`, dan `Stuff` tetap dipakai untuk kompatibilitas.

## Bot dan objektif

`OfflineParticipantSystem` memberi identitas peserta. `OfflineNavigation` membentuk navigasi. `OfflineBotInputSystem` mengisi input gerak, bidik, tembak, dan reload melalui pengendali yang juga dipakai pemain. `OfflineBotObjectiveSystem` menjalankan keputusan bom. Aturan plant/defuse dibagikan melalui `HarvesterActions` dan sistem Harvester.

`Systems/Round` dan `Systems/Economy` menangani ronde, tim, spawn, serta pembelian. Minimap membaca identitas dan tim peserta. Bot teman mengikuti tim pilihan pemain.

## UI dan referensi

UI memakai UXML untuk hierarki dan USS untuk style. Controller berada di Runtime/UI, elemen kustom di Utilities/UI. Beberapa stylesheet menggunakan alamat Addressables, misalnya `style-sheet-killfeed`. Pemindahan menjaga GUID dan alamat tersebut.

Scene/prefab merujuk script dengan GUID `.meta`. String nama scene dan path Editor diperbarui terpisah saat penataan, lalu diuji kembali. Namespace dan batas assembly asli tetap dipertahankan.

## Audio

`Audio/WwiseProject` berisi authoring, audio asli, dan GeneratedSoundBanks. `OfflineAudioSetup` menyinkronkan bank Windows ke `Assets/StreamingAssets/Audio/GeneratedSoundBanks/Windows` bila diperlukan dan sebelum build. Bank yang sudah tersedia dapat dipakai tanpa aplikasi authoring Wwise.

Integrasi tetap di `Assets/Wwise`, dengan konfigurasi `Assets/WwiseSettings.xml`. `OfflineAudioTransition` menangani suspend/resume audio ketika perpindahan scene dan penyiapan world berat.

## Mesin simulasi

Entities, NetCode, dan Transport digunakan dalam satu proses melalui IPC. Nama world server/client merupakan implementasi simulasi lokal. Menu offline tidak menginisialisasi matchmaking, Relay, atau sesi cloud.

Assembly khusus noise, volumetric clouds, samples, dan Wwise tetap bersama `.asmdef` serta GUID-nya. Kode game utama tetap mengikuti assembly yang sudah digunakan sebelumnya.

Penataan tidak mengganti branding, model karakter, avatar, opening, background, atau voice line.
