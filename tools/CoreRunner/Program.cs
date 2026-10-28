using System;
class P{static int Main(){try{var r=MySimulator.Verification.CoreChecks.Run();Console.WriteLine(r);return 0;}catch(Exception e){Console.WriteLine("FAIL "+e);return 1;}}}
