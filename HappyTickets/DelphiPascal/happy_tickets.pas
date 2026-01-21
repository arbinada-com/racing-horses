{
    Compile:
    - FreePascal
        fpc -O2 -Cr- happy_tickets.pas
    - Delphi 7
        dcc32 -$O+ -$R- -$Q- -$I- -$C- -$D- happy_tickets.pas
    - Delphi XE, 10 and more
        dcc32 -CC -VT -NSSystem -$O+ -$R- -$Q- -$I- -$C- -$D- happy_tickets.pas
        dcc64 -CC -VT -NSSystem -$O+ -$R- -$Q- -$I- -$C- -$D- happy_tickets.pas
}

program HappyTickets;

{$IFDEF FPC}
{$MODE DELPHI} // Required for FPC
{$ELSE}
{$APPTYPE CONSOLE} // required for Delphi
{$ENDIF}

uses
  SysUtils, DateUtils;

var
  n1, n2, n3, n4, n5, n6, n7, n8: {$IFDEF FPC}0..9{$ELSE}integer{$ENDIF};
  TicketsCount: longint; // Required for FPC that uses int16 instead of int32 for integer type in some cases (see notes)
  d1, d2: TDateTime;
begin
  TicketsCount := 0;
  d1 := Now;
  for n1 := 0 to 9 do
    for n2 := 0 to 9 do
      for n3 := 0 to 9 do
        for n4 := 0 to 9 do
          for n5 := 0 to 9 do
            for n6 := 0 to 9 do
              for n7 := 0 to 9 do
        for n8 := 0 to 9 do
                  if n1 + n2 + n3 + n4 = n5 + n6 + n7 + n8 then
                  {$IFDEF FPC}
            TicketsCount := TicketsCount + 1; // Inc may be slower in FPC
          {$ELSE}
            Inc(TicketsCount);
          {$ENDIF}
  d2 := Now;
  writeln('Found ', TicketsCount, ' tickets. Elapsed time, msec: ', DateUtils.MilliSecondsBetween(d1, d2));
end.