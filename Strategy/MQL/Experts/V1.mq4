//+------------------------------------------------------------------+
//|                                               V1.mq4 |
//|                   Copyright 2005-2014, MetaQuotes Software Corp. |
//|                                              http://www.mql4.com |
//| GBPCHF - H1
//+------------------------------------------------------------------+
#property copyright   ""
#property link        ""
#property description "V1 expert advisor"

#include "OnNewBar.mqh"

const double stopLossDelta = 0.004;
const int slippage = 3;
const double maxImaDelta = 0.001;
const int point = 10000;

input const double profitDelta = 0.0015;
input const int BoxSize = 130;

const double initialBalance = 300;
const double initialVolume = 0.22;

int LastOrderType = -1;
//+------------------------------------------------------------------+
//| OnTick function                      130/0.22/0.0015                            |
//+------------------------------------------------------------------+
void OnNewBar()
 {
   if(Bars<100 /*|| Hour() < 2*/)
      return;
      
/*
   if(HasOpenPositions())
   {
      //RemovePendingTrades();
      //CloseOldPositions();
      return;
   }*/
   
   if(HasPendingPositions())
      return;
      
   //if(pendingPositionsCount == 1)
   //   RemovePendingTrades();
    
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

bool HasPendingPositions()
{
   int total=OrdersTotal();
   int orderType;
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     orderType = OrderType();     
     if(orderType == OP_BUYLIMIT || orderType == OP_BUYSTOP || orderType == OP_SELLLIMIT || orderType == OP_SELLSTOP)
     {
        return true;
     }
   }
   return false;;
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
         
       datetime hours = (TimeLocal()-OrderOpenTime())/60/60;//Количество часов, прошедшее с момента открытия ордера       
       if(hours > 10)
       {
         OrderClose(OrderTicket(),OrderLots(),closePrice,10,Red);
          --pos;
       }
     }
   }
}

int GetPendingPositionsCount()
{
   int positionsCount = 0;
   int total=OrdersTotal();
   int orderType;
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     orderType = OrderType();     
     if(orderType == OP_BUYLIMIT || orderType == OP_BUYSTOP || orderType == OP_SELLLIMIT || orderType == OP_SELLSTOP)
     {
        ++positionsCount;
     }
   }
   return positionsCount;
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

void AddPositionsByType(int currentOrderType, double nextLevel)
{
   bool isRecommendedSellPosition = true;
	if(currentOrderType == OP_SELLSTOP || currentOrderType == OP_SELLLIMIT)
	   isRecommendedSellPosition = false;
	   
	 if(LastOrderType != currentOrderType)
	 {
		CloseOpenPositions(isRecommendedSellPosition);
		LastOrderType = currentOrderType;
	 }
	      
	switch(currentOrderType)
	{	
		case OP_SELLSTOP: 	AddPendingSellStop(nextLevel);
		              /* AddRecommendedPosition(isRecommendedSellPosition)
		               if(!HasOpenPositions())
							   AddBuyPosition();*/
							break;
		case OP_BUYLIMIT: 	AddPendingBuyLimit(nextLevel);
		               /*if(!HasOpenPositions())
							   AddSellPosition();//??? 
							   */
							break;
		case OP_BUYSTOP: 	AddPendingBuyStop(nextLevel);
		               /*if(!HasOpenPositions())
							   AddSellPosition();*/
							break;
		case OP_SELLLIMIT: 	AddPendingSellLimit(nextLevel);
		               /*if(!HasOpenPositions())
							   AddBuyPosition();//???
							   */
							break;	
	}
	AddRecommendedPosition(isRecommendedSellPosition);
}

void AddRecommendedPosition(bool isRecommendedSellPosition)
{
   if(!HasOpenPositions())
	{
	   if(isRecommendedSellPosition)
	      AddSellPosition();
	   else
	      AddBuyPosition();
	}
}

void AddPositions()
{
   double currentLevel = Close[1];//Ask;
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
   double cci = iCCI(NULL, 0, period, PRICE_TYPICAL, 0);
   
   if(ma > nextDwnLevel)
   {
      if(cci > 0)
		 AddPositionsByType(OP_SELLSTOP, nextDwnLevel);	  
   }
   else if(cci < 0)
	  AddPositionsByType(OP_BUYLIMIT, nextDwnLevel);
      
   if(ma < nextUpLevel)
   {
      if(cci < 0)
		AddPositionsByType(OP_BUYSTOP, nextUpLevel);
   }
   else if(cci > 0)
	  AddPositionsByType(OP_SELLLIMIT, nextUpLevel);
}

