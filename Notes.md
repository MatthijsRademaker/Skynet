# possible scenarios with event sourcing

1. Knowledge test result can be injected in paycheck protector
   1. Which makes a follow up mail with prefilled data possible directly into the baz funnel
2. When the knowledge test fails in the pay check protecter you can link to the questions only and send a request afterwards to the contract mapper?
   1. improves CES
3. Premium is dynamic
4. Save for later

## Example flows

1. PremiumWidget
   1. PremiumWidget does a call me back request with premium 100
   2. Then a simulated button from mail -> call paycheck protector recreate customer with id (this calculates a random new premium)
      1. TODO add param for notification in BFF
2. KnowledgeTest
   1. Knowledge test is done and passes through seperate forms app
   2. Clicks link from mail -> recreate state in PP and sees passed/not passed
      1. if passed can continue immediately
      2. if failed, example/material for study idk
3. KnowledgeTest
   1. Knowledge test is done through PP and fails
   2. Clicks link to separate form to only redo the knowledge test
   3. Can continue to PP with prefilled data and passed KT
