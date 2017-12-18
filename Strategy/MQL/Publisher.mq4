//+------------------------------------------------------------------+
//|                                               Publisher.mq4 |
//+------------------------------------------------------------------+
#property copyright   "2017-2018,"
#property description "Publisher expert advisor"

input string PublisherName = "Tradevanguarda";
//input int MaxTradesCount = 2;
//input double Vol = 0.1;

//input string InitialFileLocation = "C:\\Users\\hurski\\AppData\\Roaming\\MetaQuotes\\Terminal\\Common\\Files\Tradevanguarda";
//input string PublisherFolder = "C:\\Projects\\PublisherSubscriber\\Tradevanguarda";

bool isPassProcessing = false;
bool isLogCreated = false;

const string ParamSeparator = ":";
const string TradeSeparator = "\r\n";
const int TimerPeriodSeconds = 10;

const string ActionNew = "NEW";
const string ActionClose = "CLOSE";
const string ActionUpdate = "UPD";

struct TradeStructure
{
   int ticket;
   string symbol;
   double volume; 
   int orderType;
   double price;
   double TP;
   double SL;
   string Action;
};

TradeStructure Trades[];


//+------------------------------------------------------------------+
//| OnTrade function                                                  |
//+------------------------------------------------------------------+
void OnTimer()
{
   CheckTrades();
}

void CheckTrades()
{
   TradeStructure tradesToSave[];

   TradeStructure currentTrades[];   
   GetCurrentTrades(currentTrades);    
   int total = ArraySize(currentTrades);
   
   for(int i=0;i<total;i++) 
   {
      //NEW
      if(!IsTradeExists(currentTrades[i].ticket, Trades))
      {
         AddTrade2Save(currentTrades, tradesToSave, i, ActionNew);
         continue;
      }
   
      //UPD
      if(IsTradeModified(currentTrades[i].ticket, Trades, currentTrades))
      {
         AddTrade2Save(currentTrades, tradesToSave, i, ActionUpdate);
         continue;
      }
   }
   
   //CLOSED
   total = ArraySize(Trades);
   for(int j=0;j<total;j++) 
   {
      if(!IsTradeExists(Trades[j].ticket, currentTrades))
      {
         AddTrade2Save(Trades, tradesToSave, j, ActionClose);
         continue;
      }
   }
   
   if(ArraySize(tradesToSave) > 0)
   {
      if(SaveModifiedTrades(tradesToSave))
      {
         //copy currentTrades to Trades
         CopyTradesArray(currentTrades, Trades);
         ArrayResize(currentTrades,0);
         ArrayResize(tradesToSave,0);
      }
   }
}

void AddTrade2Save(TradeStructure &trades[], TradeStructure &tradeToSave[], int tradeIndex, string Action)
{
   int total = ArraySize(tradeToSave);
   ArrayResize(tradeToSave,total+1);
   
   tradeToSave[total].ticket = trades[tradeIndex].ticket;
   tradeToSave[total].symbol = trades[tradeIndex].symbol;
   tradeToSave[total].volume = trades[tradeIndex].volume;
   tradeToSave[total].orderType = trades[tradeIndex].orderType;
   tradeToSave[total].price = trades[tradeIndex].price;
   tradeToSave[total].TP = trades[tradeIndex].TP;
   tradeToSave[total].SL = trades[tradeIndex].SL;
   tradeToSave[total].Action = Action;
}

void CopyTradesArray(TradeStructure &tradesSource[], TradeStructure &tradeDest[])
{
   ArrayResize(tradeDest,0);
   
   int total = ArraySize(tradesSource);
   ArrayResize(tradeDest,total);   
   for(int i=0;i<total;i++)
   {
      tradeDest[i].ticket = tradesSource[i].ticket;
      tradeDest[i].symbol = tradesSource[i].symbol;
      tradeDest[i].volume = tradesSource[i].volume;
      tradeDest[i].orderType = tradesSource[i].orderType;
      tradeDest[i].price = tradesSource[i].price;
      tradeDest[i].TP = tradesSource[i].TP;
      tradeDest[i].SL = tradesSource[i].SL;
      tradeDest[i].Action = tradesSource[i].Action;
   }
}

bool IsTradeExists(int ticket, TradeStructure &trades[])
{
   int total = ArraySize(trades);   
   for(int i=0;i<total;i++)
   {
      if(trades[i].ticket == ticket)
         return true;
   }
   return false;
}

bool IsTradeModified(int ticket, TradeStructure &tradesOriginal[], TradeStructure &tradesCurrent[])
{
   int originalIndex = GetTradeArrayIndex(ticket, tradesOriginal);
   if(originalIndex < 0)
      return false;
      
   int currentIndex = GetTradeArrayIndex(ticket, tradesCurrent);
   if(currentIndex < 0)
      return false;
      
   return (tradesOriginal[originalIndex].TP == tradesCurrent[currentIndex].TP
        && tradesOriginal[originalIndex].SL == tradesCurrent[currentIndex].SL);
}

