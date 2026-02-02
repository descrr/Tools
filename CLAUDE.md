# CLAUDE.md

## Repository Overview

This is a multi-project .NET Framework solution containing three independent tools focused on e-commerce pricing and forex trading automation. There is also a set of MetaTrader Expert Advisors (MQL4/MQL5) for trade execution.

## Project Structure

```
Tools/
├── PriceGenerator/          # WinForms desktop app - product price management
│   ├── PriceGenerator.sln
│   └── PriceGenerator/
│       ├── Libs/            # External DLLs (ExcelLibrary, Luxottica, OpenXml)
│       └── *.cs
├── FileReplicator/          # Console app - file system watcher & replicator
│   ├── FileReplicator.sln
│   └── FileReplicator/
├── Strategy/                # Console app - forex XO pattern analysis & backtesting
│   ├── Strategy.sln
│   ├── Strategy/            # C# analysis engine
│   ├── Data/                # Historical market data (CSV)
│   └── MQL/                 # MetaTrader Expert Advisors
│       ├── Experts/         # V1.mq4 trading EA, onnewbar.mqh
│       ├── Include/         # MQL shared headers
│       ├── Publisher.mq4    # Publishes trade positions to files
│       └── Subscriber.mq5   # Subscribes to and executes published trades
└── CLAUDE.md
```

## Languages & Frameworks

- **C# (.NET Framework 4.5 / 4.5.2)** - All three projects, built with MSBuild
- **MQL4 / MQL5** - MetaTrader Expert Advisors for forex trading automation
- **SQL Server** - Database backend for PriceGenerator (catalog: `Uno`)

## Projects in Detail

### PriceGenerator
Desktop WinForms application for collecting product data, analyzing prices, and generating Excel/YML reports. Integrates with a SQL Server database and sends email notifications.

Key classes:
- `ProductCollector` - web data collection
- `PriceAnalyzer` - pricing analysis
- `ExcelGenerator` / `ExcelWriter` / `ExcelHelper` - Excel report output
- `YMLGenerator` - YML feed generation
- `DataLayer` - static SQL data access
- `LinksGenerator` / `CaramellaLinksGenerator` - link generation utilities

External dependencies: `DocumentFormat.OpenXml`, `ExcelLibrary.dll`, `Luxottica.AdamExtensions.dll`, `Microsoft.Office.Interop.Excel`

### FileReplicator
Console utility that watches a directory for file changes and replicates modified files to a target location. Used to sync MetaTrader trade data files between directories.

### Strategy
Console application that backtests forex trading strategies using Point & Figure (XO) charting on historical data.

Key classes and data flow:
```
BarsLoader (CSV) → Bar2XoConverter → DirectionStrategySelector → DirectionStrategy
                                                                → BetStrategySelector → BaseBetStrategy
```

Betting strategy implementations (in `BetStrategy.cs`):
- **Martingale** - doubles bet after loss
- **Cumulative** - increases bet after win
- **Reset** - resets to 1 unit after loss
- **OneConstantly** - fixed 1-unit bets

Constants: `MinRank=18`, `MaxRank=18`, `MaxXoCount=120`

### MQL Expert Advisors
- `Publisher.mq4` - publishes active positions to files (NEW/UPDATE/CLOSE actions)
- `Subscriber.mq5` - reads published trades and executes them
- `V1.mq4` - trading EA based on XO patterns (box size: 130, profit delta: 0.0015)
- Communication between Publisher and Subscriber is file-based

## Build & Run

Each project has its own `.sln` file. Build with Visual Studio or MSBuild:

```
msbuild PriceGenerator/PriceGenerator.sln /p:Configuration=Release
msbuild Strategy/Strategy.sln /p:Configuration=Release
msbuild FileReplicator/FileReplicator.sln /p:Configuration=Release
```

Output: `bin\Debug\` or `bin\Release\` under each project directory.

MQL files are compiled within MetaTrader's MetaEditor (not via MSBuild).

## Testing

There are no automated tests (no test projects, no test frameworks). Validation is done manually.

## Code Conventions

- **Naming**: PascalCase for classes/methods (C# standard). Prefixes: `b` for booleans, `e` for enum types (e.g., `eBetStrategyTypes`).
- **Class suffixes**: Generator, Collector, Analyzer, Strategy, Loader, Converter for role clarity.
- **Patterns used**: Strategy pattern (betting strategies via `BaseBetStrategy`), Template pattern (binary string matching in `DirectionStrategy`).
- **Data access**: Static methods in `DataLayer.cs` with inline SQL.
- **Commit messages**: Sentence-style, past tense descriptions (e.g., "Volume calculation has been corrected").

## Key Notes for AI Assistants

- **No README exists** - this CLAUDE.md serves as the primary documentation.
- **Hardcoded paths** exist in FileReplicator and PriceGenerator (machine-specific). Be aware when modifying path logic.
- **Database coupling** - PriceGenerator selects connection strings based on machine name (`GVAPC`, `HOMEPC`, `EPBYMINW1589`).
- **External DLLs** in `PriceGenerator/Libs/` are committed binaries - do not remove.
- **MQL files** cannot be built or tested outside MetaTrader.
- **Data files** (`Strategy/Data/GBPCHF5.csv`) contain historical forex bars - large file, avoid reading entirely.
- **No package manager** (no NuGet restore needed) - dependencies are either GAC or local DLLs.
