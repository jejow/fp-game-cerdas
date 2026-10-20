# Membuka dan memainkan STEEL PROTOCOL

## Unity Editor

Gunakan **Unity 6000.0.37f1**, sesuai `ProjectSettings/ProjectVersion.txt`. Tambahkan root proyek melalui Unity Hub. Unity memulihkan package dari manifest/lockfile dan mengimpor aset. Instalasi Editor/package dapat membutuhkan jaringan; permainan tetap offline.

Mulai dari `Assets/Game/Scenes/MainMenu.unity` dan tekan Play. Untuk melihat opening, gunakan `Assets/Game/Scenes/SplashScreen.unity`.

| Scene | Peran |
| --- | --- |
| `SplashScreen.unity` | Layar boot robot, progres pemuatan, kemudian menu; Enter/Space/Escape untuk Skip. |
| `MainMenu.unity` | Menu utama, pengaturan, dan konfigurasi bot. |
| `Gameplay.unity` | Scene pertandingan. |
| `SubScenes/GameplayEntities.unity` | Data ECS yang dimuat bersama pertandingan. |

Urutan build adalah SplashScreen, MainMenu, Gameplay. Entity subscene tidak menjadi entri scene pembuka tersendiri.

## Pertandingan

Pilih **Play Offline > Custom Bot Match**, tentukan tim, jumlah bot, dan kesulitan, lalu pilih **Start Match**. Preset 1 vs 1 cocok untuk mencoba kontrol. Preset 5 vs 5 menempatkan pemain + empat bot teman melawan lima bot lawan.

| Tombol | Fungsi default |
| --- | --- |
| Esc | Pause saat pertandingan; Skip pada opening. |
| W/A/S/D | Bergerak |
| Mouse kiri / kanan | Menembak / membidik |
| Space | Melompat |
| C | Jongkok |
| R | Reload |
| E | Interaksi sesuai keadaan pertandingan |
| B | Toko senjata |
| 1 / 2 / 3 | Senjata utama / sekunder / melee |
| Scroll mouse | Pergantian perlengkapan |
| Q | Menjatuhkan perlengkapan |

Sumber binding: `Assets/Game/Input/DefaultInputSystem.inputactions`. Membeli senjata dan menjalankan objektif mengikuti fase ronde serta posisi pemain.

## Executable

Setelah membuat build, jalankan `Builds/Offline/SteelProtocol.exe` dari foldernya. Bagikan seluruh folder karena executable membutuhkan data, DLL, runtime, dan bank audio pendamping.

Executable cadangan lama ada di `../_ProjectBackups/Suna-20261015/GeneratedAfterValidation/Builds/Legacy/OfflineBeforeFullOffline-20261008/Suna.exe` jika folder cadangan masih disimpan bersama proyek. Build terbaru pada `Builds/Offline` sudah tidak ada ketika penataan dimulai. Buat rilis dari sumber sekarang mengikuti [BUILD_AND_TEST.md](BUILD_AND_TEST.md).
