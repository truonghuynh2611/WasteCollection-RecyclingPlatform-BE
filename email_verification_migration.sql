-- Add email verification and password reset fields to users table

-- Add email verification fields
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS emailverified BOOLEAN DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS verificationtoken VARCHAR(500),
ADD COLUMN IF NOT EXISTS verificationtokenexpiry TIMESTAMP WITHOUT TIME ZONE;

-- Add password reset fields
ALTER TABLE users
ADD COLUMN IF NOT EXISTS resetpasswordtoken VARCHAR(500),
ADD COLUMN IF NOT EXISTS resettokenexpiry TIMESTAMP WITHOUT TIME ZONE;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_verificationtoken ON users(verificationtoken) WHERE verificationtoken IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_users_resetpasswordtoken ON users(resetpasswordtoken) WHERE resetpasswordtoken IS NOT NULL;

-- Display result
SELECT 'Email verification and password reset fields added successfully' AS message;
