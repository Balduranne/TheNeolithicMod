﻿using System;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace TheNeolithicMod
{
    class ChimneyBehavior : BlockBehavior
    {
        private AssetLocation[][] lookFor;
        public BlockPos pos;
        public IWorldAccessor world;
        long listenerId;
        public ChimneyBehavior(Block block) : base(block) { }

        public override void Initialize(JsonObject properties)
        {
            lookFor = properties["lookFor"].AsObject<AssetLocation[][]>();
        }

        public override bool TryPlaceBlock(IWorldAccessor aworld, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling)
        {
            pos = blockSel.Position;
            world = aworld; //idunno, becomes null and breaks if I don't do this
            listenerId = world.RegisterGameTickListener(ticker, 5000);
            return true;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            world.UnregisterGameTickListener(listenerId);
        }

        public void ticker(float dt)
        {
            checkBelow(pos, world);
        }

        public void checkBelow(BlockPos pos, IWorldAccessor world)
        {
            bool exists = false;
            Block check;
            foreach (var val in lookFor)
            {
                for (int y = 0; y < pos.Y; y++)
                {
                    check = world.BlockAccessor.GetBlock(pos + new BlockPos(0, -y, 0));
                    if (new ItemStack(check).Collectible.WildCardMatch(val[0]))
                    {
                        exists = true;
                        break;
                    }
                }
                check = world.BlockAccessor.GetBlock(pos);
                if (exists && new ItemStack(check).Collectible.WildCardMatch(val[1]))
                {
                    world.BlockAccessor.SetBlock(world.GetBlock(val[2]).BlockId, pos);
                    break;
                }
                if (!exists && new ItemStack(check).Collectible.WildCardMatch(val[2]))
                {
                    world.BlockAccessor.SetBlock(world.GetBlock(val[1]).BlockId, pos);
                    break;
                }
            }
            return;
        }
    }
}
