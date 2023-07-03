using UnityEngine;
using UnityEngine.UI;

namespace GameU
{
    public class HeadsUpDisplay : MonoBehaviour
    {
        [SerializeField]
        Image staminaBar;

        private Player player;
        private Vector2 originalSize;

        private void Start()
        {
            player = FindObjectOfType<Player>();
            originalSize = staminaBar.rectTransform.sizeDelta;
        }

        private void Update()
        {
            staminaBar.rectTransform.sizeDelta = new Vector2(originalSize.x * player.Stamina, originalSize.y);
        }
    }
}