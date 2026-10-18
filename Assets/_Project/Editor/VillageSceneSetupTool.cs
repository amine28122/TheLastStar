#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TheLastStar.Player;
using TheLastStar.Abilities;
using TheLastStar.Interaction;

namespace TheLastStar.EditorTools
{
    public static class VillageSceneSetupTool
    {
        [MenuItem("Tools/The Last Star/Build Cinematic Village Scene", false, 1)]
        public static void BuildCinematicVillageScene()
        {
            // 1. Create a fresh scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Atmospheric Sky & Lighting (Midnight Cosmic Fantasy)
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.08f, 0.18f); // Deep midnight indigo
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.14f);

            // Directional Moonlight
            GameObject dirLightGO = new GameObject("Directional Light (Cosmic Moonlight)");
            var dirLight = dirLightGO.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.60f, 0.75f, 1.0f);
            dirLight.intensity = 0.85f;
            dirLight.shadows = LightShadows.Soft;
            dirLightGO.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            // 3. Post-Processing Volume (Bloom, ACES Tonemapping, Vignette)
            GameObject volumeGO = new GameObject("PostProcessing_GlobalVolume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "CinematicVillage_Profile";

            // Add Bloom (Essential for glowing starlight aesthetics)
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.6f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(new Color(0.5f, 0.85f, 1.0f));

            // Add Tonemapping (Filmic look)
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            // Add Color Adjustments
            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.Override(0.4f);
            colorAdj.contrast.Override(15f);
            colorAdj.saturation.Override(18f);

            // Add Vignette
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.4f);

            volume.profile = profile;
            AssetDatabase.CreateAsset(profile, "Assets/_Project/Settings/CinematicVillage_Profile.asset");

            // 4. Village Stone Plaza (Concentric Altar)
            GameObject plazaGroup = new GameObject("Village_Plaza_Architecture");

            // Base Ground Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Plaza_Ground_Basement";
            ground.transform.parent = plazaGroup.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f); // 80x80m
            Material groundMat = CreateLitMaterial("Mat_VillageStoneGround", new Color(0.08f, 0.10f, 0.14f), 0.1f, 0.2f);
            ground.GetComponent<MeshRenderer>().material = groundMat;

            // Altar Outer Ring (Step 1)
            GameObject step1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            step1.name = "Altar_Ring_Outer";
            step1.transform.parent = plazaGroup.transform;
            step1.transform.position = new Vector3(0f, 0.15f, 0f);
            step1.transform.localScale = new Vector3(28f, 0.15f, 28f);
            Material ringMat = CreateLitMaterial("Mat_AncientCobblestone", new Color(0.12f, 0.15f, 0.20f), 0.2f, 0.1f);
            step1.GetComponent<MeshRenderer>().material = ringMat;

            // Altar Inner Ring (Step 2)
            GameObject step2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            step2.name = "Altar_Ring_Inner";
            step2.transform.parent = plazaGroup.transform;
            step2.transform.position = new Vector3(0f, 0.35f, 0f);
            step2.transform.localScale = new Vector3(18f, 0.15f, 18f);
            step2.GetComponent<MeshRenderer>().material = ringMat;

            // 5. Ancient Monoliths with Glowing Rune Inlays
            Material runeMat = CreateLitMaterial("Mat_StarlightRunes", new Color(0.2f, 0.85f, 1.0f), 0.0f, 0.9f);
            runeMat.EnableKeyword("_EMISSION");
            runeMat.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1.0f) * 3.5f);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * (Mathf.PI * 2f / 8f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 12f, 2.5f, Mathf.Sin(angle) * 12f);

                GameObject monolith = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monolith.name = $"AncientMonolith_{i + 1}";
                monolith.transform.parent = plazaGroup.transform;
                monolith.transform.position = pos;
                monolith.transform.localScale = new Vector3(1.4f, 5.0f, 1.4f);
                monolith.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
                monolith.GetComponent<MeshRenderer>().material = ringMat;

                // Glowing Rune Core on Monolith
                GameObject runeCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
                runeCore.name = "RuneCore";
                runeCore.transform.parent = monolith.transform;
                runeCore.transform.localPosition = new Vector3(0f, 0.15f, 0.52f);
                runeCore.transform.localScale = new Vector3(0.35f, 0.65f, 0.1f);
                Object.DestroyImmediate(runeCore.GetComponent<Collider>());
                runeCore.GetComponent<MeshRenderer>().material = runeMat;

                // Subtle light from each monolith
                GameObject monoLightGO = new GameObject("MonolithGlow");
                monoLightGO.transform.parent = monolith.transform;
                monoLightGO.transform.localPosition = new Vector3(0f, 0.15f, 0.8f);
                var monoLight = monoLightGO.AddComponent<Light>();
                monoLight.type = LightType.Point;
                monoLight.range = 4f;
                monoLight.intensity = 0.8f;
                monoLight.color = new Color(0.2f, 0.7f, 1.0f);
            }

            // 6. Central Celestial Fountain & Starlight Pedestal
            GameObject fountainBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountainBase.name = "Central_Fountain_Basin";
            fountainBase.transform.parent = plazaGroup.transform;
            fountainBase.transform.position = new Vector3(0f, 0.6f, 0f);
            fountainBase.transform.localScale = new Vector3(6f, 0.35f, 6f);
            fountainBase.GetComponent<MeshRenderer>().material = ringMat;

            GameObject pedestalGO = new GameObject("Starlight_Pedestal");
            pedestalGO.transform.position = new Vector3(0f, 0.9f, 0f);

            GameObject pedestalShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestalShaft.name = "Pedestal_Shaft";
            pedestalShaft.transform.parent = pedestalGO.transform;
            pedestalShaft.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            pedestalShaft.transform.localScale = new Vector3(1.2f, 0.7f, 1.2f);
            pedestalShaft.GetComponent<MeshRenderer>().material = ringMat;

            // Hovering Diamond Crystal (Rotating)
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "Celestial_StarShard_Crystal";
            crystal.transform.parent = pedestalGO.transform;
            crystal.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            crystal.transform.localScale = new Vector3(0.85f, 1.25f, 0.85f);
            Material crystalMat = CreateLitMaterial("Mat_CelestialCrystal", new Color(0.2f, 0.9f, 1f), 0.1f, 0.95f);
            crystalMat.EnableKeyword("_EMISSION");
            crystalMat.SetColor("_EmissionColor", new Color(0.1f, 0.7f, 1f) * 4.5f);
            crystal.GetComponent<MeshRenderer>().material = crystalMat;

            GameObject crystalLightGO = new GameObject("CrystalPointLight");
            crystalLightGO.transform.parent = crystal.transform;
            crystalLightGO.transform.localPosition = Vector3.zero;
            var crystalLight = crystalLightGO.AddComponent<Light>();
            crystalLight.type = LightType.Point;
            crystalLight.range = 10f;
            crystalLight.intensity = 4.0f;
            crystalLight.color = new Color(0.25f, 0.85f, 1f);
            crystalLight.enabled = false;

            var pedestalScript = pedestalGO.AddComponent<LightSensitivePedestal>();
            SetSerializedField(pedestalScript, "crystalRenderer", crystal.GetComponent<MeshRenderer>());
            SetSerializedField(pedestalScript, "crystalLight", crystalLight);

            // Add simple floating animation component to the crystal
            crystal.AddComponent<FloatAndRotate>();

            // 7. Floating Magical Star Dust Particles (Atmospheric Magic)
            GameObject dustParticlesGO = new GameObject("VFX_AtmosphericStarDust");
            dustParticlesGO.transform.position = new Vector3(0f, 3f, 0f);
            var ps = dustParticlesGO.AddComponent<ParticleSystem>();
            var psRenderer = dustParticlesGO.GetComponent<ParticleSystemRenderer>();
            var main = ps.main;
            main.startLifetime = 6f;
            main.startSpeed = 0.4f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.6f, 0.9f, 1.0f, 0.8f);
            main.maxParticles = 300;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 35f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 8f, 30f);

            var velOverLifetime = ps.velocityOverLifetime;
            velOverLifetime.enabled = true;
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);

            psRenderer.material = runeMat;

            // 8. Player (The Little Star)
            GameObject playerGO = new GameObject("The_Little_Star");
            playerGO.transform.position = new Vector3(0f, 1.2f, -6f);

            // Stylized Body Mesh
            GameObject playerBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerBody.name = "Body_Mesh";
            playerBody.transform.parent = playerGO.transform;
            playerBody.transform.localPosition = Vector3.zero;
            playerBody.transform.localScale = new Vector3(0.75f, 0.85f, 0.75f);
            Object.DestroyImmediate(playerBody.GetComponent<Collider>());
            Material starBodyMat = CreateLitMaterial("Mat_LittleStarBody", new Color(0.85f, 0.94f, 1.0f), 0.1f, 0.5f);
            playerBody.GetComponent<MeshRenderer>().material = starBodyMat;

            // Chest Starlight Core (High Emission)
            GameObject starCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starCore.name = "Star_Core";
            starCore.transform.parent = playerGO.transform;
            starCore.transform.localPosition = new Vector3(0f, 0.32f, 0.35f);
            starCore.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            Object.DestroyImmediate(starCore.GetComponent<Collider>());
            Material starCoreMat = CreateLitMaterial("Mat_StarCoreGlow", new Color(0.3f, 0.95f, 1.0f), 0.0f, 1.0f);
            starCoreMat.EnableKeyword("_EMISSION");
            starCoreMat.SetColor("_EmissionColor", new Color(0.2f, 0.85f, 1.0f) * 6.0f);
            starCore.GetComponent<MeshRenderer>().material = starCoreMat;

            // Floating Halo / Cloak Sparkles
            GameObject haloLightGO = new GameObject("PlayerAuraLight");
            haloLightGO.transform.parent = playerGO.transform;
            haloLightGO.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            var haloLight = haloLightGO.AddComponent<Light>();
            haloLight.type = LightType.Point;
            haloLight.range = 6.0f;
            haloLight.intensity = 2.0f;
            haloLight.color = new Color(0.4f, 0.85f, 1.0f);

            // Pulse Flash Light
            GameObject pulseLightGO = new GameObject("PulseFlashLight");
            pulseLightGO.transform.parent = playerGO.transform;
            pulseLightGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var pulseLight = pulseLightGO.AddComponent<Light>();
            pulseLight.type = LightType.Point;
            pulseLight.range = 10f;
            pulseLight.intensity = 0f;
            pulseLight.color = new Color(0.6f, 0.95f, 1f);
            pulseLight.enabled = false;

            // Player Components
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.parent = playerGO.transform;
            groundCheckGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);

            var charController = playerGO.AddComponent<CharacterController>();
            charController.height = 1.7f;
            charController.radius = 0.4f;
            charController.center = Vector3.zero;

            var playerController = playerGO.AddComponent<PlayerController>();
            SetSerializedField(playerController, "groundCheck", groundCheckGO.transform);

            var pulseAbility = playerGO.AddComponent<LightPulseAbility>();
            SetSerializedField(pulseAbility, "pulseLight", pulseLight);

            // 9. Cinematic Camera Setup
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.fieldOfView = 58f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 200f;
            cameraGO.AddComponent<AudioListener>();

            // Enable Post Processing on Camera
            var camData = cameraGO.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var camFollow = cameraGO.AddComponent<ThirdPersonCameraFollow>();
            camFollow.SetTarget(playerGO.transform);

            // 10. Save and Open Scene
            string scenePath = "Assets/_Project/Scenes/Regions/R1_ForgottenVillage_Cinematic.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Debug.Log($"<color=#38bdf8>[The Last Star]</color> Cinematic Village Scene created and loaded at: {scenePath}");
            EditorUtility.DisplayDialog("THE LAST STAR — Cinematic Village", 
                "✨ The Cinematic Village Scene has been generated and opened!\n\n" +
                "Visual Highlights Included:\n" +
                "• Atmospheric Midnight Lighting & Volumetric Cosmic Fog\n" +
                "• ACES Film Color Grading, Bloom & Vignette Post-Processing\n" +
                "• Floating Magical Star Dust Particles\n" +
                "• Ancient Monoliths with Glowing Starlight Runes\n" +
                "• Rotating Celestial Star Shard Altar\n" +
                "• Player with Radiant Chest Core & Aura\n\n" +
                "Press PLAY (▶) now to experience the atmosphere!", "Start Adventure!");
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
