# Aset game

Folder ini mengumpulkan aset utama Suna: Art, Data, Input, Physics, Prefabs,
Rendering, Scenes, Scripts, dan UI. Peran setiap folder/file dicatat dalam
[struktur proyek](../../Docs/PROJECT_STRUCTURE.md) dan
[indeks file](../../Docs/FILE_INDEX.md).

Scene pembuka: `Scenes/SplashScreen.unity`. Scene untuk mencoba permainan melalui
Editor: `Scenes/MainMenu.unity`. Pertandingan memuat `Scenes/Gameplay.unity`
dan `Scenes/SubScenes/GameplayEntities.unity`.

Kode runtime dan Editor dipisah di Scripts. Layout/style UI berada di UI;
controller UI berada di Scripts/Runtime/UI. Pindahkan aset bersama `.meta` agar
referensi GUID tetap sesuai.
