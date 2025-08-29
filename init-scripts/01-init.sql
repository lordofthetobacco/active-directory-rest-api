-- Create database schema for Active Directory REST API

-- API Tokens table
CREATE TABLE api_tokens (
    id SERIAL PRIMARY KEY,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    scopes TEXT[] NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP,
    last_used_at TIMESTAMP
);

-- Audit Logs table
CREATE TABLE audit_logs (
    id SERIAL PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    api_token_id INTEGER REFERENCES api_tokens(id),
    endpoint VARCHAR(255) NOT NULL,
    method VARCHAR(10) NOT NULL,
    user_agent TEXT,
    ip_address INET,
    request_body TEXT,
    response_status INTEGER,
    response_body TEXT,
    execution_time_ms INTEGER,
    error_message TEXT,
    additional_data JSONB
);

-- Create indexes for better performance
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_audit_logs_api_token_id ON audit_logs(api_token_id);
CREATE INDEX idx_audit_logs_endpoint ON audit_logs(endpoint);
CREATE INDEX idx_api_tokens_token_hash ON api_tokens(token_hash);
CREATE INDEX idx_api_tokens_is_active ON api_tokens(is_active);

-- Insert default admin token (for development - change in production)
INSERT INTO api_tokens (token_hash, name, description, scopes) 
VALUES (
    'admin_token_hash_placeholder', 
    'Admin Token', 
    'Default admin token with full access', 
    ARRAY['users:read', 'users:write', 'users:delete', 'groups:read', 'groups:write', 'groups:delete', 'ous:read', 'ous:write', 'ous:delete', 'other:read', 'all:read', 'find:read', 'status:read']
);

-- Create function to log API calls
CREATE OR REPLACE FUNCTION log_api_call(
    p_api_token_id INTEGER,
    p_endpoint VARCHAR,
    p_method VARCHAR,
    p_user_agent TEXT,
    p_ip_address INET,
    p_request_body TEXT,
    p_response_status INTEGER,
    p_response_body TEXT,
    p_execution_time_ms INTEGER,
    p_error_message TEXT,
    p_additional_data JSONB
)
RETURNS VOID AS $$
BEGIN
    INSERT INTO audit_logs (
        api_token_id, endpoint, method, user_agent, ip_address,
        request_body, response_status, response_body, execution_time_ms,
        error_message, additional_data
    ) VALUES (
        p_api_token_id, p_endpoint, p_method, p_user_agent, p_ip_address,
        p_request_body, p_response_status, p_response_body, p_execution_time_ms,
        p_error_message, p_additional_data
    );
END;
$$ LANGUAGE plpgsql;
