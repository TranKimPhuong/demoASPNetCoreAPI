IF NOT EXISTS (SELECT TOP 1 1 FROM sys.database_principals WHERE name = 'customps')
BEGIN
       CREATE USER customps FROM LOGIN customps;
END
ALTER ROLE db_datareader ADD MEMBER customps;
ALTER ROLE db_datawriter ADD MEMBER customps;
GO
GRANT EXEC ON [dbo].usp_i_CreateVendorFromPaymentFile TO customps
GRANT EXEC ON TYPE::[dbo].tvp_PaymentConversionVendorAddresses TO customps