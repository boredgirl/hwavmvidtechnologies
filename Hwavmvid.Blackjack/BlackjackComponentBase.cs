using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hwavmvid.Blackjack
{
    public class BlackjackComponentBase : ComponentBase
    {

        public BlackjackCardDeck ContextCardDeck { get; set; }
        public List<BlackjackCard> DealerCards { get; set; } = new List<BlackjackCard>();
        public List<BlackjackCard> ClientCards { get; set; } = new List<BlackjackCard>();
        public BlackjackGameStatus GameStatus { get; set; } = BlackjackGameStatus.NewGame;

        protected override async Task OnInitializedAsync()
        {
            this.ContextCardDeck = this.GetNewCarddeck();
            await base.OnInitializedAsync();
        }

        public BlackjackCardDeck GetNewCarddeck()
        {

            BlackjackCardDeck carddeck = new BlackjackCardDeck();

            List<string> colors = new List<string>();
            colors = Enum.GetValues<BlackjackCardColor>().Select(item => item.ToString()).ToList();

            foreach (var color in colors)
            {
                List<string> types = new List<string>();
                types = Enum.GetValues<BlackjackCardType>().Select(item => item.ToString()).ToList();

                foreach (var type in types.Where(item => item != BlackjackCardType.Placeholder.ToString()).Select((item, index) => new { item = item, index = index }))
                {

                    BlackjackCard card = new BlackjackCard();
                    card.Id = Guid.NewGuid().ToString();
                    card.Color = (BlackjackCardColor)Enum.Parse(typeof(BlackjackCardColor), color);
                    card.Type = BlackjackCardType.Two;
                    card.ImageUrl = card.Color.ToString();
                    card.ImageUrlExtension = card.ImageUrlExtension;

                    if (color == BlackjackCardColor.Clubs.ToString() ||
                        color == BlackjackCardColor.Hearts.ToString())
                    {
                        card.ImageFontColor = "#A31919";
                    }
                    else if (color == BlackjackCardColor.Spades.ToString() ||
                        color == BlackjackCardColor.Diamonds.ToString())
                    {
                        card.ImageFontColor = "#000000";
                    }

                    if (type.item == BlackjackCardType.Two.ToString())
                    {
                        card.Value = 2;
                        card.Abbr = "2";
                    }
                    else if (type.item == BlackjackCardType.Three.ToString())
                    {
                        card.Value = 3;
                        card.Abbr = "3";
                    }
                    else if (type.item == BlackjackCardType.Four.ToString())
                    {
                        card.Value = 4;
                        card.Abbr = "4";
                    }
                    else if (type.item == BlackjackCardType.Five.ToString())
                    {
                        card.Value = 5;
                        card.Abbr = "5";
                    }
                    else if (type.item == BlackjackCardType.Six.ToString())
                    {
                        card.Value = 6;
                        card.Abbr = "6";
                    }
                    else if (type.item == BlackjackCardType.Seven.ToString())
                    {
                        card.Value = 7;
                        card.Abbr = "7";
                    }
                    else if (type.item == BlackjackCardType.Eight.ToString())
                    {
                        card.Value = 8;
                        card.Abbr = "8";
                    }
                    else if (type.item == BlackjackCardType.Nine.ToString())
                    {
                        card.Value = 9;
                        card.Abbr = "9";
                    }
                    else if (type.item == BlackjackCardType.Ten.ToString())
                    {
                        card.Value = 10;
                        card.Abbr = "10";
                    }
                    else if (type.item == BlackjackCardType.Jack.ToString())
                    {
                        card.Value = 10;
                        card.Abbr = "J";
                    }
                    else if (type.item == BlackjackCardType.Dame.ToString())
                    {
                        card.Value = 10;
                        card.Abbr = "D";
                    }
                    else if (type.item == BlackjackCardType.King.ToString())
                    {
                        card.Value = 10;
                        card.Abbr = "K";
                    }
                    else if (type.item == BlackjackCardType.Ace.ToString())
                    {
                        card.Value = 11;
                        card.Abbr = "A";
                    }

                    carddeck.Cards.Add(card);
                }
            }

            return carddeck;
        }
        public BlackjackCard GetCardFromStack()
        {
            var item = this.ContextCardDeck.Cards.FirstOrDefault();
            this.ContextCardDeck.Cards.Remove(item);

            return item;
        }
        public BlackjackCard GetPlaceholderCard()
        {
            BlackjackCard item = new BlackjackCard();
            item.Id = Guid.NewGuid().ToString();
            item.Type = BlackjackCardType.Placeholder;
            item.ImageUrl = "placeholder";
            item.ImageUrlExtension = ".svg";
            item.ImageFontColor = "black";

            return item;
        }

        public async Task UpdateUI()
        {
            await this.InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }

        public bool HasBlackjack(List<BlackjackCard> items)
        {
            return items.Any(item =>
                item.Type == BlackjackCardType.Ten ||
                item.Type == BlackjackCardType.Jack ||
                item.Type == BlackjackCardType.Dame ||
                item.Type == BlackjackCardType.King) && 
                items.Any(item => item.Type == BlackjackCardType.Ace);
        }
        public bool HasSplitOption(List<BlackjackCard> items)
        {
            return items.FirstOrDefault().Type.Equals(items.Skip(1).Take(1).FirstOrDefault().Type);
        }
        public bool HasLost(List<BlackjackCard> items)
        {
            return items.Sum(item => item.Value) > 21;
        }        
        public bool Has21(List<BlackjackCard> items)
        {
            return items.Sum(item => item.Value) == 21;
        }
        
        public bool HasDealerWon()
        {
            var dealersum = this.DealerCards.Sum(item => item.Value);
            var clientsum = this.ClientCards.Sum(item => item.Value);

            if (dealersum > 21)
                return false;

            return dealersum > clientsum;
        }
        public void CleanBlackjackTable()
        {
            this.ClientCards.Clear();
            this.DealerCards.Clear();
            this.ContextCardDeck.Cards.Clear();
        }

        public async Task Play_Clicked()
        {

            if (this.GameStatus != BlackjackGameStatus.NewGame)
            {
                return;
            }

            // Generate new card deck
            this.ContextCardDeck = this.GetNewCarddeck();
            this.ContextCardDeck.Cards.Shuffle();

            // Set game status
            this.GameStatus = BlackjackGameStatus.InitCardDeal;

            // Get first card for each player
            var firstClientCard = this.GetCardFromStack();
            var firstDealerCard = this.GetCardFromStack();

            this.ClientCards.Add(firstClientCard);
            this.DealerCards.Add(firstDealerCard);

            // Get second card for each player
            var secondClientCard = this.GetCardFromStack();
            var secondDealerCard = this.GetCardFromStack();

            this.ClientCards.Add(secondClientCard);
            this.DealerCards.Add(secondDealerCard);

            // Check for client blackjack
            if (this.HasBlackjack(this.ClientCards))
            {
                this.GameStatus = BlackjackGameStatus.ClientWon;

                await InvokeAsync(async () =>
                {
                    await Task.Delay(4200).ContinueWith(async (task) =>
                    {                    
                        this.CleanBlackjackTable();
                        this.GameStatus = BlackjackGameStatus.NewGame;
                        await this.UpdateUI();
                   });                    
                });
            }

            // Check for dealer blackjack
            if (this.HasBlackjack(this.DealerCards))
            {
                this.GameStatus = BlackjackGameStatus.DealerWon;

                await InvokeAsync(async () =>
                {
                    await Task.Delay(4200).ContinueWith(async (task) =>
                    {
                        this.CleanBlackjackTable();
                        this.GameStatus = BlackjackGameStatus.NewGame;
                        await this.UpdateUI();
                    });
                });
            }

            // Check for client split options
            if (this.HasSplitOption(this.ClientCards))
            {

            }

            // Continue with client gameplay
            await InvokeAsync(async () =>
            {
                await Task.Delay(1400).ContinueWith(async (task) =>
                {
                    this.GameStatus = BlackjackGameStatus.ClientGameplay;
                    await this.UpdateUI();
                });
            });
        }
        public async Task Next_Clicked()
        {

            if (this.GameStatus != BlackjackGameStatus.ClientGameplay)
            {
                return;
            }

            // Client gameplay
            var nextCard = this.GetCardFromStack();
            this.ClientCards.Add(nextCard);

            await this.ValidateClientGameplay();
        }
        private async Task ValidateClientGameplay()
        {
            if (this.Has21(this.ClientCards))
            {
                this.GameStatus = BlackjackGameStatus.DealerGameplay;
                await this.UpdateUI();

                await this.Stop_Clicked();
            }

            if (this.HasLost(this.ClientCards))
            {
                this.GameStatus = BlackjackGameStatus.DealerWon;
                await InvokeAsync(async () =>
                {
                    await Task.Delay(2400).ContinueWith(async (task) =>
                    {
                        this.CleanBlackjackTable();
                        this.GameStatus = BlackjackGameStatus.NewGame;
                        await this.UpdateUI();
                    });
                });
            }
        }

        public async Task Stop_Clicked()
        {

            // Dealer gameplay
            this.GameStatus = BlackjackGameStatus.DealerGameplay;
            await this.UpdateUI();

            while (this.GameStatus == BlackjackGameStatus.DealerGameplay)
            {
                await this.ValidateDealerGameplay();
                if (this.GameStatus != BlackjackGameStatus.DealerGameplay)
                    break;

                var nextCard = this.GetCardFromStack();
                this.DealerCards.Add(nextCard);
                await this.UpdateUI();

                await InvokeAsync(async () =>
                {
                    await Task.Delay(800);
                });
            }
        }
        private async Task ValidateDealerGameplay()
        {

            if (this.HasLost(this.DealerCards))
            {
                this.GameStatus = BlackjackGameStatus.ClientWon;
                await this.UpdateUI();

                await InvokeAsync(async () =>
                {
                    await Task.Delay(2400).ContinueWith(async (task) =>
                    {
                        this.CleanBlackjackTable();
                        this.GameStatus = BlackjackGameStatus.NewGame;
                        await this.UpdateUI();
                    });
                });
            }

            if (this.HasDealerWon())
            {
                this.GameStatus = BlackjackGameStatus.DealerWon;
                await this.UpdateUI();

                await InvokeAsync(async () =>
                {
                    await Task.Delay(2400).ContinueWith(async (task) =>
                    {
                        this.CleanBlackjackTable();
                        this.GameStatus = BlackjackGameStatus.NewGame;
                        await this.UpdateUI();
                    });
                });
            }
        }

    }
}
