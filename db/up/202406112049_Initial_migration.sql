CREATE TABLE "{{SchemaName}}"."Articles"
(
    "Id"             uuid                     NOT NULL,
    "Title"          text,
    "Guid"           text,
    "SubscriptionId" uuid                     NOT NULL,
    "Content"        text,
    "Summary"        text,
    "Source"         text,
    "IsRead"         boolean                  NOT NULL,
    "Timestamp"      TIMESTAMP WITH TIME ZONE NOT NULL,
    "Link"           text,
    "TenantId"       uuid                     NOT NULL
);


CREATE TABLE "{{SchemaName}}"."Files"
(
    "Id"        uuid    NOT NULL,
    "FileName"  text,
    "FileOwner" INTEGER NOT NULL,
    "Data"      bytea,
    "Hash"      bytea
);


CREATE TABLE "{{SchemaName}}"."HttpCacheEntries"
(
    "Id"               uuid                     NOT NULL,
    "Url"              text,
    "Content"          text,
    "ETag"             text,
    "LastModifiedDate" TIMESTAMP WITH TIME ZONE,
    "LastGet"          TIMESTAMP WITH TIME ZONE NOT NULL
);


CREATE TABLE "{{SchemaName}}"."SubscriptionLogEntries"
(
    "Id"             uuid                     NOT NULL,
    "SubscriptionId" uuid                     NOT NULL,
    "Success"        boolean                  NOT NULL,
    "Message"        text,
    "Timestamp"      TIMESTAMP WITH TIME ZONE NOT NULL,
    "TenantId"       uuid                     NOT NULL
);


CREATE TABLE "{{SchemaName}}"."Subscriptions"
(
    "Id"     uuid NOT NULL,
    "Url"    text,
    "Title"  text,
    "IconId" uuid
);


CREATE TABLE "{{SchemaName}}"."Users"
(
    "Id"       uuid NOT NULL,
    "Username" text NOT NULL
);


CREATE INDEX "IX_Articles_SubscriptionId" ON {{SchemaName}}."Articles" USING btree ("SubscriptionId");

CREATE INDEX "IX_Articles_TenantId" ON {{SchemaName}}."Articles" USING btree ("TenantId");

CREATE INDEX "IX_SubscriptionLogEntries_SubscriptionId" ON {{SchemaName}}."SubscriptionLogEntries" USING btree ("SubscriptionId");

CREATE INDEX "IX_SubscriptionLogEntries_TenantId" ON {{SchemaName}}."SubscriptionLogEntries" USING btree ("TenantId");

CREATE INDEX "IX_Subscriptions_IconId" ON {{SchemaName}}."Subscriptions" USING btree ("IconId");

CREATE UNIQUE INDEX "IX_Users_Username" ON {{SchemaName}}."Users" USING btree ("Username");

CREATE UNIQUE INDEX "PK_Articles" ON {{SchemaName}}."Articles" USING btree ("Id");

CREATE UNIQUE INDEX "PK_Files" ON {{SchemaName}}."Files" USING btree ("Id");

CREATE UNIQUE INDEX "PK_HttpCacheEntries" ON {{SchemaName}}."HttpCacheEntries" USING btree ("Id");

CREATE UNIQUE INDEX "PK_SubscriptionLogEntries" ON {{SchemaName}}."SubscriptionLogEntries" USING btree ("Id");

CREATE UNIQUE INDEX "PK_Subscriptions" ON {{SchemaName}}."Subscriptions" USING btree ("Id");

CREATE UNIQUE INDEX "PK_Users" ON {{SchemaName}}."Users" USING btree ("Id");

ALTER TABLE "{{SchemaName}}"."Articles"
    ADD CONSTRAINT "PK_Articles" PRIMARY KEY USING INDEX "PK_Articles";

ALTER TABLE "{{SchemaName}}"."Files"
    ADD CONSTRAINT "PK_Files" PRIMARY KEY USING INDEX "PK_Files";

ALTER TABLE "{{SchemaName}}"."HttpCacheEntries"
    ADD CONSTRAINT "PK_HttpCacheEntries" PRIMARY KEY USING INDEX "PK_HttpCacheEntries";

ALTER TABLE "{{SchemaName}}"."SubscriptionLogEntries"
    ADD CONSTRAINT "PK_SubscriptionLogEntries" PRIMARY KEY USING INDEX "PK_SubscriptionLogEntries";

ALTER TABLE "{{SchemaName}}"."Subscriptions"
    ADD CONSTRAINT "PK_Subscriptions" PRIMARY KEY USING INDEX "PK_Subscriptions";

ALTER TABLE "{{SchemaName}}"."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY USING INDEX "PK_Users";

ALTER TABLE "{{SchemaName}}"."Articles"
    ADD CONSTRAINT "FK_Articles_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id") ON DELETE CASCADE NOT valid;

ALTER TABLE "{{SchemaName}}"."Articles" validate CONSTRAINT "FK_Articles_Subscriptions_SubscriptionId";

ALTER TABLE "{{SchemaName}}"."Articles"
    ADD CONSTRAINT "FK_Articles_Users_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Users" ("Id") ON DELETE CASCADE NOT valid;

ALTER TABLE "{{SchemaName}}"."Articles" validate CONSTRAINT "FK_Articles_Users_TenantId";

ALTER TABLE "{{SchemaName}}"."SubscriptionLogEntries"
    ADD CONSTRAINT "FK_SubscriptionLogEntries_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id") ON DELETE CASCADE NOT valid;

ALTER TABLE "{{SchemaName}}"."SubscriptionLogEntries" validate CONSTRAINT "FK_SubscriptionLogEntries_Subscriptions_SubscriptionId";

ALTER TABLE "{{SchemaName}}"."SubscriptionLogEntries"
    ADD CONSTRAINT "FK_SubscriptionLogEntries_Users_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Users" ("Id") ON DELETE CASCADE NOT valid;

ALTER TABLE "{{SchemaName}}"."SubscriptionLogEntries" validate CONSTRAINT "FK_SubscriptionLogEntries_Users_TenantId";

ALTER TABLE "{{SchemaName}}"."Subscriptions"
    ADD CONSTRAINT "FK_Subscriptions_Files_IconId" FOREIGN KEY ("IconId") REFERENCES "Files" ("Id") NOT valid;

ALTER TABLE "{{SchemaName}}"."Subscriptions" validate CONSTRAINT "FK_Subscriptions_Files_IconId";
