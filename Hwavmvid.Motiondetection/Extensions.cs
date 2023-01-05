using System;
using System.Collections.Generic;
using System.Linq;

namespace Hwavmvid.Motiondetection
{

    public static class Extensions
    {

        public static int Bitrate(this byte[] bytes)
        {
            int bitrate = 0;
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] == 1) bitrate++;
            return bytes.Length - bitrate;
        }

        public static bool MotiondetectionByBitrate(this int fluctation, int bitrate1, int bitrate2)
        {
            return Math.Abs(bitrate1 - bitrate2) > fluctation;
        }

        public static int MotiondetectionByterate(this byte[] bytes)
        {
            var bytes1 = bytes.Take(bytes.Length / 2).ToList();
            var bytes2 = bytes.Skip(bytes.Length / 2).ToList();

            var bytearraylist1 = bytes1.ChunkItems(8).ToList();
            var bytearraylist2 = bytes2.ChunkItems(8).ToList();

            int byterate = 0;
            foreach (var item in bytearraylist1.Select((chunk1, index) => new { chunk1 = chunk1, index = index }))
            {
                long c641 = BitConverter.ToInt64(item.chunk1.ToArray());
                long c642 = BitConverter.ToInt64(bytearraylist2[item.index].ToArray());
                bool matched = c641.Equals(c642);
                if (!matched)
                    byterate++;
            }

            return byterate;
        }

        public static List<List<byte>> ChunkItems(this List<byte> bytes, int chunksize)
        {
            List<List<byte>> items = new List<List<byte>>();
            foreach (var container in bytes.Select((item, index) => new { item = item, index = index })) {

                if (container.index % chunksize == 0)
                    items.Add(new List<byte>());

                items.Last().Add(container.item);
            }

            while (items.Last().Count() != chunksize)
            {
                byte outofrangevalue = 0;
                items.Last().Add(outofrangevalue);
            }

            return items;
        }

    }
}