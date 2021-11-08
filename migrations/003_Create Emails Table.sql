CREATE TABLE public.emails (
  id SERIAL NOT NULL,
  planning_center_id int NOT NULL,
  "address" varchar(256) NOT NULL,
  label varchar(256) NOT NULL,
  is_primary BOOL NOT NULL DEFAULT false,
  CONSTRAINT pk_emails PRIMARY KEY (id),
  CONSTRAINT uq_emails_address UNIQUE ("address")
);
