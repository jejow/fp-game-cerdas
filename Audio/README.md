# Audio sumber

`WwiseProject` berisi proyek `CSLike 0.0_WwiseProject.wproj`, Originals, work unit,
dan GeneratedSoundBanks. Nama internal proyek audio dipertahankan untuk kompatibilitas.

`OfflineAudioSetup` menyinkronkan bank Windows ke
`Assets/StreamingAssets/Audio/GeneratedSoundBanks/Windows` sebelum build. Aplikasi
authoring Wwise dibutuhkan untuk mengedit/menghasilkan ulang audio, sedangkan bank
yang sudah tersedia dapat dipakai langsung oleh game.

Integrasi Unity berada di `Assets/Wwise`; konfigurasi lokasi proyek berada di
`Assets/WwiseSettings.xml`. Jangan menghapus Originals atau work unit sebagai cache.
