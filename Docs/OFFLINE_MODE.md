# Mode offline

Alur permainan: **Play Offline > Custom Bot Match > Start Match**.

| Pengaturan | Pilihan |
| --- | --- |
| Tim | Corpo / Natif |
| Bot teman | 0-4 |
| Bot lawan | 1-5 |
| Kesulitan | Easy / Normal / Hard |
| Preset | 1 vs 1 / 5 vs 5 |

Corpo memasang bom; Natif mempertahankan lokasi dan menjinakkan bom. Setiap tim maksimal lima peserta termasuk pemain. Pilihan tersimpan lokal melalui PlayerPrefs dan mengatur jumlah peserta sebenarnya.

Kode utama ada di `Assets/Game/Scripts/Runtime/Offline`. ECS/NetCode/Transport dipakai sebagai mesin simulasi melalui IPC. HUD, toko, minimap, model, dan audio mempertahankan aset yang sudah ada.

Aturan objektif diperiksa melalui **Tools > Offline > Validate objective rules**. Pengujian pertandingan tersedia pada **Tools > Offline > Run offline match smoke test**. Cara menjalankan batch ada di [BUILD_AND_TEST.md](BUILD_AND_TEST.md).

Bukti lama disimpan di `Validation/History` dengan tanggal aslinya. Bukti historis tidak menunjukkan bahwa executable cadangan sesuai dengan seluruh kode terbaru.
