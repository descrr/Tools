//+------------------------------------------------------------------+
//|                                                   Subscriber.mq5 |
//|                                                                  |
//|                                                                  |
//+------------------------------------------------------------------+
#property copyright ""
#property link      ""
#property version   "1.00"

#include <Trade\SymbolInfo.mqh>

#define EXPERT_MAGIC 112233   // MagicNumber of the expert

input string PublisherName = "Tradevanguarda";
input int MaxTradesCount = 3;
input double Vol = 0.05;

const string ParamSeparator = ":";
const string TradeSeparator = "\r\n";
const int TimerPeriodSeconds = 5;
const int slippage = 4;

const string ActionNew = "NEW";
const string ActionClose = "CLOSE";
const string ActionUpdate = "UPD";
const string DoneFolderName = "Done\\";

struct TradeStructure
{
   ulong ticket;
   string symbol;
   double volume; 
   int orderType;
   double price;
   double TP;
   double SL;
   string Action;
   string Comments;
   double PositionBid;
   double PositionAsk;
};

//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
//--- create timer
   EventSetTimer(TimerPeriodSeconds );
      
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
//--- destroy timer
   EventKillTimer();
      
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
   
  }
//+------------------------------------------------------------------+
//| Timer function                                                   |
//+------------------------------------------------------------------+
void OnTimer()
{
   CheckForUpdates();   
}
//+------------------------------------------------------------------+
void CheckForUpdates()
{
   string InpFilter=PublisherName + "\\*";
   string file_name;
   string int_dir="";
   string filePath;
   
   int    i=1,pos=0,last_pos=-1;
//--- search for the last backslash
   while(!IsStopped())
     {
      pos=StringFind(InpFilter,"\\",pos+1);
      if(pos>=0)
         last_pos=pos;
      else
         break;
     }
//--- the filter contains the folder name
   if(last_pos>=0)
      int_dir=StringSubstr(InpFilter,0,last_pos+1);
//--- get the search handle in the root of the local folder
   long search_handle=FileFindFirst(InpFilter/*"Tradevanguarda\\*"*/,file_name, FILE_COMMON);
//--- check if the FileFindFirst() is executed successfully
   if(search_handle!=INVALID_HANDLE)
     {
      //--- in a loop, check if the passed strings are the names of files or directories
      do
        {
         ResetLastError();
         //--- if it's a file, the function returns true, and if it's a directory, it returns error ERR_FILE_IS_DIRECTORY
         filePath = int_dir+file_name;
         if(FileIsExist(filePath, FILE_COMMON))
         {
            ProcessFile(int_dir, file_name);
            //PrintFormat("%d : %s name = %s",i,GetLastError()==ERR_FILE_IS_DIRECTORY ? "Directory" : "File",file_name);
         }
         
         i++;
        }
      while(FileFindNext(search_handle,file_name));
      //--- close the search handle
      FileFindClose(search_handle);
     }
}

void GetCurrentTrades(TradeStructure &trades[], int tradeTicket = 0)
{
   int total=PositionsTotal();
   if(tradeTicket > 0)
      total = 1;
   ArrayResize(trades,total);
   int tradeIndex;
   
   for(int pos=0;pos<total;pos++) 
   {
     tradeIndex = pos;
     if(tradeTicket > 0)
        tradeIndex = 0;
     
     trades[tradeIndex].ticket = PositionGetTicket(pos);
     if(trades[tradeIndex].ticket<=0)
        continue;
     
     trades[tradeIndex].symbol = PositionGetString(POSITION_SYMBOL);
     trades[tradeIndex].volume = PositionGetDouble(POSITION_VOLUME);
     trades[tradeIndex].orderType = PositionGetInteger(POSITION_TYPE);
     trades[tradeIndex].price = PositionGetDouble(POSITION_PRICE_OPEN);
     trades[tradeIndex].TP = PositionGetDouble(POSITION_TP);
     trades[tradeIndex].SL = PositionGetDouble(POSITION_SL);
     trades[tradeIndex].Action = "";
     trades[tradeIndex].Comments = PositionGetString(POSITION_COMMENT);
     
     trades[tradeIndex].PositionBid = SymbolInfoDouble(trades[tradeIndex].symbol,SYMBOL_BID);
     trades[tradeIndex].PositionAsk = SymbolInfoDouble(trades[tradeIndex].symbol,SYMBOL_ASK);
   }
}

