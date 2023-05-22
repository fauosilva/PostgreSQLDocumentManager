DROP TABLE IF EXISTS document_permissions;
DROP TABLE IF EXISTS document;
DROP TABLE IF EXISTS user_groups;
DROP TABLE IF EXISTS groups;
DROP TABLE IF EXISTS users;

CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,    
    password TEXT NOT NULL,
	role TEXT NOT NULL,
    inserted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    inserted_by TEXT NOT NULL,
    updated_at TIMESTAMPTZ NULL,
    updated_by TEXT NULL,
	CHECK (role IN ('Admin', 'Manager', 'User'))
);

CREATE TABLE groups (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE NOT NULL,
    inserted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    inserted_by TEXT NOT NULL,
    updated_at TIMESTAMPTZ NULL,
    updated_by TEXT NULL
);

CREATE TABLE user_groups (    
    user_id INT REFERENCES Users(Id) NOT NULL,
    group_id INT REFERENCES Groups(Id) NOT NULL,
    inserted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    inserted_by TEXT NOT NULL,
    updated_at TIMESTAMPTZ NULL,
    updated_by TEXT NULL,
	UNIQUE (user_id, group_id)
);

CREATE TABLE document (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    inserted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    inserted_by TEXT NOT NULL,
    updated_at TIMESTAMPTZ NULL,
    updated_by TEXT NULL,
	UNIQUE (name,description,category)
);

CREATE TABLE document_permissions (
    id SERIAL PRIMARY KEY,
    document_id INT REFERENCES Document(Id) NOT NULL,
    user_id INT REFERENCES Users(Id) NULL,
    group_id INT REFERENCES Groups(Id) NULL,
	inserted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    inserted_by TEXT NOT NULL,
    updated_at TIMESTAMPTZ NULL,
    updated_by TEXT NULL,
	UNIQUE (document_id, user_id),
	UNIQUE (document_id, group_id)
);


-- Seed Data
 
 INSERT INTO users(username, password, role, inserted_by) VALUES ('adminuser', 'AJub08dIVXIQOmdvnY7jJpoz74/a9gNPbYwAXq7K9TYrWbT8GcU2CBZ2hL6Qu/s4iA==', 'Admin', 'seed');
 
 INSERT INTO groups(name, inserted_by) VALUES ('group1', 'seed');
 INSERT INTO groups(name, inserted_by) VALUES ('group2', 'seed');
 
 INSERT into user_groups(user_id, group_id, inserted_by) VALUES (1, 1, 'seed');
 
 
 

