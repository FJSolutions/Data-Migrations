
CREATE TABLE public.email (
  id serial NOT NULL,
  address varchar(256) NOT NULL,
  label varchar(64)  NOT NULL,
  is_primary boolean NOT NULL DEFAULT true,
  CONSTRAINT pk_email PRIMARY KEY (id)
);