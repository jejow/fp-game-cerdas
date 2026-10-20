# Alat proyek

| Lokasi | Fungsi |
| --- | --- |
| `Project/update_docs.py` | Membuat ulang indeks setiap direktori, file, dan script proyek. Jalankan dari root: `python Tools/Project/update_docs.py`. |
| `Project/check_repository.py` | Memeriksa sumber pada staging Git, pasangan `.meta`, GUID, pointer/objek LFS, pola kredensial, dan tautan dokumentasi. Opsi `--lfs-fsck` memverifikasi hash objek LFS. |
| `Project/Invoke-Unity.ps1` | Menjalankan Validate, Smoke, atau Build melalui Unity batch. |
| `ThirdPartySources` | Arsip source integrasi Wwise asli yang dipertahankan. |

Alat Unity yang memerlukan assembly Editor berada di
`Assets/Game/Scripts/Editor`, bukan di folder Tools. Detail penggunaan ada di
[BUILD_AND_TEST.md](../Docs/BUILD_AND_TEST.md).