void ProcessFile(string path, string fileName)
{
   //get c trades
   TradeStructure currentTrades[];  
   GetCurrentTrades(currentTrades);
   
   TradeStructure trades[];   
   LoadTrades(path+fileName, trades);
   
   int total = ArraySize(trades);
   for(int i=0;i<total;i++) 
   {
      if(trades[i].Action == ActionNew)
      {
         if(IsTradeExists(trades[i].Comments/*, currentTrades, ArraySize(trades)*/))
            continue;
      
         AddTrade(trades, i);
      }
      else if(trades[i].Action == ActionUpdate)
      {
         UpdateTrade(trades, i);
      }
      else if(trades[i].Action == ActionClose)
      {
         CloseTrade(trades, i);
      }
   }

   //Move to Done
   FileMove(path+fileName, FILE_COMMON, DoneFolderName+fileName, FILE_COMMON);
}

bool IsTradeExists(int ticket/*, TradeStructure &trades[]*/)
{
   TradeStructure currentTrades[];   
   GetCurrentTrades(currentTrades);
   
   int total = ArraySize(currentTrades);   
   for(int i=0;i<total;i++)
   {
      if(currentTrades[i].Comments == ticket)
         return true;
   }
   return false;
}

void AddTrade(TradeStructure &trades[], int tradeIndex)
{
   if(OrdersTotal() >= MaxTradesCount)
      return;
         
   /*CSymbolInfo *si=new CSymbolInfo;
   if(!si.Name(trades[tradeIndex].symbol)
   || !si.RefreshRates()
   || !si.Select())
      return;
      
   double price = trades[tradeIndex].price;
   if(trades[tradeIndex].orderType == ORDER_TYPE_BUY)
   {
      price = si.Bid();
   }
   else if(trades[tradeIndex].orderType == ORDER_TYPE_SELL)
   {
      price = si.Ask();
   }
   */
   
   double price = trades[tradeIndex].price;
   if(trades[tradeIndex].orderType == ORDER_TYPE_BUY)
   {
      price = SymbolInfoDouble(trades[tradeIndex].symbol,SYMBOL_BID);
   }
   else if(trades[tradeIndex].orderType == ORDER_TYPE_SELL)
   {
      price = SymbolInfoDouble(trades[tradeIndex].symbol,SYMBOL_ASK);
   }
   
   ENUM_TRADE_REQUEST_ACTIONS action = TRADE_ACTION_PENDING;
   if(trades[tradeIndex].orderType == ORDER_TYPE_BUY
   || trades[tradeIndex].orderType == ORDER_TYPE_SELL)
      action = TRADE_ACTION_DEAL;   
      
   MqlTradeRequest request={0}; 
   request.action=action;                       // setting a pending order 
   request.magic=EXPERT_MAGIC;                  // ORDER_MAGIC 
   request.symbol=trades[tradeIndex].symbol;    // symbol 
   request.volume=Vol;    // volume
   request.sl=trades[tradeIndex].SL;            // Stop Loss is not specified 
   request.tp=trades[tradeIndex].TP;            // Take Profit is not specified      
   request.type=trades[tradeIndex].orderType;   // order type 
   request.price=price;  // open 
   request.comment = trades[tradeIndex].Comments;
//--- send a trade request 
   MqlTradeResult result={0}; 

   if(!OrderSend(request,result))
   {
      return;
   }
}

