/*
Compiling examples:
- GCC
    gcc happy_tickets.c -O2 -o happy_tickets
- MSVC (run from VS prompt or set paths)
    set PATH=C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\bin;C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE;%PATH%set LIB=C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\lib;C:\Program Files\Microsoft SDKs\Windows\v6.0A\Libset INCLUDE=C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\include
    cl.exe /O2 happy_tickets.c
- Borland/Embarcadero
    set PATH=C:\Borland\BCC55\Bin;%PATH%
    bcc32.exe -O2 -I"C:\Borland\BCC55\Include" -L"C:\Borland\BCC55\Lib" happy_tickets.c
*/

#include <stdio.h>
#include <time.h>

int main()
{
  unsigned char n1, n2, n3, n4, n5, n6, n7, n8;
  long int tickets_count = 0;
  clock_t t1, t2;  // forward declaration to prevent MS VC 14.0 error
  double msec;
  t1 = clock();
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
  t2 = clock();
  msec = (double)(t2 - t1) / ((double)CLOCKS_PER_SEC / 1000);
  printf("Found %ld tickets. Time elapsed: %.0f msec\n", tickets_count, msec);
  return 0;
}