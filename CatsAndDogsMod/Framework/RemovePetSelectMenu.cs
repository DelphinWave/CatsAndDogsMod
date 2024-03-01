using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace CatsAndDogsMod.Framework
{
    class RemovePetSelectMenu : IClickableMenu
    {
        // Handling Pet & Skin
        public int currentPetIndex = 0;
        public Dictionary<string, Texture2D> petTextureMap; // string pet name

        // Menu Textures
        public ClickableTextureComponent petPreview;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent okButton;

        // Constants
        private static readonly int petSpriteWidth, petSpriteHeight = petSpriteWidth = 32;
        private static readonly float petPreviewScale = 4f;
        private static readonly int petSpriteIndex = 4;
        private static readonly int menuPadding = 64;
        private static readonly int okButtonWidth, okButtonHeight = okButtonWidth = 64;
        private static readonly int backButtonWidth, forwardButtonWidth = backButtonWidth = 48;
        private static readonly int backButtonHeight, forwardButtonHeight = backButtonHeight = 44;

        private static readonly int maxWidthOfMenu = (petSpriteWidth * (int)petPreviewScale) + menuPadding;
        private static readonly int maxHeightOfMenu = (petSpriteHeight * (int)petPreviewScale) + menuPadding;

        private readonly int backButtonId = 44;
        private readonly int forwardButtonId = 33;
        private readonly int okButtonId = 46;

        public RemovePetSelectMenu(Dictionary<string, Texture2D> petTextureMap)
        {
            this.currentPetIndex = 0;
            this.petTextureMap = new Dictionary<string, Texture2D>(petTextureMap);
            resetBounds();
        }

        public Texture2D CurrentPetTexture => this.petTextureMap.ElementAt(currentPetIndex).Value;

        public override void receiveGamePadButton(Buttons b)
        {
            // TODO: add fix for controller
            base.receiveGamePadButton(b);
            if (b == Buttons.LeftTrigger)
            {
                this.currentPetIndex--;
                if (this.currentPetIndex < 1)
                    this.currentPetIndex = this.petTextureMap.Count -1;

                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                updatePetPreview();
            }
            if (b == Buttons.RightTrigger)
            {
                this.currentPetIndex++;
                if (this.currentPetIndex >= petTextureMap.Count)
                    this.currentPetIndex = 0;

                this.forwardButton.scale = this.forwardButton.baseScale;
                Game1.playSound("shwip");
                updatePetPreview();
            }
            if (b == Buttons.A)
            {
                selectPetToRemove();
                base.exitThisMenu();
                Game1.playSound("smallSelect");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (this.backButton.containsPoint(x, y))
            {
                this.currentPetIndex--;
                if (this.currentPetIndex < 1)
                    this.currentPetIndex = this.petTextureMap.Count - 1;

                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                updatePetPreview();
            }
            if (this.forwardButton.containsPoint(x, y))
            {
                this.currentPetIndex++;
                if (this.currentPetIndex >= petTextureMap.Count)
                    this.currentPetIndex = 0;

                this.forwardButton.scale = this.forwardButton.baseScale;
                Game1.playSound("shwip");
                updatePetPreview();
            }
            if (this.okButton.containsPoint(x, y))
            {
                selectPetToRemove();
                base.exitThisMenu();
                Game1.playSound("smallSelect");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.backButton.tryHover(x, y);
            this.forwardButton.tryHover(x, y);
            this.okButton.tryHover(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.resetBounds();
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);

            IClickableMenu.drawTextureBox(b, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White);
            base.draw(b);
            this.petPreview.draw(b);
            this.backButton.draw(b);
            this.forwardButton.draw(b);
            this.okButton.draw(b);
            drawMouse(b);
        }

        private void selectPetToRemove()
        {
            // TODO: test
            var petToRemove = petTextureMap.ElementAt(currentPetIndex);

            ModEntry.RemovePet(petToRemove.Key);
        }

        private void updatePetPreview()
        {
            this.petPreview = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + menuPadding, base.yPositionOnScreen + menuPadding, petSpriteWidth, petSpriteHeight), CurrentPetTexture, Game1.getSourceRectForStandardTileSheet(CurrentPetTexture, petSpriteIndex, petSpriteWidth, petSpriteHeight), petPreviewScale);
        }

        private void resetBounds()
        {
            base.xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfMenu / 2 - IClickableMenu.spaceToClearSideBorder;
            base.yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfMenu / 2 - IClickableMenu.spaceToClearTopBorder;
            base.width = maxWidthOfMenu + IClickableMenu.spaceToClearSideBorder;
            base.height = maxHeightOfMenu + IClickableMenu.spaceToClearTopBorder;
            base.initialize(base.xPositionOnScreen, base.yPositionOnScreen, base.width + menuPadding, base.height + menuPadding, showUpperRightCloseButton: true);

            this.updatePetPreview();

            this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + menuPadding, base.yPositionOnScreen + (petSpriteHeight * (int)petPreviewScale) + menuPadding, backButtonWidth, backButtonHeight), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, backButtonId), 1f)
            {
                myID = backButtonId,
                rightNeighborID = forwardButtonId
            };
            this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - menuPadding - forwardButtonWidth, base.yPositionOnScreen + (petSpriteHeight * (int)petPreviewScale) + menuPadding, forwardButtonWidth, forwardButtonHeight), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, forwardButtonId), 1f)
            {
                myID = forwardButtonId,
                leftNeighborID = backButtonId,
                rightNeighborID = okButtonId
            };
            this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - okButtonWidth - (menuPadding / 4), base.yPositionOnScreen + base.height - okButtonHeight - (menuPadding / 4), okButtonWidth, okButtonHeight), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, okButtonId), 1f)
            {
                myID = okButtonId,
                leftNeighborID = forwardButtonId,
                rightNeighborID = -99998
            };
        }
    }
}
