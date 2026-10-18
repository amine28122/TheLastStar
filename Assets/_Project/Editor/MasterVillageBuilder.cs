#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TheLastStar.Core;
using TheLastStar.Player;
using TheLastStar.Abilities;
using TheLastStar.Interaction;

namespace TheLastStar.EditorTools
{
    public static class MasterVillageBuilder
    {
        private const string FBX_PATH = "Assets/_Project/Art/Environments/CastleKit/Models/FBX format/";

        [MenuItem("Tools/The Last Star/🌟 Build Storybook Fantasy Village", false, 0)]
        public static void BuildMasterVillage()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Atmosphere, Lighting & Volumetric Cosmic Fog
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.05f, 0.15f); // Deep midnight indigo
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.16f);

            // Moonlight Directional Light
            GameObject moonLightGO = new GameObject("Directional Light (Cosmic Moon)");
            var moonLight = moonLightGO.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.55f, 0.70f, 1.0f);
            moonLight.intensity = 0.65f;
            moonLight.shadows = LightShadows.Soft;
            moonLightGO.transform.rotation = Quaternion.Euler(52f, -38f, 0f);

            // 2. Post-Processing Profile (URP Bloom, ACES, Color Grading)
            SetupPostProcessing();

            // 3. Falling Shooting Stars Particle Effect (Just like the concept art!)
            CreateShootingStarsSystem();

            // 4. Village Architecture (Using real 3D FBX modular models)
            GameObject villageGroup = new GameObject("Village_Environment_Architecture");

            // Cobblestone ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Village_Cobblestone_Courtyard";
            ground.transform.parent = villageGroup.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            Material groundMat = CreateLitMaterial("Mat_CourtyardCobble", new Color(0.07f, 0.09f, 0.12f), 0.3f, 0.1f);
            ground.GetComponent<MeshRenderer>().material = groundMat;

            // Load FBX Prefabs
            SpawnBuilding(villageGroup.transform, new Vector3(-12f, 0f, 10f), 45f);
            SpawnBuilding(villageGroup.transform, new Vector3(12f, 0f, 10f), -45f);
            SpawnBuilding(villageGroup.transform, new Vector3(-14f, 0f, -4f), 90f);
            SpawnBuilding(villageGroup.transform, new Vector3(14f, 0f, -4f), -90f);

            // Stone Arches & Walls around square
            SpawnMesh(villageGroup.transform, "wall-narrow-gate.fbx", new Vector3(0f, 0f, 18f), Quaternion.Euler(0f, 0f, 0f), 1.5f);
            SpawnMesh(villageGroup.transform, "wall.fbx", new Vector3(-7f, 0f, 18f), Quaternion.Euler(0f, 0f, 0f), 1.5f);
            SpawnMesh(villageGroup.transform, "wall.fbx", new Vector3(7f, 0f, 18f), Quaternion.Euler(0f, 0f, 0f), 1.5f);

            // Fantasy Trees
            SpawnMesh(villageGroup.transform, "tree-large.fbx", new Vector3(-18f, 0f, 6f), Quaternion.identity, 1.8f);
            SpawnMesh(villageGroup.transform, "tree-large.fbx", new Vector3(18f, 0f, 7f), Quaternion.identity, 1.9f);
            SpawnMesh(villageGroup.transform, "tree-small.fbx", new Vector3(-8f, 0f, 15f), Quaternion.identity, 1.4f);
            SpawnMesh(villageGroup.transform, "tree-small.fbx", new Vector3(9f, 0f, 14f), Quaternion.identity, 1.4f);

            // Decorative Boulders & Rocks
            SpawnMesh(villageGroup.transform, "rocks-large.fbx", new Vector3(-5f, 0f, 7f), Quaternion.Euler(0f, 25f, 0f), 1.2f);
            SpawnMesh(villageGroup.transform, "rocks-small.fbx", new Vector3(5f, 0f, 6f), Quaternion.Euler(0f, -60f, 0f), 1.3f);

            // Stone Steps in foreground
            SpawnMesh(villageGroup.transform, "stairs-stone.fbx", new Vector3(2.5f, 0f, -1.5f), Quaternion.Euler(0f, -30f, 0f), 1.3f);

            // 5. Central Ancient Fountain & Starlight Pedestal
            GameObject pedestalAltar = new GameObject("Central_Starlight_Altar");
            pedestalAltar.transform.position = new Vector3(-1.0f, 0f, 2.5f);

            GameObject pedestalPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestalPillar.name = "Ornate_Stone_Pedestal";
            pedestalPillar.transform.parent = pedestalAltar.transform;
            pedestalPillar.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            pedestalPillar.transform.localScale = new Vector3(1.1f, 0.75f, 1.1f);
            Material stoneMat = CreateLitMaterial("Mat_AncientCarvedStone", new Color(0.18f, 0.22f, 0.28f), 0.2f, 0.1f);
            pedestalPillar.GetComponent<MeshRenderer>().material = stoneMat;

            // 6. The Little Star (Player Character on Pedestal with Cosmic Cape)
            GameObject playerGO = CreateTheLittleStar(new Vector3(-1.0f, 1.55f, 2.5f));

            // 7. Luma (Fox Companion with Glowing Lantern Tail)
            GameObject lumaGO = CreateLumaCompanion(new Vector3(1.8f, 0.45f, 0.8f));

            // 8. Cinematic Main Camera Setup
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cameraGO.transform.position = new Vector3(0.5f, 2.0f, -3.8f);
            cameraGO.transform.rotation = Quaternion.Euler(14f, -10f, 0f);

            var cam = cameraGO.AddComponent<Camera>();
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.2f;
            cameraGO.AddComponent<AudioListener>();

            var camData = cameraGO.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var camFollow = cameraGO.AddComponent<ThirdPersonCameraFollow>();
            camFollow.SetTarget(playerGO.transform);

            // Save and open scene
            string sceneDir = "Assets/_Project/Scenes/Regions";
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);
            string scenePath = $"{sceneDir}/R1_ForgottenVillage_Master.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Debug.Log($"<color=#38bdf8>[The Last Star]</color> Masterpiece Storybook Scene Created Successfully: {scenePath}");
            EditorUtility.DisplayDialog("THE LAST STAR", 
                "✨ Welcome to The Forgotten Village!\n\n" +
                "Featuring:\n" +
                "• 3D Medieval Architecture, Towers, Walls & Trees\n" +
                "• The Little Star with Custom Cosmic Starfield Cape\n" +
                "• Luma with Glowing Lantern Tail & Warm Lighting\n" +
                "• Falling Shooting Stars (Meteor Shower) Particle VFX\n" +
                "• ACES Cinematic Bloom & Midnight Cosmic Sky\n\n" +
                "Press PLAY (▶) now to experience the game!", "Start Journey!");
        }

        private static GameObject CreateTheLittleStar(Vector3 position)
        {
            GameObject playerGO = new GameObject("The_Little_Star");
            playerGO.transform.position = position;

            // Luminous Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body_LuminousSpirit";
            body.transform.parent = playerGO.transform;
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Material spiritMat = CreateLitMaterial("Mat_StarSpiritBody", new Color(0.75f, 0.92f, 1.0f), 0.1f, 0.9f);
            spiritMat.EnableKeyword("_EMISSION");
            spiritMat.SetColor("_EmissionColor", new Color(0.4f, 0.85f, 1.0f) * 2.2f);
            body.GetComponent<MeshRenderer>().material = spiritMat;

            // Cosmic Cape (With Custom Starfield Shader)
            GameObject cape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cape.name = "Cosmic_Cape";
            cape.transform.parent = playerGO.transform;
            cape.transform.localPosition = new Vector3(0f, 0.55f, -0.1f);
            cape.transform.localScale = new Vector3(0.75f, 0.65f, 0.55f);
            Object.DestroyImmediate(cape.GetComponent<Collider>());

            Shader cosmicShader = Shader.Find("TheLastStar/CosmicCape");
            Material capeMat = new Material(cosmicShader != null ? cosmicShader : Shader.Find("Universal Render Pipeline/Lit"));
            capeMat.name = "Mat_CosmicStarfieldCape";
            cape.GetComponent<MeshRenderer>().material = capeMat;

            // Radiant Chest Star Core
            GameObject starCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starCore.name = "Chest_StarCore";
            starCore.transform.parent = playerGO.transform;
            starCore.transform.localPosition = new Vector3(0f, 0.8f, 0.32f);
            starCore.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            Object.DestroyImmediate(starCore.GetComponent<Collider>());
            Material starCoreMat = CreateLitMaterial("Mat_StarCoreRadiance", new Color(0.9f, 0.98f, 1.0f), 0.0f, 1.0f);
            starCoreMat.EnableKeyword("_EMISSION");
            starCoreMat.SetColor("_EmissionColor", new Color(0.35f, 0.92f, 1.0f) * 6.5f);
            starCore.GetComponent<MeshRenderer>().material = starCoreMat;

            // Blue Starlight Point Light
            GameObject starLightGO = new GameObject("Starlight_Core_Light");
            starLightGO.transform.parent = playerGO.transform;
            starLightGO.transform.localPosition = new Vector3(0f, 0.8f, 0.35f);
            var starLight = starLightGO.AddComponent<Light>();
            starLight.type = LightType.Point;
            starLight.range = 5.5f;
            starLight.intensity = 2.4f;
            starLight.color = new Color(0.35f, 0.88f, 1.0f);

            // Ground Check
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.parent = playerGO.transform;
            groundCheckGO.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            // Player Components
            var cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.4f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.7f, 0f);

            var pc = playerGO.AddComponent<PlayerController>();
            SetSerializedField(pc, "groundCheck", groundCheckGO.transform);

            var ability = playerGO.AddComponent<LightPulseAbility>();

            return playerGO;
        }

        private static GameObject CreateLumaCompanion(Vector3 position)
        {
            GameObject lumaGO = new GameObject("Luma_Fox_Companion");
            lumaGO.transform.position = position;
            lumaGO.transform.rotation = Quaternion.Euler(0f, -135f, 0f); // Looking towards The Little Star

            // Golden Fox Fur Material
            Material foxFurMat = CreateLitMaterial("Mat_LumaGoldenFur", new Color(0.95f, 0.72f, 0.35f), 0.3f, 0.1f);
            Material whiteFurMat = CreateLitMaterial("Mat_LumaWhiteChest", new Color(0.98f, 0.95f, 0.90f), 0.2f, 0.0f);

            // Fox Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Fox_Body";
            body.transform.parent = lumaGO.transform;
            body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            body.transform.localScale = new Vector3(0.45f, 0.45f, 0.7f);
            body.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().material = foxFurMat;

            // Fox Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Fox_Head";
            head.transform.parent = lumaGO.transform;
            head.transform.localPosition = new Vector3(0f, 0.75f, 0.35f);
            head.transform.localScale = new Vector3(0.42f, 0.4f, 0.48f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<MeshRenderer>().material = whiteFurMat;

            // Fox Antler / Long Ears
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ear.name = $"Fox_Ear_{(i == -1 ? "L" : "R")}";
                ear.transform.parent = head.transform;
                ear.transform.localPosition = new Vector3(i * 0.2f, 0.32f, 0.05f);
                ear.transform.localRotation = Quaternion.Euler(-15f, 0f, i * -25f);
                ear.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
                Object.DestroyImmediate(ear.GetComponent<Collider>());
                ear.GetComponent<MeshRenderer>().material = foxFurMat;
            }

            // Fluffy Glowing Tail carrying Lantern
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tail.name = "Fluffy_Golden_Tail";
            tail.transform.parent = lumaGO.transform;
            tail.transform.localPosition = new Vector3(0f, 0.55f, -0.45f);
            tail.transform.localScale = new Vector3(0.45f, 0.65f, 0.45f);
            tail.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            Object.DestroyImmediate(tail.GetComponent<Collider>());
            Material glowingTailMat = CreateLitMaterial("Mat_LumaTailGlow", new Color(1.0f, 0.85f, 0.45f), 0.1f, 0.8f);
            glowingTailMat.EnableKeyword("_EMISSION");
            glowingTailMat.SetColor("_EmissionColor", new Color(1.0f, 0.75f, 0.25f) * 3.5f);
            tail.GetComponent<MeshRenderer>().material = glowingTailMat;

            // Brass Lantern attached to tail
            GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lantern.name = "Tail_Lantern";
            lantern.transform.parent = tail.transform;
            lantern.transform.localPosition = new Vector3(0f, 0.25f, -0.3f);
            lantern.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            Object.DestroyImmediate(lantern.GetComponent<Collider>());
            Material lanternGlass = CreateLitMaterial("Mat_LanternFlame", new Color(1.0f, 0.7f, 0.2f), 0.0f, 1.0f);
            lanternGlass.EnableKeyword("_EMISSION");
            lanternGlass.SetColor("_EmissionColor", new Color(1.0f, 0.65f, 0.15f) * 6.0f);
            lantern.GetComponent<MeshRenderer>().material = lanternGlass;

            // Warm Golden Point Light (The warm contrast in the artwork!)
            GameObject lanternLightGO = new GameObject("Lantern_Warm_Light");
            lanternLightGO.transform.parent = lantern.transform;
            lanternLightGO.transform.localPosition = Vector3.zero;
            var lanternLight = lanternLightGO.AddComponent<Light>();
            lanternLight.type = LightType.Point;
            lanternLight.range = 7.0f;
            lanternLight.intensity = 3.2f;
            lanternLight.color = new Color(1.0f, 0.68f, 0.22f);
            lanternLight.shadows = LightShadows.Soft;

            return lumaGO;
        }

        private static void CreateShootingStarsSystem()
        {
            GameObject meteorsGO = new GameObject("VFX_ShootingStars_MeteorShower");
            meteorsGO.transform.position = new Vector3(0f, 25f, 5f);
            var ps = meteorsGO.AddComponent<ParticleSystem>();
            var psRenderer = meteorsGO.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.startLifetime = 1.8f;
            main.startSpeed = 16f;
            main.startSize = 0.25f;
            main.startColor = new Color(0.75f, 0.90f, 1.0f, 0.95f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(45f, 2f, 45f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(-12f, -18f); // Falling down!
            velocity.x = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);

            // Trail renderer for shooting stars
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 1.0f;
            trails.lifetime = 0.45f;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1.0f, 0.0f);

            Material trailMat = CreateLitMaterial("Mat_ShootingStarTrail", new Color(0.6f, 0.85f, 1.0f), 0f, 1f);
            trailMat.EnableKeyword("_EMISSION");
            trailMat.SetColor("_EmissionColor", new Color(0.5f, 0.85f, 1.0f) * 5.0f);
            psRenderer.trailMaterial = trailMat;
        }

        private static void SetupPostProcessing()
        {
            GameObject volumeGO = new GameObject("PostProcessing_GlobalVolume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "MasterpieceVillage_Profile";

            // Bloom
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(2.2f);
            bloom.threshold.Override(0.80f);
            bloom.scatter.Override(0.75f);
            bloom.tint.Override(new Color(0.6f, 0.85f, 1.0f));

            // Tonemapping
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            // Color Adjustments
            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.Override(0.5f);
            colorAdj.contrast.Override(20f);
            colorAdj.saturation.Override(25f);

            // Vignette
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.38f);
            vignette.smoothness.Override(0.45f);

            volume.profile = profile;

            string settingsFolder = "Assets/_Project/Settings";
            if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);
            string profilePath = $"{settingsFolder}/MasterpieceVillage_Profile.asset";
            if (File.Exists(profilePath)) AssetDatabase.DeleteAsset(profilePath);
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        private static void SpawnBuilding(Transform parent, Vector3 position, float rotationY)
        {
            GameObject building = new GameObject("Medieval_House");
            building.transform.parent = parent;
            building.transform.position = position;
            building.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

            SpawnMesh(building.transform, "tower-square-base.fbx", Vector3.zero, Quaternion.identity, 2.0f);
            SpawnMesh(building.transform, "tower-square-mid-windows.fbx", new Vector3(0f, 4.0f, 0f), Quaternion.identity, 2.0f);
            SpawnMesh(building.transform, "tower-square-roof.fbx", new Vector3(0f, 8.0f, 0f), Quaternion.identity, 2.0f);
        }

        private static GameObject SpawnMesh(Transform parent, string fileName, Vector3 localPos, Quaternion localRot, float scale)
        {
            string fullPath = FBX_PATH + fileName;
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (fbx != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                instance.transform.parent = parent;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = localRot;
                instance.transform.localScale = Vector3.one * scale;
                return instance;
            }
            return null;
        }

        private static Material CreateLitMaterial(string name, Color baseColor, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.name = name;
            mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is Object objVal)
                    prop.objectReferenceValue = objVal;
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif

