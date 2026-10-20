INSERT INTO categories (name) VALUES
    ('Food'),
    ('Transport'),
    ('Bills'),
    ('Other');


INSERT INTO expenses (description, amount, category_id)
SELECT v.description, v.amount, c.id
FROM (VALUES
    ('Waakye',          25.50, 'Food'),
    ('Kenkey and fish', 18.00, 'Food'),
    ('Jollof rice',     35.00, 'Food'),
    ('Groceries',      120.75, 'Food'),
    ('Trotro fare',      6.00, 'Transport'),
    ('Uber to campus',  45.00, 'Transport'),
    ('Fuel',           200.00, 'Transport'),
    ('Electricity',     85.00, 'Bills'),
    ('Data bundle',     50.00, 'Bills'),
    ('Water bill',      30.00, 'Bills'),
    ('Haircut',         40.00, 'Other'),
    ('Birthday gift',   75.00, 'Other')
) AS v (description, amount, category)
JOIN categories c ON c.name = v.category;
