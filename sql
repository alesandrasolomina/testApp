CREATE TABLE departments (
	id INTEGER NOT NULL PRIMARY KEY,
	parentid INTEGER REFERENCES departments(id),
	managerid INTEGER,
	name TEXT NOT NULL,
	phone TEXT,
	manager_name TEXT

);


CREATE TABLE departments_names (
	id INTEGER NOT NULL PRIMARY KEY,
	department_id INTEGER NOT NULL,
	name TEXT NOT NULL
	

	CONSTRAINT  UC_department_id UNIQUE (department_id)
);

CREATE TABLE jobtitle (
	id INTEGER NOT NULL PRIMARY KEY,
	name TEXT NOT NULL
);


CREATE TABLE employees (
	id INTEGER NOT NULL PRIMARY KEY,
	department INTEGER,
	fullname TEXT NOT NULL,
	login TEXT NOT NULL,
	password TEXT NOT NULL,
	jobtitle INTEGER,
	
	CONSTRAINT fk_jobtitle FOREIGN KEY(jobtitle) 
        REFERENCES jobtitle(id),
      
      CONSTRAINT fk_department FOREIGN KEY(department) 
      REFERENCES departments_names(department_id)
      
);


ALTER TABLE departments
    ADD CONSTRAINT fk_department FOREIGN KEY(id) 
        REFERENCES departments_names(department_id)







