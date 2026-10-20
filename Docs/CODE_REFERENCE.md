# Referensi kode proyek

Mencakup kode game, alat Editor milik proyek, dan script sandbox. Kode vendor/package tercantum dalam indeks file dan tidak digabung dengan kode game.

Total: **233 file C#** dalam **58 kelompok direktori**.
Nama tipe dan namespace lama dipertahankan. Tipe pada tabel adalah ringkasan deklarasi; buka file untuk API lengkap.

## `Assets/Development/Sandbox/UITests/Scripts`

Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [TestCameraPivotRecoil.cs](../Assets/Development/Sandbox/UITests/Scripts/TestCameraPivotRecoil.cs) | Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama. | `TestCameraPivotRecoil` |
| [TestImpactSphere.cs](../Assets/Development/Sandbox/UITests/Scripts/TestImpactSphere.cs) | Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama. | `TestImpactSphere` |
| [TestPlayerViewRotation.cs](../Assets/Development/Sandbox/UITests/Scripts/TestPlayerViewRotation.cs) | Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama. | `TestPlayerViewRotation` |
| [TestShootRecoil.cs](../Assets/Development/Sandbox/UITests/Scripts/TestShootRecoil.cs) | Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama. | `TestShootRecoil` |

## `Assets/Game/Scripts/Editor/NoiseTools`

Alat Editor pembuat/penggabung noise, dengan assembly tersendiri.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [NoiseCombiner.cs](../Assets/Game/Scripts/Editor/NoiseTools/NoiseCombiner.cs) | Alat Editor pembuat/penggabung noise, dengan assembly tersendiri. | `NoiseCombiner` |
| [NoiseGenerator.cs](../Assets/Game/Scripts/Editor/NoiseTools/NoiseGenerator.cs) | Alat Editor pembuat/penggabung noise, dengan assembly tersendiri. | `NoiseGeneratorWindow`, `NoiseType` |

## `Assets/Game/Scripts/Editor/Offline`

Validasi pertandingan, batch runner, build, dan sinkronisasi bank audio.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [OfflineAudioSetup.cs](../Assets/Game/Scripts/Editor/Offline/OfflineAudioSetup.cs) | Sinkronisasi bank Windows dari sumber Wwise sebelum build. | `OfflineAudioSetup` |
| [OfflineMatchValidation.cs](../Assets/Game/Scripts/Editor/Offline/OfflineMatchValidation.cs) | Aturan objektif, smoke test pertandingan, dan build Windows. | `OfflineMatchValidation`, `RuntimeCounter` |
| [OfflineSmokeBatch.cs](../Assets/Game/Scripts/Editor/Offline/OfflineSmokeBatch.cs) | Runner batch untuk suite pengujian offline. | `OfflineSmokeBatch` |

## `Assets/Game/Scripts/Editor/Project`

Validasi struktur/GUID; pembuatan aset presentasi robot dan pengujian menu.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [PresentationSmokeBatch.cs](../Assets/Game/Scripts/Editor/Project/PresentationSmokeBatch.cs) | Uji Play Mode untuk opening, navigasi menu, dan layout dua resolusi. | `PresentationSmokeBatch`, `Report` |
| [ProjectStructureValidation.cs](../Assets/Game/Scripts/Editor/Project/ProjectStructureValidation.cs) | Validasi GUID, scene, pemisahan kode, dan bank audio. | `ProjectStructureValidation`, `AssetIdentity`, `IdentityManifest`, `ValidationResult` |
| [RobotPresentationSetup.cs](../Assets/Game/Scripts/Editor/Project/RobotPresentationSetup.cs) | Membentuk prefab/material/potret robot serta scene opening dan menu. | `RobotPresentationSetup` |
| [RobotPresentationValidation.cs](../Assets/Game/Scripts/Editor/Project/RobotPresentationValidation.cs) | Memeriksa identitas, dependensi rilis, model robot, material, dan shader. | `RobotPresentationValidation`, `Report` |

## `Assets/Game/Scripts/Runtime/Authoring`

Komponen authoring dan Baker ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [AnimationAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/AnimationAuthoring.cs) | Komponen authoring dan Baker ECS. | `AnimationAuthoring`, `AnimationAuthoringBaker` |
| [BombSiteTriggerAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/BombSiteTriggerAuthoring.cs) | Komponen authoring dan Baker ECS. | `BombSiteTriggerAuthoring`, `BombSiteTriggerBaker` |
| [GameResourcesAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/GameResourcesAuthoring.cs) | Komponen authoring dan Baker ECS. | `GameResourcesAuthoring`, `Baker` |
| [GrenadesAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/GrenadesAuthoring.cs) | Komponen authoring dan Baker ECS. | `GrenadesAuthoring`, `Baker` |
| [HealthAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/HealthAuthoring.cs) | Komponen authoring dan Baker ECS. | `HealthAuthoring`, `Baker` |
| [NetcodePrefabsConverter.cs](../Assets/Game/Scripts/Runtime/Authoring/NetcodePrefabsConverter.cs) | Komponen authoring dan Baker ECS. | `NetcodePrefabsConverter`, `ClientPrefabData`, `VisualEffetPrefabData`, `VFXDurationData`, `PrefabsBaker` |
| [SoundAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/SoundAuthoring.cs) | Komponen authoring dan Baker ECS. | `SoundAuthoring`, `SoundAuthoringBaker` |
| [SpawnBarrierAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/SpawnBarrierAuthoring.cs) | Komponen authoring dan Baker ECS. | `SpawnBarrierAuthoring`, `SpawnBarrierAuthoringBaker` |
| [SpawnerAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/SpawnerAuthoring.cs) | Komponen authoring dan Baker ECS. | `SpawnerAuthoring`, `Baker` |
| [SpawnerSettingsAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/SpawnerSettingsAuthoring.cs) | Komponen authoring dan Baker ECS. | `SpawnerSettingsAuthoring`, `SpawnerSettingsAuthoringBaker` |
| [TeamSpawnAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/TeamSpawnAuthoring.cs) | Komponen authoring dan Baker ECS. | `TeamSpawnAuthoring`, `TeamSpawnBaker` |
| [TestPlayerDataAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/TestPlayerDataAuthoring.cs) | Komponen authoring dan Baker ECS. | `TestPlayerDataAuthoring`, `Baker`, `TestPlayerData` |
| [TracerRoundAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/TracerRoundAuthoring.cs) | Komponen authoring dan Baker ECS. | `TracerRoundAuthoring`, `TracerRoundAuthoringBaker` |

