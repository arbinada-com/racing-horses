/*
Compile:
	go build happytickets.go
*/

package main

import (
  "fmt"
  "time"
)

func main() {
  var tickets_count int = 0
  var t1 = time.Now()
  for n1 := 0; n1 < 10; n1++ {
    for n2 := 0; n2 < 10; n2++ {
      for n3 := 0; n3 < 10; n3++ {
        for n4 := 0; n4 < 10; n4++ {
          for n5 := 0; n5 < 10; n5++ {
            for n6 := 0; n6 < 10; n6++ {
              for n7 := 0; n7 < 10; n7++ {
                for n8 := 0; n8 < 10; n8++ {
                  if n1 + n2 + n3 + n4 == n5 + n6 + n7 + n8 {
                    tickets_count++
                  }
                }
              }
            }
          }
        }
      }
    }
  }
  var t2 = time.Now()
  var diff = t2.Sub(t1)
  fmt.Printf("Found %d tickets. Time elapsed: %d msec\n",
    tickets_count, diff.Nanoseconds() / int64(time.Millisecond))
}
