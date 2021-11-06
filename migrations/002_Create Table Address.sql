
CREATE TABLE public.addresses (
  id serial NOT NULL,
  planning_center_id int NOT NULL,
  address varchar(256) NOT NULL,
  label varchar(64)  NOT NULL,
  is_primary boolean NOT NULL DEFAULT true,
  CONSTRAINT pk_email PRIMARY KEY (id)
);