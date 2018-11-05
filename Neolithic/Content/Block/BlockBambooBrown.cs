﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace TheNeolithicMod
{
    public class BlockBambooBrown : Block, ITreeGenerator
    {
        Block greenSeg1;
        Block greenSeg2;
        Block greenSeg3;

        Block brownSeg1;
        Block brownSeg2;
        Block brownSeg3;

        Block leaves;

        static Random rand = new Random();


        public override void OnLoaded(ICoreAPI api)
        {
            ICoreServerAPI sapi = api as ICoreServerAPI;
            if (sapi != null)
            {
                if (Code.Path.Equals("bamboobrowngrown-segment1"))
                {
                    sapi.RegisterTreeGenerator(new AssetLocation("bamboobrowngrown"), this);
                }
            }

            if (brownSeg1 == null)
            {
                IBlockAccessor blockAccess = api.World.BlockAccessor;

                brownSeg1 = blockAccess.GetBlock(new AssetLocation("bamboobrowngrown-segment1"));
                brownSeg2 = blockAccess.GetBlock(new AssetLocation("bamboobrowngrown-segment2"));
                brownSeg3 = blockAccess.GetBlock(new AssetLocation("bamboobrowngrown-segment3"));

                leaves = blockAccess.GetBlock(new AssetLocation("bambooleaves-grown"));
            }

        }


        public string Type()
        {
            return LastCodePart(1);
        }

        public Block NextSegment(IBlockAccessor blockAccess)
        {

            string part = LastCodePart();


            return Type() == "green" ?
                (part == "segment1" ? greenSeg2 : (part == "segment2" ? greenSeg3 : null)) :
                (part == "segment1" ? brownSeg2 : (part == "segment2" ? brownSeg3 : null))
            ;
            
        }


        public void GrowTree(IBlockAccessor blockAccessor, BlockPos pos, float sizeModifier = 1, float vineGrowthChance = 0, float forestDensity = 0)
        {
            double quantity = 1 + (1 + rand.NextDouble() * 4) * (1 - forestDensity) * (1 - forestDensity);

            while (quantity-- > 0)
            {
                GrowStalk(blockAccessor, pos.UpCopy(), sizeModifier, vineGrowthChance);

                pos.X += rand.Next(5) - 2;
                pos.Z += rand.Next(5) - 2;

                // Test up to 2 blocks up and down.
                bool foundSuitableBlock = false;
                for (int y = 2; y >= -2; y--)
                {
                    Block block = blockAccessor.GetBlock(pos.X, pos.Y + y, pos.Z);
                    if (block.Fertility > 0)
                    {
                        pos.Y = pos.Y + y;
                        foundSuitableBlock = true;
                        break;
                    }
                }
                if (!foundSuitableBlock) break;
            }
        }

        private void GrowStalk(IBlockAccessor blockAccessor, BlockPos upos, float sizeModifier, float vineGrowthChance)
        {
            Block block = this;
            int height = 7 + rand.Next(4);
            int nextSegmentAtHeight = height / 3;

            BlockPos npos = upos.Copy();

            for (int i = 0; i < height; i++)
            {
                if (!blockAccessor.GetBlock(upos).IsReplacableBy(block)) break;

                blockAccessor.SetBlock(block.BlockId, upos);

                if (nextSegmentAtHeight <= i)
                {
                    block = ((BlockBambooBrown)block).NextSegment(blockAccessor);
                    nextSegmentAtHeight += height / 3;
                }

                if (block == null) break;

                if (block == greenSeg3 || block == brownSeg3)
                {
                    foreach (BlockFacing facing in BlockFacing.ALLFACES)
                    {
                        float chanceFac = facing == BlockFacing.UP ? 0 : 0.25f;

                        if (rand.NextDouble() > chanceFac)
                        {
                            npos.Set(upos.X + facing.Normali.X, upos.Y + facing.Normali.Y, upos.Z + facing.Normali.Z);

                            if (blockAccessor.GetBlock(npos).Replaceable >= leaves.Replaceable)
                            {
                                blockAccessor.SetBlock(leaves.BlockId, npos);
                            }
                            else continue;

                            foreach (BlockFacing facing2 in BlockFacing.ALLFACES)
                            {
                                if (rand.NextDouble() > 0.5)
                                {
                                    npos.Set(upos.X + facing.Normali.X + facing2.Normali.X, upos.Y + facing.Normali.Y + facing2.Normali.Y, upos.Z + facing.Normali.Z + facing2.Normali.Z);

                                    if (blockAccessor.GetBlock(npos).Replaceable >= leaves.Replaceable)
                                    {
                                        blockAccessor.SetBlock(leaves.BlockId, npos);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                upos.Up();
            }
        }




        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing)
        {
            if (this != brownSeg3 && this != capi.World.GetBlock(new AssetLocation("bamboo-placed-green-segment3"))) return base.GetRandomColor(capi, pos, facing);

            if (Textures == null || Textures.Count == 0) return 0;
            CompositeTexture tex;
            if (!Textures.TryGetValue(facing.Code, out tex))
            {
                tex = Textures.First().Value;
            }
            if (tex?.Baked == null) return 0;

            int color = capi.BlockTextureAtlas.GetRandomPixel(tex.Baked.TextureSubId);

            return capi.ApplyColorTintOnRgba(1, color, pos.X, pos.Y, pos.Z);
        }

    }
}
