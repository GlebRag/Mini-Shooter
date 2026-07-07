using UnityEngine;
using UnityEngine.UI;
using Runtime.Combat.Effects;

namespace Runtime.UI
{
    public class EnemyEffectsVisualizer : MonoBehaviour
    {
        [SerializeField] private EffectsManager _effectsManager;
        [SerializeField] private Image _iconDisplay;

        private void OnEnable()
        {
            if (_effectsManager == null) return;

            _effectsManager.OnEffectAdded += HandleEffectAdded;
            _effectsManager.OnEffectRemoved += HandleEffectRemoved;

            _iconDisplay.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (_effectsManager == null) return;

            _effectsManager.OnEffectAdded -= HandleEffectAdded;
            _effectsManager.OnEffectRemoved -= HandleEffectRemoved;
        }

        private void HandleEffectAdded(StatusEffect effect)
        {
            if (effect.Config.Icon != null)
            {
                _iconDisplay.sprite = effect.Config.Icon;
                _iconDisplay.gameObject.SetActive(true);
            }
        }

        private void HandleEffectRemoved(StatusEffect effect)
        {
            _iconDisplay.gameObject.SetActive(false);
        }
    }
}