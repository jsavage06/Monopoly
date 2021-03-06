﻿using Monopoly.Board;
using Monopoly.Board.Locations;
using Monopoly.Cards;

namespace Monopoly.Handlers
{
    public class TurnHandler : ITurnHandler
    {
        private IJailer  jailer;
        private IBanker  banker;
        private IMovementHandler movementHandler;
        private IDice dice;
        private ICardHandler cardHandler;

        public TurnHandler(IJailer jailer, IBanker banker, IMovementHandler movementHandler, IDice dice, ICardHandler cardHandler)
        {
            this.jailer = jailer;
            this.banker = banker;
            this.movementHandler = movementHandler;
            this.dice = dice;
            this.cardHandler = cardHandler;
        }

        public void DoTurn(IPlayer player)
        {
            dice.Roll();
            DoTurn(player, dice.Score, dice.WasDoubles);
        }

        public virtual void DoTurn(IPlayer player, int distance, bool rolledDoubles)
        {
            if (jailer.PlayerIsImprisoned(player))
            {
                DoJailTurn(player, distance, rolledDoubles);
                return;
            }

            if (rolledDoubles)
            {
                player.DoublesCount++;
            }
            else
            {
                player.DoublesCount = 0;
            }

            if (player.DoublesCount == 3) 
            {
                SendPlayerToJail(player);
                return; 
            }

            DoStandardTurn(player, distance, rolledDoubles);
        }

        public virtual void DoJailTurn(IPlayer player, int distance, bool rolledDoubles)
        {

            switch (player.GetJailStrategy())
            {
                case JailStrategy.UseGetOutOfJailCard:
                    ReleasePlayerFromJailUsingCard(player);
                    break;
                case JailStrategy.Pay:
                    HandleGetOutOfJailByPaying(player);
                    break;

                default: // Handles "case JailStrategy.RollDoubles:"
                    HandleGetOutOfJailByRollingDoublesStrategy(player, distance, rolledDoubles);
                    break;
            }
        }

        public void DoStandardTurn(IPlayer player, int distance, bool RolledDoubles)
        {
            movementHandler.MovePlayer(player, distance);

            if (player.PlayerLocation.Group == PropertyGroup.Jail)
            {
                SendPlayerToJail(player);
            }

            player.CompleteLandOnLocationTasks();
            
            HandleDrawCardCase(player);
            
            movementHandler.HandlePurchasing(player);
        }

        private void HandleDrawCardCase(IPlayer player)
        {
            if (player.PlayerLocation.Group == PropertyGroup.Chance)
            {
                ICard card = cardHandler.DrawChanceCard();

                if (card.GetType() == typeof (GetOutOfJailCard))
                {
                    player.AddGetOutOfJailCard(card);
                }
                
                CompleteCardTasks(player, card);
                Discard(card);
            }

            else if (player.PlayerLocation.Group == PropertyGroup.Chest)
            {
                ICard card = DrawChestCard();

                if (card.GetType() == typeof(GetOutOfJailCard))
                {
                    player.AddGetOutOfJailCard(card);
                }
                card.Tasks.ForEach(x => x.Complete(player));
                Discard(card);
            }
        }

        public void ReleasePlayerFromJailUsingCard(IPlayer player)
        {
            cardHandler.Discard(player.SurrenderGetOutOfJailCard());
            jailer.ReleasePlayerFromJail(player);
            movementHandler.MovePlayerDirectlyToSpaceNumber(player, 10);
            DoTurn(player);
        }

        public void HandleGetOutOfJailByRollingDoublesStrategy(IPlayer player, int distance, bool rolledDoubles)
        {
            if (rolledDoubles) 
            {
                ReleasePlayerFromJail(player);
                DoTurn(player, distance, false);
            }
            else 
            {
                jailer.DecreaseSentence(player);

                if (jailer.GetRemainingSentence(player) == 0)
                {
                    HandleGetOutOfJailByPaying(player);
                    DoTurn(player, distance, rolledDoubles);
                }
            }
        }

        public void HandleGetOutOfJailByPaying(IPlayer player)
        {
            banker.ChargePlayerToGetOutOfJail(player);
            ReleasePlayerFromJail(player);
        }

        public void SendPlayerToJail(IPlayer player)
        {
            player.PlayerLocation = new JailLocation();
            jailer.Imprison(player);
            player.DoublesCount = 0;
        }

        public void ReleasePlayerFromJail(IPlayer player)
        {
            jailer.ReleasePlayerFromJail(player);
            player.PlayerLocation = new JailVisitingLocation();
        }

        public ICard DrawChestCard()
        {
            return cardHandler.DrawChestCard();
        }

        public void Discard(ICard card)
        {
            cardHandler.Discard(card);
        }

        public void CompleteCardTasks(IPlayer player, ICard card)
        {
            card.Tasks.ForEach(x => x.Complete(player));
        }
    }
}
