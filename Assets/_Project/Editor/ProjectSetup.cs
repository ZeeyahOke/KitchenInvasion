using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEditor.XR.ARKit;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace KitchenInvasion.EditorTools
{
    public static class ProjectSetup
    {
        const string Root = "Assets/_Project/";
        const string ScenePath = Root + "Scenes/Main.unity";

        static readonly Color Cream = Hex("FFF4E0");
        static readonly Color Tomato = Hex("E94F37");
        static readonly Color Mustard = Hex("F6C344");
        static readonly Color Basil = Hex("3E9B4F");
        static readonly Color Brown = Hex("3B2A20");

        static AddAndRemoveRequest packageRequest;
        static TMP_FontAsset font;
        static Sprite rounded;

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }

        [MenuItem("Kitchen Invasion/1 Install Packages")]
        public static void InstallPackages()
        {
            string[] add = { "com.unity.xr.arfoundation", "com.unity.xr.arkit", "com.unity.xr.management", "com.unity.inputsystem", "com.unity.ugui" };
            string[] remove = { };
            packageRequest = Client.AddAndRemove(add, remove);
            EditorApplication.update += WaitForPackages;
        }

        static void WaitForPackages()
        {
            if (!packageRequest.IsCompleted) return;
            EditorApplication.update -= WaitForPackages;
            if (packageRequest.Status == StatusCode.Success) Debug.Log("KI_SETUP packages installed");
            else Debug.LogError("KI_SETUP package error " + packageRequest.Error.message);
        }

        [MenuItem("Kitchen Invasion/2 Configure Project")]
        public static void ConfigureProject()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            PlayerSettings.companyName = "FawziyyahOke";
            PlayerSettings.productName = "Kitchen Invasion";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.fawziyyahoke.kitcheninvasion");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.fawziyyahoke.kitcheninvasion");
            PlayerSettings.iOS.cameraUsageDescription = "Kitchen Invasion uses the camera for AR";
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;
            PlayerSettings.bundleVersion = "1.0";

            SetupXR();
            AddARBackgroundFeature();
            ImportTMPEssentials();
            DeleteTemplateAssets();
            AssetDatabase.SaveAssets();
            Debug.Log("KI_SETUP project configured");
        }

        static void SetupXR()
        {
            XRGeneralSettingsPerBuildTarget perTarget;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out perTarget);
            if (perTarget == null)
            {
                System.IO.Directory.CreateDirectory("Assets/XR");
                perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perTarget, true);
            }
            AssignLoader(perTarget, BuildTargetGroup.iOS, "UnityEngine.XR.ARKit.ARKitLoader");
            AssignLoader(perTarget, BuildTargetGroup.Standalone, "UnityEngine.XR.Simulation.SimulationLoader");

            ARKitSettings arkit = ARKitSettings.GetOrCreateSettings();
            if (!AssetDatabase.Contains(arkit))
            {
                System.IO.Directory.CreateDirectory("Assets/XR/Settings");
                AssetDatabase.CreateAsset(arkit, "Assets/XR/Settings/ARKitSettings.asset");
            }
            arkit.requirement = ARKitSettings.Requirement.Required;
            arkit.faceTracking = false;
            ARKitSettings.currentSettings = arkit;
            EditorUtility.SetDirty(arkit);
        }

        static void AssignLoader(XRGeneralSettingsPerBuildTarget perTarget, BuildTargetGroup group, string loader)
        {
            if (!perTarget.HasManagerSettingsForBuildTarget(group))
                perTarget.CreateDefaultManagerSettingsForBuildTarget(group);
            XRGeneralSettings settings = perTarget.SettingsForBuildTarget(group);
            settings.InitManagerOnStart = true;
            bool ok = XRPackageMetadataStore.AssignLoader(settings.Manager, loader, group);
            Debug.Log("KI_SETUP loader " + loader + " for " + group + " : " + ok);
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(perTarget);
        }

        static void AddARBackgroundFeature()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:UniversalRendererData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/")) continue;
                UniversalRendererData data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                bool has = false;
                foreach (ScriptableRendererFeature f in data.rendererFeatures)
                    if (f is ARBackgroundRendererFeature) has = true;
                if (has) continue;
                ARBackgroundRendererFeature feature = ScriptableObject.CreateInstance<ARBackgroundRendererFeature>();
                feature.name = "ARBackgroundRendererFeature";
                AssetDatabase.AddObjectToAsset(feature, data);
                data.rendererFeatures.Add(feature);
                SerializedObject so = new SerializedObject(data);
                SerializedProperty map = so.FindProperty("m_RendererFeatureMap");
                map.arraySize++;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string g, out long localId);
                map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(data);
                Debug.Log("KI_SETUP added AR background feature to " + path);
            }
        }

        static void ImportTMPEssentials()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0) return;
            string[] found = System.IO.Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", System.IO.SearchOption.AllDirectories);
            if (found.Length > 0) UnityEditor.AssetPackage.Package.Import(found[0], false);
        }

        static void DeleteTemplateAssets()
        {
            AssetDatabase.DeleteAsset("Assets/TutorialInfo");
            AssetDatabase.DeleteAsset("Assets/Readme.asset");
            AssetDatabase.DeleteAsset("Assets/Scenes");
            AssetDatabase.DeleteAsset("Assets/Settings/SampleSceneProfile.asset");
        }

        [MenuItem("Kitchen Invasion/3 Build Content")]
        public static void BuildContent()
        {
            SetupImporters();
            font = CreateFont();
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Textures/ui_rounded.png");

            Material tomatoMat = FoodMaterial("M_Tomato", "colormap.png");
            Material pepperMat = FoodMaterial("M_Pepper", "colormap_pepper.png");
            Material potMat = FoodMaterial("M_Pot", "colormap.png");
            Material eyeMat = LitMaterial("M_Eye", Hex("1E1410"));
            Material playerBulletMat = UnlitMaterial("M_PlayerBullet", Hex("FFE066"));
            Material pepperBulletMat = UnlitMaterial("M_PepperBullet", Hex("7CFC5A"));
            Material planeMat = PlaneMaterial();

            DifficultySettings easy = Difficulty("Easy", 2.5f, 1f, 150);
            DifficultySettings hard = Difficulty("Hard", 1.5f, 1.3f, 100);

            GameObject planePrefab = CreatePlanePrefab(planeMat);
            GameObject potPrefab = CreateModelPrefab("CookingPot", "pot.obj", potMat);
            Bullet playerBullet = CreateBulletPrefab("PlayerBullet", playerBulletMat, 0.03f, 1, 2f, true);
            Bullet pepperBullet = CreateBulletPrefab("PepperBullet", pepperBulletMat, 0.045f, 5, 4f, false);
            MeleeEnemy tomato = CreateTomatoPrefab(tomatoMat, eyeMat);
            ShooterEnemy pepper = CreatePepperPrefab(pepperMat, eyeMat);

            AssetDatabase.SaveAssets();
            BuildScene(planePrefab, potPrefab, playerBullet, pepperBullet, tomato, pepper, easy, hard);
            AssetDatabase.SaveAssets();
            Debug.Log("KI_SETUP content built");
        }

        static void SetupImporters()
        {
            TextureImporter tile = (TextureImporter)AssetImporter.GetAtPath(Root + "Textures/kitchen_tile_name.png");
            tile.wrapMode = TextureWrapMode.Repeat;
            tile.alphaIsTransparency = true;
            tile.anisoLevel = 4;
            tile.SaveAndReimport();

            TextureImporter ui = (TextureImporter)AssetImporter.GetAtPath(Root + "Textures/ui_rounded.png");
            ui.textureType = TextureImporterType.Sprite;
            ui.spriteImportMode = SpriteImportMode.Single;
            ui.spriteBorder = new Vector4(26, 26, 26, 26);
            ui.alphaIsTransparency = true;
            ui.mipmapEnabled = false;
            ui.SaveAndReimport();

            foreach (string model in new[] { "tomato.obj", "pepper.obj", "pot.obj" })
            {
                ModelImporter mi = (ModelImporter)AssetImporter.GetAtPath(Root + "Models/" + model);
                mi.materialImportMode = ModelImporterMaterialImportMode.None;
                mi.importAnimation = false;
                mi.SaveAndReimport();
            }
        }

        static TMP_FontAsset CreateFont()
        {
            string path = Root + "Fonts/LilitaOne SDF.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) return existing;
            Font ttf = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/LilitaOne-Regular.ttf");
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            AssetDatabase.CreateAsset(asset, path);
            asset.atlasTextures[0].name = "LilitaOne Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = "LilitaOne Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.TryAddCharacters(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz|");
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static Material SaveMaterial(Material m, string name)
        {
            string path = Root + "Materials/" + name + ".mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Material FoodMaterial(string name, string texture)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/" + texture));
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Smoothness", 0.35f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return SaveMaterial(m, name);
        }

        static Material LitMaterial(string name, Color color)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", 0.6f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return SaveMaterial(m, name);
        }

        static Material UnlitMaterial(string name, Color color)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", color);
            return SaveMaterial(m, name);
        }

        static Material PlaneMaterial()
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/kitchen_tile_name.png"));
            m.SetTextureScale("_BaseMap", new Vector2(1.5f, 1.5f));
            m.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.9f));
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Cull", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            return SaveMaterial(m, "M_KitchenPlane");
        }

        static DifficultySettings Difficulty(string name, float interval, float speed, int health)
        {
            string path = Root + "ScriptableObjects/" + name + ".asset";
            DifficultySettings d = AssetDatabase.LoadAssetAtPath<DifficultySettings>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<DifficultySettings>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.difficultyName = name;
            d.spawnInterval = interval;
            d.enemySpeedMultiplier = speed;
            d.playerMaxHealth = health;
            EditorUtility.SetDirty(d);
            return d;
        }

        static T SavePrefab<T>(GameObject go) where T : Component
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefabs/" + go.name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<T>();
        }

        static T LoadPrefab<T>(string name) where T : Component
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/" + name + ".prefab").GetComponent<T>();
        }

        static void Set(Object target, string field, object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogError("KI_SETUP missing field " + field + " on " + target.name); return; }
            if (value is Object o)
            {
                p.objectReferenceValue = o;
                if (o == null) Debug.LogError("KI_SETUP null reference for " + field);
            }
            else if (value is int i) p.intValue = i;
            else if (value is float f) p.floatValue = f;
            else if (value is bool b) p.boolValue = b;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreatePlanePrefab(Material planeMat)
        {
            GameObject go = new GameObject("KitchenPlane");
            go.AddComponent<ARPlane>();
            go.AddComponent<MeshFilter>();
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = planeMat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            go.AddComponent<ARPlaneMeshVisualizer>();
            return SavePrefab<Transform>(go).gameObject;
        }

        static Mesh LoadMesh(string model)
        {
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(Root + "Models/" + model))
                if (o is Mesh m) return m;
            Debug.LogError("KI_SETUP no mesh in " + model);
            return null;
        }

        static GameObject AddModel(GameObject root, string model, Material mat)
        {
            GameObject child = new GameObject("Model");
            child.transform.SetParent(root.transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = LoadMesh(model);
            child.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return child;
        }

        static GameObject CreateModelPrefab(string name, string model, Material mat)
        {
            GameObject go = new GameObject(name);
            AddModel(go, model, mat);
            return SavePrefab<Transform>(go).gameObject;
        }

        static Bullet CreateBulletPrefab(string name, Material mat, float size, int damage, float life, bool byPlayer)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            go.GetComponent<SphereCollider>().isTrigger = true;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Bullet b = go.AddComponent<Bullet>();
            Set(b, "damage", damage);
            Set(b, "lifetime", life);
            Set(b, "firedByPlayer", byPlayer);
            return SavePrefab<Bullet>(go);
        }

        static void AddFace(GameObject root, Material eyeMat, float height, float front, float spread, float size)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.DestroyImmediate(eye.GetComponent<Collider>());
                eye.name = "Eye";
                eye.transform.SetParent(root.transform, false);
                eye.transform.localPosition = new Vector3(side * spread, height, front);
                eye.transform.localScale = Vector3.one * size;
                eye.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;

                GameObject brow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(brow.GetComponent<Collider>());
                brow.name = "AngryBrow";
                brow.transform.SetParent(root.transform, false);
                brow.transform.localPosition = new Vector3(side * spread, height + size * 1.1f, front);
                brow.transform.localRotation = Quaternion.Euler(0f, 0f, side * 25f);
                brow.transform.localScale = new Vector3(size * 1.5f, size * 0.35f, size * 0.35f);
                brow.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;
            }
        }

        static void AddEnemyPhysics(GameObject go, Collider col)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        static MeleeEnemy CreateTomatoPrefab(Material mat, Material eyeMat)
        {
            GameObject go = new GameObject("Tomato");
            AddModel(go, "tomato.obj", mat);
            AddFace(go, eyeMat, 0.14f, 0.13f, 0.05f, 0.04f);
            SphereCollider col = go.AddComponent<SphereCollider>();
            col.center = new Vector3(0f, 0.125f, 0f);
            col.radius = 0.14f;
            AddEnemyPhysics(go, col);
            MeleeEnemy e = go.AddComponent<MeleeEnemy>();
            Set(e, "maxHealth", 3);
            Set(e, "moveSpeed", 0.3f);
            Set(e, "scoreValue", 10);
            return SavePrefab<MeleeEnemy>(go);
        }

        static ShooterEnemy CreatePepperPrefab(Material mat, Material eyeMat)
        {
            GameObject go = new GameObject("Pepper");
            AddModel(go, "pepper.obj", mat);
            AddFace(go, eyeMat, 0.17f, 0.035f, 0.02f, 0.024f);
            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.135f, 0f);
            col.radius = 0.06f;
            col.height = 0.27f;
            AddEnemyPhysics(go, col);
            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.2f, 0.08f);
            ShooterEnemy e = go.AddComponent<ShooterEnemy>();
            Set(e, "maxHealth", 2);
            Set(e, "moveSpeed", 0.25f);
            Set(e, "scoreValue", 20);
            Set(e, "muzzle", muzzle.transform);
            return SavePrefab<ShooterEnemy>(go);
        }

        static void BuildScene(GameObject planePrefab, GameObject potPrefab, Bullet playerBullet, Bullet pepperBullet,
            MeleeEnemy tomato, ShooterEnemy pepper, DifficultySettings easy, DifficultySettings hard)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            playerBullet = LoadPrefab<Bullet>("PlayerBullet");
            pepperBullet = LoadPrefab<Bullet>("PepperBullet");
            tomato = LoadPrefab<MeleeEnemy>("Tomato");
            pepper = LoadPrefab<ShooterEnemy>("Pepper");
            planePrefab = LoadPrefab<Transform>("KitchenPlane").gameObject;
            potPrefab = LoadPrefab<Transform>("CookingPot").gameObject;
            easy = AssetDatabase.LoadAssetAtPath<DifficultySettings>(Root + "ScriptableObjects/Easy.asset");
            hard = AssetDatabase.LoadAssetAtPath<DifficultySettings>(Root + "ScriptableObjects/Hard.asset");

            GameObject light = new GameObject("Directional Light");
            Light l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
            l.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            new GameObject("AR Session", typeof(ARSession), typeof(ARInputManager));

            EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)");
            XROrigin origin = Object.FindAnyObjectByType<XROrigin>();
            GameObject originGo = origin.gameObject;
            ARPlaneManager planes = originGo.AddComponent<ARPlaneManager>();
            planes.planePrefab = planePrefab;
            planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            ARRaycastManager raycaster = originGo.AddComponent<ARRaycastManager>();

            GameObject cam = origin.Camera.gameObject;
            SphereCollider playerCollider = cam.AddComponent<SphereCollider>();
            playerCollider.isTrigger = true;
            playerCollider.radius = 0.2f;
            Rigidbody camBody = cam.AddComponent<Rigidbody>();
            camBody.isKinematic = true;
            camBody.useGravity = false;
            PlayerHealth health = cam.AddComponent<PlayerHealth>();
            PlayerShooter shooter = cam.AddComponent<PlayerShooter>();

            PotPlacer placer = originGo.AddComponent<PotPlacer>();
            Set(placer, "raycastManager", raycaster);
            Set(placer, "planeManager", planes);
            Set(placer, "potPrefab", potPrefab);
            Set(placer, "playerCamera", cam.transform);

            ObjectPool playerPool = CreatePool("Player Bullet Pool", playerBullet, 20);
            ObjectPool pepperPool = CreatePool("Pepper Bullet Pool", pepperBullet, 30);
            Set(shooter, "bulletPool", playerPool);
            Set(shooter, "potPlacer", placer);

            GameObject spawnerGo = new GameObject("Enemy Spawner");
            EnemyFactory factory = spawnerGo.AddComponent<EnemyFactory>();
            Set(factory, "tomatoPrefab", tomato);
            Set(factory, "pepperPrefab", pepper);
            Set(factory, "pepperBulletPool", pepperPool);
            Set(factory, "player", health);
            EnemySpawner spawner = spawnerGo.AddComponent<EnemySpawner>();
            Set(spawner, "factory", factory);
            Set(spawner, "potPlacer", placer);

            GameManager gm = new GameObject("Game Manager").AddComponent<GameManager>();
            Set(gm, "easySettings", easy);
            Set(gm, "hardSettings", hard);
            Set(gm, "playerHealth", health);
            Set(gm, "enemySpawner", spawner);
            Set(gm, "potPlacer", placer);

            BuildAudio();
            BuildUI();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static ObjectPool CreatePool(string name, Bullet prefab, int size)
        {
            ObjectPool pool = new GameObject(name).AddComponent<ObjectPool>();
            Set(pool, "bulletPrefab", prefab);
            Set(pool, "poolSize", size);
            return pool;
        }

        static AudioClip Clip(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/" + name + ".ogg");
        }

        static void BuildAudio()
        {
            GameObject go = new GameObject("Audio Manager");
            AudioSource sfx = go.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            AudioSource music = go.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.volume = 0.35f;
            AudioManager am = go.AddComponent<AudioManager>();
            Set(am, "sfxSource", sfx);
            Set(am, "musicSource", music);
            Set(am, "playerShoot", Clip("player_shoot"));
            Set(am, "playerDeath", Clip("player_death"));
            Set(am, "enemySpawn", Clip("enemy_spawn"));
            Set(am, "enemyKilled", Clip("enemy_splat"));
            Set(am, "pepperShoot", Clip("pepper_shoot"));
            Set(am, "tomatoHit", Clip("tomato_hit"));
            Set(am, "buttonClick", Clip("ui_click"));
            Set(am, "backgroundMusic", Clip("music_loop"));
        }

        static RectTransform Rect(GameObject go, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static Image MakeImage(string name, Transform parent, Color color, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, bool useRounded = true)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Rect(go, parent, aMin, aMax, pos, size);
            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            if (useRounded)
            {
                img.sprite = rounded;
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 box)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Rect(go, parent, aMin, aMax, pos, box);
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
        }

        static Button MakeButton(string name, Transform parent, string label, Color bg, Color fg, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            Image img = MakeImage(name, parent, bg, c, c, pos, size);
            img.raycastTarget = true;
            Button b = img.gameObject.AddComponent<Button>();
            ColorBlock colors = b.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            b.colors = colors;
            MakeText("Label", img.transform, label, size.y * 0.42f, fg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UnityEventTools.AddPersistentListener(b.onClick, action);
            return b;
        }

        static Toggle MakeToggle(string name, Transform parent, string label, Vector2 pos, ToggleGroup group, bool on)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            Image bg = MakeImage(name, parent, Color.white, c, c, pos, new Vector2(290f, 110f));
            bg.raycastTarget = true;
            Image check = MakeImage("Selected", bg.transform, Basil, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI text = MakeText("Label", bg.transform, label, 52f, Brown, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Toggle t = bg.gameObject.AddComponent<Toggle>();
            t.targetGraphic = bg;
            t.graphic = check;
            t.group = group;
            t.isOn = on;
            text.color = Brown;
            return t;
        }

        static GameObject FullPanel(string name, Transform parent, Color color)
        {
            Image img = MakeImage(name, parent, color, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
            img.raycastTarget = true;
            return img.gameObject;
        }

        static void BuildUI()
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            Transform root = canvasGo.transform;
            UIManager ui = canvasGo.AddComponent<UIManager>();

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            Vector2 c = new Vector2(0.5f, 0.5f);
            Vector2 top = new Vector2(0.5f, 1f);
            Vector2 bottom = new Vector2(0.5f, 0f);

            GameObject hud = new GameObject("HUD Panel", typeof(RectTransform));
            Rect(hud, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bar = MakeImage("Top Bar", hud.transform, new Color(Brown.r, Brown.g, Brown.b, 0.82f), top, top, new Vector2(0f, -150f), new Vector2(940f, 160f));
            Image healthBack = MakeImage("Health Back", bar.transform, new Color(0f, 0f, 0f, 0.45f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(250f, 0f), new Vector2(420f, 64f));
            Image healthFill = MakeImage("Health Fill", healthBack.transform, Basil, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            healthFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            TextMeshProUGUI healthText = MakeText("Health Text", healthBack.transform, "100 HP", 40f, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI scoreText = MakeText("Score Text", bar.transform, "Score 0", 56f, Mustard, c, c, new Vector2(120f, 0f), new Vector2(300f, 100f));
            TextMeshProUGUI timeText = MakeText("Time Text", bar.transform, "90s", 70f, Color.white, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(200f, 100f));
            TextMeshProUGUI fpsText = MakeText("FPS Text", hud.transform, "60 FPS", 30f, new Color(1f, 1f, 1f, 0.8f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -260f), new Vector2(200f, 50f));
            hud.AddComponent<FpsCounter>();
            Set(hud.GetComponent<FpsCounter>(), "fpsText", fpsText);
            MakeImage("Crosshair", hud.transform, new Color(1f, 1f, 1f, 0.9f), c, c, Vector2.zero, new Vector2(26f, 26f));
            Image hint = MakeImage("Place Hint", hud.transform, Mustard, bottom, bottom, new Vector2(0f, 230f), new Vector2(940f, 170f));
            MakeText("Hint Text", hint.transform, "Point at the floor, then tap to place the pot", 48f, Brown, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-60f, 0f));

            GameObject start = FullPanel("Start Panel", root, new Color(Cream.r, Cream.g, Cream.b, 0.95f));
            MakeText("Title", start.transform, "Kitchen\nInvasion", 150f, Tomato, c, c, new Vector2(0f, 400f), new Vector2(1000f, 340f)).lineSpacing = -20f;
            MakeText("Subtitle", start.transform, "Survive 90 seconds of angry food!", 44f, Brown, c, c, new Vector2(0f, 200f), new Vector2(1000f, 70f));
            MakeText("Difficulty Label", start.transform, "Difficulty", 40f, Brown, c, c, new Vector2(0f, 115f), new Vector2(600f, 60f));
            ToggleGroup group = start.AddComponent<ToggleGroup>();
            MakeToggle("Easy Toggle", start.transform, "Easy", new Vector2(-160f, 25f), group, true);
            Toggle hardToggle = MakeToggle("Hard Toggle", start.transform, "Hard", new Vector2(160f, 25f), group, false);
            UnityEventTools.AddPersistentListener(hardToggle.onValueChanged, ui.PressHardMode);
            MakeButton("Start Button", start.transform, "START", Tomato, Color.white, new Vector2(0f, -150f), new Vector2(620f, 150f), ui.PressStart);
            MakeButton("Leaderboard Button", start.transform, "LEADERBOARD", Mustard, Brown, new Vector2(0f, -320f), new Vector2(620f, 120f), ui.PressLeaderboard);
            MakeText("Credit", start.transform, "made by Fawziyyah Oke", 34f, Brown, c, c, new Vector2(0f, -460f), new Vector2(800f, 50f));

            GameObject end = FullPanel("End Panel", root, new Color(Cream.r, Cream.g, Cream.b, 0.95f));
            TextMeshProUGUI resultTitle = MakeText("Result Title", end.transform, "You Survived!", 110f, Tomato, c, c, new Vector2(0f, 420f), new Vector2(1000f, 160f));
            Image card = MakeImage("Stats Card", end.transform, Color.white, c, c, new Vector2(0f, 150f), new Vector2(860f, 340f));
            TextMeshProUGUI resultStats = MakeText("Result Stats", card.transform, "Score: 0", 54f, Brown, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-60f, -60f));
            resultStats.lineSpacing = 20f;
            MakeButton("Restart Button", end.transform, "RESTART", Tomato, Color.white, new Vector2(0f, -150f), new Vector2(620f, 150f), ui.PressStart);
            MakeButton("Menu Button", end.transform, "MAIN MENU", Mustard, Brown, new Vector2(0f, -320f), new Vector2(620f, 120f), ui.PressMainMenu);

            GameObject board = FullPanel("Leaderboard Panel", root, new Color(Cream.r, Cream.g, Cream.b, 0.97f));
            MakeText("Board Title", board.transform, "Last 5 Games", 90f, Tomato, c, c, new Vector2(0f, 480f), new Vector2(1000f, 130f));
            Image boardCard = MakeImage("Board Card", board.transform, Color.white, c, c, new Vector2(0f, 30f), new Vector2(900f, 740f));
            TextMeshProUGUI boardText = MakeText("Board Text", boardCard.transform, "No games played yet.", 36f, Brown, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-70f, -70f));
            boardText.alignment = TextAlignmentOptions.TopLeft;
            MakeButton("Back Button", board.transform, "BACK", Tomato, Color.white, new Vector2(0f, -470f), new Vector2(620f, 120f), ui.PressMainMenu);

            Image overlay = MakeImage("Damage Overlay", root, new Color(Tomato.r, Tomato.g, 0.1f, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
            DamageOverlay damage = overlay.gameObject.AddComponent<DamageOverlay>();
            Set(damage, "overlay", overlay);

            Set(ui, "startPanel", start);
            Set(ui, "hudPanel", hud);
            Set(ui, "endPanel", end);
            Set(ui, "leaderboardPanel", board);
            Set(ui, "healthFill", healthFill.rectTransform);
            Set(ui, "healthText", healthText);
            Set(ui, "scoreText", scoreText);
            Set(ui, "timeText", timeText);
            Set(ui, "placeHint", hint.gameObject);
            Set(ui, "resultTitle", resultTitle);
            Set(ui, "resultStats", resultStats);
            Set(ui, "leaderboardText", boardText);

            hud.SetActive(false);
            end.SetActive(false);
            board.SetActive(false);
        }

        [MenuItem("Kitchen Invasion/4 Check Project Validation")]
        public static void CheckValidation()
        {
            PropertyInfo prop = typeof(BuildValidator).GetProperty("PlatformRules", BindingFlags.NonPublic | BindingFlags.Static);
            IDictionary all = (IDictionary)prop.GetValue(null);
            int remaining = 0;
            foreach (BuildTargetGroup group in new[] { BuildTargetGroup.iOS, BuildTargetGroup.Standalone })
            {
                if (!all.Contains(group)) continue;
                foreach (BuildValidationRule rule in (List<BuildValidationRule>)all[group])
                {
                    if (rule.IsRuleEnabled != null && !rule.IsRuleEnabled()) continue;
                    if (rule.CheckPredicate == null || rule.CheckPredicate()) continue;
                    Debug.Log("KI_SETUP issue [" + group + "] " + rule.Category + ": " + rule.Message);
                    if (rule.FixIt != null && rule.FixItAutomatic)
                    {
                        rule.FixIt();
                        Debug.Log("KI_SETUP fixed: " + rule.Message);
                    }
                    if (!rule.CheckPredicate())
                    {
                        remaining++;
                        Debug.LogWarning("KI_SETUP still failing [" + group + "] " + rule.Message);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("KI_SETUP validation done, remaining issues: " + remaining);
        }

        [MenuItem("Kitchen Invasion/Debug/Start Round And Place Pot")]
        public static void DebugStartRound()
        {
            GameManager.Instance.StartGame();
            PotPlacer placer = Object.FindAnyObjectByType<PotPlacer>();
            Transform cam = Camera.main.transform;
            Vector3 flat = cam.forward;
            flat.y = 0f;
            Vector3 spot = cam.position + flat.normalized * 1.2f + Vector3.down * 1.0f;
            typeof(PotPlacer).GetMethod("PlacePot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(placer, new object[] { spot });
            Debug.Log("KI_DEBUG pot placed at " + spot + " camera " + cam.position);
        }

        [MenuItem("Kitchen Invasion/Debug/Shoot At Nearest Enemy")]
        public static void DebugShoot()
        {
            Enemy[] enemies = Object.FindObjectsByType<Enemy>();
            Transform cam = Camera.main.transform;
            if (enemies.Length > 0) cam.LookAt(enemies[0].transform.position + Vector3.up * 0.12f);
            for (int i = 0; i < 3; i++)
                typeof(PlayerShooter).GetMethod("Shoot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Object.FindAnyObjectByType<PlayerShooter>(), null);
            Debug.Log("KI_DEBUG shot, enemies alive " + enemies.Length);
        }

        [MenuItem("Kitchen Invasion/Debug/Press Start")]
        public static void DebugPressStart() { Object.FindAnyObjectByType<UIManager>().PressStart(); }

        [MenuItem("Kitchen Invasion/Debug/Press Leaderboard")]
        public static void DebugPressLeaderboard() { Object.FindAnyObjectByType<UIManager>().PressLeaderboard(); }

        [MenuItem("Kitchen Invasion/Debug/Press Main Menu")]
        public static void DebugPressMenu() { Object.FindAnyObjectByType<UIManager>().PressMainMenu(); }

        [MenuItem("Kitchen Invasion/Debug/Select Hard")]
        public static void DebugHard() { GameObject.Find("Hard Toggle").GetComponent<Toggle>().isOn = true; }

        [MenuItem("Kitchen Invasion/Debug/Show Plane Sample")]
        public static void DebugPlaneSample()
        {
            Transform cam = Camera.main.transform;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.position = new Vector3(cam.position.x, cam.position.y - 1.2f, cam.position.z) + new Vector3(cam.forward.x, 0f, cam.forward.z).normalized * 1.0f;
            quad.transform.rotation = Quaternion.Euler(90f, cam.eulerAngles.y, 0f);
            quad.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/M_KitchenPlane.mat");
        }

        [MenuItem("Kitchen Invasion/Debug/Log State")]
        public static void DebugLogState()
        {
            GameManager g = GameManager.Instance;
            string names = "";
            FieldInfo hp = typeof(Enemy).GetField("health", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (Enemy e in Object.FindObjectsByType<Enemy>()) names += e.name + "@" + e.transform.position + " hp " + hp.GetValue(e) + ", ";
            foreach (Bullet b in Object.FindObjectsByType<Bullet>()) names += " bullet@" + b.transform.position;
            Debug.Log("KI_DEBUG difficulty " + g.Difficulty.difficultyName + " planes " + Object.FindObjectsByType<ARPlane>().Length + " playing " + g.IsPlaying + " score " + g.Score + " killed " + g.EnemiesDefeated + " time " + g.TimeLeft + " enemies " + names);
        }

        [MenuItem("Kitchen Invasion/Debug/Capture Game View")]
        public static void DebugCapture()
        {
            ScreenCapture.CaptureScreenshot("Logs/ki_shot.png");
            Debug.Log("KI_DEBUG captured");
        }

        [MenuItem("Kitchen Invasion/5 Build iOS")]
        public static void BuildIOS()
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = new[] { ScenePath };
            options.locationPathName = "Builds/iOS";
            options.target = BuildTarget.iOS;
            options.options = BuildOptions.None;
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("KI_SETUP build result " + report.summary.result + " errors " + report.summary.totalErrors + " warnings " + report.summary.totalWarnings + " size " + report.summary.totalSize);
        }
    }
}
