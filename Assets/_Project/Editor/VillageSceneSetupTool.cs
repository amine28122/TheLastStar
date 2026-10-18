#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheLastStar.Player;
using TheLastStar.Abilities;
using TheLastStar.Interaction;

namespace TheLastStar.EditorTools
{
    public static class VillageSceneSetupTool
    {
        [MenuItem("Tools/The Last Star/Build Playable Test Scene (Village)", false, 1)]
        public static void BuildVillageTestScene()
        {
            // 1. Create a fresh scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Lighting & Sky
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.28f);

            GameObject dirLightGO = new GameObject("Directional Light (Moonlight)");
            var dirLight = dirLightGO.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.65f, 0.75f, 1.0f);
            dirLight.intensity = 0.9f;
            dirLightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 3. Ground / Village Plaza
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "VillagePlaza_Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f); // 80x80m
            var groundRenderer = ground.GetComponent<MeshRenderer>();
            Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            groundMat.color = new Color(0.18f, 0.22f, 0.26f);
            groundRenderer.material = groundMat;

            // 4. Village Decorative Ruins (Pillars & Arches)
            GameObject ruinsGroup = new GameObject("Village_Ruins");
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (Mathf.PI * 2f / 8f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 16f, 2.5f, Mathf.Sin(angle) * 16f);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"AncientPillar_{i + 1}";
                pillar.transform.parent = ruinsGroup.transform;
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(1.4f, 2.5f, 1.4f);
                Material pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                pillarMat.color = new Color(0.25f, 0.28f, 0.35f);
                pillar.GetComponent<MeshRenderer>().material = pillarMat;
            }

            // 5. Light-Sensitive Pedestal (Puzzle Core)
            GameObject pedestalGO = new GameObject("Starlight_Pedestal");
            pedestalGO.transform.position = new Vector3(0f, 0f, 8f);

            GameObject pedestalBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestalBase.name = "Pedestal_Base";
            pedestalBase.transform.parent = pedestalGO.transform;
            pedestalBase.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            pedestalBase.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);

            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "Starlight_Crystal";
            crystal.transform.parent = pedestalGO.transform;
            crystal.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            crystal.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);

            Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            crystalMat.color = new Color(0.3f, 0.8f, 1f);
            crystal.GetComponent<MeshRenderer>().material = crystalMat;

            GameObject crystalLightGO = new GameObject("CrystalLight");
            crystalLightGO.transform.parent = crystal.transform;
            crystalLightGO.transform.localPosition = Vector3.zero;
            var crystalLight = crystalLightGO.AddComponent<Light>();
            crystalLight.type = LightType.Point;
            crystalLight.range = 8f;
            crystalLight.intensity = 3.5f;
            crystalLight.color = new Color(0.2f, 0.9f, 1f);
            crystalLight.enabled = false;

            var pedestalScript = pedestalGO.AddComponent<LightSensitivePedestal>();
            SetSerializedField(pedestalScript, "crystalRenderer", crystal.GetComponent<MeshRenderer>());
            SetSerializedField(pedestalScript, "crystalLight", crystalLight);

            // 6. Player (The Little Star)
            GameObject playerGO = new GameObject("The_Little_Star");
            playerGO.transform.position = new Vector3(0f, 1.0f, 0f);

            // Player Visual Body
            GameObject playerBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerBody.name = "Body_Mesh";
            playerBody.transform.parent = playerGO.transform;
            playerBody.transform.localPosition = Vector3.zero;
            playerBody.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
            Object.DestroyImmediate(playerBody.GetComponent<Collider>()); // Let CharacterController handle collisions
            Material bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bodyMat.color = new Color(0.85f, 0.92f, 1f);
            playerBody.GetComponent<MeshRenderer>().material = bodyMat;

            // Player Starlight Core (Glow on chest)
            GameObject starCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starCore.name = "Star_Core";
            starCore.transform.parent = playerGO.transform;
            starCore.transform.localPosition = new Vector3(0f, 0.35f, 0.35f);
            starCore.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Object.DestroyImmediate(starCore.GetComponent<Collider>());
            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            coreMat.color = new Color(0.3f, 0.9f, 1f);
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_EmissionColor", new Color(0.3f, 0.9f, 1f) * 4f);
            starCore.GetComponent<MeshRenderer>().material = coreMat;

            // Player Ambient Starlight PointLight
            GameObject playerLightGO = new GameObject("StarAmbientLight");
            playerLightGO.transform.parent = playerGO.transform;
            playerLightGO.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            var playerLight = playerLightGO.AddComponent<Light>();
            playerLight.type = LightType.Point;
            playerLight.range = 5f;
            playerLight.intensity = 1.8f;
            playerLight.color = new Color(0.4f, 0.85f, 1f);

            // Player Pulse Flash Light
            GameObject pulseLightGO = new GameObject("PulseFlashLight");
            pulseLightGO.transform.parent = playerGO.transform;
            pulseLightGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var pulseLight = pulseLightGO.AddComponent<Light>();
            pulseLight.type = LightType.Point;
            pulseLight.range = 8f;
            pulseLight.intensity = 0f;
            pulseLight.color = new Color(0.5f, 0.95f, 1f);
            pulseLight.enabled = false;

            // Ground Check Transform
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.parent = playerGO.transform;
            groundCheckGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);

            // Player Components
            var charController = playerGO.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.radius = 0.45f;
            charController.center = Vector3.zero;

            var playerController = playerGO.AddComponent<PlayerController>();
            SetSerializedField(playerController, "groundCheck", groundCheckGO.transform);

            var pulseAbility = playerGO.AddComponent<LightPulseAbility>();
            SetSerializedField(pulseAbility, "pulseLight", pulseLight);

            // 7. Third-Person Camera
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
            var camFollow = cameraGO.AddComponent<ThirdPersonCameraFollow>();
            camFollow.SetTarget(playerGO.transform);

            // 8. Save Scene
            string scenePath = "Assets/_Project/Scenes/Regions/R1_ForgottenVillage_Test.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"<color=#55ff88>[The Last Star]</color> Playable Village Scene created successfully at: {scenePath}");
            EditorUtility.DisplayDialog("The Last Star", "Village Test Scene has been created successfully!\n\nControls:\n- WASD: Move\n- Shift: Sprint\n- Space: Jump\n- Mouse: Orbit Camera\n- Q or Right-Click: Luminescent Pulse (Activates the Starlight Pedestal!)\n- Esc: Toggle Cursor Lock\n\nClick Play in Unity to test!", "Awesome!");
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
