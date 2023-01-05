using Hwavmvid.Rouletteshared.Enums;

namespace Hwavmvid.Rouletteshared.Items
{
    public class RouletteBetoptionsItem
    {

        public RouletteBetoptionsItem(RouletteBetoptionsType type)
        {

            if (type == RouletteBetoptionsType.Red ||
                type == RouletteBetoptionsType.Black)
            {
                this.Key = type;
                this.Value = type.ToString();
            }

            else if (type == RouletteBetoptionsType.FirstHalf)
            {
                this.Key = type;
                this.Value = "1/Half";
            }

            else if (type == RouletteBetoptionsType.SecondHalf)
            {
                this.Key = type;
                this.Value = "2/Half";
            }

            else if (type == RouletteBetoptionsType.FirstTwelve)
            {
                this.Key = type;
                this.Value = "1/12";
            }

            else if (type == RouletteBetoptionsType.SecondTwelve)
            {
                this.Key = type;
                this.Value = "2/12";
            }

            else if (type == RouletteBetoptionsType.ThirdTwelve)
            {
                this.Key = type;
                this.Value = "3/12";
            }

        }
        public RouletteBetoptionsType Key { get; set; }
        public string Value { get; set; }

    }
}
