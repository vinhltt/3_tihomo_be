-- TiHoMo Database Initialization Script
-- Tạo databases cho ExcelApi và CoreFinance services
-- Created for message queue testing implementation

-- Create databases
CREATE DATABASE "TiHoMo_CoreFinance_Dev" 
    WITH ENCODING = 'UTF8' 
    LC_COLLATE = 'C' 
    LC_CTYPE = 'C';

CREATE DATABASE "TiHoMo_ExcelApi_Dev" 
    WITH ENCODING = 'UTF8' 
    LC_COLLATE = 'C' 
    LC_CTYPE = 'C';

-- Grant permissions to tihomo user
GRANT ALL PRIVILEGES ON DATABASE "TiHoMo_CoreFinance_Dev" TO tihomo;
GRANT ALL PRIVILEGES ON DATABASE "TiHoMo_ExcelApi_Dev" TO tihomo;

-- Connect to CoreFinance database và create basic tables for message queue testing
\c "TiHoMo_CoreFinance_Dev"

-- Enable uuid extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Transaction Batch table để track message batches
CREATE TABLE IF NOT EXISTS transaction_batches (
    batch_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    correlation_id UUID NOT NULL,
    source VARCHAR(50) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    expected_count INTEGER NOT NULL DEFAULT 0,
    processed_count INTEGER NOT NULL DEFAULT 0,
    failed_count INTEGER NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'Processing',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Transactions table để store individual transactions từ Excel files
CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    batch_id UUID REFERENCES transaction_batches(batch_id) ON DELETE CASCADE,
    correlation_id UUID NOT NULL,
    transaction_date TIMESTAMP WITH TIME ZONE NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    description TEXT,
    category VARCHAR(100),
    reference VARCHAR(100),
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    raw_data JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_transaction_batches_correlation_id ON transaction_batches(correlation_id);
CREATE INDEX IF NOT EXISTS idx_transaction_batches_status ON transaction_batches(status);
CREATE INDEX IF NOT EXISTS idx_transactions_batch_id ON transactions(batch_id);
CREATE INDEX IF NOT EXISTS idx_transactions_correlation_id ON transactions(correlation_id);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON transactions(transaction_date);

-- Connect to ExcelApi database
\c "TiHoMo_ExcelApi_Dev"

-- Enable uuid extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- File Processing Log table để track Excel file processing
CREATE TABLE IF NOT EXISTS file_processing_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    correlation_id UUID NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_size BIGINT,
    processing_started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processing_completed_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(20) NOT NULL DEFAULT 'Processing',
    rows_extracted INTEGER DEFAULT 0,
    error_message TEXT,
    metadata JSONB
);

-- Index for correlation tracking
CREATE INDEX IF NOT EXISTS idx_file_processing_logs_correlation_id ON file_processing_logs(correlation_id);
CREATE INDEX IF NOT EXISTS idx_file_processing_logs_status ON file_processing_logs(status);
