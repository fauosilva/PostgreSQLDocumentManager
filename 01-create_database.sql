CREATE DATABASE documentmanager;

CREATE TABLE "Users" (
    "Id" INT NOT NULL,
    "Username" TEXT NOT NULL,
    "Password" TEXT NOT NULL,
    "InsertedAt" TIMESTAMPTZ NOT NULL,
    "InsertedBy" TEXT NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "UpdatedBy" TEXT NULL     
);