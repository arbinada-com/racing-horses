"""
Standard:
    python3 happy_tickets.py
Compiled to native executable code with Nuitka:
    python -m nuitka --standalone happy_tickets.py
"""

import time

def calculate():
    tickets_count = 0
    for n1 in range(0, 10):
        for n2 in range(0, 10):
            for n3 in range(0, 10):
                for n4 in range(0, 10):
                    for n5 in range(0, 10):
                        for n6 in range(0, 10):
                            for n7 in range(0, 10):
                                for n8 in range(0, 10):
                                    if n1 + n2 + n3 + n4 == n5 + n6 + n7 + n8:
                                        tickets_count += 1
    return tickets_count

start = time.perf_counter()
result = calculate()
end = time.perf_counter()
print(f"Found {result} tickets. Time elapsed, sec: {end - start}")
