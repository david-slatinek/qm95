CREATE TABLE IF NOT EXISTS customer (
    id_customer INTEGER PRIMARY KEY,
    name VARCHAR(30) NOT NULL,
    lastname VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(50) NOT NULL UNIQUE,
    salt VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS account_type (
    id_account_type INTEGER PRIMARY KEY,
    description VARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS account (
    id_account INTEGER PRIMARY KEY,
    opening_date DATE NOT NULL,
    balance DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    closing_date DATE,
    fk_account_type INTEGER NOT NULL,
    fk_customer INTEGER NOT NULL,
    FOREIGN KEY (fk_account_type) REFERENCES account_type(id_account_type) ON UPDATE CASCADE ON DELETE RESTRICT,
    FOREIGN KEY (fk_customer) REFERENCES customer(id_customer) ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS transfer_type (
    id_transfer_type INTEGER PRIMARY KEY,
    description VARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS transfer (
    amount DECIMAL(8, 2) NOT NULL,
    transfer_date DATE NOT NULL,
    fk_account INTEGER NOT NULL,
    fk_transfer_type INTEGER NOT NULL,
    FOREIGN KEY (fk_account) REFERENCES account(id_account) ON UPDATE CASCADE ON DELETE RESTRICT,
    FOREIGN KEY (fk_transfer_type) REFERENCES transfer_type(id_transfer_type) ON UPDATE CASCADE ON DELETE RESTRICT
);

INSERT INTO account_type (description) VALUES ('checking'), ('savings');

INSERT INTO transfer_type (description) VALUES ('deposit'), ('withdraw');
