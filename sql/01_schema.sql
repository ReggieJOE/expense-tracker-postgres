DROP VIEW  IF EXISTS expense_details;
DROP TABLE IF EXISTS expenses;
DROP TABLE IF EXISTS categories;

-- One row per category (replaces the C# categories array)
CREATE TABLE categories (
    id   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- One row per expense (replaces the C# List<Expense>)
CREATE TABLE expenses (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    description VARCHAR(100)  NOT NULL,
    amount      NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    category_id INT           NOT NULL REFERENCES categories (id),
    created_at  TIMESTAMP     NOT NULL DEFAULT NOW()
);

-- A view = a saved SELECT. It joins the two tables so the C# code
-- can read "expense + category name" with one simple query.
CREATE VIEW expense_details AS
SELECT e.id,
       e.description,
       e.amount,
       c.name AS category,
       e.created_at
FROM expenses e
JOIN categories c ON c.id = e.category_id;