void UpdateTrade(TradeStructure &trades[], int tradeIndex)
{
   if(!IsTradeExists(trades[tradeIndex].Comments))
      return;

   TradeStructure currentTrades[];
   GetCurrentTrades(currentTrades, trades[tradeIndex].Comments);
   if(ArraySize(currentTrades) != 1)
      return;
      
   MqlTradeRequest request={0};
   request.action=TRADE_ACTION_SLTP; // type of trade operation
   request.position = currentTrades[0].ticket;
   request.magic=EXPERT_MAGIC;                  // ORDER_MAGIC 
   request.symbol=currentTrades[0].symbol;    // symbol 
   request.sl=trades[tradeIndex].SL;            // Stop Loss is not specified 
   request.tp=trades[tradeIndex].TP;            // Take Profit is not specified      
//--- send a trade request 
   MqlTradeResult result={0}; 

   if(!OrderSend(request,result))
   {
      return;
   }
}

void CloseTrade(TradeStructure &trades[], int tradeIndex)
{
   if(!IsTradeExists(trades[tradeIndex].Comments))
      return;
  
   TradeStructure currentTrades[];
   GetCurrentTrades(currentTrades, trades[tradeIndex].Comments);
   if(ArraySize(currentTrades) != 1)
      return;
       
   MqlTradeRequest request={0};
   request.action=TRADE_ACTION_DEAL; // type of trade operation
   request.position = currentTrades[0].ticket;
   request.symbol =currentTrades[0].symbol;          // symbol 
   request.volume   =currentTrades[0].volume;                   // volume of the position
   request.deviation=slippage;                        // allowed deviation from the price
   request.magic=EXPERT_MAGIC;                  // ORDER_MAGIC 
   
   if(currentTrades[0].orderType==POSITION_TYPE_BUY)
   {
    request.price=SymbolInfoDouble(request.symbol,SYMBOL_BID);
    request.type =ORDER_TYPE_SELL;
   }
   else
   {
     request.price=SymbolInfoDouble(request.symbol,SYMBOL_ASK);
     request.type =ORDER_TYPE_BUY;
   }
         
//--- send a trade request 
   MqlTradeResult result={0};

   if(!OrderSend(request,result))
   {
      return;
   }
}
void LoadTrades(string filePath, TradeStructure &trades[])
{   
   ResetLastError(); 
   int file_handle=FileOpen(filePath,FILE_READ|FILE_TXT|FILE_ANSI|FILE_COMMON); 
   if(file_handle!=INVALID_HANDLE) 
     { 
      //--- additional variables 
      int    str_size; 
      string str; 
      //--- read data from the file 
      while(!FileIsEnding(file_handle)) 
        { 
         //--- find out how many symbols are used for writing the time 
         str_size=FileReadInteger(file_handle,INT_VALUE); 
         //--- read the string 
         str=FileReadString(file_handle,str_size);
         LoadTrade(str, trades);
        } 
      //--- close the file
      FileClose(file_handle);
     } 
}

void LoadTrade(string tradeRow, TradeStructure &trades[])
{
   tradeRow = tradeRow + ParamSeparator;
   int total = ArraySize(trades);
   ArrayResize(trades, total+1);
   
   int paramIndex = 0;
   string paramValue = "";  
   for(int i = 0; i < StringLen(tradeRow); i++)
   {
      string character = StringSubstr(tradeRow, i, 1);//tradeRow[i];
      if(character == ParamSeparator)
      {
         FillTradeParam(trades, paramIndex, paramValue);
         paramValue = "";
         ++paramIndex;         
      }
      else
      {
         paramValue = paramValue + character;
      }
      
      if(paramIndex >= 8)
         break;
   }
}

void FillTradeParam(TradeStructure &trades[], int paramIndex, string paramValue)
{
   int total = ArraySize(trades)-1;   
   switch(paramIndex)
   {
      case 0: trades[total].Comments = paramValue; break; //Ticket --> Comment
      case 1: trades[total].symbol = paramValue; break; //symbol
      case 2: trades[total].volume = paramValue; break; //volume
      case 3: trades[total].orderType = paramValue; break; //orderType
      case 4: trades[total].price = paramValue; break; //price
      case 5: trades[total].TP = paramValue; break; //TP
      case 6: trades[total].SL = paramValue; break; //SL
      case 7: trades[total].Action = paramValue; break; //Action      
   }
}