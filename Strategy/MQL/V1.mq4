//+------------------------------------------------------------------+
//|                                               V1.mq4 |
//|                   Copyright 2005-2014, MetaQuotes Software Corp. |
//|                                              http://www.mql4.com |
//+------------------------------------------------------------------+
#property copyright   ""
#property link        ""
#property description "V1 expert advisor"

const double profitDelta = 0.001;
const double stopLossDelta = 0.01;
const int slippage = 3;
const double initialBalance = 300;
const double maxImaDelta = 0.001;
const int point = 10000;

const int BoxSize = 112;
const double initialVolume = 0.22;
//+------------------------------------------------------------------+
//| OnTick function                      112/0.22                            |
//+------------------------------------------------------------------+
void OnTick()
 {
   if(Bars<100 || Hour() < 2)
      return;
      
   if(HasOpenPositions())
   {
      RemovePendingTrades();
      //CloseOldPositions();
      return;
   }
   
   bool hasPendingBuy = HasPendingBuy();
   bool hasPendingSell = HasPendingSell();   
   if(hasPendingBuy || hasPendingSell)
   {
      //if(hasPendingBuy && hasPendingSell)
         return;
      //RemovePendingTrades();
   }
   
   AddPositions();
 }

//+------------------------------------------------------------------+
//| Has open positions                                         |
//+------------------------------------------------------------------+
bool HasOpenPositions()
{
   int total=OrdersTotal();
   int order_type; 
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     order_type = OrderType();
     
     if(order_type == OP_BUY || order_type == OP_SELL)
      return true;
   }
   return false;
}

void CloseOldPositions()
{
   int total=OrdersTotal();
   int order_type; 
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     order_type = OrderType();
     
     if(order_type == OP_BUY || order_type == OP_SELL)
     {
       double closePrice = Ask;
       if(order_type == OP_BUY)
         closePrice = Bid;
         
       datetime days = (TimeCurrent()-OrderOpenTime())/60/60/24;//Количество дней, прошедшее с момента открытия ордера       
       if(days > 1)
       {
         OrderClose(OrderTicket(),OrderLots(),closePrice,10,Red);
          --pos;
       }
     }
   }
}

//+------------------------------------------------------------------+
//| Has Buy Limit                                        |
//+------------------------------------------------------------------+
bool HasPendingBuy()
{
   return HasPendingPosition(OP_BUYLIMIT) || HasPendingPosition(OP_BUYSTOP);
}

bool HasPendingSell()
{
   return HasPendingPosition(OP_SELLLIMIT) || HasPendingPosition(OP_SELLSTOP);
}

bool HasPendingPosition(int cmd)
{

   int total=OrdersTotal();
   int orderType;
   int orderTicket;
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     orderType = OrderType();
     
     if(orderType == cmd)
     {
         return true;
     }
   }
   return false;
}

//+------------------------------------------------------------------+
//| Remove Pending Trades                                        |
//+------------------------------------------------------------------+
void RemovePendingTrades()
{
   int total=OrdersTotal();
   int orderType;
   int orderTicket;
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     orderType = OrderType();
     
     if(orderType == OP_BUYLIMIT || orderType == OP_BUYSTOP || orderType == OP_SELLLIMIT || orderType == OP_SELLSTOP)
     {
         orderTicket = OrderTicket();
         if(OrderDelete(orderTicket))
         {
            --pos;
         }
     }
   }
}

void AddPositions()
{
   double currentLevel = Ask;
   int intCurrentLevel = currentLevel * point;
	int rest = intCurrentLevel % BoxSize;
	
	double mult = (intCurrentLevel - rest) / BoxSize;
	double nextDwnLevel = mult * BoxSize / point;
	double nextUpLevel = nextDwnLevel + (double)BoxSize / point;
	
	for (int i = 0; i < Bars; i++)
	{
		//returned from up level
		if(High[i] > (double)((double)nextUpLevel) / (double)point)
		{
		   currentLevel = nextUpLevel;
			nextUpLevel = nextUpLevel + (double)BoxSize / point;
			nextDwnLevel = currentLevel - (double)BoxSize / point;
			break;
		}
		//returned from down level
		else if (Low[i] < (double)((double)nextDwnLevel)/(double)point)
		{
		   currentLevel = nextDwnLevel;
			nextDwnLevel = nextDwnLevel - (double)BoxSize / point;
			nextUpLevel = currentLevel + (double)BoxSize / point;
			break;
		}
	}
	
   int period = 37;
   double ma = iMA(NULL,0,period,0,MODE_SMA,PRICE_CLOSE,0);
   
   if(ma < nextUpLevel)
   {
      AddPendingSellLimit(nextUpLevel);
      AddPendingSellStop(nextDwnLevel);
   }
   else if(ma > nextDwnLevel)
   {
      AddPendingBuyLimit(nextDwnLevel);
      AddPendingBuyStop(nextUpLevel);
   }
   else
   {
      AddPendingSellLimit(nextUpLevel);
      AddPendingBuyLimit(nextDwnLevel);
   }

}


//+------------------------------------------------------------------+
//| Set up new positions                                         |
//+------------------------------------------------------------------+
void AddPendingSellLimit(double price)
{
   double volume = GetVolume();
   double sellLimitPrice = price;// + priceDelta;
   double sellLimitProfitPrice = sellLimitPrice - profitDelta;
   double sellLimitStopLossPrice = sellLimitPrice + stopLossDelta;
   
   //sell limit
   OrderSend( 
   Symbol(),        // символ 
   OP_SELLLIMIT,        // торговая операция 
   volume,              // количество лотов 
   sellLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0,//sellLimitStopLossPrice,                   // stop loss 
   sellLimitProfitPrice // take profit 
   );
}

void AddPendingSellStop(double price)
{
   double volume = GetVolume();
   double sellLimitPrice = price;// - priceDelta;
   double sellLimitProfitPrice = sellLimitPrice - profitDelta;
  
   //sell limit
   OrderSend( 
   Symbol(),        // символ 
   OP_SELLSTOP,        // торговая операция 
   volume,              // количество лотов 
   sellLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0,                   // stop loss 
   sellLimitProfitPrice // take profit 
   );
}

void AddPendingBuyStop(double price)
{
   double volume = GetVolume();
   double buyLimitPrice = price;// + priceDelta;
   double buyLimitProfitPrice = buyLimitPrice + profitDelta;
      
    //buy stop
   OrderSend( 
   Symbol(),        // символ 
   OP_BUYSTOP,        // торговая операция 
   volume,              // количество лотов 
   buyLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0,                   // stop loss 
   buyLimitProfitPrice // take profit 
   );
}

void AddPendingBuyLimit(double price)
{
   double volume = GetVolume();
   double buyLimitPrice = price;// - priceDelta;
   double buyLimitProfitPrice = buyLimitPrice + profitDelta;
   double buyLimitStopLossPrice = buyLimitPrice - stopLossDelta;
   
    //buy limit
   OrderSend( 
   Symbol(),        // символ 
   OP_BUYLIMIT,        // торговая операция 
   volume,              // количество лотов 
   buyLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0,//buyLimitStopLossPrice,                   // stop loss 
   buyLimitProfitPrice // take profit 
   );
}

double GetVolume()
{
   return initialVolume*AccountInfoDouble(ACCOUNT_BALANCE)/initialBalance;
   //return initialVolume;
}
  //+------------------------------------------------------------------+
