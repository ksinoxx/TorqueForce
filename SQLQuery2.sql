ALTER TABLE customer_order ALTER COLUMN employee_id INT NULL;
-- или если нужен дефолт
ALTER TABLE customer_order ADD CONSTRAINT DF_customer_order_employee_id DEFAULT 0 FOR employee_id;