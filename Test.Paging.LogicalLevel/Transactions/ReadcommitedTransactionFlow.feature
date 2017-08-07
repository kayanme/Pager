Feature: ReadCommited TransactionFlow

Background:
   Given configured physical headered page with:
    | Locks | Version |
	| True  | False   |	   
   And configured transactable page with read lock naming '1' and write lock naming '2'
   And transaction with isolation level 'ReadCommitted'
   

Scenario: Get header when nothing modified
    Given we have a headered page
	When we get a header
	Then we expect acquiring read lock on page
	Then we expect read a header from physical page
	Then we expect release read lock on page
	And finally return a result from physical page


Scenario: Modify header and commit transaction
    Given we have a headered page
	When we modify a header
	Then we expect acquiring write lock on page
	Then we commit the transaction
	And expect header modified to physical page
	Then we expect release write lock on page
	And so be it

Scenario: Add record and commit transaction
    Given we have a page
	When we add a record
	Then we expect acquiring write lock on page
	Then we check if we can add new record on physical page
	Then we commit the transaction
	And expect record adding 
  
  