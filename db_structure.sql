BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Accounts" (
	"AccountNumber"	TEXT,
	"FirstName"	TEXT NOT NULL,
	"LastName"	TEXT NOT NULL,
	"Balance"	REAL NOT NULL,
	"UserID"	INTEGER NOT NULL,
	"Username"	TEXT,
	"Password"	TEXT,
	PRIMARY KEY("AccountNumber"),
	FOREIGN KEY("UserID") REFERENCES "Users"("UserID") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Cards" (
	"AccountNumber"	TEXT,
	"CardType"	TEXT,
	"IsActive"	INTEGER,
	"CardTerms"	TEXT,
	PRIMARY KEY("AccountNumber")
);
CREATE TABLE IF NOT EXISTS "Loans" (
	"LoanID"	INTEGER,
	"AccountNumber"	TEXT NOT NULL,
	"LoanType"	TEXT NOT NULL CHECK("LoanType" IN ('Personal', 'Home', 'Car')),
	"Amount"	DECIMAL(10, 2) NOT NULL,
	"RemainingAmount"	DECIMAL(10, 2),
	"Status"	TEXT NOT NULL,
	PRIMARY KEY("LoanID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Transactions" (
	"TransactionID"	INTEGER,
	"AccountNumber"	TEXT NOT NULL,
	"Type"	TEXT NOT NULL CHECK("Type" IN ('Deposit', 'Withdraw', 'Transfer', 'LoanPayment')),
	"Amount"	REAL NOT NULL,
	"Date"	TEXT NOT NULL,
	"LoanID"	INTEGER,
	PRIMARY KEY("TransactionID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Users" (
	"UserID"	INTEGER,
	"Username"	TEXT NOT NULL UNIQUE,
	"PasswordHash"	TEXT NOT NULL,
	"Role"	TEXT NOT NULL CHECK("Role" IN ('Admin', 'Klient')),
	"IsAdmin"	INTEGER DEFAULT 0,
	"AccountNumber"	TEXT,
	"CanLogin"	INTEGER DEFAULT 1,
	PRIMARY KEY("UserID" AUTOINCREMENT)
);
COMMIT;
