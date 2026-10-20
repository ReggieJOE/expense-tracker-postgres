CREATE OR REPLACE PROCEDURE add_expense(
    p_description VARCHAR,
    p_amount      NUMERIC,
    p_category_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_amount IS NULL OR p_amount <= 0 THEN
        RAISE EXCEPTION 'Amount must be greater than zero';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM categories WHERE id = p_category_id) THEN
        RAISE EXCEPTION 'Category % does not exist', p_category_id;
    END IF;

    INSERT INTO expenses (description, amount, category_id)
    VALUES (p_description, p_amount, p_category_id);
END;
$$;

-- ---------------------------------------------------------------------
-- delete_expense: removes one expense, or complains if it is not there.
--   CALL delete_expense(3);
-- ---------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE delete_expense(p_id INT)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM expenses WHERE id = p_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Expense % not found', p_id;
    END IF;
END;
$$;

-- ---------------------------------------------------------------------
-- category_totals: total spent per category (0 for unused categories).
--   SELECT * FROM category_totals();
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION category_totals()
RETURNS TABLE (category VARCHAR, total NUMERIC)
LANGUAGE sql
AS $$
    SELECT c.name, COALESCE(SUM(e.amount), 0)
    FROM categories c
    LEFT JOIN expenses e ON e.category_id = c.id
    GROUP BY c.name
    ORDER BY c.name;
$$;
