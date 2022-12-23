using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
/* Done
* The blackHole must grow for each block it eats X
* The blocks must be pulled off then be dragged to the center of the black hole X
* Must grow by block size X
* Object must face the current velocity x
 */
/* TODO
* They can fade to red or disappear imedatly 
* The black hole must have a screen warping affect to represent sucking in the area
* Drag players toward it
* Increasing blackholes force towards players the closer they are
* Show on map
*/
namespace BlackHole
{
    public class NPCBlackHole : ModNPC
    {
        [AutoloadBossHead]
        public class FakeTile
        {
            public Vector2 position;
            public Vector2 velocity;
            public Rectangle frame;
            public Texture2D texture;
            public float newton = 0.25f;

            public FakeTile(Terraria.Tile tile, Vector2 position)
            {
                this.position = position;
                this.texture = ((Texture2D)TextureAssets.Tile[tile.TileType]);
                this.frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
            }
            public bool Update(SpriteBatch spriteBatch) /*True if object should be deleted*/
            {
                position += velocity;
                float angle = (float) Math.Atan2(velocity.Y, velocity.X);
                spriteBatch.Draw(this.texture, this.position - Main.screenPosition - new Vector2(8, 0), frame, Color.White, angle, new Vector2(0), 1.0f, SpriteEffects.None, 0f);
                return false;
                //render
            }
        };
        List<FakeTile> fakeTiles = new List<FakeTile>();
        NPCBlackHole()
        {
        }
        public override bool CheckDead()
        {
            Entity.active = true;
            return false;
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override void SetDefaults()
        {
            Texture2D texture = ((Texture2D)Terraria.ModLoader.ModContent.Request<Texture2D>(nameof(BlackHole) + "/" + nameof(NPCBlackHole)));
            Entity.width = texture.Width;
            Entity.height = texture.Height;
            Entity.lifeMax = int.MaxValue / 10;
            Entity.boss = true;
            Entity.noGravity = true;
            Entity.immortal = false;
            Entity.despawnEncouraged = false;
            Array.ForEach(Entity.buffImmune, buffImmune => { buffImmune = true; });
            Entity.noTileCollide = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            ReLogic.Content.Asset<Texture2D> texture = TextureAssets.Npc[Entity.type];
            Vector2 textureSize = texture.Size();
        restartLoop:
            foreach (FakeTile fakeTile in fakeTiles)
            {
                float radius = 1;
                float fakeTileDestroyDistance = radius * Entity.scale;
                if (fakeTile.Update(spriteBatch) || fakeTile.position.Distance(Entity.Center) < fakeTileDestroyDistance)
                {
                    Entity.scale += fakeTile.newton;
                    Entity.position.X += ((Entity.Center.X - fakeTile.position.X < 0) ? fakeTile.newton : -fakeTile.newton); //; += //fakeTile.Size /2;
                    Entity.position.Y += ((Entity.Center.Y - fakeTile.position.Y < 0) ? fakeTile.newton : -fakeTile.newton); //; += //fakeTile.Size /2;
                    fakeTiles.Remove(fakeTile);
                    //Main.NewText("Nom!", null);
                    goto restartLoop; //restructure loop encoreged
                }
                float force = fakeTile.newton / fakeTile.position.Distance(Entity.position) * 1.50f;
                
                fakeTile.velocity.X += (fakeTile.position.X + fakeTile.newton / 2 - Entity.Center.X < 0) ? force : -force; //if left of BlackHole then force else -force
                fakeTile.velocity.Y += (fakeTile.position.Y + fakeTile.newton / 2 - Entity.Center.Y < 0) ? force : -force; //if Top of BlackHole then force else -force
            }

            spriteBatch.Draw(((Texture2D)texture), Entity.position - textureSize / 2 * Entity.scale - Main.screenPosition, new Rectangle(0,0, (int)textureSize.X, (int)textureSize.Y), Color.White, 0, new Vector2(0), Entity.scale, SpriteEffects.None, 0f);

            return false;
        }
        public override void AI()
        {
            Vector2 startPosition = Entity.TopLeft;
            Vector2 endPosition = Entity.BottomRight;

            Vector2 position = startPosition;
            int radius = 16 * 2;
            for (int tileY = (int)startPosition.Y - radius * 16; tileY <= endPosition.Y + radius * 16; tileY += 16)
            {
                for (int tileX = (int)startPosition.X - radius * 16; tileX <= endPosition.X + radius * 16; tileX += 16)
                {
                    Tile tile = Framing.GetTileSafely((int)tileX / 16, tileY / 16);
                    if (tile.HasTile == false)
                        continue;
                    
                    fakeTiles.Add(new FakeTile(tile, new Vector2(tileX, tileY)));
                }
            }
            for (int tileY = (int)startPosition.Y - radius * 16; tileY <= endPosition.Y + radius * 16; tileY += 16) //Fixes issue that causes partal object distruction like sunflowers having only one tile of their body taken and the rest deleted
            { //Bypassing KillTile should prevent the need for this loop
                for (int tileX = (int)startPosition.X - radius * 16; tileX <= endPosition.X + radius * 16; tileX += 16)
                {
                    WorldGen.KillTile(tileX / 16, tileY / 16, noItem: true);
                }
            }
            /*
            foreach (Player player in Main.player)
            {
                if (!player.active)
                    continue;
                if (NPC.Distance(player.position) > 10)
                    continue;
            }
            */
        }
    }
}