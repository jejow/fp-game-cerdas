# Kode proyek

`Runtime` menyimpan kode yang dipakai permainan. `Editor` menyimpan alat Unity
untuk maintenance, audio, validasi, dan build. Lihat
[referensi setiap file C#](../../../Docs/CODE_REFERENCE.md) dan
[alur arsitektur](../../../Docs/ARCHITECTURE.md).

Modul runtime dikelompokkan berdasarkan tanggung jawab. Namespace dan nama tipe
historis dipertahankan untuk kompatibilitas serialisasi. Assembly noise mempunyai
`.asmdef` sendiri. Kode vendor seperti Wwise dan volumetric clouds berada di luar
folder kode game utama.

`Runtime/Input/DefaultInputSystem.cs` adalah hasil otomatis. Edit sumber binding di
`../Input/DefaultInputSystem.inputactions`, lalu biarkan Unity menghasilkan wrapper.
