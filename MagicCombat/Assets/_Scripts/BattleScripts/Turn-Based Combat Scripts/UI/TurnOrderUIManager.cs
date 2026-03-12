using TurnBased.UI;
using UnityEngine;

namespace TurnBased
{
    public class TurnOrderUIManager
    {
        System.Collections.Generic.List<GameObject> instanciatedItems;
        private UICollection_SO UI_PrefabData;
        private ScrollableContentPrefabData scrollablePrefabData;
        private GameObject SummoningCirclePrefab;

        private UnityEngine.Sprite SummoningCircleImage;
        private Color SUMMONING_CIRCLE_COLOUR = new(0.4941177f, 0.7372549f, 1);
        public TurnOrderUIManager(UICollection_SO UI_PrefabData, ScrollableContentPrefabData contentData) 
        {
            this.scrollablePrefabData = contentData;

            this.UI_PrefabData = UI_PrefabData; 
            this.SummoningCircleImage = UI_PrefabData.SummoningCircleSprite;

            CreateTurnOrderUI(BattleMediator.Instance.GetTurnOrderList());
            BattleMediator.OnUpdateTurnOrder += BattleMediator_OnUpdateTurnOrder;
        }

        ~TurnOrderUIManager()
        {
            DeleteIcons();
            BattleMediator.OnUpdateTurnOrder -= BattleMediator_OnUpdateTurnOrder;
        }

        private GameObject GetSummoningCircleUIPrefab()
        {
            if (this.SummoningCirclePrefab == null && this.SummoningCircleImage != null) {
                GameObject gO = new("SummoningCircle");
                gO.AddComponent<RectTransform>();
                gO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image img = gO.AddComponent<UnityEngine.UI.Image>();
                img.sprite = this.SummoningCircleImage;
                img.color = SUMMONING_CIRCLE_COLOUR;

                gO.AddComponent<UnityEngine.UI.Outline>();

                this.SummoningCirclePrefab = gO;
            } 
            if(this.SummoningCirclePrefab != null) return this.SummoningCirclePrefab;
            return null;
        }

        private RectTransform GetBackgroundTransformOfItem(GameObject itemGameObject)
        {
            if (itemGameObject != null && itemGameObject.TryGetComponent(out ItemPrefabData itemPrefabData))
            {
                return itemPrefabData.BackgroundTransform;
            }

            else
            {
                return null;
            }
        }

        private GameObject CreateItem(Transform parent)
        {
            return GameObject.Instantiate(this.UI_PrefabData.ScrollableItemPrefab, parent);
        }
        private GameObject CreateSummoningCircle(Transform parent)
        {
            return GameObject.Instantiate(this.SummoningCirclePrefab, parent);
        }
        private GameObject CreateSlot(Transform parent)
        {
            return GameObject.Instantiate(this.UI_PrefabData.ScrollableSlotPrefab, parent);
        }

        private void SetImageOfSlot(GameObject slot, UnityEngine.Sprite img)
        {
            if (slot != null && slot.TryGetComponent(out SlotPrefabData slotData))
            {
                if(slotData.UnitImageTransform != null && slotData.UnitImageTransform.gameObject.TryGetComponent(out UnityEngine.UI.Image image))
                {
                    image.sprite = img;
                }
                else
                {
                    Debug.LogWarning("ERROR: SLOT DOES NOT HAVE A CHILD CALLED 'UnitImage' OR DOES NOT HAVE AN IMAGE COMPONENT!");
                }
            }
            else
            {
                Debug.LogWarning("ERROR: SLOT IS NULL OR NOT NAMED 'SLOT'!");
            }
        }

        private void DeleteIcons()
        {
            foreach (GameObject obj in instanciatedItems)
            {
                GameObject.DestroyImmediate(obj);
            }
        }

        private void CreateTurnOrderUI(System.Collections.Generic.List<UnitIndex> list)
        {
            /*  For each entry in the list, we need to create a slot for each entry.
             *  The 0-index slot, needs to be on the Summoning Circle however.
             */

            GameObject itemObject, slot, summoningCircleParent;
            RectTransform bgTransform;
            BaseBattleUnit battleUnit;
            for (int i = 0; i < list.Count; i++)
            {
                itemObject = CreateItem(this.scrollablePrefabData.ScrollableContentTransform);
                bgTransform = GetBackgroundTransformOfItem(itemObject);

                if (i == 0)
                {
                    GetSummoningCircleUIPrefab();
                    summoningCircleParent = CreateSummoningCircle(bgTransform);
                    slot = CreateSlot(summoningCircleParent.transform);

                    /*  Get the image of the Unit. And Set it  */
                    battleUnit = BattleMediator.Instance.GetBattleUnitOfUnitIndex(list[i]);
                    SetImageOfSlot(slot, battleUnit.GetBaseUnit().sprite);
                }
                else
                {


                    slot = CreateSlot(bgTransform.transform);

                    /*  Get the image of the Unit. And Set it  */
                    battleUnit = BattleMediator.Instance.GetBattleUnitOfUnitIndex(list[i]);
                    SetImageOfSlot(slot, battleUnit.GetBaseUnit().sprite);
                }

                instanciatedItems.Add(itemObject);
            }
        }

        private void BattleMediator_OnUpdateTurnOrder(System.Collections.Generic.List<UnitIndex> list)
        {
            DeleteIcons();
            CreateTurnOrderUI(list);
        }
    }
}