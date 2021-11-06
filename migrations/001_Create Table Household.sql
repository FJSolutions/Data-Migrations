
CREATE TABLE public.households (
  id serial NOT NULL,
  planning_center_id int NOT NULL,
  name varchar(128) NOT NULL,
  CONSTRAINT pk_household PRIMARY KEY (id)
);
