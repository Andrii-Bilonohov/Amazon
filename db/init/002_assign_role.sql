DO $$
DECLARE
    target_email TEXT := 'seller@example.com';
    target_role TEXT := 'seller';
    target_user_id BIGINT;
BEGIN
    -- check the name
    IF target_role NOT IN ('buyer','seller','moderator','admin') THEN
	RAISE EXCEPTION 'Unknown role: %', target_role;
    END IF;

    -- find the user
   SELECT id
   INTO target_user_id
   FROM users
   WHERE email = target_email;

   IF target_user_id IS NULL THEN 
       RAISE EXCEPTION 'The user with this email % is not found',
           target_email;
   END IF;
-- delete the previous roles
 DELETE FROM sellers
 WHERE user_id = target_user_id;

 DELETE FROM moderators
 WHERE user_id = target_user_id;

 DELETE FROM admins
 WHERE user_id = target_user_id;
-- get a new role
 CASE target_role
     WHEN 'buyer' THEN 
       -- The buyer is saved as a basic user
         NULL;

     WHEN 'seller' THEN
	 INSERT INTO sellers (user_id)
	 VALUES (target_user_id);

     WHEN 'moderator' THEN
	 INSERT INTO moderators (user_id)
	 VALUES (target_user_id);

     WHEN 'admin' THEN
	 INSERT INTO admins (user_id)
	 VALUES (target_user_id);
 END CASE;
 RAISE NOTICE 'The role % was gived to a user %',
     target_role,target_email;
END;
$$;
