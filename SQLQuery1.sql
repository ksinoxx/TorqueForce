
-- Создание базы данных
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'auto_parts_store')
BEGIN
    CREATE DATABASE auto_parts_store;
END
GO

USE auto_parts_store;
GO

-- Очистка существующих таблиц (если есть)
IF OBJECT_ID('dbo.order_step', 'U') IS NOT NULL DROP TABLE dbo.order_step;
IF OBJECT_ID('dbo.order_part', 'U') IS NOT NULL DROP TABLE dbo.order_part;
IF OBJECT_ID('dbo.customer_order', 'U') IS NOT NULL DROP TABLE dbo.customer_order;
IF OBJECT_ID('dbo.supply_part', 'U') IS NOT NULL DROP TABLE dbo.supply_part;
IF OBJECT_ID('dbo.supply', 'U') IS NOT NULL DROP TABLE dbo.supply;
IF OBJECT_ID('dbo.part_compatibility', 'U') IS NOT NULL DROP TABLE dbo.part_compatibility;
IF OBJECT_ID('dbo.part', 'U') IS NOT NULL DROP TABLE dbo.part;
IF OBJECT_ID('dbo.car_model', 'U') IS NOT NULL DROP TABLE dbo.car_model;
IF OBJECT_ID('dbo.brand', 'U') IS NOT NULL DROP TABLE dbo.brand;
IF OBJECT_ID('dbo.manufacturer', 'U') IS NOT NULL DROP TABLE dbo.manufacturer;
IF OBJECT_ID('dbo.category', 'U') IS NOT NULL DROP TABLE dbo.category;
IF OBJECT_ID('dbo.client', 'U') IS NOT NULL DROP TABLE dbo.client;
IF OBJECT_ID('dbo.employee', 'U') IS NOT NULL DROP TABLE dbo.employee;
IF OBJECT_ID('dbo.supplier', 'U') IS NOT NULL DROP TABLE dbo.supplier;
IF OBJECT_ID('dbo.step', 'U') IS NOT NULL DROP TABLE dbo.step;
GO


-- Создание таблиц (в правильном порядке)

-- 1. Производители
CREATE TABLE manufacturer (
    manufacturer_id INT IDENTITY(1,1) PRIMARY KEY,
    manufacturer_name NVARCHAR(100) NOT NULL
);
GO

