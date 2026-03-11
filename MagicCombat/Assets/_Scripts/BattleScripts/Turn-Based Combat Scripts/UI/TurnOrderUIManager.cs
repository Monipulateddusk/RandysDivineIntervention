using UnityEngine;

namespace TurnBased
{
    public class TurnOrderUIManager
    {
        private GameObject ScrollableRootGameObject, ContentParentGameObject;
        private GameObject SummoningCirclePrefab, ItemParentPrefab, SlotPrefab;
        private UnityEngine.UI.Image SummoningCircleImage;
        private Color SUMMONING_CIRCLE_COLOUR = new(0.4941177f, 0.7372549f, 1);
        public TurnOrderUIManager(GameObject ScrollableContentRoot) 
        {
            this.ScrollableRootGameObject = ScrollableContentRoot;

            BattleMediator.OnUpdateTurnOrder += BattleMediator_OnUpdateTurnOrder;
        }

        ~TurnOrderUIManager()
        {
            BattleMediator.OnUpdateTurnOrder -= BattleMediator_OnUpdateTurnOrder;
        }

        private GameObject GetSummoningCircleUIPrefab()
        {
            if (this.SummoningCirclePrefab == null && this.SummoningCircleImage != null) {
                GameObject gO = new("SummoningCircle");
                gO.AddComponent<RectTransform>();
                gO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image img = gO.AddComponent<UnityEngine.UI.Image>();
                img.sprite = this.SummoningCircleImage.sprite;
                img.color = SUMMONING_CIRCLE_COLOUR;

                gO.AddComponent<UnityEngine.UI.Outline>();

                this.SummoningCirclePrefab = gO;
            } 
            if(this.SummoningCirclePrefab != null) return this.SummoningCirclePrefab;
            return null;
        }

        private RectTransform GetBackgroundTransformOfItem(GameObject itemGameObject)
        {
            if (itemGameObject != null)
            {
                Transform sliderBG = itemGameObject.transform.Find("SliderBG");
                /*  Checking to ensure the SliderBG was found.  */
                if (sliderBG == null)
                {
                    Debug.LogWarning("ERROR: SliderBG NOT FOUND!");
                    return null;
                }

                Transform background = sliderBG.Find("Background").transform;
                if (background == null)
                {
                    Debug.LogWarning("ERROR: background NOT FOUND!");
                    return null;
                }
                return ((RectTransform)background);
            }
            Debug.LogWarning("ERROR: NULL REFERANCE itemGameObject");
            return null;
        }

        private GameObject CreateItem(Transform parent)
        {
            return GameObject.Instantiate(this.ItemParentPrefab, parent);
        }
        private GameObject CreateSummoningCircle(Transform parent)
        {
            return GameObject.Instantiate(this.SummoningCirclePrefab, parent);
        }

        private void CreateTurnOrderUI(System.Collections.Generic.List<UnitIndex> list)
        {
            /*  For each entry in the list, we need to create a slot for each entry.
             *  The 0-index slot, needs to be on the Summoning Circle however.
             */



            for(int i = 0; i < list.Count; i++)
            {
                GameObject itemObject = CreateItem(ContentParentGameObject.transform);
                RectTransform bgTransform = GetBackgroundTransformOfItem(itemObject);

                if (i == 0)
                {
                    GetSummoningCircleUIPrefab();
                    GameObject summoningCircleParent = CreateSummoningCircle(bgTransform);
                }
            }
        }

        private void BattleMediator_OnUpdateTurnOrder(System.Collections.Generic.List<UnitIndex> list)
        {
            CreateTurnOrderUI(list);
        }
    }
}