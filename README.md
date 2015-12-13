# CodingStandardCodeAnalyzers

The project currently contains following C# Roslyn code analyzers:

1) DateTimeClassUsageCodeAnalyzer
  Wanr on usage of DateTime.Now and DateTime.Today properties. Corresponding code fix suggest 2 options: use DateTimeOffset class or use NodeTime equivalents.

2) TimeMeasurementCodeAnalyzer
  Suggest to measure the elapsed time of some operation using the Stopwatch class. Corresponding code fix inserts necessary calls to create instance of Stopwath, start the timer, stop it and get elapsed time.
  
3) IfStatementCodeAnalyzer
  Suggest to always use braces if IF-THEN and IF-ELSE statements. Corresponding code fix inserts necessary braces.
  
4) FieldAccessCodeAnalyzer
  Check that all fields of a class are private, the only exception is static readonly fields. Corresponding code fix changes access modifier to private.

Nuget feed with the analyzers: https://www.myget.org/F/codingstandardcodeanalyzers/api/v3/index.json