## `Assets/Game/Scripts/Runtime/Authoring/Character`

Komponen authoring dan Baker ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterAuthoring`, `Baker` |
| [CharacterColliderAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterColliderAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterColliderAuthoring`, `CharacterColliderAuthoringBaker` |
| [CharacterColliderDataAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterColliderDataAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterColliderDataAuthoring`, `CharacterColliderDataAuthoringBaker` |
| [CharacterInputDebugAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterInputDebugAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterInputDebugAuthoring`, `Baker` |
| [CharacterMoneyAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterMoneyAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterMoneyAuthoring`, `Baker` |
| [CharacterRotationAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CharacterRotationAuthoring.cs) | Komponen authoring dan Baker ECS. | `CharacterRotationAuthoring`, `CharacterRotationAuthoringBaker` |
| [CommonCharacterModelAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/CommonCharacterModelAuthoring.cs) | Komponen authoring dan Baker ECS. | `CommonCharacterModelAuthoring`, `CommonCharacterModelAuthoringBaker` |
| [FirstPersonCharacterModelAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/FirstPersonCharacterModelAuthoring.cs) | Komponen authoring dan Baker ECS. | `FirstPersonCharacterModelAuthoring`, `FirstPersonCharacterModelAuthoringBaker` |
| [ThirdPersonCharacterModelAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Character/ThirdPersonCharacterModelAuthoring.cs) | Komponen authoring dan Baker ECS. | `ThirdPersonCharacterModelAuthoring`, `ThirdPersonCharacterModelAuthoringBaker` |

## `Assets/Game/Scripts/Runtime/Authoring/Stuff`

Komponen authoring dan Baker ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [DropedStuffAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Stuff/DropedStuffAuthoring.cs) | Komponen authoring dan Baker ECS. | `DropedStuffAuthoring`, `Baker` |
| [HarvesterAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Stuff/HarvesterAuthoring.cs) | Komponen authoring dan Baker ECS. | `HarvesterAuthoring`, `Baker` |
| [MeleeWeaponAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Stuff/MeleeWeaponAuthoring.cs) | Komponen authoring dan Baker ECS. | `MeleeWeaponAuthoring`, `Baker` |
| [RangedWeaponAuthoring.cs](../Assets/Game/Scripts/Runtime/Authoring/Stuff/RangedWeaponAuthoring.cs) | Komponen authoring dan Baker ECS. | `RangedWeaponAuthoring`, `Baker` |

## `Assets/Game/Scripts/Runtime/Behaviours`

Komponen GameObject untuk kamera, decal, audio, dan presentasi.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CameraController.cs](../Assets/Game/Scripts/Runtime/Behaviours/CameraController.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `CameraController` |
| [DecalManager.cs](../Assets/Game/Scripts/Runtime/Behaviours/DecalManager.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `DecalManager` |
| [DecalTimer.cs](../Assets/Game/Scripts/Runtime/Behaviours/DecalTimer.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `DecalTimer` |

## `Assets/Game/Scripts/Runtime/Behaviours/Character`

Komponen GameObject untuk kamera, decal, audio, dan presentasi.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [FirstPersonCharacterModelBehaviour.cs](../Assets/Game/Scripts/Runtime/Behaviours/Character/FirstPersonCharacterModelBehaviour.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `FirstPersonCharacterModelBehaviour` |
| [ThirdPersonCharacterModelBehaviour.cs](../Assets/Game/Scripts/Runtime/Behaviours/Character/ThirdPersonCharacterModelBehaviour.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `ThirdPersonCharacterModelBehaviour` |

## `Assets/Game/Scripts/Runtime/Behaviours/Sound`

Komponen GameObject untuk kamera, decal, audio, dan presentasi.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [SoundList.cs](../Assets/Game/Scripts/Runtime/Behaviours/Sound/SoundList.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `SoundList` |
| [SoundManager.cs](../Assets/Game/Scripts/Runtime/Behaviours/Sound/SoundManager.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `SoundManager` |
| [SoundTest.cs](../Assets/Game/Scripts/Runtime/Behaviours/Sound/SoundTest.cs) | Komponen GameObject untuk kamera, decal, audio, dan presentasi. | `SoundTest` |

## `Assets/Game/Scripts/Runtime/Components`

Definisi data, tag, dan buffer ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [AnimationComponent.cs](../Assets/Game/Scripts/Runtime/Components/AnimationComponent.cs) | Definisi data, tag, dan buffer ECS. | `AnimatorReference`, `AnimationFloatBufferElement`, `AnimationIntBufferElement`, `AnimationBoolBufferElement`, `AnimationTriggerBufferElement` |
| [ApplyDamageComponent.cs](../Assets/Game/Scripts/Runtime/Components/ApplyDamageComponent.cs) | Definisi data, tag, dan buffer ECS. | `DamageSource`, `Damageable`, `ApplyDamage`, `ApplyDamageCommand`, `KillDamageCommand` |
| [BombSiteTriggerComponent.cs](../Assets/Game/Scripts/Runtime/Components/BombSiteTriggerComponent.cs) | Definisi data, tag, dan buffer ECS. | `BombSiteTriggerComponent` |
| [CameraComponent.cs](../Assets/Game/Scripts/Runtime/Components/CameraComponent.cs) | Definisi data, tag, dan buffer ECS. | `CameraIsAtached` |
| [ClientSettingsComponent.cs](../Assets/Game/Scripts/Runtime/Components/ClientSettingsComponent.cs) | Definisi data, tag, dan buffer ECS. | `ClientSettingsComponent`, `ClientSettingsSaveTag` |
| [ErrorMessageComponent.cs](../Assets/Game/Scripts/Runtime/Components/ErrorMessageComponent.cs) | Definisi data, tag, dan buffer ECS. | `ErrorMessageComponent` |
| [GameResourcesComponents.cs](../Assets/Game/Scripts/Runtime/Components/GameResourcesComponents.cs) | Definisi data, tag, dan buffer ECS. | `StuffEntityPrefabsBuffer`, `GameResourcesDatabase`, `StuffDatabase`, `InstantiateStuffQueue`, `GameResourcesViewPrefabs` |
| [GrenadeComponent.cs](../Assets/Game/Scripts/Runtime/Components/GrenadeComponent.cs) | Definisi data, tag, dan buffer ECS. | `GrenadeDynamicData`, `GrenadeCommonData`, `GrenadeDatabaseAccess`, `ReleasedGrenade` |
| [GrenadeEffectsComponents.cs](../Assets/Game/Scripts/Runtime/Components/GrenadeEffectsComponents.cs) | Definisi data, tag, dan buffer ECS. | `FlashGrenadeEffect`, `SmokeGrenadeEffect` |
| [HarvesterComponent.cs](../Assets/Game/Scripts/Runtime/Components/HarvesterComponent.cs) | Definisi data, tag, dan buffer ECS. | `HarvesterComponent`, `HarvesterPlanting`, `HarvesterDefusing`, `HarvesterPlanted`, `RpcHarvesterOwnerChange`, `RpcHarvesterDropped`, `RpcHarvesterPlanted`, `RpcHarvesterDefuseStop`; +6 tipe lain |
| [HarvesterOldComponent.cs](../Assets/Game/Scripts/Runtime/Components/HarvesterOldComponent.cs) | Definisi data, tag, dan buffer ECS. | Atribut/utility tanpa tipe bernama. |
| [HitComponent.cs](../Assets/Game/Scripts/Runtime/Components/HitComponent.cs) | Definisi data, tag, dan buffer ECS. | `HitCommand` |
| [MeleeWeaponComponents.cs](../Assets/Game/Scripts/Runtime/Components/MeleeWeaponComponents.cs) | Definisi data, tag, dan buffer ECS. | `MeleeWeaponDynamicData`, `MeleeWeaponCommonData`, `MeleeWeaponDatabaseAccess` |
| [PhaseTimesBuffer.cs](../Assets/Game/Scripts/Runtime/Components/PhaseTimesBuffer.cs) | Definisi data, tag, dan buffer ECS. | `PhaseTimesBuffer` |
| [PlayerHarvesterActionsComponent.cs](../Assets/Game/Scripts/Runtime/Components/PlayerHarvesterActionsComponent.cs) | Definisi data, tag, dan buffer ECS. | `PlayerHarvesterActions` |
| [RangedWeaponComponents.cs](../Assets/Game/Scripts/Runtime/Components/RangedWeaponComponents.cs) | Definisi data, tag, dan buffer ECS. | `RangedWeaponState`, `RangedWeaponDynamicData`, `RangedWeaponCommonData`, `RangedWeaponDatabaseAccess` |
| [RoundComponent.cs](../Assets/Game/Scripts/Runtime/Components/RoundComponent.cs) | Definisi data, tag, dan buffer ECS. | `RoundComponent`, `PlayerAliveCounts`, `RoundCollectorPlantedComponent`, `GameOverRpcCommand`, `DeactivateSpawnBarriersCommand` |
| [RoundComponentAuthoring.cs](../Assets/Game/Scripts/Runtime/Components/RoundComponentAuthoring.cs) | Definisi data, tag, dan buffer ECS. | `RoundComponentAuthoring`, `RoundComponentBaker` |
| [ScopeComponent.cs](../Assets/Game/Scripts/Runtime/Components/ScopeComponent.cs) | Definisi data, tag, dan buffer ECS. | `ScopeComponent` |
| [ShopCommand.cs](../Assets/Game/Scripts/Runtime/Components/ShopCommand.cs) | Definisi data, tag, dan buffer ECS. | `ShopCommand` |
| [SoundComponent.cs](../Assets/Game/Scripts/Runtime/Components/SoundComponent.cs) | Definisi data, tag, dan buffer ECS. | `SoundRpc`, `SoundRegister`, `SoundEmitter` |
| [SpawnBarrierComponent.cs](../Assets/Game/Scripts/Runtime/Components/SpawnBarrierComponent.cs) | Definisi data, tag, dan buffer ECS. | `SpawnBarrierComponent`, `SpawnFenceMaterialOverride` |
| [SpawnPointBufferComponent.cs](../Assets/Game/Scripts/Runtime/Components/SpawnPointBufferComponent.cs) | Definisi data, tag, dan buffer ECS. | `SpawnPointBufferComponent` |
| [SpawnerComponent.cs](../Assets/Game/Scripts/Runtime/Components/SpawnerComponent.cs) | Definisi data, tag, dan buffer ECS. | `SpawnerComponent` |
| [SpawnerSettingsComponent.cs](../Assets/Game/Scripts/Runtime/Components/SpawnerSettingsComponent.cs) | Definisi data, tag, dan buffer ECS. | `SpawnerSettingsTag`, `AutoRespawnIsEnable` |
| [StuffComponents.cs](../Assets/Game/Scripts/Runtime/Components/StuffComponents.cs) | Definisi data, tag, dan buffer ECS. | `StuffSlot`, `StuffType`, `StuffState`, `StuffDatabaseAccess`, `StuffEntityInHandRef`, `StuffDynamicData`, `IsStuffInHand`, `StuffProcessPending`; +2 tipe lain |
| [TeamSpawnComponent.cs](../Assets/Game/Scripts/Runtime/Components/TeamSpawnComponent.cs) | Definisi data, tag, dan buffer ECS. | `TeamSpawnComponent` |
| [TeamsSideTag.cs](../Assets/Game/Scripts/Runtime/Components/TeamsSideTag.cs) | Definisi data, tag, dan buffer ECS. | `NatifTeamTag`, `CorpoTeamTag`, `NeutralTeamTag` |
| [TracerRoundComponent.cs](../Assets/Game/Scripts/Runtime/Components/TracerRoundComponent.cs) | Definisi data, tag, dan buffer ECS. | `TracerRoundComponent` |

## `Assets/Game/Scripts/Runtime/Components/Character`

Definisi data, tag, dan buffer ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterColliderComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CharacterColliderComponent.cs) | Definisi data, tag, dan buffer ECS. | `CharacterColliderInitEntityTag`, `CharacterColliderComponent` |
| [CharacterColliderDataComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CharacterColliderDataComponent.cs) | Definisi data, tag, dan buffer ECS. | `CharacterColliderDataComponent` |
| [CharacterComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CharacterComponent.cs) | Definisi data, tag, dan buffer ECS. | `CharacterTag`, `CharacterIsEnable`, `CharacterDeadColliderTag`, `EnemyOutlineMaterialOverride`, `CharacterComponent`, `CharacterShootStartPositionDelta`, `CharacterClientAttachedComponent`, `CharacterDefaultStuffName`; +7 tipe lain |
| [CharacterInputComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CharacterInputComponent.cs) | Definisi data, tag, dan buffer ECS. | `CharacterInput` |
| [CharacterRotationComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CharacterRotationComponent.cs) | Definisi data, tag, dan buffer ECS. | `CharacterViewRotation` |
| [CommonCharacterModelComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/CommonCharacterModelComponent.cs) | Definisi data, tag, dan buffer ECS. | `CommonCharacterModelBonesName`, `CommonCharacterModelBonesTransform` |
| [FirstPersonCharacterModelComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/FirstPersonCharacterModelComponent.cs) | Definisi data, tag, dan buffer ECS. | `FirstPersonCharacterModelPrefab`, `FirstPersonCharacterModelReference`, `FPVVisualRecoil` |
| [ThirdPersonCharacterModelComponent.cs](../Assets/Game/Scripts/Runtime/Components/Character/ThirdPersonCharacterModelComponent.cs) | Definisi data, tag, dan buffer ECS. | `ThirdPersonCharacterModelPrefab`, `ThirdPersonCharacterModelReference`, `ThirdPersonCharacterModelBonesTransform` |

## `Assets/Game/Scripts/Runtime/Data/Characters`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterColliderBones.cs](../Assets/Game/Scripts/Runtime/Data/Characters/CharacterColliderBones.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `CharacterColliderBones` |
| [CharacterColliderData.cs](../Assets/Game/Scripts/Runtime/Data/Characters/CharacterColliderData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `CharacterColliderData` |

## `Assets/Game/Scripts/Runtime/Data/Characters/ModelAnimator`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ModelAnimatorData.cs](../Assets/Game/Scripts/Runtime/Data/Characters/ModelAnimator/ModelAnimatorData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `ModelAnimatorData` |

## `Assets/Game/Scripts/Runtime/Data/Equipment`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GrenadeData.cs](../Assets/Game/Scripts/Runtime/Data/Equipment/GrenadeData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `GrenadeTriggerType`, `GrenadeType`, `GrenadeData` |
| [HarvesterData.cs](../Assets/Game/Scripts/Runtime/Data/Equipment/HarvesterData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `HarvesterData` |
| [MeleeWeaponData.cs](../Assets/Game/Scripts/Runtime/Data/Equipment/MeleeWeaponData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `MeleeWeaponData` |
| [RangedWeaponData.cs](../Assets/Game/Scripts/Runtime/Data/Equipment/RangedWeaponData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `RangedWeaponData` |

## `Assets/Game/Scripts/Runtime/Data/Settings`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ClientSettingsData.cs](../Assets/Game/Scripts/Runtime/Data/Settings/ClientSettingsData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `ClientSettingsData` |

## `Assets/Game/Scripts/Runtime/Data/UI`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [UITextureData.cs](../Assets/Game/Scripts/Runtime/Data/UI/UITextureData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `UITextureData`, `UITexture` |

## `Assets/Game/Scripts/Runtime/Data/WeaponMods`

Definisi C# ScriptableObject; instance dipisah di Game/Data.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CrossData.cs](../Assets/Game/Scripts/Runtime/Data/WeaponMods/CrossData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `CrossData` |
| [HandleData.cs](../Assets/Game/Scripts/Runtime/Data/WeaponMods/HandleData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `HandleData` |
| [MagazineData.cs](../Assets/Game/Scripts/Runtime/Data/WeaponMods/MagazineData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `MagazineData` |
| [ScopeData.cs](../Assets/Game/Scripts/Runtime/Data/WeaponMods/ScopeData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `ScopeData` |
| [SilencerData.cs](../Assets/Game/Scripts/Runtime/Data/WeaponMods/SilencerData.cs) | Definisi C# ScriptableObject; instance dipisah di Game/Data. | `SilencerData` |

## `Assets/Game/Scripts/Runtime/Diagnostics`

Kode diagnostik lokal dari sumber yang dipertahankan.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [InputDebug.cs](../Assets/Game/Scripts/Runtime/Diagnostics/InputDebug.cs) | Kode diagnostik lokal dari sumber yang dipertahankan. | `CharacterDebugInput`, `DebugInputSystem`, `CharacterInputDebugSystem` |

## `Assets/Game/Scripts/Runtime/Effects`

Penghubung efek visual senjata dan objektif.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [HarvesterVfxLink.cs](../Assets/Game/Scripts/Runtime/Effects/HarvesterVfxLink.cs) | Penghubung efek visual senjata dan objektif. | `HarvesterVfxLink` |
| [WeaponVfxLink.cs](../Assets/Game/Scripts/Runtime/Effects/WeaponVfxLink.cs) | Penghubung efek visual senjata dan objektif. | `WeaponVfxLink` |

## `Assets/Game/Scripts/Runtime/Enums`

Enumerasi untuk konfigurasi dan state game.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterColliderType.cs](../Assets/Game/Scripts/Runtime/Enums/CharacterColliderType.cs) | Enumerasi untuk konfigurasi dan state game. | `CharacterColliderType` |
| [RoundPhase.cs](../Assets/Game/Scripts/Runtime/Enums/RoundPhase.cs) | Enumerasi untuk konfigurasi dan state game. | `RoundPhase` |
| [TeamSideType.cs](../Assets/Game/Scripts/Runtime/Enums/TeamSideType.cs) | Enumerasi untuk konfigurasi dan state game. | `TeamSideType` |

## `Assets/Game/Scripts/Runtime/Input`

Wrapper Input System yang dihasilkan otomatis.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [DefaultInputSystem.cs](../Assets/Game/Scripts/Runtime/Input/DefaultInputSystem.cs) | Wrapper input otomatis; edit asset inputactions, bukan file ini. | `PlayerActions`, `UIActions`, `HarvesterActions`, `IPlayerActions`, `IUIActions`, `IHarvesterActions` |

## `Assets/Game/Scripts/Runtime/Interfaces`

Kontrak antarbagian kode.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [IRoundManager.cs](../Assets/Game/Scripts/Runtime/Interfaces/IRoundManager.cs) | Kontrak antarbagian kode. | `IRoundManager`, `ScoreChangedComponent` |

## `Assets/Game/Scripts/Runtime/Loading`

Perpindahan scene dan urutan opening/loading.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [LoadManager.cs](../Assets/Game/Scripts/Runtime/Loading/LoadManager.cs) | Perpindahan scene dan urutan opening/loading. | `LoadManager` |
| [LoadUtils.cs](../Assets/Game/Scripts/Runtime/Loading/LoadUtils.cs) | Perpindahan scene dan urutan opening/loading. | `LoadUtils` |
| [SceneLoader.cs](../Assets/Game/Scripts/Runtime/Loading/SceneLoader.cs) | Perpindahan scene dan urutan opening/loading. | `SceneID`, `SceneLoader` |
| [VideoPlayerSceneHandler.cs](../Assets/Game/Scripts/Runtime/Loading/VideoPlayerSceneHandler.cs) | Controller video historis; opening rilis memakai OpeningScreenController. | `VideoPlayerSceneHandler` |

## `Assets/Game/Scripts/Runtime/Noise`

Assembly utilitas noise yang juga dipakai alat Editor.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [FBM.cs](../Assets/Game/Scripts/Runtime/Noise/FBM.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `FBM` |
| [Noise.cs](../Assets/Game/Scripts/Runtime/Noise/Noise.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `Noise` |
| [Perlin.cs](../Assets/Game/Scripts/Runtime/Noise/Perlin.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `Perlin` |
| [Simplex.cs](../Assets/Game/Scripts/Runtime/Noise/Simplex.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `Simplex` |
| [Value.cs](../Assets/Game/Scripts/Runtime/Noise/Value.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `InterpolationType`, `Value` |
| [White.cs](../Assets/Game/Scripts/Runtime/Noise/White.cs) | Assembly utilitas noise yang juga dipakai alat Editor. | `White` |

## `Assets/Game/Scripts/Runtime/Offline`

Pertandingan lokal, bot, navigasi, opsi, dan transisi audio.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [OfflineAudioTransition.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineAudioTransition.cs) | Suspend/resume audio pada transisi yang berat. | `OfflineAudioTransition` |
| [OfflineBotInputSystem.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineBotInputSystem.cs) | Keputusan/input gerak, bidik, tembak, dan reload bot. | `OfflineBotState`, `OfflineBotInputSystem`, `Brain` |
| [OfflineBotObjectiveSystem.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineBotObjectiveSystem.cs) | Perilaku bot terhadap objektif bom. | `OfflineBotObjectiveSystem` |
| [OfflineMatch.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineMatch.cs) | Lifecycle pertandingan, world simulasi, scene gameplay, dan cleanup. | `BotDifficulty`, `OfflineMatch`, `OfflineDriverConstructor` |
| [OfflineMatchOptions.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineMatchOptions.cs) | Validasi, preset, dan penyimpanan preferensi pertandingan. | `OfflineMatchOptions` |
| [OfflineMatchSettings.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineMatchSettings.cs) | Data konfigurasi pertandingan lokal. | `OfflineMatchSettings` |
| [OfflineNavigation.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineNavigation.cs) | Pembuatan dan pencarian navigasi bot pada peta. | `OfflineNavigation`, `NavigationDebugData` |
| [OfflineParticipantSystem.cs](../Assets/Game/Scripts/Runtime/Offline/OfflineParticipantSystem.cs) | Identitas dan peserta pemain/bot dalam simulasi. | `OfflineBot`, `OfflineParticipantSystem` |
| [OfflinePlayerValidation.cs](../Assets/Game/Scripts/Runtime/Offline/OfflinePlayerValidation.cs) | Pemeriksaan standalone yang hanya aktif dengan argumen eksplisit. | `OfflinePlayerValidation` |

## `Assets/Game/Scripts/Runtime/Session`

Pengelolaan sesi dan progres pertandingan.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GameManager.cs](../Assets/Game/Scripts/Runtime/Session/GameManager.cs) | Meneruskan permintaan bermain dari menu ke OfflineMatch. | `GameManager` |
| [SessionData.cs](../Assets/Game/Scripts/Runtime/Session/SessionData.cs) | State/progres loading yang digunakan binding UI. | `SessionData`, `LoadingSteps` |

## `Assets/Game/Scripts/Runtime/Simulation/Client`

World client/server lokal, bootstrap, smoothing, dan RPC internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ClientAuthoring.cs](../Assets/Game/Scripts/Runtime/Simulation/Client/ClientAuthoring.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ClientAuthoring`, `Baker` |
| [ClientComponent.cs](../Assets/Game/Scripts/Runtime/Simulation/Client/ClientComponent.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ClientCharacterAttached` |
| [ClientPredictionSmoothingSystem.cs](../Assets/Game/Scripts/Runtime/Simulation/Client/ClientPredictionSmoothingSystem.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ClientPredictionSmoothingSystem`, `ClientTransformSmoothingAction`, `DefaultStaticUserParams`, `MaxDistKey`, `DeltaKey` |
| [ClientSystem.cs](../Assets/Game/Scripts/Runtime/Simulation/Client/ClientSystem.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ClientMessageRpcCommand`, `ClientSystem` |
| [GoInGameClientSystem.cs](../Assets/Game/Scripts/Runtime/Simulation/Client/GoInGameClientSystem.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `GoInGameCommand`, `GoInGameClientSystem` |

## `Assets/Game/Scripts/Runtime/Simulation/Common`

World client/server lokal, bootstrap, smoothing, dan RPC internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CommonAutoBootstrap.cs](../Assets/Game/Scripts/Runtime/Simulation/Common/CommonAutoBootstrap.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `CommonAutoConnect` |
| [CommonHealth.cs](../Assets/Game/Scripts/Runtime/Simulation/Common/CommonHealth.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `MaxHealthComponent`, `CurrentHealthComponent`, `DamageBufferElement`, `DamageThisTickCommand`, `DamageIndicator`, `KillDamageIndicator`, `HasNoHealthTag`, `CalculateFrameDamageSystem`; +7 tipe lain |
| [NetcodeConnectionEvent.cs](../Assets/Game/Scripts/Runtime/Simulation/Common/NetcodeConnectionEvent.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `NetcodeConnectionEvent` |

## `Assets/Game/Scripts/Runtime/Simulation/RPC`

World client/server lokal, bootstrap, smoothing, dan RPC internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [RpcUtils.cs](../Assets/Game/Scripts/Runtime/Simulation/RPC/RpcUtils.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `RpcUtils` |
| [ServerRpcCommand.cs](../Assets/Game/Scripts/Runtime/Simulation/RPC/ServerRpcCommand.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | Atribut/utility tanpa tipe bernama. |

## `Assets/Game/Scripts/Runtime/Simulation/Server`

World client/server lokal, bootstrap, smoothing, dan RPC internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GoInGameServerSystem.cs](../Assets/Game/Scripts/Runtime/Simulation/Server/GoInGameServerSystem.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `GoInGameServerSystem` |
| [ServerBuildOptimization.cs](../Assets/Game/Scripts/Runtime/Simulation/Server/ServerBuildOptimization.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ServerBuildOptimization` |
| [ServerConsole.cs](../Assets/Game/Scripts/Runtime/Simulation/Server/ServerConsole.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ServerConsole`, `LogType` |
| [ServerData.cs](../Assets/Game/Scripts/Runtime/Simulation/Server/ServerData.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ServerDataComponent`, `ServerDataSystem` |

## `Assets/Game/Scripts/Runtime/Simulation/Utilities`

World client/server lokal, bootstrap, smoothing, dan RPC internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [NetworkUtils.cs](../Assets/Game/Scripts/Runtime/Simulation/Utilities/NetworkUtils.cs) | World client/server lokal, bootstrap, smoothing, dan RPC internal. | `ClientConnectionState`, `ClientTransportHelper` |

## `Assets/Game/Scripts/Runtime/Systems/Character`

Sistem gerak, input, model, collider, dan presentasi karakter.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterAnimationSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterAnimationSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `ServerCharacterAnimationSystem` |
| [CharacterColliderSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterColliderSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CommonCharacterColliderSystem` |
| [CharacterEnvironementColliderSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterEnvironementColliderSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CharacterEnvironementColliderSystem` |
| [CharacterInputSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterInputSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CharacterInputSystem` |
| [CharacterInstantiateStuffSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterInstantiateStuffSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CharacterInstantiateStuffSystem` |
| [CharacterMovementSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterMovementSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CharacterMovementSystem`, `CharacterMovementJob` |
| [CharacterRotationSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/CharacterRotationSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `CommonCharacterRotationSystem` |
| [EnemyOutlineSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/EnemyOutlineSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `EnemyOutlineSystem` |
| [FirstPersonCharacterModelSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/FirstPersonCharacterModelSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `ClientFirstPersonCharacterModelSystem` |
| [ThirdPersonCharacterModelSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Character/ThirdPersonCharacterModelSystem.cs) | Sistem gerak, input, model, collider, dan presentasi karakter. | `ServerThirdPersonCharacterModelSystem`, `ClientThirdPersonCharacterModelSystem` |

## `Assets/Game/Scripts/Runtime/Systems/Combat`

Sistem damage, kematian, granat, proyektil, dan recoil.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GrenadeEffectsSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/GrenadeEffectsSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `GrenadeEffectsSystem` |
| [GrenadeSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/GrenadeSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `GrenadeVFXType`, `GrenadeVFXCommand`, `GrenadeSmokeFade`, `GrenadeVFXSystem`, `GrenadeSystem`, `GrenadeThrowSystem`, `GrenadeCollisionJob` |
| [HitSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/HitSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `DestroyTag`, `Lifetime`, `HitSystem`, `TracerData` |
| [OnDeathSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/OnDeathSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `OnDeathSystem` |
| [TracerRoundSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/TracerRoundSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `TracerRoundSystem` |
| [VisualRecoilSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Combat/VisualRecoilSystem.cs) | Sistem damage, kematian, granat, proyektil, dan recoil. | `VisualRecoilSystem` |

## `Assets/Game/Scripts/Runtime/Systems/Economy`

Sistem uang dan pembelian perlengkapan.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [MoneyRelatedSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Economy/MoneyRelatedSystem.cs) | Sistem uang dan pembelian perlengkapan. | `MoneyRelatedSystem` |
| [ShopSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Economy/ShopSystem.cs) | Sistem uang dan pembelian perlengkapan. | `ShopSystem` |

## `Assets/Game/Scripts/Runtime/Systems/GameResources`

Sistem pembaruan gameplay ECS.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GameResourcesInstantiateSystem.cs](../Assets/Game/Scripts/Runtime/Systems/GameResources/GameResourcesInstantiateSystem.cs) | Sistem pembaruan gameplay ECS. | `InstanciateEntityStuffSystem`, `ProcessPendingStuffSystem` |

## `Assets/Game/Scripts/Runtime/Systems/Harvester`

Sistem plant, defuse, dan state objektif bom.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [HarvesterDefuseSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Harvester/HarvesterDefuseSystemServer.cs) | Sistem plant, defuse, dan state objektif bom. | `HarvesterDefuseSystemServer` |
| [HarvesterPlantingSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Harvester/HarvesterPlantingSystemServer.cs) | Sistem plant, defuse, dan state objektif bom. | `HarvesterPlantingSystemServer` |
| [HarvesterSystemClient.cs](../Assets/Game/Scripts/Runtime/Systems/Harvester/HarvesterSystemClient.cs) | Sistem plant, defuse, dan state objektif bom. | `HarvesterSystemClient` |
| [HarvesterSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Harvester/HarvesterSystemServer.cs) | Sistem plant, defuse, dan state objektif bom. | `HarvesterSystemServer` |

## `Assets/Game/Scripts/Runtime/Systems/Presentation`

Sistem kamera, animasi, suara, dan pengaturan presentasi.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [AnimationSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Presentation/AnimationSystem.cs) | Sistem kamera, animasi, suara, dan pengaturan presentasi. | `ServerClearAnimationBufferSystem`, `ServerAnimationSystem`, `ClientAnimationSystem` |
| [CameraSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Presentation/CameraSystem.cs) | Sistem kamera, animasi, suara, dan pengaturan presentasi. | `CameraSystem` |
| [ClientSettingsSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Presentation/ClientSettingsSystem.cs) | Sistem kamera, animasi, suara, dan pengaturan presentasi. | `ClientSettingsSystem` |
| [SoundSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Presentation/SoundSystem.cs) | Sistem kamera, animasi, suara, dan pengaturan presentasi. | `SoundBankSystemClient`, `SoundPlayRPCSystemClient`, `SoundMainMenuVolumeSystemClient` |

## `Assets/Game/Scripts/Runtime/Systems/Round`

Sistem ronde, tim, peserta, spawn, dan trigger objektif.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CountPlayersSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Round/CountPlayersSystemServer.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `CountPlayersSystemServer` |
| [RoundSystemClient.cs](../Assets/Game/Scripts/Runtime/Systems/Round/RoundSystemClient.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `RoundSystemClient`, `TeamWinRoundArgs`, `RequestRoundDataRpcCommand` |
| [RoundSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Round/RoundSystemServer.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `RoundSystemServer`, `NewRoundTag`, `VictoryRpcCommand`, `ChangePhaseRpcCommand`, `UpdateRoundDataRpcCommand` |
| [SpawnerSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Round/SpawnerSystem.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `WaitForRespawnTag`, `ResetStuffTag`, `ShouldBeDropped`, `ShouldBeDestroyed`, `ShouldEmptyInventory`, `OnDieSystem`, `OnDieJob`, `RespawnSystem` |
| [TeamChoiceSystemClient.cs](../Assets/Game/Scripts/Runtime/Systems/Round/TeamChoiceSystemClient.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `TeamChoiceComponent`, `MessageToServer`, `TeamChoiceSystemClient` |
| [TeamChoiceSystemServer.cs](../Assets/Game/Scripts/Runtime/Systems/Round/TeamChoiceSystemServer.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `ChooseTeamRpc`, `TeamChoiceSystemServer` |
| [TriggerResolutionServerSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Round/TriggerResolutionServerSystem.cs) | Sistem ronde, tim, peserta, spawn, dan trigger objektif. | `TriggerResolutionServerSystem`, `SiteTriggerJob` |

## `Assets/Game/Scripts/Runtime/Systems/Simulation`

Sistem pengelolaan simulasi dan pesan internal.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [DestroyTagSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Simulation/DestroyTagSystem.cs) | Sistem pengelolaan simulasi dan pesan internal. | `DestroyTagSystem` |
| [RpcLoggerSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Simulation/RpcLoggerSystem.cs) | Sistem pengelolaan simulasi dan pesan internal. | Atribut/utility tanpa tipe bernama. |
| [ServerRpcCommandSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Simulation/ServerRpcCommandSystem.cs) | Sistem pengelolaan simulasi dan pesan internal. | Atribut/utility tanpa tipe bernama. |
| [ServerSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Simulation/ServerSystem.cs) | Sistem pengelolaan simulasi dan pesan internal. | `ServerMessageRpcCommand`, `ClientComponent`, `ServerSystem` |

## `Assets/Game/Scripts/Runtime/Systems/Stuff`

Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [DropStuffSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/DropStuffSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `DropStuffSystem`, `StuffDropedCleanup` |
| [PickStuffSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/PickStuffSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `PickStuffSystem` |
| [RangedWeaponSystemClient.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/RangedWeaponSystemClient.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | Atribut/utility tanpa tipe bernama. |
| [ReloadSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/ReloadSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `RangedWeaponReloadSystem` |
| [ShootSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/ShootSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `HasHitComponent`, `ShootSystem` |
| [StrikeSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/StrikeSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `StrikeSystem` |
| [StuffSystemClient.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/StuffSystemClient.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `StuffSystemClient` |
| [StuffUtilsSystem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/StuffUtilsSystem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `EquipStuffQueue`, `UnequipStuffQueue`, `EquipStuffSystem`, `UnequipStuffSystem` |
| [SwitchStuffSytem.cs](../Assets/Game/Scripts/Runtime/Systems/Stuff/SwitchStuffSytem.cs) | Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan. | `SwitchStuffSystem` |

## `Assets/Game/Scripts/Runtime/Systems/UI`

Penghubung state simulasi dengan HUD dan UI.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ErrorWindowCallerSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/ErrorWindowCallerSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `ErrorWindowCallerSystem`, `ErrorMessage` |
| [HarvesterDefusingLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/HarvesterDefusingLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `HarvesterDefusingLinkSystem`, `HarversterDefuseRunning` |
| [HarvesterPlantingLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/HarvesterPlantingLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `HarvesterPlantingLinkSystem`, `HarversterPlantRunning` |
| [InGameHUDSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/InGameHUDSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `InGameHUDSystem`, `HealthArgs`, `AmmoArgs`, `MoneyArgs`, `HitArgs`, `FlashGrenadeArgs`, `SmokeGrenadeArgs`, `PositionArgs`; +1 tipe lain |
| [MinimapTeamLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/MinimapTeamLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `MinimapTeamLinkSystem`, `MinimapTeamArgs` |
| [RoundManagerLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/RoundManagerLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `RoundManagerLinkSystem` |
| [SettingsLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/SettingsLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `SettingsLinkSystem` |
| [WeaponListLinkSystem.cs](../Assets/Game/Scripts/Runtime/Systems/UI/WeaponListLinkSystem.cs) | Penghubung state simulasi dengan HUD dan UI. | `WeaponListLinkSystem`, `StuffListChangeEventArgs`, `StuffIdEventArgs` |

## `Assets/Game/Scripts/Runtime/UI`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [UICentralController.cs](../Assets/Game/Scripts/Runtime/UI/UICentralController.cs) | Controller tampilan UI Toolkit. | `IUIController`, `UICentralController`, `UIState` |

## `Assets/Game/Scripts/Runtime/UI/Branding`

Identitas STEEL PROTOCOL yang dibagikan opening dan menu.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [GameIdentity.cs](../Assets/Game/Scripts/Runtime/UI/Branding/GameIdentity.cs) | Nama produk, tagline, executable, dan label identitas bersama. | `GameIdentity` |

## `Assets/Game/Scripts/Runtime/UI/ConstantHUD`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [HUDController.cs](../Assets/Game/Scripts/Runtime/UI/ConstantHUD/HUDController.cs) | Controller HUD selama pertandingan. | `HUDController`, `WeaponMap` |

## `Assets/Game/Scripts/Runtime/UI/ErrorsMenu`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ErrorWindowController.cs](../Assets/Game/Scripts/Runtime/UI/ErrorsMenu/ErrorWindowController.cs) | Controller tampilan UI Toolkit. | `ErrorWindowController` |

## `Assets/Game/Scripts/Runtime/UI/Loading`

Opening robot, progres pemuatan menu, dan UI loading pertandingan.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [LoadingScreen.cs](../Assets/Game/Scripts/Runtime/UI/Loading/LoadingScreen.cs) | Opening robot, progres pemuatan menu, dan UI loading pertandingan. | `LoadingScreen` |
| [OpeningScreenController.cs](../Assets/Game/Scripts/Runtime/UI/Loading/OpeningScreenController.cs) | Pemuatan menu nyata, progres opening, dan input Skip. | `OpeningScreenController` |

## `Assets/Game/Scripts/Runtime/UI/MainMenu`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [MainMenuController.cs](../Assets/Game/Scripts/Runtime/UI/MainMenu/MainMenuController.cs) | Interaksi menu dan konfigurasi custom bot match. | `MainMenuController` |
| [MenuRobotDisplay.cs](../Assets/Game/Scripts/Runtime/UI/MainMenu/MenuRobotDisplay.cs) | Gerakan idle ringan unit robot yang dipamerkan di menu. | `MenuRobotDisplay` |

## `Assets/Game/Scripts/Runtime/UI/PauseMenu`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [PauseMenuController.cs](../Assets/Game/Scripts/Runtime/UI/PauseMenu/PauseMenuController.cs) | Controller tampilan UI Toolkit. | `PauseMenuController` |

## `Assets/Game/Scripts/Runtime/UI/SettingsMenu`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [SettingsMenuController.cs](../Assets/Game/Scripts/Runtime/UI/SettingsMenu/SettingsMenuController.cs) | Controller tampilan UI Toolkit. | `NameCommand`, `SettingsMenuController` |

## `Assets/Game/Scripts/Runtime/UI/StuffShop`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [ShopController.cs](../Assets/Game/Scripts/Runtime/UI/StuffShop/ShopController.cs) | Controller tampilan UI Toolkit. | `ShopController` |

## `Assets/Game/Scripts/Runtime/UI/TeamChoice`

Controller tampilan UI Toolkit.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [TeamChoiceController.cs](../Assets/Game/Scripts/Runtime/UI/TeamChoice/TeamChoiceController.cs) | Controller tampilan UI Toolkit. | `TeamChoiceController` |

## `Assets/Game/Scripts/Runtime/Utilities`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [AnimationUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/AnimationUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `AnimationUtils` |
| [AssemblyInfo.cs](../Assets/Game/Scripts/Runtime/Utilities/AssemblyInfo.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | Atribut/utility tanpa tipe bernama. |
| [DeleteCloneName.cs](../Assets/Game/Scripts/Runtime/Utilities/DeleteCloneName.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `Dellet` |
| [FreezeRotationSystem.cs](../Assets/Game/Scripts/Runtime/Utilities/FreezeRotationSystem.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `FreezeAllRotationTag`, `FreezeAllRotationSystem` |
| [HarvesterActions.cs](../Assets/Game/Scripts/Runtime/Utilities/HarvesterActions.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `HarvesterActions` |
| [MathUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/MathUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `MathUtils` |
| [PlayerHelpers.cs](../Assets/Game/Scripts/Runtime/Utilities/PlayerHelpers.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `PlayerHelpers`, `AliveCounts`, `GlobalTeamCount`, `TeamList` |
| [RoundUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/RoundUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `RoundUtils` |
| [Singleton.cs](../Assets/Game/Scripts/Runtime/Utilities/Singleton.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `Singleton`, `MasterSingleton` |
| [SoundUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/SoundUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `SoundMapping`, `SoundGroupMapping`, `SoundUtils` |
| [SpawnerUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/SpawnerUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `SpawnerUtils` |

## `Assets/Game/Scripts/Runtime/Utilities/Character`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [CharacterColliderUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/CharacterColliderUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `CharacterColliderUtils` |
| [CharacterShootUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/CharacterShootUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `CharacterShootUtils` |
| [CharacterUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/CharacterUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `CharacterUtils` |
| [CharacterViewUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/CharacterViewUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `CharacterViewUtils` |
| [CommonCharacterModelUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/CommonCharacterModelUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `CommonCharacterModelUtils` |
| [FirstPersonCharacterModelUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/FirstPersonCharacterModelUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `FirstPersonCharacterModelUtils` |
| [ThirdPersonCharacterModelUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Character/ThirdPersonCharacterModelUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `ThirdPersonCharacterModelUtils` |

## `Assets/Game/Scripts/Runtime/Utilities/Stuff`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [StuffUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/Stuff/StuffUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `StuffUtils` |

## `Assets/Game/Scripts/Runtime/Utilities/UI`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [SafeVisualElement.cs](../Assets/Game/Scripts/Runtime/Utilities/UI/SafeVisualElement.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `SafeVisualElement` |
| [UIDocumentUtils.cs](../Assets/Game/Scripts/Runtime/Utilities/UI/UIDocumentUtils.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `UIDocumentUtils` |

## `Assets/Game/Scripts/Runtime/Utilities/UI/SinglePurposeElement`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [DamageIndicatorElement.cs](../Assets/Game/Scripts/Runtime/Utilities/UI/SinglePurposeElement/DamageIndicatorElement.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `DamageIndicatorElement` |
| [KillFeedElement.cs](../Assets/Game/Scripts/Runtime/Utilities/UI/SinglePurposeElement/KillFeedElement.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `KillFeedElement` |

## `Assets/Game/Scripts/Runtime/Utilities/UI/VisualElements`

Fungsi dan komponen pendukung yang digunakan bersama.

| File | Peran | Tipe yang dideklarasikan |
| --- | --- | --- |
| [FloatingWindow.cs](../Assets/Game/Scripts/Runtime/Utilities/UI/VisualElements/FloatingWindow.cs) | Fungsi dan komponen pendukung yang digunakan bersama. | `FloatingWindow` |

