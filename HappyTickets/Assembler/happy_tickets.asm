; Asm : nasm -f elf64 -l happytickets.lst  happytickets.asm
; Link: ld -s -o happytickets happytickets.o

section .data
  ticket_count      dq 0
  start_time        dq 0
  stop_time         dq 0
  clock_per_msec    dq 0
  msec              dq 0
  msec_in_sec       dq 1000
  msg_result        db '. Elapsed time, msec: '
  msglen_result     equ $-msg_result
  buf               db '123546890'
  buflen            equ $-buf
  LF                db 0xA
  LFlen             equ $-LF
  timeval:
    tv_sec  dd 0
    tv_usec dd 0

section .text
  global _start

_start:
  call get_clock_per_millisecond
  call get_start_time

  ; begin calculating
  mov rbx, 0 ; store count in register
  mov r8, 10
  loop_1:
    mov r9, 10
    loop_2:
      mov r10, 10
      loop_3:
        mov r11, 10
        loop_4:
          mov r12, 10
          loop_5:
            mov r13, 10
            loop_6:
              mov r14, 10
              loop_7:
                mov r15, 10
                loop_8:
                  mov rax, r8
                  add rax, r9
                  add rax, r10
                  add rax, r11
                  sub rax, r12
                  sub rax, r13
                  sub rax, r14
                  sub rax, r15
                  jnz not_happy
                    inc rbx
                  not_happy:
                    dec r15
                jnz loop_8
                dec r14
              jnz loop_7
              dec r13
            jnz loop_6
            dec r12
          jnz loop_5
          dec r11
        jnz loop_4
        dec r10
      jnz loop_3
      dec r9
    jnz loop_2
    dec r8
  jnz loop_1

  mov [ticket_count], rbx
  ; end of calculations

  call get_stop_time

  ; elapsed time
  mov rax, [stop_time]
  sub rax, [start_time]
  mov rdx, 0                 ; load upper half of dividend with zero
  div qword [clock_per_msec] ; divide rdx:rax by value
  mov [msec], rax

  ; print results
  mov rax, [ticket_count]
  call print_number
  mov rcx, msg_result
  mov rdx, msglen_result
  call print_str
  mov rax, [msec]
  call print_number
  call print_newline

  ; exiting program
  mov rax, 1  ; the system call for exit (sys_exit)
  mov rbx, 0  ; exit with return code of 0 (no error)
  int 0x80    ; system call
  ret

;------------------------
; subroutines

get_timestamp:
  xor rax, rax
  cpuid
  rdtsc
  ret

get_start_time:
  call get_timestamp
  mov [start_time], eax
  mov [start_time + 4], edx
  ret

get_stop_time:
  call get_timestamp
  mov [stop_time], eax
  mov [stop_time + 4], edx
  ret

get_clock_per_millisecond:
  call get_start_time
  ; sleep for 1 seconds and 0 nanoseconds
  mov dword [tv_sec], 1
  mov dword [tv_usec], 0
  mov eax, 162
  mov ebx, timeval
  mov ecx, 0
  int 0x80
  ;
  call get_stop_time
  mov rax, [stop_time]
  sub rax, [start_time]
  mov rdx, 0
  div qword [msec_in_sec]
  mov [clock_per_msec], rax
  ret

print_number:
  mov rbx, buf + buflen - 1
  mov rcx, buflen
  mov rdi, 10
  next_digit:
    mov rdx, 0
    div rdi
    add rdx, 48 ; '0' ASCII code
    mov [rbx], dl
    dec rbx
  loop next_digit

  mov rcx, buf
  mov rdx, buflen
  call print_str
  ret

print_newline:
  mov rcx, LF
  mov rdx, LFlen
  call print_str
  ret

print_str:
  mov rax, 4
  mov rbx, 1
  int 0x80
  ret
