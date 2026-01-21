/*
Compile:
    rustc happy_tickets.rs -C opt_level=3 -o happy_tickets
*/

use std::time::Instant;

fn main() {
  let mut tickets_count: u32 = 0;
  let start = Instant::now();
  for n1 in 0..10 {
    for n2 in 0..10 {
      for n3 in 0..10 {
        for n4 in 0..10 {
          for n5 in 0..10 {
            for n6 in 0..10 {
              for n7 in 0..10 {
                for n8 in 0..10 {
                  if n1 + n2 + n3 + n4 == n5 + n6 + n7 + n8 {
                    tickets_count = tickets_count + 1;
                  }
                }
              }
            }
          }
        }
      }
    }
  }
  let elapsed = start.elapsed();
  println!("Found {} tickets. Time elapsed: {} ms", tickets_count, elapsed.as_millis());
}