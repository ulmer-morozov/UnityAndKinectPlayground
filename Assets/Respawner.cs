using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets
{
    public class Respawner : MonoBehaviour
    {
        public int CurrentPreset;

        private readonly IList<FillerPreset> _presets;
        private readonly IList<GameObject> _fillInstances;

        public Respawner()
        {
            _presets = new List<FillerPreset>();
            _fillInstances = new List<GameObject>();
        }

        public void Awake()
        {
            _presets.Add(new FillerPreset("Ball", 1000, 0.4f, 0.2f));
            _presets.Add(new FillerPreset("Skull", 50, 0.2f, 1.1f));
        }

        public void Start()
        {
            if (!_presets.Any())
                return;

            SetPreset(0);
        }

        private void SetPreset(FillerPreset preset)
        {
            if (preset == null)
            {
                Debug.LogError("Не удалось выставить презет, так как он null");
                return;
            }

            foreach (var instance in _fillInstances)
                Destroy(instance);

            var etalon = Resources.Load(preset.ResourceName) as GameObject;
            if (etalon == null)
            {
                Debug.LogError(string.Format("Не удалось найти ресурс {0}", preset.ResourceName));
                return;
            }

            const float dispersion = 5.4f;

            for (var i = 0; i < preset.Count; i++)
            {
                var newObject = Instantiate(etalon, Vector3.zero, Quaternion.identity);

                var posX = dispersion * Random.value - dispersion / 2;
                var posZ = dispersion * Random.value - dispersion / 2;
                var posY = 2 + i / 10 * 2 * preset.Distance;

                newObject.transform.position = new Vector3(posX, posY, posZ);
                newObject.transform.localScale = preset.LocalScale;

                _fillInstances.Add(newObject);
            }
        }

        private void SetPreset(int presetPos)
        {
            if (presetPos >= _presets.Count || CurrentPreset < 0)
                presetPos = 0;

            var preset = _presets[presetPos];
            CurrentPreset = presetPos;
            SetPreset(preset);
        }

        public void Update()
        {
            if (Input.GetKeyDown("r"))
            {
                SetPreset(CurrentPreset);
            }
            if (Input.GetKeyDown("space"))
            {
                CurrentPreset++;
                CurrentPreset = CurrentPreset % _presets.Count;
                SetPreset(CurrentPreset);
            }
        }


    }
}