int GetTradeArrayIndex(int ticket, TradeStructure &trades[])
{
   int total = ArraySize(trades);   
   for(int i=0;i<total;i++)
   {
      if(Trades[i].volume == ticket)
         return i;
   }

   return -1;
}

void OnInit()
{
//--- check for history and trading
   if(Bars<100)
      return;
    
   InitialLoading();
   EventSetTimer(TimerPeriodSeconds);   
}
  
void InitialLoading()
{
   if(ArraySize(Trades) > 0)
   {
      ArrayFree(Trades);
   }
   GetCurrentTrades(Trades);
}

bool SaveModifiedTrades(TradeStructure &trades[])
{
   string fileName = GetFileName();
   string fileData = GetTradesRows(trades);
   string targetFileName = PublisherName + "//" + fileName;
  
   ResetLastError();
   int file_handle=FileOpen(fileName, FILE_WRITE|FILE_TXT|FILE_COMMON);
   if(file_handle!=INVALID_HANDLE)
   {
      FileWriteString(file_handle, fileData);
      FileClose(file_handle);
      //FileMove(fileName, FILE_COMMON, targetFileName, FILE_REWRITE);
      //FileCopy(fileName, FILE_COMMON, targetFileName, FILE_REWRITE);
   }
   else
   {
      int err = GetLastError();
      return false;
   }
   return true;
}

/*
void SaveModifiedTrades()
{
   string fileName = GetFileName();
   TradeStructure currentTrades[];   
   GetCurrentTrades(currentTrades);
   
   string fileData = GetTradesRows(currentTrades);
   string targetFileName = PublisherName + "//" + fileName;
  
   ResetLastError();
   int file_handle=FileOpen(fileName, FILE_WRITE|FILE_TXT|FILE_COMMON);
   if(file_handle!=INVALID_HANDLE)
   {
      FileWriteString(file_handle, fileData);
      FileClose(file_handle);
      //FileMove(fileName, FILE_COMMON, targetFileName, FILE_REWRITE);
      //FileCopy(fileName, FILE_COMMON, targetFileName, FILE_REWRITE);
   }
   else
   {
      int err = GetLastError(); 
   }
}*/

string AddParam(string tradeData, string param)
{
   return tradeData + ParamSeparator + param;
}

string AddTrade(string tradesData, string tradeData)
{
   if(tradesData == "")
      return tradeData;
   return tradesData + TradeSeparator + tradeData;
}

void GetCurrentTrades(TradeStructure &trades[])
{
   int total=OrdersTotal();
   ArrayResize(trades,total);
   
   for(int pos=0;pos<total;pos++) 
   {
     if(OrderSelect(pos,SELECT_BY_POS,MODE_TRADES)==false)
        continue;
     
     trades[pos].ticket = OrderTicket();
     trades[pos].symbol = OrderSymbol();
     trades[pos].volume = OrderLots();
     trades[pos].orderType = OrderType();
     trades[pos].price = OrderOpenPrice();
     trades[pos].TP = OrderTakeProfit();
     trades[pos].SL = OrderStopLoss();
     trades[pos].Action = "";
   }
}

string GetTradesRows(TradeStructure &trades[])
{
   int total = ArraySize(trades);
   string tradesData = "";
   
   for(int i=0;i<total;i++) 
   {      
      /*
     ticket No - 0
     symbol    - 1
     volume    - 2
     orderType - 3
     price     - 4
     TP        - 5
     SL        - 6
     Action    - 7
     */
     string tradeData = trades[i].ticket;    //0     
     tradeData = AddParam(tradeData, trades[i].symbol);  //1
     tradeData = AddParam(tradeData, trades[i].volume);  //2
     tradeData = AddParam(tradeData, trades[i].orderType);//3
     tradeData = AddParam(tradeData, trades[i].price);   //4
     tradeData = AddParam(tradeData, trades[i].TP);      //5
     tradeData = AddParam(tradeData, trades[i].SL);      //6
     tradeData = AddParam(tradeData, trades[i].Action);  //7
     
     tradesData = AddTrade(tradesData, tradeData);
   }
   
   //return "Test"+"\r\n";
   return tradesData;
}

string GetFileName()
{
   //return "test.txt";
   return PublisherName + "_" + Year() + "." + Month() + "." + Day() + " " + Hour() + "." + Minute() + "." + Seconds() + ".txt";
   //TimeLocal
}
//+------------------------------------------------------------------+
