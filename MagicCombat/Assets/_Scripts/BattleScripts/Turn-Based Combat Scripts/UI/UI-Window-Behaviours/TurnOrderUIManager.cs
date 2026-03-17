using System.Collections.Generic;
using TurnBased.UI;
using UnityEngine;

namespace TurnBased
{
    public class DialogueBoxAttachment
    {
        public event System.Action OnDestroy;
        private SizableWindowBaseBehaviour dialogueBoxOwner;

        public DialogueBoxAttachment(SizableWindowBaseBehaviour dialogueBoxOwner)
        {
            this.dialogueBoxOwner = dialogueBoxOwner;
        }
        
        public virtual void Destroy()
        {
            OnDestroy?.Invoke();
        }

        public void SetDialogueBoxOwner(SizableWindowBaseBehaviour dBB)
        {
            if (dBB == null)
            {
                Debug.Log("Attempting to assign a NULL referance");
                return;
            }
            
            this.dialogueBoxOwner = dBB;
        }
        public SizableWindowBaseBehaviour GetDialogueBoxOwner() => dialogueBoxOwner;
    }

    public class DefaultDialogueBoxAttachment : DialogueBoxAttachment
    {
        public DefaultDialogueBoxAttachment(SizableWindowBaseBehaviour dialogueBoxOwner) : base(dialogueBoxOwner)
        {
        }
    }

    public class TurnOrderUIManager : DialogueBoxAttachment
    {
        System.Collections.Generic.List<GameObject> instanciatedItems;
        private UICollection_SO UI_PrefabData;
        private ScrollableContentPrefabData scrollablePrefabData;
        private GameObject SummoningCirclePrefab;

        private Color SUMMONING_CIRCLE_COLOUR = new(0.4941177f, 0.7372549f, 1);
        public TurnOrderUIManager(DialogueBoxBehaviour dialogueBoxOwner, UICollection_SO UI_PrefabData, ScrollableContentPrefabData contentData) : base(dialogueBoxOwner)
        {
            this.UI_PrefabData = UI_PrefabData;
            this.scrollablePrefabData = contentData;

            this.instanciatedItems = new();

            CreateTurnOrderUI(BattleMediator.Instance.GetCurrentUnit(), BattleMediator.Instance.GetTurnOrderList());
            BattleMediator.OnUpdateTurnOrder += BattleMediator_OnUpdateTurnOrder;
        }

        public override void Destroy()
        {
            base.Destroy();
            DeleteIcons();
            BattleMediator.OnUpdateTurnOrder -= BattleMediator_OnUpdateTurnOrder;
        }

        ~TurnOrderUIManager()
        {
            Destroy();
        }

        private GameObject GetSummoningCircleUIPrefab()
        {
            if (this.SummoningCirclePrefab == null && UI_PrefabData.SummoningCircleSprite != null) {
                GameObject gO = new("SummoningCircle");
                RectTransform transform = gO.AddComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = new Vector2(1,1);
                transform.SetLeft(-50);
                transform.SetTop(-50);
                transform.SetRight(-50);
                transform.SetBottom(-50);

                gO.AddComponent<CanvasRenderer>();
                UnityEngine.UI.Image img = gO.AddComponent<UnityEngine.UI.Image>();
                img.sprite = this.UI_PrefabData.SummoningCircleSprite;
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
                return itemPrefabData.GetBackgroundTransform();
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
                if(slotData.GetUnitImageTransform().gameObject.TryGetComponent(out UnityEngine.UI.Image image))
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
            instanciatedItems.Clear();
            instanciatedItems = new();
        }

        private void CreateTurnOrderUI(UnitIndex? currentUnit, System.Collections.Generic.List<UnitIndex> subsequentUnits)
        {
            if(currentUnit == null) { Debug.LogWarning("CURRENT UNIT IS NULL!"); return; }

            List<UnitIndex> turnOrder = new()
            {
                currentUnit.Value
            };
            turnOrder.AddRange(subsequentUnits);
            /*  For each entry in the list, we need to create a slot for each entry.
             *  The 0-index slot, needs to be on the Summoning Circle however.
             */

            GameObject itemObject, slot, summoningCircleParent;
            RectTransform bgTransform;
            BaseBattleUnit battleUnit;
            for (int i = 0; i < turnOrder.Count; i++)
            {
                itemObject = CreateItem(this.scrollablePrefabData.GetScrollableContentTransform());
                bgTransform = GetBackgroundTransformOfItem(itemObject);

                if (i == 0)
                {
                    GetSummoningCircleUIPrefab();
                    summoningCircleParent = CreateSummoningCircle(bgTransform);
                    slot = CreateSlot(summoningCircleParent.transform);

                    /*  Get the image of the Unit. And Set it  */
                    battleUnit = BattleMediator.Instance.GetBattleUnitOfUnitIndex(turnOrder[i]);
                    SetImageOfSlot(slot, battleUnit.GetBaseUnit().sprite);
                }
                else
                {


                    slot = CreateSlot(bgTransform.transform);

                    /*  Get the image of the Unit. And Set it  */
                    battleUnit = BattleMediator.Instance.GetBattleUnitOfUnitIndex(turnOrder[i]);
                    SetImageOfSlot(slot, battleUnit.GetBaseUnit().sprite);
                }

                instanciatedItems.Add(itemObject);
            }
        }

        private void BattleMediator_OnUpdateTurnOrder(System.Collections.Generic.List<UnitIndex> list)
        {
            DeleteIcons();
            CreateTurnOrderUI(BattleMediator.Instance.GetCurrentUnit(), list);
        }
    }
}