-- 2. Бренды
CREATE TABLE brand (
    brand_id INT IDENTITY(1,1) PRIMARY KEY,
    brand_name NVARCHAR(100) NOT NULL,
    manufacturer_id INT NOT NULL,
    CONSTRAINT FK_brand_manufacturer FOREIGN KEY (manufacturer_id) 
        REFERENCES manufacturer(manufacturer_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 3. Категории
CREATE TABLE category (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL
);
GO

-- 4. Модели авто
CREATE TABLE car_model (
    car_model_id INT IDENTITY(1,1) PRIMARY KEY,
    car_model_name NVARCHAR(100) NOT NULL,
    year_start INT NULL,
    year_end INT NULL
);
GO

-- 5. Запчасти
CREATE TABLE part (
    part_id INT IDENTITY(1,1) PRIMARY KEY,
    part_name NVARCHAR(150) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    category_id INT NOT NULL,
    brand_id INT NOT NULL,
    CONSTRAINT FK_part_category FOREIGN KEY (category_id) 
        REFERENCES category(category_id)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT FK_part_brand FOREIGN KEY (brand_id) 
        REFERENCES brand(brand_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 6. Совместимость запчастей с авто
CREATE TABLE part_compatibility (
    part_id INT NOT NULL,
    car_model_id INT NOT NULL,
    CONSTRAINT PK_part_compatibility PRIMARY KEY (part_id, car_model_id),
    CONSTRAINT FK_part_compatibility_part FOREIGN KEY (part_id) 
        REFERENCES part(part_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_part_compatibility_car_model FOREIGN KEY (car_model_id) 
        REFERENCES car_model(car_model_id)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 7. Клиенты
CREATE TABLE client (
    client_id INT IDENTITY(1,1) PRIMARY KEY,
    name_client NVARCHAR(150) NOT NULL,
    phone NVARCHAR(20) NOT NULL,
    email NVARCHAR(100) NOT NULL
);
GO

-- 8. Сотрудники
CREATE TABLE employee (
    employee_id INT IDENTITY(1,1) PRIMARY KEY,
    name_employee NVARCHAR(150) NOT NULL,
    position NVARCHAR(100) NOT NULL
);
GO

-- 9. Поставщики
CREATE TABLE supplier (
    supplier_id INT IDENTITY(1,1) PRIMARY KEY,
    name_supplier NVARCHAR(150) NOT NULL,
    phone NVARCHAR(20) NOT NULL,
    email NVARCHAR(100) NOT NULL
);
GO

-- 10. Этапы заказа (справочник)
CREATE TABLE step (
    step_id INT IDENTITY(1,1) PRIMARY KEY,
    name_step NVARCHAR(100) NOT NULL
);
GO

-- 11. Поставки
CREATE TABLE supply (
    supply_id INT IDENTITY(1,1) PRIMARY KEY,
    supplier_id INT NOT NULL,
    supply_date DATETIME NOT NULL,
    employee_id INT NOT NULL,
    CONSTRAINT FK_supply_supplier FOREIGN KEY (supplier_id) 
        REFERENCES supplier(supplier_id)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT FK_supply_employee FOREIGN KEY (employee_id) 
        REFERENCES employee(employee_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 12. Позиции поставок
CREATE TABLE supply_part (
    supply_part_id INT IDENTITY(1,1) PRIMARY KEY,
    supply_id INT NOT NULL,
    part_id INT NOT NULL,
    quantity INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_supply_part_supply FOREIGN KEY (supply_id) 
        REFERENCES supply(supply_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_supply_part_part FOREIGN KEY (part_id) 
        REFERENCES part(part_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 13. Заказы клиентов
CREATE TABLE customer_order (
    customer_order_id INT IDENTITY(1,1) PRIMARY KEY,
    client_id INT NOT NULL,
    employee_id INT NOT NULL,
    order_date DATETIME NOT NULL,
    total_price DECIMAL(10,2) NULL,
    CONSTRAINT FK_customer_order_client FOREIGN KEY (client_id) 
        REFERENCES client(client_id)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT FK_customer_order_employee FOREIGN KEY (employee_id) 
        REFERENCES employee(employee_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 14. Позиции заказов
CREATE TABLE order_part (
    customer_order_id INT NOT NULL,
    part_id INT NOT NULL,
    quantity INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_order_part PRIMARY KEY (customer_order_id, part_id),
    CONSTRAINT FK_order_part_customer_order FOREIGN KEY (customer_order_id) 
        REFERENCES customer_order(customer_order_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_order_part_part FOREIGN KEY (part_id) 
        REFERENCES part(part_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- 15. Этапы выполнения заказа
CREATE TABLE order_step (
    order_step_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_order_id INT NOT NULL,
    step_id INT NOT NULL,
    date_start DATETIME NOT NULL,
    date_end DATETIME NULL,
    CONSTRAINT FK_order_step_customer_order FOREIGN KEY (customer_order_id) 
        REFERENCES customer_order(customer_order_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_order_step_step FOREIGN KEY (step_id) 
        REFERENCES step(step_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- Триггеры

-- Триггер: обновление остатков при заказе + проверка
CREATE TRIGGER update_stock_after_order
ON order_part
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @part_id INT;
    DECLARE @quantity INT;
    DECLARE @current_stock INT;
    
    SELECT @part_id = i.part_id, @quantity = i.quantity 
    FROM INSERTED i;
    
    SELECT @current_stock = stock FROM part WHERE part_id = @part_id;
    
    IF @current_stock < @quantity
    BEGIN
        RAISERROR('Недостаточно товара на складе', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    UPDATE part
    SET stock = stock - @quantity
    WHERE part_id = @part_id;
END;
GO

-- Триггер: обновление остатков при поставке
CREATE TRIGGER update_stock_after_supply
ON supply_part
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @part_id INT;
    DECLARE @quantity INT;
    
    SELECT @part_id = i.part_id, @quantity = i.quantity 
    FROM INSERTED i;
    
    UPDATE part
    SET stock = stock + @quantity
    WHERE part_id = @part_id;
END;
GO


-- Тестовые данные

-- Производители
INSERT INTO manufacturer (manufacturer_name) VALUES 
(N'Bosch'), (N'NGK'), (N'Brembo'), (N'MANN'), (N'Castrol');
GO

-- Бренды
INSERT INTO brand (brand_name, manufacturer_id) VALUES 
(N'Bosch Auto', 1),
(N'NGK Spark', 2),
(N'Brembo Brake', 3),
(N'MANN Filter', 4),
(N'Castrol Oil', 5);
GO

-- Категории
INSERT INTO category (category_name) VALUES 
(N'Фильтры'),
(N'Тормозная система'),
(N'Свечи зажигания'),
(N'Масла'),
(N'Ремкомплекты');
GO

-- Модели авто
INSERT INTO car_model (car_model_name, year_start, year_end) VALUES 
(N'ВАЗ-2110', 1996, 2010),
(N'ВАЗ-2112', 1998, 2013),
(N'Lada Granta', 2011, NULL),
(N'Lada Vesta', 2015, NULL),
(N'KIA Rio', 2011, 2023),
(N'Hyundai Solaris', 2010, NULL);
GO

-- Запчасти
INSERT INTO part (part_name, price, stock, category_id, brand_id) VALUES
(N'Фильтр масляный Bosch 0451103062', 450.00, 15, 1, 1),
(N'Фильтр воздушный Bosch 1987429180', 680.00, 8, 1, 1),
(N'Фильтр топливный Bosch 0450900001', 520.00, 12, 1, 1),
(N'Фильтр салона Bosch 1987429197', 750.00, 5, 1, 1),
(N'Тормозные колодки передние Brembo P85014', 1800.00, 20, 2, 3),
(N'Тормозные колодки задние Brembo P85015', 1600.00, 18, 2, 3),
(N'Диски тормозные передние Brembo 09.A424.11', 3500.00, 6, 2, 3),
(N'Диски тормозные задние Brembo 09.A425.11', 3200.00, 4, 2, 3),
(N'Свеча зажигания NGK BPR6ES', 280.00, 50, 3, 2),
(N'Свеча зажигания иридиевая NGK LFR7AIX', 1200.00, 30, 3, 2),
(N'Масло моторное Castrol EDGE 5W-30 5л', 3200.00, 25, 4, 5),
(N'Масло трансмиссионное Castrol Syntrax 75W-90 1л', 950.00, 15, 4, 5),
(N'Ремкомплект сцепления Lada 2110', 2200.00, 8, 5, 1),
(N'Ремкомплект ГРМ Lada 2112', 1500.00, 12, 5, 1),
(N'Ремкомплект подвески передней', 3800.00, 5, 5, 3);
GO

-- Совместимость запчастей с авто
INSERT INTO part_compatibility (part_id, car_model_id) VALUES
(1, 1), (1, 2),  -- Фильтр масляный под ВАЗ-2110 и 2112
(2, 1), (2, 2),  -- Фильтр воздушный под ВАЗ-2110 и 2112
(3, 3), (3, 4),  -- Фильтр топливный под Гранту и Весту
(4, 5), (4, 6),  -- Фильтр салона под Рио и Солярис
(5, 1), (5, 2),  -- Тормозные колодки под ВАЗ-2110 и 2112
(6, 1), (6, 2),  -- Тормозные колодки задние под ВАЗ-2110 и 2112
(7, 5), (7, 6),  -- Диски передние под Рио и Солярис
(8, 5), (8, 6),  -- Диски задние под Рио и Солярис
(9, 1), (9, 2),  -- Свеча под ВАЗ-2110 и 2112
(10, 3), (10, 4),-- Иридиевая свеча под Гранту и Весту
(11, 1), (11, 2), (11, 3), (11, 4), (11, 5), (11, 6), -- Масло под все авто
(12, 1), (12, 2), (12, 3), (12, 4), (12, 5), (12, 6), -- Трансмиссионное масло под все авто
(13, 1),         -- Ремкомплект сцепления под ВАЗ-2110
(14, 2),         -- Ремкомплект ГРМ под ВАЗ-2112
(15, 3), (15, 4);-- Ремкомплект подвески под Гранту и Весту
GO

-- Клиенты
INSERT INTO client (name_client, phone, email) VALUES
(N'Иванов Иван Иванович', N'+79001234567', N'ivanov@mail.ru'),
(N'Петров Пётр Петрович', N'+79002345678', N'petrov@mail.ru'),
(N'Сидоров Сидор Сидорович', N'+79003456789', N'sidorov@mail.ru');
GO

-- Сотрудники
INSERT INTO employee (name_employee, position) VALUES
(N'Смирнов Алексей', N'Менеджер'),
(N'Козлов Дмитрий', N'Кладовщик'),
(N'Васильев Сергей', N'Директор');
GO

-- Поставщики
INSERT INTO supplier (name_supplier, phone, email) VALUES
(N'ООО "Авто детали"', N'+7 (495) 123-45-67', N'info@autodetali.ru'),
(N'Завод "Босх-РУ"', N'+7 (812) 987-65-43', N'zakaz@bosch.ru');
GO

-- Этапы заказа (справочник)
INSERT INTO step (name_step) VALUES
(N'Принят'),
(N'Оплачен'),
(N'Отправлен'),
(N'Доставлен');
GO

-- Поставки
INSERT INTO supply (supplier_id, supply_date, employee_id) VALUES
(1, GETDATE(), 2),
(2, GETDATE(), 2);
GO

-- Позиции поставок
INSERT INTO supply_part (supply_id, part_id, quantity, price) VALUES
(1, 1, 20, 450.00),
(1, 2, 15, 680.00),
(1, 3, 25, 520.00),
(1, 5, 30, 1800.00),
(1, 9, 100, 280.00),
(2, 4, 10, 750.00),
(2, 6, 25, 1600.00),
(2, 10, 40, 1200.00),
(2, 11, 30, 3200.00),
(2, 14, 20, 1500.00);
GO

-- Заказы
INSERT INTO customer_order (client_id, employee_id, order_date, total_price) VALUES
(1, 1, GETDATE(), 900.00),   -- 2 шт фильтра масляного
(2, 1, GETDATE(), 1400.00),  -- 5 шт свечей
(3, 1, GETDATE(), 3500.00);  -- 1 шт тормозной диск
GO

-- Позиции заказов
INSERT INTO order_part (customer_order_id, part_id, quantity, price) VALUES
(1, 1, 2, 450.00),
(2, 9, 5, 280.00),
(3, 7, 1, 3500.00);
GO

-- Этапы заказов
INSERT INTO order_step (customer_order_id, step_id, date_start, date_end) VALUES
(1, 1, GETDATE(), GETDATE()),     -- Принят
(1, 2, GETDATE(), NULL),          -- Оплачен (в процессе)
(2, 1, GETDATE(), GETDATE()),     -- Принят
(2, 2, GETDATE(), GETDATE()),     -- Оплачен
(2, 3, GETDATE(), NULL),          -- Отправлен (в процессе)
(3, 1, GETDATE(), NULL);          -- Принят (в процессе)
GO

-- Проверка данных

SELECT 
    (SELECT COUNT(*) FROM part) AS parts_count,
    (SELECT COUNT(*) FROM customer_order) AS orders_count,
    (SELECT COUNT(*) FROM supply) AS supplies_count,
    (SELECT COUNT(*) FROM client) AS clients_count,
    (SELECT COUNT(*) FROM employee) AS employees_count;
GO﻿
