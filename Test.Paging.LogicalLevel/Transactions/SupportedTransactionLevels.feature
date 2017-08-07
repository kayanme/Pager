
Feature: Common transaction support
	

Scenario: Transaction level support	for locking
    Given configured physical page with:
	| Locks | Version |
	| True  | False   |	
	Then the supported transaction levels are: 
		| IsolationLevel         |
		| ReadCommitted   |
		


Scenario: Transaction level support	for versioning
    Given configured physical page with:
	| Locks | Version |
	| True  | False   |
	Then the supported transaction levels are: 
		| IsolationLevel         |
		| ReadCommitted   |
		

Scenario: Transaction level support	for locking for headers
    Given configured physical headered page with:
	| Locks | Version |
	| True  | False   |	
	Then the supported transaction levels are: 
		| IsolationLevel         |
		| ReadCommitted   |
		

Scenario: Transaction level support	for versioning for headers
    Given configured physical headered page with:
	| Locks | Version |
	| False  | True   |	
	Then the supported transaction levels are: 
		| IsolationLevel         |
		| ReadCommitted   |
		