void ProcessLevels(double nextDwnLevel, double currentLevel, double nextUpLevel)
{
   int period = 37;
   double ma = iMA(NULL,0,period,0,MODE_SMA,PRICE_CLOSE,0);
   double currentPrice = (Ask + Bid)/2;
   
   //current
   if(ProcessLevel(currentLevel, currentPrice, ma))
      return;
   
   //down
   if(ProcessLevel(nextDwnLevel, currentPrice, ma))
      return;
      
   //up
   if(ProcessLevel(nextUpLevel, currentPrice, ma))
      return;

}

bool ProcessLevel(double level, double currentPrice, double ma)
{
   // is price on the level
   if(currentPrice < level + slippage/point && currentPrice > level - slippage/point)
   {
      //if(ma > currentPrice)
      //   return AddSellPosition();
      return AddBuyPosition();
   }
}

bool AddSellPosition()
{
   double volume = GetVolume();
   double sellPrice = Ask;
   double sellProfitPrice = sellPrice - profitDelta;
   
   //sell
   return OrderSend( 
   Symbol(),        // символ 
   OP_SELL,        // торговая операция 
   volume,              // количество лотов 
   sellPrice,      // цена 
   slippage,            // проскальзывание 
   0,//sellLimitStopLossPrice,                   // stop loss 
   sellProfitPrice // take profit 
   ) != -1;
}

bool AddBuyPosition()
{
   double volume = GetVolume();
   double buyPrice = Bid;
   double buyProfitPrice = buyPrice + profitDelta;
      
   //buy
   return OrderSend( 
   Symbol(),        // символ 
   OP_BUY,        // торговая операция 
   volume,              // количество лотов 
   buyPrice,      // цена 
   slippage,            // проскальзывание 
   0,                   // stop loss 
   buyProfitPrice // take profit 
   ) != -1;

}

void CloseOpenPositions(bool leaveSellPositions)
{
	if(!HasOpenPositions())
		return;
	
   int total=OrdersTotal();
   int orderType;
   int orderTicket;
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
      continue;
      
     orderType = OrderType();     
     if(orderType != OP_BUY && orderType != OP_SELL)
     {
        continue;
     }
     
     if(leaveSellPositions && orderType == OP_SELL)
         continue;
      
     if(!leaveSellPositions && orderType == OP_BUY)
         continue;
     
	  double closePrice = Ask;
	  if(orderType == OP_SELL)
	     closePrice = Bid;
	  
	  orderTicket = OrderTicket();
	  if(OrderClose(orderTicket, OrderLots(), closePrice, slippage))
	  {
	 	--pos;
	  }
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
   0, //sellLimitStopLossPrice,                   // stop loss 
   sellLimitProfitPrice, // take profit 
   NULL,                //comment
   0,                   //magic
   GetExpiration()           //expiration
   );
}

void AddPendingSellStop(double price)
{
   double volume = GetVolume();
   double sellLimitPrice = price;// - priceDelta;
   double sellLimitProfitPrice = sellLimitPrice - profitDelta;
   double sellLimitStopLossPrice = sellLimitPrice + stopLossDelta;
  
   //sell limit
   OrderSend( 
   Symbol(),        // символ 
   OP_SELLSTOP,        // торговая операция 
   volume,              // количество лотов 
   sellLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0, //sellLimitStopLossPrice,                   // stop loss 
   sellLimitProfitPrice, // take profit 
   NULL,                //comment
   0,                   //magic
   GetExpiration()           //expiration
   );
}

void AddPendingBuyStop(double price)
{
   double volume = GetVolume();
   double buyLimitPrice = price;// + priceDelta;
   double buyLimitProfitPrice = buyLimitPrice + profitDelta;
   double buyLimitStopLossPrice = buyLimitPrice - stopLossDelta;
      
    //buy stop
   OrderSend( 
   Symbol(),        // символ 
   OP_BUYSTOP,        // торговая операция 
   volume,              // количество лотов 
   buyLimitPrice,      // цена 
   slippage,            // проскальзывание 
   0, //buyLimitStopLossPrice,                   // stop loss 
   buyLimitProfitPrice, // take profit 
   NULL,                //comment
   0,                   //magic
   GetExpiration()           //expiration
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
   0, buyLimitStopLossPrice,                   // stop loss 
   buyLimitProfitPrice, // take profit 
   NULL,                //comment
   0,                   //magic
   GetExpiration()           //expiration
   );
}

double GetVolume()
{
   double vol = (initialVolume*AccountInfoDouble(ACCOUNT_BALANCE)/initialBalance)/3;
   if(vol > 1 || vol < 0.01)
      vol = 0.01;
    
    return vol;
}

datetime GetExpiration()
{
   return /*TimeLocal*/ TimeCurrent() + 60*(ChartPeriod()-1);
}
  //+------------------------------------------------------------------+
