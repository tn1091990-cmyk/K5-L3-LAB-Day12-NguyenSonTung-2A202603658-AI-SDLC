// The whole view layer of Đảo Đèn Lồng, in one script. Everything is procedural
// (primitives + lights + OnGUI) so the project carries no binary art assets.
//
// The game is driven by the SAME DaoDenLong.Core simulation the tests run: one
// Step() every 1/60 s, fed with a live input frame (move vector, camera yaw,
// jump/interact/pulse buttons). The core owns all rules; this script only draws.
//
// Controls: WASD/arrows move, Space jump, E interact (deposit), Q or left click
// pulse (Tỏa Sáng), mouse while holding right button turns the camera, R replays.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DaoDenLong.Core;

namespace DaoDenLong.View
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private Simulation _sim;
        private int _seed = 1;
        private double _camYaw;                       // degrees, fed into the sim input
        private float _accum;                         // 60 Hz step accumulator

        // visuals, kept in dictionaries keyed by the core ids
        private readonly Dictionary<string, GameObject> _moteVis = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _shadeVis = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Light> _lanternLight = new Dictionary<string, Light>();
        private readonly Dictionary<string, Renderer> _lanternBody = new Dictionary<string, Renderer>();
        private Transform _playerVis;
        private Renderer _playerBody;
        private Camera _cam;
        private float _pulseFlashUntil;

        private Material _matMote, _matShade, _matPlayer, _matGround, _matLanternOn, _matLanternOff;

        private void Awake()
        {
            _seed = ReadSeed();

            _matMote = Mat(new Color(1f, 0.85f, 0.35f), new Color(1f, 0.8f, 0.25f));
            _matShade = Mat(new Color(0.05f, 0.04f, 0.09f), new Color(0.02f, 0.01f, 0.05f));
            _matPlayer = Mat(new Color(0.9f, 0.85f, 0.7f), new Color(0.25f, 0.2f, 0.1f));
            _matGround = Mat(new Color(0.1f, 0.18f, 0.16f), Color.black);
            _matLanternOn = Mat(new Color(0.85f, 0.25f, 0.15f), new Color(1f, 0.45f, 0.1f));
            _matLanternOff = Mat(new Color(0.35f, 0.12f, 0.1f), Color.black);

            _sim = new Simulation();
            _sim.Build(_seed);            // default Night 1 layout, standard tuning
            BuildWorld();                 // draws what the core just built

            _playerVis = BuildPlayer();
            _cam = Camera.main != null ? Camera.main : new GameObject("Main Camera").AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.02f, 0.03f, 0.07f);   // the night sky
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.09f);
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 70f;
        }

        private int ReadSeed()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-seed" || args[i] == "--seed")
                    if (int.TryParse(args[i + 1], out int v)) return v;
            return 1;
        }

        private void BuildWorld()
        {
            // the moon: one cold directional light
            Light moon = new GameObject("Moon").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.55f, 0.6f, 0.8f);
            moon.intensity = 0.45f;
            moon.transform.rotation = Quaternion.Euler(55f, 30f, 0f);

            // the island: a flat disc of ground at y = 0
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Island";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(80f, 0.5f, 80f);
            ground.GetComponent<Renderer>().sharedMaterial = _matGround;

            // the five big lanterns, drawn straight from what the core built
            foreach (Lantern l in _sim.Lanterns)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Lantern_" + l.Id;
                pole.transform.position = new Vector3((float)l.Pos.X, 1.2f, (float)l.Pos.Z);
                pole.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "LanternBody_" + l.Id;
                body.transform.SetParent(pole.transform);
                body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                body.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
                body.GetComponent<Renderer>().sharedMaterial = _matLanternOff;
                _lanternBody[l.Id] = body.GetComponent<Renderer>();

                Light glow = body.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = new Color(1f, 0.5f, 0.15f);
                glow.range = 8f;
                glow.intensity = 0f;              // dark until the lantern is lit
                _lanternLight[l.Id] = glow;
            }
        }

        /// <summary>A simple cone as a generated mesh (Unity 2022 has no cone primitive).</summary>
        private GameObject MakeCone(string name, float radius, float height)
        {
            GameObject go = new GameObject(name);
            const int segs = 16;
            Vector3[] v = new Vector3[segs + 2];
            v[0] = new Vector3(0f, height, 0f);                        // tip
            v[segs + 1] = new Vector3(0f, 0f, 0f);                     // base centre
            for (int i = 0; i < segs; i++)
            {
                double a = i * 2.0 * Math.PI / segs;
                v[i + 1] = new Vector3((float)(radius * Math.Cos(a)), 0f, (float)(radius * Math.Sin(a)));
            }
            int[] tri = new int[segs * 6];
            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                tri[i * 6 + 0] = 0; tri[i * 6 + 1] = i + 1; tri[i * 6 + 2] = next + 1;            // side
                tri[i * 6 + 3] = segs + 1; tri[i * 6 + 4] = next + 1; tri[i * 6 + 5] = i + 1;     // base
            }
            Mesh mesh = new Mesh { name = "ConeMesh" };
            mesh.vertices = v;
            mesh.triangles = tri;
            mesh.RecalculateNormals();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            go.AddComponent<MeshRenderer>();
            return go;
        }

        private Transform BuildPlayer()
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "May"; // Mây, the lantern keeper — ASCII name, texture is the hat
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            body.GetComponent<Renderer>().sharedMaterial = _matPlayer;

            GameObject hat = MakeCone("NonLa", 0.42f, 0.22f);   // the nón lá, a flat cone
            hat.transform.SetParent(body.transform);
            hat.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            hat.GetComponent<Renderer>().sharedMaterial = _matMote;

            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere); // the little lantern
            hand.name = "TayDen";
            hand.transform.SetParent(body.transform);
            hand.transform.localPosition = new Vector3(0.55f, 0f, 0.4f);
            hand.transform.localScale = new Vector3(0.22f, 0.3f, 0.22f);
            hand.GetComponent<Renderer>().sharedMaterial = _matMote;

            return body.transform;
        }

        private void FixedUpdate()
        {
            _accum += Time.timeScale > 0f ? Time.fixedDeltaTime : 0f;
            while (_accum >= 1f / Simulation.TicksPerSecond && !_sim.IsOver)
            {
                _accum -= 1f / Simulation.TicksPerSecond;
                _sim.Step(LiveInput());
                FlushEvents();
            }
            SyncVisuals();
            if (Input.GetKeyDown(KeyCode.R) && _sim.IsOver) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private InputFrame LiveInput()
        {
            InputFrame f = new InputFrame();
            f.Tick = _sim.Tick;
            f.MoveX = Input.GetAxisRaw("Horizontal");     // -1..1, the core normalises long vectors
            f.MoveZ = Input.GetAxisRaw("Vertical");
            if (Input.GetMouseButton(1)) _camYaw += Input.GetAxis("Mouse X") * 3.0f;   // drag to look
            f.CamYawDeg = _camYaw;
            f.Jump = Input.GetKey(KeyCode.Space);
            f.Interact = Input.GetKey(KeyCode.E);
            f.Pulse = Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0);       // edge = one pulse
            return f;
        }

        private void FlushEvents()
        {
            foreach (SimEvent e in _sim.PendingEvents)
                if (e.Name == "pulse") _pulseFlashUntil = Time.time + 0.25f;
        }

        private void SyncVisuals()
        {
            // motes
            foreach (Mote m in _sim.Motes)
            {
                GameObject v;
                if (!_moteVis.TryGetValue(m.Id, out v))
                {
                    v = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    v.name = "Mote_" + m.Id;
                    v.transform.localScale = Vector3.one * 0.25f;
                    v.GetComponent<Renderer>().sharedMaterial = _matMote;
                    Collider c = v.GetComponent<Collider>(); if (c != null) Destroy(c);
                    _moteVis[m.Id] = v;
                }
                float bob = 0.8f + 0.15f * Mathf.Sin((Time.time + m.Id.GetHashCode()) * 3f);
                v.transform.position = new Vector3((float)m.Pos.X, bob, (float)m.Pos.Z);
            }
            foreach (KeyValuePair<string, GameObject> kv in _moteVis)
                if (!Contains(_sim.Motes, kv.Key)) kv.Value.SetActive(false);

            // shades
            foreach (Shade s in _sim.Shades)
            {
                GameObject v;
                if (!_shadeVis.TryGetValue(s.Id, out v))
                {
                    v = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    v.name = "Shade_" + s.Id;
                    v.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
                    v.GetComponent<Renderer>().sharedMaterial = _matShade;
                    Collider c = v.GetComponent<Collider>(); if (c != null) Destroy(c);
                    _shadeVis[s.Id] = v;
                }
                v.transform.position = new Vector3((float)s.Pos.X, (float)s.Pos.Y, (float)s.Pos.Z);
                v.GetComponent<Renderer>().sharedMaterial = s.State == ShadeState.stunned ? _matLanternOff : _matShade;
            }
            foreach (KeyValuePair<string, GameObject> kv in _shadeVis)
                if (!ContainsShade(_sim.Shades, kv.Key)) kv.Value.SetActive(false);

            // lanterns: the light turns on when the core says lit
            foreach (Lantern l in _sim.Lanterns)
            {
                Light glow;
                if (_lanternLight.TryGetValue(l.Id, out glow)) glow.intensity = l.Lit ? 2.5f : 0f;
                Renderer r;
                if (_lanternBody.TryGetValue(l.Id, out r)) r.sharedMaterial = l.Lit ? _matLanternOn : _matLanternOff;
            }

            // Mây
            _playerVis.position = new Vector3((float)_sim.Player.Pos.X, (float)_sim.Player.Pos.Y + 0.9f, (float)_sim.Player.Pos.Z);
            _playerVis.rotation = Quaternion.Euler(0f, (float)_sim.Player.FacingDeg, 0f);
            Color flash = _sim.Player.InvulnTicks > 0 && (Time.renderedFrameCount / 6) % 2 == 0
                ? new Color(1f, 0.4f, 0.4f) : new Color(0.9f, 0.85f, 0.7f);
            _playerVis.GetComponent<Renderer>().sharedMaterial.color = flash;

            // third-person camera behind the player, looking along the input yaw
            double rad = _camYaw * Math.PI / 180.0;
            Vector3 head = _playerVis.position + Vector3.up * 1.2f;
            _cam.transform.position = head + new Vector3((float)Math.Sin(rad) * 6f, 2.2f, (float)Math.Cos(rad) * 6f);
            _cam.transform.rotation = Quaternion.LookRotation(head - _cam.transform.position);
        }

        private static bool Contains(List<Mote> list, string id) { foreach (Mote m in list) if (m.Id == id) return true; return false; }
        private static bool ContainsShade(List<Shade> list, string id) { foreach (Shade s in list) if (s.Id == id) return true; return false; }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 340, 200));
            GUILayout.Label("ĐẢO ĐÈN LỒNG — Night 1   seed " + _seed);
            GUILayout.Label("Lửa (flames):  " + Row(_sim.Player.Flames));
            GUILayout.Label("Đom đóm (motes): " + _sim.Player.Carried + " / " + _sim.T.CarryCapacity
                            + "   (pulse costs " + _sim.T.PulseCost + ")");
            GUILayout.Label("Đèn đã thắp (lanterns lit): " + LitCount() + " / " + _sim.T.LanternCountNight1);
            GUILayout.Label("Điểm (score): " + _sim.Score);
            GUILayout.Label("Thời gian còn lại: " + (int)Math.Max(0, Math.Floor(_sim.TimeLeftS)) + " s");
            GUILayout.EndArea();

            if (Time.time < _pulseFlashUntil)
                GUI.Label(new Rect(Screen.width / 2f - 120, Screen.height / 2f + 40, 240, 30), "— Tỏa Sáng! —");

            if (_sim.IsOver)
            {
                string msg = _sim.State == "won" ? "Win — all lanterns lit!" : "Lose — " + _sim.OverReason;
                GUILayout.BeginArea(new Rect(Screen.width / 2f - 220, Screen.height / 2f - 70, 440, 140));
                GUILayout.Label("<b>" + msg + "</b>");
                GUILayout.Label("Final score: " + _sim.Score);
                GUILayout.Label("Press R to replay");
                GUILayout.EndArea();
            }
            else
            {
                GUI.Label(new Rect(16, Screen.height - 36, Screen.width - 32, 24),
                          "WASD move  ·  Space jump  ·  E deposit  ·  Q / click pulse  ·  hold right-drag: camera");
            }
        }

        private string Row(int n) { return new string('●', Math.Max(0, n)); }
        private int LitCount() { int c = 0; foreach (Lantern l in _sim.Lanterns) if (l.Lit) c++; return c; }

        private static Material Mat(Color baseColor, Color emissive)
        {
            Material m = new Material(Shader.Find("Standard"));
            m.color = baseColor;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emissive);
            return m;
        }
    }
}
