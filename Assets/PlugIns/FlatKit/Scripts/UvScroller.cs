using UnityEngine;

namespace FlatKit
{
    public class UvScroller : MonoBehaviour
    {
        public  Material targetMaterial;
        public  float    speedX;
        public  float    speedY;
        private Vector2  initOffset;

        private Vector2 offset;

        private void Start()
        {
            offset     = targetMaterial.mainTextureOffset;
            initOffset = targetMaterial.mainTextureOffset;
        }

        private void Update()
        {
            offset.x                         += speedX * Time.deltaTime;
            offset.y                         += speedY * Time.deltaTime;
            targetMaterial.mainTextureOffset =  offset;
        }

        private void OnDisable()
        {
            targetMaterial.mainTextureOffset = initOffset;
        }
    }
}