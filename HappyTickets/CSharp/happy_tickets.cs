/*
Compile:
- .NET
    csc happy_tickets.cs -optimize
- Mono (obsolete)
    mcs happy_tickets.cs -optimize
*/

using System;

namespace HappyTickets
{
  class Entry
  {
    public static void Main (string[] args)
    {
      byte n1, n2, n3, n4, n5, n6, n7, n8;
      int tickets_count = 0;
      DateTime d1 = DateTime.Now;
      for (n1 = 0; n1 < 10; n1++)
    for (n2 = 0; n2 < 10; n2++)
      for (n3 = 0; n3 < 10; n3++)
        for (n4 = 0; n4 < 10; n4++)
          for (n5 = 0; n5 < 10; n5++)
        for (n6 = 0; n6 < 10; n6++)
          for (n7 = 0; n7 < 10; n7++)
            for (n8 = 0; n8 < 10; n8++)
              if (n1 + n2 + n3 + n4 == n5 + n6 + n7 + n8)
            tickets_count++;
      DateTime d2 = DateTime.Now;
      int msec = (int)d2.Subtract(d1).TotalMilliseconds;
      Console.WriteLine("Found {0} tickets. Time elapsed: {1} msec", tickets_count, msec);
    }
  }
}